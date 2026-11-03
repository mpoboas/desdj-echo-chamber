using NUnit.Framework;

namespace EchoChamber.Tests
{
    public class ContentImportTests
    {
        [Test]
        public void ImportedContentMatchesDataFiles()
        {
            var c = TestContent.Content();
            Assert.AreEqual(21, c.pool.Count);
            Assert.AreEqual(14, c.scripted.Count);
            Assert.AreEqual(3, c.days.Count);
            Assert.AreEqual(14, c.neighbours.Count);
            Assert.AreEqual(7, c.places.Count);
            Assert.AreEqual(5, c.endings.Count);
        }

        [Test]
        public void DayTargetsAreFromTheSpec()
        {
            var c = TestContent.Content();
            Assert.AreEqual(140, c.days[0].target);
            Assert.AreEqual(220, c.days[1].target);
            Assert.AreEqual(260, c.days[2].target);
        }

        [Test]
        public void EveryScriptedReferenceResolves()
        {
            var c = TestContent.Content();
            foreach (var day in c.days)
                foreach (var turn in day.turns)
                    foreach (var e in turn.entries)
                        if (e.kind == TurnEntryKind.Scripted)
                            Assert.IsNotNull(c.FindScripted(e.postId), e.postId);
        }

        [Test]
        public void ScriptedPostsKeepTheirHiddenEffects()
        {
            var c = TestContent.Content();
            var r1 = c.FindScripted("r1");
            Assert.AreEqual(PostCategory.Sens, r1.category);
            Assert.AreEqual(4f, r1.effects.medo);
            Assert.AreEqual(2f, r1.effects.desc);
            Assert.AreEqual(2.0f, r1.value, 0.0001f);
            Assert.AreEqual(2f, c.FindScripted("liv1").livraria);
        }

        [Test]
        public void SevenNeighbourTraitsAreMapped()
        {
            var c = TestContent.Content();
            Assert.AreEqual(Trait.Solidary, c.neighbours[0].trait);
            Assert.AreEqual(Trait.Sceptic, c.neighbours[1].trait);
            Assert.AreEqual(Trait.Reader, c.neighbours[10].trait);
            Assert.AreEqual(PlaceId.CasasN, c.neighbours[0].home);
            Assert.AreEqual(PlaceId.CasasS, c.neighbours[1].home);
        }
    }
}
