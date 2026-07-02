# AI ShaderGraph Generator

**AI 驱动的 Unity ShaderGraph 自动生成工具**

将 AI（如 DeepSeek、ChatGPT）生成的 JSON 描述一键转换为完整的 Unity ShaderGraph 资产，支持 URP 和 Built-In 渲染管线，覆盖 100+ 节点类型。

---

## 功能特性

- **AI → ShaderGraph** — 解析 AI 返回的 JSON 结构，自动创建节点、连接、黑板属性和管线配置
- **多管线支持**
  - **URP**: Lit / Unlit / Fullscreen / Canvas / SixWay / Decal / SpriteLit / SpriteUnlit / SpriteCustomLit
  - **Built-In**: Lit / Unlit / Canvas
- **100+ 节点类型** — 覆盖 Input、Math、Artistic、Procedural、UV、Logic 等全类别
- **子图系统** — 端口解析、代码生成、Base64 嵌入式自动重建，跨项目共享子图
- **自动布局** — 拓扑排序 + 最长路径算法，节点自动排列，数据流从左到右
- **可视化窗口** — AI ShaderGraph 生成器 + 子图端口解析器，双击即用
- **渲染状态精细控制** — 表面类型、混合模式、深度测试、面剔除、Alpha Clip 等

---

## 环境要求

| 依赖 | 版本 |
|------|------|
| Unity | 6000.0.68f1 或更高 |
| 渲染管线 | URP 或 Built-In |
| Shader Graph | 内置包（随 Unity 安装） |

---

## 安装

### 方式一：Package Manager (Git URL)

Unity 菜单 → **Window > Package Manager** → 左上角 `+` → **Add package from git URL**：

```
https://github.com/1ongwa2021/AIshaderGraph.git
```

### 方式二：本地导入

将本仓库克隆或下载到 Unity 项目的 `Packages/` 目录下。

---

## 快速开始

1. Unity 菜单 → **Tools > 龙哥的秘密花园 > AI ShaderGraph 生成器**
2. 选择目标目录（或在 Project 窗口中选中文件夹后点击"使用 Project 窗口中选中的目录"）
3. 将 AI 返回的 JSON 粘贴到文本框中
4. 点击 **生成 ShaderGraph**
5. 生成的 `.shadergraph` 资产自动出现在目标目录，并在检视面板中选中

### JSON 格式示例

```json
{
  "Pipeline": "URP",
  "Target": "Lit",
  "Blackboard": [
    { "Name": "_MainTex", "Type": "Texture2D", "DefaultValue": "white" },
    { "Name": "_BaseColor", "Type": "Color", "DefaultValue": [1.0, 1.0, 1.0, 1.0] },
    { "Name": "_Metallic", "Type": "Float", "DefaultValue": 0.0 },
    { "Name": "_Smoothness", "Type": "Float", "DefaultValue": 0.5 }
  ],
  "Nodes": [
    { "Id": "UV0", "Type": "UVNode" },
    { "Id": "Time", "Type": "TimeNode" },
    { "Id": "MainTex", "Type": "PropertyNode", "PropertyName": "_MainTex" },
    { "Id": "SampleTex", "Type": "SampleTexture2DNode" },
    { "Id": "Multiply", "Type": "MultiplyNode" },
    { "Id": "BaseColor", "Type": "PropertyNode", "PropertyName": "_BaseColor" }
  ],
  "Connections": [
    { "From": "UV0", "FromSlot": "UVNode.Output.Out", "To": "SampleTex", "ToSlot": "SampleTexture2DNode.Input.UV" },
    { "From": "Time", "FromSlot": "TimeNode.Output.Time", "To": "Multiply", "ToSlot": "MultiplyNode.Input.A" },
    { "From": "SampleTex", "FromSlot": "SampleTexture2DNode.Output.RGBA", "To": "Multiply", "ToSlot": "MultiplyNode.Input.B" }
  ],
  "Blocks": [
    { "Slot": "BaseColor", "Source": { "NodeId": "Multiply", "SlotName": "MultiplyNode.Output.Out" } }
  ],
  "GraphSettings": {
    "SurfaceType": "Opaque",
    "BlendMode": "Alpha",
    "RenderFace": "Front",
    "AlphaClip": false
  }
}
```

---

## 项目结构

```
AIshaderGraph/
├── package.json              # UPM 包清单
├── README.md                 # 本文档
├── AI助手/                   # 核心功能目录
│   ├── DeepSeekChat.asmdef   # Editor 程序集定义
│   ├── 节点帮助器/节点/
│   │   ├── AIShaderGraphGeneratorWindow.cs  # 主窗口
│   │   ├── ColorNodeParams.cs               # 节点参数类
│   │   ├── ShaderGraphNodeType.cs           # 节点类型枚举
│   │   ├── Slots.cs                         # 插槽定义
│   │   └── 重构项目/                        # 重构核心
│   │       ├── ShaderGraphBuilder.cs        # JSON → ShaderGraph 构建引擎
│   │       ├── GraphDataContext.cs          # 图操作上下文
│   │       ├── ShaderGraphReflectionHelper.cs # 反射基础设施
│   │       ├── NodeLayoutHelper.cs          # 自动布局算法
│   │       ├── NodeParameterConverter.cs    # 参数转换器
│   │       ├── ShaderPropertyFactory.cs     # 黑板属性工厂
│   │       ├── URPShaderGraphHelper.cs      # URP 管线辅助
│   │       ├── BuiltInShaderGraphHelper.cs  # Built-In 管线辅助
│   │       └── 子图/                        # 子图系统
│   │           ├── SubGraphResolver.cs      # 子图解析器
│   │           ├── SubGraphPortsAttribute.cs # 子图端口标记
│   │           ├── EnumConverter.cs         # 枚举转换
│   │           └── 子图编辑器/              # 子图可视化编辑
│   │               ├── SubGraphParserWindow.cs  # 端口解析窗口
│   │               ├── SubGraphBuilder.cs       # 子图构建器
│   │               ├── SubGraphRebuilder.cs     # Base64 重建器
│   │               ├── SubGraphJsonExporter.cs  # JSON 导出器
│   │               └── SubGraphDataTypes.cs     # 数据结构类型
│   └── CreateUnlitShaderGraphPureReflection.cs # 测试与示例
└── 测试位置/                   # Shader 测试资产
```

---

## 架构概览

```
AI JSON ──► ShaderGraphBuilder
                │
                ├── GraphDataContext (操作上下文 + 资产缓存)
                │       ├── ShaderGraphReflectionHelper (反射层)
                │       ├── URPShaderGraphHelper / BuiltInShaderGraphHelper
                │       ├── ShaderPropertyFactory (黑板属性)
                │       └── NodeLayoutHelper (自动布局)
                │
                └── 输出: .shadergraph 资产
```

- **ShaderGraphBuilder**: 核心入口，解析 JSON，按步骤构建完整图
- **GraphDataContext**: 单个资产的上下文，管理节点添加/连接/保存
- **ShaderGraphReflectionHelper**: 通过反射访问 ShaderGraph 内部 API（类型查找、GraphData 序列化）
- **管线辅助**: 封装 URP/Built-In 的 Target 激活、SubTarget 设置、块节点管理
- **子图系统**: 支持解析子图端口、生成 C# 嵌入式代码、Base64 自动重建

---

## 子图系统

### 端口解析器

Unity 菜单 → **Tools > 龙哥的秘密花园 > 子图端口解析器**

选择一个 `.shadersubgraph` 资产，自动解析其输入/输出端口，生成包含 Base64 的 C# 静态类代码，支持跨项目自动重建子图。

### 嵌入式重建

在代码中添加 `[SubGraphPorts]` 特性标记静态类，嵌入子图数据的 Base64 字符串。当解析器找不到子图资产时，自动从嵌入数据重建。

---

## 开发

本项目为 Editor-only 程序集，所有代码仅在 Unity Editor 中运行，不影响运行时构建。

如需添加新的 ShaderGraph 节点类型：
1. 在 `ShaderGraphNodeType.cs` 添加枚举值
2. 在 `Slots.cs` 中定义插槽常量
3. 在 `NodeParameterConverter.cs` 添加参数映射
4. （可选）在 `ColorNodeParams.cs` 添加参数类

---

## 许可证

MIT