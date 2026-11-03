using System;
using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>
    /// The four crisis rings (grocery, council, bookshop, square) and the events they trigger
    /// (PrototypeSpec section 5). Also owns the police patrol and the "ads nervous" flag.
    /// </summary>
    public class CrisisRings
    {
        private readonly TuningData t;

        public float Mercearia;
        public float Junta;
        public float Livraria;
        public float Praca;

        public bool MerceariaOpen = true;
        public bool LivrariaOpen = true;
        public bool PoliceOnStreet;
        public float PoliceX;
        public float Festa;
        public float Riot;
        public bool AdsNervous;

        public CrisisRings(TuningData tuning)
        {
            t = tuning;
            Livraria = t.livrariaStartDay1;
        }

        public bool FestaActive => Festa > 0f;
        public bool RiotActive => Riot > 0f;

        public void ResetForDay(int day)
        {
            Mercearia = 0f;
            Junta = 0f;
            Praca = 0f;
            Livraria = day == 0 ? t.livrariaStartDay1 : t.livrariaStartLater;
        }

        /// <summary>Advances rings and returns the events that fired this step.</summary>
        public void Step(float dt, IList<Neighbour> all, int day, float tension, List<SimEvent> events, int turn)
        {
            float stress = Metrics.AvgStress(all);
            int protesters = 0, together = 0;
            foreach (var n in all)
            {
                if (n.Place == PlaceId.Junta) protesters++;
                if (n.Place == PlaceId.Praca && n.Pert > t.pracaPertThreshold) together++;
            }

            if (MerceariaOpen)
            {
                Mercearia = SimMath.Clamp01(Mercearia + (stress - t.merceariaThreshold) * t.merceariaRate * dt);
                if (Mercearia >= 1f)
                {
                    MerceariaOpen = false;
                    Mercearia = 0f;
                    events.Add(new SimEvent { Kind = SimEventKind.MerceariaClosed, Day = day, Turn = turn });
                }
            }

            Junta = SimMath.Clamp01(Junta + (protesters - t.juntaThreshold) * t.juntaRate * dt);
            if (Junta >= 1f)
            {
                Junta = 0f;
                Riot = t.riotDuration;
                foreach (var n in all) n.Stress = SimMath.Clamp01(n.Stress + t.riotStress);
                events.Add(new SimEvent { Kind = SimEventKind.Riot, Day = day, Turn = turn });
            }

            if (day >= 1 && LivrariaOpen)
            {
                Livraria = SimMath.Clamp01(Livraria - t.livrariaDecay * dt);
                if (Livraria <= 0f)
                {
                    LivrariaOpen = false;
                    events.Add(new SimEvent { Kind = SimEventKind.LivrariaClosed, Day = day, Turn = turn });
                }
            }

            Praca = SimMath.Clamp01(Praca + (together * t.pracaTogetherRate - t.pracaDecay) * dt);
            if (Praca >= 1f)
            {
                Praca = 0f;
                Festa = t.festaDuration;
                foreach (var n in all)
                {
                    n.Pert = SimMath.Clamp01(n.Pert + t.festaPert);
                    n.Stress = SimMath.Clamp01(n.Stress + t.festaStress);
                }
                events.Add(new SimEvent { Kind = SimEventKind.Festa, Good = true, Day = day, Turn = turn });
            }

            if (Festa > 0f) Festa -= dt;
            if (Riot > 0f) Riot -= dt;

            if (!PoliceOnStreet && tension > t.policeStartTension)
            {
                PoliceOnStreet = true;
                PoliceX = -t.policeRange;
                events.Add(new SimEvent { Kind = SimEventKind.PoliceStarted, Day = day, Turn = turn });
            }
            if (PoliceOnStreet)
            {
                PoliceX += t.policeSpeed * dt;
                if (PoliceX > t.policeRange)
                {
                    if (tension < t.policeLeaveTension) PoliceOnStreet = false;
                    else PoliceX = -t.policeRange;
                }
            }

            bool nervous = tension > t.nervousTension;
            if (nervous && !AdsNervous)
                events.Add(new SimEvent { Kind = SimEventKind.AdsNervous, Day = day, Turn = turn });
            AdsNervous = nervous;
        }
    }
}
