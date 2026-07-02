using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using 龙哥的秘密花园.节点库;

namespace 龙哥的秘密花园.节点库
{
    /// <summary>
    /// URP ShaderGraph 专用辅助类，封装 Target/SubTarget 激活、块操作、渲染状态设置等。
    /// 所有内部类型均通过反射操作，避免直接引用 internal 类型。
    /// </summary>
    public class URPShaderGraphHelper
    {
      
        private readonly GraphDataContext m_Context;
        private object m_GraphData;
        private const string UniversalTargetTypeName = "UnityEditor.Rendering.Universal.ShaderGraph.UniversalTarget";
        private const string UniversalUnlitSubTargetTypeName = "UnityEditor.Rendering.Universal.ShaderGraph.UniversalUnlitSubTarget";
        private const string UniversalLitSubTargetTypeName = "UnityEditor.Rendering.Universal.ShaderGraph.UniversalLitSubTarget";

        public URPShaderGraphHelper(GraphDataContext context)
        {
            m_Context = context ?? throw new ArgumentNullException(nameof(context));
            m_GraphData = context.GraphData;
        }

        public void Refresh() => m_GraphData = m_Context.GraphData;

        #region 辅助反射方法
        
        public void SetWorkflowMode(WorkflowMode mode)
        {
            var subTarget = GetActiveSubTarget();
            if (subTarget == null) return;
            var obj= GetEnumValue("UnityEditor.Rendering.Universal.ShaderGraph.WorkflowMode", mode.ToString());
            SetFieldValue(subTarget, "m_WorkflowMode", obj, BindingFlags.NonPublic | BindingFlags.Instance);
            m_Context.Save();
        }
        public void SetReceiveShadows(bool receive)
        {
            var target = GetActiveUniversalTarget();
            if (target == null) return;
            SetFieldValue(target, "m_ReceiveShadows", receive, BindingFlags.NonPublic | BindingFlags.Instance);
            m_Context.Save();
        }
        public void SetFragmentNormalSpace(NormalDropOffSpace space)
        {
            var subTarget = GetActiveSubTarget();
            if (subTarget == null) return;
            var obj = GetEnumValue("UnityEditor.ShaderGraph.NormalDropOffSpace", space.ToString());
            SetFieldValue(subTarget, "m_NormalDropOffSpace", obj, BindingFlags.NonPublic | BindingFlags.Instance);
            m_Context.Save();
        }
        private static object CallMethod(object obj, string methodName, BindingFlags flags, Type[] paramTypes, object[] args)
        {
            if (obj == null) return null;
            MethodInfo method;
            if (paramTypes == null || paramTypes.Length == 0)
            {
                method = obj.GetType().GetMethod(methodName, flags);
            }
            else
            {
                method = obj.GetType().GetMethod(methodName, flags, null, paramTypes, null);
            }
            return method?.Invoke(obj, args);
        }

        private static object GetFieldValue(object obj, string fieldName, BindingFlags flags)
        {
            if (obj == null) return null;
            var field = obj.GetType().GetField(fieldName, flags);
            return field?.GetValue(obj);
        }

        private static void SetFieldValue(object obj, string fieldName, object value, BindingFlags flags)
        {
            if (obj == null) return;
            var field = obj.GetType().GetField(fieldName, flags);
            field?.SetValue(obj, value);
        }

        private static object GetPropertyValue(object obj, string propName, BindingFlags flags)
        {
            if (obj == null) return null;
            var prop = obj.GetType().GetProperty(propName, flags);
            return prop?.GetValue(obj);
        }

        private static void SetPropertyValue(object obj, string propName, object value, BindingFlags flags)
        {
            if (obj == null) return;
            var prop = obj.GetType().GetProperty(propName, flags);
            if (prop != null && prop.CanWrite)
                prop.SetValue(obj, value);
        }

        #endregion

       // Target 操作

        private List<object> GetActiveTargets()
        {
            var result = new List<object>();
            var activeTargets = GetFieldValue(m_GraphData, "m_ActiveTargets", BindingFlags.NonPublic | BindingFlags.Instance) as System.Collections.IList;
            if (activeTargets == null) return result;

            foreach (var item in activeTargets)
            {
                var target = ShaderGraphReflectionHelper.UnwrapJsonData(item);
                if (target != null) result.Add(target);
            }
            return result;
        }

        private object GetActiveUniversalTarget()
        {
            var targetType = ShaderGraphReflectionHelper.FindType(UniversalTargetTypeName);
            if (targetType == null) return null;
            foreach (var t in GetActiveTargets())
                if (t.GetType() == targetType)
                    return t;
            return null;
        }
        #region 辅助方法

        /// <summary>
        /// 确保 m_AllPotentialTargets 列表已填充（反序列化后可能为空）
        /// </summary>
        private void EnsurePotentialTargets()
        {
            var potentialTargets = GetFieldValue(m_GraphData, "m_AllPotentialTargets", BindingFlags.NonPublic | BindingFlags.Instance) as System.Collections.IList;
            if (potentialTargets == null || potentialTargets.Count == 0)
            {
                var addKnownMethod = m_GraphData.GetType().GetMethod("AddKnownTargetsToPotentialTargets", BindingFlags.NonPublic | BindingFlags.Instance);
                addKnownMethod?.Invoke(m_GraphData, null);
                Debug.Log("已手动填充 m_AllPotentialTargets");
            }
        }

        public bool ActivateUniversalTarget()
{


    // 1. 检查 GraphData 类型
    var graphDataType = m_GraphData?.GetType();


    // 2. 查找 UniversalTarget 类型
    var universalTargetType = ShaderGraphReflectionHelper.FindType(UniversalTargetTypeName);
  
    if (universalTargetType == null)
    {

        return false;
    }

    // 3. 检查已激活的 UniversalTarget
    var existingActive = GetActiveUniversalTarget();
 
    if (existingActive != null)
    {

        return true;
    }

    // 4. 获取 m_AllPotentialTargets 列表
    var potentialTargetsField = m_GraphData.GetType().GetField("m_AllPotentialTargets", BindingFlags.NonPublic | BindingFlags.Instance);
    var potentialTargets = potentialTargetsField?.GetValue(m_GraphData) as System.Collections.IList;
  

    // 如果列表为空，尝试手动调用 AddKnownTargetsToPotentialTargets
    if (potentialTargets == null || potentialTargets.Count == 0)
    {
        var addKnownMethod = m_GraphData.GetType().GetMethod("AddKnownTargetsToPotentialTargets", BindingFlags.NonPublic | BindingFlags.Instance);
     
        if (addKnownMethod != null)
        {
            addKnownMethod.Invoke(m_GraphData, null);
         
            potentialTargets = potentialTargetsField?.GetValue(m_GraphData) as System.Collections.IList;
            
        }
        else
        {
           
        }
    }

    if (potentialTargets == null || potentialTargets.Count == 0)
    {
       
        return false;
    }

    // 6. 遍历查找 UniversalTarget 实例
    object foundTarget = null;
    int idx = 0;
    foreach (var pt in potentialTargets)
    {
        var ptType = pt?.GetType();
        
        if (pt == null) continue;

        var getTargetMethod = ptType.GetMethod("GetTarget");
       
        if (getTargetMethod == null) continue;

        var target = getTargetMethod.Invoke(pt, null);
        var targetTypeName = target?.GetType().FullName ?? "null";
        
        if (target != null && target.GetType() == universalTargetType)
        {
            foundTarget = target;
          
            break;
        }
        idx++;
    }

    if (foundTarget == null)
    {
        
        return false;
    }

    // 7. 查找 Target 基类类型
    var targetBaseType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Target");
   
    if (targetBaseType == null)
    {
        return false;
    }

    // 8. 查找 SetTargetActive 方法（两个参数：Target, bool）
    var setActiveMethod = m_GraphData.GetType().GetMethod("SetTargetActive", new[] { targetBaseType, typeof(bool) });
  
    if (setActiveMethod == null)
    {
        // 尝试无参数的签名（兼容旧版本）
        setActiveMethod = m_GraphData.GetType().GetMethod("SetTargetActive", new[] { targetBaseType });
        if (setActiveMethod == null)
            return false;
        
   
    }

    // 9. 调用 SetTargetActive
    try
    {
        if (setActiveMethod.GetParameters().Length == 2)
            setActiveMethod.Invoke(m_GraphData, new object[] { foundTarget, false });
        else
            setActiveMethod.Invoke(m_GraphData, new[] { foundTarget });
       
    }
    catch (Exception ex)
    {
    
        return false;
    }

    // 10. 排序并验证
    try
    {
        var sortMethod = m_GraphData.GetType().GetMethod("SortActiveTargets", Type.EmptyTypes);
        sortMethod?.Invoke(m_GraphData, null);
        
    }
    catch (Exception ex)
    {
    }

    try
    {
        var validateMethod = m_GraphData.GetType().GetMethod("ValidateGraph", Type.EmptyTypes);
        validateMethod?.Invoke(m_GraphData, null);
      
    }
    catch (Exception ex)
    {
        
    }

    // 12. 保存并输出最终状态
    m_Context.Save();
  

    var activeTargetsList = GetFieldValue(m_GraphData, "m_ActiveTargets", BindingFlags.NonPublic | BindingFlags.Instance) as System.Collections.IList;
    
    return true;
}
        public bool SetActiveSubTarget(string subTargetTypeName)
        {
            var targetType = ShaderGraphReflectionHelper.FindType(UniversalTargetTypeName);
            if (targetType == null) return false;

            object universalTarget = GetActiveUniversalTarget();
            if (universalTarget == null) return false;

            var subTargetType = ShaderGraphReflectionHelper.FindType(subTargetTypeName);
            if (subTargetType == null) return false;

            var success = (bool)CallMethod(universalTarget, "TrySetActiveSubTarget", BindingFlags.Public | BindingFlags.Instance, new[] { typeof(Type) }, new object[] { subTargetType });

            if (success) m_Context.Save();
            return success;
        }
    #region Sprite 预设配置

/// <summary> 配置为 URP Sprite Lit ShaderGraph </summary>
public void SetupAsURPSpriteLit()
{
    EnsureContextsExist();
    ActivateUniversalTarget();
    SetActiveSubTargetByType("UnityEditor.Rendering.Universal.ShaderGraph.UniversalSpriteLitSubTarget");
    ClearAllBlocks();

    // 必需块（来自 CreateSpriteLitShaderGraph.cs）
    AddVertexBlock(VertexBlockType.Position);
    AddVertexBlock(VertexBlockType.Normal);
    AddVertexBlock(VertexBlockType.Tangent);
    AddSurfaceBlock(SurfaceBlockType.BaseColor);
    AddUniversalSurfaceBlock("SpriteMask");
    AddSurfaceBlock(SurfaceBlockType.NormalTS);
    AddSurfaceBlock(SurfaceBlockType.Alpha);

    // Sprite 默认透明混合
    SetSurfaceType(SurfaceType.Transparent);
    SetBlendMode(BlendMode.Alpha);

    m_Context.Save();
    Debug.Log("URP Sprite Lit 配置完成");
}

/// <summary> 配置为 URP Sprite Unlit ShaderGraph </summary>
public void SetupAsURPSpriteUnlit()
{
    EnsureContextsExist();
    ActivateUniversalTarget();
    SetActiveSubTargetByType("UnityEditor.Rendering.Universal.ShaderGraph.UniversalSpriteUnlitSubTarget");
    ClearAllBlocks();

    // 必需块（来自 CreateSpriteUnlitShaderGraph.cs）
    AddVertexBlock(VertexBlockType.Position);
    AddVertexBlock(VertexBlockType.Normal);
    AddVertexBlock(VertexBlockType.Tangent);
    AddSurfaceBlock(SurfaceBlockType.BaseColor);
    AddSurfaceBlock(SurfaceBlockType.Alpha);

    // Sprite 默认透明混合
    SetSurfaceType(SurfaceType.Transparent);
    SetBlendMode(BlendMode.Alpha);

    m_Context.Save();
    Debug.Log("URP Sprite Unlit 配置完成");
}

/// <summary> 配置为 URP Sprite Custom Lit ShaderGraph </summary>
public void SetupAsURPSpriteCustomLit()
{
    EnsureContextsExist();
    ActivateUniversalTarget();
    SetActiveSubTargetByType("UnityEditor.Rendering.Universal.ShaderGraph.UniversalSpriteCustomLitSubTarget");
    ClearAllBlocks();

    // 必需块（来自 CreateSpriteCustomLitShaderGraph.cs）
    AddVertexBlock(VertexBlockType.Position);
    AddVertexBlock(VertexBlockType.Normal);
    AddVertexBlock(VertexBlockType.Tangent);
    AddSurfaceBlock(SurfaceBlockType.BaseColor);
    AddUniversalSurfaceBlock("SpriteMask");
    AddSurfaceBlock(SurfaceBlockType.NormalTS);
    AddSurfaceBlock(SurfaceBlockType.Alpha);

    // Sprite 默认透明混合
    SetSurfaceType(SurfaceType.Transparent);
    SetBlendMode(BlendMode.Alpha);

    m_Context.Save();
    Debug.Log("URP Sprite Custom Lit 配置完成");
}

#endregion
        public bool SetUnlitSubTarget() => SetActiveSubTarget(UniversalUnlitSubTargetTypeName);
        public bool SetLitSubTarget() => SetActiveSubTarget(UniversalLitSubTargetTypeName);

        private object GetActiveSubTarget()
        {
            var universalTarget = GetActiveUniversalTarget();
            return GetPropertyValue(universalTarget, "activeSubTarget", BindingFlags.Public | BindingFlags.Instance);
        }

        #endregion

        #region 完整预设配置

/// <summary> 配置为 URP Fullscreen ShaderGraph </summary>
public void SetupAsURPFullscreen()
{
    EnsureContextsExist();
    ActivateUniversalTarget();
    SetActiveSubTargetByType("UnityEditor.Rendering.Universal.ShaderGraph.UniversalFullscreenSubTarget");
    ClearAllBlocks();

    AddSurfaceBlock(SurfaceBlockType.BaseColor);
    AddSurfaceBlock(SurfaceBlockType.Alpha);

    m_Context.Save();
}

/// <summary> 配置为 URP Canvas ShaderGraph </summary>
public void SetupAsURPCanvas()
{
    EnsureContextsExist();
    ActivateUniversalTarget();
    SetActiveSubTargetByType("UnityEditor.Rendering.Universal.ShaderGraph.UniversalCanvasSubTarget");
    ClearAllBlocks();

    AddSurfaceBlock(SurfaceBlockType.BaseColor);
    AddSurfaceBlock(SurfaceBlockType.Emission);
    AddSurfaceBlock(SurfaceBlockType.Alpha);
    AddSurfaceBlock(SurfaceBlockType.AlphaClipThreshold);

    // Canvas 特殊设置
    SetSurfaceType(SurfaceType.Transparent);
    SetBlendMode(BlendMode.Alpha);

    m_Context.Save();
}

/// <summary> 配置为 URP Six Way ShaderGraph </summary>
public void SetupAsURPSixWay()
{
    EnsureContextsExist();
    ActivateUniversalTarget();
    SetActiveSubTargetByType("UnityEditor.Rendering.Universal.ShaderGraph.UniversalSixWaySubTarget");
    ClearAllBlocks();

    AddVertexBlock(VertexBlockType.Position);
    AddVertexBlock(VertexBlockType.Normal);
    AddVertexBlock(VertexBlockType.Tangent);

    AddSurfaceBlock(SurfaceBlockType.BaseColor);
    AddSurfaceBlock(SurfaceBlockType.MapRightTopBack);
    AddSurfaceBlock(SurfaceBlockType.MapLeftBottomFront);
    AddSurfaceBlock(SurfaceBlockType.AbsorptionStrength);
    AddSurfaceBlock(SurfaceBlockType.Emission);
    AddSurfaceBlock(SurfaceBlockType.Occlusion);
    AddSurfaceBlock(SurfaceBlockType.Alpha);

    m_Context.Save();
}
public bool SetActiveSubTarget(Type subTargetType)
{
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("=== SetActiveSubTarget 调试信息 ===");
    sb.AppendLine($"目标 SubTarget 类型: {subTargetType?.FullName ?? "null"}");

    var universalTarget = GetActiveUniversalTarget();
    sb.AppendLine($"UniversalTarget 实例: {(universalTarget != null ? "存在" : "不存在")}");
    if (universalTarget == null)
    {
        Debug.LogError(sb.ToString());
        return false;
    }

    var trySetMethod = universalTarget.GetType().GetMethod("TrySetActiveSubTarget", new[] { typeof(Type) });
    sb.AppendLine($"TrySetActiveSubTarget 方法查找: {(trySetMethod != null ? "成功" : "失败")}");
    if (trySetMethod == null)
    {
        Debug.LogError(sb.ToString());
        return false;
    }

    try
    {
        var success = (bool)trySetMethod.Invoke(universalTarget, new object[] { subTargetType });
        sb.AppendLine($"调用结果: {(success ? "成功" : "失败")}");
        if (success)
        {
            m_Context.Save();
            sb.AppendLine("已保存 GraphData");
        }
        Debug.Log(sb.ToString());
        return success;
    }
    catch (Exception ex)
    {
        sb.AppendLine($"异常: {ex.Message}");
        Debug.LogError(sb.ToString());
        return false;
    }
}
/// <summary> 配置为 URP Decal ShaderGraph </summary>
public void SetupAsURPDecal()
{
    EnsureContextsExist();
    ActivateUniversalTarget();
    SetActiveSubTargetByType("UnityEditor.Rendering.Universal.ShaderGraph.UniversalDecalSubTarget");
    ClearAllBlocks();

    AddVertexBlock(VertexBlockType.Position);
    AddVertexBlock(VertexBlockType.Normal);
    AddVertexBlock(VertexBlockType.Tangent);

    AddSurfaceBlock(SurfaceBlockType.BaseColor);
    AddSurfaceBlock(SurfaceBlockType.Alpha);
    AddSurfaceBlock(SurfaceBlockType.NormalTS);
    AddUniversalSurfaceBlock("NormalAlpha");      // 使用 UniversalBlockFields 定义的块
    AddSurfaceBlock(SurfaceBlockType.Metallic);
    AddSurfaceBlock(SurfaceBlockType.Occlusion);
    AddSurfaceBlock(SurfaceBlockType.Smoothness);
    AddUniversalSurfaceBlock("MAOSAlpha");
    AddSurfaceBlock(SurfaceBlockType.Emission);

    SetCastShadows(true);

    m_Context.Save();
}
/// <summary> 设置是否投射阴影</summary>
public void SetCastShadows(bool castShadows)
{
    var target = GetActiveUniversalTarget();
    if (target == null) return;
    SetFieldValue(target, "m_CastShadows", castShadows, BindingFlags.NonPublic | BindingFlags.Instance);
    m_Context.Save();
}
/// <summary> 设置是否支持 LOD Cross Fade</summary>
public void SetSupportsLODCrossFade(bool enabled)
{
    var target = GetActiveUniversalTarget();
    if (target == null) return;
    SetFieldValue(target, "m_SupportsLODCrossFade", enabled, BindingFlags.NonPublic | BindingFlags.Instance);
    m_Context.Save();
}
/// <summary> 设置附加运动向量模式（None / TimeBased / Custom）</summary>
public void SetAdditionalMotionVectorMode(AdditionalMotionVectorMode mode)
{
    var target = GetActiveUniversalTarget();
    if (target == null) return;
    var obj=GetEnumValue("UnityEditor.Rendering.Universal.ShaderGraph.AdditionalMotionVectorMode", mode.ToString());
    SetFieldValue(target, "m_AdditionalMotionVectorMode", obj, BindingFlags.NonPublic | BindingFlags.Instance);
    m_Context.Save();
}
/// <summary> 设置是否启用 Alembic 运动向量</summary>
public void SetAlembicMotionVectors(bool enabled)
{
    var target = GetActiveUniversalTarget();
    if (target == null) return;
    SetFieldValue(target, "m_AlembicMotionVectors", enabled, BindingFlags.NonPublic | BindingFlags.Instance);
    m_Context.Save();
}
/// <summary> 设置自定义编辑器 GUI 类名</summary>
public void SetCustomEditorGUI(string editorGUIName)
{
    var target = GetActiveUniversalTarget();
    if (target == null) return;
    SetFieldValue(target, "m_CustomEditorGUI", editorGUIName, BindingFlags.NonPublic | BindingFlags.Instance);
    m_Context.Save();
}
/// <summary> 设置是否支持 VFX Graph</summary>
public void SetSupportVFX(bool support)
{
    var target = GetActiveUniversalTarget();
    if (target == null) return;
    SetFieldValue(target, "m_SupportVFX", support, BindingFlags.NonPublic | BindingFlags.Instance);
    m_Context.Save();
}
/// <summary> 通过类型名称激活 SubTarget（增加反射安全性）</summary>
private void SetActiveSubTargetByType(string typeName)
{
    var subTargetType = ShaderGraphReflectionHelper.FindType(typeName);
    if (subTargetType == null)
    {
        Debug.LogError($"找不到 SubTarget 类型: {typeName}");
        return;
    }
    SetActiveSubTarget(subTargetType);
}

/// <summary> 添加 Universal 专用的 Surface 块（如 NormalAlpha, MAOSAlpha）</summary>
private void AddUniversalSurfaceBlock(string blockName)
{
    AddBlock(ShaderGraphContextType.Fragment, $"SurfaceDescription.{blockName}", -1);
}

#endregion
        
        #region 块操作
        

        private object GetContextData(ShaderGraphContextType contextType)
        {
            string fieldName = contextType == ShaderGraphContextType.Vertex ? "m_VertexContext" : "m_FragmentContext";
            return GetFieldValue(m_GraphData, fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private System.Collections.IList GetBlockList(ShaderGraphContextType contextType)
        {
            var contextData = GetContextData(contextType);
            if (contextData == null) return null;
            return GetFieldValue(contextData, "m_Blocks", BindingFlags.NonPublic | BindingFlags.Instance) as System.Collections.IList;
        }

        public object GetBlockByDescriptor(ShaderGraphContextType contextType, string descriptorSerialized)
        {
            var blocks = GetBlockList(contextType);
            if (blocks == null) return null;

            foreach (var blockRef in blocks)
            {
                // 从 JsonRef<BlockNode> 中取出真正的 BlockNode
                var blockValue = GetPropertyValue(blockRef, "value", BindingFlags.Public | BindingFlags.Instance);
                if (blockValue == null) continue;

                var serializedDescriptor = GetFieldValue(blockValue, "m_SerializedDescriptor", BindingFlags.NonPublic | BindingFlags.Instance) as string;
                if (serializedDescriptor == descriptorSerialized)
                    return blockValue;
            }
            return null;
        }
        public void EnsureContextsExist()
        {
            if (GetContextData(ShaderGraphContextType.Vertex) == null)
            {
                CallMethod(m_GraphData, "AddContexts", BindingFlags.Public | BindingFlags.Instance, null, null);
            }
        }
        public object GetSurfaceBlock(SurfaceBlockType blockType)
            => GetBlockByDescriptor(ShaderGraphContextType.Fragment, $"SurfaceDescription.{blockType}");

        public object GetVertexBlock(VertexBlockType blockType)
            => GetBlockByDescriptor(ShaderGraphContextType.Vertex, $"VertexDescription.{blockType}");

        public List<object> GetAllBlocks(ShaderGraphContextType contextType)
        {
            var result = new List<object>();
            var blocks = GetBlockList(contextType);
            if (blocks == null) return result;
            foreach (var blockRef in blocks)
            {
                // JsonRef<T> 有一个 value 属性（getter 返回 T）
                var value = GetPropertyValue(blockRef, "value", BindingFlags.Public | BindingFlags.Instance);
                if (value != null)
                    result.Add(value);
            }
            return result;
        }
        #region Graph Settings 设置
        /// <summary> 设置是否允许材质覆写（Allow Material Override）</summary>
        public void SetAllowMaterialOverride(bool allow)
        {
            var target = GetActiveUniversalTarget();
            if (target == null) return;
            //GetEnumValue("UnityEditor.Rendering.Universal.ShaderGraph.AlphaMode",)
            SetFieldValue(target, "m_AllowMaterialOverride", allow, BindingFlags.NonPublic | BindingFlags.Instance);
            m_Context.Save();
        }
        
        #endregion
       /// <summary>
/// <summary>
/// 向指定上下文添加一个块节点（例如 "SurfaceDescription.BaseColor"）
/// </summary>
/// <param name="contextType">上下文类型（Vertex 或 Fragment）</param>
/// <param name="descriptorSerialized">描述符字符串，如 "SurfaceDescription.BaseColor"</param>
/// <param name="index">插入位置，-1 表示末尾</param>
/// <returns>添加的 BlockNode 实例，失败返回 null</returns>
public object AddBlock(ShaderGraphContextType contextType, string descriptorSerialized, int index = -1)
{
    // 1. 获取 GraphData 中的 m_BlockFieldDescriptors 列表
    var allDescriptors = GetFieldValue(m_GraphData, "m_BlockFieldDescriptors", BindingFlags.NonPublic | BindingFlags.Instance) as System.Collections.IList;
    if (allDescriptors == null)
    {
        Debug.LogError("[AddBlock] 无法获取 m_BlockFieldDescriptors");
        return null;
    }

    // 2. 查找匹配的 BlockFieldDescriptor
    object targetDescriptor = null;
    foreach (var desc in allDescriptors)
    {
        // tag 和 name 可能是字段或属性（不同 Unity 版本有差异）
        string tag = (GetPropertyValue(desc, "tag", BindingFlags.Public | BindingFlags.Instance)
                   ?? GetFieldValue(desc, "tag", BindingFlags.NonPublic | BindingFlags.Instance)) as string;
        string name = (GetPropertyValue(desc, "name", BindingFlags.Public | BindingFlags.Instance)
                    ?? GetFieldValue(desc, "name", BindingFlags.NonPublic | BindingFlags.Instance)) as string;
        if ($"{tag}.{name}" == descriptorSerialized)
        {
            targetDescriptor = desc;
            break;
        }
    }
    if (targetDescriptor == null)
    {
        Debug.LogError($"[AddBlock] 找不到 BlockFieldDescriptor: {descriptorSerialized}");
        return null;
    }

    // 3. 创建 BlockNode 实例并调用 Init
    var blockNodeType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.BlockNode");
    if (blockNodeType == null)
    {
        Debug.LogError("[AddBlock] 找不到 BlockNode 类型");
        return null;
    }
    var blockNode = Activator.CreateInstance(blockNodeType);
    var initMethod = blockNodeType.GetMethod("Init", BindingFlags.Public | BindingFlags.Instance);
    initMethod?.Invoke(blockNode, new[] { targetDescriptor });

    // 4. 强制调用 OnBeforeSerialize，确保 m_SerializedDescriptor 被填充
    var onBeforeSerialize = blockNodeType.GetMethod("OnBeforeSerialize", BindingFlags.Public | BindingFlags.Instance);
    onBeforeSerialize?.Invoke(blockNode, null);

    // 如果仍未设置，手动写入字段（兜底）
    var currentDesc = GetFieldValue(blockNode, "m_SerializedDescriptor", BindingFlags.NonPublic | BindingFlags.Instance) as string;
    if (string.IsNullOrEmpty(currentDesc))
    {
        SetFieldValue(blockNode, "m_SerializedDescriptor", descriptorSerialized, BindingFlags.NonPublic | BindingFlags.Instance);
        Debug.Log($"[AddBlock] 手动设置 m_SerializedDescriptor = {descriptorSerialized}");
    }

    // 5. 获取上下文数据对象
    var contextData = GetContextData(contextType);
    if (contextData == null)
    {
        Debug.LogError($"[AddBlock] 无法获取上下文数据: {contextType}");
        return null;
    }

    // 6. 调用 GraphData.AddBlock 公共方法（这是最可靠的方式）
    var addBlockMethod = m_GraphData.GetType().GetMethod("AddBlock",
        BindingFlags.Public | BindingFlags.Instance,
        null,
        new[] { blockNodeType, contextData.GetType(), typeof(int) },
        null);
    if (addBlockMethod != null)
    {
        addBlockMethod.Invoke(m_GraphData, new[] { blockNode, contextData, index });
       // m_Context.Save();
      
        return blockNode;
    }

    // 7. 降级方案（如果公共方法不存在，例如极旧版本）
    Debug.LogWarning("[AddBlock] GraphData.AddBlock 方法不存在，使用降级方案手动添加。");
    ShaderGraphReflectionHelper.SetNodeId(blockNode);
    SetPropertyValue(blockNode, "owner", m_GraphData, BindingFlags.Public | BindingFlags.Instance);
    SetPropertyValue(blockNode, "contextData", contextData, BindingFlags.Public | BindingFlags.Instance);
    ShaderGraphReflectionHelper.AddNodeToGraph(m_GraphData, blockNode);

    // 包装为 JsonRef<BlockNode>
    var jsonRefType = ShaderGraphReflectionHelper.FindGenericType(
        "UnityEditor.ShaderGraph.Serialization.JsonRef`1",
        blockNodeType);
    if (jsonRefType == null)
    {
        Debug.LogError("[AddBlock] 找不到 JsonRef<T> 类型");
        return null;
    }
    object jsonRef;
    var implicitOp = jsonRefType.GetMethod("op_Implicit", new[] { blockNodeType });
    if (implicitOp != null)
    {
        jsonRef = implicitOp.Invoke(null, new[] { blockNode });
    }
    else
    {
        jsonRef = Activator.CreateInstance(jsonRefType);
        SetFieldValue(jsonRef, "m_Value", blockNode, BindingFlags.NonPublic | BindingFlags.Instance);
    }

    var blocks = GetBlockList(contextType);
    if (blocks == null)
    {
        Debug.LogError("[AddBlock] 无法获取上下文块列表");
        return null;
    }
    if (index < 0 || index >= blocks.Count)
        blocks.Add(jsonRef);
    else
        blocks.Insert(index, jsonRef);

    ShaderGraphReflectionHelper.ValidateGraph(m_GraphData);
   // m_Context.Save();
    Debug.Log($"[AddBlock] 手动添加块完成: {descriptorSerialized}");
    return blockNode;
}
        public object AddSurfaceBlock(SurfaceBlockType blockType, int index = -1)
            => AddBlock(ShaderGraphContextType.Fragment, $"SurfaceDescription.{blockType}", index);

        public object AddVertexBlock(VertexBlockType blockType, int index = -1)
            => AddBlock(ShaderGraphContextType.Vertex, $"VertexDescription.{blockType}", index);

        public bool RemoveBlock(object blockNode)
        {
            var actualBlock = ShaderGraphReflectionHelper.UnwrapJsonData(blockNode);
            if (actualBlock == null) return false;

            var contextData = GetPropertyValue(actualBlock, "contextData", BindingFlags.Public | BindingFlags.Instance);
            if (contextData == null) return false;

            var vertexContext = GetContextData(ShaderGraphContextType.Vertex);
            ShaderGraphContextType contextType = (contextData == vertexContext) ? ShaderGraphContextType.Vertex : ShaderGraphContextType.Fragment;

            var blocks = GetBlockList(contextType);
            if (blocks == null) return false;

            object toRemove = null;
            foreach (var item in blocks)
            {
                var val = ShaderGraphReflectionHelper.UnwrapJsonData(item);
                if (val == actualBlock) { toRemove = item; break; }
            }
            if (toRemove != null)
            {
                blocks.Remove(toRemove);
                CallMethod(m_GraphData, "RemoveNode", BindingFlags.Public | BindingFlags.Instance, new[] { actualBlock.GetType() }, new[] { actualBlock });
              //  m_Context.Save();
                return true;
            }
            return false;
        }

        #endregion
        
        
        
        
        
        
        /// <summary>
        /// 获取当前图中所有块的调试信息（包括描述符）
        /// </summary>
        public string GetBlocksDebugInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== 当前图中的块节点 ===");

            var vertexBlocks = GetAllBlocks(ShaderGraphContextType.Vertex);
            sb.AppendLine($"[Vertex Context] 块数量: {vertexBlocks.Count}");
            foreach (var block in vertexBlocks)
            {
                var desc = GetBlockDescriptorString(block);
                sb.AppendLine($"  - {desc}");
            }

            var fragmentBlocks = GetAllBlocks(ShaderGraphContextType.Fragment);
            sb.AppendLine($"[Fragment Context] 块数量: {fragmentBlocks.Count}");
            foreach (var block in fragmentBlocks)
            {
                var desc = GetBlockDescriptorString(block);
                sb.AppendLine($"  - {desc}");
            }

            return sb.ToString();
        }

        private string GetBlockDescriptorString(object block)
        {
            if (block == null) return "null";

            // 1. 优先读取 m_SerializedDescriptor
            var serialized = GetFieldValue(block, "m_SerializedDescriptor", BindingFlags.NonPublic | BindingFlags.Instance) as string;
            if (!string.IsNullOrEmpty(serialized))
                return serialized;

            // 2. 尝试通过 descriptor 属性获取
            var descriptor = GetPropertyValue(block, "descriptor", BindingFlags.Public | BindingFlags.Instance);
            if (descriptor != null)
            {
                string tag = (GetFieldValue(descriptor, "tag", BindingFlags.NonPublic | BindingFlags.Instance)
                              ?? GetPropertyValue(descriptor, "tag", BindingFlags.Public | BindingFlags.Instance)) as string;
                string name = (GetFieldValue(descriptor, "name", BindingFlags.NonPublic | BindingFlags.Instance)
                               ?? GetPropertyValue(descriptor, "name", BindingFlags.Public | BindingFlags.Instance)) as string;
                if (!string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(name))
                    return $"{tag}.{name}";
            }

            // 3. 如果是 JsonRef，显示其内部类型
            if (block.GetType().Name.Contains("JsonRef"))
            {
                var inner = GetPropertyValue(block, "value", BindingFlags.Public | BindingFlags.Instance);
                if (inner != null)
                    return $"JsonRef<{inner.GetType().Name}>";
            }

            return $"Unknown({block.GetType().Name})";
        }
        
        /// <summary>
        /// 将源节点的输出插槽连接到指定上下文的指定块节点
        /// </summary>
        /// <param name="sourceNode">源节点实例</param>
        /// <param name="sourceOutputSlotId">源节点输出插槽 ID（默认为 0）</param>
        /// <param name="contextType">目标上下文类型</param>
        /// <param name="blockType">块类型枚举（VertexBlockType 或 SurfaceBlockType）</param>
        /// <param name="blockInputSlotId">块节点的输入插槽 ID（通常为 0）</param>
        /// <returns>是否连接成功</returns>
        public bool ConnectToBlock(object sourceNode, int sourceOutputSlotId, ShaderGraphContextType contextType, Enum blockType, int blockInputSlotId = 0)
        {
            
            // 根据枚举类型构造描述符字符串
            string descriptor;
            if (blockType is VertexBlockType vbt)
                descriptor = $"VertexDescription.{vbt}";
            else if (blockType is SurfaceBlockType sbt)
                descriptor = $"SurfaceDescription.{sbt}";
            else
            {
                Debug.LogError($"不支持的块类型: {blockType.GetType()}");
                return false;
            }

            // 获取目标块节点
            var targetBlock = GetBlockByDescriptor(contextType, descriptor);
            if (targetBlock == null)
            {
                Debug.LogError($"找不到目标块: {descriptor} 在 {contextType} 上下文中");
                return false;
            }

            // 调用底层 ConnectSlots
            return m_Context.ConnectSlots(sourceNode, sourceOutputSlotId, targetBlock, blockInputSlotId);
        }

        /// <summary>
        /// 将源节点的输出插槽（按名称）连接到指定上下文的指定块节点
        /// </summary>
        public bool ConnectToBlock(object sourceNode, string sourceOutputSlotName, ShaderGraphContextType contextType, Enum blockType, int blockInputSlotId = 0)
        {
            var actualNode = ShaderGraphReflectionHelper.UnwrapJsonData(sourceNode);
            var outputSlot = ShaderGraphReflectionHelper.FindMaterialSlot(actualNode, sourceOutputSlotName, isOutput: true);
            if (outputSlot == null)
            {
                Debug.LogError($"源节点没有名为 {sourceOutputSlotName} 的输出插槽");
                return false;
            }
            int outputSlotId = ShaderGraphReflectionHelper.GetSlotId(outputSlot);
            return ConnectToBlock(sourceNode, outputSlotId, contextType, blockType, blockInputSlotId);
        }
        #region Graph Settings
        private static object GetEnumValue(string typeName, string enumName)
        {
            var enumType = ShaderGraphReflectionHelper.FindType(typeName);
            if (enumType == null) return 0;
            return Enum.Parse(enumType, enumName);
        }
        /// <summary> 设置表面类型（Opaque / Transparent）</summary>
        public void SetSurfaceType(SurfaceType surfaceType)
        {
            var target = GetActiveUniversalTarget();
            if (target == null) return;
           var  obj= GetEnumValue("UnityEditor.Rendering.Universal.ShaderGraph.SurfaceType", surfaceType.ToString());
            SetFieldValue(target, "m_SurfaceType", obj, BindingFlags.NonPublic | BindingFlags.Instance);
            //m_Context.Save();
        }

        /// <summary> 设置混合模式（Alpha / Premultiply / Additive / Multiply）</summary>
        public void SetBlendMode(BlendMode blendMode)
        {
            var target = GetActiveUniversalTarget();
            if (target == null) return;
            var obj= GetEnumValue("UnityEditor.Rendering.Universal.ShaderGraph.AlphaMode", blendMode.ToString());
            SetFieldValue(target, "m_AlphaMode", obj, BindingFlags.NonPublic | BindingFlags.Instance);
            //m_Context.Save();
        }
        /// <summary> 设置渲染面（Front / Back / Both）</summary>
        public void SetRenderFace(RenderFace renderFace)
        {
            var target = GetActiveUniversalTarget();
            if (target == null) return;
            var obj= GetEnumValue("UnityEditor.Rendering.Universal.ShaderGraph.RenderFace", renderFace.ToString());
            SetFieldValue(target, "m_RenderFace", obj, BindingFlags.NonPublic | BindingFlags.Instance);
           // m_Context.Save();
        }
        /// <summary> 设置深度写入模式（Auto / ForceEnabled / ForceDisabled）</summary>
        public void SetZWriteControl(ZWriteControl zWriteControl)
        {
            var target = GetActiveUniversalTarget();
            if (target == null) return;
            var obj= GetEnumValue("UnityEditor.Rendering.Universal.ShaderGraph.ZWriteControl", zWriteControl.ToString());
            
            SetFieldValue(target, "m_ZWriteControl", obj, BindingFlags.NonPublic | BindingFlags.Instance);
            //m_Context.Save();
        }
        /// <summary> 设置深度测试模式（LEqual / Less / Greater / Always 等）</summary>
        public void SetZTestMode(ZTestMode zTestMode)
        {
            var target = GetActiveUniversalTarget();
            if (target == null) return;
            
            var obj= GetEnumValue("UnityEditor.Rendering.Universal.ShaderGraph.ZTestMode", zTestMode.ToString());

            SetFieldValue(target, "m_ZTestMode", obj, BindingFlags.NonPublic | BindingFlags.Instance);
            //m_Context.Save();
        }
        
        /// <summary> 设置是否启用 Alpha Clipping（裁剪）</summary>
        public void SetAlphaClip(bool enabled)
        {
            var target = GetActiveUniversalTarget();
            if (target == null) return;
            SetFieldValue(target, "m_AlphaClip", enabled, BindingFlags.NonPublic | BindingFlags.Instance);
            //m_Context.Save();
        }
        public void SetTwoSided(bool twoSided)
        {
            var subTarget = GetActiveSubTarget();
            if (subTarget == null) return;

            var type = subTarget.GetType();
            var prop = type.GetProperty("twoSided", BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
                prop.SetValue(subTarget, twoSided);
            else
                SetFieldValue(subTarget, "m_TwoSided", twoSided, BindingFlags.NonPublic | BindingFlags.Instance);
            //m_Context.Save();
        }

        public void SetGraphDefaultPrecision(GraphPrecision precision)
        {
            CallMethod(m_GraphData, "SetGraphDefaultPrecision", BindingFlags.Public | BindingFlags.Instance, new[] { typeof(GraphPrecision) }, new object[] { precision });
           // m_Context.Save();
        }

        #endregion

        #region 预设配置
        public void ClearAllBlocks()
{
    // 1. 清理所有与 BlockNode 相关的边
    var edgesField = m_GraphData.GetType().GetField("m_Edges", BindingFlags.NonPublic | BindingFlags.Instance);
    var edgesList = edgesField?.GetValue(m_GraphData) as System.Collections.IList;
    if (edgesList != null)
    {
        for (int i = edgesList.Count - 1; i >= 0; i--)
        {
            var edge = edgesList[i];
            // 使用反射获取边的 inputSlot 和 outputSlot
            var inputSlotProp = edge.GetType().GetProperty("inputSlot", BindingFlags.Public | BindingFlags.Instance);
            var outputSlotProp = edge.GetType().GetProperty("outputSlot", BindingFlags.Public | BindingFlags.Instance);
            if (inputSlotProp == null || outputSlotProp == null) continue;

            var inputSlot = inputSlotProp.GetValue(edge);
            var outputSlot = outputSlotProp.GetValue(edge);

            // 获取节点
            var inputNode = GetPropertyValue(inputSlot, "node", BindingFlags.Public | BindingFlags.Instance);
            var outputNode = GetPropertyValue(outputSlot, "node", BindingFlags.Public | BindingFlags.Instance);

            // 如果任一节点为 null 或是 BlockNode，则移除此边
            if (inputNode == null || outputNode == null ||
                inputNode.GetType().Name == "BlockNode" ||
                outputNode.GetType().Name == "BlockNode")
            {
                edgesList.RemoveAt(i);
            }
        }
    }

    // 2. 清空上下文中的块列表
    var vertexBlocks = GetBlockList(ShaderGraphContextType.Vertex);
    var fragmentBlocks = GetBlockList(ShaderGraphContextType.Fragment);
    vertexBlocks?.Clear();
    fragmentBlocks?.Clear();

    // 3. 从 m_Nodes 中移除所有 BlockNode
    var nodesField = m_GraphData.GetType().GetField("m_Nodes", BindingFlags.NonPublic | BindingFlags.Instance);
    var nodesList = nodesField?.GetValue(m_GraphData) as System.Collections.IList;
    if (nodesList != null)
    {
        for (int i = nodesList.Count - 1; i >= 0; i--)
        {
            var node = ShaderGraphReflectionHelper.UnwrapJsonData(nodesList[i]);
            if (node != null && node.GetType().Name == "BlockNode")
                nodesList.RemoveAt(i);
        }
    }

    // 4. 保存图（此时图状态干净，不会触发空引用）
    m_Context.Save();
}
        public void SetupAsURPUnlit()
        {
            EnsureContextsExist();
            ActivateUniversalTarget();
            SetUnlitSubTarget();

            // 清空旧块，避免残留干扰
            ClearAllBlocks();

            // 添加 Unlit 必需的块
            AddVertexBlock(VertexBlockType.Position);
            AddVertexBlock(VertexBlockType.Normal);
            AddVertexBlock(VertexBlockType.Tangent);
            AddSurfaceBlock(SurfaceBlockType.BaseColor);

            m_Context.Save();
            Debug.Log("URP Unlit 配置完成，块信息:\n" + GetBlocksDebugInfo());
        }

        public void SetupAsURPLit()
        {
            EnsureContextsExist();
            ActivateUniversalTarget();
            SetLitSubTarget();

            if (GetVertexBlock(VertexBlockType.Position) == null) AddVertexBlock(VertexBlockType.Position);
            if (GetVertexBlock(VertexBlockType.Normal) == null) AddVertexBlock(VertexBlockType.Normal);
            if (GetVertexBlock(VertexBlockType.Tangent) == null) AddVertexBlock(VertexBlockType.Tangent);

            if (GetSurfaceBlock(SurfaceBlockType.BaseColor) == null) AddSurfaceBlock(SurfaceBlockType.BaseColor);
            if (GetSurfaceBlock(SurfaceBlockType.NormalTS) == null) AddSurfaceBlock(SurfaceBlockType.NormalTS);
            if (GetSurfaceBlock(SurfaceBlockType.Metallic) == null) AddSurfaceBlock(SurfaceBlockType.Metallic);
            if (GetSurfaceBlock(SurfaceBlockType.Smoothness) == null) AddSurfaceBlock(SurfaceBlockType.Smoothness);
            if (GetSurfaceBlock(SurfaceBlockType.Emission) == null) AddSurfaceBlock(SurfaceBlockType.Emission);
            if (GetSurfaceBlock(SurfaceBlockType.Occlusion) == null) AddSurfaceBlock(SurfaceBlockType.Occlusion);

           // m_Context.Save();
        }

        private void RemoveUnusedBlocksForUnlit()
        {
            string[] blocksToRemove = {
                "SurfaceDescription.NormalTS", "SurfaceDescription.NormalOS", "SurfaceDescription.NormalWS",
                "SurfaceDescription.Metallic", "SurfaceDescription.Specular", "SurfaceDescription.Smoothness",
                "SurfaceDescription.Occlusion", "SurfaceDescription.Emission", "SurfaceDescription.AlphaClipThreshold",
                "SurfaceDescription.CoatMask", "SurfaceDescription.CoatSmoothness"
            };

            foreach (var desc in blocksToRemove)
            {
                var block = GetBlockByDescriptor(ShaderGraphContextType.Fragment, desc);
                if (block != null) RemoveBlock(block);
            }
        }

        #endregion
    }
}