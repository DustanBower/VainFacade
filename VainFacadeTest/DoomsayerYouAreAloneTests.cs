using NUnit.Framework;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.UnitTest;
using System.Linq;
using VainFacadePlaytest.Doomsayer;
using Handelabra;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace VainFacadeTest
{
    [TestFixture()]
    public class DoomsayerYouAreAloneTests : BaseTest
    {
        protected TurnTakerController doomsayer { get { return FindVillain("Doomsayer"); } }
        protected Card countless { get { return GetCard("CountlessWords"); } }
        protected Card acolytes { get { return GetCard("AcolytesOfTheBlackThorn"); } }
        protected Card bladewight { get { return GetCard("Bladewight"); } }
        protected Card ingini { get { return GetCard("Ingini"); } }
        protected Card alone { get { return GetCard("YouAreAlone"); } }

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
        public void TestDamage()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Check that isolated hero can still deal other heroes damage and be dealt damage by other heroes
            ClearInitialCards();
            PlayCard(alone);
            QuickHPStorage(ra, bunker);
            DealDamage(ra, bunker, 1, DamageType.Melee);
            DealDamage(bunker, ra, 1, DamageType.Melee);
            QuickHPCheck(-1, -1);
        }

        [Test()]
        public void TestDamageSelect()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Check that isolated hero can still be selected as a damage target by other heroes and vice versa
            ClearInitialCards();
            PlayCard(alone);
            Card flak = PlayCard("FlakCannon");

            DecisionSelectTarget = bunker.CharacterCard;
            QuickHPStorage(bunker);
            UsePower(ra);
            QuickHPCheck(-2);

            DecisionSelectTarget = ra.CharacterCard;
            QuickHPStorage(ra);
            UsePower(flak);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestPlayCard()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy/FreedomFiveLegacyCharacter", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Check that isolated hero cannot be selected to play a card
            ClearInitialCards();
            PlayCard(alone);

            AssertNextDecisionChoices(new TurnTaker[] { ra.TurnTaker, legacy.TurnTaker, scholar.TurnTaker }, new TurnTaker[] { bunker.TurnTaker });
            UsePower(legacy);
        }

        [Test()]
        public void TestPlayCardReverse()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy/FreedomFiveLegacyCharacter", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += MakeDecisions2;

            //Check that isolated hero cannot let another player play a card
            ClearInitialCards();
            SetHitPoints(legacy.CharacterCard, 10);
            PlayCard(alone);

            AssertNextDecisionChoices(new TurnTaker[] { legacy.TurnTaker }, new TurnTaker[] {ra.TurnTaker, bunker.TurnTaker , scholar.TurnTaker});
            UsePower(legacy);
        }

        [Test()]
        public void TestUsePower()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy/AmericasGreatestLegacyCharacter", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Check that isolated hero cannot be selected to use a power
            ClearInitialCards();
            PlayCard(alone);

            AssertNextDecisionChoices(new TurnTaker[] { ra.TurnTaker, legacy.TurnTaker, scholar.TurnTaker }, new TurnTaker[] { bunker.TurnTaker });
            UsePower(legacy);
        }

        [Test()]
        public void TestUsePowerReverse()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy/AmericasGreatestLegacyCharacter", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += MakeDecisions2;

            //Check that isolated hero cannot let another player use a power
            ClearInitialCards();
            SetHitPoints(legacy.CharacterCard, 10);
            PlayCard(alone);

            AssertNextDecisionChoices(new TurnTaker[] { legacy.TurnTaker }, new TurnTaker[] {ra.TurnTaker, bunker.TurnTaker, scholar.TurnTaker });
            UsePower(legacy);
        }

        [Test()]
        public void TestXtremeArgent()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheArgentAdept/XtremePrimeWardensArgentAdeptCharacter", "InsulaPrimalis");
            StartGame();

            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += MakeDecisions2;

            //Check that Xtreme Prime Wardens Argent Adept can damage the isolated hero, but doesn't let them play a card or use a power
            ClearInitialCards();
            SetHitPoints(bunker, 10);
            PlayCard(alone);

            Card ammo = PutInHand("AmmoDrop");
            Card flak = PutOnDeck("FlakCannon");
            DecisionSelectCardToPlay = ammo;
            DecisionSelectTarget = bunker.CharacterCard;

            QuickHPStorage(bunker);
            UsePower(adept);
            QuickHPCheck(-2);
            AssertInHand(ammo);
            AssertOnTopOfDeck(flak);
        }

        [Test()]
        public void TestXtremeArgentReversed()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheArgentAdept/XtremePrimeWardensArgentAdeptCharacter", "InsulaPrimalis");
            StartGame();
            SetHitPoints(adept, 10);

            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += MakeDecisions2;


            //Check that isolated Xtreme Prime Wardens Argent Adept can damage other heroes, but doesn't let them play a card or use a power
            if (!alone.IsInPlayAndHasGameText)
            {
                ClearInitialCards();
                PlayCard(alone);
            }

            Card ammo = PutInHand("AmmoDrop");
            Card flak = PutOnDeck("FlakCannon");
            DecisionSelectCardToPlay = ammo;
            DecisionSelectTarget = bunker.CharacterCard;

            QuickHPStorage(bunker);
            UsePower(adept);
            QuickHPCheck(-2);
            AssertInHand(ammo);
            AssertOnTopOfDeck(flak);
        }

        [Test()]
        public void TestDraw()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Tachyon/TachyonFreedomSixCharacter", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();
            SetHitPoints(ra, 10);

            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += MakeDecisions2;

            //Check that Team Leader Tachyon doesn't make the isolated hero draw a card
            

            if (!alone.IsInPlayAndHasGameText)
            {
                ClearInitialCards();
                PlayCard(alone);
            }

            QuickHandStorage(tachyon, legacy, bunker, ra);
            UsePower(tachyon);
            QuickHandCheck(1, 1, 1, 0);
        }

        [Test()]
        public void TestDrawReverse()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Tachyon/TachyonFreedomSixCharacter", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            SetHitPoints(tachyon, 10);

            StartGame();

            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += MakeDecisions2;

            //Check that isolated Team Leader Tachyon doesn't make other heroes draw
            if (!alone.IsInPlayAndHasGameText)
            {
                ClearInitialCards();
                PlayCard(alone);
            }

            QuickHandStorage(tachyon, legacy, bunker, ra);
            UsePower(tachyon);
            QuickHandCheck(1, 0, 0, 0);
        }

        [Test()]
        public void TestDamageIncrease()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Tachyon", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            SetHitPoints(tachyon, 10);
            StartGame();

            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += MakeDecisions2;

            //Check that damage dealt by the isolated hero is not increased by Galvanize
            if (!alone.IsInPlayAndHasGameText)
            {
                ClearInitialCards();
                PlayCard(alone);
            }
            

            Card rex = PlayCard("EnragedTRex");

            UsePower(legacy);

            QuickHPStorage(rex);
            DealDamage(tachyon, rex, 1, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(rex);
            DealDamage(legacy, rex, 1, DamageType.Melee);
            QuickHPCheck(-2);

            QuickHPStorage(rex);
            DealDamage(bunker, rex, 1, DamageType.Melee);
            QuickHPCheck(-2);

            QuickHPStorage(rex);
            DealDamage(ra, rex, 1, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestDamageIncreaseReverse()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Tachyon", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += MakeDecisions2;

            //Check that damage dealt by other heroes is not increased by Galvanize when Legacy is isolated
            ClearInitialCards();
            SetHitPoints(legacy, 10);
            PlayCard(alone);

            Card rex = PlayCard("EnragedTRex");

            UsePower(legacy);

            QuickHPStorage(rex);
            DealDamage(tachyon, rex, 1, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(rex);
            DealDamage(legacy, rex, 1, DamageType.Melee);
            QuickHPCheck(-2);

            QuickHPStorage(rex);
            DealDamage(bunker, rex, 1, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(rex);
            DealDamage(ra, rex, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }
    }
}