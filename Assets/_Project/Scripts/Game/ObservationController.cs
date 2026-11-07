using System.Text;
using UnityEngine;

namespace EchoChamber
{
    /// <summary>Player clicked a neighbour with a visible reaction: write it in the notebook and maybe reveal something.</summary>
    public class ObservationController : MonoBehaviour
    {
        public static ObservationController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void Observe(int index)
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Sim == null) return;
            var sim = gm.Sim;
            if (sim.Phase == GamePhase.Intro || sim.Phase == GamePhase.End) return;
            var n = sim.Neighbours[index];
            var r = sim.Observe(n);
            if (!r.Noted)
            {
                SubtitleView.Say("Caderno", $"{n.Name} está {Texts.ActionLabel(n.Action)}. Nada de novo para anotar agora.", SubtitleView.Style.Hint, 2.6f);
                return;
            }
            var sb = new StringBuilder();
            if (r.Reaction.Kind == ReactionKind.Silenciado)
                sb.Append($"{n.Name} {ReactionStyle.Label(r.Reaction.Kind)} depois de um post seu de {ReactionStyle.CategoryLabel(r.Reaction.Category)} não ter saído.");
            else
                sb.Append($"{n.Name} {ReactionStyle.Label(r.Reaction.Kind)}{(r.Reaction.Strong ? " <b>com força</b>" : "")} com um post de {ReactionStyle.CategoryLabel(r.Reaction.Category)}.");
            if (r.TraitDiscovered)
            {
                sb.Append($" <b>Percebeste: {n.Name} é {n.Data.traitLabel}</b> ({Texts.TraitDescription(n.Trait, n.Data.traitLabel)}).");
                GameAudio.Good();
            }
            foreach (var m in r.NewRelations) sb.Append($" Anda muito com {m.Name}.");
            SubtitleView.Say("Caderno", sb.ToString(), SubtitleView.Style.Hint, 5.6f);
            GameAudio.Note();
        }
    }
}
