using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.ShaderGraph.Internal;

namespace 龙哥的秘密花园.节点库
{
    /// <summary>
    /// ShaderGraph 操作上下文：封装 GraphData 的加载、缓存与高级操作。
    /// </summary>
    public class GraphDataContext
    {
        private static readonly Dictionary<string, GraphDataContext> Cache = new Dictionary<string, GraphDataContext>();
        private URPShaderGraphHelper m_URPHelper;
        public URPShaderGraphHelper URP => m_URPHelper ??= new URPShaderGraphHelper(this);
        private BuiltInShaderGraphHelper m_BuiltInHelper;
        public BuiltInShaderGraphHelper BuiltIn => m_BuiltInHelper ??= new BuiltInShaderGraphHelper(this);
        public string AssetPath { get; }
        public object GraphData { get; private set; }

        private GraphDataContext(string assetPath)
        {
            AssetPath = assetPath;
            Load();
        }

        public static GraphDataContext GetOrCreate(string assetPath)
        {
            if (Cache.TryGetValue(assetPath, out var ctx))
                return ctx;

            ctx = new GraphDataContext(assetPath);
            Cache[assetPath] = ctx;
            return ctx;
        }

        public static void ClearCache() => Cache.Clear();

        private void Load()
        {
            GraphData = ShaderGraphReflectionHelper.LoadGraphData(AssetPath, out string error);
            if (GraphData == null)
                throw new Exception($"加载 GraphData 失败: {error}");
        }

        public void Save()
        {
            if (!ShaderGraphReflectionHelper.SaveGraphData(GraphData, AssetPath, out string error))
                throw new Exception($"保存失败: {error}");

            // 获取 GraphData.owner 并标记为脏
            var ownerProp = GraphData.GetType().GetProperty("owner", BindingFlags.Public | BindingFlags.Instance);
            var owner = ownerProp?.GetValue(GraphData) as UnityEngine.Object;
            if (owner != null)
            {
                EditorUtility.SetDirty(owner);
            }

            // 重新导入资产，强制 Shader Graph 窗口重新加载
            AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);

            // 重新加载 GraphData 以保持缓存同步
            Load();
            m_URPHelper?.Refresh();
        }
        /// <summary>
        /// 根据节点 ID 从当前 GraphData 中查找节点实例。
        /// </summary>
        public object GetNodeById(string nodeId)
        {
            var graphDataType = GraphData.GetType();
            var nodesField = graphDataType.GetField("m_Nodes", BindingFlags.NonPublic | BindingFlags.Instance);
            var nodesList = nodesField?.GetValue(GraphData) as System.Collections.IList;
            if (nodesList == null) return null;

            foreach (var item in nodesList)
            {
                var node = ShaderGraphReflectionHelper.UnwrapJsonData(item);
                var id = ShaderGraphReflectionHelper.GetNodeId(node);
                if (id == nodeId)
                    return node;
            }
            return null;
        }
        public void SetNodePosition(object node, Rect position, bool saveImmediately = true)
        {
            // 尝试获取节点 ID
            var actualNode = ShaderGraphReflectionHelper.UnwrapJsonData(node);
            var nodeId = ShaderGraphReflectionHelper.GetNodeId(actualNode);
    
            // 如果节点 ID 有效，且节点可能已失效（例如 owner 为 null 或 owner 不是当前 GraphData），则重新获取
            if (!string.IsNullOrEmpty(nodeId))
            {
                // 检查节点是否仍然属于当前 GraphData
                bool isValid = false;
                var ownerProp = actualNode.GetType().GetProperty("owner", BindingFlags.Public | BindingFlags.Instance);
                var owner = ownerProp?.GetValue(actualNode);
                if (owner == GraphData)
                    isValid = true;
        
                if (!isValid)
                {
                    var freshNode = GetNodeById(nodeId);
                    if (freshNode != null)
                        actualNode = freshNode;
                }
            }
    
            ShaderGraphReflectionHelper.SetNodePosition(actualNode, position);
            if (saveImmediately)
                Save();
        }
        public object AddNodeWithId(ShaderGraphNodeType nodeType, string nodeId, object parameters = null, Rect? position = null)
        {
            if (!NodeTypeNameMap.TryGetValue(nodeType, out string typeName))
                throw new ArgumentException($"未注册的节点类型: {nodeType}");

            var properties = NodeParameterConverter.ConvertToProperties(nodeType, parameters);
            // 添加临时属性传递节点 ID
            if (properties == null)
                properties = new Dictionary<string, object>();
            properties["_Temp_NodeId"] = nodeId;
            return AddNodeInternal(typeName, properties, position);
        }
        /// <summary>
        /// 使用 NodeLayoutHelper 自动计算并设置所有节点的位置。
        /// </summary>
        /// <param name="nodesInfo">节点信息列表，每个元素应包含 Id 和 Type 属性（字符串）</param>
        /// <param name="connectionsInfo">连接信息列表，每个元素应包含 From 和 To 属性（字符串）</param>
        public void ApplyAutoLayout(IEnumerable<dynamic> nodesInfo, IEnumerable<dynamic> connectionsInfo, Dictionary<string, object> nodeMap)
        {
            var coords = NodeLayoutHelper.CalculateLayout(nodesInfo, connectionsInfo);
            foreach (var kv in coords)
            {
                string jsonId = kv.Key;
                Vector2 pos = kv.Value;
                if (nodeMap.TryGetValue(jsonId, out object node))
                {
                    Rect newRect = new Rect(pos.x, pos.y, 150, 80);
                    SetNodePosition(node, newRect);
                }
            }
            Save();
            Debug.Log($"自动布局完成，共设置 {coords.Count} 个节点的位置");
        }
        // ---------- 添加节点 ----------
        public object AddNode<TParams>(ShaderGraphNodeType nodeType, TParams parameters, Rect? position = null)
            where TParams : class
        {
            if (!NodeTypeNameMap.TryGetValue(nodeType, out string typeName))
                throw new ArgumentException($"未注册的节点类型: {nodeType}");

            var properties = NodeParameterConverter.ConvertToProperties(nodeType, parameters);
            return AddNodeInternal(typeName, properties, position);
        }

        public object AddNode(ShaderGraphNodeType nodeType, Rect? position = null)
        {
            return AddNode<object>(nodeType, null, position);
        }
        public static bool TryGetNodeTypeName(ShaderGraphNodeType nodeType, out string typeName)
        {
            return NodeTypeNameMap.TryGetValue(nodeType, out typeName);
        }
       // ========== 以下是各个具体插槽类型的创建方法 ==========

#region Vector / Boolean

object CreateVector1Slot(int slotId, string displayName, string shaderOutputName, object slotType, float defaultValue, object stageCapability)
{
    var type = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Vector1MaterialSlot");
    if (type == null) throw new Exception("找不到 Vector1MaterialSlot 类型");
    // 构造函数: (int slotId, string displayName, string shaderOutputName, SlotType slotType, float value, ShaderStageCapability stageCapability, string label1 = null, bool hidden = false)
    var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), typeof(float), stageCapability.GetType(), typeof(string), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, defaultValue, stageCapability, null, false });
    // 降级：无 label 参数
    ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), typeof(float), stageCapability.GetType(), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, defaultValue, stageCapability, false });
    throw new Exception("未找到 Vector1MaterialSlot 的合适构造函数");
}

object CreateVector2Slot(int slotId, string displayName, string shaderOutputName, object slotType, Vector2 defaultValue, object stageCapability)
{
    var type = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Vector2MaterialSlot");
    if (type == null) throw new Exception("找不到 Vector2MaterialSlot 类型");
    // 构造函数: (int slotId, string displayName, string shaderOutputName, SlotType slotType, Vector2 value, ShaderStageCapability stageCapability, string label1 = null, string label2 = null, bool hidden = false)
    var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), typeof(Vector2), stageCapability.GetType(), typeof(string), typeof(string), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, defaultValue, stageCapability, null, null, false });
    ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), typeof(Vector2), stageCapability.GetType(), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, defaultValue, stageCapability, false });
    throw new Exception("未找到 Vector2MaterialSlot 的合适构造函数");
}

object CreateVector3Slot(int slotId, string displayName, string shaderOutputName, object slotType, Vector3 defaultValue, object stageCapability)
{
    var type = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Vector3MaterialSlot");
    if (type == null) throw new Exception("找不到 Vector3MaterialSlot 类型");
    // 构造函数: (int slotId, string displayName, string shaderOutputName, SlotType slotType, Vector3 value, ShaderStageCapability stageCapability, string label1 = null, string label2 = null, string label3 = null, bool hidden = false)
    var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), typeof(Vector3), stageCapability.GetType(), typeof(string), typeof(string), typeof(string), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, defaultValue, stageCapability, null, null, null, false });
    ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), typeof(Vector3), stageCapability.GetType(), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, defaultValue, stageCapability, false });
    throw new Exception("未找到 Vector3MaterialSlot 的合适构造函数");
}

object CreateVector4Slot(int slotId, string displayName, string shaderOutputName, object slotType, Vector4 defaultValue, object stageCapability)
{
    var type = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Vector4MaterialSlot");
    if (type == null) throw new Exception("找不到 Vector4MaterialSlot 类型");
    // 构造函数: (int slotId, string displayName, string shaderOutputName, SlotType slotType, Vector4 value, ShaderStageCapability stageCapability, string label1 = null, ... bool hidden = false)
    var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), typeof(Vector4), stageCapability.GetType(), typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, defaultValue, stageCapability, null, null, null, null, false });
    ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), typeof(Vector4), stageCapability.GetType(), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, defaultValue, stageCapability, false });
    throw new Exception("未找到 Vector4MaterialSlot 的合适构造函数");
}

object CreateBooleanSlot(int slotId, string displayName, string shaderOutputName, object slotType, bool defaultValue, object stageCapability)
{
    var type = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.BooleanMaterialSlot");
    if (type == null) throw new Exception("找不到 BooleanMaterialSlot 类型");
    // 构造函数: (int slotId, string displayName, string shaderOutputName, SlotType slotType, bool value, ShaderStageCapability stageCapability, bool hidden = false)
    var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), typeof(bool), stageCapability.GetType(), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, defaultValue, stageCapability, false });
    throw new Exception("未找到 BooleanMaterialSlot 的合适构造函数");
}

#endregion

#region Texture / SamplerState / VirtualTexture

object CreateTexture2DSlot(int slotId, string displayName, string shaderOutputName, bool isInput, object slotType, object stageCapability)
{
    string typeName = isInput ? "UnityEditor.ShaderGraph.Texture2DInputMaterialSlot" : "UnityEditor.ShaderGraph.Texture2DMaterialSlot";
    var type = ShaderGraphReflectionHelper.FindType(typeName);
    if (type == null) throw new Exception($"找不到 {typeName} 类型");
    if (isInput)
    {
        // Texture2DInputMaterialSlot: (int slotId, string displayName, string shaderOutputName, ShaderStageCapability stageCapability, bool hidden)
        var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), stageCapability.GetType(), typeof(bool) });
        if (ctor != null)
            return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, stageCapability, false });
    }
    else
    {
        // Texture2DMaterialSlot: (int slotId, string displayName, string shaderOutputName, SlotType slotType, ShaderStageCapability stageCapability, bool hidden)
        var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), stageCapability.GetType(), typeof(bool) });
        if (ctor != null)
            return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, stageCapability, false });
    }
    throw new Exception($"未找到 {typeName} 的合适构造函数");
}

object CreateTexture2DArraySlot(int slotId, string displayName, string shaderOutputName, bool isInput, object slotType, object stageCapability)
{
    string typeName = isInput ? "UnityEditor.ShaderGraph.Texture2DArrayInputMaterialSlot" : "UnityEditor.ShaderGraph.Texture2DArrayMaterialSlot";
    var type = ShaderGraphReflectionHelper.FindType(typeName);
    if (type == null) throw new Exception($"找不到 {typeName} 类型");
    if (isInput)
    {
        var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), stageCapability.GetType(), typeof(bool) });
        if (ctor != null)
            return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, stageCapability, false });
    }
    else
    {
        var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), stageCapability.GetType(), typeof(bool) });
        if (ctor != null)
            return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, stageCapability, false });
    }
    throw new Exception($"未找到 {typeName} 的合适构造函数");
}

object CreateTexture3DSlot(int slotId, string displayName, string shaderOutputName, bool isInput, object slotType, object stageCapability)
{
    string typeName = isInput ? "UnityEditor.ShaderGraph.Texture3DInputMaterialSlot" : "UnityEditor.ShaderGraph.Texture3DMaterialSlot";
    var type = ShaderGraphReflectionHelper.FindType(typeName);
    if (type == null) throw new Exception($"找不到 {typeName} 类型");
    if (isInput)
    {
        var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), stageCapability.GetType(), typeof(bool) });
        if (ctor != null)
            return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, stageCapability, false });
    }
    else
    {
        var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), stageCapability.GetType(), typeof(bool) });
        if (ctor != null)
            return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, stageCapability, false });
    }
    throw new Exception($"未找到 {typeName} 的合适构造函数");
}

object CreateCubemapSlot(int slotId, string displayName, string shaderOutputName, bool isInput, object slotType, object stageCapability)
{
    string typeName = isInput ? "UnityEditor.ShaderGraph.CubemapInputMaterialSlot" : "UnityEditor.ShaderGraph.CubemapMaterialSlot";
    var type = ShaderGraphReflectionHelper.FindType(typeName);
    if (type == null) throw new Exception($"找不到 {typeName} 类型");
    if (isInput)
    {
        var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), stageCapability.GetType(), typeof(bool) });
        if (ctor != null)
            return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, stageCapability, false });
    }
    else
    {
        var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), stageCapability.GetType(), typeof(bool) });
        if (ctor != null)
            return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, stageCapability, false });
    }
    throw new Exception($"未找到 {typeName} 的合适构造函数");
}

object CreateSamplerStateSlot(int slotId, string displayName, string shaderOutputName, bool isInput, object slotType, object stageCapability)
{
    // SamplerStateMaterialSlot 没有单独的 Input/Output 类，统一使用 SamplerStateMaterialSlot
    var type = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.SamplerStateMaterialSlot");
    if (type == null) throw new Exception("找不到 SamplerStateMaterialSlot 类型");
    // 构造函数: (int slotId, string displayName, string shaderOutputName, SlotType slotType, ShaderStageCapability stageCapability, bool hidden)
    var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), stageCapability.GetType(), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, stageCapability, false });
    throw new Exception("未找到 SamplerStateMaterialSlot 的合适构造函数");
}

object CreateVirtualTextureSlot(int slotId, string displayName, string shaderOutputName, bool isInput, object slotType, object stageCapability)
{
    string typeName = isInput ? "UnityEditor.ShaderGraph.VirtualTextureInputMaterialSlot" : "UnityEditor.ShaderGraph.VirtualTextureMaterialSlot";
    var type = ShaderGraphReflectionHelper.FindType(typeName);
    if (type == null) throw new Exception($"找不到 {typeName} 类型");
    if (isInput)
    {
        var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), stageCapability.GetType(), typeof(bool) });
        if (ctor != null)
            return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, stageCapability, false });
    }
    else
    {
        var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), stageCapability.GetType(), typeof(bool) });
        if (ctor != null)
            return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, stageCapability, false });
    }
    throw new Exception($"未找到 {typeName} 的合适构造函数");
}

object CreateGradientSlot(int slotId, string displayName, string shaderOutputName, bool isInput, object slotType, object stageCapability)
{
    string typeName = isInput ? "UnityEditor.ShaderGraph.GradientInputMaterialSlot" : "UnityEditor.ShaderGraph.GradientMaterialSlot";
    var type = ShaderGraphReflectionHelper.FindType(typeName);
    if (type == null) throw new Exception($"找不到 {typeName} 类型");
    if (isInput)
    {
        // GradientInputMaterialSlot: (int slotId, string displayName, string shaderOutputName, ShaderStageCapability stageCapability, bool hidden)
        var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), stageCapability.GetType(), typeof(bool) });
        if (ctor != null)
            return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, stageCapability, false });
    }
    else
    {
        // GradientMaterialSlot: (int slotId, string displayName, string shaderOutputName, SlotType slotType, ShaderStageCapability stageCapability, bool hidden)
        var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), stageCapability.GetType(), typeof(bool) });
        if (ctor != null)
            return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, stageCapability, false });
    }
    throw new Exception($"未找到 {typeName} 的合适构造函数");
}

#endregion

#region Matrix

object CreateMatrix2Slot(int slotId, string displayName, string shaderOutputName, object slotType, object stageCapability)
{
    var type = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Matrix2MaterialSlot");
    if (type == null) throw new Exception("找不到 Matrix2MaterialSlot 类型");
    // 构造函数: (int slotId, string displayName, string shaderOutputName, SlotType slotType, ShaderStageCapability stageCapability, bool hidden)
    var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), stageCapability.GetType(), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, stageCapability, false });
    throw new Exception("未找到 Matrix2MaterialSlot 的合适构造函数");
}

object CreateMatrix3Slot(int slotId, string displayName, string shaderOutputName, object slotType, object stageCapability)
{
    var type = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Matrix3MaterialSlot");
    if (type == null) throw new Exception("找不到 Matrix3MaterialSlot 类型");
    var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), stageCapability.GetType(), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, stageCapability, false });
    throw new Exception("未找到 Matrix3MaterialSlot 的合适构造函数");
}

object CreateMatrix4Slot(int slotId, string displayName, string shaderOutputName, object slotType, object stageCapability)
{
    var type = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Matrix4MaterialSlot");
    if (type == null) throw new Exception("找不到 Matrix4MaterialSlot 类型");
    var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), stageCapability.GetType(), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, stageCapability, false });
    throw new Exception("未找到 Matrix4MaterialSlot 的合适构造函数");
}

object CreateDynamicMatrixSlot(int slotId, string displayName, string shaderOutputName, object slotType, object stageCapability)
{
    var type = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.DynamicMatrixMaterialSlot");
    if (type == null) throw new Exception("找不到 DynamicMatrixMaterialSlot 类型");
    var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), stageCapability.GetType(), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, stageCapability, false });
    throw new Exception("未找到 DynamicMatrixMaterialSlot 的合适构造函数");
}

object CreateDynamicVectorSlot(int slotId, string displayName, string shaderOutputName, object slotType, Vector4 defaultValue, object stageCapability)
{
    var type = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.DynamicVectorMaterialSlot");
    if (type == null) throw new Exception("找不到 DynamicVectorMaterialSlot 类型");
    // 构造函数: (int slotId, string displayName, string shaderOutputName, SlotType slotType, Vector4 value, ShaderStageCapability stageCapability, bool hidden)
    var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), typeof(Vector4), stageCapability.GetType(), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, defaultValue, stageCapability, false });
    throw new Exception("未找到 DynamicVectorMaterialSlot 的合适构造函数");
}

object CreateDynamicValueSlot(int slotId, string displayName, string shaderOutputName, object slotType, Matrix4x4 defaultValue, object stageCapability)
{
    var type = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.DynamicValueMaterialSlot");
    if (type == null) throw new Exception("找不到 DynamicValueMaterialSlot 类型");
    var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), typeof(Matrix4x4), stageCapability.GetType(), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, defaultValue, stageCapability, false });
    throw new Exception("未找到 DynamicValueMaterialSlot 的合适构造函数");
}

#endregion

#region Special

object CreatePropertyConnectionStateSlot(int slotId, string displayName, string shaderOutputName, object slotType, object stageCapability)
{
    var type = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.PropertyConnectionStateMaterialSlot");
    if (type == null) throw new Exception("找不到 PropertyConnectionStateMaterialSlot 类型");
    var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), slotType.GetType(), stageCapability.GetType(), typeof(bool) });
    if (ctor != null)
        return ctor.Invoke(new object[] { slotId, displayName, shaderOutputName, slotType, stageCapability, false });
    throw new Exception("未找到 PropertyConnectionStateMaterialSlot 的合适构造函数");
}

#endregion

private object AddNodeInternal(string nodeTypeName, Dictionary<string, object> properties, Rect? position)
{
    // 1. 创建节点实例
    var node = ShaderGraphReflectionHelper.CreateNodeInstance(nodeTypeName);
    if (node == null)
    {
        Debug.LogError($"[AddNodeInternal] 无法创建节点实例: {nodeTypeName}");
        return null;
    }

    // 检查是否有自定义节点 ID
    string customNodeId = null;
    if (properties != null && properties.TryGetValue("_Temp_NodeId", out var idObj))
    {
        customNodeId = idObj as string;        properties.Remove("_Temp_NodeId");
    }

    // ---------- 子图节点特殊处理 ----------
    if (nodeTypeName == "UnityEditor.ShaderGraph.SubGraphNode")
    {
        string identifier = null;
        SubGraphIdentifierType idType = SubGraphIdentifierType.Auto;

        if (properties != null)
        {
            if (properties.TryGetValue("_Temp_SubGraphIdentifier", out var idObj2))
                identifier = idObj2 as string;
            if (properties.TryGetValue("_Temp_SubGraphIdentifierType", out var typeObj))
                idType = (SubGraphIdentifierType)typeObj;

            properties.Remove("_Temp_SubGraphIdentifier");
            properties.Remove("_Temp_SubGraphIdentifierType");
        }

        if (string.IsNullOrEmpty(identifier))
        {
            var subGraphAttr = node.GetType().GetCustomAttribute<SubGraphPortsAttribute>();
            identifier = subGraphAttr?.SubGraphIdentifier;
        }

        if (!string.IsNullOrEmpty(identifier))
        {
            var subGraphAsset = SubGraphResolver.Resolve(identifier, idType);
            if (subGraphAsset == null)
                throw new Exception($"[AddNodeInternal] 无法解析子图标识符: '{identifier}' (类型: {idType})");

            var assetField = node.GetType().GetField("m_Asset", BindingFlags.NonPublic | BindingFlags.Instance)
                          ?? node.GetType().GetField("asset", BindingFlags.Public | BindingFlags.Instance);

            if (assetField != null)
            {
                assetField.SetValue(node, subGraphAsset);
                Debug.Log($"[AddNodeInternal] 已通过字段设置子图资产: {assetField.Name}");
            }
            else
            {
                var assetProp = node.GetType().GetProperty("asset", BindingFlags.Public | BindingFlags.Instance);
                if (assetProp != null)
                {
                    assetProp.SetValue(node, subGraphAsset);
                    Debug.Log($"[AddNodeInternal] 已通过属性设置子图资产: {assetProp.Name}");
                }
                else
                {
                    Debug.LogError("[AddNodeInternal] SubGraphNode 中找不到 asset 字段或属性");
                }
            }

            var updateSlotsMethod = node.GetType().GetMethod("UpdateSlots", BindingFlags.Public | BindingFlags.Instance);
            if (updateSlotsMethod != null)
            {
                updateSlotsMethod.Invoke(node, null);
                Debug.Log("[AddNodeInternal] 已调用 UpdateSlots");
            }
            else
            {
                Debug.LogWarning("[AddNodeInternal] 未找到 UpdateSlots 方法");
            }
        }
        else
        {
            Debug.LogWarning("[AddNodeInternal] SubGraphNode 未提供子图标识符，且未通过 SubGraphPortsAttribute 指定。");
        }
    }

    // 2. 设置节点 ID
    if (!string.IsNullOrEmpty(customNodeId))
        ShaderGraphReflectionHelper.SetNodeId(node, customNodeId);
    else
        ShaderGraphReflectionHelper.SetNodeId(node);

    // 3. 设置位置
    if (position.HasValue)
    {
        ShaderGraphReflectionHelper.SetNodePosition(node, position.Value);
    }

    // 4. 添加到图
    ShaderGraphReflectionHelper.AddNodeToGraph(GraphData, node);

    // 5. 初始化节点（确保插槽存在）
    var nodeType = node.GetType();
    var updateMethod = nodeType.GetMethod("UpdateNodeAfterDeserialization", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    updateMethod?.Invoke(node, null);

    // 6. 处理 PropertyNode 特殊参数
    bool isPropertyNode = (nodeTypeName == "UnityEditor.ShaderGraph.PropertyNode");
    string propertyRefName = null;
    if (isPropertyNode && properties != null && properties.TryGetValue("_Temp_PropertyRefName", out var refNameObj))
    {
        propertyRefName = refNameObj as string;
        properties.Remove("_Temp_PropertyRefName");
    }

    // 7. 设置普通属性（如 m_Value）
    if (properties != null)
    {
        foreach (var kv in properties)
        {
            ShaderGraphReflectionHelper.SetNodeProperty(node, kv.Key, kv.Value);
        }
        
        
    }
     // ===== 新增：针对几何节点直接设置 m_Space 和 m_PositionSource 字段 =====
    var actualNodeForGeo = ShaderGraphReflectionHelper.UnwrapJsonData(node);
    if (actualNodeForGeo != null)
    {
        // 处理 m_Space 字段（适用于 PositionNode, NormalVectorNode, ViewDirectionNode, TangentVectorNode, BitangentVectorNode, ViewVectorNode）
        if (properties != null && properties.TryGetValue("m_Space", out var spaceVal))
        {
            var spaceField = actualNodeForGeo.GetType().GetField("m_Space", BindingFlags.NonPublic | BindingFlags.Instance);
            if (spaceField == null)
                spaceField = actualNodeForGeo.GetType().BaseType?.GetField("m_Space", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (spaceField != null)
            {
                object enumValue = spaceVal;
                if (spaceVal is string enumName)
                {
                    enumValue = Enum.Parse(spaceField.FieldType, enumName);
                }
                spaceField.SetValue(actualNodeForGeo, enumValue);
                Debug.Log($"[AddNodeInternal] 直接设置 {nodeTypeName}.m_Space = {enumValue} (类型: {enumValue.GetType()})");
            }
            else
            {
                Debug.LogWarning($"未找到 {nodeTypeName} 的 m_Space 字段");
            }
        }

        // 处理 m_PositionSource 字段（仅 PositionNode）
        if (properties != null && properties.TryGetValue("m_PositionSource", out var sourceVal))
        {
            var sourceField = actualNodeForGeo.GetType().GetField("m_PositionSource", BindingFlags.NonPublic | BindingFlags.Instance);
            if (sourceField != null)
            {
                object enumValue = sourceVal;
                if (sourceVal is string enumName)
                {
                    enumValue = Enum.Parse(sourceField.FieldType, enumName);
                }
                sourceField.SetValue(actualNodeForGeo, enumValue);
                Debug.Log($"[AddNodeInternal] 直接设置 {nodeTypeName}.m_PositionSource = {enumValue}");
            }
            else
            {
                Debug.LogWarning($"未找到 {nodeTypeName} 的 m_PositionSource 字段");
            }
        }
    }

    // 8. PropertyNode 关联属性
    if (isPropertyNode && !string.IsNullOrEmpty(propertyRefName))
    {
        var shaderProp = ShaderGraphReflectionHelper.FindShaderPropertyByName(GraphData, propertyRefName);
        if (shaderProp != null)
        {
            ShaderGraphReflectionHelper.SetNodeProperty(node, "m_Property", shaderProp);
        }
    }

    // 9. 对于 Vector 节点，通过反射调用 FindInputSlot<T> 设置插槽值
    if (properties != null && properties.TryGetValue("m_Value", out var valueObj))
    {
        int dimension = 0;
        int[] inputSlotIds = null;
        int outputSlotId = 0;
        if (nodeTypeName.Contains("Vector1Node")) { dimension = 1; inputSlotIds = new[] { 1 }; outputSlotId = 0; }
        else if (nodeTypeName.Contains("Vector2Node")) { dimension = 2; inputSlotIds = new[] { 1, 2 }; outputSlotId = 0; }
        else if (nodeTypeName.Contains("Vector3Node")) { dimension = 3; inputSlotIds = new[] { 1, 2, 3 }; outputSlotId = 0; }
        else if (nodeTypeName.Contains("Vector4Node")) { dimension = 4; inputSlotIds = new[] { 1, 2, 3, 4 }; outputSlotId = 0; }

        if (dimension > 0)
        {
            // 转换值为 Vector4
            Vector4 vec = Vector4.zero;
            if (valueObj is float f1) vec = new Vector4(f1, 0, 0, 0);
            else if (valueObj is Vector2 v2) vec = new Vector4(v2.x, v2.y, 0, 0);
            else if (valueObj is Vector3 v3) vec = new Vector4(v3.x, v3.y, v3.z, 0);
            else if (valueObj is Vector4 v4) vec = v4;
            else if (valueObj is Color c) vec = new Vector4(c.r, c.g, c.b, c.a);

            // 获取 MaterialSlot 类型（internal 但可通过反射获取）
            var materialSlotType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.MaterialSlot");
            if (materialSlotType != null)
            {
                // 获取泛型方法 FindInputSlot<T> 和 FindOutputSlot<T>
                var findInputSlotMethod = nodeType.GetMethod("FindInputSlot", BindingFlags.Public | BindingFlags.Instance);
                var findOutputSlotMethod = nodeType.GetMethod("FindOutputSlot", BindingFlags.Public | BindingFlags.Instance);
                if (findInputSlotMethod != null && findInputSlotMethod.IsGenericMethodDefinition)
                {
                    var genericInputMethod = findInputSlotMethod.MakeGenericMethod(materialSlotType);
                    var genericOutputMethod = findOutputSlotMethod?.MakeGenericMethod(materialSlotType);

                    // 设置输入插槽
                    for (int i = 0; i < inputSlotIds.Length; i++)
                    {
                        int slotId = inputSlotIds[i];
                        var slot = genericInputMethod.Invoke(node, new object[] { slotId });
                        if (slot != null)
                        {
                            var valueProp = slot.GetType().GetProperty("value");
                            if (valueProp != null && valueProp.CanWrite)
                            {
                                float component = i == 0 ? vec.x : (i == 1 ? vec.y : (i == 2 ? vec.z : vec.w));
                                valueProp.SetValue(slot, component);
                            }
                        }
                    }

                    // 设置输出插槽
                    if (genericOutputMethod != null)
                    {
                        var outputSlot = genericOutputMethod.Invoke(node, new object[] { outputSlotId });
                        if (outputSlot != null)
                        {
                            var valueProp = outputSlot.GetType().GetProperty("value");
                            if (valueProp != null && valueProp.CanWrite)
                            {
                                if (dimension == 1) valueProp.SetValue(outputSlot, vec.x);
                                else if (dimension == 2) valueProp.SetValue(outputSlot, (Vector2)vec);
                                else if (dimension == 3) valueProp.SetValue(outputSlot, (Vector3)vec);
                                else if (dimension == 4) valueProp.SetValue(outputSlot, vec);
                            }
                        }
                    }
                }
            }
        }
    }

    // ===== 重点修改：CustomFunctionNode 处理（完整版，支持所有插槽类型）=====
    if (nodeTypeName == "UnityEditor.ShaderGraph.CustomFunctionNode")
    {
        // 确保获取真实节点实例（去除 JsonData 包装）
        var actualNode = ShaderGraphReflectionHelper.UnwrapJsonData(node);
        if (actualNode == null)
        {
            Debug.LogError("[AddNodeInternal] 无法获取 CustomFunctionNode 的真实实例");
            return node;
        }

        // 1. 输出函数体（调试）
        var bodyField = actualNode.GetType().GetField("m_FunctionBody", BindingFlags.NonPublic | BindingFlags.Instance);
        var currentBody = bodyField?.GetValue(actualNode) as string;
        Debug.Log($"[AddNodeInternal] 当前 functionBody: '{currentBody}'");

        // 2. 手动创建插槽
        if (properties != null && properties.TryGetValue("_SlotsData", out var slotsDataObj))
        {
            var slotDefs = slotsDataObj as IEnumerable<SlotDefinition>;
            if (slotDefs != null && slotDefs.Any())
            {
                Debug.Log($"[AddNodeInternal] _SlotsData 包含 {slotDefs.Count()} 个定义");

                // 获取必要的内部类型
                var slotTypeType = ShaderGraphReflectionHelper.FindType("UnityEditor.Graphing.SlotType");
                var shaderStageCapabilityType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.ShaderStageCapability");

                // 获取 AddSlot 方法
                var materialSlotBaseType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.MaterialSlot");
                MethodInfo addSlotMethod = actualNode.GetType().GetMethod("AddSlot",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy,
                    null, new Type[] { materialSlotBaseType, typeof(bool) }, null);
                if (addSlotMethod == null)
                {
                    addSlotMethod = actualNode.GetType().GetMethod("AddSlot",
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy,
                        null, new Type[] { materialSlotBaseType }, null);
                }
                if (addSlotMethod == null)
                {
                    var abstractMaterialNodeType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.AbstractMaterialNode");
                    addSlotMethod = abstractMaterialNodeType?.GetMethod("AddSlot", new Type[] { materialSlotBaseType, typeof(bool) });
                    if (addSlotMethod == null)
                        addSlotMethod = abstractMaterialNodeType?.GetMethod("AddSlot", new Type[] { materialSlotBaseType });
                }
                if (addSlotMethod == null)
                {
                    Debug.LogError("[AddNodeInternal] 找不到任何 AddSlot 方法重载，插槽创建失败");
                    properties.Remove("_SlotsData");
                    return node;
                }

                int nextId = 0;
                foreach (var def in slotDefs)
                {
                    try
                    {
                        object slot = CreateMaterialSlotByDefinition(def, nextId++, slotTypeType, shaderStageCapabilityType);
                        if (slot != null)
                        {
                            if (addSlotMethod.GetParameters().Length == 2)
                                addSlotMethod.Invoke(actualNode, new[] { slot, true });
                            else
                                addSlotMethod.Invoke(actualNode, new[] { slot });
                            Debug.Log($"[AddNodeInternal] 成功添加插槽: {def.DisplayName} (Type: {def.ValueType}, IsInput: {def.IsInput})");
                        }
                        else
                        {
                            Debug.LogWarning($"[AddNodeInternal] 未能创建插槽: {def.DisplayName} (Type: {def.ValueType})");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[AddNodeInternal] 创建插槽 {def.DisplayName} 时出错: {ex.Message}\n{ex.StackTrace}");
                    }
                }

                properties.Remove("_SlotsData");
            }
            else
            {
                Debug.LogWarning("[AddNodeInternal] _SlotsData 为空或无有效定义");
            }
        }
        else
        {
            Debug.LogWarning("[AddNodeInternal] CustomFunctionNode 未提供 _SlotsData");
        }

        // 3. 最终验证插槽数量
        var slotsField = actualNode.GetType().GetField("m_Slots", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        var slotsList = slotsField?.GetValue(actualNode) as System.Collections.IList;
        Debug.Log($"[AddNodeInternal] 最终插槽数量: {slotsList?.Count ?? -1}");
    }

    // 10. 再次调用 UpdateNodeAfterDeserialization 确保状态一致
    updateMethod?.Invoke(node, null);

    // 11. 调用 SetupSlots (某些节点需要)
    var setupSlots = nodeType.GetMethod("SetupSlots", BindingFlags.Public | BindingFlags.Instance);
    setupSlots?.Invoke(node, null);

    // 12. 验证图并保存
    ShaderGraphReflectionHelper.ValidateGraph(GraphData);
    Save();

    return node;
}
 
 private object CreateMaterialSlotByDefinition(SlotDefinition def, int slotId, Type slotTypeType, Type stageCapabilityType)
{
    // 将公开枚举转换为内部枚举
    object internalSlotType = def.IsInput ? Enum.Parse(slotTypeType, "Input") : Enum.Parse(slotTypeType, "Output");
    object internalStageCapability = Enum.Parse(stageCapabilityType, "All"); // 可根据 def.StageCapability 扩展

    // 根据 ValueType 分支处理
    switch (def.ValueType)
    {
        case SlotValueType.Vector1:
            return CreateVector1Slot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, internalSlotType, def.DefaultValue.x, internalStageCapability);
        case SlotValueType.Vector2:
            return CreateVector2Slot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, internalSlotType, new Vector2(def.DefaultValue.x, def.DefaultValue.y), internalStageCapability);
        case SlotValueType.Vector3:
            return CreateVector3Slot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, internalSlotType, new Vector3(def.DefaultValue.x, def.DefaultValue.y, def.DefaultValue.z), internalStageCapability);
        case SlotValueType.Vector4:
            return CreateVector4Slot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, internalSlotType, def.DefaultValue, internalStageCapability);
        case SlotValueType.Boolean:
            return CreateBooleanSlot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, internalSlotType, def.DefaultValue.x != 0f, internalStageCapability);
        case SlotValueType.Texture2D:
            return CreateTexture2DSlot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, def.IsInput, internalSlotType, internalStageCapability);
        case SlotValueType.Texture2DArray:
            return CreateTexture2DArraySlot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, def.IsInput, internalSlotType, internalStageCapability);
        case SlotValueType.Texture3D:
            return CreateTexture3DSlot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, def.IsInput, internalSlotType, internalStageCapability);
        case SlotValueType.Cubemap:
            return CreateCubemapSlot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, def.IsInput, internalSlotType, internalStageCapability);
        case SlotValueType.SamplerState:
            return CreateSamplerStateSlot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, def.IsInput, internalSlotType, internalStageCapability);
        case SlotValueType.VirtualTexture:
            return CreateVirtualTextureSlot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, def.IsInput, internalSlotType, internalStageCapability);
        case SlotValueType.Gradient:
            return CreateGradientSlot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, def.IsInput, internalSlotType, internalStageCapability);
        case SlotValueType.Matrix2:
            return CreateMatrix2Slot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, internalSlotType, internalStageCapability);
        case SlotValueType.Matrix3:
            return CreateMatrix3Slot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, internalSlotType, internalStageCapability);
        case SlotValueType.Matrix4:
            return CreateMatrix4Slot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, internalSlotType, internalStageCapability);
        case SlotValueType.DynamicMatrix:
            return CreateDynamicMatrixSlot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, internalSlotType, internalStageCapability);
        case SlotValueType.DynamicVector:
            return CreateDynamicVectorSlot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, internalSlotType, def.DefaultValue, internalStageCapability);
        case SlotValueType.Dynamic:
            return CreateDynamicValueSlot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, internalSlotType, new Matrix4x4(def.DefaultValue, Vector4.zero, Vector4.zero, Vector4.zero), internalStageCapability);
        case SlotValueType.PropertyConnectionState:
            return CreatePropertyConnectionStateSlot(slotId, def.DisplayName, def.ShaderOutputName ?? def.DisplayName, internalSlotType, internalStageCapability);
        default:
            Debug.LogError($"[CreateMaterialSlot] 不支持的插槽类型: {def.ValueType}");
            return null;
    }
}

 
 
private int GetSlotCount(object node)
{
    var slotsField = node.GetType().GetField("m_Slots", BindingFlags.NonPublic | BindingFlags.Instance);
    if (slotsField == null) return -1;
    var slotsList = slotsField.GetValue(node) as System.Collections.IList;
    return slotsList?.Count ?? 0;
}
  public void ClearAllShaderProperties()
{
    var graphDataType = GraphData.GetType();

    // 清空属性列表 (m_Properties)
    var propsField = graphDataType.GetField("m_Properties", BindingFlags.NonPublic | BindingFlags.Instance);
    var propsList = propsField?.GetValue(GraphData) as System.Collections.IList;
    propsList?.Clear();

    // 清空关键字列表 (m_Keywords)
    var keywordsField = graphDataType.GetField("m_Keywords", BindingFlags.NonPublic | BindingFlags.Instance);
    var keywordsList = keywordsField?.GetValue(GraphData) as System.Collections.IList;
    keywordsList?.Clear();

    // 清空下拉列表 (m_Dropdowns)
    var dropdownsField = graphDataType.GetField("m_Dropdowns", BindingFlags.NonPublic | BindingFlags.Instance);
    var dropdownsList = dropdownsField?.GetValue(GraphData) as System.Collections.IList;
    dropdownsList?.Clear();

    // 清空分类数据 (m_CategoryData)
    var categoriesField = graphDataType.GetField("m_CategoryData", BindingFlags.NonPublic | BindingFlags.Instance);
    var categoriesList = categoriesField?.GetValue(GraphData) as System.Collections.IList;
    categoriesList?.Clear();

  
}
  // ---------- 连接节点（仅基于插槽 ID）----------
        /// <summary>
        /// 通过插槽 ID 连接两个节点。这是推荐的可靠连接方式。
        /// </summary>
        /// <param name="fromNode">源节点实例</param>
        /// <param name="outputSlotId">源节点输出插槽 ID</param>
        /// <param name="toNode">目标节点实例</param>
        /// <param name="inputSlotId">目标节点输入插槽 ID</param>
        /// <returns>是否连接成功</returns>
        public bool ConnectSlots(object fromNode, int outputSlotId, object toNode, int inputSlotId)
        {
            try
            {
                object actualFrom = ShaderGraphReflectionHelper.UnwrapJsonData(fromNode);
                object actualTo = ShaderGraphReflectionHelper.UnwrapJsonData(toNode);

                var graphDataType = GraphData.GetType();
                var slotRefType = ShaderGraphReflectionHelper.FindType("UnityEditor.Graphing.SlotReference");
                var edgeType = ShaderGraphReflectionHelper.FindType("UnityEditor.Graphing.Edge");

                if (slotRefType == null || edgeType == null)
                {
                    Debug.LogError("找不到 SlotReference 或 Edge 类型");
                    return false;
                }

                object outputSlotRef = Activator.CreateInstance(slotRefType, new object[] { actualFrom, outputSlotId });
                object inputSlotRef = Activator.CreateInstance(slotRefType, new object[] { actualTo, inputSlotId });
                object edge = Activator.CreateInstance(edgeType, new object[] { outputSlotRef, inputSlotRef });

                var edgesField = graphDataType.GetField("m_Edges", BindingFlags.NonPublic | BindingFlags.Instance);
                var edgesList = edgesField?.GetValue(GraphData) as System.Collections.IList;
                edgesList?.Add(edge);

                UpdateNodeEdges(actualFrom, actualTo, edge);
                Save();

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"连接失败: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }
        /// <summary>
        /// 断开两个插槽之间的连线
        /// </summary>
        public bool DisconnectSlots(object fromNode, int outputSlotId, object toNode, int inputSlotId)
        {
            object actualFrom = ShaderGraphReflectionHelper.UnwrapJsonData(fromNode);
            object actualTo = ShaderGraphReflectionHelper.UnwrapJsonData(toNode);

            // 获取当前所有的边
            var edgesField = GraphData.GetType().GetField("m_Edges", BindingFlags.NonPublic | BindingFlags.Instance);
            var edgesList = edgesField?.GetValue(GraphData) as System.Collections.IList;
            if (edgesList == null) return false;

            // 查找匹配的边
            object targetEdge = null;
            foreach (var edge in edgesList)
            {
                var outputSlotProp = edge.GetType().GetField("m_OutputSlot", BindingFlags.NonPublic | BindingFlags.Instance);
                var inputSlotProp = edge.GetType().GetField("m_InputSlot", BindingFlags.NonPublic | BindingFlags.Instance);
                if (outputSlotProp == null || inputSlotProp == null) continue;

                var outputSlotRef = outputSlotProp.GetValue(edge);
                var inputSlotRef = inputSlotProp.GetValue(edge);

                // 获取 SlotReference 的字段
                var nodeField = outputSlotRef.GetType().GetField("m_Node", BindingFlags.NonPublic | BindingFlags.Instance);
                var slotIdField = outputSlotRef.GetType().GetField("m_SlotId", BindingFlags.NonPublic | BindingFlags.Instance);

                var outNode = (nodeField?.GetValue(outputSlotRef) as dynamic)?.value;
                var outSlotId = (int)slotIdField?.GetValue(outputSlotRef);

                var inNode = (nodeField?.GetValue(inputSlotRef) as dynamic)?.value;
                var inSlotId = (int)slotIdField?.GetValue(inputSlotRef);

                if (outNode == actualFrom && outSlotId == outputSlotId && inNode == actualTo && inSlotId == inputSlotId)
                {
                    targetEdge = edge;
                    break;
                }
            }

            if (targetEdge == null)
            {
                Debug.LogWarning("未找到指定的连线");
                return false;
            }

            // 调用 RemoveEdge
            var removeMethod = GraphData.GetType().GetMethod("RemoveEdge", new Type[] { targetEdge.GetType().GetInterfaces().FirstOrDefault(i => i.Name == "IEdge") });
            if (removeMethod == null)
            {
                Debug.LogError("找不到 GraphData.RemoveEdge 方法");
                return false;
            }

            try
            {
                removeMethod.Invoke(GraphData, new[] { targetEdge });
                Save();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"断开连线失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 断开节点上所有连线（输入和输出）
        /// </summary>
        public void DisconnectAllSlots(object node)
        {
            object actualNode = ShaderGraphReflectionHelper.UnwrapJsonData(node);
            if (actualNode == null) return;

            var edgesField = GraphData.GetType().GetField("m_Edges", BindingFlags.NonPublic | BindingFlags.Instance);
            var edgesList = edgesField?.GetValue(GraphData) as System.Collections.IList;
            if (edgesList == null) return;

            var edgesToRemove = new List<object>();
            var nodeId = ShaderGraphReflectionHelper.GetNodeId(actualNode);

            foreach (var edge in edgesList)
            {
                var outputSlotProp = edge.GetType().GetField("m_OutputSlot", BindingFlags.NonPublic | BindingFlags.Instance);
                var inputSlotProp = edge.GetType().GetField("m_InputSlot", BindingFlags.NonPublic | BindingFlags.Instance);
                if (outputSlotProp == null || inputSlotProp == null) continue;

                var outputSlotRef = outputSlotProp.GetValue(edge);
                var inputSlotRef = inputSlotProp.GetValue(edge);

                var nodeField = outputSlotRef.GetType().GetField("m_Node", BindingFlags.NonPublic | BindingFlags.Instance);
                var outNode = (nodeField?.GetValue(outputSlotRef) as dynamic)?.value;
                var inNode = (nodeField?.GetValue(inputSlotRef) as dynamic)?.value;

                string outId = ShaderGraphReflectionHelper.GetNodeId(outNode);
                string inId = ShaderGraphReflectionHelper.GetNodeId(inNode);

                if (outId == nodeId || inId == nodeId)
                {
                    edgesToRemove.Add(edge);
                }
            }

            if (edgesToRemove.Count == 0) return;

            // 转换为数组并调用 RemoveEdges
            var edgeArray = Array.CreateInstance(edgesToRemove[0].GetType().GetInterfaces().First(i => i.Name == "IEdge"), edgesToRemove.Count);
            for (int i = 0; i < edgesToRemove.Count; i++)
                edgeArray.SetValue(edgesToRemove[i], i);

            var removeMethod = GraphData.GetType().GetMethod("RemoveEdges", new Type[] { edgeArray.GetType() });
            removeMethod?.Invoke(GraphData, new[] { edgeArray });

            Save();
            Debug.Log($"已断开节点 {ShaderGraphReflectionHelper.GetNodeName(actualNode)} 上的所有连线");
        }
        private void UpdateNodeEdges(object fromNode, object toNode, object edge)
        {
            var nodeEdgesField = GraphData.GetType().GetField("m_NodeEdges", BindingFlags.NonPublic | BindingFlags.Instance);
            var nodeEdgesDict = nodeEdgesField?.GetValue(GraphData) as System.Collections.IDictionary;
            if (nodeEdgesDict == null) return;

            string fromId = ShaderGraphReflectionHelper.GetNodeId(fromNode);
            string toId = ShaderGraphReflectionHelper.GetNodeId(toNode);

            var edgeInterface = ShaderGraphReflectionHelper.FindType("UnityEditor.Graphing.IEdge");
            var listType = typeof(List<>).MakeGenericType(edgeInterface);

            if (!nodeEdgesDict.Contains(fromId))
                nodeEdgesDict[fromId] = Activator.CreateInstance(listType);
            if (!nodeEdgesDict.Contains(toId))
                nodeEdgesDict[toId] = Activator.CreateInstance(listType);

            ((System.Collections.IList)nodeEdgesDict[fromId]).Add(edge);
            ((System.Collections.IList)nodeEdgesDict[toId]).Add(edge);
        }
        /// <summary>
        /// 从图中移除指定节点
        /// </summary>
        public bool RemoveNode(object node)
        {
            object actualNode = ShaderGraphReflectionHelper.UnwrapJsonData(node);
            if (actualNode == null) return false;

            var graphDataType = GraphData.GetType();

            // 调用 RemoveNode 方法
            var removeMethod = graphDataType.GetMethod("RemoveNode", new Type[] { actualNode.GetType() });
            if (removeMethod == null)
            {
                Debug.LogError("找不到 GraphData.RemoveNode 方法");
                return false;
            }

            try
            {
                removeMethod.Invoke(GraphData, new[] { actualNode });
                Save();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"移除节点失败: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// 清除图中所有节点、边和分组
        /// </summary>
        public void ClearAllNodes()
        {
            var graphDataType = GraphData.GetType();

            // 清空 m_Nodes
            var nodesField = graphDataType.GetField("m_Nodes", BindingFlags.NonPublic | BindingFlags.Instance);
            var nodesList = nodesField?.GetValue(GraphData) as System.Collections.IList;
            nodesList?.Clear();

            // 清空 m_Edges
            var edgesField = graphDataType.GetField("m_Edges", BindingFlags.NonPublic | BindingFlags.Instance);
            var edgesList = edgesField?.GetValue(GraphData) as System.Collections.IList;
            edgesList?.Clear();

            // 清空 m_NodeDictionary
            var dictField = graphDataType.GetField("m_NodeDictionary", BindingFlags.NonPublic | BindingFlags.Instance);
            var dict = dictField?.GetValue(GraphData) as System.Collections.IDictionary;
            dict?.Clear();

            // 清空 m_NodeEdges（如果存在）
            var nodeEdgesField = graphDataType.GetField("m_NodeEdges", BindingFlags.NonPublic | BindingFlags.Instance);
            var nodeEdgesDict = nodeEdgesField?.GetValue(GraphData) as System.Collections.IDictionary;
            nodeEdgesDict?.Clear();

            // 不要清空 m_GroupDatas, m_GroupItems, m_StickyNoteDatas

           
            Debug.Log("已清除所有节点和边（保留组数据）");
        }
        /// <summary>
        /// 从黑板中移除指定的属性（通过传入属性对象的引用名称查找实际实例）
        /// </summary>
        public bool RemoveShaderProperty(AbstractShaderProperty property)
        {
            // 通过反射获取 referenceName
            var refNameProp = property.GetType().GetProperty("referenceName", BindingFlags.Public | BindingFlags.Instance);
            if (refNameProp == null)
            {
                Debug.LogError("AbstractShaderProperty 类型不包含 referenceName 属性");
                return false;
            }
    
            string referenceName = refNameProp.GetValue(property) as string;
            if (string.IsNullOrEmpty(referenceName))
            {
                Debug.LogError("无法获取属性的引用名称");
                return false;
            }

            // 直接调用字符串重载版本
            return RemoveShaderProperty(referenceName);
        }

        /// <summary>
        /// 通过引用名称移除黑板属性
        /// </summary>
        public bool RemoveShaderProperty(string referenceName)
        {
            var actualProp = ShaderGraphReflectionHelper.FindShaderPropertyByName(GraphData, referenceName);
            if (actualProp == null)
            {
                Debug.LogError($"图中找不到名为 {referenceName} 的属性");
                return false;
            }

            var removeMethod = GraphData.GetType().GetMethod("RemoveGraphInput", new Type[] { typeof(AbstractShaderProperty) });
            if (removeMethod == null)
            {
                Debug.LogError("找不到 GraphData.RemoveGraphInput 方法");
                return false;
            }

            try
            {
                removeMethod.Invoke(GraphData, new object[] { actualProp });
                Save();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"移除属性失败: {ex.Message}");
                return false;
            }
        }
        /// <param name="property"></param>
        // ---------- 黑板属性 ----------
        public void AddShaderProperty(AbstractShaderProperty property)
        {
            var graphDataType = GraphData.GetType();
            var addMethod = graphDataType.GetMethod("AddGraphInputNoSanitization", BindingFlags.NonPublic | BindingFlags.Instance);
            addMethod?.Invoke(GraphData, new object[] { property, -1 });

           
            Save();
        }
        private void ConfigureCustomFunctionSlots(object node, IEnumerable<SlotDefinition> slots)
        {
            var nodeType = node.GetType();
            // 清空现有插槽
            var clearMethod = nodeType.GetMethod("RemoveSlotsNameNotMatching", BindingFlags.Public | BindingFlags.Instance);
            clearMethod?.Invoke(node, new object[] { new int[0], true });
    
            var materialSlotType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.MaterialSlot");
            var createMethod = materialSlotType?.GetMethod("CreateMaterialSlot", BindingFlags.Public | BindingFlags.Static);
    
            int nextId = 0;
            foreach (var def in slots)
            {
                object internalValueType = EnumConverter.ToInternalEnum(def.ValueType);
                object internalStageCapability = EnumConverter.ToInternalEnum(def.StageCapability);
                object internalSlotType = def.IsInput ? EnumConverter.ToInternalEnum(SlotType.Input) : EnumConverter.ToInternalEnum(SlotType.Output);
        
                object slot = createMethod.Invoke(null, new object[]
                {
                    internalValueType,
                    nextId++,
                    def.DisplayName,
                    string.IsNullOrEmpty(def.ShaderOutputName) ? def.DisplayName : def.ShaderOutputName,
                    internalSlotType,
                    def.DefaultValue,
                    internalStageCapability,
                    false
                });
        
                // 使用 AddSlot 方法添加
                var addSlotMethod = nodeType.GetMethod("AddSlot", new Type[] { materialSlotType });
                addSlotMethod.Invoke(node, new object[] { slot });
            }
    
            // 不需要调用 UpdateSlots，因为插槽已手动设置
        }     // ---------- 节点类型映射 ----------
        private static readonly Dictionary<ShaderGraphNodeType, string> NodeTypeNameMap = new Dictionary<ShaderGraphNodeType, string>
        {
            { ShaderGraphNodeType.ColorNode, "UnityEditor.ShaderGraph.ColorNode" },
            { ShaderGraphNodeType.BooleanNode, "UnityEditor.ShaderGraph.BooleanNode" },
            { ShaderGraphNodeType.ConstantNode, "UnityEditor.ShaderGraph.ConstantNode" },
            { ShaderGraphNodeType.IntegerNode, "UnityEditor.ShaderGraph.IntegerNode" },
            { ShaderGraphNodeType.SliderNode, "UnityEditor.ShaderGraph.SliderNode" },
            { ShaderGraphNodeType.TimeNode, "UnityEditor.ShaderGraph.TimeNode" },
            { ShaderGraphNodeType.Vector1Node, "UnityEditor.ShaderGraph.Vector1Node" },
            { ShaderGraphNodeType.Vector2Node, "UnityEditor.ShaderGraph.Vector2Node" },
            { ShaderGraphNodeType.Vector3Node, "UnityEditor.ShaderGraph.Vector3Node" },
            { ShaderGraphNodeType.Vector4Node, "UnityEditor.ShaderGraph.Vector4Node" },
            { ShaderGraphNodeType.InstanceIDNode, "UnityEditor.ShaderGraph.InstanceIDNode" },
            { ShaderGraphNodeType.NormalVectorNode, "UnityEditor.ShaderGraph.NormalVectorNode" },
            { ShaderGraphNodeType.PositionNode, "UnityEditor.ShaderGraph.PositionNode" },
            { ShaderGraphNodeType.ScreenPositionNode, "UnityEditor.ShaderGraph.ScreenPositionNode" },
            { ShaderGraphNodeType.TangentVectorNode, "UnityEditor.ShaderGraph.TangentVectorNode" },
            { ShaderGraphNodeType.UVNode, "UnityEditor.ShaderGraph.UVNode" },
            { ShaderGraphNodeType.VertexColorNode, "UnityEditor.ShaderGraph.VertexColorNode" },
            { ShaderGraphNodeType.VertexIDNode, "UnityEditor.ShaderGraph.VertexIDNode" },
            { ShaderGraphNodeType.ViewDirectionNode, "UnityEditor.ShaderGraph.ViewDirectionNode" },
            { ShaderGraphNodeType.ViewVectorNode, "UnityEditor.ShaderGraph.ViewVectorNode" },
            { ShaderGraphNodeType.BitangentVectorNode, "UnityEditor.ShaderGraph.BitangentVectorNode" },
            { ShaderGraphNodeType.BlackbodyNode, "UnityEditor.ShaderGraph.BlackbodyNode" },
            { ShaderGraphNodeType.GradientNode, "UnityEditor.ShaderGraph.GradientNode" },
            { ShaderGraphNodeType.SampleGradientNode, "UnityEditor.ShaderGraph.SampleGradient" },
            { ShaderGraphNodeType.AmbientNode, "UnityEditor.ShaderGraph.AmbientNode" },
            { ShaderGraphNodeType.BakedGINode, "UnityEditor.ShaderGraph.BakedGINode" },
            { ShaderGraphNodeType.MainLightDirectionNode, "UnityEditor.ShaderGraph.MainLightDirectionNode" },
            { ShaderGraphNodeType.ReflectionProbeNode, "UnityEditor.ShaderGraph.ReflectionProbeNode" },
            { ShaderGraphNodeType.Matrix2Node, "UnityEditor.ShaderGraph.Matrix2Node" },
            { ShaderGraphNodeType.Matrix3Node, "UnityEditor.ShaderGraph.Matrix3Node" },
            { ShaderGraphNodeType.Matrix4Node, "UnityEditor.ShaderGraph.Matrix4Node" },
            { ShaderGraphNodeType.TransformationMatrixNode, "UnityEditor.ShaderGraph.TransformationMatrixNode" },
            { ShaderGraphNodeType.DielectricSpecularNode, "UnityEditor.ShaderGraph.DielectricSpecularNode" },
            { ShaderGraphNodeType.MetalReflectanceNode, "UnityEditor.ShaderGraph.MetalReflectanceNode" },
            { ShaderGraphNodeType.SceneDepthNode, "UnityEditor.ShaderGraph.SceneDepthNode" },
            { ShaderGraphNodeType.SceneDepthDifferenceNode, "UnityEditor.ShaderGraph.SceneDepthDifferenceNode" },
            { ShaderGraphNodeType.SampleTexture2DNode, "UnityEditor.ShaderGraph.SampleTexture2DNode" },
            { ShaderGraphNodeType.SampleTexture2DLODNode, "UnityEditor.ShaderGraph.SampleTexture2DLODNode" },
            { ShaderGraphNodeType.SampleTexture2DArrayNode, "UnityEditor.ShaderGraph.SampleTexture2DArrayNode" },
            { ShaderGraphNodeType.SampleTexture3DNode, "UnityEditor.ShaderGraph.SampleTexture3DNode" },
            { ShaderGraphNodeType.SampleVirtualTextureNode, "UnityEditor.ShaderGraph.SampleVirtualTextureNode" },
            { ShaderGraphNodeType.SamplerStateNode, "UnityEditor.ShaderGraph.SamplerStateNode" },
            { ShaderGraphNodeType.CalculateLevelOfDetailTexture2DNode, "UnityEditor.ShaderGraph.CalculateLevelOfDetailTexture2DNode" },
            { ShaderGraphNodeType.Texture2DAssetNode, "UnityEditor.ShaderGraph.Texture2DAssetNode" },
            { ShaderGraphNodeType.Texture2DArrayAssetNode, "UnityEditor.ShaderGraph.Texture2DArrayAssetNode" },
            { ShaderGraphNodeType.Texture3DAssetNode, "UnityEditor.ShaderGraph.Texture3DAssetNode" },
            { ShaderGraphNodeType.CubemapAssetNode, "UnityEditor.ShaderGraph.CubemapAssetNode" },
            { ShaderGraphNodeType.SampleCubemapNode, "UnityEditor.ShaderGraph.SampleCubemapNode" },
            { ShaderGraphNodeType.SampleRawCubemapNode, "UnityEditor.ShaderGraph.SampleRawCubemapNode" },
            { ShaderGraphNodeType.GatherTexture2DNode, "UnityEditor.ShaderGraph.GatherTexture2DNode" },
            { ShaderGraphNodeType.TexelSizeNode, "UnityEditor.ShaderGraph.Texture2DPropertiesNode" },
            { ShaderGraphNodeType.CustomInterpolatorNode, "UnityEditor.ShaderGraph.CustomInterpolatorNode" },
            { ShaderGraphNodeType.PropertyNode, "UnityEditor.ShaderGraph.PropertyNode" },
            { ShaderGraphNodeType.AddNode, "UnityEditor.ShaderGraph.AddNode" },
            //-----------------------------------------------
            { ShaderGraphNodeType.ChannelMixerNode, "UnityEditor.ShaderGraph.ChannelMixerNode" },
            { ShaderGraphNodeType.ContrastNode, "UnityEditor.ShaderGraph.ContrastNode" },
            { ShaderGraphNodeType.HueNode, "UnityEditor.ShaderGraph.HueNode" },
            { ShaderGraphNodeType.InvertColorsNode, "UnityEditor.ShaderGraph.InvertColorsNode" },
            { ShaderGraphNodeType.ReplaceColorNode, "UnityEditor.ShaderGraph.ReplaceColorNode" },
            { ShaderGraphNodeType.SaturationNode, "UnityEditor.ShaderGraph.SaturationNode" },
            { ShaderGraphNodeType.WhiteBalanceNode, "UnityEditor.ShaderGraph.WhiteBalanceNode" },
            { ShaderGraphNodeType.BlendNode, "UnityEditor.ShaderGraph.BlendNode" },
            { ShaderGraphNodeType.DitherNode, "UnityEditor.ShaderGraph.DitherNode" },
            { ShaderGraphNodeType.FadeTransitionNode, "UnityEditor.ShaderGraph.FadeTransitionNode" },
            { ShaderGraphNodeType.ChannelMaskNode, "UnityEditor.ShaderGraph.ChannelMaskNode" },
            { ShaderGraphNodeType.ColorMaskNode, "UnityEditor.ShaderGraph.ColorMaskNode" },
            { ShaderGraphNodeType.NormalBlendNode, "UnityEditor.ShaderGraph.NormalBlendNode" },
            { ShaderGraphNodeType.NormalFromHeightNode, "UnityEditor.ShaderGraph.NormalFromHeightNode" },
            { ShaderGraphNodeType.NormalFromTextureNode, "UnityEditor.ShaderGraph.NormalFromTextureNode" },
            { ShaderGraphNodeType.NormalReconstructZNode, "UnityEditor.ShaderGraph.NormalReconstructZNode" },
            { ShaderGraphNodeType.NormalStrengthNode, "UnityEditor.ShaderGraph.NormalStrengthNode" },
            { ShaderGraphNodeType.NormalUnpackNode, "UnityEditor.ShaderGraph.NormalUnpackNode" },
            { ShaderGraphNodeType.ColorspaceConversionNode, "UnityEditor.ShaderGraph.ColorspaceConversionNode" },
            { ShaderGraphNodeType.CombineNode, "UnityEditor.ShaderGraph.CombineNode" },
            { ShaderGraphNodeType.FlipNode, "UnityEditor.ShaderGraph.FlipNode" },
            { ShaderGraphNodeType.SplitNode, "UnityEditor.ShaderGraph.SplitNode" },
            { ShaderGraphNodeType.SwizzleNode, "UnityEditor.ShaderGraph.SwizzleNode" },
            { ShaderGraphNodeType.AbsoluteNode, "UnityEditor.ShaderGraph.AbsoluteNode" },
            { ShaderGraphNodeType.ExponentialNode, "UnityEditor.ShaderGraph.ExponentialNode" },
            { ShaderGraphNodeType.LengthNode, "UnityEditor.ShaderGraph.LengthNode" },
            { ShaderGraphNodeType.LogNode, "UnityEditor.ShaderGraph.LogNode" },
            { ShaderGraphNodeType.ModuloNode, "UnityEditor.ShaderGraph.ModuloNode" },
            { ShaderGraphNodeType.NegateNode, "UnityEditor.ShaderGraph.NegateNode" },
            { ShaderGraphNodeType.NormalizeNode, "UnityEditor.ShaderGraph.NormalizeNode" },
            { ShaderGraphNodeType.PosterizeNode, "UnityEditor.ShaderGraph.PosterizeNode" },
            { ShaderGraphNodeType.ReciprocalNode, "UnityEditor.ShaderGraph.ReciprocalNode" },
            { ShaderGraphNodeType.ReciprocalSquareRootNode, "UnityEditor.ShaderGraph.ReciprocalSquareRootNode" },
            { ShaderGraphNodeType.DivideNode, "UnityEditor.ShaderGraph.DivideNode" },
            { ShaderGraphNodeType.MultiplyNode, "UnityEditor.ShaderGraph.MultiplyNode" },
            { ShaderGraphNodeType.PowerNode, "UnityEditor.ShaderGraph.PowerNode" },
            { ShaderGraphNodeType.SquareRootNode, "UnityEditor.ShaderGraph.SquareRootNode" },
            { ShaderGraphNodeType.SubtractNode, "UnityEditor.ShaderGraph.SubtractNode" },
            { ShaderGraphNodeType.DDXNode, "UnityEditor.ShaderGraph.DDXNode" },
            { ShaderGraphNodeType.DDXYNode, "UnityEditor.ShaderGraph.DDXYNode" },
            { ShaderGraphNodeType.DDYNode, "UnityEditor.ShaderGraph.DDYNode" },
            { ShaderGraphNodeType.InverseLerpNode, "UnityEditor.ShaderGraph.InverseLerpNode" },
            { ShaderGraphNodeType.LerpNode, "UnityEditor.ShaderGraph.LerpNode" },
            { ShaderGraphNodeType.SmoothstepNode, "UnityEditor.ShaderGraph.SmoothstepNode" },
            { ShaderGraphNodeType.MatrixConstructionNode, "UnityEditor.ShaderGraph.MatrixConstructionNode" },
            { ShaderGraphNodeType.MatrixDeterminantNode, "UnityEditor.ShaderGraph.MatrixDeterminantNode" },
            { ShaderGraphNodeType.MatrixSplitNode, "UnityEditor.ShaderGraph.MatrixSplitNode" },
            { ShaderGraphNodeType.MatrixTransposeNode, "UnityEditor.ShaderGraph.MatrixTransposeNode" },
            { ShaderGraphNodeType.ClampNode, "UnityEditor.ShaderGraph.ClampNode" },
            { ShaderGraphNodeType.FractionNode, "UnityEditor.ShaderGraph.FractionNode" },
            { ShaderGraphNodeType.MaximumNode, "UnityEditor.ShaderGraph.MaximumNode" },
            { ShaderGraphNodeType.MinimumNode, "UnityEditor.ShaderGraph.MinimumNode" },
            { ShaderGraphNodeType.OneMinusNode, "UnityEditor.ShaderGraph.OneMinusNode" },
            { ShaderGraphNodeType.RandomRangeNode, "UnityEditor.ShaderGraph.RandomRangeNode" },
            { ShaderGraphNodeType.RemapNode, "UnityEditor.ShaderGraph.RemapNode" },
            { ShaderGraphNodeType.SaturateNode, "UnityEditor.ShaderGraph.SaturateNode" },
            { ShaderGraphNodeType.CeilingNode, "UnityEditor.ShaderGraph.CeilingNode" },
            { ShaderGraphNodeType.FloorNode, "UnityEditor.ShaderGraph.FloorNode" },
            { ShaderGraphNodeType.RoundNode, "UnityEditor.ShaderGraph.RoundNode" },
            { ShaderGraphNodeType.SignNode, "UnityEditor.ShaderGraph.SignNode" },
            { ShaderGraphNodeType.StepNode, "UnityEditor.ShaderGraph.StepNode" },
            { ShaderGraphNodeType.TruncateNode, "UnityEditor.ShaderGraph.TruncateNode" },
            { ShaderGraphNodeType.SineNode, "UnityEditor.ShaderGraph.SineNode" },
            { ShaderGraphNodeType.TangentNode, "UnityEditor.ShaderGraph.TangentNode" },
            { ShaderGraphNodeType.ArccosineNode, "UnityEditor.ShaderGraph.ArccosineNode" },
            { ShaderGraphNodeType.ArcsineNode, "UnityEditor.ShaderGraph.ArcsineNode" },
            { ShaderGraphNodeType.Arctangent2Node, "UnityEditor.ShaderGraph.Arctangent2Node" },
            { ShaderGraphNodeType.ArctangentNode, "UnityEditor.ShaderGraph.ArctangentNode" },
            { ShaderGraphNodeType.CosineNode, "UnityEditor.ShaderGraph.CosineNode" },
            { ShaderGraphNodeType.DegreesToRadiansNode, "UnityEditor.ShaderGraph.DegreesToRadiansNode" },
            { ShaderGraphNodeType.HyperbolicCosineNode, "UnityEditor.ShaderGraph.HyperbolicCosineNode" },
            { ShaderGraphNodeType.HyperbolicSineNode, "UnityEditor.ShaderGraph.HyperbolicSineNode" },
            { ShaderGraphNodeType.HyperbolicTangentNode, "UnityEditor.ShaderGraph.HyperbolicTangentNode" },
            { ShaderGraphNodeType.RadiansToDegreesNode, "UnityEditor.ShaderGraph.RadiansToDegreesNode" },
            { ShaderGraphNodeType.CrossProductNode, "UnityEditor.ShaderGraph.CrossProductNode" },
            { ShaderGraphNodeType.DistanceNode, "UnityEditor.ShaderGraph.DistanceNode" },
            { ShaderGraphNodeType.DotProductNode, "UnityEditor.ShaderGraph.DotProductNode" },
            { ShaderGraphNodeType.FresnelEffectNode, "UnityEditor.ShaderGraph.FresnelNode" },        // 注意类名是 FresnelNode
            { ShaderGraphNodeType.ProjectionNode, "UnityEditor.ShaderGraph.ProjectionNode" },
            { ShaderGraphNodeType.ReflectionNode, "UnityEditor.ShaderGraph.ReflectionNode" },
            { ShaderGraphNodeType.RefractNode, "UnityEditor.ShaderGraph.RefractNode" },
            { ShaderGraphNodeType.RejectionNode, "UnityEditor.ShaderGraph.RejectionNode" },
            { ShaderGraphNodeType.RotateAboutAxisNode, "UnityEditor.ShaderGraph.RotateAboutAxisNode" },
            { ShaderGraphNodeType.SphereMaskNode, "UnityEditor.ShaderGraph.SphereMaskNode" },
            { ShaderGraphNodeType.TransformNode, "UnityEditor.ShaderGraph.TransformNode" },
            { ShaderGraphNodeType.NoiseSineWaveNode, "UnityEditor.ShaderGraph.NoiseSineWaveNode" },
            { ShaderGraphNodeType.SawtoothWaveNode, "UnityEditor.ShaderGraph.SawtoothWaveNode" },
            { ShaderGraphNodeType.SquareWaveNode, "UnityEditor.ShaderGraph.SquareWaveNode" },
            { ShaderGraphNodeType.TriangleWaveNode, "UnityEditor.ShaderGraph.TriangleWaveNode" },
            { ShaderGraphNodeType.ComputeDeformNode, "UnityEditor.ShaderGraph.ComputeDeformNode" },
            { ShaderGraphNodeType.LinearBlendSkinningNode, "UnityEditor.ShaderGraph.LinearBlendSkinningNode" },
            { ShaderGraphNodeType.GradientNoiseNode, "UnityEditor.ShaderGraph.GradientNoiseNode" },
            { ShaderGraphNodeType.SimpleNoiseNode, "UnityEditor.ShaderGraph.NoiseNode" },    
            { ShaderGraphNodeType.VoronoiNode, "UnityEditor.ShaderGraph.VoronoiNode" },
            { ShaderGraphNodeType.EllipseNode, "UnityEditor.ShaderGraph.EllipseNode" },
            { ShaderGraphNodeType.PolygonNode, "UnityEditor.ShaderGraph.PolygonNode" },
            { ShaderGraphNodeType.RectangleNode, "UnityEditor.ShaderGraph.RectangleNode" },
            { ShaderGraphNodeType.RoundedPolygonNode, "UnityEditor.ShaderGraph.RoundedPolygonNode" },
            { ShaderGraphNodeType.RoundedRectangleNode, "UnityEditor.ShaderGraph.RoundedRectangleNode" },
            { ShaderGraphNodeType.CheckerboardNode, "UnityEditor.ShaderGraph.CheckerboardNode" },
            { ShaderGraphNodeType.OrNode, "UnityEditor.ShaderGraph.OrNode" },
            { ShaderGraphNodeType.AllNode, "UnityEditor.ShaderGraph.AllNode" },
            { ShaderGraphNodeType.AndNode, "UnityEditor.ShaderGraph.AndNode" },
            { ShaderGraphNodeType.AnyNode, "UnityEditor.ShaderGraph.AnyNode" },
            { ShaderGraphNodeType.BranchNode, "UnityEditor.ShaderGraph.BranchNode" },
            { ShaderGraphNodeType.BranchOnInputConnectionNode, "UnityEditor.ShaderGraph.BranchOnInputConnectionNode" },
            { ShaderGraphNodeType.ComparisonNode, "UnityEditor.ShaderGraph.ComparisonNode" },
            { ShaderGraphNodeType.IsFrontFaceNode, "UnityEditor.ShaderGraph.IsFrontFaceNode" },
            { ShaderGraphNodeType.IsInfiniteNode, "UnityEditor.ShaderGraph.IsInfiniteNode" },
            { ShaderGraphNodeType.IsNanNode, "UnityEditor.ShaderGraph.IsNanNode" },
            { ShaderGraphNodeType.NandNode, "UnityEditor.ShaderGraph.NandNode" },
            { ShaderGraphNodeType.NotNode, "UnityEditor.ShaderGraph.NotNode" },
            { ShaderGraphNodeType.ParallaxMappingNode, "UnityEditor.ShaderGraph.ParallaxMappingNode" },
            { ShaderGraphNodeType.ParallaxOcclusionMappingNode, "UnityEditor.ShaderGraph.ParallaxOcclusionMappingNode" },
            { ShaderGraphNodeType.PolarCoordinatesNode, "UnityEditor.ShaderGraph.PolarCoordinatesNode" },
            { ShaderGraphNodeType.RadialShearNode, "UnityEditor.ShaderGraph.RadialShearNode" },
            { ShaderGraphNodeType.RotateNode, "UnityEditor.ShaderGraph.RotateNode" },
            { ShaderGraphNodeType.SpherizeNode, "UnityEditor.ShaderGraph.SpherizeNode" },
            { ShaderGraphNodeType.TilingAndOffsetNode, "UnityEditor.ShaderGraph.TilingAndOffsetNode" },
            { ShaderGraphNodeType.TriplanarNode, "UnityEditor.ShaderGraph.TriplanarNode" },
            { ShaderGraphNodeType.TwirlNode, "UnityEditor.ShaderGraph.TwirlNode" },
            { ShaderGraphNodeType.FlipbookNode, "UnityEditor.ShaderGraph.FlipbookNode" },
            { ShaderGraphNodeType.DropdownNode, "UnityEditor.ShaderGraph.DropdownNode" },
            { ShaderGraphNodeType.KeywordNode, "UnityEditor.ShaderGraph.KeywordNode" },
            { ShaderGraphNodeType.CustomFunctionNode, "UnityEditor.ShaderGraph.CustomFunctionNode" },
            { ShaderGraphNodeType.PreviewNode, "UnityEditor.ShaderGraph.PreviewNode" },
            { ShaderGraphNodeType.SplitTextureTransformNode, "UnityEditor.ShaderGraph.SplitTextureTransformNode" },
            { ShaderGraphNodeType.MeterValueNode, "UnityEditor.ShaderGraph.MeterValueNode" },
            { ShaderGraphNodeType.RangeBarValueNode, "UnityEditor.ShaderGraph.RangeBarNode" },      // 注意类名差异
            { ShaderGraphNodeType.RectTransformSizeNode, "UnityEditor.ShaderGraph.RectTransformSizeNode" },
            { ShaderGraphNodeType.SelectableBranchNode, "UnityEditor.ShaderGraph.SelectableBranchNode" },
            { ShaderGraphNodeType.SelectableStateNode, "UnityEditor.ShaderGraph.SelectableStateNode" },
            { ShaderGraphNodeType.SliderValueNode, "UnityEditor.ShaderGraph.SliderValueNode" },
            { ShaderGraphNodeType.ToggleStateNode, "UnityEditor.ShaderGraph.ToggleStateNode" },
            { ShaderGraphNodeType.SubGraphNode, "UnityEditor.ShaderGraph.SubGraphNode" },
            { ShaderGraphNodeType.UniversalSampleBufferNode, "UnityEditor.Rendering.Universal.UniversalSampleBufferNode" },
            { ShaderGraphNodeType.LightTextureNode, "UnityEngine.Experimental.Rendering.Universal.LightTextureNode" },
        };
    }
}