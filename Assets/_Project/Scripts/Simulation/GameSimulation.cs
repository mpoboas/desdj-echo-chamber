using System;
using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>
    /// Orchestrates the whole game logic without any scene: days, turns, neighbours, posts, metrics,
    /// rings, discovery and endings. Advance it with Tick(dt).
    /// </summary>
    public class GameSimulation
    {
        public readonly TuningData Tuning;
        public readonly ContentSet Content;
        public readonly SimRandom Rng;

        public readonly List<Neighbour> Neighbours = new List<Neighbour>();
        public readonly UtilityBrain Brain;
        public readonly ContagionSystem Contagion;
        public readonly Metrics Metrics;
        public readonly CrisisRings Rings;
        public readonly DiscoveryModel Discovery;
        public readonly TrafficCycle Traffic;
        public readonly WorldRouter Router;
        public readonly PostPicker Picker;
        public readonly PostDelivery Deliverer;

        // Flow state
        public GamePhase Phase = GamePhase.Intro;
        public int Day;
        public int Turn;
        public int RevenueDay;
        public int RevenueTotal;
        public readonly int[] RevenueByDay;
        public readonly float[] Weights = new float[5];
        public readonly List<PostData> TurnPosts = new List<PostData>();
        public readonly List<Delivery> Deliveries = new List<Delivery>();
        public TurnLog Log;
        public float SimTime;
        public float DecideTime;
        public float DecideMax;
        /// <summary>The decide clock only runs once the first slip exists (set by the presentation).</summary>
        public bool DecideClockRunning;
        /// <summary>
        /// Boss pressure: 0 when the last revenue goal was met, otherwise +1 per failed day in a row (capped).
        /// Each level adds imposed posts to every turn of the next day.
        /// </summary>
        public int Pressure;
        /// <summary>Posts of the current turn that the boss imposed (cannot be hidden).</summary>
        public readonly HashSet<PostData> ImposedPosts = new HashSet<PostData>();
        /// <summary>OCULTAR stamps left this turn.</summary>
        public int InkLeft;
        private readonly Dictionary<PostData, string> authors = new Dictionary<PostData, string>();
        public readonly List<List<DayGoal>> DayResults = new List<List<DayGoal>>();
        public readonly List<KeyValuePair<float, float>> History = new List<KeyValuePair<float, float>>();
        public EndingId? FinalEnding;
        /// <summary>True when the neighbourhood was balanced but the player had not understood it enough.</summary>
        public bool BalanceMissedForUnderstanding;

        public readonly Dictionary<PostCategory, int> CategorySeen = new Dictionary<PostCategory, int>();

        public event Action<SimEvent> EventRaised;

        private readonly List<SimEvent> eventBuffer = new List<SimEvent>();
        private float histTimer;

        public GameSimulation(ContentSet content, TuningData tuning, int seed) : this(content, tuning, new SimRandom(seed))
        {
        }

        public GameSimulation(ContentSet content, TuningData tuning, SimRandom rng)
        {
            Content = content;
            Tuning = tuning;
            Rng = rng;
            Brain = new UtilityBrain(tuning, rng);
            Contagion = new ContagionSystem(tuning);
            Metrics = new Metrics(tuning);
            Rings = new CrisisRings(tuning);
            Discovery = new DiscoveryModel(tuning);
            Traffic = new TrafficCycle(tuning);
            Router = new WorldRouter(tuning, content, rng);
            Picker = new PostPicker(tuning, content, rng);
            Deliverer = new PostDelivery(tuning, rng);
            RevenueByDay = new int[tuning.daysCount];
            for (int i = 0; i < Weights.Length; i++) Weights[i] = tuning.mixerDefault;
            SpawnNeighbours();
        }

        public int DaysCount => Tuning.daysCount;
        public DayData CurrentDay => Content.days[Day];
        public bool IsLastTurnOfDay => Turn == Tuning.turnsPerDay - 1;
        public bool IsNightTurn => Turn == 2;
        public bool IsFirstTurnEver => Day == 0 && Turn == 0;

        private void SpawnNeighbours()
        {
            foreach (NeighbourData d in Content.neighbours)
            {
                var n = new Neighbour { Data = d };
                n.Stress = Rng.Range(Tuning.initStress);
                n.Pert = Rng.Range(Tuning.initPert);
                n.Compra = Rng.Range(Tuning.initCompra);
                n.Desc = Rng.Range(Tuning.initDesc);
                n.Apat = Rng.Range(Tuning.initApat);
                n.LivAff = d.trait == Trait.Reader ? Tuning.initLivAffReader : Tuning.initLivAff;
                n.Phone = d.trait == Trait.Addict ? Rng.Range(Tuning.initPhoneAddict) : Rng.Range(Tuning.initPhone);
                n.Dwell = Rng.Range(Tuning.initDwell);
                n.Place = d.home;
                n.Target = d.home;
                PathPoint p = Router.ZonePoint(d.home);
                n.X = p.X;
                n.Y = p.Y;
                Neighbours.Add(n);
            }
        }

        // ------------------------------------------------------------------ flow

        public BrainContext MakeBrainContext()
        {
            return new BrainContext
            {
                IsNight = IsNightTurn,
                MerceariaOpen = Rings.MerceariaOpen,
                LivrariaOpen = Rings.LivrariaOpen,
                PoliceOnStreet = Rings.PoliceOnStreet,
                FestaActive = Rings.FestaActive
            };
        }

        /// <summary>Builds the list of posts for the current turn from the day script.</summary>
        public List<PostData> BuildTurnPosts()
        {
            var posts = new List<PostData>();
            TurnData spec = CurrentDay.turns[Turn];
            foreach (TurnEntry it in spec.entries)
            {
                switch (it.kind)
                {
                    case TurnEntryKind.Scripted:
                        posts.Add(Content.FindScripted(it.postId));
                        break;
                    case TurnEntryKind.MerceariaAd:
                        posts.Add(Content.pool.Find(p => p.author == "Mercearia Central"));
                        break;
                    case TurnEntryKind.Random:
                        for (int i = 0; i < it.count; i++) posts.Add(Picker.Pick(Neighbours));
                        break;
                }
            }
            ImposedPosts.Clear();
            int imposed = Pressure * Tuning.imposedPerLevel;
            for (int i = 0; i < imposed; i++)
            {
                PostData p = Picker.PickImposed(posts);
                posts.Add(p);
                ImposedPosts.Add(p);
            }
            return posts;
        }

        public bool IsImposed(PostData post) => ImposedPosts.Contains(post);

        /// <summary>Author shown on the slip. Posts without an author get a random neighbour, fixed for the turn.</summary>
        public string AuthorOf(PostData post)
        {
            if (!string.IsNullOrEmpty(post.author)) return post.author;
            return authors.TryGetValue(post, out string a) ? a : "";
        }

        /// <summary>True if the player may still stamp OCULTAR on this post.</summary>
        public bool CanHide(PostData post) => InkLeft > 0 && !IsImposed(post);

        public void SpendInk()
        {
            if (InkLeft > 0) InkLeft--;
        }

        public void StartTurn()
        {
            Phase = GamePhase.Decide;
            Deliveries.Clear();
            TurnPosts.Clear();
            TurnPosts.AddRange(BuildTurnPosts());
            InkLeft = Tuning.hidesPerTurn;
            authors.Clear();
            foreach (PostData post in TurnPosts)
                if (string.IsNullOrEmpty(post.author) && !authors.ContainsKey(post))
                    authors[post] = Neighbours[Rng.Index(Neighbours.Count)].Name;
            DecideTime = 0f;
            DecideClockRunning = false;
            DecideMax = IsFirstTurnEver ? float.PositiveInfinity : Tuning.decideBase + TurnPosts.Count * Tuning.decidePerPost;
            Log = new TurnLog
            {
                StartCohesion = Metrics.Cohesion,
                StartTension = Metrics.Tension,
                StartRevenue = RevenueDay
            };
        }

        /// <summary>Starts the first turn of the game.</summary>
        public void StartGame()
        {
            Day = 0;
            Turn = 0;
            StartTurn();
        }

        public int EstimateRevenue(PostData post)
        {
            return Deliverer.EstimateRevenue(post, Weights[(int)post.category] / 100f, Neighbours, Rings.AdsNervous);
        }

        /// <summary>
        /// Locks in the decisions and starts the simulate phase.
        /// Entries not decided by the player must come in with Mode = Auto.
        /// </summary>
        public void Publish(IList<KeyValuePair<PostData, DecisionMode>> decisions)
        {
            Log.Posts.Clear();
            Deliveries.Clear();
            int hidden = 0;
            for (int i = 0; i < decisions.Count; i++)
            {
                PostData post = decisions[i].Key;
                bool imposed = IsImposed(post);
                DecisionMode mode = decisions[i].Value;
                // The boss does not allow hiding imposed posts.
                if (imposed && mode == DecisionMode.Hide) mode = DecisionMode.Normal;
                // The stamp runs out of ink: hides beyond the turn budget go out as normal.
                if (mode == DecisionMode.Hide && ++hidden > Tuning.hidesPerTurn) mode = DecisionMode.Normal;
                var entry = new TurnPostEntry { Post = post, Mode = mode, Imposed = imposed, Author = AuthorOf(post) };
                Log.Posts.Add(entry);
                Deliveries.Add(new Delivery { Entry = entry, At = Tuning.firstDeliveryAt + i * Tuning.deliveryInterval });
            }
            Phase = GamePhase.Simulate;
            SimTime = 0f;
        }

        public void Tick(float dt)
        {
            if (Phase == GamePhase.Decide)
            {
                if (DecideClockRunning) DecideTime += dt;
                StepNeighbours(dt * Tuning.decideTimeScale, false);
            }
            else if (Phase == GamePhase.Simulate)
            {
                StepNeighbours(dt, true);
                StepSimulation(dt);
            }

            Traffic.Advance(dt * (Phase == GamePhase.Simulate ? 1f : Tuning.trafficDecideScale));
            Metrics.Update(Neighbours, Rings.RiotActive, Rings.PoliceOnStreet);

            if (Phase == GamePhase.Simulate || Phase == GamePhase.Decide)
            {
                histTimer += dt;
                if (histTimer >= Tuning.histInterval)
                {
                    histTimer = 0f;
                    History.Add(new KeyValuePair<float, float>(Metrics.Cohesion, Metrics.Tension));
                }
            }
        }

        public bool DecideTimeUp => DecideTime >= DecideMax;

        private void StepSimulation(float dt)
        {
            SimTime += dt;
            foreach (var d in Deliveries)
            {
                if (!d.Started && SimTime >= d.At) DeliverOne(d);
                if (d.Started) d.Time += dt;
            }

            eventBuffer.Clear();
            Rings.Step(dt, Neighbours, Day, Metrics.Tension, eventBuffer, Turn);
            foreach (var e in eventBuffer)
            {
                if (e.Kind != SimEventKind.AdsNervous) Log.Events.Add(e);
                EventRaised?.Invoke(e);
            }

            if (SimTime >= Tuning.simLength) EndTurn();
        }

        private void DeliverOne(Delivery d)
        {
            int rev = Deliverer.Deliver(d, Neighbours, Weights, Rings.AdsNervous);
            RevenueDay += rev;
            RevenueTotal += rev;
            if (d.Entry.Mode == DecisionMode.Hide) ResentSilencing(d.Entry);
        }

        /// <summary>A neighbour whose post was hidden feels silenced: more distrust, less belonging.</summary>
        private void ResentSilencing(TurnPostEntry entry)
        {
            foreach (Neighbour n in Neighbours)
            {
                if (n.Name != entry.Author) continue;
                n.Desc = SimMath.Clamp01(n.Desc + Tuning.hiddenAuthorDesc);
                n.Pert = SimMath.Clamp01(n.Pert - Tuning.hiddenAuthorPert);
                // The author visibly reacts, so the player can link the silencing to the behaviour.
                n.Reaction = new Reaction
                {
                    Id = Deliverer.NextReactionId(),
                    Kind = ReactionKind.Silenciado,
                    Base = ReactionKind.Silenciado,
                    Strong = false,
                    Category = entry.Post.category,
                    TimeLeft = Tuning.reactionDuration
                };
                n.IconTime = 0f;
                return;
            }
        }

        private void EndTurn()
        {
            foreach (var d in Deliveries)
                if (!d.Started) DeliverOne(d);
            Phase = GamePhase.Report;
            foreach (var e in Log.Posts)
            {
                CategorySeen.TryGetValue(e.Post.category, out int c);
                CategorySeen[e.Post.category] = c + 1;
            }
        }

        /// <summary>Called when the player closes the newspaper. Moves to the next turn or closes the day.</summary>
        public void ContinueAfterReport()
        {
            if (Phase != GamePhase.Report) return;
            if (IsLastTurnOfDay)
            {
                CloseDay();
            }
            else
            {
                Turn++;
                StartTurn();
            }
        }

        public void CloseDay()
        {
            DayData d = CurrentDay;
            RevenueByDay[Day] = RevenueDay;
            var goals = new List<DayGoal>
            {
                new DayGoal { Kind = DayGoalKind.Revenue, Target = d.target, Achieved = RevenueDay >= d.target }
            };
            if (Day == 1) goals.Add(new DayGoal { Kind = DayGoalKind.LivrariaOpen, Achieved = Rings.LivrariaOpen });
            goals.Add(new DayGoal { Kind = DayGoalKind.TensionBelow, Achieved = Metrics.Tension < Tuning.nervousTension });
            DayResults.Add(goals);
            Pressure = goals[0].Achieved ? 0 : Math.Min(Pressure + 1, Tuning.pressureMax);

            if (Day == Tuning.daysCount - 1)
            {
                FinalEnding = ResolveEnding();
                Phase = GamePhase.End;
                return;
            }

            foreach (var n in Neighbours)
            {
                n.Stress = SimMath.Lerp(n.Stress, Tuning.restStress, Tuning.nightStressPull);
                n.Desc = SimMath.Lerp(n.Desc, Tuning.restDesc, Tuning.nightDescPull);
                n.Apat = SimMath.Lerp(n.Apat, Tuning.restApat, Tuning.nightApatPull);
            }
            if (!Rings.MerceariaOpen && Metrics.AvgStress(Neighbours) < Tuning.merceariaReopenStress) Rings.MerceariaOpen = true;
            Rings.PoliceOnStreet = false;
            Phase = GamePhase.DayEnd;
        }

        public EndingId ResolveEnding()
        {
            float lastTarget = Content.days[Tuning.daysCount - 1].target;
            float apat = Metrics.AvgApat(Neighbours);
            EndingId ending = EndingResolver.Resolve(Tuning, apat, Metrics.Tension, Metrics.Cohesion, RevenueDay, lastTarget, Understanding);
            BalanceMissedForUnderstanding = ending == EndingId.Gentry
                && EndingResolver.Resolve(Tuning, apat, Metrics.Tension, Metrics.Cohesion, RevenueDay, lastTarget, 1f) == EndingId.Balance;
            return ending;
        }

        /// <summary>Called when the player closes the end-of-day card.</summary>
        public void BeginNextDay()
        {
            if (Phase != GamePhase.DayEnd) return;
            Day++;
            Turn = 0;
            RevenueDay = 0;
            Rings.ResetForDay(Day);
            StartTurn();
        }

        // ------------------------------------------------------------------ neighbours

        private void StepNeighbours(float dt, bool live)
        {
            foreach (Neighbour n in Neighbours)
            {
                if (live)
                {
                    n.Stress += (Tuning.restStress - n.Stress) * Tuning.stressReturn * dt;
                    n.Desc += (Tuning.restDesc - n.Desc) * Tuning.descReturn * dt;
                    n.Apat += (Tuning.restApat - n.Apat) * Tuning.apatReturn * dt;
                    n.Compra = SimMath.Clamp01(n.Compra + Tuning.compraGain * dt);
                    n.LivAff = Math.Max(n.Trait == Trait.Reader ? Tuning.livAffMinReader : Tuning.livAffMin, n.LivAff - Tuning.livAffDecay * dt);
                    if (Rings.PoliceOnStreet && !n.Place.HasValue) n.Stress = SimMath.Clamp01(n.Stress + Tuning.policeStress * dt);
                    if (n.Place == PlaceId.Cafe)
                    {
                        n.Pert = SimMath.Clamp01(n.Pert + Tuning.cafePert * dt);
                        n.Stress = SimMath.Clamp01(n.Stress - Tuning.cafeStress * dt);
                    }
                    if (n.Place == PlaceId.Praca)
                    {
                        n.Stress = SimMath.Clamp01(n.Stress - Tuning.pracaStress * dt);
                        n.Pert = SimMath.Clamp01(n.Pert + Tuning.pracaPert * dt);
                    }
                    if (n.Place == n.Home) n.Stress = SimMath.Clamp01(n.Stress - Tuning.homeStress * dt);
                    if (n.Action == ActionKind.Ecra)
                    {
                        n.Apat = SimMath.Clamp01(n.Apat + Tuning.ecraApat * dt);
                        n.Pert = SimMath.Clamp01(n.Pert - Tuning.ecraPert * dt);
                    }
                    if (n.Place == PlaceId.Junta) n.Desc = SimMath.Clamp01(n.Desc + Tuning.juntaDesc * dt);
                    if (n.Reaction != null && n.Reaction.TimeLeft > 0f) n.Reaction.TimeLeft -= dt;
                }

                if (n.Path.Count > 0)
                {
                    PathPoint p = n.Path[0];
                    float dx = p.X - n.X, dy = p.Y - n.Y;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    float sp = Tuning.walkSpeed * dt * (n.Stress > Tuning.fastStressThreshold ? Tuning.fastSpeedMultiplier : 1f);
                    if (p.Cross && !n.Crossing)
                    {
                        if (!Traffic.PedGreen)
                        {
                            n.Waiting = true;
                            if (n.IconTime > 0f) n.IconTime -= dt;
                            continue;
                        }
                        n.Crossing = true;
                        n.Waiting = false;
                    }
                    if (d <= sp)
                    {
                        n.X = p.X;
                        n.Y = p.Y;
                        n.Path.RemoveAt(0);
                        n.Crossing = false;
                        if (n.Path.Count == 0) Arrive(n);
                    }
                    else
                    {
                        n.X += dx / d * sp;
                        n.Y += dy / d * sp;
                    }
                }
                else
                {
                    n.Dwell -= dt;
                    if (n.Dwell <= 0f) Decide(n);
                }
                if (n.IconTime > 0f) n.IconTime -= dt;
            }

            if (live) Contagion.Step(Neighbours, dt);
        }

        private static PlaceId? PlaceOf(ActionKind a)
        {
            switch (a)
            {
                case ActionKind.Cafe: return PlaceId.Cafe;
                case ActionKind.Mercearia: return PlaceId.Mercearia;
                case ActionKind.Livraria: return PlaceId.Livraria;
                case ActionKind.Junta: return PlaceId.Junta;
                case ActionKind.Praca: return PlaceId.Praca;
                default: return null;
            }
        }

        private void Decide(Neighbour n)
        {
            ActionKind a = Brain.Choose(n, MakeBrainContext());
            n.Action = a;
            if (a == ActionKind.Ecra)
            {
                n.Dwell = Rng.Range(Tuning.dwellEcra);
                return;
            }
            PlaceId key = a == ActionKind.Casa ? n.Home : PlaceOf(a).Value;
            if (n.Place.HasValue && n.Place.Value == key)
            {
                n.Path.Clear();
                n.Path.Add(Router.ZonePoint(key));
                n.Target = key;
                n.Place = null;
                return;
            }
            Router.RouteTo(n, key);
        }

        private void Arrive(Neighbour n)
        {
            n.Place = n.Target;
            n.Dwell = n.Action == ActionKind.Casa ? Rng.Range(Tuning.dwellHome) : Rng.Range(Tuning.dwellOther);
            if (n.Action == ActionKind.Mercearia) n.Compra = Math.Max(0f, n.Compra - Tuning.mercCompraDrop);
            if (n.Action == ActionKind.Livraria && Day >= 1 && Phase == GamePhase.Simulate)
            {
                Rings.Livraria = Math.Min(1f, Rings.Livraria + Tuning.livVisitRing);
                n.Pert = SimMath.Clamp01(n.Pert + Tuning.livVisitPert);
            }
        }

        // ------------------------------------------------------------------ discovery

        /// <summary>Player clicked a neighbour to note their reaction.</summary>
        public ObserveResult Observe(Neighbour n)
        {
            ObserveResult r = Discovery.Observe(n, Neighbours);
            if (r.Noted && Log != null)
            {
                Log.Notes++;
                if (r.TraitDiscovered) Log.Reveals.Add(new Reveal { Kind = RevealKind.Trait, A = n });
                foreach (var m in r.NewRelations) Log.Reveals.Add(new Reveal { Kind = RevealKind.Relation, A = n, B = m });
            }
            return r;
        }

        public float Understanding => Discovery.Understanding(Neighbours);
    }
}
