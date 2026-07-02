using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using 龙哥的秘密花园.ShaderGraphBuilder;

namespace 龙哥的秘密花园.节点库
{
    /// <summary>
    /// 子图解析窗口：选择一个子图资产，解析其输入输出端口，并生成对应的静态类代码（用于硬编码端口映射）。
    /// 支持将子图文件完整内容嵌入为 Base64，用于跨项目重建。支持输入功能描述供 AI 知识库使用。
    /// </summary>
    public class SubGraphParserWindow : EditorWindow
    {
        private const string PREFS_KEY_OUTPUT_PATH = "SubGraphParser_DefaultOutputPath";
        private string m_ExportedJson = "";
        private Vector2 m_JsonScrollPos;
        private UnityEngine.Object m_SelectedAsset;
        private string m_ClassName = "";
        private string m_SubGraphIdentifier = "";
        private string m_OutputPath = "Assets/";
        private string m_AssetPath = "";
        private string m_Description = "";
        private Vector2 m_ScrollPos;
        private Vector2 m_MainScrollPos;      // 主滚动视图位置

        private List<PortInfo> m_InputPorts = new List<PortInfo>();
        private List<PortInfo> m_OutputPorts = new List<PortInfo>();

        private bool m_PortsLoaded = false;

        private string m_DefaultOutputPath = "Assets/";

        private MonoScript m_TargetScript;

        [MenuItem("Tools/龙哥的秘密花园/子图端口解析器")]
        public static void ShowWindow()
        {
            var window = GetWindow<SubGraphParserWindow>("子图端口解析器");
            window.minSize = new Vector2(650, 950);
            window.Show();
        }

        private void OnEnable()
        {
            m_DefaultOutputPath = EditorPrefs.GetString(PREFS_KEY_OUTPUT_PATH, "Assets/");
            m_OutputPath = m_DefaultOutputPath;
        }

        private void OnGUI()
        {
            // ---------- 切换到 AI ShaderGraph 生成器按钮 ----------
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("切换到 AI ShaderGraph 生成器", GUILayout.Width(200)))
            {
                // 通过反射获取窗口类型并打开（避免循环依赖）
                var generatorType = Type.GetType("龙哥的秘密花园.Editor.AIShaderGraphGeneratorWindow");
                if (generatorType != null)
                {
                    var method = generatorType.GetMethod("ShowWindow", BindingFlags.Public | BindingFlags.Static);
                    method?.Invoke(null, null);
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "找不到 AI ShaderGraph 生成器窗口，请确保脚本已存在。", "确定");
                }
                this.Close();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            // ---------- 主滚动视图 ----------
            m_MainScrollPos = EditorGUILayout.BeginScrollView(m_MainScrollPos);

            GUILayout.Label("子图端口解析与代码生成", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // ---------- 默认输出路径配置 ----------
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("默认输出路径", GUILayout.Width(100));
            m_DefaultOutputPath = EditorGUILayout.TextField(m_DefaultOutputPath);
            if (GUILayout.Button("浏览", GUILayout.Width(50)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("选择默认输出目录", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        m_DefaultOutputPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("错误", "请选择 Assets 目录内的文件夹", "确定");
                        return;
                    }
                }
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("保存为默认", GUILayout.Width(100)))
            {
                EditorPrefs.SetString(PREFS_KEY_OUTPUT_PATH, m_DefaultOutputPath);
                Debug.Log($"默认输出路径已保存: {m_DefaultOutputPath}");
            }
            if (GUILayout.Button("重置默认", GUILayout.Width(100)))
            {
                m_DefaultOutputPath = "Assets/";
                EditorPrefs.SetString(PREFS_KEY_OUTPUT_PATH, m_DefaultOutputPath);
                m_OutputPath = m_DefaultOutputPath;
                Debug.Log("默认输出路径已重置为 Assets/");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // ---------- 资产选择 ----------
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("子图资产", GUILayout.Width(80));
            m_SelectedAsset = EditorGUILayout.ObjectField(m_SelectedAsset, typeof(UnityEngine.Object), false, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            if (m_SelectedAsset != null)
            {
                var assetType = m_SelectedAsset.GetType();
                if (assetType.FullName != "UnityEditor.ShaderGraph.SubGraphAsset")
                {
                    EditorGUILayout.HelpBox("请选择一个 Shader Sub Graph 资产（.shadersubgraph）", MessageType.Warning);
                }
            }

            // ---------- 快速创建测试子图 ----------
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("创建测试子图", GUILayout.Width(120)))
            {
                CreateTestSubGraph();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // ---------- 类名、标识符、资产路径、描述 ----------
            m_ClassName = EditorGUILayout.TextField("生成的类名", m_ClassName);
            m_SubGraphIdentifier = EditorGUILayout.TextField("子图标识符 (用于解析)", m_SubGraphIdentifier);
            m_AssetPath = EditorGUILayout.TextField("默认资产路径 (如 Assets/MySubGraph.shadersubgraph)", m_AssetPath);

            EditorGUILayout.LabelField("子图描述 (用于 AI 知识库)", EditorStyles.boldLabel);
            m_Description = EditorGUILayout.TextArea(m_Description, GUILayout.Height(60));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("当前输出路径", GUILayout.Width(100));
            m_OutputPath = EditorGUILayout.TextField(m_OutputPath);
            if (GUILayout.Button("使用默认", GUILayout.Width(80)))
            {
                m_OutputPath = m_DefaultOutputPath;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // ---------- 解析按钮 ----------
            GUI.enabled = m_SelectedAsset != null && m_SelectedAsset.GetType().FullName == "UnityEditor.ShaderGraph.SubGraphAsset";
            if (GUILayout.Button("解析端口", GUILayout.Height(30)))
            {
                ParseSubGraphPorts();
            }
            GUI.enabled = true;

            EditorGUILayout.Space(10);

            // ---------- 显示解析结果 ----------
            if (m_PortsLoaded)
            {
                m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, GUILayout.Height(250));

                EditorGUILayout.LabelField("输入端口", EditorStyles.boldLabel);
                if (m_InputPorts.Count == 0)
                {
                    EditorGUILayout.LabelField("  (无)");
                }
                else
                {
                    foreach (var port in m_InputPorts)
                    {
                        EditorGUILayout.LabelField($"  • {port.DisplayName}  [{port.ReferenceName}]  ({port.PropertyType})");
                    }
                }

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("输出端口", EditorStyles.boldLabel);
                if (m_OutputPorts.Count == 0)
                {
                    EditorGUILayout.LabelField("  (无)");
                }
                else
                {
                    foreach (var port in m_OutputPorts)
                    {
                        EditorGUILayout.LabelField($"  • {port.DisplayName}");
                    }
                }

                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(10);

                // ---------- 生成代码按钮 ----------
                GUI.enabled = !string.IsNullOrEmpty(m_ClassName);
                if (GUILayout.Button("生成静态类代码（含描述及文件 Base64）", GUILayout.Height(30)))
                {
                    GenerateStaticClassCode();
                }
                GUI.enabled = true;
            }


            // ---------- AI 分析 JSON 导出 ----------
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("AI 分析 JSON 导出", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            GUI.enabled = m_SelectedAsset != null && m_SelectedAsset.GetType().FullName == "UnityEditor.ShaderGraph.SubGraphAsset";
            if (GUILayout.Button("生成分析 JSON", GUILayout.Height(30)))
            {
                try
                {
                    m_ExportedJson = SubGraphJsonExporter.ExportToJson(m_SelectedAsset);
                }
                catch (Exception ex)
                {
                    m_ExportedJson = "导出失败: " + ex.Message;
                    Debug.LogError(ex);
                }
            }
            GUI.enabled = true;

            if (!string.IsNullOrEmpty(m_ExportedJson))
            {
                EditorGUILayout.Space(5);
                m_JsonScrollPos = EditorGUILayout.BeginScrollView(m_JsonScrollPos, GUILayout.Height(200));
                EditorGUILayout.TextArea(m_ExportedJson, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("复制 JSON 到剪贴板"))
                {
                    GUIUtility.systemCopyBuffer = m_ExportedJson;
                    EditorUtility.DisplayDialog("成功", "JSON 已复制到剪贴板。", "确定");
                }
                if (GUILayout.Button("清空显示"))
                {
                    m_ExportedJson = "";
                }
                EditorGUILayout.EndHorizontal();
            }

            // ---------- 子图重建工具 ----------
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("子图重建工具", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 拖拽目标脚本
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("拖入脚本", GUILayout.Width(80));
            m_TargetScript = (MonoScript)EditorGUILayout.ObjectField(m_TargetScript, typeof(MonoScript), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("从拖入的脚本重建子图", GUILayout.Height(30)))
            {
                RebuildSubGraphFromTargetScript();
            }
            if (GUILayout.Button("从 Project 选中文件重建", GUILayout.Height(30)))
            {
                RebuildSubGraphFromSelectedScript();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "将带有 [SubGraphPorts] 特性的脚本拖入上方字段，点击按钮即可从内嵌 Base64 重建子图资产。\n" +
                "若特性中指定了 AssetPath，将作为保存对话框的默认路径。",
                MessageType.Info);

            EditorGUILayout.EndScrollView();
        }

        private void CreateTestSubGraph()
        {
            try
            {
                var createSubGraphType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.CreateShaderSubGraph");
                if (createSubGraphType == null)
                {
                    EditorUtility.DisplayDialog("错误", "找不到 CreateShaderSubGraph 类型，请确认 Unity 版本支持。", "确定");
                    return;
                }

                var createMethod = createSubGraphType.GetMethod("CreateMaterialSubGraph", BindingFlags.Public | BindingFlags.Static);
                if (createMethod == null)
                {
                    EditorUtility.DisplayDialog("错误", "找不到 CreateMaterialSubGraph 方法。", "确定");
                    return;
                }

                createMethod.Invoke(null, null);
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建测试子图失败: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("创建失败", $"创建子图时发生错误:\n{ex.Message}", "确定");
            }
        }

        private void ParseSubGraphPorts()
        {
            m_InputPorts.Clear();
            m_OutputPorts.Clear();

            var asset = m_SelectedAsset;
            var assetType = asset.GetType();

            try
            {
                var loadMethod = assetType.GetMethod("LoadGraphData", BindingFlags.Public | BindingFlags.Instance);
                if (loadMethod != null)
                {
                    loadMethod.Invoke(asset, null);
                }

                var subGraphDataField = assetType.GetField("m_SubGraphData", BindingFlags.NonPublic | BindingFlags.Instance);
                if (subGraphDataField == null)
                {
                    Debug.LogError("无法找到 m_SubGraphData 字段");
                    return;
                }
                var subGraphData = subGraphDataField.GetValue(asset);
                if (subGraphData == null)
                {
                    Debug.LogError("m_SubGraphData 为空");
                    return;
                }

                var subGraphDataType = subGraphData.GetType();

                // 解析输入
                var inputsField = subGraphDataType.GetField("inputs", BindingFlags.Public | BindingFlags.Instance);
                if (inputsField != null)
                {
                    var inputsList = inputsField.GetValue(subGraphData) as System.Collections.IList;
                    if (inputsList != null)
                    {
                        foreach (var item in inputsList)
                        {
                            var valueProp = item.GetType().GetProperty("value");
                            var property = valueProp?.GetValue(item);
                            if (property != null)
                            {
                                var propType = property.GetType();
                                string displayName = propType.GetProperty("displayName")?.GetValue(property) as string ?? "";
                                string referenceName = propType.GetProperty("referenceName")?.GetValue(property) as string ?? "";
                                string propTypeStr = propType.Name.Replace("ShaderProperty", "");

                                m_InputPorts.Add(new PortInfo
                                {
                                    DisplayName = displayName,
                                    ReferenceName = referenceName,
                                    PropertyType = propTypeStr
                                });
                            }
                        }
                    }
                }

                // 解析输出
                var outputsField = subGraphDataType.GetField("outputs", BindingFlags.Public | BindingFlags.Instance);
                if (outputsField != null)
                {
                    var outputsList = outputsField.GetValue(subGraphData) as System.Collections.IList;
                    if (outputsList != null)
                    {
                        foreach (var item in outputsList)
                        {
                            var valueProp = item.GetType().GetProperty("value");
                            var slot = valueProp?.GetValue(item);
                            if (slot != null)
                            {
                                string rawDisplayName = slot.GetType().GetMethod("RawDisplayName", BindingFlags.Public | BindingFlags.Instance)?.Invoke(slot, null) as string;
                                if (string.IsNullOrEmpty(rawDisplayName))
                                    rawDisplayName = slot.GetType().GetProperty("displayName")?.GetValue(slot) as string ?? "";
                                m_OutputPorts.Add(new PortInfo
                                {
                                    DisplayName = rawDisplayName,
                                    ReferenceName = rawDisplayName,
                                    PropertyType = "Slot"
                                });
                            }
                        }
                    }
                }

                m_PortsLoaded = true;

                if (string.IsNullOrEmpty(m_ClassName))
                {
                    string assetName = asset.name;
                    m_ClassName = string.Concat(assetName.Where(c => char.IsLetterOrDigit(c) || c == '_'));
                    if (string.IsNullOrEmpty(m_ClassName))
                        m_ClassName = "MySubGraph";
                    else if (char.IsDigit(m_ClassName[0]))
                        m_ClassName = "_" + m_ClassName;
                }

                if (string.IsNullOrEmpty(m_SubGraphIdentifier))
                {
                    m_SubGraphIdentifier = asset.name;
                }

                if (string.IsNullOrEmpty(m_AssetPath))
                {
                    m_AssetPath = $"Assets/GeneratedSubGraphs/{m_SubGraphIdentifier}.shadersubgraph";
                }

                Debug.Log($"成功解析子图端口：{m_InputPorts.Count} 个输入，{m_OutputPorts.Count} 个输出");
            }
            catch (Exception ex)
            {
                Debug.LogError($"解析子图端口失败: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("解析失败", $"解析子图端口时出错:\n{ex.Message}", "确定");
            }
        }

        private string ExtractFileBase64(UnityEngine.Object subGraphAsset)
        {
            try
            {
                return SubGraphRebuilder.ExtractBase64FromAsset(subGraphAsset);
            }
            catch (Exception ex)
            {
                Debug.LogError($"提取文件内容失败: {ex.Message}");
                return null;
            }
        }

        private void GenerateStaticClassCode()
        {
            if (string.IsNullOrEmpty(m_ClassName))
            {
                EditorUtility.DisplayDialog("错误", "请先输入类名", "确定");
                return;
            }

            string base64Content = ExtractFileBase64(m_SelectedAsset);
            if (string.IsNullOrEmpty(base64Content))
            {
                EditorUtility.DisplayDialog("错误", "无法提取子图文件内容。", "确定");
                return;
            }

            const int chunkSize = 80;
            var chunks = new List<string>();
            for (int i = 0; i < base64Content.Length; i += chunkSize)
            {
                int length = Math.Min(chunkSize, base64Content.Length - i);
                chunks.Add(base64Content.Substring(i, length));
            }

            var sb = new StringBuilder();
            sb.AppendLine("using 龙哥的秘密花园.节点库;");
            sb.AppendLine();
            sb.AppendLine($"/// <summary>");
            sb.AppendLine($"/// 子图端口定义：{m_SubGraphIdentifier}");
            if (!string.IsNullOrEmpty(m_Description))
                sb.AppendLine($"/// 功能描述：{m_Description}");
            sb.AppendLine($"/// 自动生成，请勿手动修改。包含完整文件 Base64 用于跨项目重建。");
            sb.AppendLine($"/// </summary>");

            // 构建特性参数
            var attrParams = new List<string> { $"\"{m_SubGraphIdentifier}\"" };
            if (!string.IsNullOrEmpty(m_AssetPath))
                attrParams.Add($"AssetPath = \"{m_AssetPath}\"");
            if (!string.IsNullOrEmpty(m_Description))
                attrParams.Add($"Description = \"{EscapeForCSharp(m_Description)}\"");
            sb.AppendLine($"[SubGraphPorts({string.Join(", ", attrParams)})]");

            sb.AppendLine($"public static class {m_ClassName}");
            sb.AppendLine("{");

            // 输入端口
            sb.AppendLine("    public static class Input");
            sb.AppendLine("    {");
            foreach (var port in m_InputPorts)
            {
                string fieldName = ToValidCSharpIdentifier(port.ReferenceName);
                sb.AppendLine($"        public const string {fieldName} = \"{port.ReferenceName}\";");
            }
            sb.AppendLine("    }");
            sb.AppendLine();

            // 输出端口
            sb.AppendLine("    public static class Output");
            sb.AppendLine("    {");
            foreach (var port in m_OutputPorts)
            {
                string fieldName = ToValidCSharpIdentifier(port.DisplayName);
                sb.AppendLine($"        public const string {fieldName} = \"{port.DisplayName}\";");
            }
            sb.AppendLine("    }");
            sb.AppendLine();

            // 内嵌 Base64 常量（移至最后）
            sb.AppendLine("    /// <summary>子图资产的完整文件内容（Base64），用于重建资产。</summary>");
            sb.AppendLine("    public const string EmbeddedFileBase64 = ");
            for (int i = 0; i < chunks.Count; i++)
            {
                if (i == 0)
                    sb.AppendLine($"        @\"{chunks[i]}\" +");
                else if (i < chunks.Count - 1)
                    sb.AppendLine($"        @\"{chunks[i]}\" +");
                else
                    sb.AppendLine($"        @\"{chunks[i]}\";");
            }

            sb.AppendLine("}");

            string code = sb.ToString();
            string fileName = m_ClassName + ".cs";
            string fullPath = Path.Combine(m_OutputPath, fileName);

            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, code);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("成功", $"代码已生成并保存到:\n{fullPath}", "确定");

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fullPath);
            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            }
        }

        private string ToValidCSharpIdentifier(string input)
        {
            if (string.IsNullOrEmpty(input)) return "_";
            var sb = new StringBuilder();
            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
                else
                    sb.Append('_');
            }
            string result = sb.ToString();
            if (result.Length > 0 && char.IsDigit(result[0]))
                result = "_" + result;
            return string.IsNullOrEmpty(result) ? "_" : result;
        }

        private string EscapeForCSharp(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        // ---------- 从脚本中提取 AssetPath 特性值 ----------
        private string ExtractAssetPathFromScriptContent(string scriptContent)
        {
            var match = Regex.Match(scriptContent, @"\[SubGraphPorts\s*\(\s*""[^""]*""\s*(?:,\s*AssetPath\s*=\s*""([^""]*)"")\s*\)\s*\]");
            if (match.Success)
                return match.Groups[1].Value;
            return null;
        }

        private void RebuildSubGraphFromTargetScript()
        {
            if (m_TargetScript == null)
            {
                EditorUtility.DisplayDialog("错误", "请先拖入一个脚本文件。", "确定");
                return;
            }

            string scriptPath = AssetDatabase.GetAssetPath(m_TargetScript);
            if (!scriptPath.EndsWith(".cs"))
            {
                EditorUtility.DisplayDialog("错误", "请拖入一个 C# 脚本文件。", "确定");
                return;
            }

            string scriptContent = File.ReadAllText(scriptPath);
            RebuildFromScriptContent(scriptContent, Path.GetFileNameWithoutExtension(scriptPath));
        }

        private void RebuildSubGraphFromSelectedScript()
        {
            var selected = Selection.activeObject;
            if (selected == null || !AssetDatabase.GetAssetPath(selected).EndsWith(".cs"))
            {
                EditorUtility.DisplayDialog("错误", "请在 Project 窗口中选中一个包含子图端口定义的 .cs 文件。", "确定");
                return;
            }

            string scriptPath = AssetDatabase.GetAssetPath(selected);
            string scriptContent = File.ReadAllText(scriptPath);
            RebuildFromScriptContent(scriptContent, Path.GetFileNameWithoutExtension(scriptPath));
        }

        private void RebuildFromScriptContent(string scriptContent, string defaultName)
        {
            // 提取 EmbeddedFileBase64 常量
            var match = Regex.Match(scriptContent,
                @"public\s+const\s+string\s+EmbeddedFileBase64\s*=\s*((?:@""[^""]*""\s*\+\s*)*@""[^""]*"")\s*;",
                RegexOptions.Singleline);
            if (!match.Success)
            {
                EditorUtility.DisplayDialog("错误", "在文件中未找到 EmbeddedFileBase64 常量定义。", "确定");
                return;
            }

            string combinedBase64 = "";
            var fragmentMatches = Regex.Matches(match.Groups[1].Value, @"@""([^""]*)""");
            foreach (Match frag in fragmentMatches)
            {
                combinedBase64 += frag.Groups[1].Value;
            }

            string cleanBase64 = Regex.Replace(combinedBase64, @"[^A-Za-z0-9+/=]", "");

            string className = defaultName;
            var classMatch = Regex.Match(scriptContent, @"public\s+static\s+class\s+(\w+)");
            if (classMatch.Success)
                className = classMatch.Groups[1].Value;

            string defaultAssetPath = ExtractAssetPathFromScriptContent(scriptContent);
            if (string.IsNullOrEmpty(defaultAssetPath))
            {
                defaultAssetPath = $"Assets/GeneratedSubGraphs/{className}.shadersubgraph";
            }

            string savePath = EditorUtility.SaveFilePanelInProject("重建子图", className, "shadersubgraph", "选择子图保存位置", Path.GetDirectoryName(defaultAssetPath));
            if (string.IsNullOrEmpty(savePath))
                return;

            try
            {
                SubGraphRebuilder.RebuildFromBase64(cleanBase64, savePath);
                EditorUtility.DisplayDialog("成功", $"子图已成功重建并保存到:\n{savePath}", "确定");
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(savePath);
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                    Selection.activeObject = asset;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"重建子图失败: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("重建失败", $"重建子图时发生错误:\n{ex.Message}", "确定");
            }
        }

        [Serializable]
        private class PortInfo
        {
            public string DisplayName;
            public string ReferenceName;
            public string PropertyType;
        }
    }
}