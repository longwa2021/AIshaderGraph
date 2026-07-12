using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.AI.MCP.Editor.ToolRegistry;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
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
            catch (Exception ex) { return new { success = false, error = ex.Message }; }
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
        [McpTool("aishader_get_knowledge", "Get ShaderGraph format reference, available node types with slots and parameters")]
        public static object Execute(GetKnowledgeParams p)
        {
            try
            {
                string root = GetPackageRoot();
                string path = Path.Combine(root, "Editor", "知识库", "knowledge_summary.json");
                if (!File.Exists(path))
                    return new { error = "Not found: " + path };

                string raw = File.ReadAllText(path);
                JObject json = JObject.Parse(raw);
                string cat = p.Category ?? "all";

                if (cat == "format")
                    return new { format = json["format"], pipeline_targets = json["pipeline_targets"], fragment_blocks = json["fragment_blocks"] };
                if (cat == "nodes")
                    return new { nodes = json["nodes"] };

                return new { hint = "Use category=format|nodes", node_count = (json["nodes"] as JArray)?.Count ?? 0 };
            }
            catch (Exception ex) { return new { error = ex.GetType().Name + ": " + ex.Message }; }
        }

       
        private static string GetPackageRoot()
        {
            // 1. Git 安装 → Packages/ 虚拟路径（Path.GetFullPath 解析到物理路径）
            string packagesPath = Path.GetFullPath("Packages/com.longwa.aishadergraph");
            Debug.Log(packagesPath);
            return packagesPath;
        }
    }

    public static class AiShaderPingTool
    {
        [McpTool("aishader_ping", "Test: returns the package root path")]
        public static object Execute()
        {
            string root = Path.Combine(Application.dataPath, "AIshaderGraph");
            return new { dataPath = Application.dataPath, root = root, exists = Directory.Exists(root) };
        }
    }
    
    [Serializable]
    public class GetKnowledgeParams
    {
        [McpDescription("Category: format, nodes, all", Required = false)]
        public string Category { get; set; }
    }
}
