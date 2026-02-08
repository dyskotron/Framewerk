# Chapter 5: Popups — Video Script

## Video Overview
- **Duration:** ~12-15 minutes
- **Format:** Screen recording with voice-over
- **Goal:** Show complete popup creation and usage workflow

---

## INTRO (0:00 - 0:30)

### Visual
- Framewerk logo animation
- Quick montage of popups in action (MessageBox, confirmation dialog, list picker)

### Script
> "In this chapter, we'll explore Framewerk's popup system. You'll learn how to create modal dialogs, confirmation windows, and notification boxes — the bread and butter of any game UI. Let's dive in."

---

## SECTION 1: Popup Architecture (0:30 - 2:30)

### Visual
- Show diagram: PopupView → PopupMediator → PopupManager triangle
- Highlight each component as mentioned

### Script
> "Framewerk's popup system has three main components. First, PopupView — the base class for your popup UI. Second, PopupMediator — handles the logic, button creation, and lifecycle. Third, PopupManager — a service that instantiates popups and tracks what's open."

### Visual
- Open `PopupView.cs` in IDE, highlight key fields:
  - `buttonContainer`
  - `buttonPrefab`

### Script
> "PopupView is intentionally minimal. It just holds two references: a button container where dynamic buttons will spawn, and a button prefab to clone."

### Visual
- Open `PopupMediator.cs`, highlight:
  - `PopupButtonSetting` injection
  - `Init()` method
  - `PopupClosedSignal`

### Script
> "PopupMediator does the heavy lifting. It receives button settings via injection, creates buttons in the Init method, and dispatches a signal when closed. Your popup mediators will extend this."

### Visual
- Show `PopupButtonSetting` class

### Script
> "PopupButtonSetting is how you configure each button: the label text, a click handler or promise, and whether clicking it closes the popup."

---

## SECTION 2: Creating a Popup with the Wizard (2:30 - 5:30)

### Visual
- Unity Editor open
- Click Framewerk menu

### Script
> "Let's create a confirmation popup from scratch. Go to Framewerk, Create UI Component."

### Visual
- Wizard window opens
- Fill in:
  - Name: `Confirm`
  - Type: `Popup` (dropdown)
  - Script folder: browse to `Assets/Scripts/UI`
  - Prefab folder: `Assets/Prefabs/UI`
  - Namespace: `MyGame.UI`

### Script
> "I'll name this Confirm — the wizard automatically appends 'Popup' to the prefab name. Select Popup from the type dropdown. Choose where to save scripts and the prefab. Set your namespace."

### Visual
- Point out Preview section showing:
  - `ConfirmPopupView.cs`
  - `ConfirmPopupMediator.cs`  
  - `ConfirmPopup.prefab`
  - Addressable ID

### Script
> "The preview shows exactly what will be generated. Notice the addressable ID — this is how PopupManager finds your prefab."

### Visual
- Click Generate
- Show console: "Waiting for recompile..."
- After recompile, show Project window with new files

### Script
> "Click Generate. Unity compiles the new scripts, then the wizard creates the prefab and makes it addressable."

### Visual
- Open the generated `ConfirmPopupView.cs`
- Show it extends `PopupView, IPopupView`

### Script
> "The generated view is clean — just extends PopupView and implements IPopupView. We'll add our custom fields here."

### Visual
- Open `ConfirmPopupMediator.cs`
- Show it extends `PopupMediator<ConfirmPopupView>`

### Script
> "The mediator extends the generic PopupMediator with our view type. This gives us access to the View property and all the popup infrastructure."

---

## SECTION 3: Setting Up the Prefab (5:30 - 7:30)

### Visual
- Double-click `ConfirmPopup.prefab` to open in Prefab Mode
- Show empty prefab

### Script
> "Let's set up the prefab UI. Every popup needs at least a button container and button prefab."

### Visual
- Build UI step by step:
  1. Add Image as background (semi-transparent black)
  2. Add Panel child with background
  3. Add TextMeshPro for title
  4. Add TextMeshPro for message
  5. Add empty GameObject "ButtonContainer" with HorizontalLayoutGroup
  6. Create Button prefab with TMP child

### Script
> "I'll add a semi-transparent background to block interaction, a panel for content, title and message text fields, and a button container with a horizontal layout group."

### Visual
- Create a button with TMP Label child
- Drag to `buttonPrefab` field on View
- Disable the button in hierarchy

### Script
> "For the button prefab, create a simple button with a TextMeshPro label. Drag it to the buttonPrefab field on the view component, then disable it — it's just a template for cloning."

### Visual
- Drag ButtonContainer to `buttonContainer` field
- Show completed prefab hierarchy

### Script
> "Assign the button container reference. Now our popup is ready to receive dynamic buttons at runtime."

---

## SECTION 4: Adding Custom Content (7:30 - 9:00)

### Visual
- Edit `ConfirmPopupView.cs`
- Add fields:
```csharp
public TextMeshProUGUI titleText;
public TextMeshProUGUI messageText;
```

### Script
> "Let's add our custom text references to the view."

### Visual
- Edit `ConfirmPopupMediator.cs`
- Add injected properties and Init override:
```csharp
[Inject] public string Title { get; set; }
[Inject] public string Message { get; set; }

public override void OnRegister()
{
    base.OnRegister();
    Init(View);
    View.titleText.text = Title;
    View.messageText.text = Message;
}
```

### Script
> "In the mediator, we inject Title and Message strings. In OnRegister, after calling the base method, we call Init to set up buttons, then populate our custom text fields."

### Visual
- Back to Unity, assign TMP references in prefab

### Script
> "Back in Unity, wire up the text component references and save the prefab."

---

## SECTION 5: Showing the Popup (9:00 - 11:00)

### Visual
- Create test script or open existing mediator
- Type code for showing popup

### Script
> "Now let's show our popup. Inject IPopupManager, then call InstantiatePopup."

### Visual
- Show basic call:
```csharp
PopupManager.InstantiatePopup<ConfirmPopupView>();
```

### Script
> "The simplest call just specifies the view type. But we want to pass data and buttons."

### Visual
- Show full example:
```csharp
var buttons = new PopupButtonSetting[]
{
    new PopupButtonSetting
    {
        optionText = "Yes",
        clickHandler = () => Debug.Log("Confirmed!"),
        closesPopup = true
    },
    new PopupButtonSetting
    {
        optionText = "No",  
        clickHandler = () => Debug.Log("Cancelled"),
        closesPopup = true
    }
};

PopupManager.InstantiatePopup<ConfirmPopupView>(
    new object[] { "Delete Item?", "This cannot be undone." },
    buttons
);
```

### Script
> "Create an array of button settings with labels and handlers. Then pass custom data as an object array — Title and Message in order — followed by the buttons."

### Visual
- Play mode, trigger popup
- Show popup appearing with correct text and buttons
- Click each button, show console logs

### Script
> "Let's test it. The popup appears with our custom message. Each button works and closes the popup."

---

## SECTION 6: Built-in MessageBox (11:00 - 12:00)

### Visual
- Show code using ShowMessageBoxSignal

### Script
> "For quick notifications, Framewerk includes ShowMessageBoxSignal. Just dispatch it with a promise and message."

### Visual
```csharp
var okPromise = new Promise();
okPromise.AddListener(() => Debug.Log("Dismissed"));
ShowMessageBoxSignal.Dispatch(okPromise, "Save complete!");
```

### Script
> "The promise fires when the user clicks OK. This is perfect for simple alerts without creating custom popups."

### Visual
- Run and show MessageBox appearing

### Script
> "Make sure you've bound ShowMessageBoxSignal to ShowMessageBoxCommand in your context."

---

## SECTION 7: Lifecycle & Cleanup (12:00 - 13:30)

### Visual
- Show listening to PopupOpenedSignal and PopupClosedSignal

### Script
> "You can react to any popup opening or closing by listening to the global signals."

### Visual
```csharp
PopupOpenedSignal.AddListener(popup => {
    // Pause game, show overlay
});

PopupClosedSignal.AddListener(popup => {
    // Resume game
});
```

### Script
> "Use this to pause gameplay, dim the screen, or track analytics."

### Visual
- Show CloseAllPopups call

### Script
> "To close all popups at once — maybe when returning to the main menu — call CloseAllPopups on the PopupManager."

---

## OUTRO (13:30 - 14:00)

### Visual
- Split screen showing popup code and running popup
- Transition to chapter summary card

### Script
> "That's the popup system! You've learned to create custom popups with the wizard, configure dynamic buttons, pass custom data, and manage popup lifecycle. Next chapter, we'll explore asset management and skinning. See you there!"

### Visual
- End card with:
  - "Chapter 5: Popups ✓"
  - "Next: Chapter 6 — Asset Management"
  - Framewerk logo

---

## B-ROLL SHOTS NEEDED

1. **Popup montage:** Various popups opening/closing (5 seconds)
2. **Wizard walkthrough:** Screen recording of full wizard flow
3. **Prefab setup:** Building UI hierarchy in prefab mode
4. **Runtime demo:** Popup appearing with button interactions
5. **MessageBox demo:** Quick notification appearing

---

## KEY TIMESTAMPS

| Timestamp | Topic |
|-----------|-------|
| 0:30 | Architecture overview |
| 2:30 | Using Component Wizard |
| 5:30 | Prefab setup |
| 7:30 | Custom content |
| 9:00 | Showing popups |
| 11:00 | Built-in MessageBox |
| 12:00 | Lifecycle signals |

---

## GRAPHICS/OVERLAYS

1. **Architecture diagram:** Three boxes connected (View → Mediator → Manager)
2. **PopupButtonSetting fields:** Annotated code block
3. **Prefab hierarchy:** Visual tree diagram
4. **Summary card:** Key methods table
