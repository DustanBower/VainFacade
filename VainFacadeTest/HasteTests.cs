using NUnit.Framework;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.UnitTest;
using System.Linq;
using VainFacadePlaytest.Haste;
using Handelabra;
using System;
using System.Collections.Generic;

namespace VainFacadeTest
{
    [TestFixture()]
    public class HasteTests:BaseTest
	{
        protected HeroTurnTakerController haste { get { return FindHero("Haste"); } }
        protected HeroTurnTakerController friday { get { return FindHero("Friday"); } }
        protected TokenPool SpeedPool { get { return haste.CharacterCard.FindTokenPool("SpeedPool"); } }
        protected TurnTakerController doomsayer { get { return FindVillain("Doomsayer"); } }
        protected TurnTakerController ember { get { return FindHero("Ember"); } }
        protected Card countless { get { return GetCard("CountlessWords"); } }
        protected Card acolytes { get { return GetCard("AcolytesOfTheBlackThorn"); } }
        protected Card bladewight { get { return GetCard("Bladewight"); } }
        protected Card ingini { get { return GetCard("Ingini"); } }

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

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(haste.CharacterCard, 1);
            DealDamage(villain, haste, 2, DamageType.Melee, true);
        }

        [Test()]
        public void TestLoadHaste()
        {
            SetupGameController("BaronBlade", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "TheScholar", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(haste);
            Assert.IsInstanceOf(typeof(HasteCharacterCardController), haste.CharacterCardController);

            Assert.AreEqual(26, haste.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestInnatePower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Add 3 tokens to your Speed pool.
            //Until the start of your next turn, when a non-hero card enters play, you may remove 2 tokens from your speed pool. If you do, play a card.
            QuickTokenPoolStorage(SpeedPool);
            UsePower(haste);
            QuickTokenPoolCheck(3);

            DecisionYesNo = true;
            Card killing = PutInHand("KillingTime");
            Card instant = PutInHand("InstantAnalysis");
            DecisionSelectCardToPlay = killing;
            GoToEndOfTurn(akash);
            QuickTokenPoolStorage(SpeedPool);
            PlayCard("LivingRockslide");
            QuickTokenPoolCheck(-2);
            AssertIsInPlay(killing);

            DecisionSelectCardToPlay = instant;
            QuickTokenPoolStorage(SpeedPool);
            PlayCard("ArborealPhalanges");
            QuickTokenPoolCheck(-1);
            AssertInHand(instant);

            AddTokensToPool(SpeedPool, 2);
            GoToStartOfTurn(haste);
            QuickTokenPoolStorage(SpeedPool);
            PlayCard("Entomb");
            QuickTokenPoolCheck(0);
            AssertInHand(instant);
        }

        //[Test()]
        //public void TestInnatePowerDouble()
        //{
        //    //This works in game but not in unit test
        //    SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
        //    StartGame();
        //    //Add 3 tokens to your Speed pool.
        //    //Until the start of your next turn, when a non-hero card enters play, you may remove 2 tokens from your speed pool. If you do, play a card.
        //    QuickTokenPoolStorage(SpeedPool);
        //    UsePower(haste);
        //    QuickTokenPoolCheck(3);
        //    GoToStartOfTurn(legacy);
        //    QuickTokenPoolStorage(SpeedPool);
        //    UsePower(haste);
        //    QuickTokenPoolCheck(3);

        //    DecisionYesNo = true;
        //    MoveAllCards(haste, haste.HeroTurnTaker.Hand, haste.TurnTaker.Trash);
        //    Card blur = PutInHand("MotionBlur");
        //    Card alacrity = PutInHand("UnmatchedAlacrity");
        //    GoToEndOfTurn(akash);
        //    QuickTokenPoolStorage(SpeedPool);
        //    PlayCard("Entomb");
        //    AssertIsInPlay(blur);
        //    AssertIsInPlay(alacrity);
        //    QuickTokenPoolCheck(-4);
        //}

        [Test()]
        public void TestInnatePowerCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Guise", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Add 3 tokens to your Speed pool.
            //Until the start of your next turn, when a non-hero card enters play, you may remove 2 tokens from your speed pool. If you do, play a card.
            DecisionSelectCard = haste.CharacterCard;
            QuickTokenPoolStorage(SpeedPool);
            GoToPlayCardPhaseAndPlayCard(guise,"ICanDoThatToo");
            QuickTokenPoolCheck(3);

            Card kawaii = PutInHand("SuperUltraKawaii");
            GoToPlayCardPhase(FindEnvironment());
            DecisionSelectCardToPlay = kawaii;
            DecisionYesNo = true;
            QuickTokenPoolStorage(SpeedPool);
            PlayCard("ObsidianField");
            AssertIsInPlay(kawaii);
            QuickTokenPoolCheck(-2);
        }

        [Test()]
        public void TestInnatePowerRepOfEarth()
        {
            SetupGameController("AkashBhuta", "Tachyon", "Legacy", "Bunker", "TheScholar", "TheCelestialTribunal");
            StartGame();
            //Add 3 tokens to your Speed pool.
            //Until the start of your next turn, when a non-hero card enters play, you may remove 2 tokens from your speed pool. If you do, play a card.
            DecisionSelectFromBoxTurnTakerIdentifier = "VainFacadePlaytest.Haste";
            DecisionSelectFromBoxIdentifiers = new string[] { "VainFacadePlaytest.HasteCharacter" };
            Card rep = PlayCard("RepresentativeOfEarth");
            Card witness = PlayCard("CharacterWitness");
            Card hasteChar = GetCard("HasteCharacter");
            AssertIsInPlay(hasteChar);

            TokenPool speed = hasteChar.FindTokenPool("SpeedPool");

            //Let Bunker use Haste's power to play Ammo Drop
            DecisionSelectCard = bunker.CharacterCard;
            DecisionYesNo = true;
            Card ammo = PutInHand("AmmoDrop");
            DecisionSelectCardToPlay = ammo;

            QuickTokenPoolStorage(speed);
            GoToStartOfTurn(FindEnvironment());
            QuickTokenPoolCheck(3);

            QuickTokenPoolStorage(speed);
            PlayCard("Entomb");
            AssertIsInPlay(ammo);
            QuickTokenPoolCheck(-2);
        }

        [Test()]
        public void TestIncap1Discard()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Select a target. Reduce the next damage dealt to that target by 2.
            SetupIncap(akash);
            DecisionSelectCard = legacy.CharacterCard;
            UseIncapacitatedAbility(haste, 0);
            QuickHPStorage(bunker);
            DealDamage(akash, bunker, 3, DamageType.Melee);
            QuickHPCheck(-3);
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
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //1 player may play a card.
            SetupIncap(akash);
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card fortitude = PutInHand("Fortitude");
            DecisionSelectCardToPlay = fortitude;
            UseIncapacitatedAbility(haste, 1);
            AssertIsInPlay(fortitude);
        }

        [Test()]
        public void TestIncap3()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //1 hero may use a power.
            SetupIncap(akash);
            Card ammo = PutOnDeck("AmmoDrop");
            DecisionSelectTurnTaker = bunker.TurnTaker;
            QuickHandStorage(bunker);
            UseIncapacitatedAbility(haste,2);
            QuickHandCheck(1);
            AssertInHand(ammo);
        }

        [Test()]
        public void TestBulletTimePrevent()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Power: Add 3 tokens to your speed pool.
            //Until the start of your next turn, when {Haste} would be dealt damage by a source other than {Haste}, remove any number of tokens from your speed pool.
            //If the number of tokens equals or exceeds that damage, prevent that damage.
            Card bullet = PlayCard("BulletTime");
            GoToUsePowerPhase(haste);
            QuickTokenPoolStorage(SpeedPool);
            UsePower(bullet);
            QuickTokenPoolCheck(3);

            QuickTokenPoolStorage(SpeedPool);
            QuickHPStorage(haste);
            DecisionSelectNumber = 2;
            DealDamage(akash, haste, 2, DamageType.Melee);
            QuickTokenPoolCheck(-2);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestBulletTimeNoPrevent()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Power: Add 3 tokens to your speed pool.
            //Until the start of your next turn, when {Haste} would be dealt damage by a source other than {Haste}, remove any number of tokens from your speed pool.
            //If the number of tokens equals or exceeds that damage, prevent that damage.
            Card bullet = PlayCard("BulletTime");
            GoToUsePowerPhase(haste);
            QuickTokenPoolStorage(SpeedPool);
            UsePower(bullet);
            QuickTokenPoolCheck(3);

            QuickTokenPoolStorage(SpeedPool);
            QuickHPStorage(haste);
            DecisionSelectNumber = 1;
            DealDamage(akash, haste, 2, DamageType.Melee);
            QuickTokenPoolCheck(-1);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestBulletTimeExpire()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Power: Add 3 tokens to your speed pool.
            //Until the start of your next turn, when {Haste} would be dealt damage by a source other than {Haste}, remove any number of tokens from your speed pool.
            //If the number of tokens equals or exceeds that damage, prevent that damage.
            Card bullet = PlayCard("BulletTime");
            GoToUsePowerPhase(haste);
            QuickTokenPoolStorage(SpeedPool);
            UsePower(bullet);
            QuickTokenPoolCheck(3);

            GoToStartOfTurn(haste);

            QuickTokenPoolStorage(SpeedPool);
            QuickHPStorage(haste);
            DecisionSelectNumber = 2;
            DealDamage(akash, haste, 2, DamageType.Melee);
            QuickTokenPoolCheck(0);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestBulletTimeCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //Power: Add 3 tokens to your speed pool.
            //Until the start of your next turn, when {Haste} would be dealt damage by a source other than {Haste}, remove any number of tokens from your speed pool.
            //If the number of tokens equals or exceeds that damage, prevent that damage.
            Card bullet = PlayCard("BulletTime");
            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = GoToPlayCardPhaseAndPlayCard(guise,"UhYeahImThatGuy");
            QuickTokenPoolStorage(SpeedPool);
            UsePower(uhyeah);
            QuickTokenPoolCheck(3);

            QuickTokenPoolStorage(SpeedPool);
            QuickHPStorage(guise);
            DecisionSelectNumber = 2;
            DealDamage(akash, guise, 2, DamageType.Melee);
            QuickTokenPoolCheck(-2);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestFoldingTimeNoIncrease()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When one of your cards enters play, {Haste} may deal 1 target 1 energy damage. You may remove a token from your speed pool. If you do, increase that damage by 1.
            //When {Haste} deals himself damage, draw up to X cards, where X = the damage dealt this way.

            DecisionSelectTarget = akash.CharacterCard;

            //Make sure Folding Time doesn't react to itself entering play
            QuickHPStorage(akash);
            Card folding = PlayCard("FoldingTime");
            QuickHPCheck(0);

            AddTokensToPool(SpeedPool, 1);

            DecisionYesNo = false;
            QuickHPStorage(akash);
            QuickTokenPoolStorage(SpeedPool);
            PlayCard("BulletTime");
            QuickHPCheck(-1);
            QuickTokenPoolCheck(0);
        }

        [Test()]
        public void TestFoldingTimeIncrease()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When one of your cards enters play, {Haste} may deal 1 target 1 energy damage. You may remove a token from your speed pool. If you do, increase that damage by 1.
            //When {Haste} deals himself damage, draw up to X cards, where X = the damage dealt this way.

            DecisionSelectTarget = akash.CharacterCard;

            //Make sure Folding Time doesn't react to itself entering play
            QuickHPStorage(akash);
            Card folding = PlayCard("FoldingTime");
            QuickHPCheck(0);

            AddTokensToPool(SpeedPool, 1);

            DecisionYesNo = true;
            QuickHPStorage(akash);
            QuickTokenPoolStorage(SpeedPool);
            PlayCard("BulletTime");
            QuickHPCheck(-2);
            QuickTokenPoolCheck(-1);
        }

        [Test()]
        public void TestFoldingTimeNoTokens()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When one of your cards enters play, {Haste} may deal 1 target 1 energy damage. You may remove a token from your speed pool. If you do, increase that damage by 1.
            //When {Haste} deals himself damage, draw up to X cards, where X = the damage dealt this way.

            DecisionSelectTarget = akash.CharacterCard;

            //Make sure Folding Time doesn't react to itself entering play
            QuickHPStorage(akash);
            Card folding = PlayCard("FoldingTime");
            QuickHPCheck(0);

            DecisionYesNo = true;
            QuickHPStorage(akash);
            QuickTokenPoolStorage(SpeedPool);
            PlayCard("BulletTime");
            QuickHPCheck(-1);
            QuickTokenPoolCheck(0);
        }

        [Test()]
        public void TestFoldingTimeOtherCard()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When one of your cards enters play, {Haste} may deal 1 target 1 energy damage. You may remove a token from your speed pool. If you do, increase that damage by 1.
            //When {Haste} deals himself damage, draw up to X cards, where X = the damage dealt this way.

            DecisionSelectTarget = akash.CharacterCard;

            //Make sure Folding Time doesn't react to itself entering play
            QuickHPStorage(akash);
            Card folding = PlayCard("FoldingTime");
            QuickHPCheck(0);

            AddTokensToPool(SpeedPool, 1);

            DecisionYesNo = true;
            QuickHPStorage(akash);
            QuickTokenPoolStorage(SpeedPool);
            PlayCard("Fortitude");
            QuickHPCheck(0);
            QuickTokenPoolCheck(0);
        }

        [Test()]
        public void TestFoldingTimeCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When one of your cards enters play, {Haste} may deal 1 target 1 energy damage. You may remove a token from your speed pool. If you do, increase that damage by 1.
            //When {Haste} deals himself damage, draw up to X cards, where X = the damage dealt this way.

            DecisionSelectTarget = akash.CharacterCard;

            //Make sure Folding Time doesn't react to itself entering play
            QuickHPStorage(akash);
            Card folding = PlayCard("FoldingTime");
            QuickHPCheck(0);

            DecisionSelectTurnTaker = haste.TurnTaker;
            QuickHPStorage(akash);
            Card uhyeah = GoToPlayCardPhaseAndPlayCard(guise, "UhYeahImThatGuy");
            QuickHPCheck(0);

            AddTokensToPool(SpeedPool, 2);

            DecisionYesNo = true;
            QuickHPStorage(akash);
            QuickTokenPoolStorage(SpeedPool);
            PlayCard("SuperUltraKawaii");
            QuickHPCheck(-2);
            QuickTokenPoolCheck(-1);
        }

        [Test()]
        public void TestFoldingTimeCopy2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When one of your cards enters play, {Haste} may deal 1 target 1 energy damage. You may remove a token from your speed pool. If you do, increase that damage by 1.
            //When {Haste} deals himself damage, draw up to X cards, where X = the damage dealt this way.

            DecisionSelectTarget = akash.CharacterCard;

            //Make sure Folding Time doesn't react to itself entering play
            Card folding = PlayCard("FoldingTime");
            Card standing = PlayCard("StandingStill");

            DecisionSelectTurnTaker = haste.TurnTaker;
            QuickHPStorage(akash);
            Card uhyeah = GoToPlayCardPhaseAndPlayCard(guise, "UhYeahImThatGuy");

            AddTokensToPool(SpeedPool, 2);

            //Make sure that if Uh Yeah is copying Folding Time and Standing Still, Folding Time won't react
            //to the damage from Standing Still's power
            DecisionYesNo = true;
            QuickHPStorage(akash);
            QuickTokenPoolStorage(SpeedPool);
            UsePower(uhyeah);
            QuickHPCheck(-2);
            QuickTokenPoolCheck(1);
        }

        [Test()]
        public void TestFoldingTimeSelfDamage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When one of your cards enters play, {Haste} may deal 1 target 1 energy damage. You may remove a token from your speed pool. If you do, increase that damage by 1.
            //When {Haste} deals himself damage, draw up to X cards, where X = the damage dealt this way.

            Card folding = PlayCard("FoldingTime");
            QuickHandStorage(haste);
            DealDamage(haste, haste, 2, DamageType.Melee);
            QuickHandCheck(2);

            QuickHandStorage(haste);
            DealDamage(haste, haste, 1, DamageType.Melee);
            QuickHandCheck(1);

            QuickHandStorage(haste);
            DealDamage(legacy, haste, 2, DamageType.Melee);
            QuickHandCheck(0);

            QuickHandStorage(haste);
            DealDamage(haste, legacy, 2, DamageType.Melee);
            QuickHandCheck(0);
        }

        [Test()]
        public void TestFoldingTimeSelfDamageCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When one of your cards enters play, {Haste} may deal 1 target 1 energy damage. You may remove a token from your speed pool. If you do, increase that damage by 1.
            //When {Haste} deals himself damage, draw up to X cards, where X = the damage dealt this way.

            Card folding = PlayCard("FoldingTime");


            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = GoToPlayCardPhaseAndPlayCard(guise, "UhYeahImThatGuy");

            QuickHandStorage(haste,guise);
            DealDamage(guise, guise, 2, DamageType.Melee);
            QuickHandCheck(0,2);

            QuickHandStorage(haste,guise);
            DealDamage(guise, guise, 1, DamageType.Melee);
            QuickHandCheck(0,1);

            QuickHandStorage(haste,guise);
            DealDamage(haste, guise, 2, DamageType.Melee);
            QuickHandCheck(0,0);

            QuickHandStorage(haste,guise);
            DealDamage(guise, haste, 2, DamageType.Melee);
            QuickHandCheck(0,0);
        }

        [Test()]
        public void TestHighSpeedCollision()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //Remove any number of tokens from your speed pool.
            //{Haste} deals 1 target X melee damage, where X = 2 plus the number of tokens removed this way.

            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(akash);
            Card highspeed = PlayCard("HighSpeedCollision");
            QuickHPCheck(-2);

            AddTokensToPool(SpeedPool, 2);
            DecisionSelectNumber = 2;
            QuickHPStorage(akash);
            PlayCard(highspeed);
            QuickHPCheck(-4);
        }

        [Test()]
        public void TestInAFlash()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //Add 2 tokens to your speed pool.
            //You may draw a card
            //Destroy any number of hero ongoing or equipment cards. For each card destroyed this way, you may destroy an environment or ongoing card.
            Card entomb = PlayCard("Entomb");
            Card obsidian = PlayCard("ObsidianField");
            Card fortitude = PlayCard("Fortitude");
            Card ammo = PlayCard("AmmoDrop");
            Card flash = PutInTrash("InAFlash");
            QuickHandStorage(haste);
            QuickTokenPoolStorage(SpeedPool);
            PlayCard(flash);
            QuickTokenPoolCheck(2);
            QuickHandCheck(1);

            AssertInTrash(new Card[] { entomb, obsidian, fortitude, ammo, flash });
        }

        [Test()]
        public void TestInstantAnalysis()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 2 tokens to your speed pool.
            //At the end of your turn, you may remove any number of tokens from your speed pool. X players draw a card, where X = the number of tokens removed this way.

            QuickTokenPoolStorage(SpeedPool);
            Card instant = PlayCard("InstantAnalysis");
            QuickTokenPoolCheck(2);

            DecisionSelectNumber = 2;
            DecisionSelectTurnTakers = new TurnTaker[] { haste.TurnTaker, legacy.TurnTaker };
            QuickHandStorage(haste, legacy);
            QuickTokenPoolStorage(SpeedPool);
            GoToEndOfTurn(haste);
            QuickHandCheck(1, 1);
            QuickTokenPoolCheck(-2);
        }

        [Test()]
        public void TestInstantAnalysisCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 2 tokens to your speed pool.
            //At the end of your turn, you may remove any number of tokens from your speed pool. X players draw a card, where X = the number of tokens removed this way.

            GoToPlayCardPhase(guise);
            Card instant = PlayCard("InstantAnalysis");

            QuickTokenPoolStorage(SpeedPool);
            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");
            QuickTokenPoolCheck(2);

            ResetDecisions();

            DecisionSelectNumber = 2;
            DecisionSelectTurnTakers = new TurnTaker[] { haste.TurnTaker, legacy.TurnTaker };
            QuickHandStorage(haste, legacy);
            QuickTokenPoolStorage(SpeedPool);
            GoToEndOfTurn(guise);
            QuickHandCheck(1, 1);
            QuickTokenPoolCheck(-2);
        }

        [Test()]
        public void TestKillingTimeAddToken()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When a card not named {KillingTime} adds tokens to your speed pool, add a token to your speed pool.
            //At the start of your turn, increase the next damage dealt to {Haste} by 1 for every 10 tokens in your speed pool. Then {Haste} deals himself 0 irreducible toxic damage.
            Card killing = PlayCard("KillingTime");

            //Check that token addition is increased to 3
            QuickTokenPoolStorage(SpeedPool);
            Card instant = PlayCard("InstantAnalysis");
            QuickTokenPoolCheck(3);
            DestroyCard(instant);

            //Check that it works on following turns
            GoToPlayCardPhase(legacy);
            QuickTokenPoolStorage(SpeedPool);
            PlayCard(instant);
            QuickTokenPoolCheck(3);
            DestroyCard(instant);

            //Check that it works after reload
            GoToPlayCardPhase(bunker);
            SaveAndLoad(this.GameController);
            instant = GetCard("InstantAnalysis");
            killing = GetCard("KillingTime", 0, (Card c) => c.IsInPlayAndHasGameText);
            QuickTokenPoolStorage(SpeedPool);
            PlayCard(instant);
            QuickTokenPoolCheck(3);
            DestroyCard(instant);

            //Check that the increase does not persist after Killing Time is destroyed.
            DestroyCard(killing);
            QuickTokenPoolStorage(SpeedPool);
            PlayCard(instant);
            QuickTokenPoolCheck(2);
            DestroyCard(instant);

            //Check that it works properly the next time Killing Time enters play
            PlayCard(killing);
            QuickTokenPoolStorage(SpeedPool);
            PlayCard(instant);
            QuickTokenPoolCheck(3);
        }

        [Test()]
        public void TestKillingTimeAddTokenCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When a card not named {KillingTime} adds tokens to your speed pool, add a token to your speed pool.
            //At the start of your turn, increase the next damage dealt to {Haste} by 1 for every 10 tokens in your speed pool. Then {Haste} deals himself 0 irreducible toxic damage.
            Card killing = PlayCard("KillingTime");
            Card instant = PlayCard("InstantAnalysis");

            //Check that the token add from playing Uh Yeah is increased
            QuickTokenPoolStorage(SpeedPool);
            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");
            QuickTokenPoolCheck(4);

            Card standing = PlayCard("StandingStill");

            //Check that the token add from Standing Still's power is increased
            QuickTokenPoolStorage(SpeedPool);
            UsePower(standing);
            QuickTokenPoolCheck(3);

            //Check that the token add from the copied Standing Still power is increased
            QuickTokenPoolStorage(SpeedPool);
            UsePower(uhyeah);
            QuickTokenPoolCheck(3);

            //Check that it works on the next turn
            DestroyCard(instant);
            GoToPlayCardPhase(legacy);
            QuickTokenPoolStorage(SpeedPool);
            UsePower(uhyeah);
            QuickTokenPoolCheck(3);

            //Check that it works on rewind
            GoToPlayCardPhase(bunker);
            SaveAndLoad(this.GameController);
            uhyeah = GetCard("UhYeahImThatGuy", 0, (Card c) => c.IsInPlayAndHasGameText);
            standing = GetCard("StandingStill", 0, (Card c) => c.IsInPlayAndHasGameText);
            killing = GetCard("KillingTime", 0, (Card c) => c.IsInPlayAndHasGameText);
            QuickTokenPoolStorage(SpeedPool);
            UsePower(uhyeah);
            QuickTokenPoolCheck(3);

            //Check that when Uh Yeah is destroyed, its increase no longer applies
            DestroyCard(uhyeah);
            QuickTokenPoolStorage(SpeedPool);
            UsePower(standing);
            QuickTokenPoolCheck(2);
        }

        [Test()]
        public void TestKillingTimeAddTokenCopy2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When a card not named {KillingTime} adds tokens to your speed pool, add a token to your speed pool.
            //At the start of your turn, increase the next damage dealt to {Haste} by 1 for every 10 tokens in your speed pool. Then {Haste} deals himself 0 irreducible toxic damage.
            Card killing = PlayCard("KillingTime");
            Card instant = PlayCard("InstantAnalysis");

            //Check that the token add from playing Uh Yeah is increased
            QuickTokenPoolStorage(SpeedPool);
            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");
            QuickTokenPoolCheck(4);

            Card standing = PlayCard("StandingStill");

            //Check that the token add from the coped Standing Still power is increased
            QuickTokenPoolStorage(SpeedPool);
            UsePower(uhyeah);
            QuickTokenPoolCheck(3);

            //Check that when Killing Time is destroyed, Uh Yeah is no longer copying it
            DestroyCard(killing);
            QuickTokenPoolStorage(SpeedPool);
            UsePower(standing);
            QuickTokenPoolCheck(1);
        }

        [Test()]
        public void TestKillingTimeCopyAlone()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When a card not named {KillingTime} adds tokens to your speed pool, add a token to your speed pool.
            //At the start of your turn, increase the next damage dealt to {Haste} by 1 for every 10 tokens in your speed pool. Then {Haste} deals himself 0 irreducible toxic damage.
            ClearInitialCards();

            Card killing = PlayCard("KillingTime");

            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");

            QuickTokenPoolStorage(SpeedPool);
            Card instant = PlayCard("InstantAnalysis");
            QuickTokenPoolCheck(4);
            DestroyCard(instant);

            SetHitPoints(guise, 10);
            PlayCard("YouAreAlone");

            QuickTokenPoolStorage(SpeedPool);
            PlayCard(instant);
            QuickTokenPoolCheck(3);
        }

        [Test()]
        public void TestKillingTimeCopyAlone2()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When a card not named {KillingTime} adds tokens to your speed pool, add a token to your speed pool.
            //At the start of your turn, increase the next damage dealt to {Haste} by 1 for every 10 tokens in your speed pool. Then {Haste} deals himself 0 irreducible toxic damage.
            ClearInitialCards();

            Card killing = PlayCard("KillingTime");

            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");

            QuickTokenPoolStorage(SpeedPool);
            Card instant = PlayCard("InstantAnalysis");
            QuickTokenPoolCheck(4);
            DestroyCard(instant);

            SetHitPoints(haste, 10);
            PlayCard("YouAreAlone");

            QuickTokenPoolStorage(SpeedPool);
            PlayCard(instant);
            QuickTokenPoolCheck(3);
        }

        [Test()]
        public void TestKillingTimeCopyIsolated()
        {
            SetupGameController("MissInformation", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When a card not named {KillingTime} adds tokens to your speed pool, add a token to your speed pool.
            //At the start of your turn, increase the next damage dealt to {Haste} by 1 for every 10 tokens in your speed pool. Then {Haste} deals himself 0 irreducible toxic damage.
            Card killing = PlayCard("KillingTime");

            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");

            QuickTokenPoolStorage(SpeedPool);
            Card instant = PlayCard("InstantAnalysis");
            QuickTokenPoolCheck(4);
            DestroyCard(instant);

            Card isolated = PlayCard("IsolatedHero");
            AssertAtLocation(isolated, haste.CharacterCard.NextToLocation);

            QuickTokenPoolStorage(SpeedPool);
            PlayCard(instant);
            QuickTokenPoolCheck(3);
        }

        [Test()]
        public void TestKillingTimeStartOfTurn()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When a card not named {KillingTime} adds tokens to your speed pool, add a token to your speed pool.
            //At the start of your turn, increase the next damage dealt to {Haste} by 1 for every 10 tokens in your speed pool. Then {Haste} deals himself 0 irreducible toxic damage.
            Card killing = PlayCard("KillingTime");
            AddTokensToPool(SpeedPool, 11);

            QuickHPStorage(haste);
            GoToStartOfTurn(haste);
            QuickHPCheck(-1);

            AddTokensToPool(SpeedPool, 45);
            QuickHPStorage(haste);
            GoToStartOfTurn(haste);
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestKillingTimeStartOfTurnCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When a card not named {KillingTime} adds tokens to your speed pool, add a token to your speed pool.
            //At the start of your turn, increase the next damage dealt to {Haste} by 1 for every 10 tokens in your speed pool. Then {Haste} deals himself 0 irreducible toxic damage.
            Card killing = PlayCard("KillingTime");
            AddTokensToPool(SpeedPool, 45);

            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");

            QuickHPStorage(guise);
            GoToStartOfTurn(guise);
            QuickHPCheck(-4);
        }

        [Test()]
        public void TestMotionBlurReduce()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When you play a card, you may select a target that has not been selected this way this turn. Reduce the next damage dealt by that target by 1.
            //Power: Play a card. You may discard a card to play a card.
            DecisionSelectCard = akash.CharacterCard;
            Card motion = PlayCard("MotionBlur");

            //Make sure Motion Blur doesn't trigger on itself
            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 2, DamageType.Melee);
            QuickHPCheck(-2);

            //Check that the damage reduction works
            GoToStartOfTurn(haste);
            PlayCard("BulletTime");
            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 2, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that it only reduces the next damage
            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 2, DamageType.Melee);
            QuickHPCheck(-2);

            ResetDecisions();

            AssertNextDecisionChoices(null, new Card[] { akash.CharacterCard });
            PlayCard("InstantAnalysis");

            GoToStartOfTurn(legacy);
            AssertNextDecisionChoices(new Card[] { akash.CharacterCard });
            PlayCard("KillingTime");
        }

        [Test()]
        public void TestMotionBlurPower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When you play a card, you may select a target that has not been selected this way this turn. Reduce the next damage dealt by that target by 1.
            //Power: Play a card. You may discard a card to play a card.
            Card motion = PlayCard("MotionBlur");
            MoveAllCards(haste, haste.HeroTurnTaker.Hand, haste.TurnTaker.Trash);
            Card bullet = PutInHand("BulletTime");
            Card highspeed = PutInHand("HighSpeedCollision");
            Card killing = PutInHand("KillingTime");
            AssertDecisionIsOptional(SelectionType.DiscardCard);
            DecisionDiscardCard = highspeed;
            UsePower(motion);
            AssertIsInPlay(new Card[] { bullet, killing });
            AssertInTrash(highspeed);
        }

        [Test()]
        public void TestQuickReactionsMoveCard()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 3 tokens to your speed pool.
            //When a hero ongoing or equipment would be destroyed, you may remove 3 tokens from your speed pool. If you do, put that card on top of its deck. Otherwise, if it is one of your cards, add 2 tokens to your speed pool.

            QuickTokenPoolStorage(SpeedPool);
            Card quick = PlayCard("QuickReactions");
            QuickTokenPoolCheck(3);

            Card fortitude = PlayCard("Fortitude");
            DecisionYesNo = true;
            QuickTokenPoolStorage(SpeedPool);
            DestroyCard(fortitude);
            AssertOnTopOfDeck(fortitude);
            QuickTokenPoolCheck(-3);
        }

        [Test()]
        public void TestQuickReactionsDoNotMoveCardOtherHero()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 3 tokens to your speed pool.
            //When a hero ongoing or equipment would be destroyed, you may remove 3 tokens from your speed pool. If you do, put that card on top of its deck. Otherwise, if it is one of your cards, add 2 tokens to your speed pool.

            Card quick = PlayCard("QuickReactions");
            DecisionYesNo = false;
            Card fortitude = PlayCard("Fortitude");

            QuickTokenPoolStorage(SpeedPool);
            DestroyCard(fortitude);
            AssertInTrash(fortitude);
            QuickTokenPoolCheck(0);
        }

        [Test()]
        public void TestQuickReactionsDoNotMoveCardHaste()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 3 tokens to your speed pool.
            //When a hero ongoing or equipment would be destroyed, you may remove 3 tokens from your speed pool. If you do, put that card on top of its deck. Otherwise, if it is one of your cards, add 2 tokens to your speed pool.

            Card quick = PlayCard("QuickReactions");
            DecisionYesNo = false;
            Card bullet = PlayCard("BulletTime");

            QuickTokenPoolStorage(SpeedPool);
            DestroyCard(bullet);
            AssertInTrash(bullet);
            QuickTokenPoolCheck(2);
        }

        [Test()]
        public void TestQuickReactionsMoveCardCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 3 tokens to your speed pool.
            //When a hero ongoing or equipment would be destroyed, you may remove 3 tokens from your speed pool. If you do, put that card on top of its deck. Otherwise, if it is one of your cards, add 2 tokens to your speed pool.

            Card quick = PlayCard("QuickReactions");

            DecisionSelectTurnTaker = haste.TurnTaker;
            QuickTokenPoolStorage(SpeedPool);
            Card uhyeah = PlayCard("UhYeahImThatGuy");
            QuickTokenPoolCheck(3);

            Card fortitude = PlayCard("Fortitude");
            DecisionsYesNo = new bool[] { false, true };
            QuickTokenPoolStorage(SpeedPool);
            DestroyCard(fortitude);
            AssertOnTopOfDeck(fortitude);
            QuickTokenPoolCheck(-3);
        }

        [Test()]
        public void TestQuickReactionsDoNotMoveCardHasteCopy1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 3 tokens to your speed pool.
            //When a hero ongoing or equipment would be destroyed, you may remove 3 tokens from your speed pool. If you do, put that card on top of its deck. Otherwise, if it is one of your cards, add 2 tokens to your speed pool.

            Card quick = PlayCard("QuickReactions");
            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");
            DecisionsYesNo = new bool[] {false,true};
            Card bullet = PlayCard("BulletTime");

            //If Haste says no and Guise says yes to removing tokens for a Haste card, Haste should add 2 tokens and Guise should remove 3 tokens and put it on top of the deck
            QuickTokenPoolStorage(SpeedPool);
            DestroyCard(bullet);
            AssertOnTopOfDeck(bullet);
            QuickTokenPoolCheck(-1);
        }

        [Test()]
        public void TestQuickReactionsDoNotMoveCardHasteCopy2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 3 tokens to your speed pool.
            //When a hero ongoing or equipment would be destroyed, you may remove 3 tokens from your speed pool. If you do, put that card on top of its deck. Otherwise, if it is one of your cards, add 2 tokens to your speed pool.

            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");
            Card quick = PlayCard("QuickReactions");
            
            DecisionsYesNo = new bool[] { false, true };
            Card kawaii = PlayCard("SuperUltraKawaii");

            //If Guise says no and Haste says yes to removing tokens for a Guise card, Guise should add 2 tokens and haste should remove 3 tokens and put it on top of the deck
            QuickTokenPoolStorage(SpeedPool);
            DestroyCard(kawaii);
            AssertOnTopOfDeck(kawaii);
            QuickTokenPoolCheck(-1);
        }

        [Test()]
        public void TestQuickReactionsDoNotMoveCardHasteCopy3()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 3 tokens to your speed pool.
            //When a hero ongoing or equipment would be destroyed, you may remove 3 tokens from your speed pool. If you do, put that card on top of its deck. Otherwise, if it is one of your cards, add 2 tokens to your speed pool.

            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");
            Card quick = PlayCard("QuickReactions");

            DecisionYesNo = false;
            Card bullet = PlayCard("BulletTime");

            //If both Guise and Haste say no to a Haste card, only Haste should add 2 tokens
            QuickTokenPoolStorage(SpeedPool);
            DestroyCard(bullet);
            AssertInTrash(bullet);
            QuickTokenPoolCheck(2);
        }

        [Test()]
        public void TestQuickReactionsDoNotMoveCardHasteCopy4()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 3 tokens to your speed pool.
            //When a hero ongoing or equipment would be destroyed, you may remove 3 tokens from your speed pool. If you do, put that card on top of its deck. Otherwise, if it is one of your cards, add 2 tokens to your speed pool.

            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");
            Card quick = PlayCard("QuickReactions");

            DecisionYesNo = false;
            Card kawaii = PlayCard("SuperUltraKawaii");

            //If both Guise and Haste say no to a Guise card, only Haste should add 2 tokens
            QuickTokenPoolStorage(SpeedPool);
            DestroyCard(kawaii);
            AssertInTrash(kawaii);
            QuickTokenPoolCheck(2);
        }

        [Test()]
        public void TestRunningCirclesDiscard()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 3 tokens to your speed pool.
            //When you discard a card, add a token to your speed pool.
            //At the end of your turn, you may discard a card and draw a card.

            QuickTokenPoolStorage(SpeedPool);
            Card running = PlayCard("RunningCircles");
            QuickTokenPoolCheck(3);

            QuickTokenPoolStorage(SpeedPool);
            DiscardCard(haste);
            QuickTokenPoolCheck(1);
        }

        [Test()]
        public void TestRunningCirclesDiscardCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 3 tokens to your speed pool.
            //When you discard a card, add a token to your speed pool.
            //At the end of your turn, you may discard a card and draw a card.

            Card running = PlayCard("RunningCircles");

            DecisionSelectTurnTaker = haste.TurnTaker;
            QuickTokenPoolStorage(SpeedPool);
            Card uhyeah = PlayCard("UhYeahImThatGuy");
            QuickTokenPoolCheck(3);

            QuickTokenPoolStorage(SpeedPool);
            DiscardCard(guise);
            QuickTokenPoolCheck(1);
        }

        [Test()]
        public void TestRunningCirclesEndOfTurn()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 3 tokens to your speed pool.
            //When you discard a card, add a token to your speed pool.
            //At the end of your turn, you may discard a card and draw a card.

            Card running = PlayCard("RunningCircles");

            Card bullet = PutOnDeck("BulletTime");
            Card killing = PutInHand("KillingTime");
            DecisionDiscardCard = killing;
            GoToEndOfTurn(haste);
            AssertInTrash(killing);
            AssertInHand(bullet);
        }

        [Test()]
        public void TestScoutingAhead()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 4 tokens to your speed pool.
            //At the end of your turn, you may remove 2 tokens from your speed pool. If you do, discard the top card of a deck or reveal and replace it, then repeat this text.

            QuickTokenPoolStorage(SpeedPool);
            Card scout = PlayCard("ScoutingAhead");
            QuickTokenPoolCheck(4);

            AddTokensToPool(SpeedPool, 10);

            DecisionsYesNo = new bool[] {true,true,false};

            DecisionSelectFunctions = new int?[] { 0, 1 };

            DecisionSelectLocation = new LocationChoice(akash.TurnTaker.Deck);

            Card entomb = PutOnDeck("Entomb");
            Card rockslide = PutOnDeck("LivingRockslide");

            //First discard Living Rockslide from Akash's deck, then reveal and replace Entomb, then stop
            QuickTokenPoolStorage(SpeedPool);
            GoToEndOfTurn(haste);
            QuickTokenPoolCheck(-4);
            AssertInTrash(rockslide);
            AssertOnTopOfDeck(entomb);
            Assert.IsTrue(FindCardController(entomb).IsPositionKnown);
        }

        [Test()]
        public void TestScoutingAheadOblivaeon()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis", "Megalopolis" ,"MobileDefensePlatform", "RuinsOfAtlantis", "RookCity" }, shieldIdentifier: "ThePrimaryObjective");
            StartGame();
            //When this card enters play, add 4 tokens to your speed pool.
            //At the end of your turn, you may remove 2 tokens from your speed pool. If you do, discard the top card of a deck or reveal and replace it, then repeat this text.

            GoToPlayCardPhase(haste);

            Card scout = PlayCard("ScoutingAhead");

            Card elemental = MoveCard(oblivaeon, "ElementalPandemonium", scionDeck);

            SwitchBattleZone(bunker);

            QuickTokenPoolStorage(SpeedPool);

            DecisionsYesNo = new bool[] {true, false };

            DecisionSelectFunction =  0;
            
            DecisionSelectLocation = new LocationChoice(scionDeck);
            //Haste should see the decks of Haste, Legacy, Scholar, Insula Primalis, OblivAeon, and Scion
            IEnumerable<Location> include = new Location[] { scionDeck, oblivaeon.TurnTaker.Deck, haste.TurnTaker.Deck, legacy.TurnTaker.Deck, FindEnvironment(bzOne).TurnTaker.Deck };
            IEnumerable<Location> exclude = new Location[] { aeonDeck, bunker.TurnTaker.Deck, FindEnvironment(bzTwo).TurnTaker.Deck };
            AssertNextDecisionChoices(include.Select((Location L) => new LocationChoice(L)), exclude.Select((Location L) => new LocationChoice(L)));
            QuickHPStorage(haste);

            GoToEndOfTurn(haste);
            AssertAtLocation(elemental, scionTrash);
        }

        [Test()]
        public void TestScoutingAheadOblivaeon2()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis", "Megalopolis", "MobileDefensePlatform", "RuinsOfAtlantis", "RookCity" }, shieldIdentifier: "ThePrimaryObjective");
            StartGame();
            //When this card enters play, add 4 tokens to your speed pool.
            //At the end of your turn, you may remove 2 tokens from your speed pool. If you do, discard the top card of a deck or reveal and replace it, then repeat this text.

            GoToPlayCardPhase(haste);

            Card scout = PlayCard("ScoutingAhead");

            Card elemental = MoveCard(oblivaeon, "ElementalPandemonium", scionDeck);

            SwitchBattleZone(bunker);

            SwitchBattleZone(haste);

            QuickTokenPoolStorage(SpeedPool);

            DecisionsYesNo = new bool[] { true, false };

            DecisionSelectFunction = 0;

            DecisionSelectLocation = new LocationChoice(scionDeck);
            //Haste should see the decks of Haste, Legacy, Scholar, Insula Primalis, OblivAeon, and Scion
            IEnumerable<Location> exclude = new Location[] { oblivaeon.TurnTaker.Deck, legacy.TurnTaker.Deck, FindEnvironment(bzOne).TurnTaker.Deck };
            IEnumerable<Location> include = new Location[] { scionDeck, aeonDeck, haste.TurnTaker.Deck, bunker.TurnTaker.Deck, FindEnvironment(bzTwo).TurnTaker.Deck };
            AssertNextDecisionChoices(include.Select((Location L) => new LocationChoice(L)), exclude.Select((Location L) => new LocationChoice(L)));
            QuickHPStorage(haste);

            GoToEndOfTurn(haste);
            AssertAtLocation(elemental, scionTrash);
        }

        [Test()]
        public void TestScoutingAheadCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When this card enters play, add 4 tokens to your speed pool.
            //At the end of your turn, you may remove 2 tokens from your speed pool. If you do, discard the top card of a deck or reveal and replace it, then repeat this text.
            GoToPlayCardPhase(guise);
            Card scout = PlayCard("ScoutingAhead");

            QuickTokenPoolStorage(SpeedPool);
            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");
            QuickTokenPoolCheck(4);

            AddTokensToPool(SpeedPool, 10);

            DecisionsYesNo = new bool[] { true, true, false };

            DecisionSelectFunctions = new int?[] { 0, 1 };

            DecisionSelectLocation = new LocationChoice(akash.TurnTaker.Deck);

            Card entomb = PutOnDeck("Entomb");
            Card rockslide = PutOnDeck("LivingRockslide");

            //First discard Living Rockslide from Akash's deck, then reveal and replace Entomb, then stop
            QuickTokenPoolStorage(SpeedPool);
            GoToEndOfTurn(guise);
            QuickTokenPoolCheck(-4);
            AssertInTrash(rockslide);
            AssertOnTopOfDeck(entomb);
            Assert.IsTrue(FindCardController(entomb).IsPositionKnown);
        }

        [Test()]
        public void TestStandingStillDamage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When {Haste} deals himself damage, he deals up to X targets other than himself 2 melee damage each, where X = the damage dealt this way.
            //Power: Add a token to your speed pool. {Haste} deals a target 2 melee damage.

            Card standing = PlayCard("StandingStill");
            Card rockslide = PlayCard("LivingRockslide");
            Card arboreal = PlayCard("ArborealPhalanges");
            Card brambles = PlayCard("EnsnaringBrambles");

            QuickHPStorage(haste.CharacterCard, akash.CharacterCard, rockslide, arboreal, brambles);
            AssertNextDecisionChoices(new Card[] { akash.CharacterCard, rockslide, arboreal,brambles, legacy.CharacterCard, bunker.CharacterCard, guise.CharacterCard }, new Card[] { haste.CharacterCard });
            DecisionSelectTargets = new Card[] { akash.CharacterCard, rockslide, arboreal, brambles };
            DealDamage(haste, haste, 3, DamageType.Melee);
            QuickHPCheck(-3, -2, -2, -2, 0);
        }

        [Test()]
        public void TestStandingStillPower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When {Haste} deals himself damage, he deals up to X targets other than himself 2 melee damage each, where X = the damage dealt this way.
            //Power: Add a token to your speed pool. {Haste} deals a target 2 melee damage.
            Card standing = PlayCard("StandingStill");
            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(akash);
            QuickTokenPoolStorage(SpeedPool);
            UsePower(standing);
            QuickTokenPoolCheck(1);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestStandingStillPowerCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When {Haste} deals himself damage, he deals up to X targets other than himself 2 melee damage each, where X = the damage dealt this way.
            //Power: Add a token to your speed pool. {Haste} deals a target 2 melee damage.
            Card standing = PlayCard("StandingStill");

            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");

            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(akash);
            QuickTokenPoolStorage(SpeedPool);
            UsePower(uhyeah);
            QuickTokenPoolCheck(1);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestStreakOfFistsPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //Remove any number of tokens from your speed pool.
            //{Haste} deals X targets 2 melee damage each, where X = 2 plus the number of tokens removed this way.
            //You may draw or play a card. If you draw a card this way, shuffle this card into your deck.
            AddTokensToPool(SpeedPool, 10);
            DecisionSelectNumber = 1;
            Card rockslide = PlayCard("LivingRockslide");
            Card arboreal = PlayCard("ArborealPhalanges");
            Card brambles = PlayCard("EnsnaringBrambles");
            Card standing = PutInHand("StandingStill");
            Card killing = PutOnDeck("KillingTime");

            DecisionSelectTargets = new Card[] { akash.CharacterCard, rockslide, arboreal, brambles };
            DecisionSelectFunction = 1;
            DecisionSelectCardToPlay = standing;

            QuickTokenPoolStorage(SpeedPool);
            QuickHPStorage(akash.CharacterCard, rockslide, arboreal, brambles);
            QuickShuffleStorage(haste);

            Card streak = PlayCard("StreakOfFists");

            QuickHPCheck(-2, -2, -2, 0);
            QuickTokenPoolCheck(-1);
            AssertIsInPlay(standing);
            AssertOnTopOfDeck(killing);
            AssertInTrash(streak);
            QuickShuffleCheck(0);
        }

        [Test()]
        public void TestStreakOfFistsDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //Remove any number of tokens from your speed pool.
            //{Haste} deals X targets 2 melee damage each, where X = 2 plus the number of tokens removed this way.
            //You may draw or play a card. If you draw a card this way, shuffle this card into your deck.
            AddTokensToPool(SpeedPool, 10);
            DecisionSelectNumber = 1;
            Card rockslide = PlayCard("LivingRockslide");
            Card arboreal = PlayCard("ArborealPhalanges");
            Card brambles = PlayCard("EnsnaringBrambles");
            Card standing = PutInHand("StandingStill");
            Card killing = PutOnDeck("KillingTime");

            DecisionSelectTargets = new Card[] { akash.CharacterCard, rockslide, arboreal, brambles };
            DecisionSelectFunction = 0;
            DecisionSelectCardToPlay = standing;

            QuickTokenPoolStorage(SpeedPool);
            QuickHPStorage(akash.CharacterCard, rockslide, arboreal, brambles);
            QuickShuffleStorage(haste);

            Card streak = PlayCard("StreakOfFists");

            QuickHPCheck(-2, -2, -2, 0);
            QuickTokenPoolCheck(-1);
            AssertInHand(standing);
            AssertInHand(killing);
            AssertInDeck(streak);
            QuickShuffleCheck(1);
        }

        [Test()]
        public void TestTheSandsOfTime()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //Increase damage dealt by {Haste} by 1.
            //At the end of your turn, {Haste} may deal himself 2 energy damage. If he takes no damage this way, destroy this card.
            Card sands = PlayCard("TheSandsOfTime");
            QuickHPStorage(akash);
            DealDamage(haste, akash, 1, DamageType.Melee);
            QuickHPCheck(-2);

            DecisionYesNo = true;
            QuickHPStorage(haste);
            GoToEndOfTurn(haste);
            QuickHPCheck(-3);
            AssertIsInPlay(sands);

            QuickHPStorage(haste);
            GoToPlayCardPhase(legacy);
            PlayCard("HeroicInterception");
            GoToEndOfTurn(haste);
            QuickHPCheck(0);
            AssertInTrash(sands);
        }

        [Test()]
        public void TestUnmatchedAlacrityReduce()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When you would add tokens to your speed pool, reduce the number of tokens added this way by 2, to a minimum of 0.
            //At the end of each turn, you may remove 1 token from your speed pool to draw a card, play a card, or use a power.
            //At the start of your turn, destroy this card.
            Card alacrity = GoToPlayCardPhaseAndPlayCard(haste, "UnmatchedAlacrity");
            QuickTokenPoolStorage(SpeedPool);
            PlayCard("ScoutingAhead");
            QuickTokenPoolCheck(2);

            Card standing = PlayCard("StandingStill");
            QuickTokenPoolStorage(SpeedPool);
            UsePower(standing);
            QuickTokenPoolCheck(0);
        }

        [Test()]
        public void TestUnmatchedAlacrityReduceCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When you would add tokens to your speed pool, reduce the number of tokens added this way by 2, to a minimum of 0.
            //At the end of each turn, you may remove 1 token from your speed pool to draw a card, play a card, or use a power.
            //At the start of your turn, destroy this card.
            Card alacrity = GoToPlayCardPhaseAndPlayCard(haste, "UnmatchedAlacrity");
            Card scouting = PlayCard("ScoutingAhead");

            QuickTokenPoolStorage(SpeedPool);
            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");
            QuickTokenPoolCheck(0);

            DestroyCard(scouting);

            Card standing = PlayCard("StandingStill");
            QuickTokenPoolStorage(SpeedPool);
            UsePower(uhyeah);
            QuickTokenPoolCheck(0);

            QuickTokenPoolStorage(SpeedPool);
            PlayCard(scouting);
            QuickTokenPoolCheck(0);

        }

        [Test()]
        public void TestUnmatchedAlacrityWithKillingTime()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();

            Card alacrity = GoToPlayCardPhaseAndPlayCard(haste, "UnmatchedAlacrity");
            Card killing = PlayCard("KillingTime");

            QuickTokenPoolStorage(SpeedPool);
            Card instant = PlayCard("InstantAnalysis");
            QuickTokenPoolCheck(1);

            QuickTokenPoolStorage(SpeedPool);
            Card standing = PlayCard("StandingStill");
            UsePower(standing);
            QuickTokenPoolCheck(0);
        }

        [Test()]
        public void TestUnmatchedAlacrityWithKillingTimeCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();

            Card alacrity = GoToPlayCardPhaseAndPlayCard(haste, "UnmatchedAlacrity");
            Card killing = PlayCard("KillingTime");
            Card scouting = PlayCard("ScoutingAhead");
            DecisionSelectTurnTaker = haste.TurnTaker;

            QuickTokenPoolStorage(SpeedPool);
            Card uhyeah = PlayCard("UhYeahImThatGuy");
            QuickTokenPoolCheck(2);
        }

        [Test()]
        public void TestUnmatchedAlacrityEndAndStart()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When you would add tokens to your speed pool, reduce the number of tokens added this way by 2, to a minimum of 0.
            //At the end of each turn, you may remove 1 token from your speed pool to draw a card, play a card, or use a power.
            //At the start of your turn, destroy this card.
            AddTokensToPool(SpeedPool, 10);
            Card alacrity = GoToPlayCardPhaseAndPlayCard(haste, "UnmatchedAlacrity");
            Card standing = PutOnDeck("StandingStill");

            DecisionSelectFunction = 0;

            QuickTokenPoolStorage(SpeedPool);
            GoToEndOfTurn(haste);
            QuickTokenPoolCheck(-1);
            AssertInHand(standing);

            DecisionSelectFunction = 1;
            DecisionSelectCardToPlay = standing;
            QuickTokenPoolStorage(SpeedPool);
            GoToEndOfTurn(legacy);
            QuickTokenPoolCheck(-1);
            AssertIsInPlay(standing);

            DecisionSelectFunction = 2;
            DecisionSelectPower = standing;
            QuickHPStorage(akash);
            QuickTokenPoolStorage(SpeedPool);
            GoToEndOfTurn(bunker);
            QuickHPCheck(-2);
            QuickTokenPoolCheck(-1);

            DecisionDoNotSelectFunction = true;

            QuickTokenPoolStorage(SpeedPool);
            GoToStartOfTurn(haste);
            QuickTokenPoolCheck(0);
            AssertInTrash(alacrity);
        }

        [Test()]
        public void TestUnmatchedAlacrityEndCopy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //When you would add tokens to your speed pool, reduce the number of tokens added this way by 2, to a minimum of 0.
            //At the end of each turn, you may remove 1 token from your speed pool to draw a card, play a card, or use a power.
            //At the start of your turn, destroy this card.
            AddTokensToPool(SpeedPool, 10);
            Card alacrity = GoToPlayCardPhaseAndPlayCard(guise, "UnmatchedAlacrity");

            DecisionSelectTurnTaker = haste.TurnTaker;
            Card uhyeah = PlayCard("UhYeahImThatGuy");

            DecisionSelectFunction = 0;

            Card kawaii = PutOnDeck("SuperUltraKawaii");
            Card standing = PutOnDeck("StandingStill");

            QuickTokenPoolStorage(SpeedPool);
            GoToEndOfTurn(guise);
            QuickTokenPoolCheck(-2);
            AssertInHand(standing);
            AssertInHand(kawaii);
        }

        [Test()]
        public void TestViolatingCausality()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //Add 2 tokens to your speed pool.
            //You may remove a token from your speed pool to put the top card of another trash into play or on the bottom of its deck.
            //If a card enters play this way, {Haste} deals himself 2 energy damage. Otherwise, repeat this text.

            DecisionYesNo = true;

            Card danger = PutInTrash("DangerSense");
            Card fortitude = PutInTrash("Fortitude");
            Card rockslide = PutInTrash("LivingRockslide");
            Card entomb = PutInTrash("Entomb");
            Card standing = PutInTrash("StandingStill");
            Card obsidian = PutInTrash("ObsidianField");

            DecisionSelectFunctions = new int?[] {1,0};
            DecisionSelectCards = new Card[] { entomb, fortitude };
            QuickTokenPoolStorage(SpeedPool);
            AssertNextDecisionChoices(new Card[] { fortitude, entomb, obsidian }, new Card[] { danger, rockslide, standing });
            QuickHPStorage(haste);

            Card causality = PlayCard("ViolatingCausality");
            AssertOnBottomOfDeck(entomb);
            AssertIsInPlay(fortitude);
            QuickHPCheck(-2);
            QuickTokenPoolCheck(0);
        }

        [Test()]
        public void TestViolatingCausalityNoAdd()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "Guise", "InsulaPrimalis");
            StartGame();
            //Add 2 tokens to your speed pool.
            //You may remove a token from your speed pool to put the top card of another trash into play or on the bottom of its deck.
            //If a card enters play this way, {Haste} deals himself 2 energy damage. Otherwise, repeat this text.

            DecisionYesNo = false;

            Card danger = PutInTrash("DangerSense");
            Card fortitude = PutInTrash("Fortitude");
            Card rockslide = PutInTrash("LivingRockslide");
            Card entomb = PutInTrash("Entomb");
            Card standing = PutInTrash("StandingStill");
            Card obsidian = PutInTrash("ObsidianField");

            DecisionSelectFunctions = new int?[] { 1, 0 };
            DecisionSelectCards = new Card[] { entomb, fortitude };
            QuickTokenPoolStorage(SpeedPool);
            AssertNextDecisionChoices(new Card[] { fortitude, entomb, obsidian }, new Card[] { danger, rockslide, standing });
            QuickHPStorage(haste);

            Card causality = PlayCard("ViolatingCausality");
            AssertInTrash(entomb);
            AssertInTrash(fortitude);
            QuickHPCheck(0);
            QuickTokenPoolCheck(2);
        }

        [Test()]
        public void TestViolatingCausalityOblivaeon()
        {
            SetupGameController("OblivAeon", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis", "Megalopolis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios");
            StartGame();
            //Add 2 tokens to your speed pool.
            //You may remove a token from your speed pool to put the top card of another trash into play or on the bottom of its deck.
            //If a card enters play this way, {Haste} deals himself 2 energy damage. Otherwise, repeat this text.

            DecisionYesNo = true;

            Card fortitude = PutInTrash("Fortitude");
            Card ammo = PutInTrash("AmmoDrop");
            Card mortal = PutInTrash("MortalFormToEnergy");
            Card standing = PutInTrash("StandingStill");
            Card obsidian = PutInTrash("ObsidianField");
            Card hostage = PutInTrash("HostageSituation");

            Card focus = PutInTrash("FocusOfPower");
            Card elemental = MoveCard(oblivaeon, "ElementalPandemonium", scionTrash);
            Card thrall = MoveCard(oblivaeon, "AeonThrall", aeonTrash);

            Card locus = PlayCard(oblivaeon, "AeonLocus");

            SwitchBattleZone(bunker);
            
            QuickTokenPoolStorage(SpeedPool);

            DecisionSelectFunction = 0;
            DecisionSelectCard = fortitude;
            //Haste should see the trashes of Legacy, Scholar, OblivAeon, Aeon Men, Scion, and Insula Primalis
            AssertNextDecisionChoices(new Card[] { fortitude, mortal, focus, elemental, thrall, obsidian }, new Card[] { ammo, hostage, standing });
            QuickHPStorage(haste);

            Card causality = PlayCard("ViolatingCausality");
            AssertIsInPlay(fortitude);
            QuickHPCheck(-2);
            QuickTokenPoolCheck(1);
        }

        [Test()]
        public void TestViolatingCausalityOblivaeon2()
        {
            SetupGameController("OblivAeon", "VainFacadePlaytest.Haste", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis", "Megalopolis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios");
            StartGame();
            //Add 2 tokens to your speed pool.
            //You may remove a token from your speed pool to put the top card of another trash into play or on the bottom of its deck.
            //If a card enters play this way, {Haste} deals himself 2 energy damage. Otherwise, repeat this text.

            DecisionYesNo = true;

            Card fortitude = PutInTrash("Fortitude");
            Card ammo = PutInTrash("AmmoDrop");
            Card mortal = PutInTrash("MortalFormToEnergy");
            Card standing = PutInTrash("StandingStill");
            Card obsidian = PutInTrash("ObsidianField");
            Card hostage = PutInTrash("HostageSituation");

            Card focus = PutInTrash("FocusOfPower");
            Card elemental = MoveCard(oblivaeon, "ElementalPandemonium", scionTrash);
            Card thrall = MoveCard(oblivaeon, "AeonThrall", aeonTrash);

            Card locus = PlayCard(oblivaeon, "AeonLocus");

            SwitchBattleZone(bunker);
            SwitchBattleZone(haste);

            QuickTokenPoolStorage(SpeedPool);

            DecisionSelectFunction = 0;
            DecisionSelectCard = ammo;
            //Haste should see the trashes of Bunker, Megalopolis, and Scion
            AssertNextDecisionChoices(new Card[] { ammo, hostage, elemental }, new Card[] { fortitude, mortal, focus, thrall, obsidian, standing });
            QuickHPStorage(haste);

            Card causality = PlayCard("ViolatingCausality");
            AssertIsInPlay(ammo);
            QuickHPCheck(-2);
            QuickTokenPoolCheck(1);
        }
    }
}

