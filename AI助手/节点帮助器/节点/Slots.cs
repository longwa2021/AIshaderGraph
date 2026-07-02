namespace 龙哥的秘密花园.节点库
{
    // ============================================================
    // 输入/基础节点
    // ============================================================

    /// <summary>颜色节点，输出一个颜色值</summary>
    public static class ColorNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0;   // 输出颜色
        }
    }

    /// <summary>布尔节点，输出 true/false</summary>
    public static class BooleanNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0;   // 输出布尔值
        }
    }

    /// <summary>常量节点，输出数学常量（PI、E 等）</summary>
    public static class ConstantNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0;   // 输出常量值
        }
    }

    /// <summary>整数节点，输出一个整数值</summary>
    public static class IntegerNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0;   // 输出整数
        }
    }

    /// <summary>滑条节点，输出一个带范围限制的浮点数</summary>
    public static class SliderNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0;   // 输出浮点值
        }
    }

    /// <summary>时间节点，输出各种时间相关值</summary>
    public static class TimeNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Time = 0;       // 时间（秒）
            public const int SineTime = 1;   // 正弦时间
            public const int CosineTime = 2; // 余弦时间
            public const int DeltaTime = 3;  // 帧间隔时间
            public const int SmoothDelta = 4;// 平滑帧间隔时间
        }
    }

    /// <summary>一维向量节点（float）</summary>
    public static class Vector1Node
    {
        public static class Input
        {
            public const int X = 1;   // 输入 X 分量
        }

        public static class Output
        {
            public const int Out = 0; // 输出一维向量
        }
    }

    /// <summary>二维向量节点</summary>
    public static class Vector2Node
    {
        public static class Input
        {
            public const int X = 1;   // X 分量输入
            public const int Y = 2;   // Y 分量输入
        }

        public static class Output
        {
            public const int Out = 0; // 输出二维向量
        }
    }

    /// <summary>三维向量节点</summary>
    public static class Vector3Node
    {
        public static class Input
        {
            public const int X = 1;   // X 分量输入
            public const int Y = 2;   // Y 分量输入
            public const int Z = 3;   // Z 分量输入
        }

        public static class Output
        {
            public const int Out = 0; // 输出三维向量
        }
    }

    /// <summary>四维向量节点</summary>
    public static class Vector4Node
    {
        public static class Input
        {
            public const int X = 1;   // X 分量输入
            public const int Y = 2;   // Y 分量输入
            public const int Z = 3;   // Z 分量输入
            public const int W = 4;   // W 分量输入
        }

        public static class Output
        {
            public const int Out = 0; // 输出四维向量
        }
    }

    /// <summary>实例 ID 节点，输出当前绘制实例的唯一 ID</summary>
    public static class InstanceIDNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出实例 ID
        }
    }

    /// <summary>法线向量节点，输出顶点/像素法线</summary>
    public static class NormalVectorNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出法线方向
        }
    }

    /// <summary>位置节点，输出顶点/像素位置</summary>
    public static class PositionNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出位置坐标
        }
    }

    /// <summary>屏幕位置节点，输出屏幕空间坐标</summary>
    public static class ScreenPositionNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出屏幕位置
        }
    }

    /// <summary>切线向量节点，输出顶点/像素切线方向</summary>
    public static class TangentVectorNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出切线方向
        }
    }

    /// <summary>UV 坐标节点，输出纹理坐标</summary>
    public static class UVNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出 UV 坐标
        }
    }

    /// <summary>顶点颜色节点，输出顶点颜色</summary>
    public static class VertexColorNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出顶点颜色
        }
    }

    /// <summary>顶点 ID 节点，输出顶点索引</summary>
    public static class VertexIDNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出顶点 ID
        }
    }

    /// <summary>视线方向节点，输出从表面指向相机的方向</summary>
    public static class ViewDirectionNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出视线方向
        }
    }

    /// <summary>视图向量节点，输出从相机到表面的方向</summary>
    public static class ViewVectorNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出视图向量
        }
    }

    /// <summary>副切线向量节点</summary>
    public static class BitangentVectorNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出副切线方向
        }
    }

    /// <summary>黑体辐射节点，根据温度输出颜色</summary>
    public static class BlackbodyNode
    {
        public static class Input
        {
            public const int X = 0; // 温度输入（开尔文）
        }

        public static class Output
        {
            public const int Out = 1; // 输出颜色
        }
    }

    /// <summary>渐变节点，定义渐变资源</summary>
    public static class GradientNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出渐变对象
        }
    }

    /// <summary>采样渐变节点，根据时间从渐变中获取颜色</summary>
    public static class SampleGradientNode
    {
        public static class Input
        {
            public const int Gradient = 0; // 渐变输入
            public const int Time = 1;     // 采样时间（0-1）
        }

        public static class Output
        {
            public const int Out = 2; // 输出颜色
        }
    }

    /// <summary>环境光节点，输出环境光照颜色</summary>
    public static class AmbientNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Color_Sky = 0; // 天空环境光
            public const int Equator = 1;   // 赤道环境光
            public const int Ground = 2;    // 地面环境光
        }
    }

    /// <summary>烘焙 GI 节点，输出烘焙的全局光照</summary>
    public static class BakedGINode
    {
        public static class Input
        {
            public const int NormalWS = 0;   // 世界空间法线
            public const int PositionWS = 2; // 世界空间位置
            public const int StaticUV = 3;   // 静态 UV
            public const int DynamicUV = 4;  // 动态 UV
        }

        public static class Output
        {
            public const int Out = 1; // 输出烘焙 GI 值
        }
    }

    /// <summary>主光源方向节点</summary>
    public static class MainLightDirectionNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Direction = 0; // 输出主光源方向
        }
    }

    /// <summary>反射探针节点，采样反射探针</summary>
    public static class ReflectionProbeNode
    {
        public static class Input
        {
            public const int ViewDir = 0; // 视线方向
            public const int Normal = 1;  // 法线方向
            public const int LOD = 2;     // LOD 级别
        }

        public static class Output
        {
            public const int Direction = 3; // 输出反射方向（或颜色？根据原代码）
        }
    }

    /// <summary>2x2 矩阵节点</summary>
    public static class Matrix2Node
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出矩阵
        }
    }

    /// <summary>3x3 矩阵节点</summary>
    public static class Matrix3Node
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出矩阵
        }
    }

    /// <summary>4x4 矩阵节点</summary>
    public static class Matrix4Node
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出矩阵
        }
    }

    /// <summary>变换矩阵节点，输出常用变换矩阵</summary>
    public static class TransformationMatrixNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出矩阵
        }
    }

    /// <summary>电介质高光节点，计算电介质的反射率</summary>
    public static class DielectricSpecularNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出高光颜色
        }
    }

    /// <summary>金属反射率节点，根据金属类型输出反射率</summary>
    public static class MetalReflectanceNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出反射率
        }
    }

    /// <summary>相机节点，输出相机相关属性</summary>
    public static class CameraNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Position = 0;     // 相机位置
            public const int Direction = 1;    // 相机方向
            public const int Orthographic = 2; // 是否正交
            public const int NearPlane = 3;    // 近平面距离
            public const int FarPlane = 4;     // 远平面距离
            public const int ZBufferSign = 5;  // Z 缓冲区符号
            public const int Width = 6;        // 相机宽度
            public const int Height = 7;       // 相机高度
        }
    }

    /// <summary>眼索引节点（用于 VR）</summary>
    public static class EyeIndexNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出眼索引（0 或 1）
        }
    }

    /// <summary>雾节点，输出雾的颜色和密度</summary>
    public static class FogNode
    {
        public static class Input
        {
            public const int Position = 2; // 世界空间位置
        }

        public static class Output
        {
            public const int Color = 0;   // 雾颜色
            public const int Density = 1; // 雾密度
        }
    }

    /// <summary>物体节点，输出物体变换属性</summary>
    public static class ObjectNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Position = 0;       // 物体位置
            public const int Scale = 1;          // 物体缩放
            public const int WorldBoundsMin = 2; // 世界包围盒最小值
            public const int WorldBoundsMax = 3; // 世界包围盒最大值
            public const int BoundsSize = 4;     // 包围盒尺寸
        }
    }

    /// <summary>场景颜色节点，采样场景颜色</summary>
    public static class SceneColorNode
    {
        public static class Input
        {
            public const int UV = 0; // 屏幕 UV
        }

        public static class Output
        {
            public const int Out = 1; // 输出场景颜色
        }
    }

    /// <summary>场景深度差节点，计算深度差异</summary>
    public static class SceneDepthDifferenceNode
    {
        public static class Input
        {
            public const int SceneUV = 1;     // 屏幕 UV
            public const int PositionWS = 2;  // 世界空间位置
        }

        public static class Output
        {
            public const int Out = 0; // 输出深度差
        }
    }

    /// <summary>场景深度节点，采样场景深度</summary>
    public static class SceneDepthNode
    {
        public static class Input
        {
            public const int UV = 0; // 屏幕 UV
        }

        public static class Output
        {
            public const int Out = 1; // 输出深度值
        }
    }

    /// <summary>屏幕节点，输出屏幕尺寸</summary>
    public static class ScreenNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Width = 0;  // 屏幕宽度
            public const int Height = 1; // 屏幕高度
        }
    }

    // ============================================================
    // 纹理采样节点
    // ============================================================

    /// <summary>采样 2D 纹理节点</summary>
    public static class SampleTexture2DNode
    {
        public static class Input
        {
            public const int UV = 2;       // UV 坐标
            public const int Texture = 1;  // 纹理输入
            public const int Sampler = 3;  // 采样器输入
        }

        public static class Output
        {
            public const int RGBA = 0; // 输出 RGBA
            public const int R = 4;    // 输出 R 通道
            public const int G = 5;    // 输出 G 通道
            public const int B = 6;    // 输出 B 通道
            public const int A = 7;    // 输出 A 通道
        }
    }

    /// <summary>采样 2D 纹理 LOD 节点</summary>
    public static class SampleTexture2DLODNode
    {
        public static class Input
        {
            public const int UV = 2;       // UV 坐标
            public const int Texture = 1;  // 纹理输入
            public const int Sampler = 3;  // 采样器输入
            public const int LOD = 4;      // LOD 级别
        }

        public static class Output
        {
            public const int RGBA = 0; // 输出 RGBA
            public const int R = 5;    // 输出 R 通道
            public const int G = 6;    // 输出 G 通道
            public const int B = 7;    // 输出 B 通道
            public const int A = 8;    // 输出 A 通道
        }
    }

    /// <summary>采样 2D 纹理数组节点</summary>
    public static class SampleTexture2DArrayNode
    {
        public static class Input
        {
            public const int UV = 2;       // UV 坐标
            public const int Texture = 1;  // 纹理数组输入
            public const int Sampler = 3;  // 采样器输入
            public const int Index = 8;    // 数组索引
        }

        public static class Output
        {
            public const int RGBA = 0; // 输出 RGBA
            public const int R = 4;    // 输出 R 通道
            public const int G = 5;    // 输出 G 通道
            public const int B = 6;    // 输出 B 通道
            public const int A = 7;    // 输出 A 通道
        }
    }

    /// <summary>采样 3D 纹理节点</summary>
    public static class SampleTexture3DNode
    {
        public static class Input
        {
            public const int UV = 2;       // UVW 坐标
            public const int Texture = 1;  // 纹理输入
            public const int Sampler = 3;  // 采样器输入
        }

        public static class Output
        {
            public const int RGBA = 0; // 输出 RGBA
            public const int R = 5;    // 输出 R 通道
            public const int G = 6;    // 输出 G 通道
            public const int B = 7;    // 输出 B 通道
            public const int A = 8;    // 输出 A 通道
        }
    }

    /// <summary>采样虚拟纹理节点</summary>
    public static class SampleVirtualTextureNode
    {
        public static class Input
        {
            public const int UV = 0;   // UV 坐标
            public const int VT = 1;   // 虚拟纹理输入
        }

        public static class Output
        {
            public const int Out = 11;  // 第一层输出
            public const int Out2 = 12; // 第二层输出
            public const int Out3 = 13; // 第三层输出
            public const int Out4 = 14; // 第四层输出
        }
    }

    /// <summary>采样器状态节点，定义纹理采样参数</summary>
    public static class SamplerStateNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出采样器对象
        }
    }

    /// <summary>计算 2D 纹理 LOD 节点</summary>
    public static class CalculateLevelOfDetailTexture2DNode
    {
        public static class Input
        {
            public const int Texture = 1;  // 纹理输入
            public const int UV = 2;       // UV 坐标
            public const int Sampler = 3;  // 采样器输入
        }

        public static class Output
        {
            public const int LOD = 0; // 输出 LOD 级别
        }
    }

    /// <summary>2D 纹理资源节点</summary>
    public static class Texture2DAssetNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出纹理对象
        }
    }

    /// <summary>2D 纹理数组资源节点</summary>
    public static class Texture2DArrayAssetNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出纹理数组对象
        }
    }

    /// <summary>3D 纹理资源节点</summary>
    public static class Texture3DAssetNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出纹理对象
        }
    }

    /// <summary>立方体贴图资源节点</summary>
    public static class CubemapAssetNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出立方体贴图对象
        }
    }

    /// <summary>采样立方体贴图节点</summary>
    public static class SampleCubemapNode
    {
        public static class Input
        {
            public const int Cube = 1;     // 立方体贴图输入
            public const int ViewDir = 2;  // 视线方向
            public const int Normal = 3;   // 法线方向
            public const int Sampler = 5;  // 采样器输入
            public const int LOD = 4;      // LOD 级别
        }

        public static class Output
        {
            public const int Out = 0; // 输出采样颜色
        }
    }

    /// <summary>采样原始立方体贴图节点（不进行内部转换）</summary>
    public static class SampleRawCubemapNode
    {
        public static class Input
        {
            public const int Cube = 1;    // 立方体贴图输入
            public const int Dir = 2;     // 方向向量
            public const int Sampler = 3; // 采样器输入
            public const int LOD = 4;     // LOD 级别
        }

        public static class Output
        {
            public const int Out = 0; // 输出采样颜色
        }
    }

    /// <summary>纹理 Gather 操作节点（获取四个纹素）</summary>
    public static class GatherTexture2DNode
    {
        public static class Input
        {
            public const int Texture = 1;  // 纹理输入
            public const int UV = 2;       // UV 坐标
            public const int Sampler = 3;  // 采样器输入
            public const int Offset = 4;   // 偏移量
        }

        public static class Output
        {
            public const int RGBA = 0; // 输出 RGBA 四个分量各自四个纹素
            public const int R = 5;    // 输出 R 通道 Gather
            public const int G = 6;    // 输出 G 通道 Gather
            public const int B = 7;    // 输出 B 通道 Gather
            public const int A = 8;    // 输出 A 通道 Gather
        }
    }

    /// <summary>纹理尺寸节点，输出纹素大小和纹理尺寸</summary>
    public static class TexelSizeNode
    {
        public static class Input
        {
            public const int Texture = 1; // 纹理输入
        }

        public static class Output
        {
            public const int Width = 0;       // 纹理宽度
            public const int Height = 2;      // 纹理高度
            public const int TexelWidth = 3;  // 纹素宽度（1/宽度）
            public const int TexelHeight = 4; // 纹素高度（1/高度）
        }
    }

#if PROCEDURAL_VT_IN_GRAPH
    /// <summary>程序化虚拟纹理节点</summary>
    public static class ProceduralVirtualTextureNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出虚拟纹理对象
        }
    }
#endif

    /// <summary>自定义插值器节点，用于在顶点和片元之间传递自定义数据</summary>
    public static class CustomInterpolatorNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出插值数据
        }
    }

    // ============================================================
    // 数学运算节点
    // ============================================================

    /// <summary>加法节点</summary>
    public static class AddNode
    {
        public static class Input
        {
            public const int A = 0; // 输入 A
            public const int B = 1; // 输入 B
        }

        public static class Output
        {
            public const int Out = 2; // 输出 A + B
        }
    }

    /// <summary>属性节点，引用黑板上定义的属性</summary>
    public static class PropertyNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出属性值
        }
    }

    /// <summary>通道混合器节点</summary>
    public static class ChannelMixerNode
    {
        public static class Input
        {
            public const int In = 0; // 输入颜色
        }

        public static class Output
        {
            public const int Out = 1; // 输出混合后的颜色
        }
    }

    /// <summary>对比度节点</summary>
    public static class ContrastNode
    {
        public static class Input
        {
            public const int In = 0;       // 输入颜色
            public const int Contrast = 1; // 对比度值
        }

        public static class Output
        {
            public const int Out = 2; // 输出调整后的颜色
        }
    }

    /// <summary>色相节点</summary>
    public static class HueNode
    {
        public static class Input
        {
            public const int In = 0;      // 输入颜色
            public const int Offset = 1;  // 色相偏移
        }

        public static class Output
        {
            public const int Out = 2; // 输出调整后的颜色
        }
    }

    /// <summary>反色节点</summary>
    public static class InvertColorsNode
    {
        public static class Input
        {
            public const int In = 0; // 输入颜色
        }

        public static class Output
        {
            public const int Out = 2; // 输出反色后的颜色
        }
    }

    /// <summary>颜色替换节点</summary>
    public static class ReplaceColorNode
    {
        public static class Input
        {
            public const int In = 0;        // 输入颜色
            public const int From = 1;      // 源颜色
            public const int To = 2;        // 目标颜色
            public const int Range = 3;     // 范围
            public const int Fuzziness = 5; // 模糊度
        }

        public static class Output
        {
            public const int Out = 4; // 输出替换后的颜色
        }
    }

    /// <summary>饱和度节点</summary>
    public static class SaturationNode
    {
        public static class Input
        {
            public const int In = 0;         // 输入颜色
            public const int Saturation = 1; // 饱和度值
        }

        public static class Output
        {
            public const int Out = 2; // 输出调整后的颜色
        }
    }

    /// <summary>白平衡节点</summary>
    public static class WhiteBalanceNode
    {
        public static class Input
        {
            public const int In = 0;           // 输入颜色
            public const int Temperature = 1;  // 色温
            public const int Tint = 2;         // 色调
        }

        public static class Output
        {
            public const int Out = 3; // 输出调整后的颜色
        }
    }

    /// <summary>混合节点（Blend）</summary>
    public static class BlendNode
    {
        public static class Input
        {
            public const int Base = 0;    // 基础颜色
            public const int Blend = 1;   // 混合颜色
            public const int Opacity = 3; // 不透明度
        }

        public static class Output
        {
            public const int Out = 2; // 输出混合结果
        }
    }

    /// <summary>抖动节点（Dither）</summary>
    public static class DitherNode
    {
        public static class Input
        {
            public const int In = 0;             // 输入值
            public const int ScreenPosition = 1; // 屏幕位置
        }

        public static class Output
        {
            public const int Out = 2; // 输出抖动后的值
        }
    }

    /// <summary>淡入淡出过渡节点</summary>
    public static class FadeTransitionNode
    {
        public static class Input
        {
            public const int NoiseValue = 1;    // 噪声值
            public const int FadeValue = 2;     // 淡入淡出值
            public const int FadeContrast = 3;  // 对比度
        }

        public static class Output
        {
            public const int Fade = 0; // 输出淡入淡出结果
        }
    }

    /// <summary>通道遮罩节点</summary>
    public static class ChannelMaskNode
    {
        public static class Input
        {
            public const int In = 0; // 输入向量
        }

        public static class Output
        {
            public const int Out = 1; // 输出遮罩后的向量
        }
    }

    /// <summary>颜色遮罩节点</summary>
    public static class ColorMaskNode
    {
        public static class Input
        {
            public const int In = 0;          // 输入颜色
            public const int MaskColor = 1;   // 遮罩颜色
            public const int Range = 2;       // 范围
            public const int Fuzziness = 4;   // 模糊度
        }

        public static class Output
        {
            public const int Out = 3; // 输出遮罩值
        }
    }

    /// <summary>法线混合节点</summary>
    public static class NormalBlendNode
    {
        public static class Input
        {
            public const int A = 0; // 法线 A
            public const int B = 1; // 法线 B
        }

        public static class Output
        {
            public const int Out = 2; // 输出混合后的法线
        }
    }

    /// <summary>从高度图生成法线节点</summary>
    public static class NormalFromHeightNode
    {
        public static class Input
        {
            public const int In = 0;       // 高度值
            public const int Strength = 2; // 强度
        }

        public static class Output
        {
            public const int Out = 1; // 输出法线
        }
    }

    /// <summary>从纹理生成法线节点</summary>
    public static class NormalFromTextureNode
    {
        public static class Input
        {
            public const int Texture = 0;  // 纹理输入
            public const int UV = 1;       // UV 坐标
            public const int Sampler = 2;  // 采样器
            public const int Offset = 3;   // 偏移
            public const int Strength = 4; // 强度
        }

        public static class Output
        {
            public const int Out = 5; // 输出法线
        }
    }

    /// <summary>重建法线 Z 分量节点</summary>
    public static class NormalReconstructZNode
    {
        public static class Input
        {
            public const int In = 0; // 输入的 XY 分量
        }

        public static class Output
        {
            public const int Out = 2; // 输出完整法线
        }
    }

    /// <summary>法线强度节点</summary>
    public static class NormalStrengthNode
    {
        public static class Input
        {
            public const int In = 0;       // 输入法线
            public const int Strength = 1; // 强度
        }

        public static class Output
        {
            public const int Out = 2; // 输出调整后的法线
        }
    }

    /// <summary>解压法线节点（将压缩的法线解码）</summary>
    public static class NormalUnpackNode
    {
        public static class Input
        {
            public const int In = 0; // 压缩的法线
        }

        public static class Output
        {
            public const int Out = 1; // 输出解压后的法线
        }
    }

    /// <summary>颜色空间转换节点</summary>
    public static class ColorspaceConversionNode
    {
        public static class Input
        {
            public const int In = 0; // 输入颜色
        }

        public static class Output
        {
            public const int Out = 1; // 输出转换后的颜色
        }
    }

    /// <summary>合并节点（将多个分量合并为向量）</summary>
    public static class CombineNode
    {
        public static class Input
        {
            public const int R = 0; // R 分量
            public const int G = 1; // G 分量
            public const int B = 2; // B 分量
            public const int A = 3; // A 分量
        }

        public static class Output
        {
            public const int RGBA = 4; // 输出四维向量
            public const int RGB = 5;  // 输出三维向量
            public const int RG = 6;   // 输出二维向量
        }
    }

    /// <summary>翻转节点（翻转向量的分量）</summary>
    public static class FlipNode
    {
        public static class Input
        {
            public const int In = 0; // 输入向量
        }

        public static class Output
        {
            public const int Out = 1; // 输出翻转后的向量
        }
    }

    /// <summary>拆分节点（将向量拆分为分量）</summary>
    public static class SplitNode
    {
        public static class Input
        {
            public const int In = 0; // 输入向量
        }

        public static class Output
        {
            public const int R = 1; // R/X 分量
            public const int G = 2; // G/Y 分量
            public const int B = 3; // B/Z 分量
            public const int A = 4; // A/W 分量
        }
    }

    /// <summary>Swizzle 节点（重新排列向量分量）</summary>
    public static class SwizzleNode
    {
        public static class Input
        {
            public const int In = 0; // 输入向量
        }

        public static class Output
        {
            public const int Out = 1; // 输出重排后的向量
        }
    }

    /// <summary>绝对值节点</summary>
    public static class AbsoluteNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出绝对值
        }
    }

    /// <summary>指数节点（e^x 或 2^x）</summary>
    public static class ExponentialNode
    {
        public static class Input
        {
            public const int In = 0; // 指数
        }

        public static class Output
        {
            public const int Out = 1; // 输出结果
        }
    }

    /// <summary>长度节点（计算向量模长）</summary>
    public static class LengthNode
    {
        public static class Input
        {
            public const int In = 0; // 输入向量
        }

        public static class Output
        {
            public const int Out = 1; // 输出长度
        }
    }

    /// <summary>对数节点（ln、log2、log10）</summary>
    public static class LogNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出对数
        }
    }

    /// <summary>取模节点（求余数）</summary>
    public static class ModuloNode
    {
        public static class Input
        {
            public const int A = 0; // 被除数
            public const int B = 1; // 除数
        }

        public static class Output
        {
            public const int Out = 2; // 输出余数
        }
    }

    /// <summary>取反节点</summary>
    public static class NegateNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出相反数
        }
    }

    /// <summary>归一化节点</summary>
    public static class NormalizeNode
    {
        public static class Input
        {
            public const int In = 0; // 输入向量
        }

        public static class Output
        {
            public const int Out = 1; // 输出归一化向量
        }
    }

    /// <summary>色调分离节点（Posterize）</summary>
    public static class PosterizeNode
    {
        public static class Input
        {
            public const int In = 0;    // 输入值
            public const int Steps = 1; // 色阶数
        }

        public static class Output
        {
            public const int Out = 2; // 输出分离后的值
        }
    }
    public static class UniversalSampleBufferNode
    {
        public static class Input
        {
            public const int UV = 0;    
          
        }

        public static class Output
        {
            public const int output = 2; // 输出分离后的值
        }
    }

    /// <summary>倒数节点（1/x）</summary>
    public static class ReciprocalNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出倒数
        }
    }

    /// <summary>平方根倒数节点（1/sqrt(x)）</summary>
    public static class ReciprocalSquareRootNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出平方根倒数
        }
    }

    /// <summary>除法节点</summary>
    public static class DivideNode
    {
        public static class Input
        {
            public const int A = 0; // 被除数
            public const int B = 1; // 除数
        }

        public static class Output
        {
            public const int Out = 2; // 输出商
        }
    }

    /// <summary>乘法节点</summary>
    public static class MultiplyNode
    {
        public static class Input
        {
            public const int A = 0; // 乘数 A
            public const int B = 1; // 乘数 B
        }

        public static class Output
        {
            public const int Out = 2; // 输出积
        }
    }

    /// <summary>幂节点（x^y）</summary>
    public static class PowerNode
    {
        public static class Input
        {
            public const int A = 0; // 底数
            public const int B = 1; // 指数
        }

        public static class Output
        {
            public const int Out = 2; // 输出幂
        }
    }

    /// <summary>平方根节点</summary>
    public static class SquareRootNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出平方根
        }
    }

    /// <summary>减法节点</summary>
    public static class SubtractNode
    {
        public static class Input
        {
            public const int A = 0; // 被减数
            public const int B = 1; // 减数
        }

        public static class Output
        {
            public const int Out = 2; // 输出差
        }
    }

    /// <summary>DDX 节点（屏幕空间 X 方向偏导数）</summary>
    public static class DDXNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出偏导数
        }
    }

    /// <summary>DDXY 节点（屏幕空间 X 和 Y 方向偏导数的绝对值之和）</summary>
    public static class DDXYNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出偏导数和
        }
    }

    /// <summary>DDY 节点（屏幕空间 Y 方向偏导数）</summary>
    public static class DDYNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出偏导数
        }
    }

    /// <summary>逆线性插值节点</summary>
    public static class InverseLerpNode
    {
        public static class Input
        {
            public const int A = 0; // 范围起点
            public const int B = 1; // 范围终点
            public const int T = 2; // 插值因子
        }

        public static class Output
        {
            public const int Out = 3; // 输出逆插值结果（0-1）
        }
    }

    /// <summary>线性插值节点</summary>
    public static class LerpNode
    {
        public static class Input
        {
            public const int A = 0; // 起点值
            public const int B = 1; // 终点值
            public const int T = 2; // 插值因子
        }

        public static class Output
        {
            public const int Out = 3; // 输出插值结果
        }
    }

    /// <summary>平滑步长节点</summary>
    public static class SmoothstepNode
    {
        public static class Input
        {
            public const int Edge1 = 0; // 下限
            public const int Edge2 = 1; // 上限
            public const int In = 2;    // 输入值
        }

        public static class Output
        {
            public const int Out = 3; // 输出平滑插值结果
        }
    }

    /// <summary>矩阵构造节点（从行或列构造矩阵）</summary>
    public static class MatrixConstructionNode
    {
        public static class Input
        {
            public const int M0 = 0; // 第 0 行/列
            public const int M1 = 1; // 第 1 行/列
            public const int M2 = 2; // 第 2 行/列
            public const int M3 = 2; // 注意：原代码中 M3 索引重复，疑似应为 3
        }

        public static class Output
        {
            public const int M4x4 = 4; // 输出 4x4 矩阵
            public const int M3x3 = 5; // 输出 3x3 矩阵
            public const int M2x2 = 6; // 输出 2x2 矩阵
        }
    }

    /// <summary>矩阵行列式节点</summary>
    public static class MatrixDeterminantNode
    {
        public static class Input
        {
            public const int In = 0; // 输入矩阵
        }

        public static class Output
        {
            public const int Out = 1; // 输出行列式值
        }
    }

    /// <summary>矩阵拆分节点（拆分为行或列向量）</summary>
    public static class MatrixSplitNode
    {
        public static class Input
        {
            public const int In = 0; // 输入矩阵
        }

        public static class Output
        {
            public const int M0 = 1; // 第 0 行/列
            public const int M1 = 2; // 第 1 行/列
            public const int M2 = 3; // 第 2 行/列
            public const int M3 = 4; // 第 3 行/列
        }
    }

    /// <summary>矩阵转置节点</summary>
    public static class MatrixTransposeNode
    {
        public static class Input
        {
            public const int In = 0; // 输入矩阵
        }

        public static class Output
        {
            public const int Out = 1; // 输出转置矩阵
        }
    }

    /// <summary>钳位节点（限制范围）</summary>
    public static class ClampNode
    {
        public static class Input
        {
            public const int In = 0;   // 输入值
            public const int Min = 1;  // 最小值
            public const int Max = 2;  // 最大值
        }

        public static class Output
        {
            public const int Out = 3; // 输出钳位后的值
        }
    }

    /// <summary>取小数部分节点</summary>
    public static class FractionNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出小数部分
        }
    }

    /// <summary>最大值节点</summary>
    public static class MaximumNode
    {
        public static class Input
        {
            public const int A = 0; // 值 A
            public const int B = 1; // 值 B
        }

        public static class Output
        {
            public const int Out = 2; // 输出较大值
        }
    }

    /// <summary>最小值节点</summary>
    public static class MinimumNode
    {
        public static class Input
        {
            public const int A = 0; // 值 A
            public const int B = 1; // 值 B
        }

        public static class Output
        {
            public const int Out = 2; // 输出较小值
        }
    }

    /// <summary>1 - x 节点</summary>
    public static class OneMinusNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出 1 - x
        }
    }

    /// <summary>随机范围节点</summary>
    public static class RandomRangeNode
    {
        public static class Input
        {
            public const int Seed = 0; // 随机种子
            public const int Min = 1;  // 最小值
            public const int Max = 2;  // 最大值
        }

        public static class Output
        {
            public const int Out = 3; // 输出随机值
        }
    }

    /// <summary>重新映射节点（将一个范围映射到另一个范围）</summary>
    public static class RemapNode
    {
        public static class Input
        {
            public const int In = 0;         // 输入值
            public const int InMinMax = 1;   // 输入范围（Min, Max）
            public const int OutMinMax = 2;  // 输出范围（Min, Max）
        }

        public static class Output
        {
            public const int Out = 3; // 输出映射后的值
        }
    }

    /// <summary>饱和节点（钳位到 0-1）</summary>
    public static class SaturateNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出饱和值
        }
    }

    /// <summary>向上取整节点</summary>
    public static class CeilingNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出向上取整结果
        }
    }

    /// <summary>向下取整节点</summary>
    public static class FloorNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出向下取整结果
        }
    }

    /// <summary>四舍五入节点</summary>
    public static class RoundNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出四舍五入结果
        }
    }

    /// <summary>符号节点（返回 -1, 0, 1）</summary>
    public static class SignNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出符号
        }
    }

    /// <summary>阶跃节点（x >= edge 返回 1 否则 0）</summary>
    public static class StepNode
    {
        public static class Input
        {
            public const int Edge = 0; // 阈值
            public const int In = 1;   // 输入值
        }

        public static class Output
        {
            public const int Out = 2; // 输出阶跃结果
        }
    }

    /// <summary>截断取整节点（向零取整）</summary>
    public static class TruncateNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出截断结果
        }
    }

    /// <summary>正弦节点</summary>
    public static class SineNode
    {
        public static class Input
        {
            public const int In = 0; // 输入弧度
        }

        public static class Output
        {
            public const int Out = 1; // 输出正弦值
        }
    }

    /// <summary>正切节点</summary>
    public static class TangentNode
    {
        public static class Input
        {
            public const int In = 0; // 输入弧度
        }

        public static class Output
        {
            public const int Out = 1; // 输出正切值
        }
    }

    /// <summary>反余弦节点</summary>
    public static class ArccosineNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值 [-1,1]
        }

        public static class Output
        {
            public const int Out = 1; // 输出弧度
        }
    }

    /// <summary>反正弦节点</summary>
    public static class ArcsineNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值 [-1,1]
        }

        public static class Output
        {
            public const int Out = 1; // 输出弧度
        }
    }

    /// <summary>反正切2节点（atan2）</summary>
    public static class Arctangent2Node
    {
        public static class Input
        {
            public const int A = 0; // Y 坐标
            public const int B = 1; // X 坐标
        }

        public static class Output
        {
            public const int Out = 2; // 输出弧度
        }
    }

    /// <summary>反正切节点</summary>
    public static class ArctangentNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出弧度
        }
    }

    /// <summary>余弦节点</summary>
    public static class CosineNode
    {
        public static class Input
        {
            public const int In = 0; // 输入弧度
        }

        public static class Output
        {
            public const int Out = 1; // 输出余弦值
        }
    }

    /// <summary>度数转弧度节点</summary>
    public static class DegreesToRadiansNode
    {
        public static class Input
        {
            public const int In = 0; // 输入度数
        }

        public static class Output
        {
            public const int Out = 1; // 输出弧度
        }
    }

    /// <summary>双曲余弦节点</summary>
    public static class HyperbolicCosineNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出双曲余弦值
        }
    }

    /// <summary>双曲正弦节点</summary>
    public static class HyperbolicSineNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出双曲正弦值
        }
    }

    /// <summary>双曲正切节点</summary>
    public static class HyperbolicTangentNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出双曲正切值
        }
    }

    /// <summary>弧度转度数节点</summary>
    public static class RadiansToDegreesNode
    {
        public static class Input
        {
            public const int In = 0; // 输入弧度
        }

        public static class Output
        {
            public const int Out = 1; // 输出度数
        }
    }

    /// <summary>叉积节点</summary>
    public static class CrossProductNode
    {
        public static class Input
        {
            public const int A = 0; // 向量 A
            public const int B = 1; // 向量 B
        }

        public static class Output
        {
            public const int Out = 2; // 输出叉积向量
        }
    }

    /// <summary>距离节点</summary>
    public static class DistanceNode
    {
        public static class Input
        {
            public const int A = 0; // 点 A
            public const int B = 1; // 点 B
        }

        public static class Output
        {
            public const int Out = 2; // 输出距离
        }
    }

    /// <summary>点积节点</summary>
    public static class DotProductNode
    {
        public static class Input
        {
            public const int A = 0; // 向量 A
            public const int B = 1; // 向量 B
        }

        public static class Output
        {
            public const int Out = 2; // 输出点积值
        }
    }

    /// <summary>菲涅尔效应节点</summary>
    public static class FresnelEffectNode
    {
        public static class Input
        {
            public const int Normal = 0;  // 法线
            public const int ViewDir = 1; // 视线方向
            public const int Power = 2;   // 指数
        }

        public static class Output
        {
            public const int Out = 3; // 输出菲涅尔值
        }
    }

    /// <summary>投影节点（向量 A 投影到向量 B）</summary>
    public static class ProjectionNode
    {
        public static class Input
        {
            public const int A = 0; // 被投影向量
            public const int B = 1; // 投影方向
        }

        public static class Output
        {
            public const int Out = 2; // 输出投影结果
        }
    }

    /// <summary>反射节点（反射向量）</summary>
    public static class ReflectionNode
    {
        public static class Input
        {
            public const int In = 0;     // 入射向量
            public const int Normal = 1; // 法线
        }

        public static class Output
        {
            public const int Out = 2; // 输出反射向量
        }
    }

    /// <summary>折射节点</summary>
    public static class RefractNode
    {
        public static class Input
        {
            public const int Incident = 0;   // 入射向量
            public const int Normal = 1;     // 法线
            public const int IORSource = 2;  // 源介质折射率
            public const int IORMedium = 3;  // 目标介质折射率
        }

        public static class Output
        {
            public const int Refracted = 4;  // 输出折射向量
            public const int Intensity = 5;  // 输出强度
        }
    }

    /// <summary>拒绝节点（向量 A 垂直于 B 的分量）</summary>
    public static class RejectionNode
    {
        public static class Input
        {
            public const int A = 0; // 向量 A
            public const int B = 1; // 向量 B
        }

        public static class Output
        {
            public const int Out = 2; // 输出垂直分量
        }
    }

    /// <summary>绕轴旋转节点</summary>
    public static class RotateAboutAxisNode
    {
        public static class Input
        {
            public const int In = 0;       // 输入向量
            public const int Axis = 1;     // 旋转轴
            public const int Rotation = 2; // 旋转角度
        }

        public static class Output
        {
            public const int Out = 3; // 输出旋转后的向量
        }
    }

    /// <summary>球体遮罩节点</summary>
    public static class SphereMaskNode
    {
        public static class Input
        {
            public const int Coords = 0;   // 坐标
            public const int Center = 1;   // 中心点
            public const int Radius = 2;   // 半径
            public const int Hardness = 3; // 硬度
        }

        public static class Output
        {
            public const int Out = 4; // 输出遮罩值
        }
    }

    /// <summary>变换节点（坐标空间转换）</summary>
    public static class TransformNode
    {
        public static class Input
        {
            public const int In = 0; // 输入向量
        }

        public static class Output
        {
            public const int Out = 1; // 输出变换后的向量
        }
    }

    /// <summary>噪声正弦波节点</summary>
    public static class NoiseSineWaveNode
    {
        public static class Input
        {
            public const int In = 0;      // 输入值
            public const int MinMax = 1;  // 最小最大值范围
        }

        public static class Output
        {
            public const int Out = 2; // 输出波形值
        }
    }

    /// <summary>锯齿波节点</summary>
    public static class SawtoothWaveNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出波形值
        }
    }

    /// <summary>方波节点</summary>
    public static class SquareWaveNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出波形值
        }
    }

    /// <summary>三角波节点</summary>
    public static class TriangleWaveNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出波形值
        }
    }

    /// <summary>计算变形节点（用于顶点动画）</summary>
    public static class ComputeDeformNode
    {
        public static class Input { }

        public static class Output
        {
            public const int DeformedPosition = 0; // 变形后的位置
            public const int DeformedNormal = 1;   // 变形后的法线
            public const int DeformedTangent = 2;  // 变形后的切线
        }
    }

    /// <summary>线性蒙皮节点（骨骼动画）</summary>
    public static class LinearBlendSkinningNode
    {
        public static class Input
        {
            public const int VertexPosition = 0; // 顶点位置
            public const int VertexNormal = 1;   // 顶点法线
            public const int VertexTangent = 2;  // 顶点切线
        }

        public static class Output
        {
            public const int SkinnedPosition = 3; // 蒙皮后的位置
            public const int kinnedNormal = 4;    // 蒙皮后的法线（注意拼写错误）
            public const int kinnedTangent = 5;   // 蒙皮后的切线
        }
    }

    /// <summary>梯度噪声节点（Perlin 风格）</summary>
    public static class GradientNoiseNode
    {
        public static class Input
        {
            public const int UV = 0;    // UV 坐标
            public const int Scale = 1; // 缩放
        }

        public static class Output
        {
            public const int Out = 2; // 输出噪声值
        }
    }

    /// <summary>简单噪声节点（Value 噪声）</summary>
    public static class SimpleNoiseNode
    {
        public static class Input
        {
            public const int UV = 0;    // UV 坐标
            public const int Scale = 1; // 缩放
        }

        public static class Output
        {
            public const int Out = 2; // 输出噪声值
        }
    }

    /// <summary>Voronoi 噪声节点</summary>
    public static class VoronoiNode
    {
        public static class Input
        {
            public const int UV = 0;           // UV 坐标
            public const int AngleOffset = 1;  // 角度偏移
            public const int CellDensity = 2;  // 细胞密度
        }

        public static class Output
        {
            public const int Out = 3;   // 输出噪声值
            public const int Cells = 4; // 输出细胞颜色
        }
    }

    /// <summary>椭圆节点（生成椭圆形状）</summary>
    public static class EllipseNode
    {
        public static class Input
        {
            public const int UV = 0;      // UV 坐标
            public const int Width = 2;   // 宽度
            public const int Height = 3;  // 高度
        }

        public static class Output
        {
            public const int Out = 4; // 输出形状遮罩
        }
    }

    /// <summary>多边形节点（生成正多边形）</summary>
    public static class PolygonNode
    {
        public static class Input
        {
            public const int Sides = 1;   // 边数
            public const int UV = 0;      // UV 坐标
            public const int Width = 2;   // 宽度
            public const int Height = 3;  // 高度
        }

        public static class Output
        {
            public const int Out = 4; // 输出形状遮罩
        }
    }

    /// <summary>矩形节点</summary>
    public static class RectangleNode
    {
        public static class Input
        {
            public const int UV = 0;      // UV 坐标
            public const int Width = 1;   // 宽度
            public const int Height = 2;  // 高度
        }

        public static class Output
        {
            public const int Out = 3; // 输出形状遮罩
        }
    }

    /// <summary>圆角多边形节点</summary>
    public static class RoundedPolygonNode
    {
        public static class Input
        {
            public const int UV = 0;        // UV 坐标
            public const int Width = 1;     // 宽度
            public const int Height = 2;    // 高度
            public const int Sides = 3;     // 边数
            public const int Roundness = 4; // 圆角程度
        }

        public static class Output
        {
            public const int Out = 5; // 输出形状遮罩
        }
    }

    /// <summary>圆角矩形节点</summary>
    public static class RoundedRectangleNode
    {
        public static class Input
        {
            public const int UV = 0;      // UV 坐标
            public const int Width = 1;   // 宽度
            public const int Height = 2;  // 高度
            public const int Radius = 3;  // 圆角半径
        }

        public static class Output
        {
            public const int Out = 4; // 输出形状遮罩
        }
    }

    /// <summary>棋盘格节点</summary>
    public static class CheckerboardNode
    {
        public static class Input
        {
            public const int UV = 0;         // UV 坐标
            public const int ColorA = 1;     // 颜色 A
            public const int ColorB = 2;     // 颜色 B
            public const int Frequency = 3;  // 频率
        }

        public static class Output
        {
            public const int Out = 4; // 输出棋盘格颜色
        }
    }

    /// <summary>逻辑或节点</summary>
    public static class OrNode
    {
        public static class Input
        {
            public const int A = 0; // 输入 A
            public const int B = 1; // 输入 B
        }

        public static class Output
        {
            public const int Out = 2; // 输出 A || B
        }
    }

    /// <summary>全真节点（所有分量非零）</summary>
    public static class AllNode
    {
        public static class Input
        {
            public const int In = 0; // 输入向量
        }

        public static class Output
        {
            public const int Out = 1; // 输出布尔值
        }
    }

    /// <summary>逻辑与节点</summary>
    public static class AndNode
    {
        public static class Input
        {
            public const int A = 0; // 输入 A
            public const int B = 1; // 输入 B
        }

        public static class Output
        {
            public const int Out = 2; // 输出 A && B
        }
    }

    /// <summary>任意真节点（任一分量非零）</summary>
    public static class AnyNode
    {
        public static class Input
        {
            public const int In = 0; // 输入向量
        }

        public static class Output
        {
            public const int Out = 1; // 输出布尔值
        }
    }

    /// <summary>分支节点（根据条件选择）</summary>
    public static class BranchNode
    {
        public static class Input
        {
            public const int Predicate = 0; // 条件
            public const int True = 1;      // 条件为真时输出
            public const int False = 2;     // 条件为假时输出
        }

        public static class Output
        {
            public const int Out = 3; // 输出选择的结果
        }
    }

    /// <summary>输入连接分支节点（根据输入是否连接选择）</summary>
    public static class BranchOnInputConnectionNode
    {
        public static class Input
        {
            public const int input = 0;          // 检测的输入槽
            public const int Connected = 1;      // 已连接时的值
            public const int NotConnected = 2;   // 未连接时的值
        }

        public static class Output
        {
            public const int Out = 3; // 输出选择的结果
        }
    }

    /// <summary>比较节点</summary>
    public static class ComparisonNode
    {
        public static class Input
        {
            public const int A = 0; // 值 A
            public const int B = 1; // 值 B
        }

        public static class Output
        {
            public const int Out = 2; // 输出比较结果（布尔）
        }
    }

    /// <summary>是否正面节点（判断片元是否面向相机）</summary>
    public static class IsFrontFaceNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = 0; // 输出布尔值
        }
    }

    /// <summary>是否无穷节点</summary>
    public static class IsInfiniteNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出布尔值
        }
    }

    /// <summary>是否 NaN 节点</summary>
    public static class IsNanNode
    {
        public static class Input
        {
            public const int In = 0; // 输入值
        }

        public static class Output
        {
            public const int Out = 1; // 输出布尔值
        }
    }

    /// <summary>逻辑与非节点</summary>
    public static class NandNode
    {
        public static class Input
        {
            public const int A = 0; // 输入 A
            public const int B = 1; // 输入 B
        }

        public static class Output
        {
            public const int Out = 2; // 输出 !(A && B)
        }
    }

    /// <summary>逻辑非节点</summary>
    public static class NotNode
    {
        public static class Input
        {
            public const int In = 0; // 输入布尔值
        }

        public static class Output
        {
            public const int Out = 1; // 输出取反
        }
    }

    /// <summary>视差映射节点</summary>
    public static class ParallaxMappingNode
    {
        public static class Input
        {
            public const int Heightmap = 1;          // 高度图纹理
            public const int HeightmapSampler = 2;   // 高度图采样器
            public const int Amplitude = 3;          // 幅度
            public const int UVs = 4;                // UV 坐标
        }

        public static class Output
        {
            public const int ParallaxUVs = 0; // 输出视差偏移后的 UV
        }
    }

    /// <summary>视差遮挡映射节点（更精确的视差）</summary>
    public static class ParallaxOcclusionMappingNode
    {
        public static class Input
        {
            public const int Heightmap = 2;          // 高度图纹理
            public const int HeightmapSampler = 3;   // 高度图采样器
            public const int Amplitude = 4;          // 幅度
            public const int Steps = 5;              // 采样步数
            public const int UVs = 6;                // UV 坐标
            public const int LOD = 7;                // LOD 级别
            public const int LODThreshold = 8;       // LOD 阈值
            public const int Tiling = 10;            // 平铺
            public const int Offset = 11;            // 偏移
            public const int PrimitiveSize = 12;     // 图元尺寸
        }

        public static class Output
        {
            public const int PixelDepthOffset = 0; // 像素深度偏移
            public const int ParallaxUVs = 1;      // 视差 UV
        }
    }

    /// <summary>极坐标节点</summary>
    public static class PolarCoordinatesNode
    {
        public static class Input
        {
            public const int UV = 0;           // UV 坐标
            public const int Center = 1;       // 中心点
            public const int RadialScale = 2;  // 径向缩放
            public const int LengthScale = 3;  // 长度缩放
        }

        public static class Output
        {
            public const int Out = 4; // 输出极坐标 UV
        }
    }

    /// <summary>径向剪切节点</summary>
    public static class RadialShearNode
    {
        public static class Input
        {
            public const int UV = 0;        // UV 坐标
            public const int Center = 1;    // 中心点
            public const int Strength = 2;  // 强度
            public const int Offset = 3;    // 偏移
        }

        public static class Output
        {
            public const int Out = 4; // 输出扭曲后的 UV
        }
    }

    /// <summary>旋转节点（UV 旋转）</summary>
    public static class RotateNode
    {
        public static class Input
        {
            public const int UV = 0;        // UV 坐标
            public const int Center = 1;    // 中心点
            public const int Rotation = 2;  // 旋转角度
        }

        public static class Output
        {
            public const int Out = 3; // 输出旋转后的 UV
        }
    }

    /// <summary>球面化节点</summary>
    public static class SpherizeNode
    {
        public static class Input
        {
            public const int UV = 0;        // UV 坐标
            public const int Center = 1;    // 中心点
            public const int Strength = 2;  // 强度
            public const int Offset = 3;    // 偏移
        }

        public static class Output
        {
            public const int Out = 3; // 输出球面化后的 UV（注意索引与 Strength 冲突？原代码如此）
        }
    }

    /// <summary>平铺和偏移节点</summary>
    public static class TilingAndOffsetNode
    {
        public static class Input
        {
            public const int UV = 0;      // UV 坐标
            public const int Tiling = 1;  // 平铺
            public const int Offset = 2;  // 偏移
        }

        public static class Output
        {
            public const int Out = 3; // 输出变换后的 UV
        }
    }

    /// <summary>三平面映射节点</summary>
    public static class TriplanarNode
    {
        public static class Input
        {
            public const int Texture = 1;  // 纹理
            public const int Sampler = 2;  // 采样器
            public const int Position = 3; // 世界空间位置
            public const int Normal = 4;   // 法线
            public const int Tile = 5;     // 平铺
            public const int Blend = 6;    // 混合因子
        }

        public static class Output
        {
            public const int Out = 0; // 输出三平面采样颜色
        }
    }

    /// <summary>漩涡节点（UV 漩涡扭曲）</summary>
    public static class TwirlNode
    {
        public static class Input
        {
            public const int UV = 0;        // UV 坐标
            public const int Center = 1;    // 中心点
            public const int Strength = 2;  // 强度
            public const int Offset = 3;    // 偏移
        }

        public static class Output
        {
            public const int Out = 4; // 输出扭曲后的 UV
        }
    }

    /// <summary>Flipbook 节点（序列帧动画）</summary>
    public static class FlipbookNode
    {
        public static class Input
        {
            public const int UV = 0;      // UV 坐标
            public const int Width = 1;   // 列数
            public const int Height = 2;  // 行数
            public const int Tile = 3;    // 当前帧索引
        }

        public static class Output
        {
            public const int Out = 4; // 输出对应的 UV 子区域
        }
    }

    // ============================================================
    // 特殊节点（待完善）
    // ============================================================

    /// <summary>下拉节点（关联下拉列表资产）</summary>
    public static class DropdownNode
    {
        public static class Input { }

        public static class Output
        {
            public const int Out = -999; // 输出当前选中的值（索引）
        }
    }

    /// <summary>关键字节点（关联 Shader Keyword）</summary>
    public static class KeywordNode
    {
        public static class Input
        {
            public const int On = 1;  // 关键字开启时的值
            public const int Off = 2; // 关键字关闭时的值
        }

        public static class Output
        {
            public const int Out = 0; // 输出根据关键字选择的值
        }
    }

    /// <summary>自定义函数节点（动态 HLSL 函数）</summary>
    public static class CustomFunctionNode
    {
        // 输入和输出槽由用户自定义，此处无固定常量
        public static class Input { }
        public static class Output { }
    }

    /// <summary>预览节点（仅用于编辑器预览）</summary>
    public static class PreviewNode
    {
        public static class Input
        {
            public const int In = 0; // 预览输入
        }

        public static class Output
        {
            public const int Out = 1; // 输出原值（透传）
        }
    }

    /// <summary>拆分纹理变换节点（将纹理变换拆分为平铺和偏移）</summary>
    public static class SplitTextureTransformNode
    {
        public static class Input
        {
            public const int In = 0; // 纹理变换输入
        }

        public static class Output
        {
            public const int Tiling = 1;       // 平铺值
            public const int Offset = 2;       // 偏移值
            public const int TextureOnly = 3;  // 仅纹理（不带变换）
        }
    }
    //==========================2D==========================
    public static class LightTextureNode 
    {
        public static class Input
        {
            
        }

        public static class Output
        {
            public const int Out =0;   
        }
    }
    // ========================== UGUI 相关节点 ==========================

    public static class MeterValueNode
    {
        public static class Input { }
        public static class Output
        {
            public const int Value = 0;
        }
    }

    public static class RangeBarValueNode
    {
        public static class Input { }
        public static class Output
        {
            public const int Min = 0;
            public const int Max = 1;
            public const int Direction = 2;
        }
    }

    public static class RectTransformSizeNode
    {
        public static class Input { }
        public static class Output
        {
            public const int Size = 0;
            public const int ScaleFactor = 1;
            public const int PixelPerUnit = 2;
        }
    }

    public static class SelectableBranchNode
    {
        public static class Input
        {
            public const int State = 0;
            public const int Normal = 1;
            public const int Highlighted = 2;
            public const int Pressed = 3;
            public const int Selected = 4;
            public const int Disabled = 5;
        }
        public static class Output
        {
            public const int Out = 6;
        }
    }

    public static class SelectableStateNode
    {
        public static class Input { }
        public static class Output
        {
            public const int State = 0;
        }
    }

    public static class SliderValueNode
    {
        public static class Input { }
        public static class Output
        {
            public const int Value = 0;
            public const int Direction = 1;
        }
    }

    public static class ToggleStateNode
    {
        public static class Input { }
        public static class Output
        {
            public const int State = 0;
        }
    }
}