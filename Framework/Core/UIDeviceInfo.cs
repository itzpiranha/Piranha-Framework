using UnityEngine;

namespace Piranha.UI
{
    /// <summary>
    /// 设备类型枚举
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// 手机
        /// </summary>
        Phone,

        /// <summary>
        /// 平板
        /// </summary>
        Tablet
    }

    /// <summary>
    /// 屏幕方向
    /// </summary>
    public enum ScreenOrientation
    {
        /// <summary>
        /// 竖屏
        /// </summary>
        Portrait,

        /// <summary>
        /// 横屏
        /// </summary>
        Landscape
    }

    /// <summary>
    /// 设备信息，用于屏幕适配
    /// </summary>
    public static class UIDeviceInfo
    {
        /// <summary>
        /// 当前设备类型
        /// </summary>
        public static DeviceType DeviceType { get; private set; }

        /// <summary>
        /// 当前屏幕方向
        /// </summary>
        public static ScreenOrientation Orientation { get; private set; }

        /// <summary>
        /// 屏幕宽度
        /// </summary>
        public static float ScreenWidth => Screen.width;

        /// <summary>
        /// 屏幕高度
        /// </summary>
        public static float ScreenHeight => Screen.height;

        /// <summary>
        /// 屏幕宽高比
        /// </summary>
        public static float AspectRatio => (float)Screen.width / Screen.height;

        /// <summary>
        /// 安全区域（基于屏幕坐标）
        /// </summary>
        public static Rect SafeArea => Screen.safeArea;

        /// <summary>
        /// 是否是全面屏（有刘海或挖孔）
        /// </summary>
        public static bool IsNotched => SafeArea.x > 0 || SafeArea.y > 0 ||
                                       SafeArea.xMax < Screen.width ||
                                       SafeArea.yMax < Screen.height;

        /// <summary>
        /// 平板宽高比阈值（小于此值为平板）
        /// </summary>
        private const float TABLET_ASPECT_THRESHOLD = 1.5f;

        /// <summary>
        /// 初始化设备信息
        /// </summary>
        public static void Init()
        {
            Refresh();
        }

        /// <summary>
        /// 刷新设备信息（应在屏幕旋转时调用）
        /// </summary>
        public static void Refresh()
        {
            // 检测设备类型
            DeviceType = GetDeviceType();

            // 检测屏幕方向
            Orientation = GetScreenOrientation();

            // 设置屏幕方向
            SetScreenOrientation();
        }

        /// <summary>
        /// 获取设备类型
        /// </summary>
        private static DeviceType GetDeviceType()
        {
            // 根据宽高比判断：小于1.5认为是平板
            // 也可以根据实际屏幕英寸数判断（需要额外API）
            return AspectRatio < TABLET_ASPECT_THRESHOLD ? DeviceType.Phone : DeviceType.Tablet;
        }

        /// <summary>
        /// 获取屏幕方向
        /// </summary>
        private static ScreenOrientation GetScreenOrientation()
        {
            return Screen.width > Screen.height ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
        }

        /// <summary>
        /// 设置屏幕方向
        /// </summary>
        private static void SetScreenOrientation()
        {
#if UNITY_ANDROID || UNITY_IOS
            // 移动端自动旋转
            Screen.autorotateToPortrait = true;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortraitUpsideDown = false;
#endif
        }

        /// <summary>
        /// 获取安全区域相对于Canvas的偏移
        /// </summary>
        /// <param name="canvas">Canvas组件</param>
        /// <returns>安全区域偏移</returns>
        public static Vector4 GetSafeAreaOffset(Canvas canvas)
        {
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
                return Vector4.zero;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect == null)
                return Vector4.zero;

            // 将屏幕安全区域转换为Canvas局部坐标
            Rect screenSafeArea = SafeArea;

            // 获取Canvas的屏幕坐标
            Vector3[] canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);

            // 计算Canvas的实际屏幕尺寸
            float canvasWidth = Vector3.Distance(canvasCorners[0], canvasCorners[3]);
            float canvasHeight = Vector3.Distance(canvasCorners[0], canvasCorners[1]);

            if (canvasWidth <= 0 || canvasHeight <= 0)
                return Vector4.zero;

            // 计算归一化的安全区域（0-1）
            float normalizedX = screenSafeArea.x / Screen.width;
            float normalizedY = screenSafeArea.y / Screen.height;
            float normalizedWidth = screenSafeArea.width / Screen.width;
            float normalizedHeight = screenSafeArea.height / Screen.height;

            // 计算Canvas尺寸
            float canvasScaleX = canvasRect.rect.width;
            float canvasScaleY = canvasRect.rect.height;

            // 计算偏移量
            float left = normalizedX * canvasScaleX;
            float bottom = normalizedY * canvasScaleY;
            float right = (1 - normalizedX - normalizedWidth) * canvasScaleX;
            float top = (1 - normalizedY - normalizedHeight) * canvasScaleY;

            return new Vector4(left, bottom, right, top);
        }

        /// <summary>
        /// 日志输出当前设备信息
        /// </summary>
        public static void LogDeviceInfo()
        {
            Debug.Log($"[UIDeviceInfo] Device: {DeviceType}, Orientation: {Orientation}, " +
                      $"Resolution: {ScreenWidth}x{ScreenHeight}, Aspect: {AspectRatio:F2}, " +
                      $"SafeArea: {SafeArea}, IsNotched: {IsNotched}");
        }
    }
}
