# Chapter 4: Lists — Video Script

**Duration**: ~12-15 minutes  
**Format**: Screen recording with voiceover

---

## INTRO (0:00 - 0:30)

**VISUAL**: Framewerk logo → Demo list UI scrolling  
**VOICEOVER**:
> "Lists are everywhere in games — inventory screens, leaderboards, friend lists, shop items. In this chapter, we'll build a data-driven list from scratch using Framewerk's list system."

**VISUAL**: Quick preview of final result (friend list with avatars, selection)

---

## SECTION 1: List Anatomy (0:30 - 2:30)

### Shot 1.1 — Architecture Diagram

**VISUAL**: Animated diagram showing the 5 components  
**VOICEOVER**:
> "Framewerk lists have five main pieces. Let me break them down."

**VISUAL**: Highlight each as mentioned:
> "**ListView** — the container MonoBehaviour. It holds references to where items spawn, what prefab to use, and an optional empty state."
> 
> "**ListItemView** — goes on each item prefab. It has a SelectButton for click handling."
>
> "**IListItemDataProvider** — a marker interface for your data class. Each item in your list gets one."
>
> "**ListMediator** — the brain of the list. Spawns items, manages selection, and binds data."
>
> "**ListItemMediator** — handles individual items. Receives data, dispatches clicks."

### Shot 1.2 — Code Hierarchy

**VISUAL**: Show class hierarchy in IDE  
**VOICEOVER**:
> "The class hierarchy is clean. ListView extends View. ListMediator is generic — it takes your view type and data type. Same pattern for items."

**VISUAL**: Scroll through ListMediator.cs briefly  
**VOICEOVER**:
> "SetData takes a list and handles everything — reusing items, creating new ones, hiding extras. You don't manage the pooling yourself."

---

## SECTION 2: Creating a List with the Wizard (2:30 - 5:00)

### Shot 2.1 — Opening the Wizard

**VISUAL**: Unity Editor → Framewerk menu → Create UI Component  
**VOICEOVER**:
> "Let's build a player list. Open the Component Wizard from the Framewerk menu."

### Shot 2.2 — Configuring the Component

**VISUAL**: Fill in wizard fields (screen recording)
- Name: "Player"
- Type: Select "List"

**VOICEOVER**:
> "Name it 'Player'. The wizard automatically appends 'List' — so this creates PlayerList. Select 'List' as the type."

### Shot 2.3 — Configuring Paths

**VISUAL**: Set paths  
**VOICEOVER**:
> "Set your script folder — I'll use Assets/Scripts/UI/PlayerList. And the prefab folder — Assets/Prefabs/UI."

### Shot 2.4 — Code Generation Settings

**VISUAL**: Set namespace, enable "Setup in Context"  
**VOICEOVER**:
> "Enter your namespace. Enable 'Setup in Context' — this automatically adds the mediation binding to your context file."

### Shot 2.5 — Address Configuration

**VISUAL**: Select bootstrap, show preview  
**VOICEOVER**:
> "Select your target bootstrap. The wizard reads the ContextPrefixSO and shows you exactly what addressable IDs will be created."

### Shot 2.6 — Generate

**VISUAL**: Click Generate, show console output, files appearing  
**VOICEOVER**:
> "Click Generate. Wait for the recompile... and there they are — five scripts and two prefabs."

**VISUAL**: Show Project window with all generated files  
**VOICEOVER**:
> "PlayerListView, PlayerListMediator, PlayerListData, PlayerListItemView, PlayerListItemMediator. Plus the prefabs for the list and list item."

---

## SECTION 3: Setting Up the Prefabs (5:00 - 7:00)

### Shot 3.1 — List Container Prefab

**VISUAL**: Open PlayerList.prefab in Inspector  
**VOICEOVER**:
> "Let's set up the prefabs. Open the PlayerList prefab."

**VISUAL**: Show ListView component fields  
**VOICEOVER**:
> "Three fields to assign: ContentsParent — I'll create a ScrollView with a Vertical Layout Group. ItemPrefab — drag in the PlayerListItem prefab. EmptyContent — optional, but useful."

**VISUAL**: Create scroll view, add vertical layout group, assign references  

### Shot 3.2 — List Item Prefab

**VISUAL**: Open PlayerListItem.prefab  
**VOICEOVER**:
> "Now the item prefab. The ListItemView has a SelectButton field — this is what triggers selection when clicked."

**VISUAL**: Add UI elements (background, label, button)  
**VOICEOVER**:
> "Add your UI — a background image, a text label, whatever you need. Make sure the button covers the clickable area."

**VISUAL**: Assign SelectButton, add custom references  
**VOICEOVER**:
> "Assign the button to SelectButton. Now let's add a field for our label in the view script."

**VISUAL**: Edit PlayerListItemView.cs, add `public TextMeshProUGUI nameLabel;`  
**VOICEOVER**:
> "Add a reference for the name label. Save, go back to the prefab, and assign it."

---

## SECTION 4: Populating the List (7:00 - 9:00)

### Shot 4.1 — Data Class

**VISUAL**: Open PlayerListData.cs  
**VOICEOVER**:
> "First, let's add fields to our data class."

**VISUAL**: Add fields:
```csharp
public string PlayerId;
public string PlayerName;
public int Score;
```

**VOICEOVER**:
> "Player ID, name, score — whatever your list needs to display."

### Shot 4.2 — Binding Data to View

**VISUAL**: Open PlayerListItemMediator.cs  
**VOICEOVER**:
> "Now we bind data to the view. Override SetData in the item mediator."

**VISUAL**: Type out the code:
```csharp
public override void SetData(PlayerListData data, int index)
{
    base.SetData(data, index);
    View.nameLabel.text = data.PlayerName;
}
```

**VOICEOVER**:
> "Call the base method, then bind your fields. The data comes in, you push it to the UI. Simple."

### Shot 4.3 — Calling SetData

**VISUAL**: Open PlayerListMediator.cs  
**VOICEOVER**:
> "Finally, populate the list from the mediator. In OnRegister, create your data and call SetData."

**VISUAL**: Type out:
```csharp
public override void OnRegister()
{
    base.OnRegister();
    
    var players = new List<PlayerListData>
    {
        new PlayerListData { PlayerName = "Alice", Score = 1500 },
        new PlayerListData { PlayerName = "Bob", Score = 1200 },
        new PlayerListData { PlayerName = "Charlie", Score = 900 }
    };
    
    SetData(players);
}
```

### Shot 4.4 — Run It

**VISUAL**: Play mode, show list populating  
**VOICEOVER**:
> "Hit play... and there's our list. Three items, each showing the player name."

---

## SECTION 5: Selection & Multiselect (9:00 - 11:00)

### Shot 5.1 — Click Selection

**VISUAL**: Click items, show selection highlighting  
**VOICEOVER**:
> "Selection works out of the box. Click an item — it's selected. Click another — it switches."

### Shot 5.2 — Selection Callbacks

**VISUAL**: Override ListItemSelected in mediator  
**VOICEOVER**:
> "To respond to selection, override ListItemSelected."

**VISUAL**: Type:
```csharp
protected override void ListItemSelected(int index, PlayerListData data)
{
    Debug.Log($"Selected: {data.PlayerName}");
}
```

**VISUAL**: Play, click items, show console logs  

### Shot 5.3 — Programmatic Selection

**VISUAL**: Add selection code  
**VOICEOVER**:
> "Select items from code with SelectItemAt. Clear selection with UnselectAll."

**VISUAL**: Demo SelectItemAt(0), UnselectAll()  

### Shot 5.4 — Multiselect

**VISUAL**: Enable Multiselect flag  
**VOICEOVER**:
> "For multiple selection — like a shopping cart — set Multiselect to true."

**VISUAL**: Type in OnRegister:
```csharp
Multiselect = true;
Unselectable = true;
```

**VISUAL**: Play, click multiple items  
**VOICEOVER**:
> "Now you can select multiple items. Unselectable lets you toggle off by clicking again."

### Shot 5.5 — Visual Feedback

**VISUAL**: Override SetSelected in item mediator  
**VOICEOVER**:
> "For custom selection visuals, override SetSelected in the item mediator."

**VISUAL**: Type:
```csharp
public override void SetSelected(bool selected)
{
    base.SetSelected(selected);
    View.background.color = selected ? Color.yellow : Color.white;
}
```

**VISUAL**: Play, show color changes on selection  

---

## SECTION 6: Dynamic Lists (11:00 - 13:00)

### Shot 6.1 — Pooling Explained

**VISUAL**: Diagram showing item reuse  
**VOICEOVER**:
> "Framewerk lists pool items automatically. When you call SetData with fewer items, extras are hidden — not destroyed. When you need more, they're reactivated first, only creating new ones if necessary."

### Shot 6.2 — Removing Items

**VISUAL**: Code example  
**VOICEOVER**:
> "Remove a specific item with RemoveItemAt."

**VISUAL**: Demo removing item, list updating  

### Shot 6.3 — Empty Content

**VISUAL**: Show EmptyContent setup  
**VOICEOVER**:
> "Set up an EmptyContent panel for when the list is empty. Assign it in the ListView — it shows automatically when there's no data."

**VISUAL**: Clear the list, show empty state appearing  

---

## SECTION 7: Complete Example (13:00 - 14:00)

**VISUAL**: Scrolling through the complete friend list example from the written tutorial  
**VOICEOVER**:
> "The written tutorial has a complete friend list example with avatars, online status, and async loading. It shows all these concepts working together."

**VISUAL**: Show the finished demo running  
**VOICEOVER**:
> "This pattern handles most list UIs you'll need — leaderboards, inventories, chat lists, anything data-driven."

---

## OUTRO (14:00 - 14:30)

**VISUAL**: Summary card showing key points  
**VOICEOVER**:
> "To recap: Wizard generates everything. Data class holds your content. Item mediator binds it to UI. List mediator manages selection and pooling. It's the same pattern every time."

**VISUAL**: Transition to next chapter preview  
**VOICEOVER**:
> "Next chapter: Events and Signals — how components talk to each other without tight coupling."

---

## B-ROLL NEEDED

| Timestamp | Description |
|-----------|-------------|
| 0:20 | Demo list scrolling (final result preview) |
| 2:00 | Code scrolling through ListMediator.cs |
| 6:30 | Fast-forward prefab setup |
| 8:30 | Play mode list population |
| 10:30 | Multi-selection demo |
| 12:00 | Pooling visualization (items hiding/showing) |

## GRAPHICS NEEDED

| Item | Description |
|------|-------------|
| Architecture diagram | 5 components with arrows showing relationships |
| Class hierarchy | ListView → ListMediator, ListItemView → ListItemMediator |
| Data flow diagram | SetData → ItemMediators → Views |
| Summary card | Key points bullet list |

---

## RECORDING NOTES

- Keep cursor movements smooth and intentional
- Pause on important code for 2-3 seconds
- Use consistent project/namespace throughout
- Test all code before recording to avoid errors
- Consider split-screen for code + result when demoing
