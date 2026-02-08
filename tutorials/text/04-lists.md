# Chapter 4: Lists

Building data-driven list UIs in Framewerk.

---

## 4.1 List Anatomy

Framewerk's list system separates concerns across five core pieces:

| Component | Role |
|-----------|------|
| **ListView** | MonoBehaviour on list container. References ContentsParent (where items spawn), ItemPrefab, and optional EmptyContent |
| **ListItemView** | MonoBehaviour on each list item prefab. Contains SelectButton for click handling |
| **IListItemDataProvider** | Marker interface for your data class. Each list item gets one |
| **ListMediator** | Manages the list: spawns items, handles selection, binds data |
| **ListItemMediator** | Mediates individual items: receives data, dispatches clicks |

### Class Hierarchy

```
ListView : View
    └── ContentsParent: RectTransform (where items spawn)
    └── ItemPrefab: GameObject (the item template)
    └── EmptyContent: GameObject (shown when list is empty)

ListItemView : View
    └── SelectButton: Button (triggers selection)

ListMediator<TView, TData> : ListBaseMediator<TView, TData>
    └── SetData(List<TData>) — populates the list
    └── SelectItemAt(int) — select programmatically
    └── UnselectAll() — clear selection
    └── RemoveItemAt(int) — dynamic removal
    └── Multiselect: bool — allow multiple selection
    └── Unselectable: bool — allow toggling selection off

ListItemMediator<TView, TData> : ExtendedMediator<TView>
    └── SetData(TData, int) — receives data from parent list
    └── SetSelected(bool) — visual selection state
    └── ListItemClickedSignal — dispatched when clicked
```

### Data Flow

```
1. ListMediator.SetData(List<Data>) called
2. For each data item:
   - Reuse existing ItemMediator if available (pooling)
   - Or instantiate new item from ItemPrefab
   - Call ItemMediator.SetData(dataItem, index)
3. Hide excess item mediators (pooling)
4. Show EmptyContent if list is empty
```

---

## 4.2 Creating a List with the Wizard

The **Component Wizard** generates all required files for a list in one step.

### Step-by-Step

1. **Open Wizard**: `Framewerk > Create UI Component`

2. **Configure Component**:
   - **Name**: `Player` (wizard appends "List" automatically → `PlayerList`)
   - **Type**: Select "List"

3. **Configure Paths**:
   - **Script Folder**: e.g., `Assets/Scripts/UI/PlayerList`
   - **Prefab Folder**: e.g., `Assets/Prefabs/UI`

4. **Configure Code Generation**:
   - **Namespace**: e.g., `MyGame.UI`
   - **Setup in Context**: ✓ (adds mediation binding)
   - **Target Context**: Select your context

5. **Configure Address**:
   - **Target Bootstrap**: Select your bootstrap (reads ContextPrefixSO)
   - **Custom Prefix**: Leave empty or add feature prefix

6. **Preview**: Verify generated files and addressable IDs

7. **Generate**: Click Generate and wait for recompile

### Generated Files

| File | Purpose |
|------|---------|
| `PlayerListView.cs` | List container view |
| `PlayerListMediator.cs` | List logic and data binding |
| `PlayerListData.cs` | Data class (IListItemDataProvider) |
| `PlayerListItemView.cs` | Individual item view |
| `PlayerListItemMediator.cs` | Individual item logic |
| `PlayerList.prefab` | List container prefab |
| `PlayerListItem.prefab` | List item prefab |

### Generated Code

**PlayerListData.cs**
```csharp
using Framewerk.UI.List;

namespace MyGame.UI
{
    public class PlayerListData : IListItemDataProvider
    {
        // Add your data fields here
    }
}
```

**PlayerListView.cs**
```csharp
using Framewerk.UI.List;

namespace MyGame.UI
{
    public class PlayerListView : ListView
    {
        // Add additional view references here
    }
}
```

**PlayerListMediator.cs**
```csharp
using Framewerk.UI.List;

namespace MyGame.UI
{
    public class PlayerListMediator : ListMediator<PlayerListView, PlayerListData>
    {
        public override void OnRegister()
        {
            base.OnRegister();
        }
    }
}
```

**PlayerListItemView.cs**
```csharp
using Framewerk.UI.List;
using TMPro;

namespace MyGame.UI
{
    public class PlayerListItemView : ListItemView
    {
        public TextMeshProUGUI label;
    }
}
```

**PlayerListItemMediator.cs**
```csharp
using Framewerk.UI.List;

namespace MyGame.UI
{
    public class PlayerListItemMediator : ListItemMediator<PlayerListItemView, PlayerListData>
    {
        public override void OnRegister()
        {
            base.OnRegister();
        }
    }
}
```

---

## 4.3 Populating Lists

### Setting Up Data

First, define your data class:

```csharp
public class PlayerListData : IListItemDataProvider
{
    public string PlayerId;
    public string PlayerName;
    public int Score;
    public bool IsOnline;
    
    public PlayerListData(string id, string name, int score, bool online)
    {
        PlayerId = id;
        PlayerName = name;
        Score = score;
        IsOnline = online;
    }
}
```

### Calling SetData()

Populate the list from your mediator or command:

```csharp
public class PlayerListMediator : ListMediator<PlayerListView, PlayerListData>
{
    [Inject] public IPlayerService PlayerService { get; set; }
    
    public override void OnRegister()
    {
        base.OnRegister();
        
        // Fetch and display players
        var players = PlayerService.GetAllPlayers();
        var dataList = players.Select(p => new PlayerListData(
            p.Id, 
            p.Name, 
            p.Score, 
            p.IsOnline
        )).ToList();
        
        SetData(dataList);
    }
}
```

### Data-to-View Binding

Override `SetData` in your item mediator to bind data to UI:

```csharp
public class PlayerListItemMediator : ListItemMediator<PlayerListItemView, PlayerListData>
{
    public override void SetData(PlayerListData dataProvider, int index)
    {
        base.SetData(dataProvider, index);
        
        // Bind data to view elements
        View.label.text = dataProvider.PlayerName;
        View.scoreText.text = $"Score: {dataProvider.Score}";
        View.onlineIcon.SetActive(dataProvider.IsOnline);
    }
}
```

### Handling Item Clicks

Override `OnClick()` for additional click behavior:

```csharp
public class PlayerListItemMediator : ListItemMediator<PlayerListItemView, PlayerListData>
{
    public override void SetData(PlayerListData dataProvider, int index)
    {
        base.SetData(dataProvider, index);
        View.label.text = dataProvider.PlayerName;
    }
    
    protected override void OnClick()
    {
        // Custom click logic (selection is handled by base class)
        Debug.Log($"Clicked on player: {DataProvider.PlayerName}");
    }
}
```

### Handling Selection in Parent

Override hooks in the list mediator:

```csharp
public class PlayerListMediator : ListMediator<PlayerListView, PlayerListData>
{
    protected override void ListItemSelected(int index, PlayerListData dataProvider)
    {
        Debug.Log($"Selected: {dataProvider.PlayerName}");
        // Open player details, enable action buttons, etc.
    }
    
    protected override void ListItemUnselected(int index, PlayerListData dataProvider)
    {
        Debug.Log($"Unselected: {dataProvider.PlayerName}");
    }
    
    protected override void ListItemClicked(int index, PlayerListData dataProvider)
    {
        // Called on every click, regardless of selection state change
    }
    
    protected override void SelectionUpdated(int index)
    {
        // Called whenever selection changes (select or unselect)
    }
}
```

---

## 4.4 Selection & Multiselect

### Selection Properties

```csharp
public class ListBaseMediator<TView, TData>
{
    public List<int> SelectedItemIndexes { get; }  // Currently selected indices
    public bool Multiselect { get; set; }          // Allow multiple selection
    public bool Unselectable { get; set; }         // Allow clicking to unselect
}
```

### Programmatic Selection

```csharp
// Select item at index 2
SelectItemAt(2);

// Unselect item at index 2
UnselectItemAt(2);

// Clear all selection
UnselectAll();

// Get selected data
var selectedPlayer = GetSelectedItem();           // Single selection
var selectedPlayers = GetSelectedItems();         // Multiple selection
int? selectedIndex = GetSelectedIndex();          // Selected index (null if none)
```

### Enabling Multiselect

```csharp
public class PlayerListMediator : ListMediator<PlayerListView, PlayerListData>
{
    public override void OnRegister()
    {
        base.OnRegister();
        
        Multiselect = true;   // Allow selecting multiple items
        Unselectable = true;  // Allow toggling selection off by clicking again
    }
    
    protected override void SelectionUpdated(int index)
    {
        // Update UI based on selection count
        int count = SelectedItemIndexes.Count;
        View.deleteButton.interactable = count > 0;
        View.selectionLabel.text = $"{count} selected";
    }
}
```

### Visual Selection State

Override `SetSelected` in item mediator for custom visuals:

```csharp
public class PlayerListItemMediator : ListItemMediator<PlayerListItemView, PlayerListData>
{
    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);
        
        // Custom selection visuals
        View.background.color = selected ? Color.yellow : Color.white;
        View.checkmark.SetActive(selected);
    }
}
```

---

## 4.5 Dynamic Lists

### Adding Items

Currently, to add items, re-call `SetData()` with the updated list:

```csharp
public void AddPlayer(PlayerListData newPlayer)
{
    DataProviders.Add(newPlayer);
    SetData(DataProviders);
}
```

### Removing Items

Use `RemoveItemAt()` for efficient single-item removal:

```csharp
// Remove by index
RemoveItemAt(3);

// Remove by data reference
public void RemovePlayer(PlayerListData player)
{
    int index = FindItemIndex(player);
    if (index >= 0)
    {
        RemoveItemAt(index);
    }
}
```

### Finding Items

```csharp
// Find by data reference
int index = FindItemIndex(someData);

// Get data at index
var data = GetDataproviderAt(index);

// Custom search
protected IListItemMediator<TData> FindItemMediatorByCustom<TSearched>(
    Func<TData, TSearched, bool> comparator, 
    TSearched searchedItem);

// Example: Find by player ID
var mediator = FindItemMediatorByCustom(
    (data, id) => data.PlayerId == id, 
    "player_123"
);
```

### Object Pooling

Lists automatically pool items for performance:

```csharp
// When SetData() is called:
// 1. Existing items are reused and shown
// 2. Extra items are hidden (not destroyed)
// 3. New items are created only when needed
// 4. CreatedMediatorsCount tracks total spawned items
```

This means:
- No GC pressure from repeatedly creating/destroying items
- Hidden items retain their GameObjects
- `SetActive(false)` on hidden items

### EmptyContent

Display a message when the list is empty:

1. **In Prefab**: Add a child GameObject with your "No items" UI
2. **In ListView**: Assign it to `EmptyContent` field

```csharp
public class PlayerListView : ListView
{
    // EmptyContent is inherited from ListView
    // Just assign in Inspector
}
```

The base class handles visibility automatically:

```csharp
// In ListBaseMediator.SetData():
if (View.EmptyContent != null)
    View.EmptyContent.SetActive(dataProviders.Count == 0);
```

---

## Complete Example: Friend List

### Data Class

```csharp
public class FriendListData : IListItemDataProvider
{
    public string UserId;
    public string DisplayName;
    public string AvatarUrl;
    public bool IsOnline;
    public DateTime LastSeen;
    
    public FriendListData(User user)
    {
        UserId = user.Id;
        DisplayName = user.Name;
        AvatarUrl = user.Avatar;
        IsOnline = user.Status == UserStatus.Online;
        LastSeen = user.LastActivity;
    }
}
```

### Item View

```csharp
public class FriendListItemView : ListItemView
{
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI statusLabel;
    public Image avatarImage;
    public Image onlineIndicator;
    public Image selectionBackground;
}
```

### Item Mediator

```csharp
public class FriendListItemMediator : ListItemMediator<FriendListItemView, FriendListData>
{
    [Inject] public IAvatarLoader AvatarLoader { get; set; }
    
    public override void SetData(FriendListData data, int index)
    {
        base.SetData(data, index);
        
        View.nameLabel.text = data.DisplayName;
        View.onlineIndicator.color = data.IsOnline ? Color.green : Color.gray;
        View.statusLabel.text = data.IsOnline 
            ? "Online" 
            : $"Last seen {FormatTime(data.LastSeen)}";
        
        AvatarLoader.LoadAsync(data.AvatarUrl, View.avatarImage);
    }
    
    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);
        View.selectionBackground.enabled = selected;
    }
    
    protected override void OnClick()
    {
        // Play click sound
        AudioManager.PlaySound("ui_click");
    }
    
    private string FormatTime(DateTime time)
    {
        var diff = DateTime.Now - time;
        if (diff.TotalMinutes < 1) return "just now";
        if (diff.TotalHours < 1) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalDays < 1) return $"{(int)diff.TotalHours}h ago";
        return $"{(int)diff.TotalDays}d ago";
    }
}
```

### List Mediator

```csharp
public class FriendListMediator : ListMediator<FriendListView, FriendListData>
{
    [Inject] public IFriendService FriendService { get; set; }
    [Inject] public OpenChatSignal OpenChatSignal { get; set; }
    
    public override void OnRegister()
    {
        base.OnRegister();
        
        Multiselect = false;
        Unselectable = true;
        
        LoadFriends();
    }
    
    private async void LoadFriends()
    {
        var friends = await FriendService.GetFriendsAsync();
        var data = friends
            .OrderByDescending(f => f.IsOnline)
            .ThenBy(f => f.Name)
            .Select(f => new FriendListData(f))
            .ToList();
        
        SetData(data);
    }
    
    protected override void ListItemSelected(int index, FriendListData data)
    {
        // Open chat with selected friend
        OpenChatSignal.Dispatch(data.UserId);
    }
}
```

---

## Summary

| Concept | Key Points |
|---------|------------|
| **Architecture** | ListView + ListMediator manage container; ListItemView + ListItemMediator handle items |
| **Data Binding** | Implement IListItemDataProvider; override SetData() in item mediator |
| **Wizard** | Generates all 5 files + 2 prefabs; auto-registers mediation binding |
| **Selection** | SelectItemAt(), UnselectAll(); Multiselect/Unselectable flags |
| **Pooling** | Automatic — items hidden, not destroyed; SetActive() controls visibility |
| **EmptyContent** | Assign in ListView; auto-shown when list is empty |

**Next**: [Chapter 5: Events & Signals](05-events-signals.md) — Inter-component communication
