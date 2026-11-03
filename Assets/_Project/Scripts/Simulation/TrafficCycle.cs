namespace EchoChamber
{
    public enum CarLight
    {
        Green,
        Yellow,
        Red
    }

    public enum PedLight
    {
        Stop,
        Walk,
        Blink
    }

    /// <summary>Traffic light cycle shared by cars and pedestrians (16 s by default).</summary>
    public class TrafficCycle
    {
        private readonly TuningData tuning;
        public float Time;

        public TrafficCycle(TuningData tuning)
        {
            this.tuning = tuning;
        }

        public void Advance(float dt)
        {
            Time += dt;
        }

        private float Phase => Time % tuning.trafficCycle;

        public CarLight Car
        {
            get
            {
                float t = Phase;
                return t < tuning.carGreenEnd ? CarLight.Green : (t < tuning.carYellowEnd ? CarLight.Yellow : CarLight.Red);
            }
        }

        public bool PedGreen
        {
            get
            {
                float t = Phase;
                return t >= tuning.pedWalkStart && t < tuning.pedWalkEnd;
            }
        }

        public PedLight Ped
        {
            get
            {
                float t = Phase;
                if (t >= tuning.pedWalkStart && t < tuning.pedBlinkEnd)
                    return t < tuning.pedWalkEnd ? PedLight.Walk : PedLight.Blink;
                return PedLight.Stop;
            }
        }
    }
}
