using NUnit.Framework;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.UnitTest;
using Handelabra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadeTest
{
    [TestFixture()]
    public class SuspiciousBoyDoubleTests:BaseTest
	{
        protected TurnTakerController carnaval { get { return FindHero("Carnaval"); } }
        protected TurnTakerController doomsayer { get { return FindVillain("Doomsayer"); } }
        protected Card countless { get { return GetCard("CountlessWords"); } }

        protected void ClearInitialCards()
        {
            Card futile = MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            Card nothing = MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            Card looking = MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);

            Console.WriteLine($"Moving {FindCardsWhere((Card c) => c.IsVillain && !c.IsCharacter && c.IsInPlayAndHasGameText).Select((Card c) => c.Title).ToCommaList()} to the villain trash");
            MoveCards(doomsayer, FindCardsWhere((Card c) => c.IsVillain && !c.IsCharacter && c.IsInPlayAndHasGameText), doomsayer.TurnTaker.Trash);
            Console.WriteLine($"Moving {FindCardsWhere((Card c) => c.IsVillainTarget && c != doomsayer.CharacterCard && c.IsInPlayAndHasGameText).Select((Card c) => c.Title).ToCommaList()} to the villain trash");
            MoveCards(doomsayer, FindCardsWhere((Card c) => c.IsVillainTarget && c != doomsayer.CharacterCard && c.IsInPlayAndHasGameText), doomsayer.TurnTaker.Trash);

            MoveCard(doomsayer, futile, doomsayer.TurnTaker.Trash);
            MoveCard(doomsayer, nothing, doomsayer.TurnTaker.Trash);
            MoveCard(doomsayer, looking, doomsayer.TurnTaker.Trash);

            ShuffleTrashIntoDeck(doomsayer);
        }

        [Test()]
        public void TestVoss3HPNonIndestructible()
        {
            SetupGameController("GrandWarlordVoss", "VainFacadePlaytest.Carnaval", "Legacy", "Ra", "TimeCataclysm");
            StartGame();

            //Play this card in the play area of a target with 3 or fewer HP. Move that target under this card.
            //At the end of that target's turn, the 3 targets in this play area with the highest HP each deal themselves 2 toxic damage. Then destroy this card.
            SetHitPoints(voss, 3);
            DecisionSelectCard = voss.CharacterCard;
            Card sus = PlayCard("SuspiciousBodyDouble");
            AssertGameOver(EndingResult.VillainDestroyedVictory);
        }

        [Test()]
        public void TestVoss3HPIndestructible()
        {
            SetupGameController("GrandWarlordVoss", "VainFacadePlaytest.Carnaval", "Legacy", "Ra", "TimeCataclysm");
            StartGame();

            //Play this card in the play area of a target with 3 or fewer HP. Move that target under this card.
            //At the end of that target's turn, the 3 targets in this play area with the highest HP each deal themselves 2 toxic damage. Then destroy this card.
            SetHitPoints(voss, 3);
            PlayCard("FixedPoint");
            AssertNextDecisionChoices(null, new Card[] { voss.CharacterCard });
            Card sus = PlayCard("SuspiciousBodyDouble");
            AssertNotGameOver();
        }

        [Test()]
        public void TestDoomsayerSBD1()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "VainFacadePlaytest.Carnaval", "Legacy", "Ra", "TimeCataclysm");
            StartGame();

            //Play this card in the play area of a target with 3 or fewer HP. Move that target under this card.
            //At the end of that target's turn, the 3 targets in this play area with the highest HP each deal themselves 2 toxic damage. Then destroy this card.
            SetHitPoints(doomsayer, 3);
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            DecisionSelectCard = doomsayer.CharacterCard;
            Card sus = PlayCard("SuspiciousBodyDouble");
            AssertGameOver(EndingResult.VillainDestroyedVictory);
        }

        [Test()]
        public void TestDoomsayerSBD2()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "VainFacadePlaytest.Carnaval", "Legacy", "Ra", "TimeCataclysm");
            StartGame();

            //Play this card in the play area of a target with 3 or fewer HP. Move that target under this card.
            //At the end of that target's turn, the 3 targets in this play area with the highest HP each deal themselves 2 toxic damage. Then destroy this card.
            SetHitPoints(doomsayer, 3);
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            Card owe = PlayCard("TheyOweYou");
            DecisionSelectCard = doomsayer.CharacterCard;
            Card sus = PlayCard("SuspiciousBodyDouble");
            AssertNotGameOver();
        }

        [Test()]
        public void TestIfUnderLocationIsInPlay()
        {
            SetupGameController("GrandWarlordVoss", "VainFacadePlaytest.Carnaval", "Legacy", "Ra", "TimeCataclysm");
            StartGame();

            //Play this card in the play area of a target with 3 or fewer HP. Move that target under this card.
            //At the end of that target's turn, the 3 targets in this play area with the highest HP each deal themselves 2 toxic damage. Then destroy this card.
            Card frost = PlayCard("GeneBoundFrosthound");
            DecisionSelectCard = frost;
            Card sus = PlayCard("SuspiciousBodyDouble");
            Console.WriteLine($"The card under Suspicious Body Double {(frost.IsInPlay ? "is" : "is not")} in play.");
            Console.WriteLine($"The card under Suspicious Body Double {(frost.IsInPlayAndHasGameText ? "does" : "does not")} have game text.");
            GoToEndOfTurn(voss);
            Console.WriteLine($"The card under SBD is now at {frost.Location.GetFriendlyName()}");
        }
    }
}

