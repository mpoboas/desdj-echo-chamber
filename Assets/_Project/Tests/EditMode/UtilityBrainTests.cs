using NUnit.Framework;

namespace EchoChamber.Tests
{
    public class UtilityBrainTests
    {
        private static BrainContext Open() => new BrainContext { MerceariaOpen = true, LivrariaOpen = true };

        private static Neighbour Calm(GameSimulation sim)
        {
            var n = sim.Neighbours[0];
            n.Stress = 0.2f; n.Pert = 0.5f; n.Desc = 0.1f; n.Apat = 0.1f; n.Compra = 0.2f; n.LivAff = 0.1f;
            n.Action = ActionKind.Praca;
            return n;
        }

        [Test]
        public void ScoresMatchTheSpecFormulas()
        {
            var sim = TestContent.Sim();
            var n = sim.Neighbours[2]; // anxious
            n.Stress = 0.5f; n.Pert = 0.5f; n.Desc = 0.2f; n.Apat = 0.1f; n.Compra = 0.4f; n.LivAff = 0.3f;
            var c = Open();
            Assert.AreEqual(0.22f + 0.9f * 0.5f + 0.3f * 0.5f, sim.Brain.Score(n, ActionKind.Casa, c), 0.0001f);
            Assert.AreEqual(0.20f + 0.7f * 0.5f + 0.3f * 0.5f - 0.35f * 0.1f, sim.Brain.Score(n, ActionKind.Cafe, c), 0.0001f);
            Assert.AreEqual(0.05f + 1.1f * 0.4f, sim.Brain.Score(n, ActionKind.Mercearia, c), 0.0001f);
            Assert.AreEqual(0.02f + 0.35f * 0.5f + 0.95f * 0.3f - 0.4f * 0.1f, sim.Brain.Score(n, ActionKind.Livraria, c), 0.0001f);
            Assert.AreEqual(-0.45f + 1.35f * 0.2f + 0.35f * 0.5f - 0.3f * 0.1f, sim.Brain.Score(n, ActionKind.Junta, c), 0.0001f);
            Assert.AreEqual(0.12f + 0.45f * 0.5f + 0.25f * 0.5f, sim.Brain.Score(n, ActionKind.Praca, c), 0.0001f);
            Assert.AreEqual(-0.12f + 1.15f * 0.1f, sim.Brain.Score(n, ActionKind.Ecra, c), 0.0001f);
        }

        [Test]
        public void NightPushesHomeAndPullsAwayFromTheSquare()
        {
            var sim = TestContent.Sim();
            var n = Calm(sim);
            var day = Open();
            var night = Open();
            night.IsNight = true;
            Assert.AreEqual(0.55f, sim.Brain.Score(n, ActionKind.Casa, night) - sim.Brain.Score(n, ActionKind.Casa, day), 0.0001f);
            Assert.AreEqual(-0.33f, sim.Brain.Score(n, ActionKind.Praca, night) - sim.Brain.Score(n, ActionKind.Praca, day), 0.0001f);
        }

        [Test]
        public void ClosedShopsAreNeverChosen()
        {
            var sim = TestContent.Sim(new ConstantRandom(0f));
            var n = Calm(sim);
            n.Compra = 1f; n.LivAff = 1f;
            var c = new BrainContext { MerceariaOpen = false, LivrariaOpen = false };
            Assert.AreNotEqual(ActionKind.Mercearia, sim.Brain.Choose(n, c));
            Assert.AreNotEqual(ActionKind.Livraria, sim.Brain.Choose(n, c));
        }

        [Test]
        public void StressedNeighbourGoesHome()
        {
            var sim = TestContent.Sim(new ConstantRandom(0f));
            var n = sim.Neighbours[0];
            n.Stress = 0.95f; n.Pert = 0.3f; n.Desc = 0.1f; n.Apat = 0.1f; n.Compra = 0.1f; n.LivAff = 0.05f;
            n.Action = ActionKind.Praca;
            Assert.AreEqual(ActionKind.Casa, sim.Brain.Choose(n, Open()));
        }

        [Test]
        public void ApatheticNeighbourStaysOnTheScreen()
        {
            var sim = TestContent.Sim(new ConstantRandom(0f));
            var n = sim.Neighbours[3]; // addict
            n.Stress = 0.1f; n.Pert = 0.2f; n.Desc = 0.1f; n.Apat = 0.9f; n.Compra = 0.1f; n.LivAff = 0.05f;
            n.Action = ActionKind.Cafe;
            Assert.AreEqual(ActionKind.Ecra, sim.Brain.Choose(n, Open()));
        }

        [Test]
        public void DistrustfulNeighbourProtests()
        {
            var sim = TestContent.Sim(new ConstantRandom(0f));
            var n = sim.Neighbours[5];
            n.Stress = 0.6f; n.Pert = 0.3f; n.Desc = 0.95f; n.Apat = 0.05f; n.Compra = 0.1f; n.LivAff = 0.05f;
            n.Action = ActionKind.Cafe;
            Assert.AreEqual(ActionKind.Junta, sim.Brain.Choose(n, Open()));
        }

        [Test]
        public void CurrentActionIsPenalised()
        {
            var sim = TestContent.Sim(new ConstantRandom(0f));
            var n = Calm(sim);
            n.Action = ActionKind.Cafe;
            sim.Brain.Choose(n, Open());
            float expected = sim.Brain.Score(n, ActionKind.Cafe, Open()) - 0.18f;
            Assert.AreEqual(expected, sim.Brain.LastScores[ActionKind.Cafe], 0.0001f);
        }
    }
}
