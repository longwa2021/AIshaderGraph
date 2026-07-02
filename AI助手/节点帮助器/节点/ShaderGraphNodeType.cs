using System;

namespace 龙哥的秘密花园.节点库
{
    /// <summary>
    /// Shader Graph 节点类型枚举
    /// </summary>
    public enum ShaderGraphNodeType
    {
        // ===== Input/Basic =====
        ColorNode,
        BooleanNode,
        ConstantNode,
        IntegerNode,
        SliderNode,
        TimeNode,
        Vector1Node,
        Vector2Node,
        Vector3Node,
        Vector4Node,

        // ===== Input/Geometry =====
        InstanceIDNode,
        NormalVectorNode,
        PositionNode,
        ScreenPositionNode,
        TangentVectorNode,
        UVNode,
        VertexColorNode,
        VertexIDNode,
        ViewDirectionNode,
        ViewVectorNode,
        BitangentVectorNode,

        // ===== Input/Gradient =====
        BlackbodyNode,
        GradientNode,
        SampleGradientNode,

        // ===== Input/Lighting =====
        AmbientNode,
        BakedGINode,
        MainLightDirectionNode,
        ReflectionProbeNode,

        // ===== Input/Matrix =====
        Matrix2Node,
        Matrix3Node,
        Matrix4Node,
        TransformationMatrixNode,

        // ===== Input/PBR =====
        DielectricSpecularNode,
        MetalReflectanceNode,

        // ===== Input/Scene =====
        CameraNode,
        EyeIndexNode,
        FogNode,
        ObjectNode,
        SceneColorNode,
        SceneDepthDifferenceNode,
        SceneDepthNode,
        ScreenNode,

        // ===== Input/Texture =====
        SampleTexture2DNode,
        SampleTexture2DLODNode,
        SampleTexture2DArrayNode,
        SampleTexture3DNode,
        SampleVirtualTextureNode,
        SamplerStateNode,
        CalculateLevelOfDetailTexture2DNode,
        Texture2DAssetNode,
        Texture2DArrayAssetNode,
        Texture3DAssetNode,
        CubemapAssetNode,
        SampleCubemapNode,
        SampleRawCubemapNode,
        GatherTexture2DNode,
        TexelSizeNode,

#if PROCEDURAL_VT_IN_GRAPH
        ProceduralVirtualTextureNode,
#endif
        CustomInterpolatorNode,

        // ===== Input/Property =====
        AddNode,
        PropertyNode,

        // ===== Artistic/Color =====
        ChannelMixerNode,
        ContrastNode,
        HueNode,
        InvertColorsNode,
        ReplaceColorNode,
        SaturationNode,
        WhiteBalanceNode,

        // ===== Artistic/Blend =====
        BlendNode,
        DitherNode,
        FadeTransitionNode,

        // ===== Artistic/Mask =====
        ChannelMaskNode,
        ColorMaskNode,

        // ===== Artistic/Normal =====
        NormalBlendNode,
        NormalFromHeightNode,
        NormalFromTextureNode,
        NormalReconstructZNode,
        NormalStrengthNode,
        NormalUnpackNode,

        // ===== Artistic/Utility =====
        ColorspaceConversionNode,
        CombineNode,
        FlipNode,
        SplitNode,
        SwizzleNode,

        // ===== Math/Advanced =====
        AbsoluteNode,
        ExponentialNode,
        LengthNode,
        LogNode,
        ModuloNode,
        NegateNode,
        NormalizeNode,
        PosterizeNode,
        ReciprocalNode,
        ReciprocalSquareRootNode,

        // ===== Math/Basic =====
        DivideNode,
        MultiplyNode,
        PowerNode,
        SquareRootNode,
        SubtractNode,

        // ===== Math/Derivative =====
        DDXNode,
        DDXYNode,
        DDYNode,

        // ===== Math/Interpolation =====
        InverseLerpNode,
        LerpNode,
        SmoothstepNode,

        // ===== Math/Matrix =====
        MatrixConstructionNode,
        MatrixDeterminantNode,
        MatrixSplitNode,
        MatrixTransposeNode,

        // ===== Math/Range =====
        ClampNode,
        FractionNode,
        MaximumNode,
        MinimumNode,
        OneMinusNode,
        RandomRangeNode,
        RemapNode,
        SaturateNode,

        // ===== Math/Round =====
        CeilingNode,
        FloorNode,
        RoundNode,
        SignNode,
        StepNode,
        TruncateNode,

        // ===== Math/Trigonometry =====
        SineNode,
        TangentNode,
        ArccosineNode,
        ArcsineNode,
        Arctangent2Node,
        ArctangentNode,
        CosineNode,
        DegreesToRadiansNode,
        HyperbolicCosineNode,
        HyperbolicSineNode,
        HyperbolicTangentNode,
        RadiansToDegreesNode,

        // ===== Math/Vector =====
        CrossProductNode,
        DistanceNode,
        DotProductNode,
        FresnelEffectNode,
        ProjectionNode,
        ReflectionNode,
        RefractNode,
        RejectionNode,
        RotateAboutAxisNode,
        SphereMaskNode,
        TransformNode,

        // ===== Math/Wave =====
        NoiseSineWaveNode,
        SawtoothWaveNode,
        SquareWaveNode,
        TriangleWaveNode,

        // ===== Mesh Deformation =====
        
        ComputeDeformNode,
        LinearBlendSkinningNode,

        // ===== Procedural/Noise =====
        GradientNoiseNode,
        SimpleNoiseNode,
        VoronoiNode,

        // ===== Procedural/Shape =====
        EllipseNode,
        PolygonNode,
        RectangleNode,
        RoundedPolygonNode,
        RoundedRectangleNode,
        CheckerboardNode,

        // ===== Utility/Logic =====
        OrNode,
        AllNode,
        AndNode,
        AnyNode,
        BranchNode,
        BranchOnInputConnectionNode,
        ComparisonNode,
        IsFrontFaceNode,
        IsInfiniteNode,
        IsNanNode,
        NandNode,
        NotNode,

        // ===== UV =====
        ParallaxMappingNode,
        ParallaxOcclusionMappingNode,
        PolarCoordinatesNode,
        RadialShearNode,
        RotateNode,
        SpherizeNode,
        TilingAndOffsetNode,
        TriplanarNode,
        TwirlNode,
        FlipbookNode,

        // ===== Utility =====
        DropdownNode,
        KeywordNode,
        CustomFunctionNode,
        PreviewNode,
        SplitTextureTransformNode,
        
        //========Light Texture
        LightTextureNode ,
        
        UniversalSampleBufferNode,
        // ===== Input/UGUI =====
        MeterValueNode,
        RangeBarValueNode,
        RectTransformSizeNode,
        SelectableStateNode,
        SliderValueNode,
        ToggleStateNode,

        // ===== Utility/UGUI =====
        SelectableBranchNode,
        
        //=========特殊节点============
        SubGraphNode
        
    }

    // ============================================================
    // 公共枚举定义（按字母顺序或功能分组）
    // ============================================================
    /// <summary>镜像 UnityEditor.ShaderGraph.ShaderStageCapability</summary>
    [Flags]
    public enum ShaderStageCapability
    {
        None = 0,
        Vertex = 1 << 0,
        Fragment = 1 << 1,
        All = Vertex | Fragment
    }

    /// <summary>镜像 UnityEditor.Graphing.SlotValueType</summary>
    public enum SlotValueType
    {
        SamplerState,
        DynamicMatrix,
        Matrix4,
        Matrix3,
        Matrix2,
        Texture2D,
        Texture2DArray,
        Texture3D,
        Cubemap,
        Gradient,
        DynamicVector,
        Vector4,
        Vector3,
        Vector2,
        Vector1,
        Dynamic,
        Boolean,
        VirtualTexture,
        PropertyConnectionState
    }

    /// <summary>镜像 UnityEditor.Graphing.SlotType</summary>
    public enum SlotType
    {
        Input,
        Output
    }
    public enum BufferType
    {
        NormalWorldSpace,
        MotionVectors,
        BlitSource,
    }
    public enum ScreenSpaceType
    {
        Default,        // screenpos.xy / w ==>  [0, 1] across screen
        Raw,            // screenpos.xyzw ==> scales on distance, requires divide by w
        Center,         // Default, but remapped to [-1, 1]
        Tiled,          // frac(Center)
        Pixel           // Default * _ScreenParams.xy;   [0 .. width-1, 0.. height-1]
    };
    public enum WorkflowMode
    {
        Specular,
        Metallic
    }

    public enum NormalDropOffSpace
    {
        Object,
        Tangent,
        World
    }
    public enum BlendStyle
    {
        LightTex0,
        LightTex1,
        LightTex2,
        LightTex3,
    }
    /// <summary>
    /// 混合模式（Alpha / Premultiply / Additive / Multiply）
    /// 对应 Unity 内部 AlphaMode 枚举
    /// </summary>
    public enum BlendMode
    {
        Alpha,
        Premultiply,
        Additive,
        Multiply
    }

    public enum BlendOperationMode
    {
        Burn,
        Darken,
        Difference,
        Dodge,
        Divide,
        Exclusion,
        HardLight,
        HardMix,
        Lighten,
        LinearBurn,
        LinearDodge,
        LinearLight,
        LinearLightAddSub,
        Multiply,
        Negation,
        Overlay,
        PinLight,
        Screen,
        SoftLight,
        Subtract,
        VividLight,
        Overwrite
    }

    public enum ChannelOption
    {
        Red,
        Green,
        Blue,
        Alpha
    }

    public enum ClampTypeOption
    {
        Fastest,
        Nicest
    }

    public enum ColorspaceOption
    {
        RGB,
        Linear,
        HSV
    }

    public enum ComparisonTypeOption
    {
        Equal,
        NotEqual,
        Less,
        LessOrEqual,
        Greater,
        GreaterOrEqual
    }

    public enum ConstantTypeOption
    {
        PI,
        TAU,
        PHI,
        E,
        SQRT2
    }

    public enum ConversionTypeOption
    {
        Position,
        Direction,
        Normal
    }

    public enum CoordinateSpaceOption
    {
        Object,
        View,
        World,
        Tangent,
        AbsoluteWorld,
        RelativeWorld
    }

    public enum DepthSamplingModeOption
    {
        Linear01,
        Raw,
        Eye
    }

    public enum ExponentialBaseOption
    {
        BaseE,
        Base2
    }

    public enum HashTypeOption
    {
        Deterministic,
        LegacyMod,
        LegacySine
    }

    public enum HlslSourceTypeOption
    {
        File,
        String
    }

    public enum HLSLDeclarationOption
    {
        DoNotDeclare,
        Global,
        UnityPerMaterial
    }

    public enum HueModeOption
    {
        Degrees,
        Normalized
    }

    public enum LogBaseOption
    {
        BaseE,
        Base2,
        Base10
    }

    public enum NormalBlendModeOption
    {
        Default,
        Reoriented
    }

    public enum NormalMapSpaceOption
    {
        Tangent,
        Object
    }

    public enum OutputSpaceOption
    {
        Tangent,
        World
    }

    public enum ReciprocalMethodOption
    {
        Default,
        Fast
    }

    public enum RefractModeOption
    {
        CriticalAngle,
        Safe
    }

    public enum RotationUnitOption
    {
        Radians,
        Degrees
    }

    public enum SamplerAnisotropic
    {
        None,
        x2,
        x4,
        x8,
        x16
    }

    public enum SamplerAnisotropicOption
    {
        None,
        Aniso2,
        Aniso4,
        Aniso8,
        Aniso16
    }

    public enum SamplerFilterMode
    {
        Linear,
        Point,
        Trilinear
    }

    public enum SamplerFilterModeOption
    {
        Linear,
        Point,
        Trilinear
    }

    public enum SamplerWrapMode
    {
        Repeat,
        Clamp,
        Mirror,
        MirrorOnce
    }

    public enum SamplerWrapModeOption
    {
        Repeat,
        Clamp,
        Mirror,
        MirrorOnce
    }

    public enum ShaderGraphContextType
    {
        Vertex,
        Fragment
    }
    /// <summary>
    /// 渲染面（Front / Back / Both）
    /// 对应 Unity 内部 RenderFace 枚举
    /// 注意：值必须与 Unity 内部一致（Front=2, Back=1, Both=0）
    /// </summary>
    public enum RenderFace
    {
        Both = 0,
        Back = 1,
        Front = 2,
    }
    /// <summary>
    /// 深度写入控制（Auto / ForceEnabled / ForceDisabled）
    /// 对应 Unity 内部 ZWriteControl 枚举
    /// </summary>
    public enum ZWriteControl
    {
        Auto = 0,
        ForceEnabled = 1,
        ForceDisabled = 2,
    }
    /// <summary>
    /// 深度测试模式（与 UnityEngine.Rendering.CompareFunction 值对应）
    /// 对应 Unity 内部 ZTestMode 枚举
    /// </summary>
    public enum ZTestMode
    {
        Disabled = 0,
        Never = 1,
        Less = 2,
        Equal = 3,
        LEqual = 4,
        Greater = 5,
        NotEqual = 6,
        GEqual = 7,
        Always = 8,
    }
    /// <summary>
    /// 附加运动向量模式（None / TimeBased / Custom）
    /// 对应 Unity 内部 AdditionalMotionVectorMode 枚举
    /// </summary>
    public enum AdditionalMotionVectorMode
    {
        None,
        TimeBased,
        Custom,
    }
    public enum SurfaceBlockType
    {
        
        BaseColor,
        NormalTS,
        NormalOS,
        NormalWS,
        Metallic,
        Specular,
        Smoothness,
        Occlusion,
        Emission,
        Alpha,
        AlphaClipThreshold,
        CoatMask,
        CoatSmoothness,
        MapRightTopBack,
        MapLeftBottomFront,
        AbsorptionStrength,
        SpriteMask,
        
       
    }
    public enum SubGraphIdentifierType
    {
        Auto,
        Guid,
        AssetName,
        Alias,
        AssetPath
    }
    /// <summary>
    /// 表面类型（Opaque / Transparent）
    /// 对应 Unity 内部 SurfaceType 枚举
    /// </summary>
    public enum SurfaceType
    {
        Opaque,
        Transparent
    }

    public enum Texture2DMipSamplingModeOption
    {
        Standard,
        LOD,
        Gradient,
        Bias
    }

    public enum Texture3DMipSamplingModeOption
    {
        Standard,
        LOD
    }

    public enum TextureTypeOption
    {
        Default,
        Normal
    }

    public enum VertexBlockType
    {
        Position,
        Normal,
        Tangent
    }

    public enum VirtualTextureAddressModeOption
    {
        VtAddressMode_Wrap,
        VtAddressMode_Clamp
    }

    public enum VirtualTextureLodCalculationOption
    {
        VtLevel_Automatic,
        VtLevel_Lod,
        VtLevel_Bias,
        VtLevel_Derivatives
    }

    public enum VirtualTextureQualityModeOption
    {
        VtSampleQuality_Low,
        VtSampleQuality_High
    }

    // ============================================================
    // 以下枚举从 ColorNodeParams.cs 移动至此
    // ============================================================

    public enum MatrixAxisOption
    {
        Row,
        Column
    }

    public enum CustomBlockTypeOption
    {
        Float,
        Vector2,
        Vector3,
        Vector4
    }

    public enum PositionSourceOption
    {
        Default,
        Predisplacement
    }

    public enum ScreenSpaceTypeOption
    {
        Default,
        Position,
        Raw,
        Center,
        Tiled,
        Pixel
    }

    
    
    public enum UVChannelOption
    {
        UV0,
        UV1,
        UV2,
        UV3
    }

    public enum UnityMatrixTypeOption
    {
        Model,
        InverseModel,
        View,
        InverseView,
        Projection,
        InverseProjection,
        ViewProjection,
        InverseViewProjection
    }

    public enum DielectricMaterialTypeOption
    {
        Common,
        RustedMetal,
        Water,
        Ice,
        Glass,
        Custom
    }

    public enum MetalMaterialTypeOption
    {
        Iron,
        Silver,
        Aluminium,
        Gold,
        Copper,
        Chromium,
        Nickel,
        Titanium,
        Cobalt,
        Platinum
    }
}