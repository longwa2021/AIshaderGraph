using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace 龙哥的秘密花园.节点库
{
    /// <summary>
    /// 节点自动布局算法：根据节点间的连接关系计算每个节点的屏幕坐标。
    /// 使用最长路径法确定层级，使数据流从左向右流动，同一层级节点纵向均匀分布。
    /// </summary>
    public static class NodeLayoutHelper
    {
        // 布局间距常量（硬编码，直接修改此处数值即可调整布局）
        private const int X_STEP = 400;          // 层级之间的水平间距
        private const int Y_STEP = 150;           // 同层节点之间的垂直间距
        private const int RIGHT_PADDING = 400;   // 右侧留白距离（最右侧节点最终 X = -RIGHT_PADDING）
        private const int Y_START = 0;           // 起始Y坐标

        /// <summary>
        /// 为给定节点和连接计算布局坐标。
        /// </summary>
        /// <param name="nodes">节点列表，每个节点应包含 Id 和 Type 属性</param>
        /// <param name="connections">连接列表，每条连接包含 From 和 To 节点ID</param>
        /// <returns>节点ID到坐标的映射字典</returns>
        public static Dictionary<string, Vector2> CalculateLayout(
            IEnumerable<dynamic> nodes,
            IEnumerable<dynamic> connections)
        {
            // 构建节点ID集合
            var nodeIds = nodes.Select(n => (string)n.Id).ToHashSet();

            // 1. 构建邻接表和入度表
            var graph = new Dictionary<string, List<string>>();
            var inDegree = new Dictionary<string, int>();
            foreach (var id in nodeIds)
            {
                graph[id] = new List<string>();
                inDegree[id] = 0;
            }

            foreach (var conn in connections)
            {
                string from = conn.From;
                string to = conn.To;
                if (graph.ContainsKey(from) && graph.ContainsKey(to))
                {
                    graph[from].Add(to);
                    inDegree[to]++;
                }
            }

            // 2. 拓扑排序计算最长路径层级
            var levels = new Dictionary<string, int>();
            var queue = new Queue<string>();

            foreach (var kv in inDegree)
            {
                if (kv.Value == 0)
                {
                    levels[kv.Key] = 0;
                    queue.Enqueue(kv.Key);
                }
            }

            var tempInDegree = new Dictionary<string, int>(inDegree);
            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                int currentLevel = levels[current];
                foreach (string neighbor in graph[current])
                {
                    int newLevel = currentLevel + 1;
                    if (!levels.ContainsKey(neighbor) || levels[neighbor] < newLevel)
                    {
                        levels[neighbor] = newLevel;
                    }
                    tempInDegree[neighbor]--;
                    if (tempInDegree[neighbor] == 0)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (levels.Count != nodeIds.Count)
            {
                var missing = nodeIds.Except(levels.Keys);
                Debug.LogError($"节点布局失败：图中存在环或孤立节点，未分配层级的节点: {string.Join(", ", missing)}");
                foreach (var id in missing)
                {
                    levels[id] = 0;
                }
            }

            // 3. 特殊节点强制层级（PropertyNode 置为最左侧）
            foreach (var node in nodes)
            {
                string type = node.Type;
                string id = node.Id;
                if (type == "PropertyNode" && levels.ContainsKey(id))
                {
                    levels[id] = 0;
                }
            }

            // 查找 MasterNode（或输出上下文节点）
            string masterId = null;
            foreach (var node in nodes)
            {
                if (node.Type == "MasterNode")
                {
                    masterId = node.Id;
                    break;
                }
            }

            int maxLevel = levels.Values.Any() ? levels.Values.Max() : 0;
            if (masterId != null)
            {
                levels[masterId] = maxLevel + 1;
                maxLevel++; // 更新最大层级
            }

            // 4. 按层级分组
            var levelGroups = new Dictionary<int, List<string>>();
            foreach (var kv in levels)
            {
                int lvl = kv.Value;
                if (!levelGroups.ContainsKey(lvl))
                    levelGroups[lvl] = new List<string>();
                levelGroups[lvl].Add(kv.Key);
            }

            // 5. 计算原始坐标（使用类常量）
            var coords = new Dictionary<string, Vector2>();
            int maxY = 0;

            foreach (int lvl in levelGroups.Keys.OrderBy(l => l))
            {
                float x = lvl * X_STEP;
                var sortedIds = levelGroups[lvl].OrderBy(id => id).ToList();
                float y = Y_START;
                foreach (string id in sortedIds)
                {
                    coords[id] = new Vector2(x, y);
                    if (y > maxY) maxY = (int)y;
                    y += Y_STEP;
                }
            }

            // 6. MasterNode 垂直居中
            if (masterId != null && coords.ContainsKey(masterId))
            {
                float centerY = maxY / 2f;
                coords[masterId] = new Vector2(coords[masterId].x, centerY);
            }

            // 7. 整体平移：使最右侧节点位于 X = -RIGHT_PADDING
            if (coords.Count > 0)
            {
                float maxX = coords.Values.Max(v => v.x);
                float shift = maxX + RIGHT_PADDING;
                var keys = coords.Keys.ToList();
                foreach (var id in keys)
                {
                    var pos = coords[id];
                    coords[id] = new Vector2(pos.x - shift, pos.y);
                }
            }

            return coords;
        }
    }
}