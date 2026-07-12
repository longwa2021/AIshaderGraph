using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.AI.MCP.Editor.ToolRegistry;
using UnityEngine;
using UnityEditor;
using 龙哥的秘密花园.节点库;
using SGB = 龙哥的秘密花园.ShaderGraphBuilder.ShaderGraphBuilder;

namespace 龙哥的秘密花园.AIshaderGraph
{
    public static class AiShaderBuildJsonTool
    {
        [McpTool("aishader_build_json", "Build a complete ShaderGraph from a JSON description")]
        public static object Execute(BuildJsonParams p)
        {
            try
            {
                if (!File.Exists(p.AssetPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(p.AssetPath));
                    var graphData = ShaderGraphReflectionHelper.CreateEmptyGraphData();
                    ShaderGraphReflectionHelper.SaveGraphDataToFile(graphData, p.AssetPath, out _);
                }

                var ctx = GraphDataContext.GetOrCreate(p.AssetPath);
                SGB.BuildFromJson(p.Json, ctx);
                ctx.Save();
                AssetDatabase.Refresh();

                return new { success = true, path = p.AssetPath, message = "ShaderGraph created" };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }
    }

    [Serializable]
    public class BuildJsonParams
    {
        [McpDescription("ShaderGraph asset path, e.g. Assets/Shaders/MyShader.shadergraph", Required = true)]
        public string AssetPath;

        [McpDescription("Complete JSON description of the ShaderGraph", Required = true)]
        public string Json;
    }

    public static class AiShaderGetKnowledgeTool
    {
        private static string _cachedSummary;
        private static JObject _cachedJson;

        [McpTool("aishader_get_knowledge", "Get ShaderGraph format reference, available node types with slots and parameters")]
        public static object Execute(GetKnowledgeParams p)
        {
            if (_cachedSummary == null)
            {
                string packageRoot = GetPackageRoot();
                string summaryPath = Path.Combine(packageRoot, "Editor", "知识库", "knowledge_summary.json");
                if (File.Exists(summaryPath))
                    _cachedSummary = File.ReadAllText(summaryPath);
                else
                    return new { error = "knowledge_summary.json not found at " + summaryPath };
            }

            if (_cachedJson == null)
                _cachedJson = JObject.Parse(_cachedSummary);

            string category = p.Category ?? "all";

            if (category == "format")
                return new { format = _cachedJson["format"], pipeline_targets = _cachedJson["pipeline_targets"], fragment_blocks = _cachedJson["fragment_blocks"] };

            if (category == "nodes")
                return new { nodes = _cachedJson["nodes"] };

            if (category == "node")
            {
                string nodeType = p.NodeType;
                if (string.IsNullOrEmpty(nodeType))
                    return new { error = "NodeType required for category=node" };

                foreach (var n in _cachedJson["nodes"])
                    if (n["t"]?.ToString() == nodeType)
                        return new { node = n };

                return new { error = $"Node type '{nodeType}' not found" };
            }

            if (category == "search")
            {
                string keyword = p.Keyword?.ToLower() ?? "";
                var matches = new JArray();
                foreach (var n in _cachedJson["nodes"])
                {
                    string desc = (n["t"]?.ToString() + " " + n["c"]?.ToString()).ToLower();
                    if (string.IsNullOrEmpty(keyword) || desc.Contains(keyword))
                        matches.Add(n);
                }
                return new { count = matches.Count, nodes = matches };
            }

            return new
            {
                format = _cachedJson["format"],
                pipeline_targets = _cachedJson["pipeline_targets"],
                fragment_blocks = _cachedJson["fragment_blocks"],
                node_count = (_cachedJson["nodes"] as JArray)?.Count ?? 0,
                hint = "Use category=format for schema, category=nodes for all nodes, category=node&nodeType=X for specific, category=search&keyword=X to find nodes"
            };
        }

        private static string GetPackageRoot()
        {
            // PackageCache (UPM git 安装)
            string parent = Path.Combine(Application.dataPath, "..", "Library", "PackageCache");
            if (Directory.Exists(parent))
            {
                string[] dirs = Directory.GetDirectories(parent, "com.longwa.aishadergraph@*");
                if (dirs.Length > 0) return dirs[0];
            }

            // Assets 本地
            string local = Path.Combine(Application.dataPath, "AIshaderGraph");
            if (Directory.Exists(local)) return local;

            // Packages 本地
            return Path.Combine(Application.dataPath, "..", "Packages", "com.longwa.aishadergraph");
        }
    }

    [Serializable]
    public class GetKnowledgeParams
    {
        [McpDescription("Category: format, nodes, node, search, all", Required = false)]
        public string Category;

        [McpDescription("Node type name for category=node, e.g. SampleTexture2DNode", Required = false)]
        public string NodeType;

        [McpDescription("Search keyword for category=search", Required = false)]
        public string Keyword;
    }
}
