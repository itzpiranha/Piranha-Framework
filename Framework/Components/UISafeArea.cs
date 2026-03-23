using UnityEngine;
using UnityEngine.UI;

namespace Piranha.UI
{
    /// <summary>
    /// 安全区域适配模式
    /// </summary>
    public enum SafeAreaMode
    {
        /// <summary>
        /// 顶部安全区
        /// </summary>
        Top,

        /// <summary>
        /// 底部安全区
        /// </summary>
        Bottom,

        /// <summary>
        /// 左侧安全区
        /// </summary>
        Left,

        /// <summary>
        /// 右侧安全区
        /// </summary>
        Right,

        /// <summary>
        /// 四周安全区
        /// </summary>
        All,

        /// <summary>
        /// 水平安全区（左右）
        /// </summary>
        Horizontal,

        /// <summary>
        /// 垂直安全区（上下）
        /// </summary>
        Vertical
    }

    /// <summary>
    /// UI安全区域组件，自动处理刘海屏/挖孔屏的安全区
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [ExecuteInEditMode]
    public class UISafeArea : MonoBehaviour
    {
        /// <summary>
        /// 安全区域模式
        /// </summary>
        [SerializeField]
        private SafeAreaMode mode = SafeAreaMode.All;

        /// <summary>
        /// 是否在运行时自动更新
        /// </summary>
        [SerializeField]
        private bool autoUpdate = true;

        /// <summary>
        /// 额外的边距偏移（用于微调）
        /// </summary>
        [SerializeField]
        private Vector4 extraPadding = Vector4.zero;

        /// <summary>
        /// 是否已应用过安全区
        /// </summary>
        private bool isApplied = false;

        /// <summary>
        /// 原始的锚点和偏移
        /// </summary>
        private Vector2 originalAnchorMin;
        private Vector2 originalAnchorMax;
        private Vector4 originalOffsets;

        private RectTransform rectTransform;
        private Canvas canvas;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            ApplySafeArea();
        }

        private void OnEnable()
        {
            if (autoUpdate && Application.isPlaying)
            {
                ApplySafeArea();
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying && !isApplied)
            {
                ApplySafeArea();
            }
        }
#endif

        /// <summary>
        /// 应用安全区域
        /// </summary>
        public void ApplySafeArea()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            // 查找父级Canvas
            canvas = GetComponentInParent<Canvas>();

            // 如果没有Canvas（Editor模式下可能没有），尝试获取屏幕信息
            if (canvas == null)
            {
                ApplySafeAreaForScreen();
                return;
            }

            Rect screenSafeArea = Screen.safeArea;

            // 如果没有安全区，直接返回
            if (screenSafeArea.x <= 0 && screenSafeArea.y <= 0 &&
                screenSafeArea.xMax >= Screen.width &&
                screenSafeArea.yMax >= Screen.height)
            {
                return;
            }

            // 获取Canvas的RectTransform
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect == null)
                return;

            // 计算Canvas尺寸
            Vector2 canvasSize = canvasRect.rect.size;
            if (canvasSize.x <= 0 || canvasSize.y <= 0)
                return;

            // 计算归一化的安全区域（0-1）
            float normX = screenSafeArea.x / Screen.width;
            float normY = screenSafeArea.y / Screen.height;
            float normWidth = screenSafeArea.width / Screen.width;
            float normHeight = screenSafeArea.height / Screen.height;

            // 计算边距（基于Canvas尺寸）
            float left = normX * canvasSize.x + extraPadding.x;
            float bottom = normY * canvasSize.y + extraPadding.y;
            float right = (1 - normX - normWidth) * canvasSize.x + extraPadding.z;
            float top = (1 - normY - normHeight) * canvasSize.y + extraPadding.w;

            // 根据模式应用安全区
            switch (mode)
            {
                case SafeAreaMode.Top:
                    ApplyTopPadding(canvasSize, top);
                    break;
                case SafeAreaMode.Bottom:
                    ApplyBottomPadding(canvasSize, bottom);
                    break;
                case SafeAreaMode.Left:
                    ApplyLeftPadding(canvasSize, left);
                    break;
                case SafeAreaMode.Right:
                    ApplyRightPadding(canvasSize, right);
                    break;
                case SafeAreaMode.All:
                    ApplyAllPadding(canvasSize, left, bottom, right, top);
                    break;
                case SafeAreaMode.Horizontal:
                    ApplyHorizontalPadding(canvasSize, left, right);
                    break;
                case SafeAreaMode.Vertical:
                    ApplyVerticalPadding(canvasSize, bottom, top);
                    break;
            }

            isApplied = true;
        }

        /// <summary>
        /// 基于屏幕应用安全区（无Canvas时）
        /// </summary>
        private void ApplySafeAreaForScreen()
        {
            Rect screenSafeArea = Screen.safeArea;

            if (screenSafeArea.x <= 0 && screenSafeArea.y <= 0 &&
                screenSafeArea.xMax >= Screen.width &&
                screenSafeArea.yMax >= Screen.height)
            {
                return;
            }

            // 使用屏幕尺寸计算
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float normX = screenSafeArea.x / screenWidth;
            float normY = screenSafeArea.y / screenHeight;
            float normWidth = screenSafeArea.width / screenWidth;
            float normHeight = screenSafeArea.height / screenHeight;

            float left = normX * screenWidth + extraPadding.x;
            float bottom = normY * screenHeight + extraPadding.y;
            float right = (1 - normX - normWidth) * screenWidth + extraPadding.z;
            float top = (1 - normY - normHeight) * screenHeight + extraPadding.w;

            switch (mode)
            {
                case SafeAreaMode.Top:
                    ApplyTopPadding(new Vector2(screenWidth, screenHeight), top);
                    break;
                case SafeAreaMode.Bottom:
                    ApplyBottomPadding(new Vector2(screenWidth, screenHeight), bottom);
                    break;
                case SafeAreaMode.Left:
                    ApplyLeftPadding(new Vector2(screenWidth, screenHeight), left);
                    break;
                case SafeAreaMode.Right:
                    ApplyRightPadding(new Vector2(screenWidth, screenHeight), right);
                    break;
                case SafeAreaMode.All:
                    ApplyAllPadding(new Vector2(screenWidth, screenHeight), left, bottom, right, top);
                    break;
                case SafeAreaMode.Horizontal:
                    ApplyHorizontalPadding(new Vector2(screenWidth, screenHeight), left, right);
                    break;
                case SafeAreaMode.Vertical:
                    ApplyVerticalPadding(new Vector2(screenWidth, screenHeight), bottom, top);
                    break;
            }
        }

        private void ApplyTopPadding(Vector2 canvasSize, float top)
        {
            // 设置底部锚点，顶部偏移
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, -top);
        }

        private void ApplyBottomPadding(Vector2 canvasSize, float bottom)
        {
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -bottom);
        }

        private void ApplyLeftPadding(Vector2 canvasSize, float left)
        {
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.offsetMin = new Vector2(-left, rectTransform.offsetMin.y);
        }

        private void ApplyRightPadding(Vector2 canvasSize, float right)
        {
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 0.5f);
            rectTransform.offsetMax = new Vector2(-right, rectTransform.offsetMax.y);
        }

        private void ApplyAllPadding(Vector2 canvasSize, float left, float bottom, float right, float top)
        {
            // 铺满整个区域，内部偏移
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(-left - right, -bottom - top);
        }

        private void ApplyHorizontalPadding(Vector2 canvasSize, float left, float right)
        {
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = new Vector2(-left, rectTransform.offsetMin.y);
            rectTransform.offsetMax = new Vector2(right, rectTransform.offsetMax.y);
        }

        private void ApplyVerticalPadding(Vector2 canvasSize, float bottom, float top)
        {
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, -bottom);
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, top);
        }

        /// <summary>
        /// 设置额外边距
        /// </summary>
        public void SetExtraPadding(Vector4 padding)
        {
            extraPadding = padding;
            ApplySafeArea();
        }

        /// <summary>
        /// 设置安全区域模式
        /// </summary>
        public void SetMode(SafeAreaMode newMode)
        {
            mode = newMode;
            ApplySafeArea();
        }

        /// <summary>
        /// 重置为原始布局
        /// </summary>
        public void Reset()
        {
            isApplied = false;
            ApplySafeArea();
        }
    }
}
