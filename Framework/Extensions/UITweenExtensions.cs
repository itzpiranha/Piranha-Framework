using UnityEngine;
using DG.Tweening;
using System;

namespace Piranha.UI
{
    /// <summary>
    /// UI动画类型
    /// </summary>
    public enum UIAnimType
    {
        /// <summary>
        /// 无动画
        /// </summary>
        None,

        /// <summary>
        /// 淡入淡出
        /// </summary>
        Fade,

        /// <summary>
        /// 从上方滑入
        /// </summary>
        SlideFromTop,

        /// <summary>
        /// 从下方滑入
        /// </summary>
        SlideFromBottom,

        /// <summary>
        /// 从左侧滑入
        /// </summary>
        SlideFromLeft,

        /// <summary>
        /// 从右侧滑入
        /// </summary>
        SlideFromRight,

        /// <summary>
        /// 缩放弹出
        /// </summary>
        Scale,

        /// <summary>
        /// 从中心缩放
        /// </summary>
        ScaleFromCenter,

        /// <summary>
        /// 自定义
        /// </summary>
        Custom
    }

    /// <summary>
    /// UI动画配置
    /// </summary>
    [Serializable]
    public class UIAnimConfig
    {
        /// <summary>
        /// 打开动画类型
        /// </summary>
        public UIAnimType openAnim = UIAnimType.ScaleFromCenter;

        /// <summary>
        /// 关闭动画类型
        /// </summary>
        public UIAnimType closeAnim = UIAnimType.Scale;

        /// <summary>
        /// 动画持续时间
        /// </summary>
        public float duration = 0.3f;

        /// <summary>
        /// 打开动画延迟
        /// </summary>
        public float openDelay = 0f;

        /// <summary>
        /// 关闭动画延迟
        /// </summary>
        public float closeDelay = 0f;

        /// <summary>
        /// 缓动类型
        /// </summary>
        public Ease ease = Ease.OutBack;

        /// <summary>
        /// 自定义打开动画回调
        /// </summary>
        public Action<GameObject, float, Ease> customOpenAnim;

        /// <summary>
        /// 自定义关闭动画回调
        /// </summary>
        public Action<GameObject, float, Ease> customCloseAnim;
    }

    /// <summary>
    /// UI动画扩展方法
    /// </summary>
    public static class UITweenExtensions
    {
        /// <summary>
        /// 播放打开动画
        /// </summary>
        public static Tweener PlayOpenAnim(this GameObject target, UIAnimConfig config)
        {
            if (config == null || config.openAnim == UIAnimType.None)
                return null;

            // 确保有CanvasGroup组件
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = target.AddComponent<CanvasGroup>();

            return config.openAnim switch
            {
                UIAnimType.Fade => canvasGroup.DOFade(1f, config.duration).From(0f).SetEase(config.ease),
                UIAnimType.SlideFromTop => SlideAnim(target, Vector3.up * Screen.height, Vector3.zero, config),
                UIAnimType.SlideFromBottom => SlideAnim(target, Vector3.down * Screen.height, Vector3.zero, config),
                UIAnimType.SlideFromLeft => SlideAnim(target, Vector3.left * Screen.width, Vector3.zero, config),
                UIAnimType.SlideFromRight => SlideAnim(target, Vector3.right * Screen.width, Vector3.zero, config),
                UIAnimType.Scale => ScaleAnim(target, Vector3.zero, Vector3.one, config),
                UIAnimType.ScaleFromCenter => ScaleFromCenter(target, config),
                UIAnimType.Custom => CustomOpenAnim(target, config),
                _ => null
            };
        }

        /// <summary>
        /// 播放关闭动画
        /// </summary>
        public static Tweener PlayCloseAnim(this GameObject target, UIAnimConfig config, Action onComplete = null)
        {
            if (config == null || config.closeAnim == UIAnimType.None)
                return null;

            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = target.AddComponent<CanvasGroup>();

            return config.closeAnim switch
            {
                UIAnimType.Fade => canvasGroup.DOFade(0f, config.duration).SetEase(config.ease).OnComplete(() => onComplete?.Invoke()),
                UIAnimType.SlideFromTop => SlideAnim(target, Vector3.zero, Vector3.up * Screen.height, config, onComplete),
                UIAnimType.SlideFromBottom => SlideAnim(target, Vector3.zero, Vector3.down * Screen.height, config, onComplete),
                UIAnimType.SlideFromLeft => SlideAnim(target, Vector3.zero, Vector3.left * Screen.width, config, onComplete),
                UIAnimType.SlideFromRight => SlideAnim(target, Vector3.zero, Vector3.right * Screen.width, config, onComplete),
                UIAnimType.Scale => ScaleAnim(target, Vector3.one, Vector3.zero, config, onComplete),
                UIAnimType.ScaleFromCenter => ScaleToCenter(target, config, onComplete),
                UIAnimType.Custom => CustomCloseAnim(target, config, onComplete),
                _ => null
            };
        }

        /// <summary>
        /// 滑行动画
        /// </summary>
        private static Tweener SlideAnim(GameObject target, Vector3 from, Vector3 to, UIAnimConfig config, Action onComplete = null)
        {
            RectTransform rect = target.GetComponent<RectTransform>();
            if (rect == null) return null;

            Vector2 anchoredPos = rect.anchoredPosition;
            rect.anchoredPosition = new Vector2(from.x, from.y);

            return rect.DOAnchorPos(new Vector2(to.x, to.y), config.duration)
                .SetDelay(config.openDelay)
                .SetEase(config.ease)
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// 缩放动画
        /// </summary>
        private static Tweener ScaleAnim(GameObject target, Vector3 from, Vector3 to, UIAnimConfig config, Action onComplete = null)
        {
            target.transform.localScale = from;
            return target.transform.DOScale(to, config.duration)
                .SetDelay(config.openDelay)
                .SetEase(config.ease)
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// 从中心缩放（带弹性）
        /// </summary>
        private static Tweener ScaleFromCenter(GameObject target, UIAnimConfig config)
        {
            target.transform.localScale = Vector3.zero;
            return target.transform.DOScale(Vector3.one, config.duration)
                .SetDelay(config.openDelay)
                .SetEase(Ease.OutBack);
        }

        /// <summary>
        /// 缩放到中心
        /// </summary>
        private static Tweener ScaleToCenter(GameObject target, UIAnimConfig config, Action onComplete = null)
        {
            return target.transform.DOScale(Vector3.zero, config.duration)
                .SetDelay(config.closeDelay)
                .SetEase(Ease.InBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// 自定义打开动画
        /// </summary>
        private static Tweener CustomOpenAnim(GameObject target, UIAnimConfig config)
        {
            config.customOpenAnim?.Invoke(target, config.duration, config.ease);
            return null;
        }

        /// <summary>
        /// 自定义关闭动画
        /// </summary>
        private static Tweener CustomCloseAnim(GameObject target, UIAnimConfig config, Action onComplete = null)
        {
            if (config.customCloseAnim != null)
            {
                config.customCloseAnim?.Invoke(target, config.duration, config.ease);
                return null;
            }
            onComplete?.Invoke();
            return null;
        }

        /// <summary>
        /// 停止目标上所有DOTween动画
        /// </summary>
        public static void StopAllTweens(this GameObject target)
        {
            DOTween.Kill(target);
        }
    }
}
