using NUnit.Framework;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.UnitTest;

namespace VainFacadeTest
{
    [TestFixture()]
    public class AbandonedShackTest:BaseTest
	{

        [Test()]
        public void TestAbandonedShackSentinels()
        {
            SetupGameController("AkashBhuta", "TheSentinels", "Bunker", "Legacy", "VainFacadePlaytest.EldrenwoodVillage");
            StartGame();

            // "At the end of the environment turn, each player may shuffle their hand into their deck and draw 2 cards. Reduce damage dealt to heroes that do so by 2 until the start of the next environment turn."
            GoToPlayCardPhaseAndPlayCard(FindEnvironment(), "AbandonedShack");
            GoToEndOfTurn(FindEnvironment());

            QuickHPStorage(mainstay, idealist, writhe, medico, bunker.CharacterCard, legacy.CharacterCard);
            DealDamage(akash, mainstay, 3, DamageType.Melee);
            DealDamage(akash, idealist, 3, DamageType.Melee);
            DealDamage(akash, writhe, 3, DamageType.Melee);
            DealDamage(akash, medico, 3, DamageType.Melee);
            DealDamage(akash, bunker, 3, DamageType.Melee);
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-1, -1, -1, -1, -1, -1);
        }
    }
}