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
    public class MidnightBazaarTests : BaseTest
    {
        protected TurnTakerController bazaar { get { return FindEnvironment(); } }

        [Test()]
        public void TestASenseOfLoss()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            // Make sure it no longer plays an environment card when played.

            Card wolf = PutOnDeck("MrWolf");
            Card sense = PlayCard("ASenseOfLoss");
            AssertIsInPlay(sense);
            AssertOnTopOfDeck(wolf);
        }

        [Test()]
        public void TestDisquietNoPlay()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            // Make sure it no longer plays an environment card when played.

            Card wolf = PutOnDeck("MrWolf");
            Card disquiet = PlayCard("Disquiet");
            AssertIsInPlay(disquiet);
            AssertOnTopOfDeck(wolf);
        }

        [Test()]
        public void TestDisquietDamageIncrease()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            // Increase sonic and psychic damage dealt by targets by 1.

            //Check damage dealt by Ra
            Card disquiet = PlayCard("Disquiet");
            QuickHPStorage(ra);
            DealDamage(baron, ra, 1, DamageType.Sonic);
            QuickHPCheck(-2);

            QuickHPStorage(ra);
            DealDamage(baron, ra, 1, DamageType.Psychic);
            QuickHPCheck(-2);

            QuickHPStorage(ra);
            DealDamage(baron, ra, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check damage dealt by Mobile Defense Platform
            Card mdp = GetCard("MobileDefensePlatform", 0, (Card c) => c.IsInPlayAndHasGameText);
            QuickHPStorage(mdp);
            DealDamage(ra, mdp, 1, DamageType.Sonic);
            QuickHPCheck(-2);

            QuickHPStorage(mdp);
            DealDamage(ra, mdp, 1, DamageType.Psychic);
            QuickHPCheck(-2);

            QuickHPStorage(mdp);
            DealDamage(ra, mdp, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check damage dealt by a non-target
            QuickHPStorage(ra);
            DealDamage(disquiet, ra, 1, DamageType.Sonic);
            QuickHPCheck(-1);

            QuickHPStorage(ra);
            DealDamage(disquiet, ra, 1, DamageType.Psychic);
            QuickHPCheck(-1);

            QuickHPStorage(ra);
            DealDamage(disquiet, ra, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestDisquietMoveCards()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            // When a Threen would deal damage, 1 player may put 2 cards from their hand under The Empty Well to redirect that damage to a non-threen target.

            Card disquiet = PlayCard("Disquiet");
            Card well = PlayCard("TheEmptyWell");
            Card dancer = PlayCard("TheDancerAtTheDawn");
            Card mdp = GetCard("MobileDefensePlatform", 0, (Card c) => c.IsInPlayAndHasGameText);

            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionRedirectTarget = mdp;
            DecisionSelectFunction = 1; //Make sure the players don't have an option to put a card from in play under the Well

            QuickHandStorage(legacy);
            QuickHPStorage(ra.CharacterCard,mdp);
            DealDamage(dancer, ra, 2, DamageType.Melee, true);
            QuickHPCheck(0, -2);
            QuickHandCheck(-2);
            AssertNumberOfCardsAtLocation(well.UnderLocation,2);
        }

        [Test()]
        public void TestMrWolfCannotDealDamage()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //This card cannot deal damage if The Blinded Queen is in play

            Card wolf = PlayCard("MrWolf");

            QuickHPStorage(ra);
            DealDamage(wolf, ra, 2, DamageType.Melee);
            QuickHPCheck(-2);

            Card queen = PlayCard("TheBlindedQueen");

            QuickHPStorage(ra);
            DealDamage(wolf, ra, 2, DamageType.Melee);
            QuickHPCheckZero();
        }

        [Test()]
        public void TestMrWolfEndOfTurn()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //At the end of the environment turn, increase the next damage dealt by this card by 1...
            //...then this card deals the target with the lowest HP other than itself {H} melee damage.

            Card wolf = PlayCard("MrWolf");
            SetHitPoints(ra, 8);

            QuickHPStorage(ra);
            GoToEndOfTurn(bazaar);
            QuickHPCheck(-5);


            QuickHPStorage(ra);
            DealDamage(wolf, ra, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestRonaldCannotDealDamage()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //This card cannot deal damage if The Blinded Queen is in play

            Card ron = PlayCard("RedEyedRonald");

            QuickHPStorage(ra);
            DealDamage(ron, ra, 2, DamageType.Melee);
            QuickHPCheck(-2);

            Card queen = PlayCard("TheBlindedQueen");

            QuickHPStorage(ra);
            DealDamage(ron, ra, 2, DamageType.Melee);
            QuickHPCheckZero();
        }

        [Test()]
        public void TestRonaldEndOfTurn()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //At the end of the environment turn, increase the next damage dealt by this card by 2...
            //...then this card deals the hero target with the highest HP {H} melee damage.

            Card ron = PlayCard("RedEyedRonald");
            DecisionDoNotSelectTurnTaker = true;

            QuickHPStorage(legacy);
            GoToEndOfTurn(bazaar);
            QuickHPCheck(-6);


            QuickHPStorage(legacy);
            DealDamage(ron, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestRonaldRedirect()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //At the end of the environment turn, increase the next damage dealt by this card by 2...
            //...then this card deals the hero target with the highest HP {H} melee damage.

            Card ron = PlayCard("RedEyedRonald");
            Card well = PlayCard("TheEmptyWell");
            Card mdp = GetCard("MobileDefensePlatform", 0, (Card c) => c.IsInPlayAndHasGameText);
            DecisionSelectTurnTakers =  new TurnTaker[] { legacy.TurnTaker, ra.TurnTaker, ra.TurnTaker };
            DecisionRedirectTarget = mdp;

            QuickHPStorage(legacy.CharacterCard, mdp);
            QuickHandStorage(legacy);
            GoToEndOfTurn(bazaar);
            QuickHPCheck(0,-6);
            QuickHandCheck(-1);
        }

        [Test()]
        public void TestBlackDogHealReduce()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //Reduce all HP recovery by 1

            Card dog = PlayCard("TheBlackDog");
            Card mdp = GetCard("MobileDefensePlatform", 0, (Card c) => c.IsInPlayAndHasGameText);

            SetHitPoints(new Card[] { baron.CharacterCard, ra.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, tachyon.CharacterCard, dog, mdp }, 1);
            QuickHPStorage(baron.CharacterCard, ra.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, tachyon.CharacterCard, dog, mdp);
            GainHP(baron.CharacterCard,2);
            GainHP(ra.CharacterCard, 2);
            GainHP(legacy.CharacterCard, 2);
            GainHP(bunker.CharacterCard, 2);
            GainHP(tachyon.CharacterCard, 2);
            GainHP(dog, 2);
            GainHP(mdp, 2);
            QuickHPCheck(1, 1, 1, 1, 1, 1, 1);
        }

        [Test()]
        public void TestBlackDogEndOfTurn2Damage()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //At the end of the environment turn, this card deals the target other than itself with the second highest HP 2 irreducible infernal damage.
            //If 3 or more damage is dealt this way, destroy an ongoing or equipment card from that target's deck.

            Card dog = PlayCard("TheBlackDog");
            Card fortitude = PlayCard("Fortitude");

            QuickHPStorage(legacy);
            GoToEndOfTurn(bazaar);
            QuickHPCheck(-2);
            AssertIsInPlay(fortitude);
        }

        [Test()]
        public void TestBlackDogEndOfTurn3DamageHero()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tempest", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //At the end of the environment turn, this card deals the target other than itself with the second highest HP 2 irreducible infernal damage.
            //If 3 or more damage is dealt this way, destroy an ongoing or equipment card from that target's deck.

            SetHitPoints(new TurnTakerController[] {ra, legacy, bunker }, 10);
            Card dog = PlayCard("TheBlackDog");
            Card hurricane = PlayCard("LocalizedHurricane");
            Card shackles = PlayCard("GeneBoundShackles");
            Card fortitude = PlayCard("Fortitude");
            Card living = PlayCard("LivingForceField");

            QuickHPStorage(tempest);
            AssertNextDecisionChoices(new Card[] { hurricane, shackles }, new Card[] {fortitude, living});
            DecisionSelectCard = hurricane;
            GoToEndOfTurn(bazaar);
            QuickHPCheck(-3);
            AssertInTrash(hurricane);
        }

        [Test()]
        public void TestBlackDogEndOfTurn3DamageVillain()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tempest", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //At the end of the environment turn, this card deals the target other than itself with the second highest HP 2 irreducible infernal damage.
            //If 3 or more damage is dealt this way, destroy an ongoing or equipment card from that target's deck.

            SetHitPoints(new TurnTakerController[] { ra, legacy, bunker, tempest }, 5);
            Card dog = PlayCard("TheBlackDog");
            Card hurricane = PlayCard("LocalizedHurricane");
            Card shackles = PlayCard("GeneBoundShackles");
            Card fortitude = PlayCard("Fortitude");
            Card living = PlayCard("LivingForceField");
            Card mdp = GetCard("MobileDefensePlatform", 0, (Card c) => c.IsInPlayAndHasGameText);

            IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(1);
            effect.SourceCriteria.IsSpecificCard = dog;
            effect.NumberOfUses = 1;
            RunCoroutine(GameController.AddStatusEffect(effect, true, new CardSource(FindCardController(dog))));

            QuickHPStorage(mdp);
            AssertNextDecisionChoices(new Card[] { living }, new Card[] { fortitude, hurricane, shackles, mdp });
            DecisionSelectCard = living;
            GoToEndOfTurn(bazaar);
            QuickHPCheck(-3);
            AssertInTrash(living);
        }

        [Test()]
        public void TestBlindedQueenAggro()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tempest", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //The first time each turn a non-Hound target deals damage to a target other than itself, increase the next damage dealt to the source of that damage by 2."

            Card queen = PlayCard("TheBlindedQueen");
            Card mdp = GetCard("MobileDefensePlatform", 0, (Card c) => c.IsInPlayAndHasGameText);

            DealDamage(ra, legacy, 1, DamageType.Melee);

            //Check that other damage is not increased
            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that damage to Ra is increased
            QuickHPStorage(ra);
            DealDamage(legacy, ra, 1, DamageType.Melee);
            QuickHPCheck(-3);

            //Check that only the first instance is increased
            QuickHPStorage(ra);
            DealDamage(legacy, ra, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestEmptyWellCardEnteringPlay()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tempest", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //When a card would enter play, if another copy of that card is under this card, put that card under this card instead.
            Card well = PlayCard("TheEmptyWell");
            Card danger1 = MoveCard(bazaar,"DangerSense",well.UnderLocation);
            Card danger2 = PlayCard("DangerSense");
            AssertAtLocation(danger2, well.UnderLocation);
            AssertNumberOfCardsAtLocation(well.UnderLocation, 2);
        }

        [Test()]
        public void TestEmptyWellEndOfTurnDraw()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tempest", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //At the end of the environment turn, a player draws or discards 2 cards.
            //If a card is discarded this way, discard a card from under this card.
            //Otherwise a player puts a card from their hand under this card.
            Card well = PlayCard("TheEmptyWell");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionSelectFunction = 0;

            MoveAllCards(legacy, legacy.HeroTurnTaker.Hand, legacy.TurnTaker.Trash);
            Card fortitude = MoveCard(legacy, "Fortitude", legacy.HeroTurnTaker.Hand);
            Card danger = MoveCard(legacy, "DangerSense", legacy.TurnTaker.Deck);
            Card thokk = MoveCard(legacy, "Thokk", legacy.TurnTaker.Deck);
            Card staff = MoveCard(ra, "TheStaffOfRa", well.UnderLocation);
            DecisionSelectCard = fortitude;

            QuickHandStorage(legacy);
            GoToEndOfTurn(bazaar);
            QuickHandCheck(1);
            AssertInHand(danger, thokk);
            AssertAtLocation(new Card[] { fortitude, staff }, well.UnderLocation);
        }

        [Test()]
        public void TestEmptyWellEndOfTurnDiscard()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tempest", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //At the end of the environment turn, a player draws or discards 2 cards.
            //If a card is discarded this way, discard a card from under this card.
            //Otherwise a player puts a card from their hand under this card.
            Card well = PlayCard("TheEmptyWell");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionSelectFunction = 1;

            MoveAllCards(legacy, legacy.HeroTurnTaker.Hand, legacy.TurnTaker.Trash);
            Card fortitude = MoveCard(legacy, "Fortitude", legacy.HeroTurnTaker.Hand);
            Card strike = MoveCard(legacy, "BackFistStrike", legacy.HeroTurnTaker.Hand);
            Card danger = MoveCard(legacy, "DangerSense", legacy.TurnTaker.Deck);
            Card thokk = MoveCard(legacy, "Thokk", legacy.TurnTaker.Deck);
            Card staff = MoveCard(ra, "TheStaffOfRa", well.UnderLocation);

            QuickHandStorage(legacy);
            GoToEndOfTurn(bazaar);
            QuickHandCheck(-2);
            AssertInDeck(legacy, new Card[] {danger, thokk});
            AssertInTrash( fortitude, strike, staff );
        }

        [Test()]
        public void TestPoetOfThePaleNoDamage()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tempest", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //At the end of the environment turn, this card deals the non-Threen target with the lowest HP {H - 2} psychic damage.
            //If no damage is dealt this way, a player discards a card
            Card poet = PlayCard("ThePoetOfThePale");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card fortitude = PutInHand("Fortitude");
            DecisionDiscardCard = fortitude;
            AssertNextDecisionChoices(new TurnTaker[] { ra.TurnTaker, legacy.TurnTaker, bunker.TurnTaker, tempest.TurnTaker }, new TurnTaker[] { baron.TurnTaker, bazaar.TurnTaker });

            SetHitPoints(tempest, 7);

            QuickHPStorage(ra, legacy, bunker, tempest);
            QuickHandStorage(legacy);
            GoToEndOfTurn(bazaar);
            AssertInTrash(fortitude);
            QuickHPCheckZero();
            QuickHandCheck(-1);
        }

        [Test()]
        public void TestPoetOfThePaleDamage()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Bunker", "Tempest", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //At the end of the environment turn, this card deals the non-Threen target with the lowest HP {H - 2} psychic damage.
            //If no damage is dealt this way, a player discards a card
            Card poet = PlayCard("ThePoetOfThePale");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card fortitude = PutInHand("Fortitude");
            DecisionDiscardCard = fortitude;
            AssertNextDecisionChoices(new TurnTaker[] { ra.TurnTaker, legacy.TurnTaker, bunker.TurnTaker, tempest.TurnTaker }, new TurnTaker[] { baron.TurnTaker, bazaar.TurnTaker });

            MakeDamageIrreducibleStatusEffect effect = new MakeDamageIrreducibleStatusEffect();
            effect.SourceCriteria.IsSpecificCard = poet;
            effect.NumberOfUses = 1;
            RunCoroutine(GameController.AddStatusEffect(effect, true, new CardSource(FindCardController(poet))));

            SetHitPoints(tempest, 7);

            QuickHPStorage(ra, legacy, bunker, tempest);
            QuickHandStorage(legacy);
            GoToEndOfTurn(bazaar);
            AssertInHand(fortitude);
            QuickHPCheck(0,0,0,-2);
            QuickHandCheckZero();
        }

        [Test()]
        public void TestSingerInTheSilenceEndOfTurn()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Luminary", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //At the end of the environment turn, a hero deals themself 2 sonic damage, then that hero's player draws a card.
            //this card deals each non-Threen target 1 sonic damage then plays a Threen and an Unbinding from the environment trash.
            GoToStartOfTurn(bazaar);

            Card singer = PlayCard("TheSingerInTheSilence");
            Card turret = PlayCard("RegressionTurret");
            Card mdp = GetCard("MobileDefensePlatform", 0, (Card c) => c.IsInPlayAndHasGameText);

            AssertNextDecisionChoices(new Card[] { ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard, luminary.CharacterCard }, new Card[] { mdp, turret, singer, baron.CharacterCard });
            DecisionSelectCard = ra.CharacterCard;
            QuickHPStorage(ra);
            QuickHandStorage(ra);

            GoToEndOfTurn(bazaar);

            QuickHPCheck(-2);
            QuickHandCheck(1);
        }

        [Test()]
        public void TestVelkinorEndOfTurn()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Luminary", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //Increase damage dealt to targets next to this card by 1.
            //At the end of the environment turn, 1 player may move a card from their hand under [i]The Empty Well[/i] to move this card next to their hero. That hero gains the following power:",
            //"[b]Power:[/b] Your hero deals 1 target{BR}     5 irreducible melee damage.
            Card sword = PlayCard("Velkinor");
            Card well = PlayCard("TheEmptyWell");
            Card fortitude = PutInHand("Fortitude");
            Card staff = PutInHand("TheStaffOfRa");
            Card gaze = PutInHand("WrathfulGaze");

            DecisionSelectTurnTakers = new TurnTaker[] { legacy.TurnTaker, ra.TurnTaker, ra.TurnTaker};
            DecisionSelectCards = new Card[] { fortitude, staff };
            DecisionSelectFunction = 0;

            GoToEndOfTurn(bazaar);
            AssertAtLocation(fortitude, well.UnderLocation);
            AssertAtLocation(sword, legacy.CharacterCard.NextToLocation);
        }

        [Test()]
        public void TestVelkinorIncrease()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Luminary", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //Increase damage dealt to targets next to this card by 1.
            //At the end of the environment turn, 1 player may move a card from their hand under [i]The Empty Well[/i] to move this card next to their hero. That hero gains the following power:",
            //"[b]Power:[/b] Your hero deals 1 target{BR}     5 irreducible melee damage.
            Card sword = PlayCard("Velkinor");

            MoveCard(bazaar, sword, legacy.CharacterCard.NextToLocation);

            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 1, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestVelkinorPower()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Luminary", "VainFacadePlaytest.TheMidnightBazaar");
            StartGame();
            //Increase damage dealt to targets next to this card by 1.
            //At the end of the environment turn, 1 player may move a card from their hand under [i]The Empty Well[/i] to move this card next to their hero. That hero gains the following power:",
            //"[b]Power:[/b] Your hero deals 1 target{BR}     5 irreducible melee damage.
            Card sword = PlayCard("Velkinor");
            PlayCard("Fortitude");

            MoveCard(bazaar, sword, ra.CharacterCard.NextToLocation);

            QuickHPStorage(legacy);
            DecisionSelectTarget = legacy.CharacterCard;
            UsePower(ra,1);
            QuickHPCheck(-5);
        }
    }
}
