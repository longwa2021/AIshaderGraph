using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace 龙哥的秘密花园.节点库
{
    /// <summary>
    /// 子图资产重建器：通过直接复制原始文件内容生成正确的 .shadersubgraph 文件。
    /// </summary>
    public static class SubGraphRebuilder
    {
        /// <summary>
        /// 清洗 Base64 字符串，移除所有非法字符
        /// </summary>
        private static string SanitizeBase64(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return Regex.Replace(input, @"[^A-Za-z0-9+/=]", "");
        }

        /// <summary>
        /// 从 Base64 编码的文件内容重建子图资产。
        /// </summary>
        /// <param name="base64Content">Base64 编码的完整文件内容</param>
        /// <param name="assetPath">保存路径</param>
        public static void RebuildFromBase64(string base64Content, string assetPath)
        {
            if (string.IsNullOrEmpty(base64Content))
                throw new ArgumentException("Base64 字符串不能为空");
            if (string.IsNullOrEmpty(assetPath))
                throw new ArgumentException("保存路径不能为空");

            string cleanBase64 = SanitizeBase64(base64Content);
            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(cleanBase64);
            }
            catch (Exception ex)
            {
                throw new Exception($"Base64 解码失败: {ex.Message}");
            }

            if (!assetPath.EndsWith(".shadersubgraph", StringComparison.OrdinalIgnoreCase))
                assetPath += ".shadersubgraph";

            // 直接写入原始字节
            File.WriteAllBytes(assetPath, fileBytes);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            Debug.Log($"SubGraphRebuilder: 子图已成功重建并导入 {assetPath}");
        }

        /// <summary>
        /// 从 SubGraphAsset 的源文件直接读取完整内容并转为 Base64。
        /// </summary>
        public static string ExtractBase64FromAsset(UnityEngine.Object subGraphAsset)
        {
            string assetPath = AssetDatabase.GetAssetPath(subGraphAsset);
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
                throw new Exception("无法获取子图资产的文件路径");

            byte[] bytes = File.ReadAllBytes(assetPath);
            return Convert.ToBase64String(bytes);
        }
    }
}