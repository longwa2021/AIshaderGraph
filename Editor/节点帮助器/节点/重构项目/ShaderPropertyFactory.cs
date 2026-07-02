using System;
using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph.Internal;

namespace 龙哥的秘密花园.节点库
{
    /// <summary>
    /// ShaderGraph 黑板属性工厂：创建各种类型的 AbstractShaderProperty 实例。
    /// </summary>
    public static class ShaderPropertyFactory
    {
        public static AbstractShaderProperty CreateVector1Property(string name, float defaultValue,
            FloatType floatType = FloatType.Default, Vector2 rangeValues = default)
        {
            var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.Vector1ShaderProperty");
            if (propType == null) throw new Exception("找不到 Vector1ShaderProperty 类型");
            var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

            ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
            ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

            // 设置 value 属性
            var valueProp = propType.GetProperty("value");
            valueProp?.SetValue(prop, defaultValue);

            // 设置 floatType（属性 + 字段）
            var floatTypeProp = propType.GetProperty("floatType");
            floatTypeProp?.SetValue(prop, floatType);
            var floatTypeField = propType.GetField("m_FloatType", BindingFlags.NonPublic | BindingFlags.Instance);
            floatTypeField?.SetValue(prop, floatType);

            // 设置 rangeValues（仅当 Slider 模式时）
            if (floatType == FloatType.Slider)
            {
                var rangeProp = propType.GetProperty("rangeValues");
                rangeProp?.SetValue(prop, rangeValues);
                var rangeField = propType.GetField("m_RangeValues", BindingFlags.NonPublic | BindingFlags.Instance);
                rangeField?.SetValue(prop, rangeValues);
            }

            return prop;
        }
        public static AbstractShaderProperty CreateVector2Property(string name, Vector2 defaultValue)
{
    var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.Vector2ShaderProperty");
    if (propType == null) throw new Exception("找不到 Vector2ShaderProperty 类型");
    var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

    ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
    ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

    var valueProp = propType.GetProperty("value");
    // Vector2ShaderProperty 的 value 实际是 Vector4 类型，需要转换
    valueProp?.SetValue(prop, new Vector4(defaultValue.x, defaultValue.y, 0, 0));
    return prop;
}

public static AbstractShaderProperty CreateVector3Property(string name, Vector3 defaultValue)
{
    var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.Vector3ShaderProperty");
    if (propType == null) throw new Exception("找不到 Vector3ShaderProperty 类型");
    var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

    ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
    ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

    var valueProp = propType.GetProperty("value");
    // Vector3ShaderProperty 的 value 实际是 Vector4 类型，需要转换
    valueProp?.SetValue(prop, new Vector4(defaultValue.x, defaultValue.y, defaultValue.z, 0));
    return prop;
}

public static AbstractShaderProperty CreateVector4Property(string name, Vector4 defaultValue)
{
    var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.Vector4ShaderProperty");
    if (propType == null) throw new Exception("找不到 Vector4ShaderProperty 类型");
    var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

    ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
    ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

    var valueProp = propType.GetProperty("value");
    valueProp?.SetValue(prop, defaultValue);
    return prop;
}

public static AbstractShaderProperty CreateColorProperty(string name, Color defaultValue,
    ColorMode colorMode = ColorMode.Default, bool hdr = false)
{
    var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.ColorShaderProperty");
    if (propType == null) throw new Exception("找不到 ColorShaderProperty 类型");
    var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

    ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
    ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

    var valueProp = propType.GetProperty("value");
    valueProp?.SetValue(prop, defaultValue);

    // 设置 colorMode
    var colorModeProp = propType.GetProperty("colorMode");
    colorModeProp?.SetValue(prop, colorMode);
    var colorModeField = propType.GetField("m_ColorMode", BindingFlags.NonPublic | BindingFlags.Instance);
    colorModeField?.SetValue(prop, (int)colorMode);

    // 设置 hdr
    var hdrProp = propType.GetProperty("hdr");
    hdrProp?.SetValue(prop, hdr);
    var hdrField = propType.GetField("m_HDR", BindingFlags.NonPublic | BindingFlags.Instance);
    hdrField?.SetValue(prop, hdr);

    return prop;
}

        public static AbstractShaderProperty CreateBooleanProperty(string name, bool defaultValue)
        {
            var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.BooleanShaderProperty");
            if (propType == null) throw new Exception("找不到 BooleanShaderProperty 类型");
            var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

            ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
            ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

            var valueProp = propType.GetProperty("value");
            valueProp?.SetValue(prop, defaultValue);
            return prop;
        }

        public static AbstractShaderProperty CreateMatrix2Property(string name, Vector2 row0, Vector2 row1)
        {
            var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.Matrix2ShaderProperty");
            if (propType == null) throw new Exception("找不到 Matrix2ShaderProperty 类型");
            var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

            ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
            ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

            var row0Prop = propType.GetProperty("row0");
            row0Prop?.SetValue(prop, row0);
            var row1Prop = propType.GetProperty("row1");
            row1Prop?.SetValue(prop, row1);
            return prop;
        }

        public static AbstractShaderProperty CreateMatrix3Property(string name, Vector3 row0, Vector3 row1, Vector3 row2)
        {
            var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.Matrix3ShaderProperty");
            if (propType == null) throw new Exception("找不到 Matrix3ShaderProperty 类型");
            var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

            ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
            ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

            var row0Prop = propType.GetProperty("row0");
            row0Prop?.SetValue(prop, row0);
            var row1Prop = propType.GetProperty("row1");
            row1Prop?.SetValue(prop, row1);
            var row2Prop = propType.GetProperty("row2");
            row2Prop?.SetValue(prop, row2);
            return prop;
        }

        public static AbstractShaderProperty CreateMatrix4Property(string name, Vector4 row0, Vector4 row1, Vector4 row2, Vector4 row3)
        {
            var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.Matrix4ShaderProperty");
            if (propType == null) throw new Exception("找不到 Matrix4ShaderProperty 类型");
            var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

            ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
            ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

            var row0Prop = propType.GetProperty("row0");
            row0Prop?.SetValue(prop, row0);
            var row1Prop = propType.GetProperty("row1");
            row1Prop?.SetValue(prop, row1);
            var row2Prop = propType.GetProperty("row2");
            row2Prop?.SetValue(prop, row2);
            var row3Prop = propType.GetProperty("row3");
            row3Prop?.SetValue(prop, row3);
            return prop;
        }

        public static AbstractShaderProperty CreateTexture2DProperty(string name, Texture2D defaultValue = null)
        {
            var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.Texture2DShaderProperty");
            if (propType == null) throw new Exception("找不到 Texture2DShaderProperty 类型");
            var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

            ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
            ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

            if (defaultValue != null)
            {
                var valueProp = propType.GetProperty("value");
                if (valueProp != null)
                {
                    var serializableTextureType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.SerializableTexture");
                    var serializableTex = Activator.CreateInstance(serializableTextureType);
                    var textureField = serializableTextureType.GetField("texture");
                    textureField?.SetValue(serializableTex, defaultValue);
                    valueProp.SetValue(prop, serializableTex);
                }
            }
            return prop;
        }

        public static AbstractShaderProperty CreateTexture2DArrayProperty(string name, Texture2DArray defaultValue = null)
        {
            var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.Texture2DArrayShaderProperty");
            if (propType == null) throw new Exception("找不到 Texture2DArrayShaderProperty 类型");
            var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

            ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
            ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

            if (defaultValue != null)
            {
                var valueProp = propType.GetProperty("value");
                if (valueProp != null)
                {
                    var serializableTextureArrayType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.SerializableTextureArray");
                    var serializableTexArr = Activator.CreateInstance(serializableTextureArrayType);
                    var textureArrayField = serializableTextureArrayType.GetField("textureArray");
                    textureArrayField?.SetValue(serializableTexArr, defaultValue);
                    valueProp.SetValue(prop, serializableTexArr);
                }
            }
            return prop;
        }

        public static AbstractShaderProperty CreateTexture3DProperty(string name, Texture3D defaultValue = null)
        {
            var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.Texture3DShaderProperty");
            if (propType == null) throw new Exception("找不到 Texture3DShaderProperty 类型");
            var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

            ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
            ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

            if (defaultValue != null)
            {
                var valueProp = propType.GetProperty("value");
                if (valueProp != null)
                {
                    var serializableTextureType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.SerializableTexture");
                    var serializableTex = Activator.CreateInstance(serializableTextureType);
                    var textureField = serializableTextureType.GetField("texture");
                    textureField?.SetValue(serializableTex, defaultValue);
                    valueProp.SetValue(prop, serializableTex);
                }
            }
            return prop;
        }

        public static AbstractShaderProperty CreateCubemapProperty(string name, Cubemap defaultValue = null)
        {
            var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.CubemapShaderProperty");
            if (propType == null) throw new Exception("找不到 CubemapShaderProperty 类型");
            var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

            ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
            ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

            if (defaultValue != null)
            {
                var valueProp = propType.GetProperty("value");
                if (valueProp != null)
                {
                    var serializableCubemapType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.SerializableCubemap");
                    var serializableCube = Activator.CreateInstance(serializableCubemapType);
                    var cubemapField = serializableCubemapType.GetField("cubemap");
                    cubemapField?.SetValue(serializableCube, defaultValue);
                    valueProp.SetValue(prop, serializableCube);
                }
            }
            return prop;
        }

        public static AbstractShaderProperty CreateSamplerStateProperty(string name,
            SamplerFilterMode filter = SamplerFilterMode.Linear,
            SamplerWrapMode wrap = SamplerWrapMode.Repeat,
            SamplerAnisotropic anisotropic = SamplerAnisotropic.None)
        {
            var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.SamplerStateShaderProperty");
            if (propType == null) throw new Exception("找不到 SamplerStateShaderProperty 类型");
            var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

            ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
            ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

            var valueProp = propType.GetProperty("value");
            if (valueProp != null)
            {
                var samplerStateType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.TextureSamplerState");
                if (samplerStateType == null) throw new Exception("找不到 TextureSamplerState 类型");
                var samplerState = Activator.CreateInstance(samplerStateType);

                object internalFilter = ShaderGraphReflectionHelper.ConvertToInternalEnum("FilterMode", filter);
                object internalWrap = ShaderGraphReflectionHelper.ConvertToInternalEnum("WrapMode", wrap);
                object internalAniso = ShaderGraphReflectionHelper.ConvertToInternalEnum("Anisotropic", anisotropic);

                var filterField = samplerStateType.GetField("m_filter", BindingFlags.NonPublic | BindingFlags.Instance);
                var wrapField = samplerStateType.GetField("m_wrap", BindingFlags.NonPublic | BindingFlags.Instance);
                var anisotropicField = samplerStateType.GetField("m_anisotropic", BindingFlags.NonPublic | BindingFlags.Instance);

                filterField?.SetValue(samplerState, internalFilter);
                wrapField?.SetValue(samplerState, internalWrap);
                anisotropicField?.SetValue(samplerState, internalAniso);

                valueProp.SetValue(prop, samplerState);
            }
            return prop;
        }
       
        public static AbstractShaderProperty CreateGradientProperty(string name, Gradient defaultValue = null)
        {
            var propType = ShaderGraphReflectionHelper.FindType(" UnityEditor.ShaderGraph.GradientShaderProperty");
            if (propType == null) throw new Exception("找不到 GradientShaderProperty 类型");
            var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

            ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
            ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

            if (defaultValue != null)
            {
                var valueProp = propType.GetProperty("value");
                valueProp?.SetValue(prop, defaultValue);
            }
            return prop;
        }

        public static AbstractShaderProperty CreateVirtualTextureProperty(string name, int layers = 1)
        {
            var propType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.VirtualTextureShaderProperty");
            if (propType == null) throw new Exception("找不到 VirtualTextureShaderProperty 类型");
            var prop = Activator.CreateInstance(propType, true) as AbstractShaderProperty;

            ShaderGraphReflectionHelper.SetInternalProperty(prop, "displayName", name);
            ShaderGraphReflectionHelper.SetInternalProperty(prop, "overrideReferenceName", name);

            var valueProp = propType.GetProperty("value");
            if (valueProp != null)
            {
                var vtValue = valueProp.GetValue(prop);
                var layersField = vtValue?.GetType().GetField("layers");
                if (layersField != null)
                {
                    var listType = typeof(System.Collections.Generic.List<>);
                    var layerType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.Internal.SerializableVirtualTextureLayer");
                    if (layerType != null)
                    {
                        var genericList = listType.MakeGenericType(layerType);
                        var list = Activator.CreateInstance(genericList);
                        for (int i = 0; i < layers; i++)
                        {
                            var layer = Activator.CreateInstance(layerType);
                            var nameField = layerType.GetField("name");
                            nameField?.SetValue(layer, $"Layer{i}");
                            var layerRefNameField = layerType.GetField("layerRefName");
                            layerRefNameField?.SetValue(layer, $"{name}_Layer{i}");
                            var addMethod = genericList.GetMethod("Add");
                            addMethod?.Invoke(list, new[] { layer });
                        }
                        layersField.SetValue(vtValue, list);
                    }
                }
            }
            return prop;
        }
    }
}