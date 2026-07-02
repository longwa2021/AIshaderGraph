using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace 龙哥的秘密花园.节点库
{
    /// <summary>
    /// 子图资产解析器：将 AI 友好的标识符转换为 SubGraphAsset 实例（完全通过反射操作，不直接依赖 internal 类型）。
    /// 支持在解析失败时从静态类的内嵌 Base64 JSON 自动重建资产。
    /// </summary>
    public static class SubGraphResolver
    {
        // 别名到 GUID 的映射
        private static readonly Dictionary<string, string> AliasToGuidMap = new Dictionary<string, string>();

        // 缓存反射获取的 SubGraphAsset 类型
        private static Type s_SubGraphAssetType;
        private static Type SubGraphAssetType
        {
            get
            {
                if (s_SubGraphAssetType == null)
                {
                    s_SubGraphAssetType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.SubGraphAsset");
                }
                return s_SubGraphAssetType;
            }
        }

        // 默认自动重建路径
        private const string DEFAULT_REBUILD_DIR = "Assets/GeneratedSubGraphs/";

        /// <summary>
        /// 注册别名，便于 AI 使用友好的名称引用子图。
        /// </summary>
        /// <param name="alias">别名（如 "DissolveEffect"）</param>
        /// <param name="assetGuid">子图资产的 GUID</param>
        public static void RegisterAlias(string alias, string assetGuid)
        {
            if (string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(assetGuid))
            {
                Debug.LogWarning("SubGraphResolver: 别名或 GUID 为空，注册失败。");
                return;
            }
            AliasToGuidMap[alias] = assetGuid;
        }

        /// <summary>
        /// 清除所有注册的别名。
        /// </summary>
        public static void ClearAliases()
        {
            AliasToGuidMap.Clear();
        }

        /// <summary>
        /// 根据标识符解析 SubGraphAsset，返回 UnityEngine.Object（实际类型为 SubGraphAsset）。
        /// 如果所有常规解析均失败，尝试从对应的静态类内嵌 Base64 JSON 自动重建。
        /// </summary>
        /// <param name="identifier">标识符字符串</param>
        /// <param name="type">标识符类型（默认 Auto）</param>
        /// <returns>SubGraphAsset 实例（作为 UnityEngine.Object），解析失败返回 null</returns>
        public static UnityEngine.Object Resolve(string identifier, SubGraphIdentifierType type = SubGraphIdentifierType.Auto)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                Debug.LogError("SubGraphResolver: 标识符为空");
                return null;
            }

            if (SubGraphAssetType == null)
            {
                Debug.LogError("SubGraphResolver: 找不到 SubGraphAsset 类型，请确保 Unity ShaderGraph 可用。");
                return null;
            }

            if (type != SubGraphIdentifierType.Auto)
            {
                var result = ResolveByType(identifier, type);
                if (result != null) return result;
            }
            else
            {
                // 自动检测顺序：GUID -> 别名 -> 资产名称 -> 路径
                // 1. 尝试作为 GUID
                string path = AssetDatabase.GUIDToAssetPath(identifier);
                if (!string.IsNullOrEmpty(path))
                {
                    var asset = LoadAssetAtPath(path);
                    if (asset != null) return asset;
                }

                // 2. 尝试作为注册别名
                if (AliasToGuidMap.TryGetValue(identifier, out string guid))
                {
                    path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(path))
                    {
                        var asset = LoadAssetAtPath(path);
                        if (asset != null) return asset;
                    }
                }

                // 3. 尝试按资产名称查找（忽略扩展名）
                var guids = AssetDatabase.FindAssets($"t:{SubGraphAssetType.Name}");
                foreach (var g in guids)
                {
                    var p = AssetDatabase.GUIDToAssetPath(g);
                    var assetName = Path.GetFileNameWithoutExtension(p);
                    if (assetName.Equals(identifier, StringComparison.OrdinalIgnoreCase))
                    {
                        var asset = LoadAssetAtPath(p);
                        if (asset != null) return asset;
                    }
                }

                // 4. 尝试作为直接路径
                var directAsset = LoadAssetAtPath(identifier);
                if (directAsset != null) return directAsset;

                // 5. 尝试添加 .shadersubgraph 扩展名再试
                if (!identifier.EndsWith(".shadersubgraph", StringComparison.OrdinalIgnoreCase))
                {
                    directAsset = LoadAssetAtPath(identifier + ".shadersubgraph");
                    if (directAsset != null) return directAsset;
                }
            }

            // 所有常规解析都失败，尝试从静态类内嵌 JSON 自动重建
            var rebuiltAsset = TryRebuildFromStaticClass(identifier);
            if (rebuiltAsset != null)
                return rebuiltAsset;

            Debug.LogError($"SubGraphResolver: 无法解析标识符 '{identifier}'");
            return null;
        }

        /// <summary>
        /// 从带有 SubGraphPortsAttribute 的静态类中读取 EmbeddedFileBase64 并重建子图资产。
        /// </summary>
        private static UnityEngine.Object TryRebuildFromStaticClass(string identifier)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch { continue; }

                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute<SubGraphPortsAttribute>();
                    if (attr == null || attr.SubGraphIdentifier != identifier) continue;

                    var field = type.GetField("EmbeddedFileBase64", BindingFlags.Public | BindingFlags.Static);
                    if (field == null) continue;

                    string base64 = field.GetValue(null) as string;
                    if (string.IsNullOrEmpty(base64)) continue;

                    string cleanBase64 = Regex.Replace(base64, @"[^A-Za-z0-9+/=]", "");

                    string assetPath;
                    if (!string.IsNullOrEmpty(attr.AssetPath))
                        assetPath = attr.AssetPath;
                    else
                        assetPath = $"{DEFAULT_REBUILD_DIR}{identifier}.shadersubgraph";

                    // 确保目录存在
                    string dir = Path.GetDirectoryName(assetPath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    // 如果资产已存在且有效，直接返回
                    if (File.Exists(assetPath))
                    {
                        var existing = LoadAssetAtPath(assetPath);
                        if (existing != null)
                            return existing;
                    }

                    // 执行重建
                    try
                    {
                        SubGraphRebuilder.RebuildFromBase64(cleanBase64, assetPath);
                        // 强制同步刷新，确保文件被正确导入
                        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                        // 重新加载资产
                        var rebuiltAsset = LoadAssetAtPath(assetPath);
                        if (rebuiltAsset != null)
                        {
                            Debug.Log($"SubGraphResolver: 成功重建并加载子图 '{identifier}' 于 {assetPath}");
                            return rebuiltAsset;
                        }
                        else
                        {
                            Debug.LogError($"SubGraphResolver: 重建文件已创建但加载失败: {assetPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"SubGraphResolver: 自动重建子图 '{identifier}' 失败: {ex.Message}");
                    }
                    return null;
                }
            }
            return null;
        }

        private static UnityEngine.Object ResolveByType(string identifier, SubGraphIdentifierType type)
        {
            switch (type)
            {
                case SubGraphIdentifierType.Guid:
                    var path = AssetDatabase.GUIDToAssetPath(identifier);
                    return string.IsNullOrEmpty(path) ? null : LoadAssetAtPath(path);

                case SubGraphIdentifierType.Alias:
                    if (AliasToGuidMap.TryGetValue(identifier, out string guid))
                    {
                        path = AssetDatabase.GUIDToAssetPath(guid);
                        return string.IsNullOrEmpty(path) ? null : LoadAssetAtPath(path);
                    }
                    Debug.LogError($"SubGraphResolver: 未注册别名 '{identifier}'");
                    return null;

                case SubGraphIdentifierType.AssetName:
                    var guids = AssetDatabase.FindAssets($"t:{SubGraphAssetType.Name}");
                    foreach (var g in guids)
                    {
                        var p = AssetDatabase.GUIDToAssetPath(g);
                        var assetName = Path.GetFileNameWithoutExtension(p);
                        if (assetName.Equals(identifier, StringComparison.OrdinalIgnoreCase))
                        {
                            var asset = LoadAssetAtPath(p);
                            if (asset != null) return asset;
                        }
                    }
                    Debug.LogError($"SubGraphResolver: 未找到名称为 '{identifier}' 的子图资产");
                    return null;

                case SubGraphIdentifierType.AssetPath:
                    var assetByPath = LoadAssetAtPath(identifier);
                    if (assetByPath == null && !identifier.EndsWith(".shadersubgraph", StringComparison.OrdinalIgnoreCase))
                        assetByPath = LoadAssetAtPath(identifier + ".shadersubgraph");
                    if (assetByPath == null)
                        Debug.LogError($"SubGraphResolver: 路径 '{identifier}' 无效");
                    return assetByPath;

                default:
                    return null;
            }
        }

        /// <summary>
        /// 加载指定路径的 SubGraphAsset。
        /// </summary>
        private static UnityEngine.Object LoadAssetAtPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;

            // 直接使用泛型加载（运行时类型是 UnityEngine.Object，实际为 SubGraphAsset）
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (obj != null && obj.GetType() == SubGraphAssetType)
                return obj;

            // 备用：通过反射调用 LoadAssetAtPath<T>
            var method = typeof(AssetDatabase).GetMethod("LoadAssetAtPath", new[] { typeof(string) });
            if (method != null)
            {
                var genericMethod = method.MakeGenericMethod(SubGraphAssetType);
                try
                {
                    return genericMethod.Invoke(null, new object[] { path }) as UnityEngine.Object;
                }
                catch { }
            }
            return null;
        }
    }
}