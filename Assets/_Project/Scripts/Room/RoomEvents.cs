using System;

namespace EchoChamber
{
    /// <summary>Input events from the room, consumed by the turn flow and the views.</summary>
    public static class RoomEvents
    {
        public static event Action<ClickAction, RoomClickable> Clicked;
        public static event Action<ViewId> ViewRequested;
        public static event Action<ViewId> ViewChanged;

        public static void RaiseClicked(ClickAction a, RoomClickable c) => Clicked?.Invoke(a, c);
        public static void RaiseViewRequested(ViewId v) => ViewRequested?.Invoke(v);
        public static void RaiseViewChanged(ViewId v) => ViewChanged?.Invoke(v);
    }
}
