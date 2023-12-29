using NUnit.Framework;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.UnitTest;
using System.Linq;
using VainFacadePlaytest.Peacekeeper;
using Handelabra;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace VainFacadeTest
{
    [TestFixture()]
    public class PeacekeeperTests : BaseTest
    {
        protected HeroTurnTakerController peacekeeper { get { return FindHero("Peacekeeper"); } }
        protected HeroTurnTakerController friday { get { return FindHero("Friday"); } }

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(peacekeeper.CharacterCard, 1);
            DealDamage(villain, peacekeeper, 2, DamageType.Melee, true);
        }

        [Test()]
        public void TestLoadPeacekeeper()
        {
            SetupGameController("BaronBlade", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(peacekeeper);
            Assert.IsInstanceOf(typeof(PeacekeeperCharacterCardController), peacekeeper.CharacterCardController);

            Assert.AreEqual(29, peacekeeper.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestInnatePower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Peacekeeper deals 1 target 1 melee damage. You may draw a card.
            DecisionSelectTarget = akash.CharacterCard;
            Card codename = PutOnDeck("CodenamePeacekeeper");
            QuickHPStorage(akash);
            QuickHandStorage(peacekeeper);
            UsePower(peacekeeper);
            QuickHPCheck(-1);
            QuickHandCheck(1);
            AssertInHand(codename);
        }

        [Test()]
        public void TestIncap1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //The next time a hero uses a power, that hero deals 1 target 1 projectile damage.
            SetupIncap(akash);
            UseIncapacitatedAbility(peacekeeper, 0);
            DecisionSelectTarget = akash.CharacterCard;

            QuickHPStorage(akash);
            UsePower(bunker);
            QuickHPCheck(-1);

            QuickHPStorage(akash);
            UsePower(scholar);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestIncap1Multiple()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //The next time a hero uses a power, that hero deals 1 target 1 projectile damage.
            SetupIncap(akash);
            UseIncapacitatedAbility(peacekeeper, 0);
            UseIncapacitatedAbility(peacekeeper, 0);
            DecisionSelectTarget = akash.CharacterCard;

            QuickHPStorage(akash);
            UsePower(bunker);
            QuickHPCheck(-2);

            QuickHPStorage(akash);
            UsePower(scholar);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestIncap2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Select a hero target. Reduce the next damage dealt to that target by 2.
            SetupIncap(akash);
            DecisionSelectCard = bunker.CharacterCard;
            UseIncapacitatedAbility(peacekeeper, 1);

            QuickHPStorage(bunker);
            DealDamage(akash, bunker, 3, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(bunker);
            DealDamage(akash, bunker, 3, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestIncap2Multiple()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Select a hero target. Reduce the next damage dealt to that target by 2.
            SetupIncap(akash);
            DecisionSelectCard = bunker.CharacterCard;
            UseIncapacitatedAbility(peacekeeper, 1);
            UseIncapacitatedAbility(peacekeeper, 1);

            QuickHPStorage(bunker);
            DealDamage(akash, bunker, 5, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(bunker);
            DealDamage(akash, bunker, 5, DamageType.Melee);
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestIncap3()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //1 player may play a card.
            SetupIncap(akash);
            Card fortitude = PutInHand("Fortitude");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionSelectCardToPlay = fortitude;
            UseIncapacitatedAbility(peacekeeper, 2);
            AssertIsInPlay(fortitude);
        }

        [Test()]
        public void TestManeuverPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When this card enters play, return your other maneuvers in play to your hand.
            Card ambush = PlayCard("Ambush");

            Card bullet = PlayCard("BulletHell");
            AssertIsInPlay(bullet);
            AssertInHand(ambush);

            Card cover = PlayCard("CoverFire");
            AssertIsInPlay(cover);
            AssertInHand(bullet);

            Card sniper = PlayCard("SniperPerch");
            AssertIsInPlay(sniper);
            AssertInHand(cover);

            Card upclose = PlayCard("UpCloseAndPersonal");
            AssertIsInPlay(upclose);
            AssertInHand(sniper);

            PlayCard(ambush);
            AssertIsInPlay(ambush);
            AssertInHand(upclose);
        }

        [Test()]
        public void TestSerumPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When this card enters play, destroy your other serums.
            Card booster = PlayCard("X517BoosterShot");

            Card YTech = PlayCard("YTechCustomSerum4319B");
            AssertInTrash(booster);
            AssertIsInPlay(YTech);

            PlayCard(booster);
            AssertInTrash(YTech);
            AssertIsInPlay(booster);
        }

        [Test()]
        public void TestSymptomEndOfTurn()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //At the end of your turn, {Peacekeeper} deals himself 1 irreducible toxic damage.
            Card defensive = PlayCard("DefensiveDisplacement");
            Card toxic = PlayCard("ToxicBlood");
            Card green = PlayCard("GreenVeins");
            Card excess = PlayCard("ExcessAdrenaline");

            DecisionSelectFunction = 1;
            QuickHPStorage(peacekeeper);
            GoToEndOfTurn(peacekeeper);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestAmbush1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Prevent the first damage dealt to {Peacekeeper} by non-hero targets each turn. When {Peacekeeper} deals or is dealt damage by another target, destroy this card.
            Card ambush = PlayCard("Ambush");

            //Check that it prevents the first damage from Akash'bhuta, does not prevent the second, and destroys itself when dealt damage
            QuickHPStorage(peacekeeper);
            DealDamage(akash, peacekeeper, 1, DamageType.Melee);
            QuickHPCheck(0);
            AssertIsInPlay(ambush);

            GoToStartOfTurn(peacekeeper);

            QuickHPStorage(peacekeeper);
            DealDamage(akash, peacekeeper, 1, DamageType.Melee);
            QuickHPCheck(0);
            AssertIsInPlay(ambush);

            QuickHPStorage(peacekeeper);
            DealDamage(akash, peacekeeper, 1, DamageType.Melee);
            QuickHPCheck(-1);
            AssertInTrash(ambush);
        }

        [Test()]
        public void TestAmbush2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Prevent the first damage dealt to {Peacekeeper} by non-hero targets each turn. When {Peacekeeper} deals or is dealt damage by another target, destroy this card.
            Card ambush = PlayCard("Ambush");

            //Check that damage from a hero target is not prevented, and it destroys itself when dealt damage
            QuickHPStorage(peacekeeper);
            DealDamage(legacy, peacekeeper, 1, DamageType.Melee);
            QuickHPCheck(-1);
            AssertInTrash(ambush);
        }

        [Test()]
        public void TestAmbush3()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Prevent the first damage dealt to {Peacekeeper} by non-hero targets each turn. When {Peacekeeper} deals or is dealt damage by another target, destroy this card.
            Card ambush = PlayCard("Ambush");

            //Check that damage from Peacekeeper does not cause Ambush to destroy itself
            QuickHPStorage(peacekeeper);
            DealDamage(peacekeeper, peacekeeper, 1, DamageType.Melee);
            QuickHPCheck(-1);
            AssertIsInPlay(ambush);
        }

        [Test()]
        public void TestAmbush4()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Prevent the first damage dealt to {Peacekeeper} by non-hero targets each turn. When {Peacekeeper} deals or is dealt damage by another target, destroy this card.
            Card ambush = PlayCard("Ambush");

            //Check that if Peacekeeper deals damage to another target, Ambush is destroyed
            DealDamage(peacekeeper, akash, 1, DamageType.Melee);
            AssertInTrash(ambush);
        }

        [Test()]
        public void TestAmbush5()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Prevent the first damage dealt to {Peacekeeper} by non-hero targets each turn. When {Peacekeeper} deals or is dealt damage by another target, destroy this card.
            Card ambush = PlayCard("Ambush");

            //Check that if Peacekeeper deals damage to another hero target, Ambush is destroyed
            DealDamage(peacekeeper, legacy, 1, DamageType.Melee);
            AssertInTrash(ambush);
        }

        [Test()]
        public void TestAmbushPower1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Power: Increase damage {Peacekeeper} deals to non-hero targets by 2 until this card leaves play.
            Card ambush = PlayCard("Ambush");

            UsePower(ambush);
            //Check that damage to Akash'bhuta is increased by 2
            QuickHPStorage(akash);
            DealDamage(peacekeeper, akash, 1, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestAmbushPower2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Power: Increase damage {Peacekeeper} deals to non-hero targets by 2 until this card leaves play.
            Card ambush = PlayCard("Ambush");

            UsePower(ambush);
            //Check that damage to Legacy is not increased
            QuickHPStorage(legacy);
            DealDamage(peacekeeper, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestAmbushPower3()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Power: Increase damage {Peacekeeper} deals to non-hero targets by 2 until this card leaves play.
            Card ambush = PlayCard("Ambush");

            UsePower(ambush);
            UsePower(ambush);
            //Check that it stacks
            QuickHPStorage(akash);
            DealDamage(peacekeeper, akash, 1, DamageType.Melee);
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestBulletHell()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Prevent the first card draw during your draw phase.
            Card bullet = PlayCard("BulletHell");

            //Check that card draw is not prevented during other phases
            GoToPlayCardPhase(peacekeeper);
            QuickHandStorage(peacekeeper);
            DrawCard(peacekeeper);
            QuickHandCheck(1);

            //Check that first card draw is prevented during draw phase
            QuickHandStorage(peacekeeper);
            GoToDrawCardPhase(peacekeeper);
            DrawCard(peacekeeper);
            QuickHandCheck(0);
            QuickHandStorage(peacekeeper);
            DrawCard(peacekeeper);
            QuickHandCheck(1);
        }

        [Test()]
        public void TestBulletHellPower1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Power: Until the start of your next turn, the first time each turn any target deals damage, {Peacekeeper} may discard a card to deal 1 target 1 projectile damage.
            Card bullet = PlayCard("BulletHell");
            GoToUsePowerPhase(peacekeeper);
            UsePower(bullet);
            Card codename = PutInHand("CodenamePeacekeeper");
            Card ambush = PutInHand("Ambush");

            //Check that it works
            DecisionDiscardCard = codename;
            DecisionSelectTarget = akash.CharacterCard;
            AssertDecisionIsOptional(SelectionType.DiscardCard);
            QuickHPStorage(akash);
            DealDamage(legacy, akash,1,DamageType.Melee);
            QuickHPCheck(-2);
            AssertInTrash(codename);

            //Check that it doesn't work twice in one turn
            DecisionDiscardCard = ambush;
            QuickHPStorage(akash);
            DealDamage(legacy, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);
            AssertInHand(ambush);

            //Check that it does work on the next turn, and that it works on any damage
            GoToStartOfTurn(legacy);
            QuickHPStorage(akash);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);
            AssertInTrash(ambush);

            //Check that you don't deal damage if you don't discard
            GoToStartOfTurn(bunker);
            DecisionDoNotSelectCard = SelectionType.DiscardCard;
            QuickHPStorage(akash);
            QuickHandStorage(peacekeeper);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(0);
            QuickHandCheck(0);

            //Check that it expires at the start of Peacekeeper's turn
            GoToStartOfTurn(peacekeeper);
            Card sniper = PutInHand("SniperPerch");
            ResetDecisions();
            DecisionDiscardCard = sniper;
            QuickHPStorage(akash);
            DealDamage(legacy, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);
            AssertInHand(sniper);
        }

        [Test()]
        public void TestBulletHellPower1Reduced()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Power: Until the start of your next turn, the first time each turn any target deals damage, {Peacekeeper} may discard a card to deal 1 target 1 projectile damage.
            Card bullet = PlayCard("BulletHell");
            GoToUsePowerPhase(peacekeeper);
            UsePower(bullet);

            //Check that if damage is reduced to 0, Bullet Hell does not react
            PlayCard("Fortitude");
            QuickHPStorage(akash);
            DecisionSelectTarget = akash.CharacterCard;
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestBulletHellPower1Multiple()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Power: Until the start of your next turn, the first time each turn any target deals damage, {Peacekeeper} may discard a card to deal 1 target 1 projectile damage.
            Card bullet = PlayCard("BulletHell");
            GoToUsePowerPhase(peacekeeper);
            UsePower(bullet);
            GoToStartOfTurn(legacy);
            UsePower(bullet);

            //When the power is used multiple times per round, it should let Peacekeeper discard and damage twice
            MoveAllCards(peacekeeper, peacekeeper.HeroTurnTaker.Hand, peacekeeper.TurnTaker.Trash);
            Card sniper = PutInHand("SniperPerch");
            Card ambush = PutInHand("Ambush");
            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(akash);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(-2);
            AssertInTrash(sniper, ambush);
        }

        

        [Test()]
        public void TestBulletHellPower2Draw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Power: Draw a card or destroy this card.
            Card bullet = PlayCard("BulletHell");
            Card ambush = PutOnDeck("Ambush");
            QuickHandStorage(peacekeeper);
            DecisionSelectFunction = 0;
            UsePower(bullet, 1);
            AssertIsInPlay(bullet);
            AssertInHand(ambush);
            QuickHandCheck(1);
        }

        [Test()]
        public void TestBulletHellPower2Destroy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Power: Draw a card or destroy this card.
            Card bullet = PlayCard("BulletHell");
            DecisionSelectFunction = 1;
            UsePower(bullet, 1);
            AssertInTrash(bullet);
        }

        [Test()]
        public void TestBurstOfSpeed()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //You may draw 1 card and play 1 card. {Peacekeeper} deals himself 2 irreducible toxic damage.
            //If {Peacekeeper} was dealt damage this way, you may play 1 card.
            MoveAllCards(peacekeeper, peacekeeper.HeroTurnTaker.Hand, peacekeeper.TurnTaker.Trash);
            Card sniper = PutOnDeck("SniperPerch");
            Card frag = PutInHand("FragGrenades");
            DecisionSelectCardToPlay = sniper;

            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;

            AssertDecisionIsOptional(SelectionType.DrawCard);
            QuickHPStorage(peacekeeper);
            Card burst = PlayCard("BurstOfSpeed");
            AssertIsInPlay(sniper);
            AssertIsInPlay(frag);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestBurstOfSpeed2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //You may draw 1 card and play 1 card. {Peacekeeper} deals himself 2 irreducible toxic damage.
            //If {Peacekeeper} was dealt damage this way, you may play 1 card.
            MoveAllCards(peacekeeper, peacekeeper.HeroTurnTaker.Hand, peacekeeper.TurnTaker.Trash);
            Card sniper = PutOnDeck("SniperPerch");
            Card frag = PutInHand("FragGrenades");
            DecisionSelectCardToPlay = sniper;

            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;

            AssertDecisionIsOptional(SelectionType.PlayCard);
            QuickHPStorage(peacekeeper);
            Card burst = PlayCard("BurstOfSpeed");
            AssertIsInPlay(sniper);
            AssertIsInPlay(frag);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestBurstOfSpeedNoDamage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //You may draw 1 card and play 1 card. {Peacekeeper} deals himself 2 irreducible toxic damage.
            //If {Peacekeeper} was dealt damage this way, you may play 1 card.
            MoveAllCards(peacekeeper, peacekeeper.HeroTurnTaker.Hand, peacekeeper.TurnTaker.Trash);
            Card sniper = PutOnDeck("SniperPerch");
            Card frag = PutInHand("FragGrenades");
            DecisionSelectCardToPlay = sniper;

            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;

            PlayCard("HeroicInterception");

            QuickHPStorage(peacekeeper);
            Card burst = PlayCard("BurstOfSpeed");
            AssertIsInPlay(sniper);
            AssertInHand(frag);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestCodenamePeacekeeper()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //{Peacekeeper} deals 1 target X melee damage, where X = the number of symptoms in play.
            //{Peacekeeper} deals himself 2 toxic damage. If he is dealt damage this way, he deals the original target 3 melee damage.

            Card excess = PlayCard("ExcessAdrenaline");
            Card green = PlayCard("GreenVeins");
            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(peacekeeper, akash);

            Card codename = PlayCard("CodenamePeacekeeper");
            QuickHPCheck(-2, -5);
        }

        [Test()]
        public void TestCodenamePeacekeeperNoSelfDamage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //{Peacekeeper} deals 1 target X melee damage, where X = the number of symptoms in play.
            //{Peacekeeper} deals himself 2 toxic damage. If he is dealt damage this way, he deals the original target 3 melee damage.

            Card excess = PlayCard("ExcessAdrenaline");
            Card green = PlayCard("GreenVeins");
            PlayCard("HeroicInterception");
            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(peacekeeper, akash);

            Card codename = PlayCard("CodenamePeacekeeper");
            QuickHPCheck(0, -2);
        }

        [Test()]
        public void TestCodenamePeacekeeperNoDamage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //{Peacekeeper} deals 1 target X melee damage, where X = the number of symptoms in play.
            //{Peacekeeper} deals himself 2 toxic damage. If he is dealt damage this way, he deals the original target 3 melee damage.

            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(peacekeeper, akash);

            //If no damage was dealt, Peacekeeper should still deal the extra 3 damage to the target
            Card codename = PlayCard("CodenamePeacekeeper");
            QuickHPCheck(-2, -3);
        }

        [Test()]
        public void TestCodenamePeacekeeperReduce()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //{Peacekeeper} deals 1 target X melee damage, where X = the number of symptoms in play.
            //{Peacekeeper} deals himself 2 toxic damage. If he is dealt damage this way, he deals the original target 3 melee damage.

            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(peacekeeper, akash);
            Card excess = PlayCard("ExcessAdrenaline");
            Card carapace = PlayCard("MountainousCarapace");

            //If damage is reduced to 0, Peacekeeper should still deal the extra 3 damage to the target
            Card codename = PlayCard("CodenamePeacekeeper");
            QuickHPCheck(-2, -2);
        }

        [Test()]
        public void TestCodenamePeacekeeperPrevent()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "VoidGuardWrithe/VoidGuardWritheCosmicInventor", "TheBlock");
            StartGame();

            //{Peacekeeper} deals 1 target X melee damage, where X = the number of symptoms in play.
            //{Peacekeeper} deals himself 2 toxic damage. If he is dealt damage this way, he deals the original target 3 melee damage.

            DecisionSelectTarget = voidWrithe.CharacterCard;
            QuickHPStorage(peacekeeper, voidWrithe);
            DecisionSelectCard = voidWrithe.CharacterCard;
            UsePower(voidWrithe);
            Card excess = PlayCard("ExcessAdrenaline");
            Card green = PlayCard("GreenVeins");

            //If the damage was prevented, Peacekeeper should still deal the extra 3 damage to the target
            PlayCard("CodenamePeacekeeper");
            QuickHPCheck(-2, -3);
        }

        [Test()]
        public void TestCodenamePeacekeeperRedirect()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "VoidGuardWrithe", "TheBlock");
            StartGame();

            //{Peacekeeper} deals 1 target X melee damage, where X = the number of symptoms in play.
            //{Peacekeeper} deals himself 2 toxic damage. If he is dealt damage this way, he deals the original target 3 melee damage.
            PlayCard("TheShadowCloak");
            PlayCard("LiesOfTheShadows");
            Card excess = PlayCard("ExcessAdrenaline");
            Card green = PlayCard("GreenVeins");
            DecisionSelectCard = akash.CharacterCard;
            DecisionYesNo = true;
            DecisionSelectTarget = legacy.CharacterCard;
            QuickHPStorage(peacekeeper, legacy, akash);

            //If the damage was redirected, the extra damage is dealt to the final target
            PlayCard("CodenamePeacekeeper");
            QuickHPCheck(-2, 0, -5);
        }

        [Test()]
        public void TestCodenamePeacekeeperRedirectAndReduce()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "VoidGuardWrithe", "TheBlock");
            StartGame();

            //{Peacekeeper} deals 1 target X melee damage, where X = the number of symptoms in play.
            //{Peacekeeper} deals himself 2 toxic damage. If he is dealt damage this way, he deals the original target 3 melee damage.
            PlayCard("TheShadowCloak");
            PlayCard("LiesOfTheShadows");
            Card excess = PlayCard("ExcessAdrenaline");
            Card carapace = PlayCard("MountainousCarapace");
            DecisionSelectCard = akash.CharacterCard;
            DecisionYesNo = true;
            DecisionSelectTarget = legacy.CharacterCard;
            QuickHPStorage(peacekeeper, legacy, akash);

            //If the damage was redirected and then reduced to 0, the extra damage is dealt to the final target
            PlayCard("CodenamePeacekeeper");
            QuickHPCheck(-2, 0, -2);
        }

        [Test()]
        public void TestCoverFire()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //At the start of your turn, destroy this card.
            Card cover = PlayCard("CoverFire");
            GoToStartOfTurn(peacekeeper);
            AssertInTrash(cover);
        }

        [Test()]
        public void TestCoverFirePower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Power: Until the start of your next turn, the first time each turn any hero target would be dealt damage by another target, reduce that damage by 1, then {Peacekeeper} deals the source of that damage 2 projectile damage.
            GoToPlayCardPhase(peacekeeper);
            Card cover = PlayCard("CoverFire");
            UsePower(cover);

            //Check that it works
            QuickHPStorage(legacy, akash);
            DealDamage(akash, legacy, 2, DamageType.Melee);
            QuickHPCheck(-1, -2);

            //Check that it doesn't work a second time
            QuickHPStorage(legacy, akash);
            DealDamage(akash, legacy, 2, DamageType.Melee);
            QuickHPCheck(-2, 0);

            GoToStartOfTurn(legacy);

            //Check that it works on the next turn
            QuickHPStorage(legacy, akash);
            DealDamage(akash, legacy, 2, DamageType.Melee);
            QuickHPCheck(-1, -2);

            GoToStartOfTurn(bunker);

            //Check that it doesn't react to damage to a non-hero target
            QuickHPStorage(legacy, akash);
            DealDamage(legacy, akash, 2, DamageType.Melee);
            QuickHPCheck(0, -2);

            GoToStartOfTurn(peacekeeper);

            //Check that it expires at the start of Peacekeeper's turn
            QuickHPStorage(legacy, akash);
            DealDamage(akash, legacy, 2, DamageType.Melee);
            QuickHPCheck(-2, 0);
        }

        [Test()]
        public void TestCoverFirePowerMultiple()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Power: Until the start of your next turn, the first time each turn any hero target would be dealt damage by another target, reduce that damage by 1, then {Peacekeeper} deals the source of that damage 2 projectile damage.
            GoToPlayCardPhase(peacekeeper);
            Card cover = PlayCard("CoverFire");
            UsePower(cover);
            GoToStartOfTurn(legacy);
            UsePower(cover);

            //If the power is used twice in one round, it should reduce damage by 2 and have Peacekeeper deal 2 instances of 2 damage
            QuickHPStorage(legacy, akash);
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-1, -4);

            QuickHPStorage(legacy, akash);
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-3, 0);
        }

        [Test()]
        public void TestExcessAdrenalineDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //At the end of your turn, you may draw or play a card.
            Card adrenaline = PlayCard("ExcessAdrenaline");
            Card frag = PutOnDeck("FragGrenades");
            DecisionSelectFunction = 1;
            QuickHandStorage(peacekeeper);
            GoToEndOfTurn(peacekeeper);
            AssertInHand(frag);
            QuickHandCheck(1);
        }

        [Test()]
        public void TestExcessAdrenalinePlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //At the end of your turn, you may draw or play a card.
            Card adrenaline = PlayCard("ExcessAdrenaline");
            Card frag = PutInHand("FragGrenades");
            DecisionSelectFunction = 0;
            QuickHandStorage(peacekeeper);
            DecisionSelectCardToPlay = frag;
            GoToEndOfTurn(peacekeeper);
            AssertIsInPlay(frag);
            QuickHandCheck(-1);
        }

        [Test()]
        public void TestFragGrenades()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //{Peacekeeper} deals up to 3 targets 2 fire damage each.
            Card frag = PlayCard("FragGrenades");
            Card rockslide = PlayCard("LivingRockslide");
            Card arboreal = PlayCard("ArborealPhalanges");
            DecisionSelectTargets = new Card[] { akash.CharacterCard, rockslide, arboreal };
            QuickHPStorage(akash.CharacterCard, rockslide, arboreal);
            UsePower(frag);
            QuickHPCheck(-2, -2, -2);
        }

        [Test()]
        public void TestGreenVeins()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //The first time each turn damage is redirected from {Peacekeeper} to another target, draw a card.
            //When {Peacekeeper} would be dealt damage by any non-hero target, you may discard 2 cards to redirect that damage to another target.
            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;
            Card green = PlayCard("GreenVeins");
            Card ambush = PutInHand("Ambush");
            Card codename = PutInHand("CodenamePeacekeeper");
            Card sniper = PutOnDeck("SniperPerch");
            DecisionSelectCards = new Card[] { ambush, codename };
            DecisionYesNo = true;
            DecisionSelectTarget = akash.CharacterCard;

            //Check that it works
            QuickHPStorage(peacekeeper, akash);
            DealDamage(akash, peacekeeper, 1, DamageType.Melee);
            AssertInTrash(ambush, codename);
            AssertInHand(sniper);
            QuickHPCheck(0,-1);

            //Check that you can redirect a second time, but you do not draw a second card
            PutInHand(ambush);
            PutInHand(codename);
            Card frag = PutOnDeck("FragGrenades");
            QuickHPStorage(peacekeeper, akash);
            DealDamage(akash, peacekeeper, 1, DamageType.Melee);
            AssertInTrash(ambush, codename);
            AssertOnTopOfDeck(frag);
            QuickHPCheck(0, -1);
        }

        [Test()]
        public void TestGreenVeinsMultiple()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //The first time each turn damage is redirected from {Peacekeeper} to another target, draw a card.
            //When {Peacekeeper} would be dealt damage by any non-hero target, you may discard 2 cards to redirect that damage to another target.
            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;
            Card green = PlayCard("GreenVeins");
            Card green2 = GetCard("GreenVeins", 0, (Card c) => !c.IsInPlayAndHasGameText);
            PlayCard(green2);
            Card ambush = PutInHand("Ambush");
            Card codename = PutInHand("CodenamePeacekeeper");
            Card sniper = PutOnDeck("SniperPerch");
            Card frag = PutOnDeck("FragGrenades");
            DecisionSelectCards = new Card[] { ambush, codename };
            DecisionYesNo = true;
            DecisionSelectTarget = akash.CharacterCard;

            //Check that you draw 2 cards
            QuickHPStorage(peacekeeper, akash);
            DealDamage(akash, peacekeeper, 1, DamageType.Melee);
            AssertInTrash(ambush, codename);
            AssertInHand(sniper);
            AssertInHand(frag);
            QuickHPCheck(0, -1);

            //Check that you can redirect a second time, but you do not draw a second card
            PutInHand(ambush);
            PutInHand(codename);
            Card peace = PutOnDeck("IfYouDesirePeace");
            Card war = PutOnDeck("PrepareForWar");
            QuickHPStorage(peacekeeper, akash);
            DealDamage(akash, peacekeeper, 1, DamageType.Melee);
            AssertInTrash(ambush, codename);
            AssertOnTopOfDeck(peace,1);
            AssertOnTopOfDeck(war);
            QuickHPCheck(0, -1);
        }

        [Test()]
        public void TestGreenVeinsOtherRedirect()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheEnclaveOfTheEndlings");
            StartGame();

            //The first time each turn damage is redirected from {Peacekeeper} to another target, draw a card.
            //When {Peacekeeper} would be dealt damage by any non-hero target, you may discard 2 cards to redirect that damage to another target.
            Card green = PlayCard("GreenVeins");
            PutOnDeck("AlliesOfTheEarth");
            Card immutus = PlayCard("Immutus");
            Card frag = PutOnDeck("FragGrenades");
            DecisionSelectCard = immutus;

            DealDamage(akash, peacekeeper, 1, DamageType.Melee);
            AssertInHand(frag);

            //Check that if damage is redirected a second time, you do not draw a second card
            Card peace = PutOnDeck("IfYouDesirePeace");
            DealDamage(akash, peacekeeper, 1, DamageType.Melee);
            AssertOnTopOfDeck(peace);
        }

        [Test()]
        public void TestGreenVeinsHeroDamage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //The first time each turn damage is redirected from {Peacekeeper} to another target, draw a card.
            //When {Peacekeeper} would be dealt damage by any non-hero target, you may discard 2 cards to redirect that damage to another target.
            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;
            Card green = PlayCard("GreenVeins");
            Card ambush = PutInHand("Ambush");
            Card codename = PutInHand("CodenamePeacekeeper");
            Card sniper = PutOnDeck("SniperPerch");
            DecisionSelectCards = new Card[] { ambush, codename };
            DecisionYesNo = true;
            DecisionSelectTarget = akash.CharacterCard;

            //Check that you can't redirect damage from hero targets
            QuickHPStorage(peacekeeper, akash);
            DealDamage(legacy, peacekeeper, 1, DamageType.Melee);
            AssertInHand(ambush, codename);
            AssertOnTopOfDeck(sniper);
            QuickHPCheck(-1, 0);

            //Check that it works after being dealt damage by a hero target
            QuickHPStorage(peacekeeper, akash);
            DealDamage(akash, peacekeeper, 1, DamageType.Melee);
            AssertInTrash(ambush, codename);
            AssertInHand(sniper);
            QuickHPCheck(0, -1);
        }

        [Test()]
        public void TestGreenVeinsOtherRedirectheroDamage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheEnclaveOfTheEndlings");
            StartGame();

            //The first time each turn damage is redirected from {Peacekeeper} to another target, draw a card.
            //When {Peacekeeper} would be dealt damage by any non-hero target, you may discard 2 cards to redirect that damage to another target.
            Card green = PlayCard("GreenVeins");
            PutOnDeck("AlliesOfTheEarth");
            Card immutus = PlayCard("Immutus");
            Card frag = PutOnDeck("FragGrenades");
            DecisionSelectCard = immutus;

            //Peacekeeper should draw when damage from a hero target is redirected by another card
            DealDamage(legacy, peacekeeper, 1, DamageType.Melee);
            AssertInHand(frag);

            //Check that if damage is redirected a second time, you do not draw a second card
            Card peace = PutOnDeck("IfYouDesirePeace");
            DealDamage(legacy, peacekeeper, 1, DamageType.Melee);
            AssertOnTopOfDeck(peace);
        }

        [Test()]
        public void TestIfYouDesirePeacePlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Reveal cards from the top of your deck until 2 equipment cards are revealed. Put one into your hand or into play. Shuffle the other revealed cards into your deck.
            Card booster = PutOnDeck("X517BoosterShot");
            Card frag = PutOnDeck("FragGrenades");
            Card rifle = PutOnDeck("S399ModifiedAssaultRifle");
            Card ambush = PutOnDeck("Ambush");
            DecisionMoveCardDestination = new MoveCardDestination(peacekeeper.TurnTaker.PlayArea);
            DecisionSelectCard = frag;
            QuickShuffleStorage(peacekeeper);
            AssertNextDecisionChoices(new Card[] { rifle, frag }, new Card[] { ambush, booster });

            PlayCard("IfYouDesirePeace");
            AssertIsInPlay(frag);
            AssertInDeck(rifle);
            AssertInDeck(ambush);
            QuickShuffleCheck(1);
        }

        [Test()]
        public void TestIfYouDesirePeaceHand()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Reveal cards from the top of your deck until 2 equipment cards are revealed. Put one into your hand or into play. Shuffle the other revealed cards into your deck.
            Card booster = PutOnDeck("X517BoosterShot");
            Card frag = PutOnDeck("FragGrenades");
            Card rifle = PutOnDeck("S399ModifiedAssaultRifle");
            Card ambush = PutOnDeck("Ambush");
            DecisionMoveCardDestination = new MoveCardDestination(peacekeeper.HeroTurnTaker.Hand);
            DecisionSelectCard = frag;
            QuickShuffleStorage(peacekeeper);
            AssertNextDecisionChoices(new Card[] { rifle, frag }, new Card[] { ambush, booster });

            PlayCard("IfYouDesirePeace");
            AssertInHand(frag);
            AssertInDeck(rifle);
            AssertInDeck(ambush);
            QuickShuffleCheck(1);
        }

        [Test()]
        public void TestNBE2CombatKnife()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Increase melee damage dealt by {Peacekeeper} by 1.
            Card knife = PlayCard("NBE2CombatKnife");
            QuickHPStorage(akash);
            DealDamage(peacekeeper, akash, 1, DamageType.Melee);
            QuickHPCheck(-2);
            QuickHPStorage(akash);
            DealDamage(peacekeeper, akash, 1, DamageType.Projectile);
            QuickHPCheck(-1);
            QuickHPStorage(peacekeeper);
            DealDamage(akash, peacekeeper, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestNBE2CombatKnifePower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Power: {Peacekeeper} deals 1 target 2 melee damage. If that target dealt {Peacekeeper} melee damage since the start of your last turn, increase that damage by 2.
            Card knife = PlayCard("NBE2CombatKnife");
            DecisionSelectTarget = akash.CharacterCard;
            GoToUsePowerPhase(peacekeeper);
            QuickHPStorage(akash);
            UsePower(knife);
            QuickHPCheck(-3);

            DealDamage(akash, peacekeeper, 1, DamageType.Melee);
            GoToStartOfTurn(bunker);
            QuickHPStorage(akash);
            UsePower(knife);
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestNBE2CombatKnifePower2()
        {
            SetupGameController("AkashBhuta", "Legacy", "Bunker", "TheScholar", "VainFacadePlaytest.Peacekeeper", "TheBlock");
            StartGame();

            //Power: {Peacekeeper} deals 1 target 2 melee damage. If that target dealt {Peacekeeper} melee damage since the start of your last turn, increase that damage by 2.
            Card knife = PlayCard("NBE2CombatKnife");
            DecisionSelectTarget = akash.CharacterCard;
            GoToUsePowerPhase(peacekeeper);
            QuickHPStorage(akash);
            UsePower(knife);
            QuickHPCheck(-3);

            DealDamage(akash, peacekeeper, 1, DamageType.Melee);
            GoToStartOfTurn(bunker);
            QuickHPStorage(akash);
            UsePower(knife);
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestNBE2CombatKnifePowerCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "VainFacadePlaytest.Friday", "TheBlock");
            StartGame();

            //Power: {Peacekeeper} deals 1 target 2 melee damage. If that target dealt {Peacekeeper} melee damage since the start of your last turn, increase that damage by 2.
            Card knife = PlayCard("NBE2CombatKnife");
            DecisionSelectCard = knife;
            Card tech = PlayCard("TechnologicalDuplication");

            DecisionSelectTarget = akash.CharacterCard;
            GoToUsePowerPhase(friday);
            QuickHPStorage(akash);
            UsePower(friday,1);
            QuickHPCheck(-3);

            DealDamage(akash, friday, 1, DamageType.Melee);
            GoToStartOfTurn(bunker);
            QuickHPStorage(akash);
            UsePower(friday,1);
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestPrepareForWar()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Search your deck for a maneuver and put it into play.
            //You may put a maneuver from your trash on top of your deck.
            //Shuffle your deck.
            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;
            Card ambush = PutOnDeck("Ambush");
            Card sniper = PutInTrash("SniperPerch");
            ShuffleDeck(peacekeeper);
            DecisionSelectCard = ambush;

            QuickShuffleStorage(peacekeeper);
            PlayCard("PrepareForWar");
            AssertIsInPlay(ambush);
            AssertInDeck(sniper);
            QuickShuffleCheck(1);
        }

        [Test()]
        public void TestS399ModifiedAssaultRifle()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Increase projectile damage dealt by {Peacekeeper} by 1.
            Card rifle = PlayCard("S399ModifiedAssaultRifle");
            QuickHPStorage(akash);
            DealDamage(peacekeeper, akash, 1, DamageType.Projectile);
            QuickHPCheck(-2);
            QuickHPStorage(akash);
            DealDamage(peacekeeper, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);
            QuickHPStorage(peacekeeper);
            DealDamage(akash, peacekeeper, 1, DamageType.Projectile);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestS399ModifiedAssaultRiflePower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //{Peacekeeper} deals 1 target 2 projectile damage, then deals 1 target 2 projectile damage.
            Card rifle = PlayCard("S399ModifiedAssaultRifle");
            Card rockslide = PlayCard("LivingRockslide");
            DecisionSelectTargets = new Card[] { akash.CharacterCard, rockslide };
            QuickHPStorage(akash.CharacterCard, rockslide);
            UsePower(rifle);
            QuickHPCheck(-3, -3);
        }

        [Test()]
        public void TestSniperPerch1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Reduce damage dealt to {Peacekeeper} by 2. When {Peacekeeper} is dealt damage by another target or you use a power on another card, destroy this card.
            Card sniper = PlayCard("SniperPerch");

            //Check that damage from Akash'Bhuta is reduced, and Sniper Perch destroys itself
            QuickHPStorage(peacekeeper);
            DealDamage(akash, peacekeeper, 3, DamageType.Melee);
            QuickHPCheck(-1);
            AssertInTrash(sniper);
        }

        [Test()]
        public void TestSniperPerch2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Reduce damage dealt to {Peacekeeper} by 2. When {Peacekeeper} is dealt damage by another target or you use a power on another card, destroy this card.
            Card sniper = PlayCard("SniperPerch");

            //Check that Sniper Perch does not destroy itself if the damage is reduced to 0
            QuickHPStorage(peacekeeper);
            DealDamage(akash, peacekeeper, 2, DamageType.Melee);
            QuickHPCheck(0);
            AssertIsInPlay(sniper);

            //Check that Sniper Perch does not destroy itself when Peacekeeper deals himself damage
            QuickHPStorage(peacekeeper);
            DealDamage(peacekeeper, peacekeeper, 3, DamageType.Melee);
            QuickHPCheck(-1);
            AssertIsInPlay(sniper);

            //Check that Sniper Perch destroys itself when Peacekeeper uses his innate power
            UsePower(peacekeeper);
            AssertInTrash(sniper);
        }

        [Test()]
        public void TestSniperPerchPower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //{Peacekeeper} deals 1 target 3 irreducible projectile damage.
            Card sniper = PlayCard("SniperPerch");
            PlayCard("MountainousCarapace");
            DecisionSelectTarget = akash.CharacterCard;

            QuickHPStorage(akash);
            UsePower(sniper);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestSystemPurgeDestroy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Draw a card.
            //Destroy a serum or symptom. If a card was destroyed this way, reveal the top 2 cards of your deck. Put one into your hand and one on the bottom of the deck.
            Card green = PlayCard("GreenVeins");
            Card ytech = PlayCard("YTechCustomSerum4319B");
            Card codename = PutOnDeck("CodenamePeacekeeper");
            Card frag = PutOnDeck("FragGrenades");
            Card ambush = PutOnDeck("Ambush");
            DecisionDestroyCard = green;
            DecisionSelectCard = frag;

            PlayCard("SystemPurge");
            AssertInTrash(green);
            AssertInHand(ambush);
            AssertInHand(frag);
            AssertOnBottomOfDeck(codename);
        }

        [Test()]
        public void TestSystemPurgeNoDestroy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Draw a card.
            //Destroy a serum or symptom. If a card was destroyed this way, reveal the top 2 cards of your deck. Put one into your hand and one on the bottom of the deck.
            Card codename = PutOnDeck("CodenamePeacekeeper");
            Card frag = PutOnDeck("FragGrenades");
            Card ambush = PutOnDeck("Ambush");
            DecisionSelectCard = frag;

            PlayCard("SystemPurge");
            AssertInHand(ambush);
            AssertOnTopOfDeck(frag);
            AssertOnTopOfDeck(codename,1);
        }

        [Test()]
        public void TestToxicBlood()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Increase damage dealt by {Peacekeeper} to other targets by 1.
            PlayCard("ToxicBlood");

            //Check that it works on non-hero targets
            QuickHPStorage(akash);
            DealDamage(peacekeeper, akash, 1, DamageType.Melee);
            QuickHPCheck(-2);

            //Check that it works on hero targets
            QuickHPStorage(legacy);
            DealDamage(peacekeeper, legacy, 1, DamageType.Melee);
            QuickHPCheck(-2);

            //Check that it doesn't work on Peacekeeper
            QuickHPStorage(peacekeeper);
            DealDamage(peacekeeper, peacekeeper, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestUpCloseAndPersonal()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Play this card next to a target. Redirect all damage dealt by that target to {Peacekeeper}. Reduce non-melee damage that target deals to {Peacekeeper} by 1.
            DecisionSelectCard = akash.CharacterCard;
            Card upclose = PlayCard("UpCloseAndPersonal");
            AssertAtLocation(upclose, akash.CharacterCard.NextToLocation);

            //Check melee damage
            QuickHPStorage(peacekeeper, legacy);
            DealDamage(akash, legacy, 2, DamageType.Melee);
            QuickHPCheck(-2, 0);

            //Check non-melee damage
            QuickHPStorage(peacekeeper, legacy);
            DealDamage(akash, legacy, 2, DamageType.Cold);
            QuickHPCheck(-1, 0);
        }

        [Test()]
        public void TestUpCloseAndPersonalPower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Power: {Peacekeeper} deals the target next to this card 3 melee damage.
            DecisionSelectCard = akash.CharacterCard;
            Card upclose = PlayCard("UpCloseAndPersonal");
            AssertAtLocation(upclose, akash.CharacterCard.NextToLocation);

            QuickHPStorage(akash);
            UsePower(upclose);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestX517BoosterShot()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //Damage dealt by {Peacekeeper} to other targets is increased by 2 and irreducible.
            //Reduce damage dealt to {Peacekeeper} by 1.
            //At the start of your turn, {Peacekeeper} deals himself 2 irreducible toxic damage. Then you may play a symptom. Then destroy this card.
            Card booster = PlayCard("X517BoosterShot");
            Card toxic = PutInHand("ToxicBlood");
            Card carapace = PlayCard("MountainousCarapace");

            //Check that damage dealt from Peacekeeper to other targets is increased and irreducible
            QuickHPStorage(akash);
            DealDamage(peacekeeper, akash, 1, DamageType.Melee);
            QuickHPCheck(-3);

            QuickHPStorage(peacekeeper);
            DealDamage(peacekeeper, peacekeeper, 2, DamageType.Melee);
            QuickHPCheck(-1);

            //Check damage reduction
            QuickHPStorage(peacekeeper);
            DealDamage(akash, peacekeeper, 2, DamageType.Melee);
            QuickHPCheck(-1);

            //Check start of turn actions
            QuickHPStorage(peacekeeper);
            DecisionSelectCardToPlay = toxic;
            GoToStartOfTurn(peacekeeper);
            QuickHPCheck(-2);
            AssertIsInPlay(toxic);
            AssertInTrash(booster);
        }

        [Test()]
        public void TestYTechCustomSerum4319B()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "TheScholar", "TheBlock");
            StartGame();

            //{Peacekeeper} is immune to toxic damage.
            //At the end of your turn, return a symptom in play to your hand.
            Card ytech = PlayCard("YTechCustomSerum4319B");
            QuickHPStorage(peacekeeper);
            DealDamage(akash, peacekeeper, 1, DamageType.Toxic);
            QuickHPCheck(0);
            Card toxic = PlayCard("ToxicBlood");
            GoToEndOfTurn(peacekeeper);
            AssertInHand(toxic);
        }
    }
}