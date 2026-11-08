using NUnit.Framework;

namespace EchoChamber.Tests
{
    public class ContagionTests
    {
        private static Neighbour Place(GameSimulation sim, int i, PlaceId place, float x, float stress)
        {
            var n = sim.Neighbours[i];
            n.Place = place;
            n.Action = ActionKind.Cafe;
            n.X = x;
            n.Y = 100f;
            n.Stress = stress;
            n.Desc = stress;
            n.Pert = stress;
            return n;
        }

        [Test]
        public void CloseNeighboursConvergeAndConserveTheSum()
        {
            var sim = TestContent.Sim();
            var a = Place(sim, 0, PlaceId.Cafe, 100f, 0.9f);
            var b = Place(sim, 1, PlaceId.Cafe, 150f, 0.1f);
            var list = new System.Collections.Generic.List<Neighbour> { a, b };
            float gap0 = a.Stress - b.Stress;
            for (int i = 0; i < 1200; i++) sim.Contagion.Step(list, 0.1f);
            Assert.Less(a.Stress - b.Stress, gap0 * 0.05f);
            Assert.AreEqual(1.0f, a.Stress + b.Stress, 0.001f);
            Assert.AreEqual(1.0f, a.Pert + b.Pert, 0.001f);
        }

        [Test]
        public void RateIsThreePointFivePercentPerSecond()
        {
            var sim = TestContent.Sim();
            var a = Place(sim, 0, PlaceId.Cafe, 100f, 1.0f);
            var b = Place(sim, 1, PlaceId.Cafe, 100f, 0.0f);
            sim.Contagion.Step(new System.Collections.Generic.List<Neighbour> { a, b }, 1f);
            Assert.AreEqual(1.0f - 0.035f, a.Stress, 0.0001f);
            Assert.AreEqual(0.035f, b.Stress, 0.0001f);
        }

        [Test]
        public void NoContagionBeyondRadius()
        {
            var sim = TestContent.Sim();
            var a = Place(sim, 0, PlaceId.Cafe, 100f, 0.9f);
            var b = Place(sim, 1, PlaceId.Cafe, 100f + 111f, 0.1f);
            Assert.IsFalse(sim.Contagion.CanInteract(a, b));
        }

        [Test]
        public void NoContagionAtDifferentPlacesOnScreenOrAtHome()
        {
            var sim = TestContent.Sim();
            var a = Place(sim, 0, PlaceId.Cafe, 100f, 0.9f);
            var b = Place(sim, 1, PlaceId.Praca, 100f, 0.1f);
            Assert.IsFalse(sim.Contagion.CanInteract(a, b));

            b = Place(sim, 1, PlaceId.Cafe, 100f, 0.1f);
            b.Action = ActionKind.Ecra;
            Assert.IsFalse(sim.Contagion.CanInteract(a, b));

            a = Place(sim, 0, PlaceId.CasasN, 100f, 0.9f);
            b = Place(sim, 1, PlaceId.CasasN, 100f, 0.1f);
            Assert.IsFalse(sim.Contagion.CanInteract(a, b));
        }
    }
}
