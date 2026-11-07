using NUnit.Framework;

namespace EchoChamber.Tests
{
    public class UnderstandingTests
    {
        private const int Seeds = 40;

        [Test]
        public void EpilogueTiersFollowTheThresholds()
        {
            var t = TestContent.Tuning();
            StringAssert.Contains("número", Texts.Epilogue(0.1f, t, false));
            StringAssert.Contains("parte do bairro", Texts.Epilogue(t.epilogueMid, t, false));
            StringAssert.Contains("melhor do que ele", Texts.Epilogue(t.epilogueHigh, t, false));
            StringAssert.Contains("equilíbrio ao alcance", Texts.Epilogue(0.1f, t, true));
            StringAssert.DoesNotContain("equilíbrio ao alcance", Texts.Epilogue(0.1f, t, false));
        }

        [Test]
        public void ADiligentPlayerLearnsAboutTheNeighbourhood()
        {
            var r = HeadlessRunner.Run(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.TargetBalance(), 1);
            Assert.Greater(r.Understanding, 0.1f);
        }

        [Test]
        public void APlayerWhoNeverNotesUnderstandsNothing()
        {
            var r = HeadlessRunner.Run(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.TargetBalanceNoNotes(), 1);
            Assert.AreEqual(0f, r.Understanding);
        }

        [Test]
        public void WithoutNotesTheRareBalanceTurnsIntoGentrification()
        {
            var c = TestContent.Content();
            var t = TestContent.Tuning();
            int balanceNoNotes = 0, gentryNoNotes = 0, balanceWithNotes = 0;
            for (int s = 1; s <= Seeds; s++)
            {
                var a = HeadlessRunner.Run(c, t, PolicyLibrary.TargetBalanceNoNotes(), s);
                var b = HeadlessRunner.Run(c, t, PolicyLibrary.TargetBalance(), s);
                if (a.Ending == EndingId.Balance) balanceNoNotes++;
                if (a.Ending == EndingId.Gentry) gentryNoNotes++;
                if (b.Ending == EndingId.Balance) balanceWithNotes++;
            }
            Assert.AreEqual(0, balanceNoNotes, "no Balance without understanding");
            Assert.Greater(gentryNoNotes, 0);
            Assert.Greater(balanceWithNotes, Seeds / 3, "a diligent player still reaches Balance often");
        }

        [Test]
        public void TheFlagRemembersWhyBalanceWasMissed()
        {
            var c = TestContent.Content();
            var t = TestContent.Tuning();
            int flagged = 0;
            for (int s = 1; s <= Seeds; s++)
            {
                var r = HeadlessRunner.Run(c, t, PolicyLibrary.TargetBalanceNoNotes(), s);
                if (r.BalanceMissedForUnderstanding)
                {
                    flagged++;
                    Assert.AreEqual(EndingId.Gentry, r.Ending);
                }
            }
            Assert.Greater(flagged, 0);
        }
    }
}
