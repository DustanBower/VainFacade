using NUnit.Framework;
using System;
using VainFacadePlaytest;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using System.Collections.Generic;
using VainFacadePlaytest.Arctis;
using System.Runtime.Serialization;
using System.Security.Policy;

namespace VainFacadeTest
{
    [TestFixture()]
    public class ArctisTests:BaseTest
	{
        protected HeroTurnTakerController arctis { get { return FindHero("Arctis"); } }

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(arctis.CharacterCard, 1);
            DealDamage(villain, arctis, 2, DamageType.Melee, true);
        }

        [Test()]
        public void TestLoadArctis()
        {
            SetupGameController("BaronBlade", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(arctis);
            Assert.IsInstanceOf(typeof(ArctisCharacterCardController), arctis.CharacterCardController);

            Assert.AreEqual(29, arctis.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestInnatePowerDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //You may draw a card. If you don't, then until the end of your next turn, when cold damage would be dealt to a target, you may reduce that damage by 1 and put an icework from your hand into play.
            Card axe = PutInHand("IceAxe");
            QuickHandStorage(arctis);
            DecisionYesNo = true;
            UsePower(arctis);
            QuickHandCheck(1);
            QuickHPStorage(arctis);
            DealDamage(arctis, arctis, 2, DamageType.Cold);
            QuickHPCheck(-2);
            AssertInHand(axe);
        }

        [Test()]
        public void TestInnatePowerReduceByArctis()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //You may draw a card. If you don't, then until the end of your next turn, when cold damage would be dealt to a target, you may reduce that damage by 1 and put an icework from your hand into play.
            Card axe = PutInHand("IceAxe");
            QuickHandStorage(arctis);
            DecisionYesNo = false;
            DecisionSelectCardToPlay = axe;
            UsePower(arctis);
            QuickHandCheck(0);
            QuickHPStorage(akash);
            DecisionYesNo = true;
            DealDamage(arctis, akash, 2, DamageType.Cold);
            QuickHPCheck(-1);
            AssertIsInPlay(axe);
        }

        [Test()]
        public void TestInnatePowerReduceToArctis()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //You may draw a card. If you don't, then until the end of your next turn, when cold damage would be dealt to a target, you may reduce that damage by 1 and put an icework from your hand into play.
            Card axe = PutInHand("IceAxe");
            QuickHandStorage(arctis);
            DecisionYesNo = false;
            DecisionSelectCardToPlay = axe;
            UsePower(arctis);
            QuickHandCheck(0);
            QuickHPStorage(arctis);
            DecisionYesNo = true;
            DealDamage(akash, arctis, 2, DamageType.Cold);
            QuickHPCheck(-1);
            AssertIsInPlay(axe);
        }

        [Test()]
        public void TestInnatePowerOtherTarget()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //You may draw a card. If you don't, then until the end of your next turn, when cold damage would be dealt to a target, you may reduce that damage by 1 and put an icework from your hand into play.
            Card axe = PutInHand("IceAxe");
            QuickHandStorage(arctis);
            DecisionYesNo = false;
            DecisionSelectCardToPlay = axe;
            UsePower(arctis);
            QuickHandCheck(0);
            QuickHPStorage(legacy);
            DecisionYesNo = true;
            DealDamage(akash, legacy, 2, DamageType.Cold);
            QuickHPCheck(-2);
            AssertInHand(axe);
        }

        [Test()]
        public void TestIncap1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            SetupIncap(akash);

            //Select a target. Reduce the next damage dealt to that target by 2.
            DecisionSelectCard = legacy.CharacterCard;
            UseIncapacitatedAbility(arctis, 0);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestIncap2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            SetupIncap(akash);

            //1 hero may use a power
            DecisionSelectTurnTaker = bunker.TurnTaker;
            QuickHandStorage(bunker);
            UseIncapacitatedAbility(arctis, 1);
            QuickHandCheck(1);
        }

        [Test()]
        public void TestIncap3()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            SetupIncap(akash);

            //Destroy an ongoing card.
            Card allies = PlayCard("AlliesOfTheEarth");
            DecisionSelectCard = allies;
            UseIncapacitatedAbility(arctis, 2);
            AssertInTrash(allies);
        }

        [Test()]
        public void TestIceworkPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When this card is played, destroy it.
            //When this card is destroyed, draw 2 cards.
            MoveAllCards(arctis, arctis.HeroTurnTaker.Hand, arctis.TurnTaker.Trash);
            QuickHandStorage(arctis);
            Card axe = PlayCard("IceAxe");
            AssertInTrash(axe);
            QuickHandCheck(2);

            MoveAllCards(arctis, arctis.HeroTurnTaker.Hand, arctis.TurnTaker.Trash);
            QuickHandStorage(arctis);
            Card icicle = PlayCard("IcicleVolley");
            AssertInTrash(icicle);
            QuickHandCheck(2);

            MoveAllCards(arctis, arctis.HeroTurnTaker.Hand, arctis.TurnTaker.Trash);
            QuickHandStorage(arctis);
            Card armor = PlayCard("CrystalArmor");
            AssertInTrash(armor);
            QuickHandCheck(2);

            MoveAllCards(arctis, arctis.HeroTurnTaker.Hand, arctis.TurnTaker.Trash);
            QuickHandStorage(arctis);
            Card lance = PlayCard("FrostLance");
            AssertInTrash(lance);
            QuickHandCheck(2);

            MoveAllCards(arctis, arctis.HeroTurnTaker.Hand, arctis.TurnTaker.Trash);
            QuickHandStorage(arctis);
            Card helm = PlayCard("FrozenHelm");
            AssertInTrash(helm);
            QuickHandCheck(2);

            MoveAllCards(arctis, arctis.HeroTurnTaker.Hand, arctis.TurnTaker.Trash);
            QuickHandStorage(arctis);
            Card shield = PlayCard("GlacialShield");
            AssertInTrash(shield);
            QuickHandCheck(2);
        }

        [Test()]
        public void TestIceworkPutIntoPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            Card axe = PutIntoPlay("IceAxe");
            AssertIsInPlay(axe);

            Card icicle = PutIntoPlay("IcicleVolley");
            AssertIsInPlay(icicle);

            Card armor = PutIntoPlay("CrystalArmor");
            AssertIsInPlay(armor);

            Card lance = PutIntoPlay("FrostLance");
            AssertIsInPlay(lance);

            Card helm = PutIntoPlay("FrozenHelm");
            AssertIsInPlay(helm);

            Card shield = PutIntoPlay("GlacialShield");
            AssertIsInPlay(shield);
        }

        [Test()]
        public void TestIceworkPower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            Card axe = PutIntoPlay("IceAxe");
            AssertIsInPlay(axe);

            Card shield = PutIntoPlay("GlacialShield");
            AssertIsInPlay(shield);

            DecisionActivateAbilities = new Card[] { axe, shield };
            DecisionSelectTarget = akash.CharacterCard;
            AssertDecisionIsOptional(SelectionType.ActivateAbility);

            //If you have activated no {ice} effects this turn, activate up to two {ice} effects.
            QuickHPStorage(akash.CharacterCard, axe);
            UsePower(axe);
            DealDamage(akash.CharacterCard, axe, 2, DamageType.Melee);
            QuickHPCheck(-3, -1);

            QuickHPStorage(akash.CharacterCard, axe);
            UsePower(shield);
            DealDamage(akash.CharacterCard, axe, 2, DamageType.Melee);
            QuickHPCheck(0, -1);
        }

        [Test()]
        public void TestIceworkPowerBZ()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            Card axe = PutIntoPlay("IceAxe");
            AssertIsInPlay(axe);

            Card shield = PutIntoPlay("GlacialShield");
            AssertIsInPlay(shield);

            DecisionActivateAbilities = new Card[] { axe, shield };
            DecisionSelectTarget = progScion;
            AssertDecisionIsOptional(SelectionType.ActivateAbility);

            //If you have activated no {ice} effects this turn, activate up to two {ice} effects.
            QuickHPStorage(progScion, axe);
            UsePower(axe);
            DealDamage(progScion, axe, 2, DamageType.Melee);
            QuickHPCheck(-3, -1);

            //When Arctis moves to the other battle zone, he is still considered as having used an ice effect this turn
            SwitchBattleZone(arctis);
            DecisionSelectTarget = mindScion;
            QuickHPStorage(mindScion, axe);
            UsePower(shield);
            DealDamage(mindScion, axe, 2, DamageType.Melee);
            QuickHPCheck(0, -1);
        }

        [Test()]
        public void TestChildOfTheSnow()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //Draw a card.
            //Search your deck for an icework and put it into your hand. Shuffle your deck.
            //You may play a card.
            //{Arctis} deals himself 1 cold damage.

            MoveAllCards(arctis, arctis.HeroTurnTaker.Hand, arctis.TurnTaker.Trash);

            Card axe = GetCard("IceAxe");
            DecisionSelectCard = axe;
            Card wrath = PutOnDeck("WintersWrath");
            QuickShuffleStorage(arctis.TurnTaker.Deck);
            DecisionSelectCardToPlay = wrath;
            QuickHPStorage(arctis);

            PlayCard("ChildOfTheSnow");

            AssertIsInPlay(wrath);
            AssertInHand(axe);
            QuickShuffleCheck(1);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestChillTouchDestroy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //{Arctis} deals 1 target 1 melee and 1 cold damage.
            //Destroy an ongoing or reduce damage dealt by a target dealt cold damage this way by 2 until the start of your next turn.
            DecisionSelectTarget = akash.CharacterCard;
            Card allies = PlayCard("AlliesOfTheEarth");
            DecisionSelectFunction = 0;
            QuickHPStorage(akash, legacy);
            PlayCard("ChillTouch");
            DealDamage(akash, legacy, 3, DamageType.Melee);
            AssertInTrash(allies);
            QuickHPCheck(-2, -3);
        }

        [Test()]
        public void TestChillTouchReduce()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //{Arctis} deals 1 target 1 melee and 1 cold damage.
            //Destroy an ongoing or reduce damage dealt by a target dealt cold damage this way by 2 until the start of your next turn.
            DecisionSelectTarget = akash.CharacterCard;
            Card allies = PlayCard("AlliesOfTheEarth");
            DecisionSelectFunction = 1;
            QuickHPStorage(akash, legacy);
            PlayCard("ChillTouch");
            DealDamage(akash, legacy, 3, DamageType.Melee);
            AssertIsInPlay(allies);
            QuickHPCheck(-2, -1);
        }

        [Test()]
        public void TestChillTouchNoColdDamage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //{Arctis} deals 1 target 1 melee and 1 cold damage.
            //Destroy an ongoing or reduce damage dealt by a target dealt cold damage this way by 2 until the start of your next turn.
            DecisionSelectTarget = akash.CharacterCard;
            Card allies = PlayCard("AlliesOfTheEarth");
            PlayCard("MountainousCarapace");
            DecisionSelectFunction = 1;
            QuickHPStorage(akash, legacy);
            PlayCard("ChillTouch");
            DealDamage(akash, legacy, 3, DamageType.Melee);
            AssertIsInPlay(allies);
            QuickHPCheck(0, -3);
        }

        [Test()]
        public void TestColdAsIceDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When {Arctis} is dealt cold damage, he regains 1 hp.
            //The first time he regains hp this way each turn, you may draw a card or put a card from your hand into play. If you do neither, {Arctis} regains 1 hp.
            //When this card enters play, {Arctis} deals himself 1 cold damage.
            QuickHPStorage(arctis);
            QuickHandStorage(arctis);
            DecisionSelectFunction = 0;
            Card axe = PutOnDeck("IceAxe");
            Card volley = PutOnDeck("IcicleVolley");

            //Check that when played, Arctis hits himself for 1 and heals 1 and draws
            PlayCard("ColdAsIce");
            QuickHPCheck(0);
            QuickHandCheck(1);
            AssertInHand(volley);
            AssertOnTopOfDeck(axe);

            //Check that if Arctis gets hit with cold damage again, he heals but does not draw
            QuickHPStorage(arctis);
            QuickHandStorage(arctis);
            DealDamage(arctis, arctis, 2, DamageType.Cold);
            QuickHPCheck(-1);
            QuickHandCheck(0);
            AssertOnTopOfDeck(axe);
        }

        [Test()]
        public void TestColdAsIcePlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When {Arctis} is dealt cold damage, he regains 1 hp.
            //The first time he regains hp this way each turn, you may draw a card or put a card from your hand into play. If you do neither, {Arctis} regains 1 hp.
            //When this card enters play, {Arctis} deals himself 1 cold damage.
            QuickHPStorage(arctis);
            DecisionSelectFunction = 1;
            Card axe = PutInHand("IceAxe");
            Card volley = PutInHand("IcicleVolley");
            QuickHandStorage(arctis);
            DecisionSelectCardToPlay = volley;

            //Check that when played, Arctis hits himself for 1 and heals 1 and plays
            PlayCard("ColdAsIce");
            QuickHPCheck(0);
            QuickHandCheck(-1);
            AssertIsInPlay(volley);

            //Check that if Arctis gets hit with cold damage again, he heals but does not play
            QuickHPStorage(arctis);
            QuickHandStorage(arctis);
            DecisionSelectCardToPlay = axe;
            DealDamage(arctis, arctis, 2, DamageType.Cold);
            QuickHPCheck(-1);
            QuickHandCheck(0);
            AssertInHand(axe);
        }

        [Test()]
        public void TestColdFrontReduce1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When cold damage would be dealt to a target, you may reduce that damage by 1 or 2 to put an icework into play from your hand.
            Card axe = PutInHand("IceAxe");
            DecisionSelectCardToPlay = axe;
            DecisionSelectFunction = 0;
            AssertDecisionIsOptional(SelectionType.SelectFunction);
            Card cold = PlayCard("ColdFront");

            QuickHPStorage(arctis);
            DealDamage(akash, arctis, 3, DamageType.Cold);
            QuickHPCheck(-2);
            AssertIsInPlay(axe);
        }

        [Test()]
        public void TestColdFrontReduce2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When cold damage would be dealt to a target, you may reduce that damage by 1 or 2 to put an icework into play from your hand.
            Card axe = PutInHand("IceAxe");
            DecisionSelectCardToPlay = axe;
            DecisionSelectFunction = 1;
            AssertDecisionIsOptional(SelectionType.SelectFunction);
            Card cold = PlayCard("ColdFront");

            QuickHPStorage(arctis);
            DealDamage(akash, arctis, 3, DamageType.Cold);
            QuickHPCheck(-1);
            AssertIsInPlay(axe);
        }

        [Test()]
        public void TestColdFrontNoreduce()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When cold damage would be dealt to a target, you may reduce that damage by 1 or 2 to put an icework into play from your hand.
            Card axe = PutInHand("IceAxe");
            DecisionSelectCardToPlay = axe;
            DecisionDoNotSelectFunction = true;
            AssertDecisionIsOptional(SelectionType.SelectFunction);
            Card cold = PlayCard("ColdFront");

            QuickHPStorage(arctis);
            DealDamage(akash, arctis, 3, DamageType.Cold);
            QuickHPCheck(-3);
            AssertInHand(axe);
        }

        [Test()]
        public void TestColdFrontByArctis()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When cold damage would be dealt to a target, you may reduce that damage by 1 or 2 to put an icework into play from your hand.
            Card axe = PutInHand("IceAxe");
            DecisionSelectCardToPlay = axe;
            DecisionSelectFunction = 0;
            AssertDecisionIsOptional(SelectionType.SelectFunction);
            Card cold = PlayCard("ColdFront");

            QuickHPStorage(akash);
            DealDamage(arctis, akash, 3, DamageType.Cold);
            QuickHPCheck(-2);
            AssertIsInPlay(axe);
        }

        [Test()]
        public void TestColdFrontOtherTargets()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When cold damage would be dealt to a target, you may reduce that damage by 1 or 2 to put an icework into play from your hand.
            Card axe = PutInHand("IceAxe");
            DecisionSelectCardToPlay = axe;
            DecisionSelectFunction = 0;
            AssertDecisionIsOptional(SelectionType.SelectFunction);
            Card cold = PlayCard("ColdFront");

            QuickHPStorage(akash);
            DealDamage(legacy, akash, 3, DamageType.Cold);
            QuickHPCheck(-3);
            AssertInHand(axe);
        }

        [Test()]
        public void TestCryogenicBlood()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //Increase damage dealt by {Arctis} by 1.
            //At the end of your turn, {Arctis} may deal himself up to 2 cold damage. If he is dealt no damage this way, destroy this card.
            Card cryo = PlayCard("CryogenicBlood");
            QuickHPStorage(akash);
            DealDamage(arctis, akash, 1, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestCryogenicBloodEndOfTurn1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //Increase damage dealt by {Arctis} by 1.
            //At the end of your turn, {Arctis} may deal himself up to 2 cold damage. If he is dealt no damage this way, destroy this card.
            Card cryo = PlayCard("CryogenicBlood");
            QuickHPStorage(arctis);
            DecisionSelectNumber = 1;
            GoToEndOfTurn(arctis);
            QuickHPCheck(-2);
            AssertIsInPlay(cryo);
        }

        [Test()]
        public void TestCryogenicBloodEndOfTurn2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //Increase damage dealt by {Arctis} by 1.
            //At the end of your turn, {Arctis} may deal himself up to 2 cold damage. If he is dealt no damage this way, destroy this card.
            Card cryo = PlayCard("CryogenicBlood");
            QuickHPStorage(arctis);
            DecisionSelectNumber = 2;
            GoToEndOfTurn(arctis);
            QuickHPCheck(-3);
            AssertIsInPlay(cryo);
        }

        [Test()]
        public void TestCryogenicBloodEndOfTurnSkip()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //Increase damage dealt by {Arctis} by 1.
            //At the end of your turn, {Arctis} may deal himself up to 2 cold damage. If he is dealt no damage this way, destroy this card.
            Card cryo = PlayCard("CryogenicBlood");
            QuickHPStorage(arctis);
            DecisionSelectNumber = null;
            GoToEndOfTurn(arctis);
            QuickHPCheck(0);
            AssertInTrash(cryo);
        }

        [Test()]
        public void TestCrystalArmor()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When this card enters play, destroy all other copies of this card.
            //When {Arctis} would be dealt damage, you may redirect it to this card.
            Card armor1 = PutIntoPlay("CrystalArmor");
            Card armor2 = PutIntoPlay("CrystalArmor");
            AssertInTrash(armor1);
        }

        [Test()]
        public void TestCrystalArmorRedirect()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When this card enters play, destroy all other copies of this card.
            //When {Arctis} would be dealt damage, you may redirect it to this card.
            Card armor = PutIntoPlay("CrystalArmor");
            DecisionYesNo = true;
            QuickHPStorage(arctis.CharacterCard, armor);
            DealDamage(akash, arctis, 1, DamageType.Melee);
            QuickHPCheck(0, -1);
        }

        [Test()]
        public void TestCrystalArmorRedirectSkip()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When this card enters play, destroy all other copies of this card.
            //When {Arctis} would be dealt damage, you may redirect it to this card.
            Card armor = PutIntoPlay("CrystalArmor");
            DecisionYesNo = false;
            QuickHPStorage(arctis.CharacterCard, armor);
            DealDamage(akash, arctis, 1, DamageType.Melee);
            QuickHPCheck(-1, 0);
        }

        [Test()]
        public void TestDefensiveCombatCold()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When {Arctis} would be dealt cold damage, you may have an icework regain that much hp instead.
            //When {Arctis} would be dealt non-cold damage, you may increase that damage by 1 and redirect it to an icework.
            Card defensive = PlayCard("DefensiveCombat");
            Card axe = PutIntoPlay("IceAxe");
            SetHitPoints(axe, 1);
            DecisionYesNo = true;
            DecisionSelectCard = axe;

            QuickHPStorage(arctis.CharacterCard,axe);
            DealDamage(akash, arctis, 2, DamageType.Cold);
            QuickHPCheck(0, 2);
        }

        [Test()]
        public void TestDefensiveCombatColdSkip()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When {Arctis} would be dealt cold damage, you may have an icework regain that much hp instead.
            //When {Arctis} would be dealt non-cold damage, you may increase that damage by 1 and redirect it to an icework.
            Card defensive = PlayCard("DefensiveCombat");
            Card axe = PutIntoPlay("IceAxe");
            SetHitPoints(axe, 1);
            DecisionYesNo = false;
            DecisionSelectCard = axe;

            QuickHPStorage(arctis.CharacterCard, axe);
            DealDamage(akash, arctis, 2, DamageType.Cold);
            QuickHPCheck(-2,0);
        }

        [Test()]
        public void TestDefensiveCombatNonCold()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When {Arctis} would be dealt cold damage, you may have an icework regain that much hp instead.
            //When {Arctis} would be dealt non-cold damage, you may increase that damage by 1 and redirect it to an icework.
            Card defensive = PlayCard("DefensiveCombat");
            Card axe = PutIntoPlay("IceAxe");
            DecisionYesNo = true;
            DecisionSelectCard = axe;

            QuickHPStorage(arctis.CharacterCard, axe);
            DealDamage(akash, arctis, 1, DamageType.Melee);
            QuickHPCheck(0, -2);
        }

        [Test()]
        public void TestDefensiveCombatNonColdSkip()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When {Arctis} would be dealt cold damage, you may have an icework regain that much hp instead.
            //When {Arctis} would be dealt non-cold damage, you may increase that damage by 1 and redirect it to an icework.
            Card defensive = PlayCard("DefensiveCombat");
            Card axe = PutIntoPlay("IceAxe");
            DecisionYesNo = false;
            DecisionSelectCard = axe;

            QuickHPStorage(arctis.CharacterCard, axe);
            DealDamage(akash, arctis, 1, DamageType.Melee);
            QuickHPCheck(-1, 0);
        }

        [Test()]
        public void TestFreezingVeinsCold()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //Increase cold damage dealt by {Arctis} by 1.
            //When {Arctis} deals non-cold damage to a target, he then deals that target 1 cold damage. The type of this damage cannot be changed.
            //At the end of your turn, {Arctis} deals himself 1 cold damage.
            Card veins = PlayCard("FreezingVeins");

            QuickHPStorage(akash);
            DealDamage(arctis, akash, 1, DamageType.Cold);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestFreezingVeinsNonCold()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //Increase cold damage dealt by {Arctis} by 1.
            //When {Arctis} deals non-cold damage to a target, he then deals that target 1 cold damage. The type of this damage cannot be changed.
            //At the end of your turn, {Arctis} deals himself 1 cold damage.
            Card veins = PlayCard("FreezingVeins");

            QuickHPStorage(akash);
            DealDamage(arctis, akash, 1, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestFreezingVeinsImbuedFire()
        {
            SetupGameController("Omnitron", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "Ra", "Megalopolis");
            StartGame();
            //Increase cold damage dealt by {Arctis} by 1.
            //When {Arctis} deals non-cold damage to a target, he then deals that target 1 cold damage. The type of this damage cannot be changed.
            //At the end of your turn, {Arctis} deals himself 1 cold damage.
            Card veins = PlayCard("FreezingVeins");
            Card fire = PlayCard("ImbuedFire");
            PlayCard("AdaptivePlatingSubroutine");

            QuickHPStorage(omnitron);
            DealDamage(arctis, omnitron, 1, DamageType.Cold);
            QuickHPCheck(-4);
        }

        [Test()]
        public void TestFreezingVeinsEndOfTurn()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //Increase cold damage dealt by {Arctis} by 1.
            //When {Arctis} deals non-cold damage to a target, he then deals that target 1 cold damage. The type of this damage cannot be changed.
            //At the end of your turn, {Arctis} deals himself 1 cold damage.
            Card veins = PlayCard("FreezingVeins");

            QuickHPStorage(arctis);
            GoToEndOfTurn(arctis);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestFrostLance()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //Discard any number of cards. {Arctis} deals 1 target X plus 2 melee damage, where X = the number of cards discarded this way. Destroy this card.
            MoveAllCards(arctis, arctis.HeroTurnTaker.Hand, arctis.TurnTaker.Trash);
            Card lance = PutIntoPlay("FrostLance");
            PutInHand("IceAxe");
            PutInHand("ShardStorm");
            PutInHand("WintersWrath");
            DecisionSelectTarget = akash.CharacterCard;

            QuickHPStorage(akash);
            ActivateAbility("{ice}", lance);
            QuickHPCheck(-5);
            AssertInTrash(lance);
        }

        [Test()]
        public void TestFrozenHelm()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When this card enters play, destroy all other copies of this card.
            //When {Arctis} would be dealt damage by a target other than {Arctis}, reduce that damage by 1.
            Card helm1 = PutIntoPlay("FrozenHelm");
            Card helm2 = PutIntoPlay("FrozenHelm");
            AssertInTrash(helm1);
        }

        [Test()]
        public void TestFrozenHelmReduce()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "RuinsOfAtlantis");
            StartGame();
            //When this card enters play, destroy all other copies of this card.
            //When {Arctis} would be dealt damage by a target other than {Arctis}, reduce that damage by 1.
            Card helm = PutIntoPlay("FrozenHelm");

            //Should reduce damage from Akash'Bhuta
            QuickHPStorage(arctis);
            DealDamage(akash, arctis, 2, DamageType.Melee);
            QuickHPCheck(-1);

            //Should not reduce damage from Arctis
            QuickHPStorage(arctis);
            DealDamage(arctis, arctis, 2, DamageType.Melee);
            QuickHPCheck(-2);

            //Should not reduce damage from Hallway Collapse
            QuickHPStorage(arctis);
            PlayCard("HallwayCollapse");
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestGlacialShield()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "RuinsOfAtlantis");
            StartGame();
            //Until the start of your next turn, when a target other than {Arctis} would deal damage to one of your targets, reduce that damage by 1.
            Card shield = PutIntoPlay("GlacialShield");
            Card armor = PutIntoPlay("CrystalArmor");
            ActivateAbility("{ice}", shield);

            QuickHPStorage(armor);
            DealDamage(akash.CharacterCard, armor, 2, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(armor);
            DealDamage(arctis.CharacterCard, armor, 2, DamageType.Melee);
            QuickHPCheck(-2);

            GoToStartOfTurn(arctis);
            SetHitPoints(armor, 5);
            QuickHPStorage(armor);
            DealDamage(akash.CharacterCard, armor, 2, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestIceAxe()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "RuinsOfAtlantis");
            StartGame();
            //{Arctis} deals 1 target 3 melee damage.
            Card axe = PutIntoPlay("IceAxe");
            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(akash);
            ActivateAbility("{ice}", axe);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestIcicleVolley()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "RuinsOfAtlantis");
            StartGame();
            //{Arctis} deals up to 3 targets 2 projectile damage each. Destroy this card.
            Card volley = PutIntoPlay("IcicleVolley");
            Card rockslide = PlayCard("LivingRockslide");
            Card phalanges = PlayCard("ArborealPhalanges");

            DecisionSelectTargets = new Card[] { akash.CharacterCard, rockslide, phalanges };
            QuickHPStorage(akash.CharacterCard, rockslide, phalanges);
            ActivateAbility("{ice}",volley);
            QuickHPCheck(-2, -2, -2);
            AssertInTrash(volley);
        }

        [Test()]
        public void TestShardStorm1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "RuinsOfAtlantis");
            StartGame();
            //You may destroy any number of iceworks.
            //{Arctis} deals 1 target X plus 1 projectile damage or X targets 2 projectile damage each, where X = the number of cards destroyed this way.
            Card axe = PutIntoPlay("IceAxe");
            Card volley = PutIntoPlay("IcicleVolley");
            Card rockslide = PlayCard("LivingRockslide");
            Card phalanges = PlayCard("ArborealPhalanges");

            QuickHPStorage(akash.CharacterCard, rockslide, phalanges);
            DecisionSelectFunction = 0;
            DecisionSelectTargets = new Card[] { akash.CharacterCard, rockslide, phalanges };
            Card shard = PlayCard("ShardStorm");
            QuickHPCheck(-3, 0, 0);
            AssertInTrash(axe);
            AssertInTrash(volley);
        }

        [Test()]
        public void TestShardStorm2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "RuinsOfAtlantis");
            StartGame();
            //You may destroy any number of iceworks.
            //{Arctis} deals 1 target X plus 1 projectile damage or X targets 2 projectile damage each, where X = the number of cards destroyed this way.
            Card axe = PutIntoPlay("IceAxe");
            Card volley = PutIntoPlay("IcicleVolley");
            Card rockslide = PlayCard("LivingRockslide");
            Card phalanges = PlayCard("ArborealPhalanges");

            QuickHPStorage(akash.CharacterCard, rockslide, phalanges);
            DecisionSelectFunction = 1;
            DecisionSelectTargets = new Card[] { akash.CharacterCard, rockslide, phalanges };
            Card shard = PlayCard("ShardStorm");
            QuickHPCheck(-2, -2, 0);
            AssertInTrash(axe);
            AssertInTrash(volley);
        }

        [Test()]
        public void TestSublimeCascade()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "RuinsOfAtlantis");
            StartGame();
            //Once per turn, when {Arctis} is dealt cold damage, you may put a card from your hand into play for each point of damage {Arctis} is dealt this way.
            Card sublime = PlayCard("SublimeCascade");
            MoveAllCards(arctis, arctis.HeroTurnTaker.Hand, arctis.TurnTaker.Trash);
            Card axe = PutInHand("IceAxe");
            Card wrath = PutInHand("WintersWrath");
            Card shield = PutInHand("GlacialShield");

            DecisionSelectCards = new Card[] { axe, wrath, shield};
            DealDamage(akash, arctis, 2, DamageType.Cold);
            AssertIsInPlay(axe);
            AssertIsInPlay(wrath);
            AssertInHand(shield);
        }

        [Test()]
        public void TestWintersWrath()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Arctis", "Legacy", "Bunker", "TheScholar", "RuinsOfAtlantis");
            StartGame();
            //When an icework would be dealt damage that would reduce its hp below 1, destroy it instead.
            //When an icework is destroyed, {Arctis} deals 1 target X cold damage, where X = that card's hp before it was destroyed.
            Card wrath = PlayCard("WintersWrath");
            Card axe = PutIntoPlay("IceAxe");
            Card armor = PutIntoPlay("CrystalArmor");
            Card volley = PutIntoPlay("IcicleVolley");
            Card shield = PutIntoPlay("GlacialShield");

            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(akash);
            DealDamage(akash.CharacterCard, axe, 100, DamageType.Melee);
            QuickHPCheck(-3);

            QuickHPStorage(akash);
            DealDamage(akash.CharacterCard, armor, 100, DamageType.Melee);
            QuickHPCheck(-5);

            QuickHPStorage(akash);
            SetHitPoints(volley, 1);
            DealDamage(akash.CharacterCard, volley, 100, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(akash);
            DestroyCard(shield);
            QuickHPCheck(-3);
        }
    }
}

