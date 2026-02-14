# Future Plans / Nice-to-Have

Non-critical features that would be nice to add in the future.

---

## FSM Navigation History / Back Support (MISS-005)

**Status:** Nice-to-have, not critical

**What it would do:**
- Track state transition history in the FSM
- Allow navigation back to previous states via `GoBack()` or similar API
- Useful for wizard-style flows, nested menus, or undo-like patterns

**Example API:**
```csharp
stateMachine.GoBack();  // Return to previous state
stateMachine.CanGoBack; // Check if history exists
stateMachine.ClearHistory(); // Reset navigation stack
```

**Notes:**
- Would need to decide on history depth limits
- Consider whether all transitions should be tracked or only certain ones
- May need a "no-history" transition option for states that shouldn't be revisited
