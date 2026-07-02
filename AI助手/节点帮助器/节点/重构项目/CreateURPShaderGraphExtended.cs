using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using 龙哥的秘密花园.节点库;

public static class CreateURPShaderGraphExtended
{
    private static void CreateShaderGraph(string assetPath, Action<GraphDataContext> setupAction)
    {
        if (string.IsNullOrEmpty(assetPath)) return;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== CreateShaderGraph 调试信息 ===");
        sb.AppendLine($"资产路径: {assetPath}");

        // 1. 创建空白 GraphData
        var graphData = ShaderGraphReflectionHelper.CreateEmptyGraphData();
        sb.AppendLine($"1. 创建空白 GraphData: {(graphData != null ? "成功" : "失败")}");

        // 2. 序列化保存
        if (!ShaderGraphReflectionHelper.SaveGraphDataToFile(graphData, assetPath, out string error))
        {
            sb.AppendLine($"2. 保存失败: {error}");
            Debug.LogError(sb.ToString());
            return;
        }
        sb.AppendLine("2. 保存成功");

        // 3. 加载并配置
        var ctx = GraphDataContext.GetOrCreate(assetPath);
        sb.AppendLine($"3. GraphDataContext 加载: {(ctx != null ? "成功" : "失败")}");
        sb.AppendLine($"   - GraphData 实例: {ctx.GraphData?.GetType().FullName ?? "null"}");

        setupAction(ctx);
        ctx.Save();
        sb.AppendLine("4. 配置并保存完成");

        AssetDatabase.Refresh();
        sb.AppendLine("5. AssetDatabase 已刷新");

        Debug.Log(sb.ToString());
    }

    // ---------- 空白 Shader Graph ----------
    [MenuItem("Assets/龙哥的秘密花园/Create/Shader Graph/Blank Shader Graph", priority = 90)]
    public static void CreateBlankGraph()
    {
        // 调用 Unity 内置的 GraphUtil.CreateNewGraph()
        // 该方法会弹出保存对话框，因此不需要我们手动处理路径
        try
        {
            var graphUtilType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.GraphUtil");
            if (graphUtilType == null)
            {
                Debug.LogError("找不到 GraphUtil 类型，请确认 Unity 版本支持。");
                return;
            }

            var createMethod = graphUtilType.GetMethod("CreateNewGraph", BindingFlags.Public | BindingFlags.Static);
            if (createMethod == null)
            {
                Debug.LogError("找不到 GraphUtil.CreateNewGraph 方法。");
                return;
            }

            createMethod.Invoke(null, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"创建 Blank Shader Graph 失败: {ex.Message}");
        }
    }

    // ---------- 子图 (Sub Graph) ----------
    [MenuItem("Assets/龙哥的秘密花园/Create/Shader Graph/Sub Graph", priority = 91)]
    public static void CreateSubGraph()
    {
        // 调用 Unity 内置的 CreateShaderSubGraph.CreateMaterialSubGraph()
        try
        {
            var createSubGraphType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.CreateShaderSubGraph");
            if (createSubGraphType == null)
            {
                Debug.LogError("找不到 CreateShaderSubGraph 类型，请确认 Unity 版本支持。");
                return;
            }

            var createMethod = createSubGraphType.GetMethod("CreateMaterialSubGraph", BindingFlags.Public | BindingFlags.Static);
            if (createMethod == null)
            {
                Debug.LogError("找不到 CreateMaterialSubGraph 方法。");
                return;
            }

            createMethod.Invoke(null, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"创建 Sub Graph 失败: {ex.Message}");
        }
    }

    // ---------- 以下是原有的 URP / BuiltIn 管线特定 Shader Graph 创建菜单 ----------

    [MenuItem("Assets/龙哥的秘密花园/Create/Shader Graph/URP/Lit Shader Graph", priority = 100)]
    public static void CreateLitGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("New Lit Shader Graph", "NewLit", "shadergraph", "");
        CreateShaderGraph(path, ctx => ctx.URP.SetupAsURPLit());
    }

    [MenuItem("Assets/龙哥的秘密花园/Create/Shader Graph/URP/Unlit Shader Graph", priority = 101)]
    public static void CreateUnlitGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("New Unlit Shader Graph", "NewUnlit", "shadergraph", "");
        CreateShaderGraph(path, ctx => ctx.URP.SetupAsURPUnlit());
    }

    [MenuItem("Assets/龙哥的秘密花园/Create/Shader Graph/URP/Fullscreen Shader Graph", priority = 201)]
    public static void CreateFullscreenGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("New Fullscreen Shader Graph", "NewFullscreen", "shadergraph", "");
        CreateShaderGraph(path, ctx => ctx.URP.SetupAsURPFullscreen());
    }

    [MenuItem("Assets/龙哥的秘密花园/Create/Shader Graph/URP/Canvas Shader Graph", priority = 202)]
    public static void CreateCanvasGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("New Canvas Shader Graph", "NewCanvas", "shadergraph", "");
        CreateShaderGraph(path, ctx => ctx.URP.SetupAsURPCanvas());
    }

    [MenuItem("Assets/龙哥的秘密花园/Create/Shader Graph/URP/Six Way Shader Graph", priority = 203)]
    public static void CreateSixWayGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("New Six Way Shader Graph", "NewSixWay", "shadergraph", "");
        CreateShaderGraph(path, ctx => ctx.URP.SetupAsURPSixWay());
    }

    [MenuItem("Assets/龙哥的秘密花园/Create/Shader Graph/URP/Decal Shader Graph", priority = 204)]
    public static void CreateDecalGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("New Decal Shader Graph", "NewDecal", "shadergraph", "");
        CreateShaderGraph(path, ctx => ctx.URP.SetupAsURPDecal());
    }

    [MenuItem("Assets/龙哥的秘密花园/Create/Shader Graph/URP/Sprite Lit Shader Graph", priority = 205)]
    public static void CreateSpriteLitGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("New Sprite Lit Shader Graph", "NewSpriteLit", "shadergraph", "");
        CreateShaderGraph(path, ctx => ctx.URP.SetupAsURPSpriteLit());
    }

    [MenuItem("Assets/龙哥的秘密花园/Create/Shader Graph/URP/Sprite Unlit Shader Graph", priority = 206)]
    public static void CreateSpriteUnlitGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("New Sprite Unlit Shader Graph", "NewSpriteUnlit", "shadergraph", "");
        CreateShaderGraph(path, ctx => ctx.URP.SetupAsURPSpriteUnlit());
    }

    [MenuItem("Assets/龙哥的秘密花园/Create/Shader Graph/URP/Sprite Custom Lit Shader Graph", priority = 207)]
    public static void CreateSpriteCustomLitGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("New Sprite Custom Lit Shader Graph", "NewSpriteCustomLit", "shadergraph", "");
        CreateShaderGraph(path, ctx => ctx.URP.SetupAsURPSpriteCustomLit());
    }

    [MenuItem("Assets/龙哥的秘密花园/Create/Shader Graph/BuiltIn/Lit Shader Graph", priority = 300)]
    public static void CreateBuiltInLitGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("New BuiltIn Lit Shader Graph", "NewBuiltInLit", "shadergraph", "");
        CreateShaderGraph(path, ctx => ctx.BuiltIn.SetupAsBuiltInLit());
    }

    [MenuItem("Assets/龙哥的秘密花园/Create/Shader Graph/BuiltIn/Unlit Shader Graph", priority = 301)]
    public static void CreateBuiltInUnlitGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("New BuiltIn Unlit Shader Graph", "NewBuiltInUnlit", "shadergraph", "");
        CreateShaderGraph(path, ctx => ctx.BuiltIn.SetupAsBuiltInUnlit());
    }

    [MenuItem("Assets/龙哥的秘密花园/Create/Shader Graph/BuiltIn/Canvas Shader Graph", priority = 302)]
    public static void CreateBuiltInCanvasGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("New BuiltIn Canvas Shader Graph", "NewBuiltInCanvas", "shadergraph", "");
        CreateShaderGraph(path, ctx => ctx.BuiltIn.SetupAsBuiltInCanvas());
    }
}