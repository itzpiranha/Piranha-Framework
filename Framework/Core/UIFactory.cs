using UnityEngine;
using System;
using System.Collections.Generic;

namespace Piranha.UI
{
    /// <summary>
    /// UI工厂，负责从Resources加载UI预制体
    /// </summary>
    public class UIFactory
    {
        private const string PREFAB_BASE_PATH = "Prefabs/UI/";

        /// <summary>
        /// 已加载的预制体缓存
        /// </summary>
        private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

        /// <summary>
        /// 根据面板类型获取预制体
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <returns>预制体GameObject</returns>
        public GameObject LoadPrefab<T>() where T : UIBase
        {
            return LoadPrefab(typeof(T));
        }

        /// <summary>
        /// 根据面板类型获取预制体
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>预制体GameObject</returns>
        public GameObject LoadPrefab(Type panelType)
        {
            var attr = GetUIAttribute(panelType);
            if (attr == null)
            {
                Debug.LogError($"[UIFactory] {panelType.Name} has no UIAttribute!");
                return null;
            }

            string prefabPath = PREFAB_BASE_PATH + attr.PrefabPath;

            if (!prefabCache.TryGetValue(prefabPath, out GameObject prefab))
            {
                prefab = Resources.Load<GameObject>(prefabPath);
                if (prefab == null)
                {
                    Debug.LogError($"[UIFactory] Prefab not found: {prefabPath}");
                    return null;
                }
                prefabCache[prefabPath] = prefab;
            }

            return prefab;
        }

        /// <summary>
        /// 创建UI实例
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <returns>面板实例</returns>
        public T Create<T>() where T : UIBase
        {
            return Create(typeof(T)) as T;
        }

        /// <summary>
        /// 创建UI实例
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>面板实例</returns>
        public UIBase Create(Type panelType)
        {
            GameObject prefab = LoadPrefab(panelType);
            if (prefab == null) return null;

            GameObject instance = GameObject.Instantiate(prefab);
            UIBase panel = instance.GetComponent<UIBase>();
            if (panel == null)
            {
                panel = instance.AddComponent(panelType) as UIBase;
            }

            var attr = GetUIAttribute(panelType);
            if (attr != null)
            {
                panel.Layer = attr.Layer;
            }

            panel.InternalInit();
            return panel;
        }

        /// <summary>
        /// 获取面板类型的UI特性
        /// </summary>
        private UIAttribute GetUIAttribute(Type panelType)
        {
            var attrs = panelType.GetCustomAttributes(typeof(UIAttribute), false);
            if (attrs.Length > 0)
            {
                return attrs[0] as UIAttribute;
            }
            return null;
        }

        /// <summary>
        /// 清除预制体缓存
        /// </summary>
        public void ClearCache()
        {
            prefabCache.Clear();
        }
    }
}
