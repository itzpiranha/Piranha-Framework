# Piranha-Framework
一个轻量级、高性能的 Unity UI 框架，支持面板管理、动画过渡、缓存复用和安全区适配。


## 目录

- [功能特性](#功能特性)
- [快速开始](#快速开始)
- [核心组件](#核心组件)
- [使用教程](#使用教程)
- [分辨率适配](#分辨率适配)
- [动画系统](#动画系统)
- [API 参考](#api-参考)

---

## 功能特性

| 功能 | 描述 |
|------|------|
| 面板管理 | 打开、关闭、隐藏 UI 面板 |
| 层级系统 | Background → Common → Dialog → Tips → Top |
| 对象缓存 | 关闭后自动缓存，减少 GC |
| 动画过渡 | 支持淡入淡出、滑入滑出、缩放等动画 |
| 安全区适配 | 自动适配刘海屏、挖孔屏 |
| 设备检测 | 自动识别手机/平板并应用不同布局 |

---

## 快速开始

### 1. 创建预制体

在 `Resources/Prefabs/UI/` 目录下创建 UI 预制体：

```
Assets/Resources/Prefabs/UI/
├── Panel_Home.prefab
├── Panel_Game.prefab
├── Panel_Settings.prefab
└── Panel_Shop.prefab
```

### 2. 创建面板脚本

```csharp
using UnityEngine;
using UnityEngine.UI;
using Piranha.UI;

[UI("Panel_Home", Layer = UILayer.Common)]
public class HomePanel : UIBase
{
    [SerializeField]
    private Button btnPlay;

    protected override void OnInit()
    {
        base.OnInit();
        btnPlay?.onClick.AddListener(OnPlayClick);
    }

    protected override void OnOpen(PanelData data)
    {
        base.OnOpen(data);
        Debug.Log("HomePanel 已打开");
    }

    private void OnPlayClick()
    {
        UIManager.Instance.Open<GamePanel>();
    }
}
```

### 3. 在场景中使用

在场景中创建空物体，挂载 `UIInitializer` 组件：

```csharp
// 打开面板
UIManager.Instance.Open<HomePanel>();

// 打开面板并传递数据
UIManager.Instance.Open<SettingsPanel>(new SettingsData {
    musicVolume = 0.5f,
    sfxVolume = 0.8f
});

// 关闭面板
UIManager.Instance.Close<HomePanel>();

// 关闭所有面板
UIManager.Instance.CloseAll();
```

---

## 核心组件

### UIManager

UI 管理器单例，负责所有面板的创建、显示、隐藏和销毁。

**挂载方式**：无需手动挂载，`UIInitializer` 会自动创建。

**常用 API**：

```csharp
// 打开面板
UIManager.Instance.Open<T>(data);
UIManager.Instance.Open<T>(data, animConfig); // 带动画配置

// 关闭面板
UIManager.Instance.Close<T>();
UIManager.Instance.Close(panel); // 直接传入面板实例
UIManager.Instance.CloseAll();
UIManager.Instance.CloseTop(); // 关闭最顶层

// 获取面板
UIManager.Instance.Get<T>(); // 获取已打开的面板

// 缓存管理
UIManager.Instance.ClearCache();
```

### UIBase

所有 UI 面板的基类。

```csharp
protected override void OnInit()
{
    // 面板初始化（只调用一次）
}

protected override void OnOpen(PanelData data)
{
    // 面板打开时调用
}

protected override void OnClose()
{
    // 面板关闭时调用
}

protected override void OnPause()
{
    // 被其他面板遮挡时调用
}

protected override void OnResume()
{
    // 重新显示时调用
}
```

### UIAttribute

特性，用于声明面板属性。

```csharp
[UI("预制体名称", Layer = UILayer.层级)]
[UI("Panel_Home", Layer = UILayer.Common)]

// 完整参数示例
[UI("Panel_Settings",
    Layer = UILayer.Dialog,      // 层级
    ShowMask = true,              // 显示遮罩
    MaskClose = true,             // 点击遮罩关闭
    NeedCache = true,             // 关闭后缓存
    UseAnim = true,               // 启用动画
    OpenAnim = UIAnimType.ScaleFromCenter,
    CloseAnim = UIAnimType.Scale,
    AnimDuration = 0.3f)]
```

### UILayer

UI 层级枚举：

| 层级 | 值 | 用途 |
|------|-----|------|
| Background | 0 | 背景图 |
| Common | 100 | 普通面板 |
| Dialog | 200 | 弹窗 |
| Tips | 300 | 提示/Toast |
| Top | 400 | 最高层级 |

---

## 使用教程

### 创建第一个面板

**Step 1**: 创建预制体

1. 在 Unity 中创建 Canvas
2. 设计 UI 布局
3. 创建一个空物体作为面板根节点，挂载您的面板脚本
4. 将整个面板保存为预制体，放到 `Resources/Prefabs/UI/` 目录
5. 删除场景中的预制体

**Step 2**: 创建脚本

```csharp
using UnityEngine;
using UnityEngine.UI;
using Piranha.UI;

[UI("Panel_MyPanel", Layer = UILayer.Common)]
public class MyPanel : UIBase
{
    [SerializeField]
    private Text titleText;

    [SerializeField]
    private Button btnClose;

    protected override void OnInit()
    {
        base.OnInit();
        btnClose?.onClick.AddListener(() => Close());
    }

    protected override void OnOpen(PanelData data)
    {
        base.OnOpen(data);

        // 处理传入的数据
        if (data is MyPanelData myData)
        {
            titleText.text = myData.title;
        }
    }
}

// 定义面板数据
public class MyPanelData : PanelData
{
    public string title = "默认标题";
    public int level = 1;
}
```

**Step 3**: 打开面板

```csharp
// 简单打开
UIManager.Instance.Open<MyPanel>();

// 带数据打开
var data = new MyPanelData { title = "新标题", level = 5 };
UIManager.Instance.Open<MyPanel>(data);
```

### 面板数据传递

```csharp
// 定义数据类（继承 PanelData）
public class GamePanelData : PanelData
{
    public int level;
    public string playerName;
    public int score;
}

// 发送方
UIManager.Instance.Open<GamePanel>(new GamePanelData {
    level = 3,
    playerName = "Player1",
    score = 1000
});

// 接收方
protected override void OnOpen(PanelData data)
{
    base.OnOpen(data);

    if (data is GamePanelData gameData)
    {
        Debug.Log($"Level: {gameData.level}, Player: {gameData.playerName}");
    }
}
```

### 带遮罩的弹窗

```csharp
// 设置遮罩，点击关闭
[UI("Panel_Dialog", Layer = UILayer.Dialog, ShowMask = true, MaskClose = true)]
public class DialogPanel : UIBase
{
    // ...
}
```

---

## 分辨率适配

### 设备类型检测

框架会自动检测设备类型：

```csharp
// 获取当前设备类型
DeviceType deviceType = UIManager.Instance.CurrentDeviceType;

// 或使用静态类
DeviceType type = UIDeviceInfo.DeviceType;

// 判断设备
if (deviceType == DeviceType.Phone)
{
    // 手机逻辑
}
else if (deviceType == DeviceType.Tablet)
{
    // 平板逻辑
}
```

### 安全区适配

#### 方式一：全局安全区（UIManager 自动应用）

在 `UIInitializer` 或 `UIManager` 上启用 `Enable Safe Area`。

#### 方式二：组件安全区（推荐）

在需要适配安全区的 UI 元素上添加 `UISafeArea` 组件：

1. 选中需要适配的 UI 元素
2. 添加组件 `UISafeArea`
3. 设置 `Mode`：

| 模式 | 描述 |
|------|------|
| Top | 顶部安全区 |
| Bottom | 底部安全区 |
| Left | 左侧安全区 |
| Right | 右侧安全区 |
| All | 四周安全区 |
| Horizontal | 左右安全区 |
|安全区 |

4. 可 Vertical | 上下设置 `Extra Padding` 进行微调

```csharp
// 代码控制
UISafeArea safeArea = GetComponent<UISafeArea>();
safeArea.SetMode(SafeAreaMode.Bottom);
safeArea.SetExtraPadding(new Vector4(0, 10, 0, 0));
```

### 分辨率配置文件

创建自定义分辨率配置：

1. 在 Project 窗口右键 → Create → Piranha → UI Resolution Config
2. 配置参数：

| 参数 | 描述 |
|------|------|
| Phone Reference Resolution | 手机参考分辨率 |
| Phone Match Mode | 手机匹配模式 |
| Tablet Reference Resolution | 平板参考分辨率 |
| Tablet Match Mode | 平板匹配模式 |
| Tablet Aspect Threshold | 平板宽高比阈值（默认 1.5） |

3. 将配置文件拖入 `UIManager` 的 `Resolution Config` 字段

---

## 动画系统

### 内置动画类型

```csharp
public enum UIAnimType
{
    None,              // 无动画
    Fade,               // 淡入淡出
    SlideFromTop,       // 从上方滑入
    SlideFromBottom,    // 从下方滑入
    SlideFromLeft,      // 从左侧滑入
    SlideFromRight,     // 从右侧滑入
    Scale,              // 缩放弹出
    ScaleFromCenter,    // 从中心缩放
    Custom              // 自定义
}
```

### 使用方式

#### 方式一：通过特性配置

```csharp
[UI("Panel_My",
    UseAnim = true,
    OpenAnim = UIAnimType.ScaleFromCenter,
    CloseAnim = UIAnimType.Scale,
    AnimDuration = 0.3f)]
public class MyPanel : UIBase { }
```

#### 方式二：代码配置动画

```csharp
// 打开时带自定义动画
UIAnimConfig config = new UIAnimConfig
{
    openAnim = UIAnimType.SlideFromBottom,
    closeAnim = UIAnimType.SlideFromTop,
    duration = 0.4f,
    ease = Ease.OutBack
};

UIManager.Instance.Open<MyPanel>(data, config);
```

#### 方式三：自定义动画

```csharp
UIAnimConfig config = new UIAnimConfig
{
    openAnim = UIAnimType.Custom,
    customOpenAnim = (target, duration, ease) =>
    {
        // 自定义动画逻辑
        target.transform.DOScale(Vector3.one, duration).SetEase(ease);
    }
};
```

### 面板内播放动画

```csharp
protected override void PlayOpenAnim()
{
    // 使用特性中配置的动画
    base.PlayOpenAnim();

    // 或手动播放
    gameObject.PlayOpenAnim(new UIAnimConfig {
        openAnim = UIAnimType.Fade,
        duration = 0.3f
    });
}
```

---

## API 参考

### UIManager

| 方法 | 描述 |
|------|------|
| `Open<T>(data)` | 打开面板 |
| `Open<T>(data, config)` | 带动画配置打开 |
| `Close<T>()` | 关闭面板 |
| `Close(panel)` | 关闭指定面板 |
| `CloseAll()` | 关闭所有面板 |
| `CloseTop()` | 关闭最顶层面板 |
| `Get<T>()` | 获取已打开的面板 |
| `ClearCache()` | 清除所有缓存 |
| `GetSafeArea()` | 获取安全区域 |
| `CurrentDeviceType` | 当前设备类型 |

### UIBase

| 方法 | 描述 |
|------|------|
| `OnInit()` | 初始化（只调用一次） |
| `OnOpen(data)` | 打开时调用 |
| `OnClose()` | 关闭时调用 |
| `OnPause()` | 暂停（被遮挡） |
| `OnResume()` | 恢复（重新显示） |
| `FindChild<T>(path)` | 查找子组件 |
| `Close()` | 关闭当前面板 |
| `CloseWithAnim()` | 带动画关闭 |

### UIDeviceInfo

| 属性 | 描述 |
|------|------|
| `DeviceType` | 设备类型 |
| `Orientation` | 屏幕方向 |
| `AspectRatio` | 宽高比 |
| `SafeArea` | 安全区域 |
| `IsNotched` | 是否是全面屏 |

---

## 最佳实践

### 1. 预制体命名规范

```
Panel_XXX.prefab
- Panel_Home.prefab
- Panel_Game.prefab
- Panel_Settings.prefab
- Dialog_Confirm.prefab
- Tips_Toast.prefab
```

### 2. 脚本放置位置

```
Assets/Scripts/Framework/
├── Base/           # 基类和配置
├── Core/           # 核心管理器
├── Components/     # 扩展组件

```

### 3. 数据传递

- 使用 `PanelData` 的子类传递数据
- 避免在面板间直接引用

### 4. 缓存使用

- 频繁打开关闭的面板建议开启缓存 (`NeedCache = true`)
- 一次性面板关闭后销毁 (`NeedCache = false`)

---

## 常见问题

### Q: 面板预制体放在哪里？

**A**: 放在 `Resources/Prefabs/UI/` 目录下，路径与 `[UI("名称")]` 中的名称对应。

例如：`[UI("Panel_Home")]` → `Resources/Prefabs/UI/Panel_Home.prefab`

### Q: 如何禁用动画？

**A**: 设置 `[UI(..., UseAnim = false)]` 或在代码中 `panel.SetUseAnim(false)`

### Q: 打开面板后点击无响应？

**A**: 检查面板的 `Raycast Target` 是否正确设置，以及 Canvas 层级是否正确。

### Q: 安全区不生效？

**A**:
1. 确保 `UIManager` 上启用了 `Enable Safe Area`
2. 或在需要适配的元素上添加 `UISafeArea` 组件
3. 在真机上测试（模拟器可能不准确）

---

## 更新日志

### v1.0.0

- 初始版本
- 面板管理
- 动画系统
- 缓存机制
- 安全区适配
- 设备类型检测
