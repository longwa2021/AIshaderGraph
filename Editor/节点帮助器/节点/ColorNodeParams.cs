using UnityEngine;
using UnityEditor.ShaderGraph.Internal;
using System;
using System.Collections.Generic;

namespace 龙哥的秘密花园.节点库
{
    // ============================================================
    // 有参数的节点参数类（包含字段）
    // ============================================================

    [Serializable]
    public class ColorNodeParams
    {
        public Color color = Color.white;
        public ColorMode mode = ColorMode.Default;
    }

    [Serializable]
    public class ChannelMaskNodeParams
    {
        // 位掩码：bit0=Red, bit1=Green, bit2=Blue, bit3=Alpha，默认 -1 表示全部通道
        public int channelMask = -1;
    }

    [Serializable]
    public class NormalBlendNodeParams
    {
        public NormalBlendModeOption blendMode = NormalBlendModeOption.Default;
    }

    [Serializable]
    public class NormalFromHeightNodeParams
    {
        public OutputSpaceOption outputSpace = OutputSpaceOption.Tangent;
    }

    [Serializable]
    public class NormalUnpackNodeParams
    {
        public NormalMapSpaceOption normalMapSpace = NormalMapSpaceOption.Tangent;
    }

    [Serializable]
    public class ColorspaceConversionNodeParams
    {
        public ColorspaceOption from = ColorspaceOption.RGB;
        public ColorspaceOption to = ColorspaceOption.RGB;
    }

    [Serializable]
    public class FlipNodeParams
    {
        public bool flipRed = false;
        public bool flipGreen = false;
        public bool flipBlue = false;
        public bool flipAlpha = false;
    }

    [Serializable]
    public class SwizzleNodeParams
    {
        // 掩码字符串，例如 "xyzw", "rgba", "x", "yz" 等
        public string mask = "xxxx";
    }

    [Serializable]
    public class ExponentialNodeParams
    {
        public ExponentialBaseOption exponentialBase = ExponentialBaseOption.BaseE;
    }

    [Serializable]
    public class LogNodeParams
    {
        public LogBaseOption logBase = LogBaseOption.BaseE;
    }

    [Serializable]
    public class ReciprocalNodeParams
    {
        public ReciprocalMethodOption method = ReciprocalMethodOption.Default;
    }

    [Serializable]
    public class MatrixConstructionNodeParams
    {
        public MatrixAxisOption axis = MatrixAxisOption.Row;
    }
    [Serializable]
    public class LightTextureNodeParams
    {
        public BlendStyle blendStyle=BlendStyle.LightTex0;
    }
    [Serializable]
    public class MatrixSplitNodeParams
    {
        
        public MatrixAxisOption axis = MatrixAxisOption.Row;
    }

    [Serializable]
    public class BooleanNodeParams
    {
        public bool value = false;
    }

    [Serializable]
    public class ConstantNodeParams
    {
        public ConstantTypeOption constant = ConstantTypeOption.PI;
    }

    [Serializable]
    public class IntegerNodeParams
    {
        public int value = 0;
    }

    [Serializable]
    public class SliderNodeParams
    {
        public Vector3 value = new Vector3(0f, 0f, 1f);   // x=当前值, y=最小值, z=最大值
    }

    [Serializable]
    public class UniversalSampleBufferNodeParams
    {
        public BufferType bufferType = BufferType.NormalWorldSpace;
        public ScreenSpaceType _ScreenPosition = ScreenSpaceType.Default;
    }
    [Serializable]
    public class Vector1NodeParams
    {
        public float value = 0f;
    }

    [Serializable]
    public class Vector2NodeParams
    {
        public Vector2 value = Vector2.zero;
    }

    [Serializable]
    public class Vector3NodeParams
    {
        public Vector3 value = Vector3.zero;
    }

    [Serializable]
    public class Vector4NodeParams
    {
        public Vector4 value = Vector4.zero;
    }

    [Serializable]
    public class ChannelMixerNodeParams
    {
        public Vector3 outRed = new Vector3(1, 0, 0);
        public Vector3 outGreen = new Vector3(0, 1, 0);
        public Vector3 outBlue = new Vector3(0, 0, 1);
    }

    [Serializable]
    public class HueNodeParams
    {
        public HueModeOption mode = HueModeOption.Degrees;
    }

    [Serializable]
    public class InvertColorsNodeParams
    {
        public bool invertRed = false;
        public bool invertGreen = false;
        public bool invertBlue = false;
        public bool invertAlpha = false;
    }

    [Serializable]
    public class BlendNodeParams
    {
        public BlendOperationMode blendMode = BlendOperationMode.Overlay;
    }

    [Serializable]
    public class PositionNodeParams
    {
        public PositionSourceOption positionSource = PositionSourceOption.Default;
        public CoordinateSpaceOption space = CoordinateSpaceOption.Object; // 默认 Object 空间适合顶点挤出
    }

    [Serializable]
    public class ScreenPositionNodeParams
    {
        public ScreenSpaceTypeOption screenSpaceType = ScreenSpaceTypeOption.Default;
    }

    [Serializable]
    public class UVNodeParams
    {
        public UVChannelOption uvChannel = UVChannelOption.UV0;
    }

    [Serializable]
    public class ViewVectorNodeParams
    {
        public CoordinateSpaceOption space = CoordinateSpaceOption.World;
    }

    [Serializable]
    public class BakedGINodeParams
    {
        public bool applyScaling = true;
    }

    [Serializable]
    public class Matrix2NodeParams
    {
        public Vector2 row0 = new Vector2(1, 0);
        public Vector2 row1 = new Vector2(0, 1);
    }

    [Serializable]
    public class Matrix3NodeParams
    {
        public Vector3 row0 = new Vector3(1, 0, 0);
        public Vector3 row1 = new Vector3(0, 1, 0);
        public Vector3 row2 = new Vector3(0, 0, 1);
    }

    [Serializable]
    public class Matrix4NodeParams
    {
        public Vector4 row0 = new Vector4(1, 0, 0, 0);
        public Vector4 row1 = new Vector4(0, 1, 0, 0);
        public Vector4 row2 = new Vector4(0, 0, 1, 0);
        public Vector4 row3 = new Vector4(0, 0, 0, 1);
    }

    [Serializable]
    public class DielectricSpecularNodeParams
    {
        public DielectricMaterialTypeOption type = DielectricMaterialTypeOption.Common;
        public float range = 0.5f;
        public float indexOfRefraction = 1.0f;
    }

    [Serializable]
    public class MetalReflectanceNodeParams
    {
        public MetalMaterialTypeOption material = MetalMaterialTypeOption.Iron;
    }

    [Serializable]
    public class SceneDepthNodeParams
    {
        public DepthSamplingModeOption depthSamplingMode = DepthSamplingModeOption.Linear01;
    }

    [Serializable]
    public class SceneDepthDifferenceNodeParams
    {
        public DepthSamplingModeOption depthSamplingMode = DepthSamplingModeOption.Linear01;
    }

#if PROCEDURAL_VT_IN_GRAPH
    [Serializable]
    public class ProceduralVirtualTextureNodeParams
    {
        public string vtName = "ProceduralVirtualTexture";
        public int layers = 2;
        public HLSLDeclarationOption shaderDeclaration = HLSLDeclarationOption.UnityPerMaterial;
    }
#endif

    [Serializable]
    public class CustomInterpolatorNodeParams
    {
        public string customBlockNodeName = "K_INVALID";
        public CustomBlockTypeOption customWidth = CustomBlockTypeOption.Vector4;
    }

    [Serializable]
    public class PropertyNodeParams
    {
        public string propertyReferenceName;
    }

    [Serializable]
    public class TransformationMatrixNodeParams
    {
        public UnityMatrixTypeOption matrixType = UnityMatrixTypeOption.Model;
    }

    [Serializable]
    public class SampleTexture2DNodeParams
    {
        public TextureTypeOption textureType = TextureTypeOption.Default;
        public NormalMapSpaceOption normalMapSpace = NormalMapSpaceOption.Tangent;
        public bool enableGlobalMipBias = true;
        public Texture2DMipSamplingModeOption mipSamplingMode = Texture2DMipSamplingModeOption.Standard;
    }

    [Serializable]
    public class SampleTexture2DLODNodeParams
    {
        public TextureTypeOption textureType = TextureTypeOption.Default;
        public NormalMapSpaceOption normalMapSpace = NormalMapSpaceOption.Tangent;
    }

    [Serializable]
    public class SampleTexture2DArrayNodeParams
    {
        public bool enableGlobalMipBias = false;
        public Texture2DMipSamplingModeOption mipSamplingMode = Texture2DMipSamplingModeOption.Standard;
    }

    [Serializable]
    public class SampleTexture3DNodeParams
    {
        public Texture3DMipSamplingModeOption mipSamplingMode = Texture3DMipSamplingModeOption.Standard;
    }

    [Serializable]
    public class SampleVirtualTextureNodeParams
    {
        public VirtualTextureAddressModeOption addressMode = VirtualTextureAddressModeOption.VtAddressMode_Wrap;
        public VirtualTextureLodCalculationOption lodCalculation = VirtualTextureLodCalculationOption.VtLevel_Automatic;
        public VirtualTextureQualityModeOption sampleQuality = VirtualTextureQualityModeOption.VtSampleQuality_High;
        public bool enableGlobalMipBias = true;
        public bool noFeedback = false;
    }

    [Serializable]
    public class SamplerStateNodeParams
    {
        public SamplerFilterModeOption filter = SamplerFilterModeOption.Linear;
        public SamplerWrapModeOption wrap = SamplerWrapModeOption.Repeat;
        public SamplerAnisotropicOption anisotropic = SamplerAnisotropicOption.None;
    }

    [Serializable]
    public class RefractNodeParams
    {
        public RefractModeOption mode = RefractModeOption.Safe;
    }

    [Serializable]
    public class RotateAboutAxisNodeParams
    {
        public RotationUnitOption unit = RotationUnitOption.Radians;
    }
    
    [Serializable]
    public class TransformNodeParams
    {
        public CoordinateSpaceOption from = CoordinateSpaceOption.Object;
        public CoordinateSpaceOption to = CoordinateSpaceOption.World;
        public ConversionTypeOption conversionType = ConversionTypeOption.Position;
        public bool normalize = true;
    }
    [Serializable]
    public class SlotDefinition
    {
        public string DisplayName;
        public string ShaderOutputName;
        public SlotValueType ValueType;
        public bool IsInput;
        public Vector4 DefaultValue;
        public ShaderStageCapability StageCapability = ShaderStageCapability.All;
    }

    [Serializable]
    public class GradientNoiseNodeParams
    {
        public HashTypeOption hashType = HashTypeOption.Deterministic;
    }

    [Serializable]
    public class SimpleNoiseNodeParams
    {
        public HashTypeOption hashType = HashTypeOption.Deterministic;
    }

    [Serializable]
    public class VoronoiNodeParams
    {
        public HashTypeOption hashType = HashTypeOption.Deterministic;
    }

    [Serializable]
    public class RectangleNodeParams
    {
        public ClampTypeOption clampType = ClampTypeOption.Fastest;
    }

    [Serializable]
    public class ComparisonNodeParams
    {
        public ComparisonTypeOption comparisonType = ComparisonTypeOption.Equal;
    }

    [Serializable]
    public class ParallaxMappingNodeParams
    {
        public ChannelOption channel = ChannelOption.Green;
    }

    [Serializable]
    public class ParallaxOcclusionMappingNodeParams
    {
        public ChannelOption channel = ChannelOption.Red;
    }

    [Serializable]
    public class RotateNodeParams
    {
        public RotationUnitOption unit = RotationUnitOption.Radians;
    }

    [Serializable]
    public class TriplanarNodeParams
    {
        public TextureTypeOption textureType = TextureTypeOption.Default;
        public CoordinateSpaceOption inputSpace = CoordinateSpaceOption.AbsoluteWorld;
        public CoordinateSpaceOption normalOutputSpace = CoordinateSpaceOption.Tangent;
    }

    [Serializable]
    public class FlipbookNodeParams
    {
        public bool invertX = false;
        public bool invertY = true;
    }

    [Serializable]
    public class CustomFunctionNodeParams
    {
        public HlslSourceTypeOption sourceType = HlslSourceTypeOption.File;
        public string functionName = "Enter function name here...";
        public string functionSource = "Enter function source file path here...";
        public string functionBody = "Enter function body here...";
        public bool usePragmas = true;
        public List<SlotDefinition> Slots; 
    }
    [Serializable]
    public class SubGraphNodeParams
    {
        /// <summary>
        /// 子图标识符。可以是：
        /// - GUID
        /// - 资产名称
        /// - 相对路径
        /// - 注册的别名
        /// </summary>
        public string identifier;

        /// <summary>
        /// 标识符类型。默认为 Auto，尝试自动检测。
        /// </summary>
        public SubGraphIdentifierType identifierType = SubGraphIdentifierType.Auto;
    }
    [Serializable]
    public class CalculateLevelOfDetailTexture2DNodeParams
    {
        public bool clamp = true;
    }

    // ============================================================
    // 无参数的节点参数类（空类，仅占位）
    // ============================================================

    [Serializable]
    public class DitherNodeParams { }

    [Serializable]
    public class FadeTransitionNodeParams { }

    [Serializable]
    public class ColorMaskNodeParams { }

    [Serializable]
    public class NormalFromTextureNodeParams { }

    [Serializable]
    public class NormalReconstructZNodeParams { }

    [Serializable]
    public class NormalStrengthNodeParams { }

    [Serializable]
    public class CombineNodeParams { }

    [Serializable]
    public class SplitNodeParams { }

    [Serializable]
    public class AbsoluteNodeParams { }

    [Serializable]
    public class LengthNodeParams { }

    [Serializable]
    public class ModuloNodeParams { }

    [Serializable]
    public class NegateNodeParams { }

    [Serializable]
    public class NormalizeNodeParams { }

    [Serializable]
    public class PosterizeNodeParams { }

    [Serializable]
    public class DivideNodeParams { }

    [Serializable]
    public class MultiplyNodeParams { }

    [Serializable]
    public class PowerNodeParams { }

    [Serializable]
    public class SquareRootNodeParams { }

    [Serializable]
    public class SubtractNodeParams { }

    [Serializable]
    public class DDXNodeParams { }

    [Serializable]
    public class DDXYNodeParams { }

    [Serializable]
    public class DDYNodeParams { }

    [Serializable]
    public class InverseLerpNodeParams { }

    [Serializable]
    public class LerpNodeParams { }

    [Serializable]
    public class SmoothstepNodeParams { }

    [Serializable]
    public class MatrixDeterminantNodeParams { }

    [Serializable]
    public class MatrixTransposeNodeParams { }

    [Serializable]
    public class ClampNodeParams { }

    [Serializable]
    public class FractionNodeParams { }

    [Serializable]
    public class MaximumNodeParams { }

    [Serializable]
    public class MinimumNodeParams { }

    [Serializable]
    public class OneMinusNodeParams { }

    [Serializable]
    public class RandomRangeNodeParams { }

    [Serializable]
    public class RemapNodeParams { }

    [Serializable]
    public class SaturateNodeParams { }

    [Serializable]
    public class CeilingNodeParams { }

    [Serializable]
    public class FloorNodeParams { }

    [Serializable]
    public class RoundNodeParams { }

    [Serializable]
    public class SignNodeParams { }

    [Serializable]
    public class StepNodeParams { }

    [Serializable]
    public class TruncateNodeParams { }

    [Serializable]
    public class SineNodeParams { }

    [Serializable]
    public class TangentNodeParams { }

    [Serializable]
    public class ArccosineNodeParams { }

    [Serializable]
    public class ArcsineNodeParams { }

    [Serializable]
    public class Arctangent2NodeParams { }

    [Serializable]
    public class ArctangentNodeParams { }

    [Serializable]
    public class CosineNodeParams { }

    [Serializable]
    public class DegreesToRadiansNodeParams { }

    [Serializable]
    public class HyperbolicCosineNodeParams { }

    [Serializable]
    public class HyperbolicSineNodeParams { }

    [Serializable]
    public class HyperbolicTangentNodeParams { }

    [Serializable]
    public class RadiansToDegreesNodeParams { }

    [Serializable]
    public class ReciprocalSquareRootNodeParams { }

    [Serializable]
    public class AddNodeParams { }

    [Serializable]
    public class TimeNodeParams { }

    [Serializable]
    public class ContrastNodeParams { }

    [Serializable]
    public class ReplaceColorNodeParams { }

    [Serializable]
    public class SaturationNodeParams { }

    [Serializable]
    public class WhiteBalanceNodeParams { }

    [Serializable]
    public class VertexColorNodeParams { }

    [Serializable]
    public class VertexIDNodeParams { }

    [Serializable]
    public class ViewDirectionNodeParams
    {
        public CoordinateSpaceOption space = CoordinateSpaceOption.World; // ViewDirection 通常使用 World 空间
    }

    [Serializable]
    public class BitangentVectorNodeParams
    {
        public CoordinateSpaceOption space = CoordinateSpaceOption.Object;
    }

    [Serializable]
    public class InstanceIDNodeParams { }

    [Serializable]
    public class NormalVectorNodeParams
    {
        public CoordinateSpaceOption space = CoordinateSpaceOption.Object;
    }

    [Serializable]
    public class TangentVectorNodeParams
    {
        public CoordinateSpaceOption space = CoordinateSpaceOption.Object;
    }

    [Serializable]
    public class BlackbodyNodeParams { }

    [Serializable]
    public class GradientNodeParams { }

    [Serializable]
    public class SampleGradientNodeParams { }

    [Serializable]
    public class AmbientNodeParams { }

    [Serializable]
    public class MainLightDirectionNodeParams { }

    [Serializable]
    public class ReflectionProbeNodeParams { }

    [Serializable]
    public class Texture2DAssetNodeParams { }

    [Serializable]
    public class Texture2DArrayAssetNodeParams { }

    [Serializable]
    public class Texture3DAssetNodeParams { }

    [Serializable]
    public class CubemapAssetNodeParams { }

    [Serializable]
    public class SampleCubemapNodeParams { }

    [Serializable]
    public class SampleRawCubemapNodeParams { }

    [Serializable]
    public class GatherTexture2DNodeParams { }

    [Serializable]
    public class TexelSizeNodeParams { }

    [Serializable]
    public class CameraNodeParams { }

    [Serializable]
    public class EyeIndexNodeParams { }

    [Serializable]
    public class FogNodeParams { }

    [Serializable]
    public class ObjectNodeParams { }

    [Serializable]
    public class SceneColorNodeParams { }

    [Serializable]
    public class ScreenNodeParams { }

    [Serializable]
    public class CrossProductNodeParams { }

    [Serializable]
    public class DistanceNodeParams { }

    [Serializable]
    public class DotProductNodeParams { }

    [Serializable]
    public class FresnelEffectNodeParams { }
    // ===== UGUI 节点（无参数） =====
    [Serializable]
    public class MeterValueNodeParams { }

    [Serializable]
    public class RangeBarValueNodeParams { }

    [Serializable]
    public class RectTransformSizeNodeParams { }

    [Serializable]
    public class SelectableBranchNodeParams { }

    [Serializable]
    public class SelectableStateNodeParams { }

    [Serializable]
    public class SliderValueNodeParams { }

    [Serializable]
    public class ToggleStateNodeParams { }

    [Serializable]
    public class ProjectionNodeParams { }

    [Serializable]
    public class ReflectionNodeParams { }

    [Serializable]
    public class RejectionNodeParams { }

    [Serializable]
    public class SphereMaskNodeParams { }

    [Serializable]
    public class ComputeDeformNodeParams { }

    [Serializable]
    public class LinearBlendSkinningNodeParams { }

    [Serializable]
    public class EllipseNodeParams { }

    [Serializable]
    public class PolygonNodeParams { }

    [Serializable]
    public class RoundedPolygonNodeParams { }

    [Serializable]
    public class RoundedRectangleNodeParams { }

    [Serializable]
    public class CheckerboardNodeParams { }

    [Serializable]
    public class OrNodeParams { }

    [Serializable]
    public class AllNodeParams { }

    [Serializable]
    public class AndNodeParams { }

    [Serializable]
    public class AnyNodeParams { }

    [Serializable]
    public class BranchNodeParams { }

    [Serializable]
    public class BranchOnInputConnectionNodeParams { }

    [Serializable]
    public class PolarCoordinatesNodeParams { }

    [Serializable]
    public class RadialShearNodeParams { }

    [Serializable]
    public class SpherizeNodeParams { }

    [Serializable]
    public class TilingAndOffsetNodeParams { }

    [Serializable]
    public class TwirlNodeParams { }

    [Serializable]
    public class DropdownNodeParams { }

    [Serializable]
    public class KeywordNodeParams { }

    [Serializable]
    public class PreviewNodeParams { }

    [Serializable]
    public class SplitTextureTransformNodeParams { }

    [Serializable]
    public class IsFrontFaceNodeParams { }

    [Serializable]
    public class IsInfiniteNodeParams { }

    [Serializable]
    public class IsNanNodeParams { }

    [Serializable]
    public class NandNodeParams { }

    [Serializable]
    public class NotNodeParams { }

    [Serializable]
    public class NoiseSineWaveNodeParams { }

    [Serializable]
    public class SawtoothWaveNodeParams { }

    [Serializable]
    public class SquareWaveNodeParams { }

    [Serializable]
    public class TriangleWaveNodeParams { }

    
}
