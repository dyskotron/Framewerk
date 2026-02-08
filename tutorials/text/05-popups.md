# Chapter 5: Popups

Framewerk provides a complete popup system for modal dialogs, confirmations, and notifications. This chapter covers the architecture, creation, and management of popups.

---

## 5.1 Popup Basics

### Architecture Overview

The popup system consists of three core components:

| Component | Purpose |
|-----------|---------|
| `PopupView` | Base class for popup UI — holds references to button container and prefab |
| `PopupMediator<TView>` | Base class for popup logic — handles button creation and lifecycle |
| `PopupManager` | Service that instantiates popups and tracks open instances |

### Key Interfaces

```csharp
// Marker interface for popup views
public interface IPopupView : IView { }

// Contract for popup mediators
public interface IPopupMediator
{
    void Close();
    Signal<IPopupMediator> PopupClosedSignal { get; }
}
```

### PopupButtonSetting

Dynamic buttons are configured using `PopupButtonSetting`:

```csharp
public class PopupButtonSetting
{
    public string optionText;        // Button label text
    public Action clickHandler;      // Callback on click
    public IPromise clickPromise;    // Promise to dispatch on click
    public bool closesPopup = true;  // Auto-close popup after click?
}
```

---

## 5.2 Creating a Popup

### Using the Component Wizard

The fastest way to create a popup:

1. **Open the Wizard:** `Framewerk → Create UI Component`
2. **Enter a name:** e.g., `Confirm` (wizard appends "Popup" automatically)
3. **Select type:** `Popup`
4. **Configure paths:** Script folder and Prefab folder
5. **Set namespace:** Your project namespace
6. **Click Generate**

The wizard creates:
- `ConfirmPopupView.cs` — View extending `PopupView`
- `ConfirmPopupMediator.cs` — Mediator extending `PopupMediator<T>`
- `ConfirmPopup.prefab` — Addressable prefab

### Generated Code Structure

**View (ConfirmPopupView.cs):**
```csharp
using Framewerk.Popups;

namespace MyGame.UI
{
    public class ConfirmPopupView : PopupView, IPopupView
    {
    }
}
```

**Mediator (ConfirmPopupMediator.cs):**
```csharp
using Framewerk.Popups;

namespace MyGame.UI
{
    public class ConfirmPopupMediator : PopupMediator<ConfirmPopupView>
    {
        public override void OnRegister()
        {
            base.OnRegister();
        }
    }
}
```

### Prefab Setup

Your popup prefab must have:

1. **`buttonContainer`** — Transform where buttons are instantiated
2. **`buttonPrefab`** — Button prefab with `TextMeshProUGUI` child for label

Example hierarchy:
```
ConfirmPopup (ConfirmPopupView)
├── Background (Image - blocks interaction)
├── Panel
│   ├── TitleText (TMP)
│   ├── MessageText (TMP)
│   └── ButtonContainer (HorizontalLayoutGroup)
└── ButtonPrefab (disabled, for cloning)
    └── Label (TextMeshProUGUI)
```

### Adding Custom Content

Extend the view with your UI elements:

```csharp
using Framewerk.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.UI
{
    public class ConfirmPopupView : PopupView, IPopupView
    {
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI messageText;
        public Image iconImage;
    }
}
```

Extend the mediator to handle custom data:

```csharp
using Framewerk.Popups;

namespace MyGame.UI
{
    public class ConfirmPopupMediator : PopupMediator<ConfirmPopupView>
    {
        // Custom data injected when popup is created
        [Inject] public string Title { get; set; }
        [Inject] public string Message { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            
            // Call base Init to set up buttons
            Init(View);
            
            // Set custom content
            View.titleText.text = Title;
            View.messageText.text = Message;
        }
    }
}
```

---

## 5.3 Showing Popups

### Basic Instantiation

Inject `IPopupManager` and call `InstantiatePopup<T>()`:

```csharp
[Inject] public IPopupManager PopupManager { get; set; }

void ShowSimplePopup()
{
    PopupManager.InstantiatePopup<ConfirmPopupView>();
}
```

### With Dynamic Buttons

Pass an array of `PopupButtonSetting`:

```csharp
void ShowConfirmDialog()
{
    var buttons = new PopupButtonSetting[]
    {
        new PopupButtonSetting
        {
            optionText = "Yes",
            clickHandler = OnYesClicked,
            closesPopup = true
        },
        new PopupButtonSetting
        {
            optionText = "No",
            clickHandler = OnNoClicked,
            closesPopup = true
        }
    };

    PopupManager.InstantiatePopup<ConfirmPopupView>(buttons);
}

void OnYesClicked() { Debug.Log("User confirmed!"); }
void OnNoClicked() { Debug.Log("User cancelled."); }
```

### With Text Content

Pass text directly (useful for MessageBox-style popups):

```csharp
// Single message
PopupManager.InstantiatePopup<MessageBoxView>("Operation complete!", buttons);

// Caption and message
PopupManager.InstantiatePopup<MessageBoxView>("Success", "Your file was saved.", buttons);
```

### With Custom Data (Mediator Injects)

Pass custom objects to inject into the mediator:

```csharp
// Pass arbitrary data
var customData = new object[] { "My Title", "Custom message here" };
PopupManager.InstantiatePopup<ConfirmPopupView>(customData);
```

The mediator receives these via `[Inject]` properties matching the types:

```csharp
public class ConfirmPopupMediator : PopupMediator<ConfirmPopupView>
{
    [Inject] public string Title { get; set; }    // Receives "My Title"
    [Inject] public string Message { get; set; }  // Receives "Custom message here"
}
```

### Async Version

For `async/await` workflows:

```csharp
async Task ShowPopupAsync()
{
    var popup = await PopupManager.InstantiatePopupAsync<ConfirmPopupView>(
        buttons,
        customPrefix: null,
        ct: destroyCancellationToken
    );
    
    // Popup is now instantiated and visible
}
```

### Using Promises for Button Clicks

Instead of callbacks, use promises for reactive patterns:

```csharp
var okPromise = new Promise();
okPromise.AddListener(() => Debug.Log("OK was clicked!"));

var buttons = new PopupButtonSetting[]
{
    new PopupButtonSetting
    {
        optionText = "OK",
        clickPromise = okPromise,
        closesPopup = true
    }
};

PopupManager.InstantiatePopup<MessageBoxView>("Done!", buttons);
```

---

## 5.4 Popup Lifecycle

### Signals

The popup system fires two signals:

| Signal | When | Payload |
|--------|------|---------|
| `PopupOpenedSignal` | Popup mediator registers | `IPopupMediator` |
| `PopupClosedSignal` | Popup is closed/destroyed | `IPopupMediator` |

### Listening for Popup Events

```csharp
[Inject] public PopupOpenedSignal PopupOpenedSignal { get; set; }
[Inject] public PopupClosedSignal PopupClosedSignal { get; set; }

public override void OnRegister()
{
    PopupOpenedSignal.AddListener(OnAnyPopupOpened);
    PopupClosedSignal.AddListener(OnAnyPopupClosed);
}

void OnAnyPopupOpened(IPopupMediator popup)
{
    Debug.Log($"Popup opened: {popup.GetType().Name}");
    // Pause game, dim background, etc.
}

void OnAnyPopupClosed(IPopupMediator popup)
{
    Debug.Log($"Popup closed: {popup.GetType().Name}");
    // Resume game, restore UI state
}
```

### Closing Programmatically

**From within the popup mediator:**
```csharp
public void CloseMe()
{
    Close(); // Built-in method, dispatches PopupClosedSignal
}
```

**Close all popups:**
```csharp
[Inject] public IPopupManager PopupManager { get; set; }

void CloseEverything()
{
    PopupManager.CloseAllPopups();
}
```

### Preventing Auto-Close

Set `closesPopup = false` on buttons that shouldn't dismiss:

```csharp
new PopupButtonSetting
{
    optionText = "Apply",
    clickHandler = OnApply,
    closesPopup = false  // Popup stays open
}
```

---

## 5.5 Built-in MessageBox

Framewerk includes a ready-to-use MessageBox for quick notifications.

### Using ShowMessageBoxSignal

```csharp
[Inject] public ShowMessageBoxSignal ShowMessageBoxSignal { get; set; }

void ShowNotification()
{
    var okPromise = new Promise();
    okPromise.AddListener(OnMessageDismissed);
    
    ShowMessageBoxSignal.Dispatch(okPromise, "File saved successfully!");
}

void OnMessageDismissed()
{
    Debug.Log("User dismissed the message");
}
```

### MessageBox Architecture

The signal triggers `ShowMessageBoxCommand`:

```csharp
public class ShowMessageBoxCommand : Command
{
    [Inject] public string Message { get; set; }
    [Inject] public IPromise OkClickedPromise { get; set; }
    [Inject] public IPopupManager PopupManager { get; set; }

    public override void Execute()
    {
        PopupManager.InstantiatePopup<MessageBoxView>(Message, new PopupButtonSetting[]
        {
            new PopupButtonSetting
            {
                clickHandler = () => { },
                clickPromise = OkClickedPromise,
                closesPopup = true,
                optionText = "Ok"
            }
        });
    }
}
```

### Binding the Command

In your context:

```csharp
commandBinder.Bind<ShowMessageBoxSignal>().To<ShowMessageBoxCommand>();
```

---

## 5.6 Advanced: OkCancelWindow

For Yes/No confirmations, use the built-in OkCancelWindow:

### Using ShowOkCancelWindowSignal

```csharp
[Inject] public ShowOkCancelWindowSignal ShowOkCancelWindowSignal { get; set; }

void AskForConfirmation()
{
    ShowOkCancelWindowSignal.Dispatch(OnResult, "Are you sure you want to delete?");
}

void OnResult(bool confirmed)
{
    if (confirmed)
        DeleteItem();
    else
        Debug.Log("Cancelled");
}
```

---

## 5.7 Popup Lists

For popups that display scrollable lists, use `PopupListView` and `PopupListMediator`:

```csharp
public class ItemPickerView : PopupListView
{
    // Inherits ListView functionality + popup behavior
}

public class ItemPickerMediator : PopupListMediator<ItemPickerView, ItemData>
{
    public override void OnRegister()
    {
        base.OnRegister();
        // Set up list data
    }
}
```

---

## Summary

| Task | Method |
|------|--------|
| Create popup | Component Wizard → Popup type |
| Show basic popup | `PopupManager.InstantiatePopup<T>()` |
| Show with buttons | `PopupManager.InstantiatePopup<T>(buttons)` |
| Show with custom data | `PopupManager.InstantiatePopup<T>(new object[] { ... })` |
| Quick message | `ShowMessageBoxSignal.Dispatch(promise, "message")` |
| Yes/No dialog | `ShowOkCancelWindowSignal.Dispatch(callback, "question")` |
| Close popup | `popup.Close()` or `PopupManager.CloseAllPopups()` |
| React to popups | Listen to `PopupOpenedSignal` / `PopupClosedSignal` |

---

## Next Steps

- **Chapter 6:** Asset Management — Loading and skinning with Addressables
- **Chapter 7:** Signals & Commands — Deep dive into the event system
