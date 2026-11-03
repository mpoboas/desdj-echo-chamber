using System;

namespace EchoChamber
{
    /// <summary>
    /// Every number of the simulation. Defaults are the prototype values
    /// (docs/PrototypeSpec.md); the TuningConfig ScriptableObject wraps one instance.
    /// </summary>
    [Serializable]
    public class TuningData
    {
        // ---- World (logical 2D space of the prototype) ----
        public float worldWidth = 1000f;
        public float worldHeight = 640f;
        public float streetY = 318f;
        public float walkY = 240f;
        public float southY = 398f;
        public float crosswalkLeftX = 178f;
        public float crosswalkRightX = 822f;
        public float zoneMarginWithZone = 4f;
        public float zoneMarginNoZone = 18f;

        // ---- Movement ----
        public float walkSpeed = 62f;
        public float fastStressThreshold = 0.7f;
        public float fastSpeedMultiplier = 1.4f;
        public float decideTimeScale = 0.4f;

        // ---- Initial neighbour states ----
        public FloatRange initStress = new FloatRange(0.16f, 0.30f);
        public FloatRange initPert = new FloatRange(0.42f, 0.62f);
        public FloatRange initCompra = new FloatRange(0.20f, 0.60f);
        public FloatRange initDesc = new FloatRange(0.08f, 0.22f);
        public FloatRange initApat = new FloatRange(0.12f, 0.28f);
        public FloatRange initPhone = new FloatRange(0.40f, 0.75f);
        public FloatRange initPhoneAddict = new FloatRange(0.85f, 0.98f);
        public FloatRange initDwell = new FloatRange(0f, 6f);
        public float initLivAff = 0.10f;
        public float initLivAffReader = 0.50f;

        // ---- Rest values and return rates (per second, fraction of distance) ----
        public float restStress = 0.22f;
        public float restDesc = 0.13f;
        public float restApat = 0.20f;
        public float stressReturn = 0.008f;
        public float descReturn = 0.004f;
        public float apatReturn = 0.004f;
        public float compraGain = 0.008f;
        public float livAffDecay = 0.004f;
        public float livAffMin = 0.05f;
        public float livAffMinReader = 0.35f;

        // ---- Continuous place effects (per second) ----
        public float policeStress = 0.007f;
        public float cafePert = 0.012f;
        public float cafeStress = 0.007f;
        public float pracaStress = 0.012f;
        public float pracaPert = 0.005f;
        public float homeStress = 0.010f;
        public float ecraApat = 0.007f;
        public float ecraPert = 0.003f;
        public float juntaDesc = 0.004f;

        // ---- Dwell times ----
        public FloatRange dwellHome = new FloatRange(7f, 12f);
        public FloatRange dwellOther = new FloatRange(5f, 9f);
        public FloatRange dwellEcra = new FloatRange(5f, 8f);
        public FloatRange dwellAfterPost = new FloatRange(0.4f, 1.4f);

        // ---- Arrival effects ----
        public float mercCompraDrop = 0.45f;
        public float livVisitRing = 0.09f;
        public float livVisitPert = 0.04f;

        // ---- Utility AI ----
        public float nightBonus = 0.55f;
        public float currentActionPenalty = 0.18f;
        public float utilityNoise = 0.14f;
        public float utilityClosed = -1f;

        public float casaBase = 0.22f;
        public float casaStress = 0.9f;
        public float casaAnxiousStress = 0.3f;

        public float cafeBase = 0.20f;
        public float cafePertW = 0.7f;
        public float cafeCalm = 0.3f;
        public float cafeApat = 0.35f;

        public float merceariaBase = 0.05f;
        public float merceariaCompra = 1.1f;

        public float livrariaBase = 0.02f;
        public float livrariaPert = 0.35f;
        public float livrariaAff = 0.95f;
        public float livrariaApat = 0.4f;

        public float juntaBase = -0.45f;
        public float juntaDescW = 1.35f;
        public float juntaStress = 0.35f;
        public float juntaApat = 0.3f;

        public float pracaBase = 0.12f;
        public float pracaCalm = 0.45f;
        public float pracaPertW = 0.25f;
        public float pracaPolice = 0.15f;
        public float pracaNight = 0.6f;
        public float pracaFesta = 0.4f;

        public float ecraBase = -0.12f;
        public float ecraApatW = 1.15f;
        public float ecraAddict = 0.15f;

        // ---- Social contagion ----
        public float contagionRadius = 110f;
        public float contagionRate = 0.035f;
        public float relationRadius = 130f;
        public float relationTime = 5f;
        public int relationCap = 8;

        // ---- Post delivery ----
        public float ampMultiplier = 2.2f;
        public float hideMultiplier = 0.1f;
        public float normalMultiplier = 1f;
        public float viewBase = 0.5f;
        public float viewWeightOffset = 0.4f;
        public float viewWeightScale = 1.2f;
        public float viewEcraBoost = 1.5f;
        public float viewMax = 0.97f;
        public float effectK = 0.07f;
        public float descToStress = 0.4f;
        public float descToPert = 0.4f;
        public float apatToPert = 0.3f;
        public float compraBoost = 1.4f;
        public float empToApat = 0.3f;
        public float livAffPost = 0.15f;
        public float livAffReaderBonus = 0.2f;
        public float readerLivEmpMultiplier = 1.5f;

        // ---- Trait multipliers ----
        public float anxiousMedo = 1.5f;
        public float scepticMedo = 0.55f;
        public float scepticDesc = 0.5f;
        public float solidaryEmp = 1.4f;
        public float consumeristCons = 1.6f;
        public float addictApat = 1.4f;

        // ---- Visible reaction ----
        public float reactionMin = 1.4f;
        public float reactionStrongFactor = 1.2f;
        public float reactionDuration = 9f;
        public float iconDuration = 1.4f;
        public float raivaFromDesc = 1.1f;

        // ---- Revenue ----
        public float revenuePerView = 1.6f;
        public float pubViewBonus = 2f;
        public float pubWeightOffset = 0.5f;
        public float nervousRevenueMultiplier = 0.5f;

        // ---- Turn structure ----
        public float simLength = 26f;
        public float firstDeliveryAt = 1.2f;
        public float deliveryInterval = 1.7f;
        public float decideBase = 10f;
        public float decidePerPost = 12f;
        public float histInterval = 1.5f;
        public int daysCount = 3;
        public int turnsPerDay = 3;

        // ---- Mixer ----
        public float mixerDefault = 50f;

        // ---- Metrics ----
        public float tensionStress = 0.65f;
        public float tensionDesc = 0.35f;
        public float tensionRiot = 0.12f;
        public float tensionPolice = 0.04f;
        public float cohesionScale = 1.25f;
        public float cohesionDescPenalty = 0.6f;
        public float cohesionApatPenalty = 0.2f;
        public float initialCohesion = 0.5f;
        public float initialTension = 0.2f;
        public float policeStartTension = 0.62f;
        public float policeLeaveTension = 0.5f;
        public float nervousTension = 0.70f;

        // ---- Crisis rings ----
        public float merceariaThreshold = 0.42f;
        public float merceariaRate = 0.11f;
        public float juntaThreshold = 1.5f;
        public float juntaRate = 0.03f;
        public float livrariaDecay = 0.014f;
        public float livrariaStartDay1 = 0.75f;
        public float livrariaStartLater = 0.70f;
        public float pracaTogetherRate = 0.022f;
        public float pracaDecay = 0.012f;
        public float pracaPertThreshold = 0.5f;
        public float riotDuration = 8f;
        public float riotStress = 0.10f;
        public float festaDuration = 10f;
        public float festaPert = 0.08f;
        public float festaStress = -0.08f;
        public float policeSpeed = 2.2f;
        public float policeRange = 8f;

        // ---- End of day ----
        public float nightStressPull = 0.30f;
        public float nightDescPull = 0.15f;
        public float nightApatPull = 0.15f;
        public float merceariaReopenStress = 0.45f;

        // ---- Endings ----
        public float endingSleepApat = 0.55f;
        public float endingLockedTension = 0.65f;
        public float endingAliveCohesion = 0.55f;
        public float endingBalanceCohesion = 0.50f;

        // ---- Random post picking ----
        public float pickSensBase = 0.20f;
        public float pickSensStress = 0.50f;
        public float pickSensDesc = 0.35f;
        public float pickComBase = 0.18f;
        public float pickComPert = 0.30f;
        public float pickEntBase = 0.18f;
        public float pickEntApat = 0.45f;
        public float pickPub = 0.16f;
        public float pickLoc = 0.12f;
        public int pickRecentMemory = 9;

        // ---- Traffic (16 s cycle) ----
        public float trafficCycle = 16f;
        public float carGreenEnd = 8f;
        public float carYellowEnd = 10f;
        public float pedWalkStart = 10.4f;
        public float pedWalkEnd = 13.6f;
        public float pedBlinkEnd = 15.2f;
        public float trafficDecideScale = 0.4f;
    }
}
