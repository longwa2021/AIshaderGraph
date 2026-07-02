using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.ShaderGraph.Internal;
using 龙哥的秘密花园.节点库;

namespace 龙哥的秘密花园.ShaderGraphBuilder
{
    
    /// <summary>
    /// 从 JSON 描述构建 ShaderGraph
    /// </summary>
    public static class ShaderGraphBuilder
    {
       private static int GetSlotIdForNode(object node, string slotPath, bool isInput)
       {
           // 尝试子图端口路径解析
           if (SlotPathResolver.TryGetSubGraphPortDescriptor(slotPath, out var descriptor))
           {
               if (node.GetType().Name != "SubGraphNode")
                   throw new Exception($"路径 {slotPath} 指向子图端口，但节点类型不是 SubGraphNode");
       
               int id = ShaderGraphReflectionHelper.GetSlotIdByName(node, descriptor.PortName, isInput);
               if (id < 0)
                   throw new Exception($"在子图节点中未找到端口: {descriptor.PortName} ({(isInput ? "Input" : "Output")})");
               return id;
           }
       
           string nodeTypeName = slotPath.Split('.')[0];

if (nodeTypeName == "CustomFunctionNode")
{
    string portName = slotPath.Split('.')[2];
    
    // 直接获取插槽列表
    var methodName = isInput ? "GetInputSlots" : "GetOutputSlots";
    MethodInfo targetMethod = null;
    foreach (var m in node.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
    {
        if (m.Name == methodName && m.IsGenericMethodDefinition)
        {
            var parameters = m.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType.IsGenericType &&
                parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(List<>))
            {
                targetMethod = m;
                break;
            }
        }
    }
    if (targetMethod == null)
        throw new Exception($"未找到方法: {methodName}");
    
    var materialSlotType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.MaterialSlot");
    var genericMethod = targetMethod.MakeGenericMethod(materialSlotType);
    var listType = typeof(List<>).MakeGenericType(materialSlotType);
    var slotList = Activator.CreateInstance(listType);
    genericMethod.Invoke(node, new object[] { slotList });
    
    var countProp = listType.GetProperty("Count");
    var itemProp = listType.GetProperty("Item");
    int count = (int)countProp.GetValue(slotList);
    Debug.Log($"[GetSlotIdForNode] CustomFunctionNode 插槽数量: {count}, 方向: {(isInput ? "Input" : "Output")}");
    for (int i = 0; i < count; i++)
    {
        var slot = itemProp.GetValue(slotList, new object[] { i });
        string displayName = ShaderGraphReflectionHelper.GetSlotDisplayName(slot);
        string stripped = System.Text.RegularExpressions.Regex.Replace(displayName, @"\(\d+\)$", "");
        if (stripped == portName || displayName == portName)
        {
            return ShaderGraphReflectionHelper.GetSlotId(slot);
        }
    }
    
    var bodyField = node.GetType().GetField("m_FunctionBody", BindingFlags.NonPublic | BindingFlags.Instance);
    string body = bodyField?.GetValue(node) as string ?? "null";
    Debug.LogError($"未找到端口 {portName}，函数体: {body}");
    throw new Exception($"在节点 CustomFunctionNode 中未找到端口: {portName}");
}
           // 其他内置节点，使用原有 ID 映射
           return SlotPathResolver.GetSlotId(slotPath);
       }
        // MasterNode 输入槽名称到 SurfaceBlockType 的映射（保留用于向后兼容，新 JSON 使用 Blocks 数组）
        private static readonly Dictionary<string, SurfaceBlockType> MasterSlotToBlockMap = new Dictionary<string, SurfaceBlockType>
        {
            { "BaseColor", SurfaceBlockType.BaseColor },
            { "Normal", SurfaceBlockType.NormalTS },
            { "NormalTS", SurfaceBlockType.NormalTS },
            { "NormalOS", SurfaceBlockType.NormalOS },
            { "NormalWS", SurfaceBlockType.NormalWS },
            { "Metallic", SurfaceBlockType.Metallic },
            { "Specular", SurfaceBlockType.Specular },
            { "Smoothness", SurfaceBlockType.Smoothness },
            { "Occlusion", SurfaceBlockType.Occlusion },
            { "Emission", SurfaceBlockType.Emission },
            { "Alpha", SurfaceBlockType.Alpha },
            { "AlphaClipThreshold", SurfaceBlockType.AlphaClipThreshold },
            { "CoatMask", SurfaceBlockType.CoatMask },
            { "CoatSmoothness", SurfaceBlockType.CoatSmoothness },
            { "MapRightTopBack", SurfaceBlockType.MapRightTopBack },
            { "MapLeftBottomFront", SurfaceBlockType.MapLeftBottomFront },
            { "AbsorptionStrength", SurfaceBlockType.AbsorptionStrength },
            { "SpriteMask", SurfaceBlockType.SpriteMask },
        };

        /// <summary>
        /// 从 JSON 文件路径构建
        /// </summary>
        public static void BuildFromJsonFile(string jsonPath, GraphDataContext ctx)
        {
            string json = System.IO.File.ReadAllText(jsonPath);
            BuildFromJson(json, ctx);
        }
     private static string EnsureHLSLFileForCustomFunction(string functionName, string functionBody, string functionSourceHint, string graphName)
{
    // 如果 functionSource 已经是 GUID，则直接返回
    if (!string.IsNullOrEmpty(functionSourceHint) && Guid.TryParse(functionSourceHint, out _))
        return functionSourceHint;

    // 确定保存路径
    string directory = "Assets/GeneratedHLSL/";
    if (!AssetDatabase.IsValidFolder(directory))
    {
        string parent = Path.GetDirectoryName(directory.TrimEnd('/'));
        string folderName = Path.GetFileName(directory.TrimEnd('/'));
        if (!AssetDatabase.IsValidFolder(parent))
            parent = "Assets";
        AssetDatabase.CreateFolder(parent, folderName);
    }

    string safeName = string.Join("_", functionName.Split(Path.GetInvalidFileNameChars()));
    string fileName = $"{safeName}_{graphName}.hlsl";
    string filePath = Path.Combine(directory, fileName);
    filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);

    var sb = new System.Text.StringBuilder();
    sb.AppendLine("// Auto-generated Custom Function HLSL file with URP Lighting support");
    sb.AppendLine($"#ifndef CUSTOM_FUNCTION_{safeName.ToUpper()}_INCLUDED");
    sb.AppendLine($"#define CUSTOM_FUNCTION_{safeName.ToUpper()}_INCLUDED");
    sb.AppendLine();
    // 关键：仅在非预览模式下包含 URP 光照库
    sb.AppendLine("#if !SHADERGRAPH_PREVIEW");
    sb.AppendLine("#include \"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl\"");
    sb.AppendLine("#endif");
    sb.AppendLine();

    // 确保函数名包含 _float 后缀
    string modifiedBody = functionBody;
    if (!modifiedBody.Contains($"{functionName}_float"))
    {
        modifiedBody = System.Text.RegularExpressions.Regex.Replace(
            modifiedBody,
            $@"\b{functionName}\s*\(",
            $"{functionName}_float(");
    }
    sb.AppendLine(modifiedBody);
    sb.AppendLine();
    sb.AppendLine("#endif");

    // 写入文件
    string content = sb.ToString().Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
    File.WriteAllText(filePath, content);
    AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);

    string guid = AssetDatabase.AssetPathToGUID(filePath);
    Debug.Log($"[ShaderGraphBuilder] 已创建 HLSL 文件（含 URP 光照支持）: {filePath} (GUID: {guid})");
    return guid;
}
     
     
     
     
     
     
     
     
     /// <summary>
/// 从 JSON 字符串构建
/// </summary>
public static void BuildFromJson(string json, GraphDataContext ctx)
{
  
    
    var root = JObject.Parse(json);
    var nodesArray = root["Nodes"] as JArray;
    var connectionsArray = root["Connections"] as JArray;

    if (nodesArray == null)
        throw new Exception("JSON 缺少 Nodes 数组");

  

    if (nodesArray == null)
        throw new Exception("JSON 缺少 Nodes 数组");
   

    var nodeMap = new Dictionary<string, object>();
    var propertyMap = new Dictionary<string, AbstractShaderProperty>();

    // 1. 配置管线（根据 Pipeline 和 Target）
    ConfigurePipeline(root, ctx);

    // 2. 解析 GraphSettings
    if (root["GraphSettings"] is JObject settings)
        ApplyGraphSettings(settings, ctx);

    // 3. 创建黑板属性
    if (root["Blackboard"] is JArray blackboardArray)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== 开始添加黑板属性 ===");

        foreach (var propToken in blackboardArray)
        {
            try
            {
                var propName = propToken["Name"]?.ToString();
                var propType = propToken["Type"]?.ToString();
                var defaultValue = propToken["DefaultValue"];
                if (string.IsNullOrEmpty(propName) || string.IsNullOrEmpty(propType))
                    throw new Exception("黑板属性缺少 Name 或 Type");

                sb.AppendLine($"属性名: {propName}, 类型: {propType}, 默认值: {defaultValue}");

                var property = CreateShaderProperty(propName, propType, (JObject)propToken);
                ctx.AddShaderProperty(property);
                propertyMap[propName] = property;
                sb.AppendLine($"  实际引用名称: {property.referenceName}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"  处理属性时出错: {ex.Message}");
                Debug.LogWarning($"跳过属性 {propToken["Name"]}: {ex.Message}");
            }
        }

        foreach (var key in propertyMap.Keys)
        {
            sb.AppendLine($"  - {key}");
        }

    }

    // 4. 创建所有节点（使用 AddNodeWithId 传递 JSON 中的 Id）
    foreach (var nodeToken in nodesArray)
    {
        var type = nodeToken["Type"]?.ToString();
        if (type == "MasterNode") continue;

        var id = nodeToken["Id"]?.ToString();
        if (string.IsNullOrEmpty(id))
            throw new Exception("节点缺少 Id");

        var position = ParsePosition(nodeToken);
        object node = null;

        if (type == "PropertyNode")
        {
            
            var propName = nodeToken["PropertyName"]?.ToString();
            if (string.IsNullOrEmpty(propName))
                throw new Exception("PropertyNode 缺少 PropertyName");
            if (!propertyMap.TryGetValue(propName, out var property))
                throw new Exception($"找不到属性: {propName}");
            node = ctx.AddNodeWithId(ShaderGraphNodeType.PropertyNode, id,
                new PropertyNodeParams { propertyReferenceName = property.referenceName },
                position);
        }
        else if (type == "SubGraphNode")
        {
            // 处理子图节点
            var identifier = nodeToken["Identifier"]?.ToString();
            if (string.IsNullOrEmpty(identifier))
                throw new Exception("SubGraphNode 缺少 Identifier");

            var idTypeStr = nodeToken["IdentifierType"]?.ToString();
            SubGraphIdentifierType idType = SubGraphIdentifierType.Auto;
            if (!string.IsNullOrEmpty(idTypeStr))
                Enum.TryParse(idTypeStr, true, out idType);

            // 调用 SubGraphResolver 解析资产（会自动重建）
            var subGraphAsset = SubGraphResolver.Resolve(identifier, idType);
            if (subGraphAsset == null)
                throw new Exception($"无法解析子图: {identifier}");

            var subParams = new SubGraphNodeParams
            {
                identifier = identifier,
                identifierType = idType
            };

            node = ctx.AddNodeWithId(ShaderGraphNodeType.SubGraphNode, id, subParams, position);
        }
        else
        {
            var parameters = BuildNodeParameters(type, nodeToken);
            var nodeType = ParseNodeType(type);
            node = ctx.AddNodeWithId(nodeType, id, parameters, position);
        }

        if (node == null)
            throw new Exception($"创建节点失败: {type} ({id})");

        nodeMap[id] = node;
    }

    // 5. 处理节点间连接
    if (connectionsArray != null)
    {
        foreach (var connToken in connectionsArray)
        {
            var fromNodeId = connToken["From"]?.ToString();
            var fromSlotPath = connToken["FromSlot"]?.ToString();
            var toNodeId = connToken["To"]?.ToString();
            var toSlotPath = connToken["ToSlot"]?.ToString();

            if (string.IsNullOrEmpty(fromNodeId) || string.IsNullOrEmpty(fromSlotPath) ||
                string.IsNullOrEmpty(toNodeId) || string.IsNullOrEmpty(toSlotPath))
                continue;

            // 旧格式兼容：如果目标是 "Master"，则视为块连接
            if (toNodeId == "Master")
            {
                ConnectToBlockLegacy(ctx, nodeMap, fromNodeId, fromSlotPath, toSlotPath);
                continue;
            }
            

            var fromNode = nodeMap[fromNodeId];
            var toNode = nodeMap[toNodeId];

            int fromSlotId = GetSlotIdForNode(fromNode, fromSlotPath, isInput: false);
            int toSlotId = GetSlotIdForNode(toNode, toSlotPath, isInput: true);

            bool connected = ctx.ConnectSlots(fromNode, fromSlotId, toNode, toSlotId);
            if (!connected)
                Debug.LogWarning($"连接失败: {fromNodeId}.{fromSlotPath} -> {toNodeId}.{toSlotPath}");
        }
    }

    // 6. 处理新格式的 Blocks 数组
if (root["Blocks"] is JArray blocksArray)
{
    foreach (var blockToken in blocksArray)
    {
        var contextStr = blockToken["Context"]?.ToString();
        var blockTypeStr = blockToken["Type"]?.ToString();
        var sourceNodeId = blockToken["SourceNode"]?.ToString();
        var sourceSlotPath = blockToken["SourceSlot"]?.ToString();
        var valueToken = blockToken["Value"];

        if (string.IsNullOrEmpty(contextStr) || string.IsNullOrEmpty(blockTypeStr))
        {
            Debug.LogWarning("Block 条目缺少 Context 或 Type 字段，已跳过");
            continue;
        }

        var contextType = Enum.Parse<ShaderGraphContextType>(contextStr);

        // 获取或创建目标块节点
        object targetBlock = null;
        if (contextType == ShaderGraphContextType.Fragment)
        {
            var blockType = Enum.Parse<SurfaceBlockType>(blockTypeStr);
    
            // 处理 AlphaClipThreshold：先确保 Alpha Clip 启用，然后获取或创建块
            if (blockType == SurfaceBlockType.AlphaClipThreshold||blockType ==SurfaceBlockType.Alpha)
            {
                ctx.URP.SetAlphaClip(true);
                // // 确保块存在（如果不存在则添加）
                targetBlock = ctx.URP.GetSurfaceBlock(SurfaceBlockType.AlphaClipThreshold);
                if (targetBlock == null)
                {
                    ctx.URP.AddSurfaceBlock(SurfaceBlockType.AlphaClipThreshold);
                    targetBlock = ctx.URP.GetSurfaceBlock(SurfaceBlockType.AlphaClipThreshold);
                }
                targetBlock = ctx.URP.GetSurfaceBlock(SurfaceBlockType.Alpha);
                if (targetBlock == null)
                {
                    ctx.URP.AddSurfaceBlock(SurfaceBlockType.Alpha);
                    targetBlock = ctx.URP.GetSurfaceBlock(SurfaceBlockType.Alpha);
                }
            }
            else
            {
                // 其他块的处理保持不变
                targetBlock = ctx.URP.GetSurfaceBlock(blockType);
                if (targetBlock == null)
                {
                    ctx.URP.AddSurfaceBlock(blockType);
                    targetBlock = ctx.URP.GetSurfaceBlock(blockType);
                }
            }
        }
        else if (contextType == ShaderGraphContextType.Vertex)
        {
            var blockType = Enum.Parse<VertexBlockType>(blockTypeStr);
            targetBlock = ctx.URP.GetVertexBlock(blockType);
            if (targetBlock == null)
            {
                ctx.URP.AddVertexBlock(blockType);
                targetBlock = ctx.URP.GetVertexBlock(blockType);
            }
        }

        if (targetBlock == null)
        {
            Debug.LogError($"无法添加或获取块: {blockTypeStr}");
            continue;
        }

        // 处理连接或设置默认值
        if (!string.IsNullOrEmpty(sourceNodeId) && !string.IsNullOrEmpty(sourceSlotPath))
        {
            // 有源节点：进行连接
            var sourceNode = nodeMap[sourceNodeId];
            int sourceSlotId = GetSlotIdForNode(sourceNode, sourceSlotPath, isInput: false);
            ctx.ConnectSlots(sourceNode, sourceSlotId, targetBlock, 0);
        }
        else if (valueToken != null)
        {
            // 无源节点但有 Value：直接设置块的默认值
            SetBlockDefaultValue(targetBlock, valueToken, blockTypeStr);
        }
        else
        {
            Debug.LogWarning($"块 {blockTypeStr} 既无 SourceNode/SourceSlot 也未提供 Value，保持默认值");
        }
    }
}

    // 7. 应用自动布局（使用 nodeMap 中的节点对象，ID 为 JSON 中的原始 Id）
    var nodesForLayout = nodesArray.Select(t => new { Id = t["Id"].ToString(), Type = t["Type"].ToString() });
    var connectionsForLayout = connectionsArray?.Select(c => new { From = c["From"].ToString(), To = c["To"].ToString() }) ?? Enumerable.Empty<dynamic>();
    ctx.ApplyAutoLayout(nodesForLayout, connectionsForLayout, nodeMap);

    ctx.Save();
    Debug.Log("ShaderGraph 构建完成");
}

        #region 辅助方法
        
        
        private static object GetSlotFromBlock(object blockNode, int slotId)
        {
            // 方法1：直接读取 m_Slots 列表
            var slotsField = blockNode.GetType().GetField("m_Slots", BindingFlags.NonPublic | BindingFlags.Instance);
            if (slotsField != null)
            {
                var slotsList = slotsField.GetValue(blockNode) as System.Collections.IList;
                if (slotsList != null)
                {
                    foreach (var item in slotsList)
                    {
                        object slotItem = item;
                        // 如果是 JsonData<T> 包装，取出真实值
                        var valueProp = item.GetType().GetProperty("value");
                        if (valueProp != null)
                            slotItem = valueProp.GetValue(item);

                        if (slotItem != null)
                        {
                            var idProp = slotItem.GetType().GetProperty("id");
                            if (idProp != null)
                            {
                                int id = (int)idProp.GetValue(slotItem);
                                if (id == slotId)
                                    return slotItem;
                            }
                        }
                    }
                }
            }

            // 方法2：降级 - 尝试调用非泛型版本或通过 FindSlot<MaterialSlot>
            var findSlotMethod = blockNode.GetType().GetMethod("FindSlot", new[] { typeof(int) });
            if (findSlotMethod != null && !findSlotMethod.IsGenericMethod)
            {
                return findSlotMethod.Invoke(blockNode, new object[] { slotId });
            }

            return null;
        }
        /// <summary>
        /// 直接设置 BlockNode 的默认输入值
        /// </summary>
       private static void SetBlockDefaultValue(object blockNode, JToken valueToken, string blockType)
{
    var actualBlock = ShaderGraphReflectionHelper.UnwrapJsonData(blockNode);
    if (actualBlock == null) return;

    // ---------- 获取输入插槽 ----------
    object slot = null;
    var blockTypeRef = actualBlock.GetType();

    // 方法1：通过泛型方法 FindInputSlot<MaterialSlot>(0)
    var materialSlotType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.MaterialSlot");
    if (materialSlotType != null)
    {
        var findInputSlotMethodDef = blockTypeRef.GetMethod("FindInputSlot", BindingFlags.Public | BindingFlags.Instance);
        if (findInputSlotMethodDef != null && findInputSlotMethodDef.IsGenericMethodDefinition)
        {
            try
            {
                var genericMethod = findInputSlotMethodDef.MakeGenericMethod(materialSlotType);
                slot = genericMethod.Invoke(actualBlock, new object[] { 0 });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"调用 FindInputSlot<MaterialSlot> 失败: {ex.Message}");
            }
        }
    }

    // 方法2：回退 - 遍历 m_Slots 列表查找 ID 为 0 的输入插槽
    if (slot == null)
    {
        var slotsField = blockTypeRef.GetField("m_Slots", BindingFlags.NonPublic | BindingFlags.Instance);
        if (slotsField != null)
        {
            var slotsList = slotsField.GetValue(actualBlock) as System.Collections.IList;
            if (slotsList != null)
            {
                foreach (var item in slotsList)
                {
                    var unwrapped = ShaderGraphReflectionHelper.UnwrapJsonData(item);
                    if (unwrapped == null) continue;

                    // 检查是否为输入插槽且 ID 为 0
                    var isInputProp = unwrapped.GetType().GetProperty("isInputSlot", BindingFlags.Public | BindingFlags.Instance);
                    if (isInputProp != null && (bool)isInputProp.GetValue(unwrapped) == true)
                    {
                        var idProp = unwrapped.GetType().GetProperty("id", BindingFlags.Public | BindingFlags.Instance);
                        if (idProp != null && (int)idProp.GetValue(unwrapped) == 0)
                        {
                            slot = unwrapped;
                            break;
                        }
                    }
                }
            }
        }
    }

    if (slot == null)
    {
        Debug.LogWarning($"无法获取块 {blockType} 的输入插槽 (ID=0)，无法设置默认值");
        return;
    }

    // 转换值并设置
    object convertedValue = ConvertValueForSlot(slot, valueToken, blockType);
    if (convertedValue != null)
    {
        var valueProp = slot.GetType().GetProperty("value");
        if (valueProp != null && valueProp.CanWrite)
        {
            valueProp.SetValue(slot, convertedValue);
            Debug.Log($"已设置块 {blockType} 的默认值为: {convertedValue}");
        }
        else
        {
            Debug.LogWarning($"块 {blockType} 的插槽不支持设置 value 属性");
        }
    }
}
        
        
        
        
        
        
        
        private static object ConvertValueForSlot(object slot, JToken valueToken, string blockType)
{
    // 根据块类型确定目标类型
    switch (blockType)
    {
        case "BaseColor":
        case "Emission":
        case "Specular":
        case "MapRightTopBack":
        case "MapLeftBottomFront":
        case "SpriteMask":
            // 颜色类型
            if (valueToken.Type == JTokenType.Array)
            {
                var arr = valueToken as JArray;
                if (arr.Count >= 3)
                {
                    float r = arr[0].Value<float>();
                    float g = arr[1].Value<float>();
                    float b = arr[2].Value<float>();
                    float a = arr.Count > 3 ? arr[3].Value<float>() : 1.0f;
                    return new Color(r, g, b, a);
                }
            }
            else if (valueToken.Type == JTokenType.Float || valueToken.Type == JTokenType.Integer)
            {
                float v = valueToken.Value<float>();
                return new Color(v, v, v, 1.0f);
            }
            break;

        case "Alpha":
        case "AlphaClipThreshold":
        case "Metallic":
        case "Smoothness":
        case "Occlusion":
        case "CoatMask":
        case "CoatSmoothness":
        case "AbsorptionStrength":
            // 标量
            if (valueToken.Type == JTokenType.Float || valueToken.Type == JTokenType.Integer)
                return valueToken.Value<float>();
            break;

        case "NormalTS":
        case "NormalOS":
        case "NormalWS":
        case "Position":
        case "Tangent":
            // 三维向量
            if (valueToken.Type == JTokenType.Array)
            {
                var arr = valueToken as JArray;
                if (arr.Count >= 3)
                {
                    float x = arr[0].Value<float>();
                    float y = arr[1].Value<float>();
                    float z = arr[2].Value<float>();
                    return new Vector3(x, y, z);
                }
            }
            break;
    }

    // 通用转换尝试
    try
    {
        var slotType = slot.GetType();
        var valuePropType = slotType.GetProperty("value")?.PropertyType;
        if (valuePropType != null)
        {
            if (valuePropType == typeof(float) && (valueToken.Type == JTokenType.Float || valueToken.Type == JTokenType.Integer))
                return valueToken.Value<float>();
            if (valuePropType == typeof(Vector2) && valueToken.Type == JTokenType.Array)
            {
                var arr = valueToken as JArray;
                if (arr.Count >= 2) return new Vector2(arr[0].Value<float>(), arr[1].Value<float>());
            }
            if (valuePropType == typeof(Vector3) && valueToken.Type == JTokenType.Array)
            {
                var arr = valueToken as JArray;
                if (arr.Count >= 3) return new Vector3(arr[0].Value<float>(), arr[1].Value<float>(), arr[2].Value<float>());
            }
            if (valuePropType == typeof(Vector4) && valueToken.Type == JTokenType.Array)
            {
                var arr = valueToken as JArray;
                if (arr.Count >= 4) return new Vector4(arr[0].Value<float>(), arr[1].Value<float>(), arr[2].Value<float>(), arr[3].Value<float>());
            }
            if (valuePropType == typeof(Color) && valueToken.Type == JTokenType.Array)
            {
                var arr = valueToken as JArray;
                if (arr.Count >= 3)
                {
                    float a = arr.Count > 3 ? arr[3].Value<float>() : 1.0f;
                    return new Color(arr[0].Value<float>(), arr[1].Value<float>(), arr[2].Value<float>(), a);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"转换块 {blockType} 的值时出错: {ex.Message}");
    }

    return null;
}
        private static void ConfigurePipeline(JObject root, GraphDataContext ctx)
        {
            if (root["Pipeline"] == null || root["Target"] == null) return;

            string pipeline = root["Pipeline"].ToString();
            string target = root["Target"].ToString();

            if (pipeline == "URP")
            {
                switch (target)
                {
                    case "Unlit": ctx.URP.SetupAsURPUnlit(); break;
                    case "Lit": ctx.URP.SetupAsURPLit(); break;
                    case "Fullscreen": ctx.URP.SetupAsURPFullscreen(); break;
                    case "Canvas": ctx.URP.SetupAsURPCanvas(); break;
                    case "SixWay": ctx.URP.SetupAsURPSixWay(); break;
                    case "Decal": ctx.URP.SetupAsURPDecal(); break;
                    case "SpriteLit": ctx.URP.SetupAsURPSpriteLit(); break;
                    case "SpriteUnlit": ctx.URP.SetupAsURPSpriteUnlit(); break;
                    case "SpriteCustomLit": ctx.URP.SetupAsURPSpriteCustomLit(); break;
                    default: throw new Exception($"不支持的 URP Target: {target}");
                }
            }
            else if (pipeline == "BuiltIn")
            {
                switch (target)
                {
                    case "Lit": ctx.BuiltIn.SetupAsBuiltInLit(); break;
                    case "Unlit": ctx.BuiltIn.SetupAsBuiltInUnlit(); break;
                    case "Canvas": ctx.BuiltIn.SetupAsBuiltInCanvas(); break;
                    default: throw new Exception($"不支持的 BuiltIn Target: {target}");
                }
            }
            else throw new Exception($"不支持的管线: {pipeline}");
        }
        
        private static void ApplyGraphSettings(JObject settings, GraphDataContext ctx)
        {
            if (settings.TryGetValue("AllowMaterialOverride", out var allow))
                ctx.URP.SetAllowMaterialOverride(allow.Value<bool>());
            if (settings.TryGetValue("WorkflowMode", out var workflow))
                ctx.URP.SetWorkflowMode(Enum.Parse<WorkflowMode>(workflow.ToString(), true));
            if (settings.TryGetValue("SurfaceType", out var surface))
                ctx.URP.SetSurfaceType(Enum.Parse<SurfaceType>(surface.ToString(), true));
            if (settings.TryGetValue("BlendMode", out var blend))
                ctx.URP.SetBlendMode(Enum.Parse<BlendMode>(blend.ToString(), true));
            if (settings.TryGetValue("RenderFace", out var face))
                ctx.URP.SetRenderFace(Enum.Parse<RenderFace>(face.ToString(), true));
            if (settings.TryGetValue("ZWriteControl", out var zwrite))
                ctx.URP.SetZWriteControl(Enum.Parse<ZWriteControl>(zwrite.ToString(), true));
            if (settings.TryGetValue("ZTestMode", out var ztest))
                ctx.URP.SetZTestMode(Enum.Parse<ZTestMode>(ztest.ToString(), true));
            if (settings.TryGetValue("AlphaClip", out var alphaClip))
                ctx.URP.SetAlphaClip(alphaClip.Value<bool>());
            if (settings.TryGetValue("CastShadows", out var cast))
                ctx.URP.SetCastShadows(cast.Value<bool>());
            if (settings.TryGetValue("ReceiveShadows", out var receive))
                ctx.URP.SetReceiveShadows(receive.Value<bool>());
            if (settings.TryGetValue("SupportsLODCrossFade", out var lod))
                ctx.URP.SetSupportsLODCrossFade(lod.Value<bool>());
            if (settings.TryGetValue("AdditionalMotionVectorMode", out var motion))
                ctx.URP.SetAdditionalMotionVectorMode(Enum.Parse<AdditionalMotionVectorMode>(motion.ToString(), true));
            if (settings.TryGetValue("AlembicMotionVectors", out var alembic))
                ctx.URP.SetAlembicMotionVectors(alembic.Value<bool>());
            if (settings.TryGetValue("FragmentNormalSpace", out var normalSpace))
                ctx.URP.SetFragmentNormalSpace(Enum.Parse<NormalDropOffSpace>(normalSpace.ToString(), true));
            if (settings.TryGetValue("CustomEditorGUI", out var editor))
                ctx.URP.SetCustomEditorGUI(editor.ToString());
            if (settings.TryGetValue("SupportVFX", out var vfx))
                ctx.URP.SetSupportVFX(vfx.Value<bool>());
        }

        private static void ConnectToBlockLegacy(GraphDataContext ctx, Dictionary<string, object> nodeMap,
            string fromNodeId, string fromSlotPath, string toSlotPath)
        {
            var sourceNode = nodeMap[fromNodeId];
            var sourceSlotId = SlotPathResolver.GetSlotId(fromSlotPath);
            var blockName = toSlotPath.Split('.').Last();

            // 特殊处理：Alpha 相关块需要启用 AlphaClip
            if (blockName == "Alpha" || blockName == "AlphaClipThreshold")
                ctx.URP.SetAlphaClip(true);

            // 尝试解析为片元块
            if (Enum.TryParse<SurfaceBlockType>(blockName, true, out var surfaceBlock))
            {
                // 获取块，若不存在则自动添加
                var targetBlock = ctx.URP.GetSurfaceBlock(surfaceBlock);
                if (targetBlock == null)
                {
                    ctx.URP.AddSurfaceBlock(surfaceBlock);
                    targetBlock = ctx.URP.GetSurfaceBlock(surfaceBlock);
                    if (targetBlock == null)
                    {
                        Debug.LogWarning($"当前图形不支持或无法添加块 {surfaceBlock}，跳过连接 (from {fromNodeId})");
                        return;
                    }
                }
                ctx.URP.ConnectToBlock(sourceNode, sourceSlotId, ShaderGraphContextType.Fragment, surfaceBlock);
                return;
            }

            // 尝试解析为顶点块
            if (Enum.TryParse<VertexBlockType>(blockName, true, out var vertexBlock))
            {
                var targetBlock = ctx.URP.GetVertexBlock(vertexBlock);
                if (targetBlock == null)
                {
                    ctx.URP.AddVertexBlock(vertexBlock);
                    targetBlock = ctx.URP.GetVertexBlock(vertexBlock);
                    if (targetBlock == null)
                    {
                        Debug.LogWarning($"当前图形不支持或无法添加顶点块 {vertexBlock}，跳过连接 (from {fromNodeId})");
                        return;
                    }
                }
                ctx.URP.ConnectToBlock(sourceNode, sourceSlotId, ShaderGraphContextType.Vertex, vertexBlock);
                return;
            }

            throw new Exception($"未知块类型: {blockName}");
        }
        private static Rect ParsePosition(JToken nodeToken)
        {
            var pos = new Rect(100, 100, 150, 80);
            if (nodeToken["Position"] is JArray posArray && posArray.Count >= 2)
            {
                pos.x = posArray[0].Value<float>();
                pos.y = posArray[1].Value<float>();
            }
            return pos;
        }

        private static AbstractShaderProperty CreateShaderProperty(string name, string type, JObject propToken)
{
    var defaultValue = propToken["DefaultValue"];

    switch (type)
    {
        case "Float":
            float floatVal = defaultValue?.Value<float>() ?? 0f;
            FloatType floatType = FloatType.Default;
            Vector2 rangeValues = Vector2.zero;

            // 解析 Mode 字段（支持 "Default", "Slider", "Integer", "Enum"）
            if (propToken.TryGetValue("Mode", out var modeToken))
            {
                string modeStr = modeToken.ToString();
                if (modeStr.Equals("Slider", StringComparison.OrdinalIgnoreCase))
                    floatType = FloatType.Slider;
                else if (modeStr.Equals("Integer", StringComparison.OrdinalIgnoreCase))
                    floatType = FloatType.Integer;
                else if (modeStr.Equals("Enum", StringComparison.OrdinalIgnoreCase))
                    floatType = FloatType.Enum;
                // Default 保持默认值
            }

            // 解析 Range 字段（滑块模式下必需）
            if (propToken.TryGetValue("Range", out var rangeToken) && rangeToken is JArray rangeArr && rangeArr.Count >= 2)
            {
                rangeValues = new Vector2(rangeArr[0].Value<float>(), rangeArr[1].Value<float>());
                // 若未显式指定 Mode 为 Slider，但提供了 Range，自动设为 Slider
                if (floatType == FloatType.Default)
                    floatType = FloatType.Slider;
            }

            return ShaderPropertyFactory.CreateVector1Property(name, floatVal, floatType, rangeValues);

        case "Color":
            Color color = Color.white;
            if (defaultValue is JArray arr && arr.Count >= 3)
            {
                color = new Color(arr[0].Value<float>(), arr[1].Value<float>(), arr[2].Value<float>(),
                    arr.Count > 3 ? arr[3].Value<float>() : 1f);
            }
            ColorMode colorMode = ColorMode.Default;
            bool hdr = false;
            if (propToken.TryGetValue("Mode", out var colModeToken))
            {
                string modeStr = colModeToken.ToString();
                if (modeStr.Equals("HDR", StringComparison.OrdinalIgnoreCase))
                {
                    colorMode = ColorMode.HDR;
                    hdr = true;
                }
            }
            return ShaderPropertyFactory.CreateColorProperty(name, color, colorMode, hdr);

        // ... 其他 case 保持不变，只需将 defaultValue 的获取改为从 propToken 中提取
        case "Texture2D":
            Texture2D tex = null;
            if (defaultValue != null && defaultValue.Type == JTokenType.String)
            {
                string texName = defaultValue.Value<string>();
                if (texName == "white") tex = Texture2D.whiteTexture;
                else if (texName == "black") tex = Texture2D.blackTexture;
            }
            return ShaderPropertyFactory.CreateTexture2DProperty(name);

        case "Texture2DArray":
            return ShaderPropertyFactory.CreateTexture2DArrayProperty(name);

        case "Texture3D":
            return ShaderPropertyFactory.CreateTexture3DProperty(name);

        case "Cubemap":
            return ShaderPropertyFactory.CreateCubemapProperty(name);

        case "Vector2":
            Vector2 v2 = Vector2.zero;
            if (defaultValue is JArray arr2 && arr2.Count >= 2)
                v2 = new Vector2(arr2[0].Value<float>(), arr2[1].Value<float>());
            return ShaderPropertyFactory.CreateVector2Property(name, v2);

        case "Vector3":
            Vector3 v3 = Vector3.zero;
            if (defaultValue is JArray arr3 && arr3.Count >= 3)
                v3 = new Vector3(arr3[0].Value<float>(), arr3[1].Value<float>(), arr3[2].Value<float>());
            return ShaderPropertyFactory.CreateVector3Property(name, v3);

        case "Vector4":
            Vector4 v4 = Vector4.zero;
            if (defaultValue is JArray arr4 && arr4.Count >= 4)
                v4 = new Vector4(arr4[0].Value<float>(), arr4[1].Value<float>(), arr4[2].Value<float>(), arr4[3].Value<float>());
            return ShaderPropertyFactory.CreateVector4Property(name, v4);

        case "Boolean":
            bool boolVal = defaultValue?.Value<bool>() ?? false;
            return ShaderPropertyFactory.CreateBooleanProperty(name, boolVal);

        case "Matrix2":
            Vector2 row0 = new Vector2(1, 0);
            Vector2 row1 = new Vector2(0, 1);
            if (defaultValue is JArray m2 && m2.Count >= 4)
            {
                row0 = new Vector2(m2[0].Value<float>(), m2[1].Value<float>());
                row1 = new Vector2(m2[2].Value<float>(), m2[3].Value<float>());
            }
            return ShaderPropertyFactory.CreateMatrix2Property(name, row0, row1);

        case "Matrix3":
            Vector3 row0_3 = new Vector3(1, 0, 0);
            Vector3 row1_3 = new Vector3(0, 1, 0);
            Vector3 row2_3 = new Vector3(0, 0, 1);
            if (defaultValue is JArray m3 && m3.Count >= 9)
            {
                row0_3 = new Vector3(m3[0].Value<float>(), m3[1].Value<float>(), m3[2].Value<float>());
                row1_3 = new Vector3(m3[3].Value<float>(), m3[4].Value<float>(), m3[5].Value<float>());
                row2_3 = new Vector3(m3[6].Value<float>(), m3[7].Value<float>(), m3[8].Value<float>());
            }
            return ShaderPropertyFactory.CreateMatrix3Property(name, row0_3, row1_3, row2_3);

        case "Matrix4":
            Vector4 row0_4 = new Vector4(1, 0, 0, 0);
            Vector4 row1_4 = new Vector4(0, 1, 0, 0);
            Vector4 row2_4 = new Vector4(0, 0, 1, 0);
            Vector4 row3_4 = new Vector4(0, 0, 0, 1);
            if (defaultValue is JArray m4 && m4.Count >= 16)
            {
                row0_4 = new Vector4(m4[0].Value<float>(), m4[1].Value<float>(), m4[2].Value<float>(), m4[3].Value<float>());
                row1_4 = new Vector4(m4[4].Value<float>(), m4[5].Value<float>(), m4[6].Value<float>(), m4[7].Value<float>());
                row2_4 = new Vector4(m4[8].Value<float>(), m4[9].Value<float>(), m4[10].Value<float>(), m4[11].Value<float>());
                row3_4 = new Vector4(m4[12].Value<float>(), m4[13].Value<float>(), m4[14].Value<float>(), m4[15].Value<float>());
            }
            return ShaderPropertyFactory.CreateMatrix4Property(name, row0_4, row1_4, row2_4, row3_4);

        case "Gradient":
            return ShaderPropertyFactory.CreateGradientProperty(name);

        case "SamplerState":
            SamplerFilterMode filter = SamplerFilterMode.Linear;
            SamplerWrapMode wrap = SamplerWrapMode.Repeat;
            SamplerAnisotropic anisotropic = SamplerAnisotropic.None;
            if (propToken.TryGetValue("Filter", out var fToken))
                Enum.TryParse(fToken.ToString(), true, out filter);
            if (propToken.TryGetValue("Wrap", out var wToken))
                Enum.TryParse(wToken.ToString(), true, out wrap);
            if (propToken.TryGetValue("Anisotropic", out var aToken))
                Enum.TryParse(aToken.ToString(), true, out anisotropic);
            return ShaderPropertyFactory.CreateSamplerStateProperty(name, filter, wrap, anisotropic);

        case "VirtualTexture":
            int layers = defaultValue?.Value<int>() ?? 1;
            return ShaderPropertyFactory.CreateVirtualTextureProperty(name, layers);

        default:
            throw new Exception($"不支持的属性类型: {type}");
    }
}
        
        
        
        
        
        private static ShaderGraphNodeType ParseNodeType(string typeName)
        {
            if (Enum.TryParse<ShaderGraphNodeType>(typeName, true, out var result))
                return result;
            throw new Exception($"未知节点类型: {typeName}");
        }

        private static SurfaceBlockType ParseSurfaceBlockType(string blockName)
        {
            if (Enum.TryParse<SurfaceBlockType>(blockName, true, out var result))
                return result;
            throw new Exception($"未知块类型: {blockName}");
        }

  private static object BuildNodeParameters(string type, JToken nodeToken)
{
    var inputs = nodeToken["Inputs"] as JObject;

    // 辅助函数：从 JToken 安全获取数值（忽略字符串）
    float? GetFloat(JToken token)
    {
        if (token == null) return null;
        if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            return token.Value<float>();
        if (token.Type == JTokenType.String)
            return null; // 连接引用，忽略
        return null;
    }

    Vector2? GetVector2(JToken token)
    {
        if (token == null) return null;
        if (token.Type == JTokenType.Array && token.Count() >= 2)
        {
            var arr = token as JArray;
            if (arr[0].Type != JTokenType.String && arr[1].Type != JTokenType.String)
                return new Vector2(arr[0].Value<float>(), arr[1].Value<float>());
        }
        return null;
    }

    Vector3? GetVector3(JToken token)
    {
        if (token == null) return null;
        if (token.Type == JTokenType.Array && token.Count() >= 3)
        {
            var arr = token as JArray;
            if (arr[0].Type != JTokenType.String && arr[1].Type != JTokenType.String && arr[2].Type != JTokenType.String)
                return new Vector3(arr[0].Value<float>(), arr[1].Value<float>(), arr[2].Value<float>());
        }
        return null;
    }

    Vector4? GetVector4(JToken token)
    {
        if (token == null) return null;
        if (token.Type == JTokenType.Array && token.Count() >= 4)
        {
            var arr = token as JArray;
            if (arr[0].Type != JTokenType.String && arr[1].Type != JTokenType.String &&
                arr[2].Type != JTokenType.String && arr[3].Type != JTokenType.String)
                return new Vector4(arr[0].Value<float>(), arr[1].Value<float>(), arr[2].Value<float>(), arr[3].Value<float>());
        }
        return null;
    }

    Color? GetColor(JToken token)
    {
        var v4 = GetVector4(token);
        if (v4.HasValue)
            return new Color(v4.Value.x, v4.Value.y, v4.Value.z, v4.Value.w);
        return null;
    }

    switch (type)
    {
        case "ColorNode":
            var color = Color.white;
            var mode = ColorMode.Default;
            if (inputs != null)
            {
                var colorToken = inputs["color"];
                var colorVal = GetColor(colorToken);
                if (colorVal.HasValue)
                    color = colorVal.Value;
                var modeToken = inputs["mode"];
                if (modeToken != null)
                {
                    // 允许字符串或整数
                    if (modeToken.Type == JTokenType.String || modeToken.Type == JTokenType.Integer)
                        Enum.TryParse(modeToken.ToString(), true, out mode);
                }
            }
            return new ColorNodeParams { color = color, mode = mode };
        case "TransformNode":
            var transformParams = new TransformNodeParams();
            if (inputs != null)
            {
                if (inputs["from"]?.Type == JTokenType.String)
                    Enum.TryParse(inputs["from"].ToString(), true, out transformParams.from);
                if (inputs["to"]?.Type == JTokenType.String)
                    Enum.TryParse(inputs["to"].ToString(), true, out transformParams.to);
                if (inputs["conversionType"]?.Type == JTokenType.String)
                    Enum.TryParse(inputs["conversionType"].ToString(), true, out transformParams.conversionType);
                var normToken = inputs["normalize"];
                if (normToken != null && normToken.Type != JTokenType.String)
                    transformParams.normalize = normToken.Value<bool>();
            }
            return transformParams;
        case "Vector1Node":
            float v1 = 0;
            if (inputs != null)
            {
                var xToken = inputs["X"];
                var xVal = GetFloat(xToken);
                if (xVal.HasValue)
                    v1 = xVal.Value;
            }
            return new Vector1NodeParams { value = v1 };

        case "Vector2Node":
            Vector2 v2 = Vector2.zero;
            if (inputs != null)
            {
                var xVal = GetFloat(inputs["X"]);
                var yVal = GetFloat(inputs["Y"]);
                if (xVal.HasValue) v2.x = xVal.Value;
                if (yVal.HasValue) v2.y = yVal.Value;
            }
            return new Vector2NodeParams { value = v2 };

        case "Vector3Node":
            Vector3 v3 = Vector3.zero;
            if (inputs != null)
            {
                var xVal = GetFloat(inputs["X"]);
                var yVal = GetFloat(inputs["Y"]);
                var zVal = GetFloat(inputs["Z"]);
                if (xVal.HasValue) v3.x = xVal.Value;
                if (yVal.HasValue) v3.y = yVal.Value;
                if (zVal.HasValue) v3.z = zVal.Value;
            }
            return new Vector3NodeParams { value = v3 };

        case "Vector4Node":
            Vector4 v4 = Vector4.zero;
            if (inputs != null)
            {
                var xVal = GetFloat(inputs["X"]);
                var yVal = GetFloat(inputs["Y"]);
                var zVal = GetFloat(inputs["Z"]);
                var wVal = GetFloat(inputs["W"]);
                if (xVal.HasValue) v4.x = xVal.Value;
                if (yVal.HasValue) v4.y = yVal.Value;
                if (zVal.HasValue) v4.z = zVal.Value;
                if (wVal.HasValue) v4.w = wVal.Value;
            }
            return new Vector4NodeParams { value = v4 };

        case "BooleanNode":
            bool boolVal = false;
            if (inputs != null)
            {
                var valToken = inputs["Value"];
                if (valToken != null)
                {
                    if (valToken.Type == JTokenType.Boolean)
                        boolVal = valToken.Value<bool>();
                    else if (valToken.Type == JTokenType.String)
                        bool.TryParse(valToken.ToString(), out boolVal);
                    else if (valToken.Type == JTokenType.Integer)
                        boolVal = valToken.Value<int>() != 0;
                }
            }
            return new BooleanNodeParams { value = boolVal };

        case "IntegerNode":
            int intVal = 0;
            if (inputs != null)
            {
                var valToken = inputs["Value"];
                if (valToken != null)
                {
                    if (valToken.Type == JTokenType.Integer || valToken.Type == JTokenType.Float)
                        intVal = valToken.Value<int>();
                    else if (valToken.Type == JTokenType.String)
                        int.TryParse(valToken.ToString(), out intVal);
                }
            }
            return new IntegerNodeParams { value = intVal };

        case "SliderNode":
            Vector3 sliderVal = new Vector3(0, 0, 1);
            if (inputs != null && inputs["Value"] is JArray arr && arr.Count >= 3)
            {
                if (arr[0].Type != JTokenType.String && arr[1].Type != JTokenType.String && arr[2].Type != JTokenType.String)
                    sliderVal = new Vector3(arr[0].Value<float>(), arr[1].Value<float>(), arr[2].Value<float>());
            }
            return new SliderNodeParams { value = sliderVal };

        case "ConstantNode":
            ConstantTypeOption constant = ConstantTypeOption.PI;
            if (inputs != null)
            {
                var constToken = inputs["Constant"];
                if (constToken != null && (constToken.Type == JTokenType.String || constToken.Type == JTokenType.Integer))
                    Enum.TryParse(constToken.ToString(), true, out constant);
            }
            return new ConstantNodeParams { constant = constant };

        case "PositionNode":
            var posParams = new PositionNodeParams();
            if (inputs != null)
            {
                var token = inputs["PositionSource"];
                if (token != null && (token.Type == JTokenType.String || token.Type == JTokenType.Integer))
                    Enum.TryParse(token.ToString(), true, out posParams.positionSource);
                token = inputs["Space"];
                
                if (token != null && (token.Type == JTokenType.String || token.Type == JTokenType.Integer))
                    Enum.TryParse(token.ToString(), true, out posParams.space);
            }
            return posParams;
        case "NormalVectorNode":
            var normParams = new NormalVectorNodeParams();
            if (inputs != null)
            {
                var token = inputs["Space"];
                if (token != null && (token.Type == JTokenType.String || token.Type == JTokenType.Integer))
                    Enum.TryParse(token.ToString(), true, out normParams.space);
            }
            return normParams;
        case "ViewDirectionNode":
            var viewDirParams = new ViewDirectionNodeParams();
            if (inputs != null)
            {
                var token = inputs["Space"];
                if (token != null && (token.Type == JTokenType.String || token.Type == JTokenType.Integer))
                    Enum.TryParse(token.ToString(), true, out viewDirParams.space);
            }
            return viewDirParams;
        case "TangentVectorNode":
            var tanParams = new TangentVectorNodeParams();
            if (inputs != null)
            {
                var token = inputs["Space"];
                if (token != null && (token.Type == JTokenType.String || token.Type == JTokenType.Integer))
                    Enum.TryParse(token.ToString(), true, out tanParams.space);
            }
            return tanParams;
        case "BitangentVectorNode":
            var bitanParams = new BitangentVectorNodeParams();
            if (inputs != null)
            {
                var token = inputs["Space"];
                if (token != null && (token.Type == JTokenType.String || token.Type == JTokenType.Integer))
                    Enum.TryParse(token.ToString(), true, out bitanParams.space);
            }
            return bitanParams;
        case "ViewVectorNode":
            var viewVecParams = new ViewVectorNodeParams();
            if (inputs != null)
            {
                var token = inputs["Space"];
                if (token != null && (token.Type == JTokenType.String || token.Type == JTokenType.Integer))
                    Enum.TryParse(token.ToString(), true, out viewVecParams.space);
            }
            return viewVecParams;

        case "ScreenPositionNode":
            ScreenSpaceTypeOption screenType = ScreenSpaceTypeOption.Default;
            if (inputs != null)
            {
                var token = inputs["ScreenSpaceType"];
                if (token != null && (token.Type == JTokenType.String || token.Type == JTokenType.Integer))
                    Enum.TryParse(token.ToString(), true, out screenType);
            }
            return new ScreenPositionNodeParams { screenSpaceType = screenType };

        case "UVNode":
            UVChannelOption uvChannel = UVChannelOption.UV0;
            if (inputs != null)
            {
                var token = inputs["UVChannel"];
                if (token != null && (token.Type == JTokenType.String || token.Type == JTokenType.Integer))
                    Enum.TryParse(token.ToString(), true, out uvChannel);
            }
            return new UVNodeParams { uvChannel = uvChannel };

        

        case "SampleTexture2DNode":
            var texParams = new SampleTexture2DNodeParams();
            if (inputs != null)
            {
                var texTypeToken = inputs["textureType"];
                if (texTypeToken != null && (texTypeToken.Type == JTokenType.String || texTypeToken.Type == JTokenType.Integer))
                    Enum.TryParse(texTypeToken.ToString(), true, out texParams.textureType);

                var normSpaceToken = inputs["normalMapSpace"];
                if (normSpaceToken != null && (normSpaceToken.Type == JTokenType.String || normSpaceToken.Type == JTokenType.Integer))
                    Enum.TryParse(normSpaceToken.ToString(), true, out texParams.normalMapSpace);

                var enableMipToken = inputs["enableGlobalMipBias"];
                if (enableMipToken != null)
                {
                    if (enableMipToken.Type == JTokenType.Boolean)
                        texParams.enableGlobalMipBias = enableMipToken.Value<bool>();
                    else if (enableMipToken.Type == JTokenType.String)
                        bool.TryParse(enableMipToken.ToString(), out texParams.enableGlobalMipBias);
                }

                var mipModeToken = inputs["mipSamplingMode"];
                if (mipModeToken != null && (mipModeToken.Type == JTokenType.String || mipModeToken.Type == JTokenType.Integer))
                    Enum.TryParse(mipModeToken.ToString(), true, out texParams.mipSamplingMode);
            }
            return texParams;

        case "SamplerStateNode":
            var samplerParams = new SamplerStateNodeParams();
            if (inputs != null)
            {
                var filterToken = inputs["filter"];
                if (filterToken != null && (filterToken.Type == JTokenType.String || filterToken.Type == JTokenType.Integer))
                    Enum.TryParse(filterToken.ToString(), true, out samplerParams.filter);

                var wrapToken = inputs["wrap"];
                if (wrapToken != null && (wrapToken.Type == JTokenType.String || wrapToken.Type == JTokenType.Integer))
                    Enum.TryParse(wrapToken.ToString(), true, out samplerParams.wrap);

                var anisoToken = inputs["anisotropic"];
                if (anisoToken != null && (anisoToken.Type == JTokenType.String || anisoToken.Type == JTokenType.Integer))
                    Enum.TryParse(anisoToken.ToString(), true, out samplerParams.anisotropic);
            }
            return samplerParams;

        case "TriplanarNode":
            var triplanarParams = new TriplanarNodeParams();
            if (inputs != null)
            {
                var texTypeToken = inputs["textureType"];
                if (texTypeToken != null && (texTypeToken.Type == JTokenType.String || texTypeToken.Type == JTokenType.Integer))
                    Enum.TryParse(texTypeToken.ToString(), true, out triplanarParams.textureType);

                var inputSpaceToken = inputs["inputSpace"];
                if (inputSpaceToken != null && (inputSpaceToken.Type == JTokenType.String || inputSpaceToken.Type == JTokenType.Integer))
                    Enum.TryParse(inputSpaceToken.ToString(), true, out triplanarParams.inputSpace);

                var outputSpaceToken = inputs["normalOutputSpace"];
                if (outputSpaceToken != null && (outputSpaceToken.Type == JTokenType.String || outputSpaceToken.Type == JTokenType.Integer))
                    Enum.TryParse(outputSpaceToken.ToString(), true, out triplanarParams.normalOutputSpace);
            }
            return triplanarParams;

        case "LightTextureNode":
            var lightParams = new LightTextureNodeParams();
            if (inputs != null)
            {
                var token = inputs["blendStyle"];
                if (token != null && (token.Type == JTokenType.String || token.Type == JTokenType.Integer))
                    Enum.TryParse(token.ToString(), true, out lightParams.blendStyle);
            }
            return lightParams;

        case "CustomFunctionNode":
            var cfParams = new CustomFunctionNodeParams();
            if (inputs != null)
            {
                if (inputs.TryGetValue("sourceType", out var stToken))
                    Enum.TryParse(stToken.ToString(), true, out cfParams.sourceType);

                cfParams.functionName = inputs["functionName"]?.ToString() ?? cfParams.functionName;
                cfParams.functionSource = inputs["functionSource"]?.ToString() ?? cfParams.functionSource;
                cfParams.functionBody = inputs["functionBody"]?.ToString() ?? cfParams.functionBody;

                if (inputs.TryGetValue("usePragmas", out var upToken))
                    cfParams.usePragmas = upToken.Value<bool>();
            }

            // 如果是 File 模式，自动生成 .hlsl 文件并设置 GUID
            if (cfParams.sourceType == HlslSourceTypeOption.File)
            {
                // 如果 functionSource 为空或不是 GUID，则根据函数体生成文件
                if (string.IsNullOrEmpty(cfParams.functionSource) || !Guid.TryParse(cfParams.functionSource, out _))
                {
                    // 使用节点 ID 或函数名作为图名称的一部分（可以从外部传入，这里简单处理）
                    string graphName = nodeToken["Id"]?.ToString() ?? "Graph";
                    cfParams.functionSource = EnsureHLSLFileForCustomFunction(cfParams.functionName, cfParams.functionBody, cfParams.functionSource, graphName);
                }
            }

            if (nodeToken["Slots"] is JArray slotsArray)
            {
                var slotDefs = new List<SlotDefinition>();
                foreach (var slotToken in slotsArray)
                {
                    var def = new SlotDefinition
                    {
                        DisplayName = slotToken["DisplayName"]?.ToString(),
                        ShaderOutputName = slotToken["ShaderOutputName"]?.ToString(),
                        ValueType = Enum.Parse<SlotValueType>(slotToken["ValueType"]?.ToString() ?? "Vector1"),
                        IsInput = slotToken["IsInput"]?.Value<bool>() ?? true,
                        StageCapability = ShaderStageCapability.All
                    };
                    if (slotToken["StageCapability"] != null)
                    {
                        def.StageCapability = Enum.Parse<ShaderStageCapability>(slotToken["StageCapability"].ToString());
                    }
                    if (slotToken["DefaultValue"] is JArray dv && dv.Count >= 4)
                    {
                        def.DefaultValue = new Vector4(
                            dv[0].Value<float>(), dv[1].Value<float>(),
                            dv[2].Value<float>(), dv[3].Value<float>());
                    }
                    slotDefs.Add(def);
                }
                cfParams.Slots = slotDefs;
            }

            return cfParams;

        default:
            string paramsClassName = $"龙哥的秘密花园.节点库.{type}Params";
            Type paramsType = Type.GetType(paramsClassName);
            if (paramsType != null)
            {
                try
                {
                    var obj = Activator.CreateInstance(paramsType);
                    if (inputs != null)
                    {
                        foreach (var prop in paramsType.GetProperties())
                        {
                            var token = inputs[prop.Name];
                            if (token != null && token.Type != JTokenType.String)
                            {
                                var value = token.ToObject(prop.PropertyType);
                                prop.SetValue(obj, value);
                            }
                        }
                    }
                    return obj;
                }
                catch { }
            }
            return null;
    }
}
        #endregion
    }

   public static class SlotPathResolver
{
    private static HashSet<string> s_NodesWithStaticSlots;
    private static Dictionary<string, int> s_SlotPathToId;
    private static Dictionary<string, SubGraphPortDescriptor> s_SubGraphSlotPathToDescriptor;
    public static bool HasStaticSlotMapping(string nodeTypeName)
    {
        if (s_NodesWithStaticSlots == null)
        {
            s_NodesWithStaticSlots = new HashSet<string>();
            foreach (var key in s_SlotPathToId.Keys)
            {
                string nodeName = key.Split('.')[0];
                s_NodesWithStaticSlots.Add(nodeName);
            }
        }
        return s_NodesWithStaticSlots.Contains(nodeTypeName);
    }
    public class SubGraphPortDescriptor
    {
        public string PortName;      // 端口名称（如 "_BaseColor"）
        public bool IsInput;         // true=输入，false=输出
        public Type DeclaringType;   // 声明该常量的静态类类型
    }

    static SlotPathResolver()
    {
        s_SlotPathToId = new Dictionary<string, int>();
        s_SubGraphSlotPathToDescriptor = new Dictionary<string, SubGraphPortDescriptor>();

        var assembly = typeof(SlotPathResolver).Assembly;
        var slotsNamespace = "龙哥的秘密花园.节点库";
        var nodeTypes = assembly.GetTypes()
            .Where(t => t.Namespace == slotsNamespace && t.IsClass && t.IsAbstract && t.IsSealed);

        foreach (var nodeType in nodeTypes)
        {
            // 检查是否标记了 SubGraphPortsAttribute
            var subGraphAttr = nodeType.GetCustomAttribute<SubGraphPortsAttribute>();
            if (subGraphAttr != null)
            {
                CollectSubGraphPorts(nodeType, subGraphAttr);
                continue;
            }

            // 原有内置节点处理
            var inputClass = nodeType.GetNestedType("Input");
            var outputClass = nodeType.GetNestedType("Output");
            if (inputClass != null)
                CollectConstants(nodeType.Name, "Input", inputClass);
            if (outputClass != null)
                CollectConstants(nodeType.Name, "Output", outputClass);
        }
    }

    private static void CollectSubGraphPorts(Type nodeType, SubGraphPortsAttribute attr)
    {
        string className = nodeType.Name;
        var inputClass = nodeType.GetNestedType("Input");
        var outputClass = nodeType.GetNestedType("Output");

        if (inputClass != null)
        {
            foreach (var field in inputClass.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(string) && field.IsLiteral)
                {
                    string portName = (string)field.GetRawConstantValue();
                    string path = $"{className}.Input.{field.Name}";
                    s_SubGraphSlotPathToDescriptor[path] = new SubGraphPortDescriptor
                    {
                        PortName = portName,
                        IsInput = true,
                        DeclaringType = nodeType
                    };
                }
            }
        }

        if (outputClass != null)
        {
            foreach (var field in outputClass.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(string) && field.IsLiteral)
                {
                    string portName = (string)field.GetRawConstantValue();
                    string path = $"{className}.Output.{field.Name}";
                    s_SubGraphSlotPathToDescriptor[path] = new SubGraphPortDescriptor
                    {
                        PortName = portName,
                        IsInput = false,
                        DeclaringType = nodeType
                    };
                }
            }
        }
    }

    private static void CollectConstants(string nodeName, string ioType, Type slotClass)
    {
        var fields = slotClass.GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var field in fields)
        {
            if (field.FieldType == typeof(int))
            {
                int slotId = (int)field.GetValue(null);
                string path = $"{nodeName}.{ioType}.{field.Name}";
                s_SlotPathToId[path] = slotId;
            }
        }
    }

    /// <summary>
    /// 获取内置节点的槽 ID
    /// </summary>
    public static int GetSlotId(string slotPath)
    {
        if (s_SlotPathToId.TryGetValue(slotPath, out int id))
            return id;
        throw new ArgumentException($"未知的内置节点槽路径: {slotPath}");
    }

    /// <summary>
    /// 尝试获取子图端口描述符
    /// </summary>
    public static bool TryGetSubGraphPortDescriptor(string slotPath, out SubGraphPortDescriptor descriptor)
    {
        return s_SubGraphSlotPathToDescriptor.TryGetValue(slotPath, out descriptor);
    }

    /// <summary>
    /// 判断路径是否为子图端口路径
    /// </summary>
    public static bool IsSubGraphPortPath(string slotPath)
    {
        return s_SubGraphSlotPathToDescriptor.ContainsKey(slotPath);
    }
}
}