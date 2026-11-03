using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>
    /// Runtime state of one neighbour. The hidden values (Stress, Pert, ...) must never be shown to the
    /// player; only behaviour and Reaction are visible.
    /// </summary>
    public class Neighbour
    {
        public NeighbourData Data;
        public int Id => Data.id;
        public string Name => Data.name;
        public Trait Trait => Data.trait;
        public PlaceId Home => Data.home;

        // Logical position in the 1000x640 world
        public float X;
        public float Y;
        public readonly List<PathPoint> Path = new List<PathPoint>();
        /// <summary>Current place, or null while walking.</summary>
        public PlaceId? Place;
        public PlaceId Target;
        public ActionKind Action = ActionKind.Casa;
        public float Dwell;
        public bool Crossing;
        public bool Waiting;

        // Hidden states
        public float Stress;
        public float Pert;
        public float Compra;
        public float Desc;
        public float Apat;
        public float LivAff;
        public float Phone;

        // Visible feedback
        public Reaction Reaction;
        public PostCategory IconCategory;
        public float IconTime;
        public int PostsSeen;

        // Discovery
        public readonly Dictionary<int, float> Links = new Dictionary<int, float>();
        public readonly List<ObservationNote> Notes = new List<ObservationNote>();
        public bool TraitKnown;

        public bool IsAnxious => Trait == Trait.Anxious;
        public bool IsAddict => Trait == Trait.Addict;
        public bool IsMoving => Path.Count > 0;
    }
}
