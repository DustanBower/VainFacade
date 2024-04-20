using NUnit.Framework;
using System;
using VainFacadePlaytest;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using System.Collections.Generic;
using VainFacadePlaytest.Carnaval;
using System.Runtime.Serialization;
using System.Security.Policy;

namespace VainFacadeTest
{
    [TestFixture()]
    public class CarnavalTests:BaseTest
	{
        protected HeroTurnTakerController carnaval { get { return FindHero("Carnaval"); } }

        
        [Test()]
        public void TestLoadCarnaval()
        {
            SetupGameController("BaronBlade", "VainFacadePlaytest.Carnaval", "Legacy", "Bunker", "TheScholar", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(carnaval);
            Assert.IsInstanceOf(typeof(CarnavalCharacterCardController), carnaval.CharacterCardController);

            Assert.AreEqual(28, carnaval.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestSuspiciousBodyDouble_DestroyTarget()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Carnaval", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When this card enters play, you may destroy a target with 3 or fewer HP.
            //If a target is destroyed this way, put this card in that card's play area, otherwise destroy this card.
            Card rockslide = PlayCard("LivingRockslide");
            Card arboreal = PlayCard("ArborealPhalanges");
            Card carapace = PlayCard("MountainousCarapace");
            Card brambles = PutOnDeck("EnsnaringBrambles");
            Card train = PlayCard("PlummetingMonorail");
            PlayCard("InspiringPresence");
            SetHitPoints(rockslide, 3);
            SetHitPoints(arboreal, 6);
            SetHitPoints(brambles, 5);
            AssertNextDecisionChoices(new Card[] { rockslide }, new Card[] { akash.CharacterCard, arboreal, carapace, brambles, train, carnaval.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, scholar.CharacterCard });
            Card sbd = PlayCard("SuspiciousBodyDouble");
            AssertInTrash(rockslide);
            AssertAtLocation(sbd, akash.TurnTaker.PlayArea);

            //At the end of that play area's turn,the three targets in this play area with the highest HP each deal themselves 2 toxic damage, then destroy this card.
            QuickHPStorage(akash.CharacterCard, carnaval.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, scholar.CharacterCard, arboreal, carapace, brambles, train);
            GoToEndOfTurn(akash);

            //Akash, Arboreal Phalanges, and Mountainous Carapace should hit themselves. Damage should not be boosted by Inspiring Presence
            QuickHPCheck(-1, 0, -3, 0, 0, -2, -2, 0, 0);

            AssertInTrash(sbd);
        }

        [Test()]
        public void TestSuspiciousBodyDouble_NoAvailableTargets()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Carnaval", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            Card rockslide = PlayCard("LivingRockslide");
            Card sbd = PlayCard("SuspiciousBodyDouble");
            AssertIsInPlay(rockslide);
            AssertInTrash(sbd);
        }

        [Test()]
        public void TestSuspiciousBodyDouble_ChooseNoDestroy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Carnaval", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            DecisionDoNotSelectCard = SelectionType.DestroyCard;
            Card rockslide = PlayCard("LivingRockslide");
            SetHitPoints(rockslide, 3);
            Card sbd = PlayCard("SuspiciousBodyDouble");
            AssertIsInPlay(rockslide);
            AssertInTrash(sbd);
        }

        [Test()]
        public void TestSuspiciousBodyDouble_Character()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Carnaval", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            SetHitPoints(akash, 3);
            Card sbd = PlayCard("SuspiciousBodyDouble");
            AssertGameOver(EndingResult.VillainDestroyedVictory);
        }

        [Test()]
        public void TestSuspiciousBodyDouble_Indestructible()
        {
            SetupGameController(new string[] { "GloomWeaver", "VainFacadePlaytest.Carnaval", "Legacy", "Bunker", "TheScholar", "Megalopolis" },challenge:true);
            StartGame();

            Card relic = PlayCard("DrumOfDespair");
            SetHitPoints(relic, 3);
            Card sbd = PlayCard("SuspiciousBodyDouble");
            AssertIsInPlay(relic);
            AssertInTrash(sbd);
        }

        [Test()]
        public void TestSuspiciousBodyDouble_FlipCharacter()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Carnaval", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            SetHitPoints(legacy, 3);
            Card sbd = PlayCard("SuspiciousBodyDouble");
            AssertIncapacitated(legacy);
            AssertInTrash(sbd);
        }

        [Test()]
        public void TestSuspiciousBodyDouble_NextTo()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Carnaval", "Legacy", "Bunker", "TheScholar", "TheEnclaveOfTheEndlings");
            StartGame();

            GoToPlayCardPhase(FindEnvironment());
            Card rockslide = PutOnDeck("LivingRockslide");
            Card bloogo = PlayCard("Bloogo");
            SetHitPoints(bloogo, 3);
            GoToEndOfTurn(FindEnvironment());
            Card sbd = PlayCard("SuspiciousBodyDouble");
            AssertInTrash(bloogo);
            AssertIsInPlay(rockslide);
            AssertAtLocation(sbd, akash.TurnTaker.PlayArea);
        }
    }
}

