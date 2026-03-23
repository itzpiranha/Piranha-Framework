using UnityEngine;
using UnityEngine.UI;

namespace Piranha.UI
{
    /// <summary>
    /// 分辨率适配配置
    /// </summary>
    [CreateAssetMenu(fileName = "UIResolutionConfig", menuName = "Piranha/UI Resolution Config")]
    public class UIResolutionConfig : ScriptableObject
    {
        /// <summary>
        /// 手机参考分辨率
        /// </summary>
        [Header("Phone Settings")]
        [SerializeField]
        private Vector2 phoneReferenceResolution = new Vector2(1080, 1920);

        /// <summary>
        /// 手机匹配模式
        /// </summary>
        [SerializeField]
        private CanvasScaler.ScreenMatchMode phoneMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        /// <summary>
        /// 手机匹配值（0=宽, 1=高）
        /// </summary>
        [Range(0, 1)]
        [SerializeField]
        private float phoneMatchWidthOrHeight = 0.5f;

        /// <summary>
        /// 平板参考分辨率
        /// </summary>
        [Header("Tablet Settings")]
        [SerializeField]
        private Vector2 tabletReferenceResolution = new Vector2(1536, 2048);

        /// <summary>
        /// 平板匹配模式
        /// </summary>
        [SerializeField]
        private CanvasScaler.ScreenMatchMode tabletMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        /// <summary>
        /// 平板匹配值
        /// </summary>
        [Range(0, 1)]
        [SerializeField]
        private float tabletMatchWidthOrHeight = 0.5f;

        /// <summary>
        /// 平板宽高比阈值（小于此值认为是平板）
        /// </summary>
        [Header("Device Detection")]
        [SerializeField]
        private float tabletAspectThreshold = 1.5f;

        /// <summary>
        /// 是否自动检测设备类型
        /// </summary>
        [SerializeField]
        private bool autoDetectDevice = true;

        /// <summary>
        /// 强制使用的设备类型（autoDetectDevice为false时生效）
        /// </summary>
        [SerializeField]
        private DeviceType forceDeviceType = DeviceType.Phone;

        // Properties
        public Vector2 PhoneReferenceResolution => phoneReferenceResolution;
        public CanvasScaler.ScreenMatchMode PhoneMatchMode => phoneMatchMode;
        public float PhoneMatchWidthOrHeight => phoneMatchWidthOrHeight;

        public Vector2 TabletReferenceResolution => tabletReferenceResolution;
        public CanvasScaler.ScreenMatchMode TabletMatchMode => tabletMatchMode;
        public float TabletMatchWidthOrHeight => tabletMatchWidthOrHeight;

        public float TabletAspectThreshold => tabletAspectThreshold;
        public bool AutoDetectDevice => autoDetectDevice;
        public DeviceType ForceDeviceType => forceDeviceType;

        /// <summary>
        /// 根据设备类型获取参考分辨率
        /// </summary>
        public Vector2 GetReferenceResolution(DeviceType deviceType)
        {
            return deviceType == DeviceType.Tablet ? tabletReferenceResolution : phoneReferenceResolution;
        }

        /// <summary>
        /// 根据设备类型获取匹配模式
        /// </summary>
        public CanvasScaler.ScreenMatchMode GetMatchMode(DeviceType deviceType)
        {
            return deviceType == DeviceType.Tablet ? tabletMatchMode : phoneMatchMode;
        }

        /// <summary>
        /// 根据设备类型获取匹配值
        /// </summary>
        public float GetMatchValue(DeviceType deviceType)
        {
            return deviceType == DeviceType.Tablet ? tabletMatchWidthOrHeight : phoneMatchWidthOrHeight;
        }

        /// <summary>
        /// 检测设备类型
        /// </summary>
        public DeviceType DetectDeviceType()
        {
            if (!autoDetectDevice)
                return forceDeviceType;

            float aspectRatio = (float)Screen.width / Screen.height;
            return aspectRatio < tabletAspectThreshold ? DeviceType.Tablet : DeviceType.Phone;
        }
    }
}
