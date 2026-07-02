using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.ShaderGraph.Internal;

namespace 龙哥的秘密花园.节点库
{
    /// <summary>
    /// 节点参数转换器：将自定义参数对象转换为属性字典，供反射设置使用。
    /// </summary>
    internal static class NodeParameterConverter
    {
        public static Dictionary<string, object> ConvertToProperties(ShaderGraphNodeType nodeType, object parameters)
        {
            var dict = new Dictionary<string, object>();
            if (parameters == null) return dict;

            switch (nodeType)
            {
                
                
                // ===== 基础输入节点 =====
                case ShaderGraphNodeType.ColorNode:
                    var colorParams = parameters as ColorNodeParams;
                    if (colorParams != null)
                    {
                        dict["color.color"] = colorParams.color;
                        dict["color.mode"] = colorParams.mode;
                    }
                    break;

                case ShaderGraphNodeType.BooleanNode:
                    var boolParams = parameters as BooleanNodeParams;
                    if (boolParams != null)
                        dict["m_Value"] = boolParams.value;
                    break;

                case ShaderGraphNodeType.ConstantNode:
                    var constParams = parameters as ConstantNodeParams;
                    if (constParams != null)
                        dict["m_constant"] = GetConstantTypeValue(constParams.constant);
                    break;

                case ShaderGraphNodeType.IntegerNode:
                    var intParams = parameters as IntegerNodeParams;
                    if (intParams != null)
                        dict["m_Value"] = intParams.value;
                    break;

                case ShaderGraphNodeType.SliderNode:
                    var sliderParams = parameters as SliderNodeParams;
                    if (sliderParams != null)
                        dict["m_Value"] = sliderParams.value;
                    break;

                case ShaderGraphNodeType.Vector1Node:
                    var v1Params = parameters as Vector1NodeParams;
                    if (v1Params != null)
                        dict["m_Value"] = v1Params.value;
                    break;

                case ShaderGraphNodeType.Vector2Node:
                    var v2Params = parameters as Vector2NodeParams;
                    if (v2Params != null)
                        dict["m_Value"] = v2Params.value;
                    break;

                case ShaderGraphNodeType.Vector3Node:
                    var v3Params = parameters as Vector3NodeParams;
                    // if (v3Params != null)
                    //     dict["m_Value"] = v3Params.value;
                    if (v3Params != null)
                    {
                        // Debug.Log($"[Vector3Node] 解析参数: value = {v3Params.value}");
                        dict["m_Value"] = v3Params.value;
                    }
                    break;

                case ShaderGraphNodeType.Vector4Node:
                    var v4Params = parameters as Vector4NodeParams;
                    if (v4Params != null)
                        dict["m_Value"] = v4Params.value;
                    break;

                // ===== 坐标/空间节点 =====
                case ShaderGraphNodeType.PositionNode:
                    var posParams = parameters as PositionNodeParams;
                    if (posParams != null)
                    {
                        dict["m_PositionSource"] = posParams.positionSource.ToString(); // 直接字符串
                        dict["m_Space"] = posParams.space.ToString(); // 直接字符串
                    }
                    break;

                case ShaderGraphNodeType.ScreenPositionNode:
                    var screenParams = parameters as ScreenPositionNodeParams;
                    if (screenParams != null)
                        dict["m_ScreenSpaceType"] = GetEnumValue("UnityEditor.ShaderGraph.ScreenSpaceType", screenParams.screenSpaceType.ToString());
                    break;

                case ShaderGraphNodeType.UVNode:
                    var uvParams = parameters as UVNodeParams;
                    if (uvParams != null)
                        dict["m_OutputChannel"] = GetEnumValue("UnityEditor.ShaderGraph.Internal.UVChannel", uvParams.uvChannel.ToString());
                    break;

                    
                // ===== 自定义插值器与属性节点 =====
                case ShaderGraphNodeType.CustomInterpolatorNode:
                    var customParams = parameters as CustomInterpolatorNodeParams;
                    if (customParams != null)
                    {
                        dict["customBlockNodeName"] = customParams.customBlockNodeName;
                        dict["serializedType"] = GetEnumValue("UnityEditor.ShaderGraph.BlockNode.CustomBlockType", customParams.customWidth.ToString());
                    }
                    break;
                case ShaderGraphNodeType.UniversalSampleBufferNode:
                    var UniversalSampleBuffer = parameters as UniversalSampleBufferNodeParams;
                    if (UniversalSampleBuffer != null)
                    {
                        
                        dict["bufferType"] =GetEnumValue("UnityEditor.Rendering.Universal.UniversalSampleBufferNode.BufferType",UniversalSampleBuffer.bufferType.ToString()) ;
                        dict["_ScreenPosition"] = GetEnumValue("UnityEditor.ShaderGraph.ScreenSpaceType", UniversalSampleBuffer._ScreenPosition.ToString());
                    }
                    break;
                case ShaderGraphNodeType.PropertyNode:
                    var propParams = parameters as PropertyNodeParams;
                    if (propParams != null && !string.IsNullOrEmpty(propParams.propertyReferenceName))
                        dict["_Temp_PropertyRefName"] = propParams.propertyReferenceName;
                    break;

                // ===== 光照与 GI 节点 =====
                case ShaderGraphNodeType.BakedGINode:
                    var bakedParams = parameters as BakedGINodeParams;
                    if (bakedParams != null)
                        dict["m_ApplyScaling"] = bakedParams.applyScaling;
                    break;

                // ===== 矩阵节点 =====
                case ShaderGraphNodeType.Matrix2Node:
                    var m2Params = parameters as Matrix2NodeParams;
                    if (m2Params != null)
                    {
                        dict["m_Row0"] = m2Params.row0;
                        dict["m_Row1"] = m2Params.row1;
                    }
                    break;

                case ShaderGraphNodeType.Matrix3Node:
                    var m3Params = parameters as Matrix3NodeParams;
                    if (m3Params != null)
                    {
                        dict["m_Row0"] = m3Params.row0;
                        dict["m_Row1"] = m3Params.row1;
                        dict["m_Row2"] = m3Params.row2;
                    }
                    break;

                case ShaderGraphNodeType.Matrix4Node:
                    var m4Params = parameters as Matrix4NodeParams;
                    if (m4Params != null)
                    {
                        dict["m_Row0"] = m4Params.row0;
                        dict["m_Row1"] = m4Params.row1;
                        dict["m_Row2"] = m4Params.row2;
                        dict["m_Row3"] = m4Params.row3;
                    }
                    break;

                case ShaderGraphNodeType.TransformationMatrixNode:
                    var transParams = parameters as TransformationMatrixNodeParams;
                    if (transParams != null)
                        dict["m_MatrixType"] = GetEnumValue("UnityEditor.ShaderGraph.UnityMatrixType", transParams.matrixType.ToString());
                    break;

                case ShaderGraphNodeType.MatrixConstructionNode:
                    var matrixConstParams = parameters as MatrixConstructionNodeParams;
                    if (matrixConstParams != null)
                        dict["m_Axis"] = GetEnumValue("UnityEditor.ShaderGraph.MatrixAxis", matrixConstParams.axis.ToString());
                    break;

                case ShaderGraphNodeType.MatrixSplitNode:
                    var matrixSplitParams = parameters as MatrixSplitNodeParams;
                    if (matrixSplitParams != null)
                        dict["m_Axis"] = GetEnumValue("UnityEditor.ShaderGraph.MatrixAxis", matrixSplitParams.axis.ToString());
                    break;

                // ===== PBR 材质节点 =====
                case ShaderGraphNodeType.DielectricSpecularNode:
                    var dielParams = parameters as DielectricSpecularNodeParams;
                    if (dielParams != null)
                    {
                        var internalType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.DielectricSpecularNode+DielectricMaterial");
                        if (internalType != null)
                        {
                            var internalEnum = GetEnumValue("UnityEditor.ShaderGraph.DielectricMaterialType", dielParams.type.ToString());
                            var structObj = Activator.CreateInstance(internalType, internalEnum, dielParams.range, dielParams.indexOfRefraction);
                            dict["m_Material"] = structObj;
                        }
                    }
                    break;

                case ShaderGraphNodeType.MetalReflectanceNode:
                    var metalParams = parameters as MetalReflectanceNodeParams;
                    if (metalParams != null)
                        dict["m_Material"] = GetEnumValue("UnityEditor.ShaderGraph.MetalMaterialType", metalParams.material.ToString());
                    break;

                // ===== 场景深度节点 =====
                case ShaderGraphNodeType.SceneDepthNode:
                    var sdParams = parameters as SceneDepthNodeParams;
                    if (sdParams != null)
                        dict["m_DepthSamplingMode"] = GetEnumValue("UnityEditor.ShaderGraph.DepthSamplingMode", sdParams.depthSamplingMode.ToString());
                    break;

                case ShaderGraphNodeType.SceneDepthDifferenceNode:
                    var sddParams = parameters as SceneDepthDifferenceNodeParams;
                    if (sddParams != null)
                        dict["m_DepthSamplingMode"] = GetEnumValue("UnityEditor.ShaderGraph.DepthSamplingMode", sddParams.depthSamplingMode.ToString());
                    break;

                // ===== 纹理采样节点 =====
                case ShaderGraphNodeType.SampleTexture2DNode:
                    var tex2DParams = parameters as SampleTexture2DNodeParams;
                    if (tex2DParams != null)
                    {
                        dict["m_TextureType"] = GetEnumValue("UnityEditor.ShaderGraph.TextureType", tex2DParams.textureType.ToString());
                        dict["m_NormalMapSpace"] = GetEnumValue("UnityEditor.ShaderGraph.NormalMapSpace", tex2DParams.normalMapSpace.ToString());
                        dict["m_EnableGlobalMipBias"] = tex2DParams.enableGlobalMipBias;
                        dict["m_MipSamplingMode"] = GetEnumValue("UnityEditor.ShaderGraph.Texture2DMipSamplingMode", tex2DParams.mipSamplingMode.ToString());
                    }
                    break;

                case ShaderGraphNodeType.SampleTexture2DLODNode:
                    var lodParams = parameters as SampleTexture2DLODNodeParams;
                    if (lodParams != null)
                    {
                        dict["m_TextureType"] = GetEnumValue("UnityEditor.ShaderGraph.TextureType", lodParams.textureType.ToString());
                        dict["m_NormalMapSpace"] = GetEnumValue("UnityEditor.ShaderGraph.NormalMapSpace", lodParams.normalMapSpace.ToString());
                    }
                    break;

                case ShaderGraphNodeType.SampleTexture2DArrayNode:
                    var arrayParams = parameters as SampleTexture2DArrayNodeParams;
                    if (arrayParams != null)
                    {
                        dict["m_EnableGlobalMipBias"] = arrayParams.enableGlobalMipBias;
                        dict["m_MipSamplingMode"] = GetEnumValue("UnityEditor.ShaderGraph.Texture2DMipSamplingMode", arrayParams.mipSamplingMode.ToString());
                    }
                    break;

                case ShaderGraphNodeType.SampleTexture3DNode:
                    var tex3DParams = parameters as SampleTexture3DNodeParams;
                    if (tex3DParams != null)
                        dict["m_MipSamplingMode"] = GetEnumValue("UnityEditor.ShaderGraph.Texture3DMipSamplingMode", tex3DParams.mipSamplingMode.ToString());
                    break;

                case ShaderGraphNodeType.SampleVirtualTextureNode:
                    var vtParams = parameters as SampleVirtualTextureNodeParams;
                    if (vtParams != null)
                    {
                        dict["m_AddressMode"] = GetEnumValue("UnityEditor.ShaderGraph.SampleVirtualTextureNode+AddressMode", vtParams.addressMode.ToString());
                        dict["m_LodCalculation"] = GetEnumValue("UnityEditor.ShaderGraph.SampleVirtualTextureNode+LodCalculation", vtParams.lodCalculation.ToString());
                        dict["m_SampleQuality"] = GetEnumValue("UnityEditor.ShaderGraph.SampleVirtualTextureNode+QualityMode", vtParams.sampleQuality.ToString());
                        dict["m_EnableGlobalMipBias"] = vtParams.enableGlobalMipBias;
                        dict["m_NoFeedback"] = vtParams.noFeedback;
                    }
                    break;

                case ShaderGraphNodeType.SamplerStateNode:
                    var samplerParams = parameters as SamplerStateNodeParams;
                    if (samplerParams != null)
                    {
                        dict["m_filter"] = ShaderGraphReflectionHelper.ConvertToInternalEnum("FilterMode", samplerParams.filter);
                        dict["m_wrap"] = ShaderGraphReflectionHelper.ConvertToInternalEnum("WrapMode", samplerParams.wrap);
                        dict["m_aniso"] = ShaderGraphReflectionHelper.ConvertToInternalEnum("Anisotropic", samplerParams.anisotropic);
                    }
                    break;

                case ShaderGraphNodeType.CalculateLevelOfDetailTexture2DNode:
                    var lodCalcParams = parameters as CalculateLevelOfDetailTexture2DNodeParams;
                    if (lodCalcParams != null)
                        dict["m_Clamp"] = lodCalcParams.clamp;
                    break;

                // ===== 艺术效果节点 =====
                case ShaderGraphNodeType.ChannelMixerNode:
                    var channelParams = parameters as ChannelMixerNodeParams;
                    if (channelParams != null)
                    {
                        var internalChannelMixerType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.ChannelMixerNode+ChannelMixer");
                        if (internalChannelMixerType != null)
                        {
                            var mixer = Activator.CreateInstance(internalChannelMixerType,
                                channelParams.outRed, channelParams.outGreen, channelParams.outBlue);
                            dict["m_ChannelMixer"] = mixer;
                        }
                    }
                    break;

                case ShaderGraphNodeType.HueNode:
                    var hueParams = parameters as HueNodeParams;
                    if (hueParams != null)
                    {
                        var hueModeEnum = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.HueMode");
                        if (hueModeEnum != null)
                        {
                            var modeValue = Enum.Parse(hueModeEnum, hueParams.mode.ToString());
                            dict["m_HueMode"] = modeValue;
                        }
                    }
                    break;

                case ShaderGraphNodeType.InvertColorsNode:
                    var invertParams = parameters as InvertColorsNodeParams;
                    if (invertParams != null)
                    {
                        dict["m_RedChannel"] = invertParams.invertRed;
                        dict["m_GreenChannel"] = invertParams.invertGreen;
                        dict["m_BlueChannel"] = invertParams.invertBlue;
                        dict["m_AlphaChannel"] = invertParams.invertAlpha;
                    }
                    break;

                case ShaderGraphNodeType.BlendNode:
                    var blendParams = parameters as BlendNodeParams;
                    if (blendParams != null)
                    {
                        var blendModeEnum = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.BlendMode");
                        if (blendModeEnum != null)
                        {
                            var modeValue = Enum.Parse(blendModeEnum, blendParams.blendMode.ToString());
                            dict["m_BlendMode"] = modeValue;
                        }
                    }
                    break;

                case ShaderGraphNodeType.FlipNode:
                    var flipParams = parameters as FlipNodeParams;
                    if (flipParams != null)
                    {
                        dict["m_RedChannel"] = flipParams.flipRed;
                        dict["m_GreenChannel"] = flipParams.flipGreen;
                        dict["m_BlueChannel"] = flipParams.flipBlue;
                        dict["m_AlphaChannel"] = flipParams.flipAlpha;
                    }
                    break;

                case ShaderGraphNodeType.SwizzleNode:
                    var swizzleParams = parameters as SwizzleNodeParams;
                    if (swizzleParams != null)
                        dict["_maskInput"] = swizzleParams.mask;
                    break;

                // ===== 数学节点 =====
                case ShaderGraphNodeType.ExponentialNode:
                    var expParams = parameters as ExponentialNodeParams;
                    if (expParams != null)
                        dict["m_ExponentialBase"] = GetEnumValue("UnityEditor.ShaderGraph.ExponentialBase", expParams.exponentialBase.ToString());
                    break;

                case ShaderGraphNodeType.LogNode:
                    var logParams = parameters as LogNodeParams;
                    if (logParams != null)
                        dict["m_LogBase"] = GetEnumValue("UnityEditor.ShaderGraph.LogBase", logParams.logBase.ToString());
                    break;

                case ShaderGraphNodeType.ReciprocalNode:
                    var recipParams = parameters as ReciprocalNodeParams;
                    if (recipParams != null)
                        dict["m_ReciprocalMethod"] = GetEnumValue("UnityEditor.ShaderGraph.ReciprocalMethod", recipParams.method.ToString());
                    break;

                case ShaderGraphNodeType.TransformNode:
                    var transformParams = parameters as TransformNodeParams;
                    if (transformParams != null)
                    {
                        var conversionType = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.CoordinateSpaceConversion");
                        if (conversionType != null)
                        {
                            var fromEnum = GetEnumValue("UnityEditor.ShaderGraph.Internal.CoordinateSpace", transformParams.from.ToString());
                            var toEnum = GetEnumValue("UnityEditor.ShaderGraph.Internal.CoordinateSpace", transformParams.to.ToString());
                            var conversion = Activator.CreateInstance(conversionType, fromEnum, toEnum);
                            dict["m_Conversion"] = conversion;
                        }
                        dict["m_ConversionType"] = GetEnumValue("UnityEditor.ShaderGraph.ConversionType", transformParams.conversionType.ToString());
                        dict["m_Normalize"] = transformParams.normalize;
                    }
                    break;

                case ShaderGraphNodeType.RefractNode:
                    var refractParams = parameters as RefractNodeParams;
                    if (refractParams != null)
                        dict["m_RefractMode"] = GetEnumValue("UnityEditor.ShaderGraph.RefractMode", refractParams.mode.ToString());
                    break;

                case ShaderGraphNodeType.RotateAboutAxisNode:
                    var rotateParams = parameters as RotateAboutAxisNodeParams;
                    if (rotateParams != null)
                        dict["m_Unit"] = GetEnumValue("UnityEditor.ShaderGraph.RotationUnit", rotateParams.unit.ToString());
                    break;

                // ===== 噪声节点 =====
                case ShaderGraphNodeType.GradientNoiseNode:
                    var gradNoiseParams = parameters as GradientNoiseNodeParams;
                    if (gradNoiseParams != null)
                    {
                        var hashTypeEnum = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.GradientNoiseNode+HashType");
                        if (hashTypeEnum != null)
                        {
                            string enumName = gradNoiseParams.hashType.ToString();
                            if (enumName == "LegacyMod") enumName = "LegacyMod";
                            else if (enumName == "LegacySine") enumName = "LegacyMod";
                            dict["m_HashType"] = Enum.Parse(hashTypeEnum, enumName);
                        }
                    }
                    break;

                case ShaderGraphNodeType.SimpleNoiseNode:
                    var simpleNoiseParams = parameters as SimpleNoiseNodeParams;
                    if (simpleNoiseParams != null)
                    {
                        var hashTypeEnum = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.NoiseNode+HashType");
                        if (hashTypeEnum != null)
                        {
                            string enumName = simpleNoiseParams.hashType.ToString();
                            if (enumName == "LegacySine") enumName = "LegacySine";
                            else if (enumName == "LegacyMod") enumName = "LegacySine";
                            dict["m_HashType"] = Enum.Parse(hashTypeEnum, enumName);
                        }
                    }
                    break;

                case ShaderGraphNodeType.VoronoiNode:
                    var voronoiParams = parameters as VoronoiNodeParams;
                    if (voronoiParams != null)
                    {
                        var hashTypeEnum = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.VoronoiNode+HashType");
                        if (hashTypeEnum != null)
                        {
                            string enumName = voronoiParams.hashType.ToString();
                            if (enumName == "LegacySine") enumName = "LegacySine";
                            else if (enumName == "LegacyMod") enumName = "LegacySine";
                            dict["m_HashType"] = Enum.Parse(hashTypeEnum, enumName);
                        }
                    }
                    break;

                // ===== UV/形状节点 =====
                case ShaderGraphNodeType.ParallaxMappingNode:
                    var parallaxParams = parameters as ParallaxMappingNodeParams;
                    if (parallaxParams != null)
                        dict["m_Channel"] = GetEnumValue("UnityEditor.ShaderGraph.Channel", parallaxParams.channel.ToString());
                    break;

                case ShaderGraphNodeType.ParallaxOcclusionMappingNode:
                    var pomParams = parameters as ParallaxOcclusionMappingNodeParams;
                    if (pomParams != null)
                        dict["m_Channel"] = GetEnumValue("UnityEditor.ShaderGraph.Channel", pomParams.channel.ToString());
                    break;

                case ShaderGraphNodeType.RotateNode:
                    var rotateUvParams = parameters as RotateNodeParams;
                    if (rotateUvParams != null)
                        dict["m_Unit"] = GetEnumValue("UnityEditor.ShaderGraph.RotationUnit", rotateUvParams.unit.ToString());
                    break;

                case ShaderGraphNodeType.TriplanarNode:
                    var triplanarParams = parameters as TriplanarNodeParams;
                    if (triplanarParams != null)
                    {
                        dict["m_TextureType"] = GetEnumValue("UnityEditor.ShaderGraph.TextureType", triplanarParams.textureType.ToString());
                        dict["m_InputSpace"] = GetEnumValue("UnityEditor.ShaderGraph.CoordinateSpace", triplanarParams.inputSpace.ToString());
                        dict["m_NormalOutputSpace"] = GetEnumValue("UnityEditor.ShaderGraph.CoordinateSpace", triplanarParams.normalOutputSpace.ToString());
                    }
                    break;

                case ShaderGraphNodeType.FlipbookNode:
                    var flipbookParams = parameters as FlipbookNodeParams;
                    if (flipbookParams != null)
                    {
                        dict["m_InvertX"] = flipbookParams.invertX;
                        dict["m_InvertY"] = flipbookParams.invertY;
                    }
                    break;

                case ShaderGraphNodeType.RectangleNode:
                    var rectParams = parameters as RectangleNodeParams;
                    if (rectParams != null)
                        dict["m_ClampType"] = GetEnumValue("UnityEditor.ShaderGraph.ClampType", rectParams.clampType.ToString());
                    break;

                case ShaderGraphNodeType.ComparisonNode:
                    var compareParams = parameters as ComparisonNodeParams;
                    if (compareParams != null)
                        dict["m_ComparisonType"] = GetEnumValue("UnityEditor.ShaderGraph.ComparisonType", compareParams.comparisonType.ToString());
                    break;

                // ===== 自定义函数节点 =====
                case ShaderGraphNodeType.CustomFunctionNode:
                    var cfParams = parameters as CustomFunctionNodeParams;
                    if (cfParams != null)
                    {
                        // 内部枚举 HlslSourceType
                        var hlslSourceType = EnumConverter.ToInternalEnum(cfParams.sourceType);
                        dict["m_SourceType"] = hlslSourceType;

                        // 函数名处理
                        string funcName = cfParams.functionName ?? "";
                        if (!string.IsNullOrEmpty(funcName) && funcName != "Enter function name here...")
                        {
                            // 注意：NodeUtils.ConvertToValidHLSLIdentifier 是内部方法，可通过反射调用
                            var nodeUtilsType = ShaderGraphReflectionHelper.FindType("UnityEditor.Graphing.NodeUtils");
                            var convertMethod = nodeUtilsType?.GetMethod("ConvertToValidHLSLIdentifier", BindingFlags.Public | BindingFlags.Static);
                            if (convertMethod != null)
                                funcName = (string)convertMethod.Invoke(null, new object[] { funcName, null });
                        }
                        dict["m_FunctionName"] = funcName;

                        // 文件 GUID 转换
                        string source = cfParams.functionSource ?? "";
                        if (cfParams.sourceType == HlslSourceTypeOption.File && !string.IsNullOrEmpty(source))
                        {
                            if (!GUID.TryParse(source, out _))
                            {
                                string guid = AssetDatabase.AssetPathToGUID(source);
                                if (!string.IsNullOrEmpty(guid))
                                    source = guid;
                            }
                        }
                        dict["m_FunctionSource"] = source;

                        dict["m_FunctionBody"] = cfParams.functionBody ?? "";
                        dict["m_FunctionSourceUsePragmas"] = cfParams.usePragmas;
                        if (cfParams.Slots != null && cfParams.Slots.Count > 0)
                        {
                            dict["_SlotsData"] = cfParams.Slots;
                        }
                    }
                    break;
                case ShaderGraphNodeType.LightTextureNode : 
                  var  LightTextureNodeParams = parameters as LightTextureNodeParams;
                  if (LightTextureNodeParams != null)
                  {
                      dict["m_BlendStyle"]=GetEnumValue("UnityEngine.Experimental.Rendering.Universal.BlendStyle", LightTextureNodeParams.blendStyle.ToString());
                  }
                  break;
                case ShaderGraphNodeType.SubGraphNode:
                    var subParams = parameters as SubGraphNodeParams;
                    if (subParams != null)
                    {
                        dict["_Temp_SubGraphIdentifier"] = subParams.identifier;
                        dict["_Temp_SubGraphIdentifierType"] = subParams.identifierType;
                    }
                    break;
                case ShaderGraphNodeType.NormalVectorNode:
                    var normParams = parameters as NormalVectorNodeParams;
                    if (normParams != null)
                    {
                        dict["m_Space"] = normParams.space.ToString();
                    }
                    break;
                case ShaderGraphNodeType.ViewDirectionNode:
                    var viewDirParams = parameters as ViewDirectionNodeParams;
                    if (viewDirParams != null)
                    {
                        dict["m_Space"] = viewDirParams.space.ToString();
                    }
                    break;
                case ShaderGraphNodeType.TangentVectorNode:
                    var tanParams = parameters as TangentVectorNodeParams;
                    if (tanParams != null)
                    {
                        dict["m_Space"] = tanParams.space.ToString();
                    }
                    break;

                case ShaderGraphNodeType.BitangentVectorNode:
                    var bitanParams = parameters as BitangentVectorNodeParams;
                    if (bitanParams != null)
                    {
                        dict["m_Space"] = bitanParams.space.ToString();
                    }
                    break;

                case ShaderGraphNodeType.ViewVectorNode:
                    var viewVecParams = parameters as ViewVectorNodeParams;
                    if (viewVecParams != null)
                    {
                        dict["m_Space"] = viewVecParams.space.ToString();
                    }
                    break;
                // ===== 无属性节点（仅占位，无需处理）=====
                case ShaderGraphNodeType.CombineNode:
                case ShaderGraphNodeType.SplitNode:
                case ShaderGraphNodeType.AbsoluteNode:
                case ShaderGraphNodeType.LengthNode:
                case ShaderGraphNodeType.ModuloNode:
                case ShaderGraphNodeType.NegateNode:
                case ShaderGraphNodeType.NormalizeNode:
                case ShaderGraphNodeType.PosterizeNode:
                case ShaderGraphNodeType.ReciprocalSquareRootNode:
                case ShaderGraphNodeType.WhiteBalanceNode:
                case ShaderGraphNodeType.ContrastNode:
                case ShaderGraphNodeType.SaturationNode:
                case ShaderGraphNodeType.ReplaceColorNode:
                case ShaderGraphNodeType.InstanceIDNode:
                case ShaderGraphNodeType.VertexColorNode:
                case ShaderGraphNodeType.PolarCoordinatesNode:
                case ShaderGraphNodeType.RadialShearNode:
                case ShaderGraphNodeType.SpherizeNode:
                case ShaderGraphNodeType.TilingAndOffsetNode:
                case ShaderGraphNodeType.TwirlNode:
                case ShaderGraphNodeType.DropdownNode:
                case ShaderGraphNodeType.KeywordNode:
                case ShaderGraphNodeType.PreviewNode:
                case ShaderGraphNodeType.SplitTextureTransformNode:
                case ShaderGraphNodeType.EllipseNode:
                case ShaderGraphNodeType.PolygonNode:
                case ShaderGraphNodeType.RoundedPolygonNode:
                case ShaderGraphNodeType.RoundedRectangleNode:
                case ShaderGraphNodeType.CheckerboardNode:
                case ShaderGraphNodeType.OrNode:
                case ShaderGraphNodeType.AllNode:
                case ShaderGraphNodeType.AndNode:
                case ShaderGraphNodeType.AnyNode:
                case ShaderGraphNodeType.BranchNode:
                case ShaderGraphNodeType.BranchOnInputConnectionNode:
                case ShaderGraphNodeType.IsFrontFaceNode:
                case ShaderGraphNodeType.IsInfiniteNode:
                case ShaderGraphNodeType.IsNanNode:
                case ShaderGraphNodeType.NandNode:
                case ShaderGraphNodeType.NotNode:
                case ShaderGraphNodeType.ComputeDeformNode:
                case ShaderGraphNodeType.LinearBlendSkinningNode:
                case ShaderGraphNodeType.VertexIDNode:
                case ShaderGraphNodeType.TimeNode:
                case ShaderGraphNodeType.BlackbodyNode:
                case ShaderGraphNodeType.GradientNode:
                case ShaderGraphNodeType.SampleGradientNode:
                case ShaderGraphNodeType.AmbientNode:
                case ShaderGraphNodeType.MainLightDirectionNode:
                case ShaderGraphNodeType.ReflectionProbeNode:
                case ShaderGraphNodeType.CameraNode:
                case ShaderGraphNodeType.EyeIndexNode:
                case ShaderGraphNodeType.FogNode:
                case ShaderGraphNodeType.ObjectNode:
                case ShaderGraphNodeType.SceneColorNode:
                case ShaderGraphNodeType.ScreenNode:
                case ShaderGraphNodeType.Texture2DAssetNode:
                case ShaderGraphNodeType.Texture2DArrayAssetNode:
                case ShaderGraphNodeType.Texture3DAssetNode:
                case ShaderGraphNodeType.CubemapAssetNode:
                case ShaderGraphNodeType.SampleCubemapNode:
                case ShaderGraphNodeType.SampleRawCubemapNode:
                case ShaderGraphNodeType.GatherTexture2DNode:
                case ShaderGraphNodeType.CrossProductNode:
                case ShaderGraphNodeType.DistanceNode:
                case ShaderGraphNodeType.DotProductNode:
                case ShaderGraphNodeType.FresnelEffectNode:
                case ShaderGraphNodeType.ProjectionNode:
                case ShaderGraphNodeType.ReflectionNode:
                case ShaderGraphNodeType.NoiseSineWaveNode:
                case ShaderGraphNodeType.SawtoothWaveNode:
                case ShaderGraphNodeType.SquareWaveNode:
                case ShaderGraphNodeType.TriangleWaveNode:
                case ShaderGraphNodeType.RejectionNode:
                case ShaderGraphNodeType.SphereMaskNode:
                case ShaderGraphNodeType.TexelSizeNode:
                case ShaderGraphNodeType.AddNode:
                case ShaderGraphNodeType.DivideNode:
                case ShaderGraphNodeType.MultiplyNode:
                case ShaderGraphNodeType.PowerNode:
                case ShaderGraphNodeType.SquareRootNode:
                case ShaderGraphNodeType.SubtractNode:
                case ShaderGraphNodeType.DDXNode:
                case ShaderGraphNodeType.DDXYNode:
                case ShaderGraphNodeType.DDYNode:
                case ShaderGraphNodeType.InverseLerpNode:
                case ShaderGraphNodeType.LerpNode:
                case ShaderGraphNodeType.SmoothstepNode:
                case ShaderGraphNodeType.MatrixDeterminantNode:
                case ShaderGraphNodeType.MatrixTransposeNode:
                case ShaderGraphNodeType.MeterValueNode:
                case ShaderGraphNodeType.RangeBarValueNode:
                case ShaderGraphNodeType.RectTransformSizeNode:
                case ShaderGraphNodeType.SelectableBranchNode:
                case ShaderGraphNodeType.SelectableStateNode:
                case ShaderGraphNodeType.SliderValueNode:
                case ShaderGraphNodeType.ToggleStateNode:
                    // 无属性节点，无需添加任何字典条目
                    break;

                default:
                    // 未知节点类型，不做处理
                    break;
            }

            return dict;
        }

        private static object GetConstantTypeValue(ConstantTypeOption option)
        {
            var constantTypeEnum = ShaderGraphReflectionHelper.FindType("UnityEditor.ShaderGraph.ConstantType");
            if (constantTypeEnum == null) return 0;
            return Enum.Parse(constantTypeEnum, option.ToString());
        }
        
        private static object GetEnumValue(string typeName, string enumName)
        {
            var enumType = ShaderGraphReflectionHelper.FindType(typeName);
            if (enumType == null)
            {
                Debug.LogError($"找不到枚举类型: {typeName}");
                return 0;
            }
            return Enum.Parse(enumType, enumName);
        }
    }
}