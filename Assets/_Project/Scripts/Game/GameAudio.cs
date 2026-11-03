using UnityEngine;

namespace EchoChamber
{
    /// <summary>Sound effects entry points. Bodies are filled in at M7 (procedural audio).</summary>
    public static class GameAudio
    {
        public static bool Muted { get; set; }

        public static void Dots() => Play("dots");
        public static void Tear() => Play("tear");
        public static void Stamp() => Play("stamp");
        public static void Lever() => Play("lever");
        public static void Tube() => Play("tube");
        public static void Ring() => Play("ring");
        public static void Good() => Play("good");
        public static void Alarm() => Play("alarm");
        public static void Note() => Play("note");
        public static void Tick() => Play("tick");
        public static void Deliver() => Play("deliver");
        public static void Click() => Play("click");

        public static System.Action<string> Hook;

        private static void Play(string id)
        {
            if (Muted) return;
            Hook?.Invoke(id);
        }
    }
}
