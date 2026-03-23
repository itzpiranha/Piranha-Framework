using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using DG.Tweening;

namespace Piranha.UI
{
    /// <summary>
    /// 所有UI面板的基类，提供通用生命周期方法
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        /// <summary>
        /// 面板是否处于活动状态
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// 面板所属层级
        /// </summary>
        public UILayer Layer { get; internal set; }

        /// <summary>
        /// 面板的SortOrder（用于同层级内的排序）
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 面板数据
        /// </summary>
        protected PanelData Data { get; private set; }

        /// <summary>
        /// 遮罩图片（如果有）
        /// </summary>
        protected Image maskImage;

        /// <summary>
        /// 动画配置
        /// </summary>
        protected UIAnimConfig animConfig = new UIAnimConfig();

        /// <summary>
        /// 是否启用动画
        /// </summary>
        protected bool useAnim = true;

        /// <summary>
        /// 内部初始化，由UIManager调用
        /// </summary>
        internal void InternalInit()
        {
            OnInit();
        }

        /// <summary>
        /// 内部打开，由UIManager调用
        /// </summary>
        internal void InternalOpen(PanelData data)
        {
            Data = data;
            IsActive = true;
            gameObject.SetActive(true);
            OnOpen(data);
        }

        /// <summary>
        /// 内部关闭，由UIManager调用
        /// </summary>
        internal void InternalClose()
        {
            OnClose();
            IsActive = false;
            gameObject.SetActive(false);
            Data = null;
        }

        /// <summary>
        /// 内部暂停，由UIManager调用（被其他UI遮挡）
        /// </summary>
        internal void InternalPause()
        {
            OnPause();
        }

        /// <summary>
        /// 内部恢复，由UIManager调用（重新显示）
        /// </summary>
        internal void InternalResume()
        {
            OnResume();
        }

        /// <summary>
        /// 面板初始化时调用（只调用一次，在预制体加载后）
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// 面板打开时调用
        /// </summary>
        /// <param name="data">打开时传递的数据</param>
        protected virtual void OnOpen(PanelData data) { }

        /// <summary>
        /// 面板关闭时调用
        /// </summary>
        protected virtual void OnClose() { }

        /// <summary>
        /// 被其他UI遮挡时调用
        /// </summary>
        protected virtual void OnPause() { }

        /// <summary>
        /// 重新显示时调用
        /// </summary>
        protected virtual void OnResume() { }

        /// <summary>
        /// 查找子组件
        /// </summary>
        protected T FindChild<T>(string path) where T : Component
        {
            var trans = transform.Find(path);
            if (trans != null)
                return trans.GetComponent<T>();
            return null;
        }

        /// <summary>
        /// 查找子物体
        /// </summary>
        protected Transform FindChild(string path)
        {
            return transform.Find(path);
        }

        /// <summary>
        /// 关闭当前面板
        /// </summary>
        protected void Close()
        {
            UIManager.Instance.Close(GetType());
        }

        /// <summary>
        /// 带动画关闭面板
        /// </summary>
        protected void CloseWithAnim()
        {
            UIManager.Instance.CloseWithAnim(this);
        }

        /// <summary>
        /// 播放打开动画
        /// </summary>
        internal virtual void PlayOpenAnim()
        {
            if (useAnim && animConfig != null)
            {
                gameObject.PlayOpenAnim(animConfig);
            }
        }

        /// <summary>
        /// 播放关闭动画
        /// </summary>
        internal virtual void PlayCloseAnim(Action onComplete)
        {
            if (useAnim && animConfig != null)
            {
                gameObject.PlayCloseAnim(animConfig, onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// 停止所有动画
        /// </summary>
        protected void StopAllAnim()
        {
            gameObject.StopAllTweens();
        }

        /// <summary>
        /// 设置动画配置
        /// </summary>
        internal void SetAnimConfig(UIAnimConfig config)
        {
            animConfig = config;
        }

        /// <summary>
        /// 设置是否使用动画
        /// </summary>
        internal void SetUseAnim(bool use)
        {
            useAnim = use;
        }
    }
}
