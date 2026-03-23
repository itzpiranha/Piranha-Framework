using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using DG.Tweening;

namespace Piranha.UI
{
    /// <summary>
    /// UI管理器核心，负责UI的创建、缓存、显示/隐藏、销毁
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        /// <summary>
        /// UIManager单例
        /// </summary>
        public static UIManager Instance { get; private set; }

        /// <summary>
        /// UI工厂
        /// </summary>
        private UIFactory factory;

        /// <summary>
        /// UI缓存池
        /// </summary>
        private UICache cache;

        /// <summary>
        /// UI栈
        /// </summary>
        private UIStack stack;

        /// <summary>
        /// Canvas根节点
        /// </summary>
        private Transform canvasRoot;

        /// <summary>
        /// Canvas组件
        /// </summary>
        private Canvas canvas;

        /// <summary>
        /// CanvasScaler组件
        /// </summary>
        private CanvasScaler canvasScaler;

        /// <summary>
        /// 各层级的根节点
        /// </summary>
        private Dictionary<UILayer, Transform> layerRoots = new Dictionary<UILayer, Transform>();

        /// <summary>
        /// 遮罩材质（用于点击关闭）
        /// </summary>
        [SerializeField]
        private Material maskMaterial;

        /// <summary>
        /// 遮罩颜色
        /// </summary>
        [SerializeField]
        private Color maskColor = new Color(0, 0, 0, 0.5f);

        /// <summary>
        /// 分辨率配置文件
        /// </summary>
        [SerializeField]
        private UIResolutionConfig resolutionConfig;

        /// <summary>
        /// 是否启用安全区适配
        /// </summary>
        [SerializeField]
        private bool enableSafeArea = true;

        /// <summary>
        /// 当前最高SortOrder
        /// </summary>
        private int currentSortOrder = 0;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool isInitialized = false;

        /// <summary>
        /// 当前设备类型
        /// </summary>
        public DeviceType CurrentDeviceType { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            factory = new UIFactory();
            cache = new UICache();
            stack = new UIStack();

            // 初始化设备信息
            UIDeviceInfo.Init();
            CurrentDeviceType = UIDeviceInfo.DeviceType;
        }

        private void Start()
        {
            if (!isInitialized)
            {
                InitCanvas();
                isInitialized = true;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                // 从后台恢复时刷新设备信息
                UIDeviceInfo.Refresh();
                CurrentDeviceType = UIDeviceInfo.DeviceType;
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            // 屏幕尺寸变化时刷新
            if (isInitialized)
            {
                UIDeviceInfo.Refresh();
                DeviceType newDeviceType = UIDeviceInfo.DeviceType;
                if (newDeviceType != CurrentDeviceType)
                {
                    CurrentDeviceType = newDeviceType;
                    ApplyResolutionConfig();
                }
            }
        }

        /// <summary>
        /// 应用分辨率配置
        /// </summary>
        private void ApplyResolutionConfig()
        {
            if (resolutionConfig == null || canvasScaler == null)
                return;

            Vector2 refRes = resolutionConfig.GetReferenceResolution(CurrentDeviceType);
            canvasScaler.referenceResolution = refRes;
            canvasScaler.screenMatchMode = resolutionConfig.GetMatchMode(CurrentDeviceType);
            canvasScaler.matchWidthOrHeight = resolutionConfig.GetMatchValue(CurrentDeviceType);

            Debug.Log($"[UIManager] Applied resolution config for {CurrentDeviceType}: {refRes}");
        }

        /// <summary>
        /// 初始化Canvas
        /// </summary>
        private void InitCanvas()
        {
            // 创建Canvas
            GameObject canvasObj = new GameObject("UIRoot");
            canvasObj.transform.SetParent(transform);
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            // 应用分辨率配置
            if (resolutionConfig != null)
            {
                ApplyResolutionConfig();
            }
            else
            {
                // 默认配置（竖屏手机）
                canvasScaler.referenceResolution = new Vector2(1080, 1920);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f;
            }

            // 添加GraphicRaycaster并配置，确保不会阻止子Canvas的射线检测
            GraphicRaycaster rootRaycaster = canvasObj.AddComponent<GraphicRaycaster>();
            rootRaycaster.ignoreReversedGraphics = true;
            rootRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

            canvasRoot = canvasObj.transform;

            // 创建各层级根节点
            CreateLayerRoot(UILayer.Background, 0);
            CreateLayerRoot(UILayer.Common, 100);
            CreateLayerRoot(UILayer.Dialog, 200);
            CreateLayerRoot(UILayer.Tips, 300);
            CreateLayerRoot(UILayer.Top, 400);

            // 应用安全区
            if (enableSafeArea)
            {
                ApplySafeAreaToCanvas();
            }

            // 打印设备信息
            UIDeviceInfo.LogDeviceInfo();
        }

        /// <summary>
        /// 应用安全区到Canvas
        /// </summary>
        private void ApplySafeAreaToCanvas()
        {
            if (canvas == null || canvasRoot == null)
                return;

            Rect safeArea = Screen.safeArea;
            if (safeArea.x <= 0 && safeArea.y <= 0 &&
                safeArea.xMax >= Screen.width && safeArea.yMax >= Screen.height)
            {
                return; // 没有安全区
            }

            // 获取Canvas的RectTransform
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect == null)
                return;

            // 计算安全区在Canvas中的偏移
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float normX = safeArea.x / screenWidth;
            float normY = safeArea.y / screenHeight;
            float normWidth = safeArea.width / screenWidth;
            float normHeight = safeArea.height / screenHeight;

            Vector2 canvasSize = canvasRect.rect.size;
            float canvasScaleX = canvasRect.localScale.x;
            float canvasScaleY = canvasRect.localScale.y;

            // 设置安全区偏移（作为额外的边距）
            float leftPadding = normX * canvasSize.x * canvasScaleX;
            float bottomPadding = normY * canvasSize.y * canvasScaleY;
            float rightPadding = (1 - normX - normWidth) * canvasSize.x * canvasScaleX;
            float topPadding = (1 - normY - normHeight) * canvasSize.y * canvasScaleY;

            // 为每个层级添加安全区偏移
            foreach (var layerRoot in layerRoots)
            {
                RectTransform layerRect = layerRoot.Value.GetComponent<RectTransform>();
                if (layerRect != null)
                {
                    // 底部层级需要偏移
                    if (layerRoot.Key == UILayer.Background)
                    {
                        layerRect.offsetMin = new Vector2(leftPadding, bottomPadding);
                        layerRect.offsetMax = new Vector2(-rightPadding, -topPadding);
                    }
                }
            }

            Debug.Log($"[UIManager] Applied safe area: {safeArea}");
        }

        /// <summary>
        /// 获取当前Canvas
        /// </summary>
        public Canvas GetCanvas()
        {
            return canvas;
        }

        /// <summary>
        /// 获取当前CanvasScaler
        /// </summary>
        public CanvasScaler GetCanvasScaler()
        {
            return canvasScaler;
        }

        /// <summary>
        /// 创建层级根节点
        /// </summary>
        private void CreateLayerRoot(UILayer layer, int sortingOrder)
        {
            GameObject layerObj = new GameObject($"Layer_{layer}");
            layerObj.transform.SetParent(canvasRoot);

            RectTransform rect = layerObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            layerObj.AddComponent<CanvasRenderer>();

            layerRoots[layer] = layerObj.transform;
        }

        /// <summary>
        /// 打开UI面板
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <param name="data">面板数据</param>
        /// <returns>面板实例</returns>
        public T Open<T>(PanelData data = null) where T : UIBase
        {
            return Open(typeof(T), data) as T;
        }

        /// <summary>
        /// 打开UI面板（带自定义动画配置）
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <param name="data">面板数据</param>
        /// <param name="animConfig">动画配置</param>
        /// <returns>面板实例</returns>
        public T Open<T>(PanelData data, UIAnimConfig animConfig) where T : UIBase
        {
            UIBase panel = Open(typeof(T), data);
            if (panel != null && animConfig != null)
            {
                panel.SetAnimConfig(animConfig);
                panel.SetUseAnim(true);
                panel.PlayOpenAnim();
            }
            return panel as T;
        }

        /// <summary>
        /// 打开UI面板
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <param name="data">面板数据</param>
        /// <returns>面板实例</returns>
        public UIBase Open(Type panelType, PanelData data = null)
        {
            UIBase panel = null;
            UIAttribute attr = GetUIAttribute(panelType);

            // 尝试从缓存获取
            if (cache.HasCached(panelType))
            {
                panel = cache.Get(panelType);
            }

            // 如果缓存中没有，创建新实例
            if (panel == null)
            {
                panel = factory.Create(panelType);
                if (panel == null) return null;
            }

            // 设置父节点
            Transform parent = GetLayerRoot(panel.Layer);
            panel.transform.SetParent(parent, false);

            // 设置SortOrder
            currentSortOrder++;
            panel.SortOrder = currentSortOrder;
            SetPanelSortOrder(panel.gameObject, currentSortOrder);

            // 添加遮罩（如果需要）
            if (attr != null && attr.ShowMask)
            {
                AddMask(panel, attr.MaskClose);
            }

            // 标记为使用中
            cache.MarkUsing(panelType, panel);

            // 压入栈
            stack.Push(panel);

            // 打开面板
            panel.InternalOpen(data);

            // 播放打开动画
            if (attr != null && attr.UseAnim)
            {
                UIAnimConfig config = new UIAnimConfig
                {
                    openAnim = attr.OpenAnim,
                    closeAnim = attr.CloseAnim,
                    duration = attr.AnimDuration
                };
                panel.SetAnimConfig(config);
                panel.PlayOpenAnim();
            }

            return panel;
        }

        /// <summary>
        /// 关闭UI面板
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        public void Close<T>() where T : UIBase
        {
            Close(typeof(T));
        }

        /// <summary>
        /// 关闭UI面板
        /// </summary>
        /// <param name="panelType">面板类型</param>
        public void Close(Type panelType)
        {
            UIBase panel = cache.GetActive(panelType);
            if (panel == null)
            {
                Debug.LogWarning($"[UIManager] Panel {panelType.Name} is not open!");
                return;
            }

            Close(panel);
        }

        /// <summary>
        /// 关闭UI面板
        /// </summary>
        /// <param name="panel">面板实例</param>
        public void Close(UIBase panel)
        {
            Type panelType = panel.GetType();
            UIAttribute attr = GetUIAttribute(panelType);
            bool needCache = attr == null || attr.NeedCache;

            // 从栈中弹出
            stack.Pop(panel);

            // 关闭面板
            panel.InternalClose();

            // 移除遮罩
            RemoveMask(panel);

            if (needCache)
            {
                // 放入缓存
                cache.Put(panelType, panel);
                panel.gameObject.SetActive(false);
            }
            else
            {
                // 销毁
                Destroy(panel.gameObject);
            }
        }

        /// <summary>
        /// 带动画关闭UI面板
        /// </summary>
        /// <param name="panel">面板实例</param>
        public void CloseWithAnim(UIBase panel)
        {
            Type panelType = panel.GetType();
            UIAttribute attr = GetUIAttribute(panelType);
            bool needCache = attr == null || attr.NeedCache;
            bool useAnim = attr == null || attr.UseAnim;

            // 从栈中弹出
            stack.Pop(panel);

            // 移除遮罩
            RemoveMask(panel);

            // 播放关闭动画
            if (useAnim)
            {
                UIAnimConfig config = new UIAnimConfig
                {
                    closeAnim = attr?.CloseAnim ?? UIAnimType.Scale,
                    duration = attr?.AnimDuration ?? 0.3f
                };

                panel.gameObject.PlayCloseAnim(config, () =>
                {
                    panel.InternalClose();
                    if (needCache)
                    {
                        cache.Put(panelType, panel);
                        panel.gameObject.SetActive(false);
                    }
                    else
                    {
                        Destroy(panel.gameObject);
                    }
                });
            }
            else
            {
                panel.InternalClose();
                if (needCache)
                {
                    cache.Put(panelType, panel);
                    panel.gameObject.SetActive(false);
                }
                else
                {
                    Destroy(panel.gameObject);
                }
            }
        }

        /// <summary>
        /// 关闭所有UI
        /// </summary>
        public void CloseAll()
        {
            List<UIBase> panels = stack.GetAllPanels();
            for (int i = panels.Count - 1; i >= 0; i--)
            {
                Close(panels[i]);
            }
        }

        /// <summary>
        /// 关闭最顶层的UI
        /// </summary>
        public void CloseTop()
        {
            UIBase topPanel = GetTopPanel();
            if (topPanel != null)
            {
                Close(topPanel);
            }
        }

        /// <summary>
        /// 获取已打开的UI面板
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <returns>面板实例，如果没有打开则返回null</returns>
        public T Get<T>() where T : UIBase
        {
            return cache.GetActive(typeof(T)) as T;
        }

        /// <summary>
        /// 获取已打开的UI面板
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>面板实例，如果没有打开则返回null</returns>
        public UIBase Get(Type panelType)
        {
            return cache.GetActive(panelType);
        }

        /// <summary>
        /// 获取最顶层的面板
        /// </summary>
        /// <returns>最顶层的面板</returns>
        public UIBase GetTopPanel()
        {
            UIBase top = null;
            int maxOrder = -1;

            foreach (var panel in stack.GetAllPanels())
            {
                if (panel.SortOrder > maxOrder && panel.IsActive)
                {
                    maxOrder = panel.SortOrder;
                    top = panel;
                }
            }

            return top;
        }

        /// <summary>
        /// 获取指定层级的根节点
        /// </summary>
        private Transform GetLayerRoot(UILayer layer)
        {
            if (layerRoots.TryGetValue(layer, out Transform root))
            {
                return root;
            }
            return canvasRoot;
        }

        /// <summary>
        /// 设置面板的SortOrder
        /// </summary>
        private void SetPanelSortOrder(GameObject obj, int order)
        {
            Canvas canvas = obj.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = obj.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
            canvas.sortingOrder = order;

            // 确保面板Canvas有GraphicRaycaster
            GraphicRaycaster raycaster = obj.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = obj.AddComponent<GraphicRaycaster>();
                // 配置raycaster确保点击能穿透到正确的面板
                raycaster.ignoreReversedGraphics = true;
                raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            }

            // 递归设置子Canvas
            Canvas[] childCanvases = obj.GetComponentsInChildren<Canvas>(true);
            for (int i = 1; i < childCanvases.Length; i++)
            {
                childCanvases[i].overrideSorting = true;
                childCanvases[i].sortingOrder = order;

                // 为子Canvas也添加GraphicRaycaster
                GraphicRaycaster childRaycaster = childCanvases[i].GetComponent<GraphicRaycaster>();
                if (childRaycaster == null)
                {
                    childRaycaster = childCanvases[i].gameObject.AddComponent<GraphicRaycaster>();
                    childRaycaster.ignoreReversedGraphics = true;
                    childRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
                }
            }
        }

        /// <summary>
        /// 添加遮罩
        /// </summary>
        private void AddMask(UIBase panel, bool clickClose)
        {
            Transform parent = panel.transform;
            RectTransform maskRect = null;
            Image maskImage = null;

            // 查找是否已有遮罩
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == "__Mask__")
                {
                    maskRect = child.GetComponent<RectTransform>();
                    maskImage = child.GetComponent<Image>();
                    maskRect.SetAsFirstSibling();
                    maskRect.gameObject.SetActive(true);
                    return;
                }
            }

            // 创建遮罩
            GameObject maskObj = new GameObject("__Mask__");
            maskRect = maskObj.AddComponent<RectTransform>();
            maskRect.SetParent(parent, false);
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;
            maskRect.SetAsFirstSibling();

            maskImage = maskObj.AddComponent<Image>();
            maskImage.color = maskColor;
            if (maskMaterial != null)
            {
                maskImage.material = maskMaterial;
            }

            if (clickClose)
            {
                Button btn = maskObj.AddComponent<Button>();
                btn.onClick.AddListener(() => Close(panel));
            }
        }

        /// <summary>
        /// 移除遮罩
        /// </summary>
        private void RemoveMask(UIBase panel)
        {
            Transform parent = panel.transform;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == "__Mask__")
                {
                    child.gameObject.SetActive(false);
                    return;
                }
            }
        }

        /// <summary>
        /// 获取UI特性
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
        /// 销毁所有缓存
        /// </summary>
        public void ClearCache()
        {
            cache.Clear();
            factory.ClearCache();
        }

        /// <summary>
        /// 获取安全区域信息
        /// </summary>
        public Rect GetSafeArea()
        {
            return Screen.safeArea;
        }

        /// <summary>
        /// 是否启用安全区
        /// </summary>
        public bool IsSafeAreaEnabled()
        {
            return enableSafeArea;
        }

        /// <summary>
        /// 设置分辨率配置
        /// </summary>
        public void SetResolutionConfig(UIResolutionConfig config)
        {
            resolutionConfig = config;
            if (isInitialized)
            {
                ApplyResolutionConfig();
            }
        }

        /// <summary>
        /// 手动触发安全区应用
        /// </summary>
        public void RefreshSafeArea()
        {
            if (isInitialized && enableSafeArea)
            {
                ApplySafeAreaToCanvas();
            }
        }

        /// <summary>
        /// 获取安全区域相对于UI的偏移量（左、下、右、上）
        /// </summary>
        public Vector4 GetSafeAreaOffset()
        {
            Rect safeArea = Screen.safeArea;
            if (canvas == null)
                return Vector4.zero;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect == null)
                return Vector4.zero;

            Vector2 canvasSize = canvasRect.rect.size;
            float canvasScaleX = canvasRect.localScale.x;
            float canvasScaleY = canvasRect.localScale.y;

            float left = (safeArea.x / Screen.width) * canvasSize.x * canvasScaleX;
            float bottom = (safeArea.y / Screen.height) * canvasSize.y * canvasScaleY;
            float right = ((Screen.width - safeArea.xMax) / Screen.width) * canvasSize.x * canvasScaleX;
            float top = ((Screen.height - safeArea.yMax) / Screen.height) * canvasSize.y * canvasScaleY;

            return new Vector4(left, bottom, right, top);
        }
    }
}
