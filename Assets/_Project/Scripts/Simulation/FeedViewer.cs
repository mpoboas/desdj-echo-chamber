using System;
using System.Collections.Generic;

namespace EchoChamber
{
    public enum ViewerState
    {
        Waiting,
        Swiping,
        Watching,
        Resting
    }

    /// <summary>
    /// One neighbour scrolling short videos: watches, skips, likes, reads and writes comments, gets tired and puts
    /// the phone down. Fed by the default algorithm and by the posts the player hands over. Pure C#, no engine types.
    /// Time spent hooked is the "tempo na app" the boss counts.
    /// </summary>
    public class FeedViewer
    {
        private readonly FeedTuning t;
        private readonly SimRandom rng;
        private readonly FeedSystem system;
        private readonly FeedContent content;
        private readonly Queue<FeedPost> queue = new Queue<FeedPost>();
        private readonly List<ViewerEvent> events = new List<ViewerEvent>();

        public FeedPerson Person { get; private set; }
        public ViewerState State { get; private set; }
        public FeedPost Current { get; private set; }
        public bool CurrentMine { get; private set; }
        public float Energy { get; private set; } = 1f;
        public float AppSeconds { get; private set; }
        public float WatchTime { get; private set; }
        public float Duration { get; private set; }
        public bool Liked { get; private set; }
        public bool CommentsOpen { get; private set; }
        public string TypedText { get; private set; } = "";
        public int Hook { get; private set; }
        public int QueuedCount { get { return queue.Count; } }

        public float Progress { get { return Duration <= 0f ? 0f : Math.Min(1f, WatchTime / Duration); } }
        public float Minutes { get { return AppSeconds / 60f; } }
        public bool Resting { get { return State == ViewerState.Resting; } }

        // per-video plan
        private float timer;
        private bool skip, hooked, doLike, doOpen, doWrite, liked, opened, written, closed, commentDone;
        private float likeAt, openAt, readEnd, typeEnd, holdEnd, interest;
        private string comment = "";
        private float typeStart;
        private float restFor;

        public FeedViewer(FeedPerson person, FeedContent content, FeedSystem system, FeedTuning tuning, SimRandom random, float startDelay)
        {
            Person = person;
            this.content = content;
            this.system = system;
            t = tuning;
            rng = random;
            timer = startDelay;
            State = ViewerState.Waiting;
        }

        public void Enqueue(FeedPost post)
        {
            queue.Enqueue(post);
            // the player's post must show up within a couple of seconds: cut the current video short
            if (State == ViewerState.Watching && !CurrentMine)
            {
                Duration = Math.Min(Duration, WatchTime + t.interruptSeconds);
                if (opened && !closed)
                {
                    closed = true;
                    CommentsOpen = false;
                    TypedText = "";
                    Emit(ViewerEventKind.CommentsClosed);
                }
                commentDone = true;
            }
        }

        public void ResetForDay()
        {
            Energy = 1f;
            AppSeconds = 0f;
            queue.Clear();
        }

        public List<ViewerEvent> DrainEvents()
        {
            var copy = new List<ViewerEvent>(events);
            events.Clear();
            return copy;
        }

        private void Emit(ViewerEventKind k, string text = null)
        {
            events.Add(new ViewerEvent { Viewer = this, Kind = k, Post = Current, Text = text, Mine = CurrentMine });
        }

        public void Step(float dt, bool running, Func<FeedViewer, FeedPost> pickDefault)
        {
            switch (State)
            {
                case ViewerState.Waiting:
                    if (!running) return;
                    timer -= dt;
                    if (timer <= 0f) BeginSwipe(pickDefault);
                    break;
                case ViewerState.Swiping:
                    timer -= dt;
                    if (timer <= 0f) BeginWatch();
                    break;
                case ViewerState.Watching:
                    Watch(dt, running);
                    break;
                case ViewerState.Resting:
                    restFor -= dt;
                    if (!running || queue.Count > 0 || restFor <= 0f)
                    {
                        Energy = t.energyAfterRest;
                        State = ViewerState.Waiting;
                        timer = 0.1f;
                        Emit(ViewerEventKind.Woke);
                    }
                    break;
            }
        }

        private void BeginSwipe(Func<FeedViewer, FeedPost> pickDefault)
        {
            if (queue.Count > 0)
            {
                Current = queue.Dequeue();
                CurrentMine = true;
            }
            else
            {
                Current = pickDefault(this);
                CurrentMine = false;
            }
            State = ViewerState.Swiping;
            timer = t.swipeSeconds;
            Liked = false;
            CommentsOpen = false;
            TypedText = "";
            WatchTime = 0f;
        }

        private void BeginWatch()
        {
            var post = Current;
            var person = Person;
            float match = person.Profile.interests.Contains(post.theme) ? 1f : 0.15f;
            float hot = post.inflamed ? 1f : 0f;
            interest = Math.Min(1f, match * 0.8f + hot * 0.45f + (post.filler ? 0f : 0.05f));
            if (CurrentMine)
            {
                Energy = Math.Min(1f, Energy + t.minePostEnergy + t.minePostEnergyByInterest * interest);
                Hook = interest >= t.hookInterest ? t.hookVideosGood : t.hookVideosOther;
            }
            hooked = Hook > 0;
            if (hooked && !CurrentMine) Hook--;

            float dur = rng.Range(2.4f, 3.6f) + interest * rng.Range(6f, 11f);
            if (hooked) dur *= 1.25f;
            skip = !CurrentMine && !hooked && interest < 0.35f && rng.Value() < 0.7f;
            if (skip) dur = rng.Range(1f, 1.8f);
            Duration = dur;

            bool angry = person.Aggression > 0.6f;
            float pLike = 0.08f + interest * 0.62f;
            float pCom = 0.04f + hot * 0.38f + match * 0.2f;
            float pWrite = hot * (0.35f + person.Aggression * 0.5f) + match * 0.12f
                           + (person.Profile.key == "Rosa" && post.theme == FeedTheme.Comunidade ? 0.3f : 0f);
            doLike = rng.Value() < pLike;
            doOpen = !skip && rng.Value() < pCom;
            doWrite = doOpen && rng.Value() < pWrite;
            likeAt = dur * (skip ? 0.5f : rng.Range(0.25f, 0.55f));
            openAt = dur * rng.Range(0.5f, 0.65f);
            readEnd = openAt + rng.Range(t.readCommentsMin, t.readCommentsMax);
            comment = doWrite ? PickComment(post, angry) : "";
            typeStart = readEnd;
            typeEnd = typeStart + comment.Length * t.typeSecondsPerChar + 0.5f;
            holdEnd = typeEnd + 0.9f;
            opened = written = closed = liked = false;
            commentDone = !doOpen;
            WatchTime = 0f;
            State = ViewerState.Watching;
            Emit(skip ? ViewerEventKind.Skipped : ViewerEventKind.VideoStarted);
        }

        private void Watch(float dt, bool running)
        {
            WatchTime += dt;
            if (running)
            {
                float rate = t.appTimeMultiplier * (0.25f + interest * 1.15f) * (hooked ? t.hookedRateMultiplier : 1f);
                AppSeconds += dt * rate;
            }
            if (doLike && !liked && WatchTime >= likeAt)
            {
                liked = true;
                Liked = true;
                Emit(ViewerEventKind.Liked);
            }
            if (doOpen)
            {
                if (!opened && WatchTime >= openAt)
                {
                    opened = true;
                    CommentsOpen = true;
                    Emit(ViewerEventKind.CommentsOpened);
                }
                if (opened && doWrite && !written)
                {
                    if (WatchTime >= typeStart)
                    {
                        int n = (int)Math.Min(comment.Length, (WatchTime - typeStart) / t.typeSecondsPerChar);
                        TypedText = comment.Substring(0, n);
                    }
                    if (WatchTime >= typeEnd)
                    {
                        written = true;
                        TypedText = "";
                        Emit(ViewerEventKind.CommentPosted, comment);
                    }
                }
                float closeAt = doWrite ? holdEnd : readEnd;
                if (opened && !closed && WatchTime >= closeAt)
                {
                    closed = true;
                    commentDone = true;
                    CommentsOpen = false;
                    Emit(ViewerEventKind.CommentsClosed);
                }
            }
            if (WatchTime >= Duration && commentDone) EndVideo();
        }

        private void EndVideo()
        {
            system.ApplyExposure(Current, Person);
            float hot = Current.inflamed ? 1f : 0f;
            Energy = Math.Max(0f, Math.Min(1f, Energy - (1f - interest) * t.fatiguePerBoring - t.fatigueBase + hot * t.hotRestores));
            if (Energy < t.restThreshold && queue.Count == 0)
            {
                State = ViewerState.Resting;
                restFor = rng.Range(t.restMin, t.restMax);
                Emit(ViewerEventKind.FellAsleep);
            }
            else
            {
                State = ViewerState.Waiting;
                timer = 0.05f;
            }
        }

        private string PickComment(FeedPost post, bool angry)
        {
            var key = Person.Profile.key;
            List<string> pool = null;
            bool hot = post.inflamed;
            if (key == "Tiago") pool = content.Pool(post.theme == FeedTheme.Meme ? "Tiago.meme" : "Tiago.other");
            else if (key == "Rosa" || key == "Leonor" || key == "Hugo") pool = content.Pool(key + (hot ? ".hot" : ".calm"));
            else pool = content.Pool(hot ? (angry ? "generic.fight" : "generic.hot") : "generic.neutral");
            if (pool == null || pool.Count == 0) pool = content.Pool(hot ? "generic.hot" : "generic.calm");
            if (pool == null || pool.Count == 0) return "...";
            return pool[rng.Index(pool.Count)];
        }
    }
}
