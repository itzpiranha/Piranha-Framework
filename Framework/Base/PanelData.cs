using System;

namespace Piranha.UI
{
    /// <summary>
    /// UI面板数据基类，所有面板传递的数据都继承此类
    /// </summary>
    [Serializable]
    public class PanelData
    {
        /// <summary>
        /// 面板是否需要缓存，默认真实（关闭后缓存）
        /// </summary>
        public bool needCache = true;
    }
}
