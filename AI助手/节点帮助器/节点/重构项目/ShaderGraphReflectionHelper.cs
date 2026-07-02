using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

namespace 龙哥的秘密花园.节点库
{
    /// <summary>
    /// 底层反射工具类：提供 ShaderGraph 内部类型的反射操作，无状态，纯静态方法。
    /// </summary>
    public static class ShaderGraphReflectionHelper
    {
        // ---------- 类型缓存 ----------
        private static readonly Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();

        /// <summary>
        /// 查找类型（支持泛型类型名称，例如 "UnityEditor.ShaderGraph.Serialization.JsonData`1"）
        /// </summary>
        public static Type FindType(string typeName)
        {
            if (TypeCache.TryGetValue(typeName, out var cached))
                return cached;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        TypeCache[typeName] = type;
                        return type;
                    }
                }
                catch
                {
                    // 忽略无法加载的程序集
                }
            }
            return null;
        }

        /// <summary>
        /// 获取开放泛型类型，并用指定类型参数构造封闭泛型类型。
        /// 例如：FindGenericType("UnityEditor.ShaderGraph.Serialization.JsonData`1", typeof(BlockNode))
        /// </summary>
        public static Type FindGenericType(string openGenericTypeName, params Type[] typeArguments)
        {
            var openType = FindType(openGenericTypeName);
            if (openType == null) return null;
            return openType.MakeGenericType(typeArguments);
        }

        /// <summary>
        /// 通过基础类型名称和类型参数名称数组获取封闭泛型类型。
        /// 例如：MakeGenericTypeByName("UnityEditor.ShaderGraph.Serialization.JsonData`1", new[] { "UnityEditor.ShaderGraph.BlockNode" })
        /// </summary>
        public static Type MakeGenericTypeByName(string openGenericTypeName, string[] typeArgumentNames)
        {
            var openType = FindType(openGenericTypeName);
            if (openType == null)
            {
                Debug.LogError($"找不到开放泛型类型: {openGenericTypeName}");
                return null;
            }

            var typeArgs = new Type[typeArgumentNames.Length];
            for (int i = 0; i < typeArgumentNames.Length; i++)
            {
                typeArgs[i] = FindType(typeArgumentNames[i]);
                if (typeArgs[i] == null)
                {
                    Debug.LogError($"找不到类型参数: {typeArgumentNames[i]}");
                    return null;
                }
            }

            try
            {
                return openType.MakeGenericType(typeArgs);
            }
            catch (Exception ex)
            {
                Debug.LogError($"构造泛型类型失败: {ex.Message}");
                return null;
            }
        }

        // ---------- GraphData 加载/保存 ----------
        public static object LoadGraphData(string assetPath, out string error)
        {
            error = null;
            try
            {
                var graphDataType = FindType("UnityEditor.ShaderGraph.GraphData");
                if (graphDataType == null) { error = "找不到 GraphData 类型"; return null; }

                var graphData = Activator.CreateInstance(graphDataType);
                var guidProp = graphDataType.GetProperty("assetGuid");
                guidProp?.SetValue(graphData, AssetDatabase.AssetPathToGUID(assetPath));

                string json = File.ReadAllText(assetPath);
                var multiJsonType = FindType("UnityEditor.ShaderGraph.Serialization.MultiJson");
                if (multiJsonType == null) { error = "找不到 MultiJson 类型"; return null; }
                var deserializeMethod = multiJsonType.GetMethod("Deserialize")?.MakeGenericMethod(graphDataType);
                deserializeMethod?.Invoke(null, new object[] { graphData, json, null, false });

                graphDataType.GetMethod("OnEnable")?.Invoke(graphData, null);
                return graphData;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static bool SaveGraphData(object graphData, string assetPath, out string error)
        {
            error = null;
            try
            {
                var multiJsonType = FindType("UnityEditor.ShaderGraph.Serialization.MultiJson");
                var serializeMethod = multiJsonType?.GetMethod("Serialize", BindingFlags.Public | BindingFlags.Static);
                if (serializeMethod == null) { error = "找不到 Serialize 方法"; return false; }
                string json = (string)serializeMethod.Invoke(null, new[] { graphData });
                if (string.IsNullOrEmpty(json)) { error = "序列化结果为空"; return false; }
                File.WriteAllText(assetPath, json);
                AssetDatabase.SaveAssets();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static bool SaveGraphDataToFile(object graphData, string assetPath, out string error)
        {
            return SaveGraphData(graphData, assetPath, out error);
        }

        // ---------- 节点操作 ----------
        public static object CreateNodeInstance(string nodeTypeName)
        {
            var nodeType = FindType(nodeTypeName);
            if (nodeType == null) return null;
            return Activator.CreateInstance(nodeType);
        }

        public static void SetNodeId(object node, string id = null)
        {
            var type = node.GetType();
            id ??= Guid.NewGuid().ToString();

            var idProp = type.GetProperty("objectId", BindingFlags.Public | BindingFlags.Instance);
            if (idProp?.CanWrite == true) { idProp.SetValue(node, id); return; }

            foreach (var propName in new[] { "guid", "id" })
            {
                var prop = type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop?.CanWrite == true) { prop.SetValue(node, id); return; }
            }
        }

        public static void SetNodePosition(object node, Rect position)
        {
            var type = node.GetType();

            // 方式1：通过 drawState 属性（首选）
            var drawStateProp = type.GetProperty("drawState", BindingFlags.Public | BindingFlags.Instance);
            if (drawStateProp != null && drawStateProp.CanWrite)
            {
                var drawState = drawStateProp.GetValue(node);
                if (drawState != null)
                {
                    var drawStateType = drawState.GetType();
                    var posProp = drawStateType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);
                    if (posProp != null && posProp.CanWrite)
                    {
                        posProp.SetValue(drawState, position);
                        drawStateProp.SetValue(node, drawState);
                        return;
                    }
                    var posField = drawStateType.GetField("m_Position", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (posField != null)
                    {
                        posField.SetValue(drawState, position);
                        drawStateProp.SetValue(node, drawState);
                        return;
                    }
                }
            }

            // 方式2：直接设置 m_DrawState 字段
            var mDrawStateField = type.GetField("m_DrawState", BindingFlags.NonPublic | BindingFlags.Instance);
            if (mDrawStateField != null)
            {
                var drawState = mDrawStateField.GetValue(node);
                if (drawState != null)
                {
                    var posProp = drawState.GetType().GetProperty("position", BindingFlags.Public | BindingFlags.Instance);
                    if (posProp != null && posProp.CanWrite)
                    {
                        posProp.SetValue(drawState, position);
                        mDrawStateField.SetValue(node, drawState);
                        return;
                    }
                    var posField = drawState.GetType().GetField("m_Position", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (posField != null)
                    {
                        posField.SetValue(drawState, position);
                        mDrawStateField.SetValue(node, drawState);
                        return;
                    }
                }
            }

            // 方式3：尝试 IRectInterface.rect
            var rectInterfaceType = FindType("UnityEditor.Graphing.IRectInterface");
            if (rectInterfaceType != null && rectInterfaceType.IsAssignableFrom(type))
            {
                var rectProp = rectInterfaceType.GetProperty("rect");
                if (rectProp != null && rectProp.CanWrite)
                {
                    rectProp.SetValue(node, position);
                    return;
                }
            }

            Debug.LogWarning($"无法设置节点 {type.Name} 的位置，所有方法均失败。");
        }

        public static void AddNodeToGraph(object graphData, object node)
        {
            var addMethod = graphData.GetType().GetMethod("AddNode", new Type[] { node.GetType() });
            if (addMethod == null)
                throw new Exception("GraphData 缺少 AddNode 方法");
            addMethod.Invoke(graphData, new[] { node });
        }

        public static void ValidateGraph(object graphData)
        {
            var validateMethod = graphData.GetType().GetMethod("ValidateGraph");
            validateMethod?.Invoke(graphData, null);
        }

        public static bool SetNodeProperty(object node, string propertyPath, object value)
        {
            if (string.IsNullOrEmpty(propertyPath)) return false;
            var parts = propertyPath.Split('.');
            if (parts.Length == 0) return false;

            object current = node;
            Type currentType = node.GetType();
            MemberInfo currentMember = null;
            object parent = null;
            MemberInfo parentMember = null;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                string part = parts[i];
                var member = GetMember(currentType, part);
                if (member == null) return false;

                parent = current;
                parentMember = member;

                current = GetValue(member, current);
                if (current == null) return false;
                currentType = current.GetType();
                currentMember = member;
            }

            string lastPart = parts[parts.Length - 1];
            var lastMember = GetMember(currentType, lastPart);
            if (lastMember == null) return false;

            object convertedValue = ConvertValue(lastMember, value);
            if (convertedValue == null && value != null) return false;

            SetValue(lastMember, current, convertedValue);

            if (current != null && current.GetType().IsValueType && parent != null && parentMember != null)
            {
                SetValue(parentMember, parent, current);
            }
          
            return true;
        }

        private static MemberInfo GetMember(Type type, string name)
        {
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) return field;
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) return prop;
            return null;
        }

        private static object GetValue(MemberInfo member, object target)
        {
            if (member is FieldInfo field) return field.GetValue(target);
            if (member is PropertyInfo prop) return prop.GetValue(target);
            return null;
        }

        private static void SetValue(MemberInfo member, object target, object value)
        {
            if (member is FieldInfo field) field.SetValue(target, value);
            else if (member is PropertyInfo prop) prop.SetValue(target, value);
        }

        private static object ConvertValue(MemberInfo member, object value)
        {
            Type targetType;
            if (member is FieldInfo field) targetType = field.FieldType;
            else if (member is PropertyInfo prop) targetType = prop.PropertyType;
            else return null;

            if (value == null) return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            Type sourceType = value.GetType();

            if (targetType.IsAssignableFrom(sourceType))
                return value;

            if (targetType.IsGenericType)
            {
                var genericDef = targetType.GetGenericTypeDefinition();
                if (genericDef != null && (genericDef.Name == "JsonData`1" || genericDef.Name == "JsonRef`1"))
                {
                    var elementType = targetType.GetGenericArguments()[0];
                    if (elementType.IsAssignableFrom(sourceType))
                    {
                        var jsonData = Activator.CreateInstance(targetType);
                        var idField = targetType.GetField("m_Id", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (idField != null)
                        {
                            var objIdProp = sourceType.GetProperty("objectId", BindingFlags.Public | BindingFlags.Instance);
                            string objectId = objIdProp?.GetValue(value) as string;
                            if (!string.IsNullOrEmpty(objectId))
                                idField.SetValue(jsonData, objectId);
                        }
                        var valueField = targetType.GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);
                        valueField?.SetValue(jsonData, value);
                        return jsonData;
                    }
                }
            }

            if (targetType == typeof(Vector2) && value is System.Collections.IEnumerable arr2)
            {
                var list = arr2.Cast<object>().ToList();
                if (list.Count >= 2)
                    return new Vector2(Convert.ToSingle(list[0]), Convert.ToSingle(list[1]));
            }
            if (targetType == typeof(Vector3) && value is System.Collections.IEnumerable arr3)
            {
                var list = arr3.Cast<object>().ToList();
                if (list.Count >= 3)
                    return new Vector3(Convert.ToSingle(list[0]), Convert.ToSingle(list[1]), Convert.ToSingle(list[2]));
            }
            if (targetType == typeof(Vector4) && value is System.Collections.IEnumerable arr4)
            {
                var list = arr4.Cast<object>().ToList();
                if (list.Count >= 4)
                    return new Vector4(Convert.ToSingle(list[0]), Convert.ToSingle(list[1]), Convert.ToSingle(list[2]), Convert.ToSingle(list[3]));
            }
            if (targetType == typeof(Color) && value is System.Collections.IEnumerable colArr)
            {
                var list = colArr.Cast<object>().ToList();
                if (list.Count >= 4)
                    return new Color(Convert.ToSingle(list[0]), Convert.ToSingle(list[1]), Convert.ToSingle(list[2]), Convert.ToSingle(list[3]));
            }

            if (targetType.IsEnum)
            {
                if (value is string enumName) return Enum.Parse(targetType, enumName);
                if (value is int intVal) return Enum.ToObject(targetType, intVal);
            }

            try { return Convert.ChangeType(value, targetType); }
            catch { return null; }
        }

        // ---------- 节点信息获取 ----------
        public static object UnwrapJsonData(object obj)
        {
            if (obj == null) return null;
            var type = obj.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition().Name == "JsonData`1")
            {
                var valueField = type.GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);
                return valueField?.GetValue(obj) ?? obj;
            }
            return obj;
        }

        public static string GetNodeId(object node)
        {
            var prop = node.GetType().GetProperty("objectId", BindingFlags.Public | BindingFlags.Instance);
            return prop?.GetValue(node) as string;
        }

        public static string GetNodeName(object node)
        {
            var prop = node.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
            return prop?.GetValue(node) as string ?? "Unknown";
        }

        public static System.Collections.IList GetAllSlots(object node, bool isOutput)
        {
            var slotsField = node.GetType().GetField("m_Slots", BindingFlags.NonPublic | BindingFlags.Instance);
            if (slotsField == null) return new List<object>();
            var slotsList = slotsField.GetValue(node) as System.Collections.IList;
            if (slotsList == null) return new List<object>();

            var result = new List<object>();
            foreach (var wrapper in slotsList)
            {
                var slot = UnwrapJsonData(wrapper);
                if (slot == null) continue;

                var isOutputSlot = slot.GetType().GetProperty("isOutputSlot", BindingFlags.Public | BindingFlags.Instance)?.GetValue(slot) as bool?;
                if (isOutputSlot.HasValue && isOutputSlot.Value == isOutput)
                    result.Add(slot);
            }
            return result;
        }

        public static string GetSlotDisplayName(object slot)
        {
            var prop = slot.GetType().GetProperty("displayName", BindingFlags.Public | BindingFlags.Instance);
            return prop?.GetValue(slot) as string ?? "";
        }

        public static int GetSlotId(object slot)
        {
            var prop = slot.GetType().GetProperty("id", BindingFlags.Public | BindingFlags.Instance);
            return prop != null ? (int)prop.GetValue(slot) : -1;
        }

        public static object FindMaterialSlot(object node, string slotName, bool isOutput)
        {
            if (node == null) return null;

            var nodeType = node.GetType();
            var methodName = isOutput ? "GetOutputSlots" : "GetInputSlots";

            var methods = nodeType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == methodName && m.IsGenericMethodDefinition);
            MethodInfo targetMethod = null;
            foreach (var m in methods)
            {
                var parameters = m.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType.IsGenericType &&
                    parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    targetMethod = m;
                    break;
                }
            }

            if (targetMethod != null)
            {
                var materialSlotType = FindType("UnityEditor.ShaderGraph.MaterialSlot");
                if (materialSlotType == null) return null;

                var listType = typeof(List<>).MakeGenericType(materialSlotType);
                var slotList = Activator.CreateInstance(listType);
                var genericMethod = targetMethod.MakeGenericMethod(materialSlotType);
                genericMethod.Invoke(node, new object[] { slotList });

                var countProp = listType.GetProperty("Count");
                var itemProp = listType.GetProperty("Item");
                int count = (int)countProp.GetValue(slotList);
                for (int i = 0; i < count; i++)
                {
                    var slot = itemProp.GetValue(slotList, new object[] { i });
                    string displayName = GetSlotDisplayName(slot);
                    string rawName = GetRawDisplayName(slot);
                    if (displayName == slotName || rawName == slotName)
                        return slot;

                    string strippedName = StripSlotTypeSuffix(slotName);
                    if (strippedName == rawName || strippedName == displayName)
                        return slot;
                }
            }

            // 回退：直接访问 m_Slots 字段
            var slotsField = nodeType.GetField("m_Slots", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (slotsField != null)
            {
                var rawList = slotsField.GetValue(node) as System.Collections.IList;
                if (rawList != null)
                {
                    foreach (var item in rawList)
                    {
                        var slot = UnwrapJsonData(item);
                        if (slot == null) continue;
                        var isOutputProp = slot.GetType().GetProperty("isOutputSlot", BindingFlags.Public | BindingFlags.Instance);
                        if (isOutputProp == null) continue;
                        if ((bool)isOutputProp.GetValue(slot) != isOutput) continue;

                        string displayName = GetSlotDisplayName(slot);
                        string rawName = GetRawDisplayName(slot);
                        if (displayName == slotName || rawName == slotName)
                            return slot;

                        string strippedName = StripSlotTypeSuffix(slotName);
                        if (strippedName == rawName || strippedName == displayName)
                            return slot;
                    }
                }
            }

            return null;
        }

        public static int GetSlotIdByName(object node, string portName, bool isInput)
        {
            var slot = FindMaterialSlot(node, portName, !isInput);
            return slot != null ? GetSlotId(slot) : -1;
        }

        private static string GetRawDisplayName(object slot)
        {
            var method = slot.GetType().GetMethod("RawDisplayName", BindingFlags.Public | BindingFlags.Instance);
            if (method != null) return method.Invoke(slot, null) as string ?? "";
            var field = slot.GetType().GetField("m_DisplayName", BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(slot) as string ?? "";
        }

        private static string StripSlotTypeSuffix(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            var match = System.Text.RegularExpressions.Regex.Match(name, @"^(.*)\(\d+\)$");
            return match.Success ? match.Groups[1].Value : name;
        }

        // ---------- 黑板属性 ----------
        public static object GetInternalProperty(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return prop?.GetValue(obj);
        }

        public static object FindShaderPropertyByName(object graphData, string referenceName)
        {
            var propsField = graphData.GetType().GetField("m_Properties", BindingFlags.NonPublic | BindingFlags.Instance);
            if (propsField == null) return null;
            var properties = propsField.GetValue(graphData) as System.Collections.IList;
            if (properties == null) return null;

            foreach (var item in properties)
            {
                object actualProp = UnwrapJsonData(item);
                if (actualProp == null) continue;

                Type propType = actualProp.GetType();
                string overrideRefName = propType.GetProperty("overrideReferenceName")?.GetValue(actualProp) as string;
                string defaultRefName = propType.GetField("m_DefaultReferenceName", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(actualProp) as string;
                string refName = propType.GetProperty("referenceName")?.GetValue(actualProp) as string;
                string displayName = propType.GetProperty("displayName")?.GetValue(actualProp) as string;

                if (overrideRefName == referenceName || defaultRefName == referenceName || refName == referenceName || displayName == referenceName)
                    return actualProp;
            }
            return null;
        }

        public static void SetInternalProperty(object obj, string propertyName, object value)
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
                prop.SetValue(obj, value);
            else
                Debug.LogError($"无法设置属性 {propertyName} 在类型 {obj.GetType().Name} 上");
        }

        public static void SetInternalField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
                field.SetValue(obj, value);
        }

        public static object ConvertToInternalEnum(string nestedTypeName, object publicEnumValue)
        {
            var samplerStateType = FindType("UnityEditor.ShaderGraph.TextureSamplerState");
            if (samplerStateType == null) return 0;
            var nestedType = samplerStateType.GetNestedType(nestedTypeName, BindingFlags.NonPublic | BindingFlags.Public);
            if (nestedType == null) return 0;
            return Enum.Parse(nestedType, publicEnumValue.ToString());
        }

        public static object CreateEmptyGraphData()
        {
            var graphDataType = FindType("UnityEditor.ShaderGraph.GraphData");
            if (graphDataType == null) throw new Exception("找不到 GraphData 类型");

            var graphData = Activator.CreateInstance(graphDataType);

            var isSubGraphField = graphDataType.GetField("isSubGraph", BindingFlags.Public | BindingFlags.Instance);
            isSubGraphField?.SetValue(graphData, false);

            var addContextsMethod = graphDataType.GetMethod("AddContexts", BindingFlags.Public | BindingFlags.Instance);
            addContextsMethod?.Invoke(graphData, null);

            var setPrecisionMethod = graphDataType.GetMethod("SetGraphDefaultPrecision", new[] { typeof(GraphPrecision) });
            if (setPrecisionMethod != null)
                setPrecisionMethod.Invoke(graphData, new object[] { GraphPrecision.Single });

            return graphData;
        }
    }
}