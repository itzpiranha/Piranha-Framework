using UnityEngine;
using System;
using System.Collections.Generic;

namespace Piranha.UI
{
    /// <summary>
    /// UI缓存池，重复利用UI对象减少GC
    /// </summary>
    public class UICache
    {
        /// <summary>
        /// 缓存的UI面板集合
        /// </summary>
        private Dictionary<Type, Queue<UIBase>> cachePool = new Dictionary<Type, Queue<UIBase>>();

        /// <summary>
        /// 正在使用的UI面板集合
        /// </summary>
        private Dictionary<Type, UIBase> activePanels = new Dictionary<Type, UIBase>();

        /// <summary>
        /// 尝试从缓存获取面板
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <returns>面板实例，如果没有缓存则返回null</returns>
        public T Get<T>() where T : UIBase
        {
            return Get(typeof(T)) as T;
        }

        /// <summary>
        /// 尝试从缓存获取面板
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>面板实例，如果没有缓存则返回null</returns>
        public UIBase Get(Type panelType)
        {
            if (cachePool.TryGetValue(panelType, out Queue<UIBase> queue))
            {
                while (queue.Count > 0)
                {
                    UIBase panel = queue.Dequeue();
                    if (panel != null)
                    {
                        activePanels[panelType] = panel;
                        return panel;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 将面板放入缓存池
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <param name="panel">面板实例</param>
        public void Put<T>(T panel) where T : UIBase
        {
            Put(typeof(T), panel);
        }

        /// <summary>
        /// 将面板放入缓存池
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <param name="panel">面板实例</param>
        public void Put(Type panelType, UIBase panel)
        {
            if (!cachePool.ContainsKey(panelType))
            {
                cachePool[panelType] = new Queue<UIBase>();
            }
            cachePool[panelType].Enqueue(panel);
            activePanels.Remove(panelType);
        }

        /// <summary>
        /// 标记面板为使用中
        /// </summary>
        public void MarkUsing(Type panelType, UIBase panel)
        {
            activePanels[panelType] = panel;
        }

        /// <summary>
        /// 检查面板是否正在使用中
        /// </summary>
        public bool IsUsing(Type panelType)
        {
            return activePanels.ContainsKey(panelType);
        }

        /// <summary>
        /// 获取正在使用的面板
        /// </summary>
        public UIBase GetActive(Type panelType)
        {
            if (activePanels.TryGetValue(panelType, out UIBase panel))
            {
                return panel;
            }
            return null;
        }

        /// <summary>
        /// 检查指定类型的面板是否已缓存
        /// </summary>
        public bool HasCached(Type panelType)
        {
            return cachePool.TryGetValue(panelType, out Queue<UIBase> queue) && queue.Count > 0;
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void Clear()
        {
            cachePool.Clear();
            activePanels.Clear();
        }

        /// <summary>
        /// 获取缓存数量
        /// </summary>
        public int GetCacheCount(Type panelType)
        {
            if (cachePool.TryGetValue(panelType, out Queue<UIBase> queue))
            {
                return queue.Count;
            }
            return 0;
        }
    }
}
