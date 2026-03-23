using UnityEngine;
using UnityEngine.EventSystems;

namespace Piranha.UI
{
    /// <summary>
    /// UI框架初始化器，挂在场景中用于初始化UI系统
    /// </summary>
    public class UIInitializer : MonoBehaviour
    {
        [SerializeField]
        private Material maskMaterial;

        [SerializeField]
        private Color maskColor = new Color(0, 0, 0, 0.5f);

        private void Awake()
        {
            // 确保UIManager已创建
            if (UIManager.Instance == null)
            {
                GameObject go = new GameObject("UIManager");
                go.AddComponent<UIManager>();
                DontDestroyOnLoad(go);
            }

          
        }

       
       
    }
}
