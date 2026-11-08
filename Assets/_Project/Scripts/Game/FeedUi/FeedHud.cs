using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoChamber
{
    /// <summary>Screen-space HUD for the v2 game: ringing phone, subtitled calls, day card and end-of-day summary.</summary>
    public class FeedHud
    {
        public VisualElement Root;
        private VisualElement chip, sub, card, summary;
        private Label who, text, cardTitle, cardSub, panelTitle, panelBody, btnText;
        private VisualElement warns, btn;
        private Action onAnswered, onLine, onSummary;
        private List<FeedLine> lines;
        private int lineIndex;
        private string goalText = "";
        private Action onDialogueDone;

        public bool Ringing { get; private set; }
        /// <summary>True while something full-screen or modal is up (room clicks are blocked).</summary>
        public bool Modal { get; private set; }

        public void Build(VisualElement parent)
        {
            Root = new VisualElement { pickingMode = PickingMode.Ignore };
            Root.AddToClassList("hud-root");
            parent.Add(Root);

            chip = new VisualElement();
            chip.AddToClassList("hud-chip");
            var ct = new Label("O telefone está a tocar");
            ct.AddToClassList("hud-chip-text");
            var cg = new Label("Atender");
            cg.AddToClassList("hud-chip-go");
            chip.Add(ct);
            chip.Add(cg);
            Root.Add(chip);

            sub = new VisualElement();
            sub.AddToClassList("hud-sub");
            who = new Label();
            who.AddToClassList("hud-who");
            text = new Label();
            text.AddToClassList("hud-text");
            var go = new Label("clica para continuar");
            go.AddToClassList("hud-go");
            sub.Add(who);
            sub.Add(text);
            sub.Add(go);
            Root.Add(sub);

            card = new VisualElement();
            card.AddToClassList("hud-card");
            cardTitle = new Label();
            cardTitle.AddToClassList("hud-card-title");
            cardSub = new Label();
            cardSub.AddToClassList("hud-card-sub");
            card.Add(cardTitle);
            card.Add(cardSub);
            Root.Add(card);

            summary = new VisualElement();
            summary.AddToClassList("hud-summary");
            var panel = new VisualElement();
            panel.AddToClassList("hud-panel");
            panelTitle = new Label();
            panelTitle.AddToClassList("hud-panel-title");
            panelBody = new Label();
            panelBody.AddToClassList("hud-panel-body");
            warns = new VisualElement();
            warns.AddToClassList("hud-warns");
            btn = new VisualElement();
            btn.AddToClassList("hud-btn");
            btnText = new Label();
            btnText.AddToClassList("hud-btn-text");
            btn.Add(btnText);
            panel.Add(panelTitle);
            panel.Add(panelBody);
            panel.Add(warns);
            panel.Add(btn);
            summary.Add(panel);
            Root.Add(summary);
        }

        /// <summary>Real mouse press, already mapped to HUD panel space. Returns true when the HUD used it.</summary>
        public bool TryClick(Vector2 p)
        {
            if (Ringing && chip.worldBound.Contains(p)) { AnswerRing(); return true; }
            if (lines != null) { NextLine(); return true; }
            if (summary.ClassListContains("hud-summary--on"))
            {
                if (btn.worldBound.Contains(p)) CloseSummary();
                return true;
            }
            return Modal;
        }

        public void ShowDayCard(string title, string subtitle, float seconds, Action done)
        {
            Modal = true;
            cardTitle.text = title;
            cardSub.text = subtitle;
            card.AddToClassList("hud-card--show");
            card.schedule.Execute(() => card.AddToClassList("hud-card--on")).StartingIn(30);
            card.schedule.Execute(() => card.RemoveFromClassList("hud-card--on")).StartingIn((long)(seconds * 1000f));
            card.schedule.Execute(() =>
            {
                card.RemoveFromClassList("hud-card--show");
                Modal = false;
                done?.Invoke();
            }).StartingIn((long)(seconds * 1000f) + 700);
        }

        public void ShowRing(Action answered)
        {
            Ringing = true;
            onAnswered = answered;
            chip.AddToClassList("hud-chip--on");
        }

        public void AnswerRing()
        {
            if (!Ringing) return;
            Ringing = false;
            chip.RemoveFromClassList("hud-chip--on");
            var cb = onAnswered;
            onAnswered = null;
            cb?.Invoke();
        }

        public void ShowDialogue(List<FeedLine> list, string goal, Action done)
        {
            if (list == null || list.Count == 0) { done?.Invoke(); return; }
            Modal = true;
            lines = list;
            lineIndex = 0;
            goalText = goal;
            onDialogueDone = done;
            sub.AddToClassList("hud-sub--on");
            NextLine();
        }

        private void NextLine()
        {
            if (lines == null) return;
            if (lineIndex >= lines.Count)
            {
                sub.RemoveFromClassList("hud-sub--on");
                lines = null;
                Modal = false;
                var cb = onDialogueDone;
                onDialogueDone = null;
                cb?.Invoke();
                return;
            }
            var l = lines[lineIndex++];
            who.text = l.speaker.ToUpperInvariant();
            text.text = l.text.Replace("{goal}", goalText);
        }

        public void ShowSummary(string title, string body, int warnings, int maxWarnings, string button, Action done)
        {
            Modal = true;
            panelTitle.text = title;
            panelBody.text = body;
            warns.Clear();
            for (int i = 0; i < maxWarnings; i++)
            {
                var w = new VisualElement();
                w.AddToClassList("hud-warn");
                if (i < warnings) w.AddToClassList("hud-warn--on");
                warns.Add(w);
            }
            btnText.text = button;
            onSummary = done;
            summary.AddToClassList("hud-summary--on");
        }

        private void CloseSummary()
        {
            summary.RemoveFromClassList("hud-summary--on");
            Modal = false;
            var cb = onSummary;
            onSummary = null;
            cb?.Invoke();
        }
    }
}
