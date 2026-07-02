using System;

namespace 龙哥的秘密花园.节点库
{
    /// <summary>
    /// 标记一个静态类为子图端口定义类，并提供子图标识符和默认资产路径。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SubGraphPortsAttribute : Attribute
    {
        /// <summary>
        /// 子图标识符，用于解析和匹配。
        /// </summary>
        public string SubGraphIdentifier { get; }

        /// <summary>
        /// 子图资产的默认保存路径（相对于项目根目录）。
        /// 若为 null 或空，或不是以 "Assets/" 开头，则自动使用 "Assets/"。
        /// </summary>
        public string AssetPath { get; set; }
        public string Description { get; set; } 
        public SubGraphPortsAttribute(string subGraphIdentifier)
        {
            SubGraphIdentifier = subGraphIdentifier;
            AssetPath = "Assets/";
        }

        public SubGraphPortsAttribute(string subGraphIdentifier, string assetPath)
        {
            SubGraphIdentifier = subGraphIdentifier;

            // 校验 assetPath 合法性
            if (string.IsNullOrWhiteSpace(assetPath) || !assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                AssetPath = "Assets/";
            }
            else
            {
                AssetPath = assetPath;
            }
        }
    }
}