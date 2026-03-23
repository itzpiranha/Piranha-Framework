using UnityEngine;
using System.Collections.Generic;

namespace Piranha.UI
{
    /// <summary>
    /// UI栈式管理，用于处理UI的层级关系（如弹窗栈）
    /// </summary>
    public class UIStack
    {
        /// <summary>
        /// UI栈集合，按层级分类
        /// </summary>
        private Dictionary<UILayer, Stack<UIBase>> layerStacks = new Dictionary<UILayer, Stack<UIBase>>();

        /// <summary>
        /// 所有打开的UI面板
        /// </summary>
        private List<UIBase> allPanels = new List<UIBase>();

        /// <summary>
        /// 将面板压入栈
        /// </summary>
        /// <param name="panel">面板实例</param>
        public void Push(UIBase panel)
        {
            if (!layerStacks.ContainsKey(panel.Layer))
            {
                layerStacks[panel.Layer] = new Stack<UIBase>();
            }

            // 如果栈顶有面板，触发暂停
            var stack = layerStacks[panel.Layer];
            if (stack.Count > 0)
            {
                stack.Peek().InternalPause();
            }

            stack.Push(panel);

            if (!allPanels.Contains(panel))
            {
                allPanels.Add(panel);
            }
        }

        /// <summary>
        /// 从栈中弹出面板
        /// </summary>
        /// <param name="panel">要弹出的面板</param>
        /// <returns>被弹出面板的下层面板</returns>
        public UIBase Pop(UIBase panel)
        {
            if (!layerStacks.ContainsKey(panel.Layer)) return null;

            var stack = layerStacks[panel.Layer];
            if (stack.Count == 0) return null;

            UIBase top = stack.Pop();
            if (top != panel)
            {
                // 如果栈顶不是要弹出的面板，尝试查找
                Debug.LogWarning($"[UIStack] Pop panel mismatch! Expected {panel.GetType().Name}");
                return null;
            }

            allPanels.Remove(panel);

            // 恢复栈顶面板
            if (stack.Count > 0)
            {
                stack.Peek().InternalResume();
                return stack.Peek();
            }

            return null;
        }

        /// <summary>
        /// 获取指定层级的栈顶面板
        /// </summary>
        /// <param name="layer">层级</param>
        /// <returns>栈顶面板</returns>
        public UIBase GetTop(UILayer layer)
        {
            if (layerStacks.TryGetValue(layer, out Stack<UIBase> stack) && stack.Count > 0)
            {
                return stack.Peek();
            }
            return null;
        }

        /// <summary>
        /// 获取所有面板
        /// </summary>
        /// <returns>所有打开的面板列表</returns>
        public List<UIBase> GetAllPanels()
        {
            return new List<UIBase>(allPanels);
        }

        /// <summary>
        /// 获取指定层级的所有面板
        /// </summary>
        /// <param name="layer">层级</param>
        /// <returns>面板列表</returns>
        public List<UIBase> GetPanels(UILayer layer)
        {
            List<UIBase> result = new List<UIBase>();
            if (layerStacks.TryGetValue(layer, out Stack<UIBase> stack))
            {
                result.AddRange(stack);
            }
            return result;
        }

        /// <summary>
        /// 关闭指定层级的所有面板
        /// </summary>
        /// <param name="layer">层级</param>
        public void CloseAll(UILayer layer)
        {
            if (!layerStacks.TryGetValue(layer, out Stack<UIBase> stack)) return;

            while (stack.Count > 0)
            {
                UIBase panel = stack.Pop();
                if (allPanels.Contains(panel))
                {
                    allPanels.Remove(panel);
                }
            }
        }

        /// <summary>
        /// 关闭所有UI
        /// </summary>
        public void CloseAll()
        {
            foreach (var stack in layerStacks.Values)
            {
                stack.Clear();
            }
            allPanels.Clear();
        }

        /// <summary>
        /// 获取指定面板的层级
        /// </summary>
        /// <param name="panel">面板</param>
        /// <returns>层级</returns>
        public UILayer GetLayer(UIBase panel)
        {
            return panel.Layer;
        }

        /// <summary>
        /// 获取栈中面板数量
        /// </summary>
        /// <param name="layer">层级</param>
        /// <returns>数量</returns>
        public int GetCount(UILayer layer)
        {
            if (layerStacks.TryGetValue(layer, out Stack<UIBase> stack))
            {
                return stack.Count;
            }
            return 0;
        }
    }
}
