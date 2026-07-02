using System;
using UnityEditor.ShaderGraph.Internal;

namespace 龙哥的秘密花园.节点库
{
    public static class EnumConverter
    {
        /// <summary>将公开枚举值转换为内部枚举对象</summary>
        public static object ToInternalEnum<T>(T publicValue) where T : Enum
        {
            string internalTypeName = GetInternalTypeName(typeof(T));
            Type internalType = ShaderGraphReflectionHelper.FindType(internalTypeName);
            
            if (internalType == null)
                throw new Exception($"找不到内部枚举类型: {internalTypeName}");
            return Enum.Parse(internalType, publicValue.ToString());
        }

        private static string GetInternalTypeName(Type publicType)
        {
            // 根据提供的源码和命名空间
            if (publicType == typeof(ShaderStageCapability))
                return "UnityEditor.ShaderGraph.ShaderStageCapability";
            if (publicType == typeof(SlotValueType))
                return "UnityEditor.ShaderGraph.SlotValueType";
            if (publicType == typeof(SlotType))
                return "UnityEditor.Graphing.SlotType";   // 注意命名空间
            if (publicType == typeof(HlslSourceTypeOption))
                return "UnityEditor.ShaderGraph.Drawing.HlslSourceType";
            if (publicType == typeof(ColorMode))
                return "UnityEditor.ShaderGraph.Internal.ColorMode";
            throw new NotSupportedException($"未配置的内部类型映射: {publicType.Name}");
        }
    }
}