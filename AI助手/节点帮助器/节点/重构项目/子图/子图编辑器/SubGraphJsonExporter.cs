using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace 龙哥的秘密花园.ShaderGraphBuilder
{
    public static class SubGraphJsonExporter
    {
        // 完整类型名到简写名的映射（已包含 Node 后缀）
       private static readonly Dictionary<string, string> NodeTypeShortNameMap = new Dictionary<string, string>
{
    { "UnityEditor.ShaderGraph.ColorNode", "ColorNode" },
    { "UnityEditor.ShaderGraph.BooleanNode", "BooleanNode" },
    { "UnityEditor.ShaderGraph.ConstantNode", "ConstantNode" },
    { "UnityEditor.ShaderGraph.IntegerNode", "IntegerNode" },
    { "UnityEditor.ShaderGraph.SliderNode", "SliderNode" },
    { "UnityEditor.ShaderGraph.TimeNode", "TimeNode" },
    { "UnityEditor.ShaderGraph.Vector1Node", "Vector1Node" },
    { "UnityEditor.ShaderGraph.Vector2Node", "Vector2Node" },
    { "UnityEditor.ShaderGraph.Vector3Node", "Vector3Node" },
    { "UnityEditor.ShaderGraph.Vector4Node", "Vector4Node" },
    { "UnityEditor.ShaderGraph.InstanceIDNode", "InstanceIDNode" },
    { "UnityEditor.ShaderGraph.NormalVectorNode", "NormalVectorNode" },
    { "UnityEditor.ShaderGraph.PositionNode", "PositionNode" },
    { "UnityEditor.ShaderGraph.ScreenPositionNode", "ScreenPositionNode" },
    { "UnityEditor.ShaderGraph.TangentVectorNode", "TangentVectorNode" },
    { "UnityEditor.ShaderGraph.UVNode", "UVNode" },
    { "UnityEditor.ShaderGraph.VertexColorNode", "VertexColorNode" },
    { "UnityEditor.ShaderGraph.VertexIDNode", "VertexIDNode" },
    { "UnityEditor.ShaderGraph.ViewDirectionNode", "ViewDirectionNode" },
    { "UnityEditor.ShaderGraph.ViewVectorNode", "ViewVectorNode" },
    { "UnityEditor.ShaderGraph.BitangentVectorNode", "BitangentVectorNode" },
    { "UnityEditor.ShaderGraph.BlackbodyNode", "BlackbodyNode" },
    { "UnityEditor.ShaderGraph.GradientNode", "GradientNode" },
    { "UnityEditor.ShaderGraph.SampleGradient", "SampleGradientNode" },
    { "UnityEditor.ShaderGraph.AmbientNode", "AmbientNode" },
    { "UnityEditor.ShaderGraph.BakedGINode", "BakedGINode" },
    { "UnityEditor.ShaderGraph.MainLightDirectionNode", "MainLightDirectionNode" },
    { "UnityEditor.ShaderGraph.ReflectionProbeNode", "ReflectionProbeNode" },
    { "UnityEditor.ShaderGraph.Matrix2Node", "Matrix2Node" },
    { "UnityEditor.ShaderGraph.Matrix3Node", "Matrix3Node" },
    { "UnityEditor.ShaderGraph.Matrix4Node", "Matrix4Node" },
    { "UnityEditor.ShaderGraph.TransformationMatrixNode", "TransformationMatrixNode" },
    { "UnityEditor.ShaderGraph.DielectricSpecularNode", "DielectricSpecularNode" },
    { "UnityEditor.ShaderGraph.MetalReflectanceNode", "MetalReflectanceNode" },
    { "UnityEditor.ShaderGraph.SceneDepthNode", "SceneDepthNode" },
    { "UnityEditor.ShaderGraph.SceneDepthDifferenceNode", "SceneDepthDifferenceNode" },
    { "UnityEditor.ShaderGraph.SampleTexture2DNode", "SampleTexture2DNode" },
    { "UnityEditor.ShaderGraph.SampleTexture2DLODNode", "SampleTexture2DLODNode" },
    { "UnityEditor.ShaderGraph.SampleTexture2DArrayNode", "SampleTexture2DArrayNode" },
    { "UnityEditor.ShaderGraph.SampleTexture3DNode", "SampleTexture3DNode" },
    { "UnityEditor.ShaderGraph.SampleVirtualTextureNode", "SampleVirtualTextureNode" },
    { "UnityEditor.ShaderGraph.SamplerStateNode", "SamplerStateNode" },
    { "UnityEditor.ShaderGraph.CalculateLevelOfDetailTexture2DNode", "CalculateLevelOfDetailTexture2DNode" },
    { "UnityEditor.ShaderGraph.Texture2DAssetNode", "Texture2DAssetNode" },
    { "UnityEditor.ShaderGraph.Texture2DArrayAssetNode", "Texture2DArrayAssetNode" },
    { "UnityEditor.ShaderGraph.Texture3DAssetNode", "Texture3DAssetNode" },
    { "UnityEditor.ShaderGraph.CubemapAssetNode", "CubemapAssetNode" },
    { "UnityEditor.ShaderGraph.SampleCubemapNode", "SampleCubemapNode" },
    { "UnityEditor.ShaderGraph.SampleRawCubemapNode", "SampleRawCubemapNode" },
    { "UnityEditor.ShaderGraph.GatherTexture2DNode", "GatherTexture2DNode" },
    { "UnityEditor.ShaderGraph.Texture2DPropertiesNode", "TexelSizeNode" },
    { "UnityEditor.ShaderGraph.CustomInterpolatorNode", "CustomInterpolatorNode" },
    { "UnityEditor.ShaderGraph.PropertyNode", "PropertyNode" },
    { "UnityEditor.ShaderGraph.AddNode", "AddNode" },
    { "UnityEditor.ShaderGraph.ChannelMixerNode", "ChannelMixerNode" },
    { "UnityEditor.ShaderGraph.ContrastNode", "ContrastNode" },
    { "UnityEditor.ShaderGraph.HueNode", "HueNode" },
    { "UnityEditor.ShaderGraph.InvertColorsNode", "InvertColorsNode" },
    { "UnityEditor.ShaderGraph.ReplaceColorNode", "ReplaceColorNode" },
    { "UnityEditor.ShaderGraph.SaturationNode", "SaturationNode" },
    { "UnityEditor.ShaderGraph.WhiteBalanceNode", "WhiteBalanceNode" },
    { "UnityEditor.ShaderGraph.BlendNode", "BlendNode" },
    { "UnityEditor.ShaderGraph.DitherNode", "DitherNode" },
    { "UnityEditor.ShaderGraph.FadeTransitionNode", "FadeTransitionNode" },
    { "UnityEditor.ShaderGraph.ChannelMaskNode", "ChannelMaskNode" },
    { "UnityEditor.ShaderGraph.ColorMaskNode", "ColorMaskNode" },
    { "UnityEditor.ShaderGraph.NormalBlendNode", "NormalBlendNode" },
    { "UnityEditor.ShaderGraph.NormalFromHeightNode", "NormalFromHeightNode" },
    { "UnityEditor.ShaderGraph.NormalFromTextureNode", "NormalFromTextureNode" },
    { "UnityEditor.ShaderGraph.NormalReconstructZNode", "NormalReconstructZNode" },
    { "UnityEditor.ShaderGraph.NormalStrengthNode", "NormalStrengthNode" },
    { "UnityEditor.ShaderGraph.NormalUnpackNode", "NormalUnpackNode" },
    { "UnityEditor.ShaderGraph.ColorspaceConversionNode", "ColorspaceConversionNode" },
    { "UnityEditor.ShaderGraph.CombineNode", "CombineNode" },
    { "UnityEditor.ShaderGraph.FlipNode", "FlipNode" },
    { "UnityEditor.ShaderGraph.SplitNode", "SplitNode" },
    { "UnityEditor.ShaderGraph.SwizzleNode", "SwizzleNode" },
    { "UnityEditor.ShaderGraph.AbsoluteNode", "AbsoluteNode" },
    { "UnityEditor.ShaderGraph.ExponentialNode", "ExponentialNode" },
    { "UnityEditor.ShaderGraph.LengthNode", "LengthNode" },
    { "UnityEditor.ShaderGraph.LogNode", "LogNode" },
    { "UnityEditor.ShaderGraph.ModuloNode", "ModuloNode" },
    { "UnityEditor.ShaderGraph.NegateNode", "NegateNode" },
    { "UnityEditor.ShaderGraph.NormalizeNode", "NormalizeNode" },
    { "UnityEditor.ShaderGraph.PosterizeNode", "PosterizeNode" },
    { "UnityEditor.ShaderGraph.ReciprocalNode", "ReciprocalNode" },
    { "UnityEditor.ShaderGraph.ReciprocalSquareRootNode", "ReciprocalSquareRootNode" },
    { "UnityEditor.ShaderGraph.DivideNode", "DivideNode" },
    { "UnityEditor.ShaderGraph.MultiplyNode", "MultiplyNode" },
    { "UnityEditor.ShaderGraph.PowerNode", "PowerNode" },
    { "UnityEditor.ShaderGraph.SquareRootNode", "SquareRootNode" },
    { "UnityEditor.ShaderGraph.SubtractNode", "SubtractNode" },
    { "UnityEditor.ShaderGraph.DDXNode", "DDXNode" },
    { "UnityEditor.ShaderGraph.DDXYNode", "DDXYNode" },
    { "UnityEditor.ShaderGraph.DDYNode", "DDYNode" },
    { "UnityEditor.ShaderGraph.InverseLerpNode", "InverseLerpNode" },
    { "UnityEditor.ShaderGraph.LerpNode", "LerpNode" },
    { "UnityEditor.ShaderGraph.SmoothstepNode", "SmoothstepNode" },
    { "UnityEditor.ShaderGraph.MatrixConstructionNode", "MatrixConstructionNode" },
    { "UnityEditor.ShaderGraph.MatrixDeterminantNode", "MatrixDeterminantNode" },
    { "UnityEditor.ShaderGraph.MatrixSplitNode", "MatrixSplitNode" },
    { "UnityEditor.ShaderGraph.MatrixTransposeNode", "MatrixTransposeNode" },
    { "UnityEditor.ShaderGraph.ClampNode", "ClampNode" },
    { "UnityEditor.ShaderGraph.FractionNode", "FractionNode" },
    { "UnityEditor.ShaderGraph.MaximumNode", "MaximumNode" },
    { "UnityEditor.ShaderGraph.MinimumNode", "MinimumNode" },
    { "UnityEditor.ShaderGraph.OneMinusNode", "OneMinusNode" },
    { "UnityEditor.ShaderGraph.RandomRangeNode", "RandomRangeNode" },
    { "UnityEditor.ShaderGraph.RemapNode", "RemapNode" },
    { "UnityEditor.ShaderGraph.SaturateNode", "SaturateNode" },
    { "UnityEditor.ShaderGraph.CeilingNode", "CeilingNode" },
    { "UnityEditor.ShaderGraph.FloorNode", "FloorNode" },
    { "UnityEditor.ShaderGraph.RoundNode", "RoundNode" },
    { "UnityEditor.ShaderGraph.SignNode", "SignNode" },
    { "UnityEditor.ShaderGraph.StepNode", "StepNode" },
    { "UnityEditor.ShaderGraph.TruncateNode", "TruncateNode" },
    { "UnityEditor.ShaderGraph.SineNode", "SineNode" },
    { "UnityEditor.ShaderGraph.TangentNode", "TangentNode" },
    { "UnityEditor.ShaderGraph.ArccosineNode", "ArccosineNode" },
    { "UnityEditor.ShaderGraph.ArcsineNode", "ArcsineNode" },
    { "UnityEditor.ShaderGraph.Arctangent2Node", "Arctangent2Node" },
    { "UnityEditor.ShaderGraph.ArctangentNode", "ArctangentNode" },
    { "UnityEditor.ShaderGraph.CosineNode", "CosineNode" },
    { "UnityEditor.ShaderGraph.DegreesToRadiansNode", "DegreesToRadiansNode" },
    { "UnityEditor.ShaderGraph.HyperbolicCosineNode", "HyperbolicCosineNode" },
    { "UnityEditor.ShaderGraph.HyperbolicSineNode", "HyperbolicSineNode" },
    { "UnityEditor.ShaderGraph.HyperbolicTangentNode", "HyperbolicTangentNode" },
    { "UnityEditor.ShaderGraph.RadiansToDegreesNode", "RadiansToDegreesNode" },
    { "UnityEditor.ShaderGraph.CrossProductNode", "CrossProductNode" },
    { "UnityEditor.ShaderGraph.DistanceNode", "DistanceNode" },
    { "UnityEditor.ShaderGraph.DotProductNode", "DotProductNode" },
    { "UnityEditor.ShaderGraph.FresnelNode", "FresnelEffectNode" },
    { "UnityEditor.ShaderGraph.ProjectionNode", "ProjectionNode" },
    { "UnityEditor.ShaderGraph.ReflectionNode", "ReflectionNode" },
    { "UnityEditor.ShaderGraph.RefractNode", "RefractNode" },
    { "UnityEditor.ShaderGraph.RejectionNode", "RejectionNode" },
    { "UnityEditor.ShaderGraph.RotateAboutAxisNode", "RotateAboutAxisNode" },
    { "UnityEditor.ShaderGraph.SphereMaskNode", "SphereMaskNode" },
    { "UnityEditor.ShaderGraph.TransformNode", "TransformNode" },
    { "UnityEditor.ShaderGraph.NoiseSineWaveNode", "NoiseSineWaveNode" },
    { "UnityEditor.ShaderGraph.SawtoothWaveNode", "SawtoothWaveNode" },
    { "UnityEditor.ShaderGraph.SquareWaveNode", "SquareWaveNode" },
    { "UnityEditor.ShaderGraph.TriangleWaveNode", "TriangleWaveNode" },
    { "UnityEditor.ShaderGraph.ComputeDeformNode", "ComputeDeformNode" },
    { "UnityEditor.ShaderGraph.LinearBlendSkinningNode", "LinearBlendSkinningNode" },
    { "UnityEditor.ShaderGraph.GradientNoiseNode", "GradientNoiseNode" },
    { "UnityEditor.ShaderGraph.NoiseNode", "SimpleNoiseNode" },
    { "UnityEditor.ShaderGraph.VoronoiNode", "VoronoiNode" },
    { "UnityEditor.ShaderGraph.EllipseNode", "EllipseNode" },
    { "UnityEditor.ShaderGraph.PolygonNode", "PolygonNode" },
    { "UnityEditor.ShaderGraph.RectangleNode", "RectangleNode" },
    { "UnityEditor.ShaderGraph.RoundedPolygonNode", "RoundedPolygonNode" },
    { "UnityEditor.ShaderGraph.RoundedRectangleNode", "RoundedRectangleNode" },
    { "UnityEditor.ShaderGraph.CheckerboardNode", "CheckerboardNode" },
    { "UnityEditor.ShaderGraph.OrNode", "OrNode" },
    { "UnityEditor.ShaderGraph.AllNode", "AllNode" },
    { "UnityEditor.ShaderGraph.AndNode", "AndNode" },
    { "UnityEditor.ShaderGraph.AnyNode", "AnyNode" },
    { "UnityEditor.ShaderGraph.BranchNode", "BranchNode" },
    { "UnityEditor.ShaderGraph.BranchOnInputConnectionNode", "BranchOnInputConnectionNode" },
    { "UnityEditor.ShaderGraph.ComparisonNode", "ComparisonNode" },
    { "UnityEditor.ShaderGraph.IsFrontFaceNode", "IsFrontFaceNode" },
    { "UnityEditor.ShaderGraph.IsInfiniteNode", "IsInfiniteNode" },
    { "UnityEditor.ShaderGraph.IsNanNode", "IsNanNode" },
    { "UnityEditor.ShaderGraph.NandNode", "NandNode" },
    { "UnityEditor.ShaderGraph.NotNode", "NotNode" },
    { "UnityEditor.ShaderGraph.ParallaxMappingNode", "ParallaxMappingNode" },
    { "UnityEditor.ShaderGraph.ParallaxOcclusionMappingNode", "ParallaxOcclusionMappingNode" },
    { "UnityEditor.ShaderGraph.PolarCoordinatesNode", "PolarCoordinatesNode" },
    { "UnityEditor.ShaderGraph.RadialShearNode", "RadialShearNode" },
    { "UnityEditor.ShaderGraph.RotateNode", "RotateNode" },
    { "UnityEditor.ShaderGraph.SpherizeNode", "SpherizeNode" },
    { "UnityEditor.ShaderGraph.TilingAndOffsetNode", "TilingAndOffsetNode" },
    { "UnityEditor.ShaderGraph.TriplanarNode", "TriplanarNode" },
    { "UnityEditor.ShaderGraph.TwirlNode", "TwirlNode" },
    { "UnityEditor.ShaderGraph.FlipbookNode", "FlipbookNode" },
    { "UnityEditor.ShaderGraph.DropdownNode", "DropdownNode" },
    { "UnityEditor.ShaderGraph.KeywordNode", "KeywordNode" },
    { "UnityEditor.ShaderGraph.CustomFunctionNode", "CustomFunctionNode" },
    { "UnityEditor.ShaderGraph.PreviewNode", "PreviewNode" },
    { "UnityEditor.ShaderGraph.SplitTextureTransformNode", "SplitTextureTransformNode" },
    { "UnityEditor.ShaderGraph.SubGraphNode", "SubGraphNode" },
    { "UnityEditor.Rendering.Universal.UniversalSampleBufferNode", "UniversalSampleBufferNode" },
    { "UnityEngine.Experimental.Rendering.Universal.LightTextureNode", "LightTextureNode" },
    { "UnityEditor.ShaderGraph.MeterValueNode", "MeterValueNode" },
    { "UnityEditor.ShaderGraph.RangeBarNode", "RangeBarValueNode" },
    { "UnityEditor.ShaderGraph.RectTransformSizeNode", "RectTransformSizeNode" },
    { "UnityEditor.ShaderGraph.SelectableBranchNode", "SelectableBranchNode" },
    { "UnityEditor.ShaderGraph.SelectableStateNode", "SelectableStateNode" },
    { "UnityEditor.ShaderGraph.SliderValueNode", "SliderValueNode" },
    { "UnityEditor.ShaderGraph.ToggleStateNode", "ToggleStateNode" },
};

        public static string ExportToJson(UnityEngine.Object subGraphAsset)
        {
            string assetPath = AssetDatabase.GetAssetPath(subGraphAsset);
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
                throw new Exception("无法获取子图资产的文件路径");

            string fileContent = File.ReadAllText(assetPath);
            var objects = ParseMultiJsonObjects(fileContent);

            GraphDataRoot graphData = null;
            foreach (var obj in objects)
            {
                if (obj.type == "UnityEditor.ShaderGraph.GraphData")
                {
                    graphData = JsonUtility.FromJson<GraphDataRoot>(obj.json);
                    break;
                }
            }
            if (graphData == null) throw new Exception("找不到 GraphData");

            var objectMap = new Dictionary<string, string>();
            foreach (var obj in objects)
            {
                var id = ExtractJsonValue(obj.json, "m_ObjectId");
                if (!string.IsNullOrEmpty(id))
                    objectMap[id] = obj.json;
            }

            var sb = new StringBuilder();
            sb.AppendLine("{");

            // Blackboard
            sb.AppendLine("  \"Blackboard\": [");
            var propertyList = new List<PropertyInfo>();
            foreach (var propRef in graphData.m_Properties)
            {
                if (objectMap.TryGetValue(propRef.m_Id, out string propJson))
                {
                    var propInfo = ExtractPropertyInfo(propJson);
                    if (propInfo != null) propertyList.Add(propInfo);
                }
            }
            for (int i = 0; i < propertyList.Count; i++)
            {
                var p = propertyList[i];
                sb.AppendLine("    {");
                sb.AppendLine($"      \"Name\": \"{p.referenceName}\",");
                sb.AppendLine($"      \"Type\": \"{p.propertyType}\",");
                sb.AppendLine($"      \"DefaultValue\": {p.defaultValue}");
                sb.AppendLine("    }" + (i < propertyList.Count - 1 ? "," : ""));
            }
            sb.AppendLine("  ],");

            // Nodes
            sb.AppendLine("  \"Nodes\": [");
            var nodeList = new List<NodeInfo>();
            foreach (var nodeRef in graphData.m_Nodes)
            {
                if (objectMap.TryGetValue(nodeRef.m_Id, out string nodeJson))
                {
                    var nodeInfo = ExtractNodeInfo(nodeJson, objectMap);
                    if (nodeInfo != null && nodeInfo.type != "SubGraphOutputNode")
                        nodeList.Add(nodeInfo);
                }
            }
            for (int i = 0; i < nodeList.Count; i++)
            {
                var node = nodeList[i];
                sb.AppendLine("    {");
                sb.AppendLine($"      \"Id\": \"{node.id}\",");
                sb.AppendLine($"      \"Type\": \"{node.type}\",");
                if (!string.IsNullOrEmpty(node.propertyName))
                    sb.AppendLine($"      \"PropertyName\": \"{node.propertyName}\",");
                if (node.inputs.Count > 0)
                {
                    sb.AppendLine("      \"Inputs\": {");
                    var inputKeys = new List<string>(node.inputs.Keys);
                    for (int j = 0; j < inputKeys.Count; j++)
                    {
                        var key = inputKeys[j];
                        var val = node.inputs[key];
                        sb.AppendLine($"        \"{key}\": {val}" + (j < inputKeys.Count - 1 ? "," : ""));
                    }
                    sb.AppendLine("      },");
                }
                sb.AppendLine("      \"Outputs\": {");
                var outputKeys = new List<string>(node.outputs.Keys);
                for (int j = 0; j < outputKeys.Count; j++)
                {
                    var key = outputKeys[j];
                    var val = node.outputs[key];
                    sb.AppendLine($"        \"{key}\": \"{val}\"" + (j < outputKeys.Count - 1 ? "," : ""));
                }
                sb.AppendLine("      }");
                if (node.customSettings.Count > 0)
                {
                    sb.AppendLine("      ,\"Settings\": {");
                    var settingKeys = new List<string>(node.customSettings.Keys);
                    for (int j = 0; j < settingKeys.Count; j++)
                    {
                        var key = settingKeys[j];
                        var val = node.customSettings[key];
                        sb.AppendLine($"        \"{key}\": \"{val}\"" + (j < settingKeys.Count - 1 ? "," : ""));
                    }
                    sb.AppendLine("      }");
                }
                sb.AppendLine("    }" + (i < nodeList.Count - 1 ? "," : ""));
            }
            sb.AppendLine("  ],");

            // Connections
            sb.AppendLine("  \"Connections\": [");
            var edgeList = new List<EdgeInfo>();
            foreach (var edgeRef in graphData.m_Edges)
            {
                var edgeInfo = ExtractEdgeInfo(edgeRef, nodeList, objectMap);
                if (edgeInfo != null) edgeList.Add(edgeInfo);
            }
            for (int i = 0; i < edgeList.Count; i++)
            {
                var e = edgeList[i];
                sb.AppendLine("    {");
                sb.AppendLine($"      \"From\": \"{e.fromNodeId}\",");
                sb.AppendLine($"      \"FromSlot\": \"{e.fromSlot}\",");
                sb.AppendLine($"      \"To\": \"{e.toNodeId}\",");
                sb.AppendLine($"      \"ToSlot\": \"{e.toSlot}\"");
                sb.AppendLine("    }" + (i < edgeList.Count - 1 ? "," : ""));
            }
            sb.AppendLine("  ]");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static List<MultiJsonObject> ParseMultiJsonObjects(string content)
        {
            var result = new List<MultiJsonObject>();
            var parts = content.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.StartsWith("{"))
                {
                    var type = ExtractJsonValue(trimmed, "m_Type");
                    var id = ExtractJsonValue(trimmed, "m_ObjectId");
                    result.Add(new MultiJsonObject { type = type, id = id, json = trimmed });
                }
            }
            return result;
        }

        private static string ExtractJsonValue(string json, string key)
        {
            var pattern = $"\"{key}\"\\s*:\\s*\"([^\"]*)\"";
            var match = System.Text.RegularExpressions.Regex.Match(json, pattern);
            if (match.Success) return match.Groups[1].Value;

            pattern = $"\"{key}\"\\s*:\\s*([^,}}\\]]*)";
            match = System.Text.RegularExpressions.Regex.Match(json, pattern);
            if (match.Success) return match.Groups[1].Value.Trim();

            return null;
        }

        private static PropertyInfo ExtractPropertyInfo(string json)
        {
            var info = new PropertyInfo();
            info.referenceName = ExtractJsonValue(json, "m_DefaultReferenceName");
            if (string.IsNullOrEmpty(info.referenceName))
                info.referenceName = ExtractJsonValue(json, "m_Name");
            var typeStr = ExtractJsonValue(json, "m_Type");
            info.propertyType = GetPropertyTypeFromString(typeStr);
            info.defaultValue = ExtractJsonValue(json, "m_Value") ?? "0.0";
            return info;
        }

        private static string GetPropertyTypeFromString(string typeStr)
        {
            if (string.IsNullOrEmpty(typeStr)) return "Float";
            if (typeStr.Contains("Matrix2")) return "Matrix2";
            if (typeStr.Contains("Matrix3")) return "Matrix3";
            if (typeStr.Contains("Matrix4")) return "Matrix4";
            if (typeStr.Contains("Vector1")) return "Float";
            if (typeStr.Contains("Vector2")) return "Vector2";
            if (typeStr.Contains("Vector3")) return "Vector3";
            if (typeStr.Contains("Vector4")) return "Vector4";
            if (typeStr.Contains("Color")) return "Color";
            if (typeStr.Contains("Boolean")) return "Boolean";
            if (typeStr.Contains("Texture2DArray")) return "Texture2DArray";
            if (typeStr.Contains("Texture2D")) return "Texture2D";
            if (typeStr.Contains("Texture3D")) return "Texture3D";
            if (typeStr.Contains("Cubemap")) return "Cubemap";
            if (typeStr.Contains("Gradient")) return "Gradient";
            if (typeStr.Contains("SamplerState")) return "SamplerState";
            if (typeStr.Contains("VirtualTexture")) return "VirtualTexture";
            return "Float";
        }

        private static NodeInfo ExtractNodeInfo(string json, Dictionary<string, string> objectMap)
        {
            var info = new NodeInfo();
            info.id = ExtractJsonValue(json, "m_ObjectId");
            var fullType = ExtractJsonValue(json, "m_Type");
            info.type = GetNodeShortType(fullType);

            // PropertyNode
            if (fullType == "UnityEditor.ShaderGraph.PropertyNode")
            {
                var propId = ExtractJsonValue(json, "m_Property");
                if (!string.IsNullOrEmpty(propId) && objectMap.TryGetValue(propId, out string propJson))
                {
                    info.propertyName = ExtractJsonValue(propJson, "m_DefaultReferenceName")
                                     ?? ExtractJsonValue(propJson, "m_Name");
                }
            }

            // CustomFunctionNode
            if (fullType == "UnityEditor.ShaderGraph.CustomFunctionNode")
            {
                info.customSettings["sourceType"] = ExtractJsonValue(json, "m_SourceType") ?? "File";
                info.customSettings["functionName"] = ExtractJsonValue(json, "m_FunctionName") ?? "";
                info.customSettings["functionSource"] = ExtractJsonValue(json, "m_FunctionSource") ?? "";
                info.customSettings["functionBody"] = ExtractJsonValue(json, "m_FunctionBody") ?? "";
                var usePragmas = ExtractJsonValue(json, "m_FunctionSourceUsePragmas");
                info.customSettings["usePragmas"] = usePragmas ?? "true";
            }

            // 提取插槽
            var slotsPattern = "\"m_Slots\"\\s*:\\s*\\[";
            var slotsStart = json.IndexOf(slotsPattern);
            if (slotsStart >= 0)
            {
                var slotIds = new List<string>();
                var idPattern = "\"m_Id\"\\s*:\\s*\"([^\"]*)\"";
                var matches = System.Text.RegularExpressions.Regex.Matches(json, idPattern);
                foreach (System.Text.RegularExpressions.Match m in matches)
                {
                    slotIds.Add(m.Groups[1].Value);
                }

                foreach (var slotId in slotIds)
                {
                    if (objectMap.TryGetValue(slotId, out string slotJson))
                    {
                        var slotType = ExtractJsonValue(slotJson, "m_SlotType");
                        var displayName = ExtractJsonValue(slotJson, "m_DisplayName");
                        if (string.IsNullOrEmpty(displayName)) continue;

                        if (slotType == "0")
                        {
                            var value = ExtractJsonValue(slotJson, "m_Value");
                            if (!string.IsNullOrEmpty(value))
                                info.inputs[displayName] = value;
                        }
                        else if (slotType == "1")
                        {
                            info.outputs[displayName] = $"{info.type}.Output.{displayName}";
                        }
                    }
                }
            }

            return info;
        }

        private static EdgeInfo ExtractEdgeInfo(EdgeData edge, List<NodeInfo> nodes, Dictionary<string, string> objectMap)
        {
            var info = new EdgeInfo();
            info.fromNodeId = edge.m_OutputSlot.m_Node.m_Id;
            info.toNodeId = edge.m_InputSlot.m_Node.m_Id;

            var fromNode = nodes.Find(n => n.id == info.fromNodeId);
            var toNode = nodes.Find(n => n.id == info.toNodeId);
            if (fromNode == null || toNode == null) return null;

            string fromSlotName = null, toSlotName = null;

            if (objectMap.TryGetValue(info.fromNodeId, out string fromJson))
            {
                fromSlotName = FindSlotNameById(fromJson, edge.m_OutputSlot.m_SlotId, objectMap);
            }
            if (objectMap.TryGetValue(info.toNodeId, out string toJson))
            {
                toSlotName = FindSlotNameById(toJson, edge.m_InputSlot.m_SlotId, objectMap);
            }

            if (string.IsNullOrEmpty(fromSlotName) || string.IsNullOrEmpty(toSlotName))
                return null;

            info.fromSlot = $"{fromNode.type}.Output.{fromSlotName}";
            info.toSlot = $"{toNode.type}.Input.{toSlotName}";
            return info;
        }

        private static string FindSlotNameById(string nodeJson, int slotId, Dictionary<string, string> objectMap)
        {
            var slotIds = new List<string>();
            var idPattern = "\"m_Id\"\\s*:\\s*\"([^\"]*)\"";
            var matches = System.Text.RegularExpressions.Regex.Matches(nodeJson, idPattern);
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                slotIds.Add(m.Groups[1].Value);
            }

            foreach (var id in slotIds)
            {
                if (objectMap.TryGetValue(id, out string slotJson))
                {
                    var sidStr = ExtractJsonValue(slotJson, "m_Id");
                    if (int.TryParse(sidStr, out int sid) && sid == slotId)
                    {
                        return ExtractJsonValue(slotJson, "m_DisplayName");
                    }
                }
            }
            return null;
        }

        private static string GetNodeShortType(string fullName)
        {
            if (NodeTypeShortNameMap.TryGetValue(fullName, out var shortName))
                return shortName;
            var lastDot = fullName.LastIndexOf('.');
            var name = lastDot >= 0 ? fullName.Substring(lastDot + 1) : fullName;
            return name;
        }

        // 辅助类
        private class MultiJsonObject
        {
            public string type;
            public string id;
            public string json;
        }

        [Serializable]
        private class GraphDataRoot
        {
            public IdRef[] m_Properties;
            public IdRef[] m_Nodes;
            public EdgeData[] m_Edges;
        }

        [Serializable]
        private class IdRef
        {
            public string m_Id;
        }

        [Serializable]
        private class EdgeData
        {
            public SlotRef m_OutputSlot;
            public SlotRef m_InputSlot;
        }

        [Serializable]
        private class SlotRef
        {
            public IdRef m_Node;
            public int m_SlotId;
        }

        private class PropertyInfo
        {
            public string referenceName;
            public string propertyType;
            public string defaultValue;
        }

        private class NodeInfo
        {
            public string id;
            public string type;
            public string propertyName;
            public Dictionary<string, string> inputs = new Dictionary<string, string>();
            public Dictionary<string, string> outputs = new Dictionary<string, string>();
            public Dictionary<string, string> customSettings = new Dictionary<string, string>();
        }

        private class EdgeInfo
        {
            public string fromNodeId;
            public string fromSlot;
            public string toNodeId;
            public string toSlot;
        }
    }
}