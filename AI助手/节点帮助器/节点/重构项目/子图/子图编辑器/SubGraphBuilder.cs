using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using 龙哥的秘密花园.节点库;

namespace 龙哥的秘密花园.Editor
{
    /// <summary>
    /// 子图构建工具：将现有 ShaderGraph 转换为子图，或从 GraphData 直接保存。
    /// </summary>
    public static class SubGraphBuilder
    {
        /// <summary>
        /// 将指定路径的 .shadergraph 资产转换为 .shadersubgraph 子图资产。
        /// </summary>
        /// <param name="sourceShaderGraphPath">源 .shadergraph 资产路径</param>
        /// <param name="outputSubGraphPath">输出 .shadersubgraph 路径</param>
        public static void ConvertShaderGraphToSubGraph(string sourceShaderGraphPath, string outputSubGraphPath)
        {
            // 加载源 GraphData
            var graphData = ShaderGraphReflectionHelper.LoadGraphData(sourceShaderGraphPath, out string error);
            if (graphData == null)
                throw new Exception($"加载源 ShaderGraph 失败: {error}");

            // 转换为子图
            ConvertGraphDataToSubGraph(graphData);

            // 保存
            if (!ShaderGraphReflectionHelper.SaveGraphDataToFile(graphData, outputSubGraphPath, out error))
                throw new Exception($"保存子图失败: {error}");

            AssetDatabase.Refresh();
            Debug.Log($"子图已生成: {outputSubGraphPath}");
        }

        /// <summary>
        /// 将内存中的 GraphData 对象转换为子图模式（修改 isSubGraph 并替换输出节点）。
        /// </summary>
        public static void ConvertGraphDataToSubGraph(object graphData)
        {
            if (graphData == null) throw new ArgumentNullException(nameof(graphData));

            var graphDataType = graphData.GetType();

            // 1. 设置为子图模式
            var isSubGraphField = graphDataType.GetField("isSubGraph", BindingFlags.Public | BindingFlags.Instance);
            if (isSubGraphField == null)
                throw new Exception("GraphData 中找不到 isSubGraph 字段");
            isSubGraphField.SetValue(graphData, true);

            // 2. 移除现有的输出节点（MasterNode 或已有的 SubGraphOutputNode）
            var outputNodeField = graphDataType.GetField("m_OutputNode", BindingFlags.NonPublic | BindingFlags.Instance);
            if (outputNodeField == null)
                throw new Exception("GraphData 中找不到 m_OutputNode 字段");

            var oldOutputNode = outputNodeField.GetValue(graphData);
            if (oldOutputNode != null)
            {
                // 从节点列表中移除
                var removeNodeMethod = graphDataType.GetMethod("RemoveNode", new[] { oldOutputNode.GetType() });
                removeNodeMethod?.Invoke(graphData, new[] { oldOutputNode });
            }

            // 3. 创建新的 SubGraphOutputNode
            var subGraphOutputNodeType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.SubGraphOutputNode");
            if (subGraphOutputNodeType == null)
                throw new Exception("找不到 SubGraphOutputNode 类型");

            var newOutputNode = Activator.CreateInstance(subGraphOutputNodeType);

            // 设置节点 ID 和位置（可自定义）
            ShaderGraphReflectionHelper.SetNodeId(newOutputNode);
            ShaderGraphReflectionHelper.SetNodePosition(newOutputNode, new Rect(200, 200, 150, 80));

            // 4. 将新输出节点添加到图中
            var addNodeMethod = graphDataType.GetMethod("AddNode", new[] { Type.GetType("UnityEditor.ShaderGraph.AbstractMaterialNode") });
            if (addNodeMethod != null)
            {
                addNodeMethod.Invoke(graphData, new[] { newOutputNode });
            }
            else
            {
                // 降级：使用反射添加到 m_Nodes 列表（已通过 AddNode 处理，通常不会失败）
                throw new Exception("GraphData 中找不到 AddNode 方法");
            }

            // 5. 设置 m_OutputNode 字段
            outputNodeField.SetValue(graphData, newOutputNode);

            // 6. 验证图并清理
            var validateMethod = graphDataType.GetMethod("ValidateGraph", Type.EmptyTypes);
            validateMethod?.Invoke(graphData, null);
        }

        /// <summary>
        /// 直接从 GraphData 对象保存为子图资产文件。
        /// </summary>
        public static void SaveGraphDataAsSubGraph(object graphData, string outputAssetPath)
        {
            if (graphData == null) throw new ArgumentNullException(nameof(graphData));
            if (string.IsNullOrEmpty(outputAssetPath)) throw new ArgumentNullException(nameof(outputAssetPath));

            // 确保目录存在
            var dir = Path.GetDirectoryName(outputAssetPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!ShaderGraphReflectionHelper.SaveGraphDataToFile(graphData, outputAssetPath, out string error))
                throw new Exception($"保存失败: {error}");

            AssetDatabase.Refresh();
        }

        // 以下为从 JSON 重建的方法（保留，但实际推荐使用 ConvertShaderGraphToSubGraph）
        public static void BuildFromJson(string json, string outputAssetPath)
        {
            // 此方法可留空或调用 ConvertShaderGraphToSubGraph 的逻辑
            throw new NotImplementedException("推荐使用 ConvertShaderGraphToSubGraph 或直接操作 GraphData");
        }
    }
}