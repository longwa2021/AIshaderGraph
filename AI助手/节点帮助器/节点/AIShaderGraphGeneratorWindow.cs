using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using 龙哥的秘密花园.节点库;
using 龙哥的秘密花园.ShaderGraphBuilder;

namespace 龙哥的秘密花园.Editor
{
    /// <summary>
    /// AI ShaderGraph 生成器窗口：根据 AI 返回的 JSON 在指定目录下创建对应的 ShaderGraph 资产。
    /// </summary>
    public class AIShaderGraphGeneratorWindow : EditorWindow
    {
        private string m_TargetDirectory = "Assets/";
        private string m_CustomAssetName = "";
        private string m_JsonInput = "";
        private Vector2 m_ScrollPos;

        [MenuItem("Tools/龙哥的秘密花园/AI ShaderGraph 生成器")]
        public static void ShowWindow()
        {
            var window = GetWindow<AIShaderGraphGeneratorWindow>("AI ShaderGraph 生成器");
            window.minSize = new Vector2(600, 550);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("AI ShaderGraph 生成器", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            // 切换到子图端口解析器
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("切换到子图端口解析器", GUILayout.Width(180)))
            {
                SubGraphParserWindow.ShowWindow();
                this.Close();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            // 目标目录选择
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("目标目录", GUILayout.Width(80));
            m_TargetDirectory = EditorGUILayout.TextField(m_TargetDirectory);
            if (GUILayout.Button("浏览", GUILayout.Width(50)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("选择目标目录", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        m_TargetDirectory = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("错误", "请选择 Assets 目录内的文件夹", "确定");
                    }
                }
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();

            // 使用当前 Project 选中目录按钮
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("使用 Project 窗口中选中的目录", GUILayout.Width(200)))
            {
                var selected = Selection.activeObject;
                if (selected != null)
                {
                    string path = AssetDatabase.GetAssetPath(selected);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        m_TargetDirectory = path;
                    }
                    else
                    {
                        m_TargetDirectory = Path.GetDirectoryName(path);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "请先在 Project 窗口中选择一个目录或文件", "确定");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 资产名称输入（可选）
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("资产名称", GUILayout.Width(80));
            m_CustomAssetName = EditorGUILayout.TextField(m_CustomAssetName);
            if (string.IsNullOrEmpty(m_CustomAssetName))
            {
                EditorGUILayout.LabelField("(自动生成)", GUILayout.Width(70));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("留空则根据 Pipeline 和 Target 自动生成名称", MessageType.None);

            EditorGUILayout.Space(10);

            // JSON 输入区域
            EditorGUILayout.LabelField("AI 返回的 JSON 字符串", EditorStyles.boldLabel);
            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, GUILayout.Height(300));
            m_JsonInput = EditorGUILayout.TextArea(m_JsonInput, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            // 生成按钮
            GUI.enabled = !string.IsNullOrEmpty(m_JsonInput) && !string.IsNullOrEmpty(m_TargetDirectory);
            if (GUILayout.Button("生成 ShaderGraph", GUILayout.Height(40)))
            {
                GenerateShaderGraph();
            }
            GUI.enabled = true;

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "将 AI 返回的 JSON 粘贴到上方文本框，点击生成按钮即可在目标目录创建对应的 ShaderGraph 资产。\n" +
                "JSON 需包含 Pipeline 和 Target 字段以确定创建的 Shader 类型。",
                MessageType.Info);
        }

        private void GenerateShaderGraph()
        {
            if (string.IsNullOrEmpty(m_JsonInput))
            {
                EditorUtility.DisplayDialog("错误", "JSON 字符串不能为空", "确定");
                return;
            }

            // 确保目标目录存在
            if (!AssetDatabase.IsValidFolder(m_TargetDirectory))
            {
                string parent = Path.GetDirectoryName(m_TargetDirectory);
                string folderName = Path.GetFileName(m_TargetDirectory);
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    EditorUtility.DisplayDialog("错误", $"目录无效: {m_TargetDirectory}", "确定");
                    return;
                }
                AssetDatabase.CreateFolder(parent, folderName);
            }

            // 解析 JSON 获取 Pipeline 和 Target
            string pipeline = "URP";
            string target = "Lit";
            string suggestedName = "NewShader";

            try
            {
                var root = JObject.Parse(m_JsonInput);
                if (root["Pipeline"] != null)
                    pipeline = root["Pipeline"].ToString();
                if (root["Target"] != null)
                    target = root["Target"].ToString();

                suggestedName = $"{pipeline}_{target}_ShaderGraph";
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"解析 JSON 失败，使用默认设置: {ex.Message}");
            }

            // 根据 Pipeline 和 Target 确定 setupAction
            Action<GraphDataContext> setupAction = null;
            bool isValid = true;

            if (pipeline == "URP")
            {
                switch (target)
                {
                    case "Unlit": setupAction = ctx => ctx.URP.SetupAsURPUnlit(); break;
                    case "Lit": setupAction = ctx => ctx.URP.SetupAsURPLit(); break;
                    case "Fullscreen": setupAction = ctx => ctx.URP.SetupAsURPFullscreen(); break;
                    case "Canvas": setupAction = ctx => ctx.URP.SetupAsURPCanvas(); break;
                    case "SixWay": setupAction = ctx => ctx.URP.SetupAsURPSixWay(); break;
                    case "Decal": setupAction = ctx => ctx.URP.SetupAsURPDecal(); break;
                    case "SpriteLit": setupAction = ctx => ctx.URP.SetupAsURPSpriteLit(); break;
                    case "SpriteUnlit": setupAction = ctx => ctx.URP.SetupAsURPSpriteUnlit(); break;
                    case "SpriteCustomLit": setupAction = ctx => ctx.URP.SetupAsURPSpriteCustomLit(); break;
                    default:
                        EditorUtility.DisplayDialog("错误", $"不支持的 URP Target: {target}", "确定");
                        isValid = false;
                        break;
                }
            }
            else if (pipeline == "BuiltIn")
            {
                switch (target)
                {
                    case "Lit": setupAction = ctx => ctx.BuiltIn.SetupAsBuiltInLit(); break;
                    case "Unlit": setupAction = ctx => ctx.BuiltIn.SetupAsBuiltInUnlit(); break;
                    case "Canvas": setupAction = ctx => ctx.BuiltIn.SetupAsBuiltInCanvas(); break;
                    default:
                        EditorUtility.DisplayDialog("错误", $"不支持的 BuiltIn Target: {target}", "确定");
                        isValid = false;
                        break;
                }
            }
            else
            {
                EditorUtility.DisplayDialog("错误", $"不支持的管线: {pipeline}", "确定");
                isValid = false;
            }

            if (!isValid || setupAction == null)
                return;

            // 确定最终资产名称
            string baseName = string.IsNullOrEmpty(m_CustomAssetName) ? suggestedName : m_CustomAssetName;
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(m_TargetDirectory, baseName + ".shadergraph"));

            try
            {
                CreateShaderGraphAtPath(assetPath, setupAction, m_JsonInput);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("生成失败", $"创建 ShaderGraph 时发生错误:\n{ex.Message}", "确定");
                Debug.LogError(ex);
                return;
            }
            
            // 刷新并选中新资产
            AssetDatabase.Refresh();
            var newAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (newAsset != null)
            {
                Selection.activeObject = newAsset;
                EditorGUIUtility.PingObject(newAsset);
                EditorUtility.DisplayDialog("成功", $"ShaderGraph 已生成:\n{assetPath}", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("警告", $"文件已创建但无法加载，请手动刷新:\n{assetPath}", "确定");
            }
        }

        private static void CreateShaderGraphAtPath(string assetPath, Action<GraphDataContext> setupAction, string jsonInput)
        {
            // 1. 创建空白 GraphData
            var graphData = ShaderGraphReflectionHelper.CreateEmptyGraphData();
            if (graphData == null)
                throw new Exception("创建空白 GraphData 失败");

            // 2. 保存到文件
            if (!ShaderGraphReflectionHelper.SaveGraphDataToFile(graphData, assetPath, out string error))
                throw new Exception($"保存失败: {error}");

            // 3. 清除缓存，强制重新加载
            GraphDataContext.ClearCache();
            AssetDatabase.Refresh();

            // 4. 加载并配置
            var ctx = GraphDataContext.GetOrCreate(assetPath);

            // 5. 应用管线预设
            setupAction(ctx);

            // 6. 构建节点
            if (!string.IsNullOrEmpty(jsonInput))
            {
                ShaderGraphBuilder.  ShaderGraphBuilder.BuildFromJson(jsonInput, ctx);
            }

            ctx.Save();
        }
    }
}