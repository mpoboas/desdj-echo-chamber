using UnityEngine;

namespace EchoChamber
{
    /// <summary>Owns the simulation and ticks it every frame. Presentation components read from Sim.</summary>
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameContent content;
        public TuningConfig tuning;
        [Tooltip("0 = random seed each run")]
        public int seed;
        [Range(0.1f, 10f)] public float timeScale = 1f;

        public GameSimulation Sim { get; private set; }

        public ContentSet Content => content.content;
        public TuningData Tuning => tuning.data;

        public static event System.Action SimulationCreated;

        private void Awake()
        {
            Instance = this;
            CreateSimulation();
        }

        public void CreateSimulation()
        {
            int s = seed != 0 ? seed : System.Environment.TickCount;
            Sim = new GameSimulation(content.content, tuning.data, s);
            SimulationCreated?.Invoke();
        }

        private void Update()
        {
            if (Sim == null) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f) * timeScale;
            // At high time scale run several fixed sub-steps so movement stays stable.
            int steps = Mathf.Max(1, Mathf.CeilToInt(dt / 0.05f));
            float sub = dt / steps;
            for (int i = 0; i < steps; i++) Sim.Tick(sub);
        }
    }
}
