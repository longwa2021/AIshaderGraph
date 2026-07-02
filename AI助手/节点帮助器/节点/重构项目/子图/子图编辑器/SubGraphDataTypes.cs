using System;
using System.Collections.Generic;
using UnityEngine;

namespace 龙哥的秘密花园.Editor
{
    [Serializable]
    public class SubGraphStructure
    {
        public string name;
        public string description;
        public List<SerializableProperty> properties = new List<SerializableProperty>();
        public List<SerializableNode> nodes = new List<SerializableNode>();
        public List<SerializableEdge> edges = new List<SerializableEdge>();
    }

    [Serializable]
    public class SerializableProperty
    {
        public string referenceName;
        public string displayName;
        public string type;          // "Float", "Color", "Texture2D", etc.
        public object defaultValue;
    }

    [Serializable]
    public class SerializableNode
    {
        public string id;
        public string type;          // 完整类型名
        public string nodeType;      // 短类型名，如 "UVNode"
        public Vector2 position;
        public Dictionary<string, object> inputs = new Dictionary<string, object>();
        public string propertyReference;
    }

    [Serializable]
    public class SerializableEdge
    {
        public string fromNode;
        public string fromSlot;
        public string toNode;
        public string toSlot;
    }
}