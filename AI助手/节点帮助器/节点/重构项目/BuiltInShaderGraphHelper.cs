using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEditor;
using 龙哥的秘密花园.节点库;

namespace 龙哥的秘密花园.节点库
{
    /// <summary>
    /// BuiltIn 管线 ShaderGraph 专用辅助类
    /// </summary>
    public class BuiltInShaderGraphHelper
    {
        private readonly GraphDataContext m_Context;
        private object m_GraphData;

        private const string BuiltInTargetTypeName = "UnityEditor.Rendering.BuiltIn.ShaderGraph.BuiltInTarget";
        private const string BuiltInLitSubTargetTypeName = "UnityEditor.Rendering.BuiltIn.ShaderGraph.BuiltInLitSubTarget";
        private const string BuiltInUnlitSubTargetTypeName = "UnityEditor.Rendering.BuiltIn.ShaderGraph.BuiltInUnlitSubTarget";
        private const string BuiltInCanvasSubTargetTypeName = "UnityEditor.Rendering.BuiltIn.ShaderGraph.BuiltInCanvasSubTarget";

        public BuiltInShaderGraphHelper(GraphDataContext context)
        {
            m_Context = context ?? throw new ArgumentNullException(nameof(context));
            m_GraphData = context.GraphData;
        }

        public void Refresh() => m_GraphData = m_Context.GraphData;

        #region 辅助反射方法

        private object GetFieldValue(object obj, string fieldName, BindingFlags flags)
        {
            if (obj == null) return null;
            var field = obj.GetType().GetField(fieldName, flags);
            return field?.GetValue(obj);
        }

        private void SetFieldValue(object obj, string fieldName, object value, BindingFlags flags)
        {
            if (obj == null) return;
            var field = obj.GetType().GetField(fieldName, flags);
            field?.SetValue(obj, value);
        }

        private object GetPropertyValue(object obj, string propName, BindingFlags flags)
        {
            if (obj == null) return null;
            var prop = obj.GetType().GetProperty(propName, flags);
            return prop?.GetValue(obj);
        }

        private void SetPropertyValue(object obj, string propName, object value, BindingFlags flags)
        {
            if (obj == null) return;
            var prop = obj.GetType().GetProperty(propName, flags);
            if (prop != null && prop.CanWrite)
                prop.SetValue(obj, value);
        }

        private object CallMethod(object obj, string methodName, BindingFlags flags, Type[] paramTypes, object[] args)
        {
            if (obj == null) return null;
            MethodInfo method;
            if (paramTypes == null || paramTypes.Length == 0)
                method = obj.GetType().GetMethod(methodName, flags);
            else
                method = obj.GetType().GetMethod(methodName, flags, null, paramTypes, null);
            return method?.Invoke(obj, args);
        }

        #endregion

        #region Target 操作

        private void EnsurePotentialTargets()
        {
            var potentialTargets = GetFieldValue(m_GraphData, "m_AllPotentialTargets", BindingFlags.NonPublic | BindingFlags.Instance) as IList;
            if (potentialTargets == null || potentialTargets.Count == 0)
            {
                var addKnownMethod = m_GraphData.GetType().GetMethod("AddKnownTargetsToPotentialTargets", BindingFlags.NonPublic | BindingFlags.Instance);
                addKnownMethod?.Invoke(m_GraphData, null);
            }
        }

        private object GetActiveBuiltInTarget()
        {
            var targetType = ShaderGraphReflectionHelper.FindType(BuiltInTargetTypeName);
            if (targetType == null) return null;

            var activeTargets = GetFieldValue(m_GraphData, "m_ActiveTargets", BindingFlags.NonPublic | BindingFlags.Instance) as IList;
            if (activeTargets != null)
            {
                foreach (var item in activeTargets)
                {
                    var target = ShaderGraphReflectionHelper.UnwrapJsonData(item);
                    if (target != null && target.GetType() == targetType)
                        return target;
                }
            }
            return null;
        }

        public bool ActivateBuiltInTarget()
        {
            EnsurePotentialTargets();

            var targetType = ShaderGraphReflectionHelper.FindType(BuiltInTargetTypeName);
            if (targetType == null)
            {
                Debug.LogError($"找不到 BuiltInTarget 类型: {BuiltInTargetTypeName}");
                return false;
            }

            if (GetActiveBuiltInTarget() != null) return true;

            var allPotentialTargets = GetFieldValue(m_GraphData, "m_AllPotentialTargets", BindingFlags.NonPublic | BindingFlags.Instance) as IList;
            if (allPotentialTargets == null || allPotentialTargets.Count == 0)
            {
                Debug.LogError("m_AllPotentialTargets 为空");
                return false;
            }

            object foundTarget = null;
            foreach (var pt in allPotentialTargets)
            {
                var getTargetMethod = pt.GetType().GetMethod("GetTarget");
                var target = getTargetMethod?.Invoke(pt, null);
                if (target != null && target.GetType() == targetType)
                {
                    foundTarget = target;
                    break;
                }
            }

            if (foundTarget == null)
            {
                Debug.LogError("未找到 BuiltInTarget 实例");
                return false;
            }

            var targetBaseType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Target");
            if (targetBaseType == null)
            {
                Debug.LogError("找不到 Target 基类类型");
                return false;
            }

            var setActiveMethod = m_GraphData.GetType().GetMethod("SetTargetActive", new[] { targetBaseType, typeof(bool) });
            if (setActiveMethod == null)
            {
                Debug.LogError("找不到 SetTargetActive 方法");
                return false;
            }

            setActiveMethod.Invoke(m_GraphData, new object[] { foundTarget, false });

            var sortMethod = m_GraphData.GetType().GetMethod("SortActiveTargets", Type.EmptyTypes);
            sortMethod?.Invoke(m_GraphData, null);
            var validateMethod = m_GraphData.GetType().GetMethod("ValidateGraph", Type.EmptyTypes);
            validateMethod?.Invoke(m_GraphData, null);

            m_Context.Save();
            return true;
        }

        public bool SetActiveSubTarget(Type subTargetType)
        {
            var builtInTarget = GetActiveBuiltInTarget();
            if (builtInTarget == null) return false;

            var trySetMethod = builtInTarget.GetType().GetMethod("TrySetActiveSubTarget", new[] { typeof(Type) });
            if (trySetMethod == null) return false;

            var success = (bool)trySetMethod.Invoke(builtInTarget, new object[] { subTargetType });
            if (success) m_Context.Save();
            return success;
        }

        #endregion

        #region 块操作

        private object GetContextData(string contextName)
        {
            return GetFieldValue(m_GraphData, contextName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private IList GetBlockList(string contextName)
        {
            var contextData = GetContextData(contextName);
            if (contextData == null) return null;
            return GetFieldValue(contextData, "m_Blocks", BindingFlags.NonPublic | BindingFlags.Instance) as IList;
        }

        public object AddBlock(string contextTypeName, string descriptorSerialized, int index = -1)
        {
            // 获取 GraphData 中的 m_BlockFieldDescriptors
            var allDescriptors = GetFieldValue(m_GraphData, "m_BlockFieldDescriptors", BindingFlags.NonPublic | BindingFlags.Instance) as IList;
            if (allDescriptors == null) return null;

            object targetDescriptor = null;
            foreach (var desc in allDescriptors)
            {
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
                Debug.LogError($"找不到 BlockFieldDescriptor: {descriptorSerialized}");
                return null;
            }

            var blockNodeType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.BlockNode");
            if (blockNodeType == null) return null;

            var blockNode = Activator.CreateInstance(blockNodeType);
            var initMethod = blockNodeType.GetMethod("Init", new[] { targetDescriptor.GetType() });
            initMethod?.Invoke(blockNode, new[] { targetDescriptor });

            var onBeforeSerialize = blockNodeType.GetMethod("OnBeforeSerialize", Type.EmptyTypes);
            onBeforeSerialize?.Invoke(blockNode, null);

            // 设置 m_SerializedDescriptor
            var serializedField = blockNodeType.GetField("m_SerializedDescriptor", BindingFlags.NonPublic | BindingFlags.Instance);
            if (serializedField != null && string.IsNullOrEmpty(serializedField.GetValue(blockNode) as string))
                serializedField.SetValue(blockNode, descriptorSerialized);

            // 获取上下文数据
            var contextData = GetContextData(contextTypeName);
            if (contextData == null) return null;

            // 调用 GraphData.AddBlock
            var addBlockMethod = m_GraphData.GetType().GetMethod("AddBlock", new[] { blockNodeType, contextData.GetType(), typeof(int) });
            if (addBlockMethod != null)
            {
                addBlockMethod.Invoke(m_GraphData, new object[] { blockNode, contextData, index });
                m_Context.Save();
                return blockNode;
            }

            // 降级方案
            ShaderGraphReflectionHelper.SetNodeId(blockNode);
            SetPropertyValue(blockNode, "owner", m_GraphData, BindingFlags.Public | BindingFlags.Instance);
            SetPropertyValue(blockNode, "contextData", contextData, BindingFlags.Public | BindingFlags.Instance);
            ShaderGraphReflectionHelper.AddNodeToGraph(m_GraphData, blockNode);

            var jsonRefType = ShaderGraphReflectionHelper.FindGenericType("UnityEditor.ShaderGraph.Serialization.JsonRef`1", blockNodeType);
            if (jsonRefType == null) return null;

            object jsonRef;
            var implicitOp = jsonRefType.GetMethod("op_Implicit", new[] { blockNodeType });
            if (implicitOp != null)
                jsonRef = implicitOp.Invoke(null, new[] { blockNode });
            else
            {
                jsonRef = Activator.CreateInstance(jsonRefType);
                SetFieldValue(jsonRef, "m_Value", blockNode, BindingFlags.NonPublic | BindingFlags.Instance);
            }

            var blocks = GetBlockList(contextTypeName);
            if (blocks == null) return null;
            if (index < 0 || index >= blocks.Count)
                blocks.Add(jsonRef);
            else
                blocks.Insert(index, jsonRef);

            ShaderGraphReflectionHelper.ValidateGraph(m_GraphData);
            m_Context.Save();
            return blockNode;
        }

        public object AddSurfaceBlock(string blockName, int index = -1)
        {
            return AddBlock("m_FragmentContext", $"SurfaceDescription.{blockName}", index);
        }

        public object AddVertexBlock(string blockName, int index = -1)
        {
            return AddBlock("m_VertexContext", $"VertexDescription.{blockName}", index);
        }

        public void ClearAllBlocks()
        {
            var vertexBlocks = GetBlockList("m_VertexContext");
            var fragmentBlocks = GetBlockList("m_FragmentContext");
            vertexBlocks?.Clear();
            fragmentBlocks?.Clear();

            var nodesField = m_GraphData.GetType().GetField("m_Nodes", BindingFlags.NonPublic | BindingFlags.Instance);
            var nodesList = nodesField?.GetValue(m_GraphData) as IList;
            if (nodesList != null)
            {
                for (int i = nodesList.Count - 1; i >= 0; i--)
                {
                    var node = ShaderGraphReflectionHelper.UnwrapJsonData(nodesList[i]);
                    if (node != null && node.GetType().Name == "BlockNode")
                        nodesList.RemoveAt(i);
                }
            }
            m_Context.Save();
        }

        public void EnsureContextsExist()
        {
            if (GetContextData("m_VertexContext") == null)
            {
                var addContexts = m_GraphData.GetType().GetMethod("AddContexts", Type.EmptyTypes);
                addContexts?.Invoke(m_GraphData, null);
            }
        }

        #endregion

        #region 预设配置

        public void SetupAsBuiltInLit()
        {
            EnsureContextsExist();
            ActivateBuiltInTarget();
            SetActiveSubTarget(ShaderGraphReflectionHelper.FindType(BuiltInLitSubTargetTypeName));
            ClearAllBlocks();

            AddVertexBlock("Position");
            AddVertexBlock("Normal");
            AddVertexBlock("Tangent");
            AddSurfaceBlock("BaseColor");
            AddSurfaceBlock("NormalTS");
            AddSurfaceBlock("Metallic");
            AddSurfaceBlock("Smoothness");
            AddSurfaceBlock("Emission");
            AddSurfaceBlock("Occlusion");

            m_Context.Save();
        }

        public void SetupAsBuiltInUnlit()
        {
            EnsureContextsExist();
            ActivateBuiltInTarget();
            SetActiveSubTarget(ShaderGraphReflectionHelper.FindType(BuiltInUnlitSubTargetTypeName));
            ClearAllBlocks();

            AddVertexBlock("Position");
            AddVertexBlock("Normal");
            AddVertexBlock("Tangent");
            AddSurfaceBlock("BaseColor");

            m_Context.Save();
        }

        public void SetupAsBuiltInCanvas()
        {
            EnsureContextsExist();
            ActivateBuiltInTarget();
            SetActiveSubTarget(ShaderGraphReflectionHelper.FindType(BuiltInCanvasSubTargetTypeName));
            ClearAllBlocks();

            AddSurfaceBlock("BaseColor");
            AddSurfaceBlock("Emission");
            AddSurfaceBlock("Alpha");
            AddSurfaceBlock("AlphaClipThreshold");

            // Canvas 默认透明
            SetSurfaceType(SurfaceType.Transparent);
            SetBlendMode(BlendMode.Alpha);

            m_Context.Save();
        }

        #endregion

        #region 材质设置

        private void SetSurfaceType(SurfaceType surfaceType)
        {
            var target = GetActiveBuiltInTarget();
            if (target == null) return;
            SetFieldValue(target, "m_SurfaceType", surfaceType, BindingFlags.NonPublic | BindingFlags.Instance);
            m_Context.Save();
        }

        private void SetBlendMode(BlendMode blendMode)
        {
            var target = GetActiveBuiltInTarget();
            if (target == null) return;
            SetFieldValue(target, "m_AlphaMode", blendMode, BindingFlags.NonPublic | BindingFlags.Instance);
            m_Context.Save();
        }

        #endregion
    }
}