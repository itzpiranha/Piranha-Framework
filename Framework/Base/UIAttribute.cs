using System;

namespace Piranha.UI
{
    /// <summary>
    /// UI特性，用于关联UI面板类与预制体
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIAttribute : Attribute
    {
        /// <summary>
        /// 预制体路径（相对于Resources/Prefabs/UI/）
        /// </summary>
        public string PrefabPath { get; }

        /// <summary>
        /// UI层级
        /// </summary>
        public UILayer Layer { get; set; } = UILayer.Common;

        /// <summary>
        /// 是否显示遮罩
        /// </summary>
        public bool ShowMask { get; set; } = false;

        /// <summary>
        /// 遮罩点击是否关闭面板
        /// </summary>
        public bool MaskClose { get; set; } = false;

        /// <summary>
        /// 是否缓存，默认真（关闭后缓存）
        /// </summary>
        public bool NeedCache { get; set; } = true;

        /// <summary>
        /// 是否启用动画，默认为true
        /// </summary>
        public bool UseAnim { get; set; } = true;

        /// <summary>
        /// 打开动画类型
        /// </summary>
        public UIAnimType OpenAnim { get; set; } = UIAnimType.ScaleFromCenter;

        /// <summary>
        /// 关闭动画类型
        /// </summary>
        public UIAnimType CloseAnim { get; set; } = UIAnimType.Scale;

        /// <summary>
        /// 动画持续时间
        /// </summary>
        public float AnimDuration { get; set; } = 0.3f;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="prefabPath">预制体路径（不包含后缀）</param>
        public UIAttribute(string prefabPath)
        {
            PrefabPath = prefabPath;
        }
    }

    /// <summary>
    /// UI层级枚举
    /// </summary>
    public enum UILayer
    {
        /// <summary>
        /// 背景层（最底层）
        /// </summary>
        Background = 0,

        /// <summary>
        /// 通用层（普通面板）
        /// </summary>
        Common = 100,

        /// <summary>
        /// 弹窗层（对话框等）
        /// </summary>
        Dialog = 200,

        /// <summary>
        /// 提示层（Toast等）
        /// </summary>
        Tips = 300,

        /// <summary>
        /// 最顶层
        /// </summary>
        Top = 400
    }
}
