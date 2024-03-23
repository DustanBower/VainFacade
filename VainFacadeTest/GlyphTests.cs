using NUnit.Framework;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.UnitTest;
using System.Linq;
using VainFacadePlaytest.Glyph;
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
    public class GlyphTests : BaseTest
    {
        protected HeroTurnTakerController glyph { get { return FindHero("Glyph"); } }
        protected HeroTurnTakerController friday { get { return FindHero("Friday"); } }

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(glyph.CharacterCard, 1);
            DealDamage(villain, glyph, 2, DamageType.Melee, true);
        }

        [Test()]
        public void TestLoadGlyph()
        {
            SetupGameController("BaronBlade", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(glyph);
            Assert.IsInstanceOf(typeof(GlyphCharacterCardController), glyph.CharacterCardController);

            Assert.AreEqual(26, glyph.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestDestroyFaceDown()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            Card mal = PutInHand("MaledictionOfInaalaFate");
            DecisionSelectCard = mal;
            DecisionSelectTurnTaker = glyph.TurnTaker;
            UsePower(glyph);
            AssertIsInPlay(mal);
            Assert.IsTrue(mal.IsFaceDownNonCharacter);

            DestroyCard(mal);
            AssertAtLocation(mal, glyph.TurnTaker.Trash);
        }

        [Test()]
        public void TestMoveFaceDown()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            Card mal = PutInHand("MaledictionOfInaalaFate");
            DecisionSelectCard = mal;
            DecisionSelectTurnTaker = glyph.TurnTaker;
            UsePower(glyph);
            AssertIsInPlay(mal);
            Assert.IsTrue(mal.IsFaceDownNonCharacter);

            MoveCard(null, mal, glyph.TurnTaker.FindSubDeck("GlyphFate"));
            AssertAtLocation(mal, glyph.TurnTaker.Deck);
        }

        [Test()]
        public void TestMoveFaceDownToBottom()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            Card mal = PutInHand("MaledictionOfInaalaFate");
            DecisionSelectCard = mal;
            DecisionSelectTurnTaker = glyph.TurnTaker;
            UsePower(glyph);
            AssertIsInPlay(mal);
            Assert.IsTrue(mal.IsFaceDownNonCharacter);

            MoveCard(null, mal, glyph.TurnTaker.FindSubDeck("GlyphFate"),true);
            AssertAtLocation(mal, glyph.TurnTaker.Deck,true);
        }

        [Test()]
        public void TestLaComodoraPower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "LaComodora", "InsulaPrimalis");
            StartGame();

            Card mal = PutInTrash("MaledictionOfInaalaFate");
            AssertAtLocation(mal, glyph.TurnTaker.Trash);

            DecisionSelectCard = mal;
            UsePower(comodora);
            AssertAtLocation(mal, glyph.TurnTaker.Deck);
            AssertOnTopOfDeck(mal, 1);
        }

        [Test()]
        public void TestF6BunkerIncap3()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker/BunkerFreedomSixCharacter", "LaComodora", "InsulaPrimalis");
            StartGame();

            Card mal = PutInTrash("MaledictionOfInaalaFate");
            AssertAtLocation(mal, glyph.TurnTaker.Trash);
            DestroyCard(bunker);

            //Put a card from a hero trash on top of its deck
            DecisionSelectCard = mal;
            UseIncapacitatedAbility(bunker,2);
            AssertAtLocation(mal, glyph.TurnTaker.Deck);
            AssertOnTopOfDeck(mal, 0);
        }

        [Test()]
        public void TestInnatePowerDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Draw up to 2 cards. You may play a card face-down in any play area or swap 2 of your face-down cards.
            QuickHandStorage(glyph);
            DecisionDoNotSelectFunction = true;
            UsePower(glyph);
            QuickHandCheck(2);
        }

        [Test()]
        public void TestInnatePowerPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Draw up to 2 cards. You may play a card face-down in any play area or swap 2 of your face-down cards.
            Card mal = PutOnDeck("MaledictionOfInaalaFate");
            QuickHandStorage(glyph);
            DecisionSelectFunction = 0;
            DecisionSelectCard = mal;
            DecisionSelectTurnTaker = akash.TurnTaker;
            UsePower(glyph);
            QuickHandCheck(1);
            AssertAtLocation(mal, akash.TurnTaker.PlayArea);
            Assert.IsTrue(mal.IsFaceDownNonCharacter);
        }

        [Test()]
        public void TestIncap1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Reveal and replace the top card of a deck. 1 player may play a card.
            Card fortitude = PutInHand("Fortitude");
            SetupIncap(akash);
            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionSelectCard = fortitude;
            UseIncapacitatedAbility(glyph, 0);
            AssertIsInPlay(fortitude);
        }

        [Test()]
        public void TestIncap2Draw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "Fanatic", "InsulaPrimalis");
            StartGame();

            //1 player draws a card or puts a relic, spell, ritual, rune, or glyph from their trash on top of their deck.
            SetupIncap(glyph);
            Card chastise = PutOnDeck("Chastise");
            DecisionSelectFunction = 0;
            DecisionSelectTurnTaker = fanatic.TurnTaker;
            UseIncapacitatedAbility(glyph, 1);
            AssertInHand(chastise);
        }

        [Test()]
        public void TestIncap2Move()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "NightMist", "InsulaPrimalis");
            StartGame();

            //1 player draws a card or puts a relic, spell, ritual, rune, or glyph from their trash on top of their deck.
            SetupIncap(glyph);
            Card amulet = PutInTrash("AmuletOfTheElderGods");
            Card obliv = PutInTrash("Oblivion");
            DecisionSelectFunction = 1;
            DecisionSelectTurnTaker = mist.TurnTaker;
            DecisionSelectCard = obliv;
            AssertNextDecisionChoices(new Card[] { amulet, obliv });
            UseIncapacitatedAbility(glyph, 1);
            AssertOnTopOfDeck(obliv);
        }

        [Test()]
        public void TestIncap3()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Destroy an environment card.
            Card volcano = PlayCard("VolcanicEruption");
            SetupIncap(akash);
            DecisionSelectCard = volcano;
            UseIncapacitatedAbility(glyph,2);
            AssertInTrash(volcano);
        }

        [Test()]
        public void TestMissionControl()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "FreedomTower");
            StartGame();

            //Mission control can't play A Brush with Death since it's limited, but it also doesn't know to discard it because of the way limited cards are handled for Glyph.
            //Glyph's character card has a trigger that moves any cards left in her revealed area to her trash.
            Card mission = PlayCard("MissionControl");
            Card brush1 = PlayCard("ABrushWithDeathDeath");
            Card brush2 = PutOnDeck("ABrushWithDeathFate");

            PutOnDeck("Fortitude");
            PutOnDeck("AmmoDrop");

            DecisionDoNotSelectFunction = true;

            GoToStartOfTurn(akash);
            //Console.WriteLine($"{brush2.Title} is at {brush2.Location.GetFriendlyName()}");
            AssertAtLocation(brush2, glyph.TurnTaker.Trash);
        }

        [Test()]
        public void TestABrushWithDeathBasePower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Glyph's base power should be able to play A Brush With Death face-down when the other copy is in play face-up
            DecisionYesNo = false;
            Card brush1 = PlayCard("ABrushWithDeathDeath");
            Card brush2 = PutInHand("ABrushWithDeathFate");
            DecisionSelectCard = brush2;
            DecisionSelectTurnTaker = glyph.TurnTaker;
            UsePower(glyph);
            AssertIsInPlay(brush2);
        }

        [Test()]
        public void TestABrushWithDeathSkipPlayPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the end of your turn, if you did not play a card, did not use a power, or did not draw a card this turn, or if {Glyph} has 10 or fewer hp, you may draw a card, play a card, or use a power.
            DecisionYesNo = false;
            Card brush = PlayCard("ABrushWithDeathDeath");
            Card sigil = PutInHand("SigiledBlade");
            DecisionDoNotSelectFunction = true;
            GoToStartOfTurn(glyph);
            UsePower(glyph);

            ResetDecisions();
            DecisionYesNo = false;
            DecisionSelectFunction = 1;
            DecisionSelectCardToPlay = sigil;
            GoToEndOfTurn(glyph);
            AssertIsInPlay(sigil);
        }

        [Test()]
        public void TestABrushWithDeathSkipPlayDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the end of your turn, if you did not play a card, did not use a power, or did not draw a card this turn, or if {Glyph} has 10 or fewer hp, you may draw a card, play a card, or use a power.
            DecisionYesNo = false;
            Card brush = PlayCard("ABrushWithDeathDeath");
            DecisionDoNotSelectFunction = true;
            GoToStartOfTurn(glyph);
            UsePower(glyph);

            ResetDecisions();
            DecisionYesNo = false;
            DecisionSelectFunction = 0;
            Card sigil = PutOnDeck("SigiledBlade");
            GoToEndOfTurn(glyph);
            AssertInHand(sigil);
        }

        [Test()]
        public void TestABrushWithDeathSkipPlayPower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the end of your turn, if you did not play a card, did not use a power, or did not draw a card this turn, or if {Glyph} has 10 or fewer hp, you may draw a card, play a card, or use a power.
            DecisionYesNo = false;
            Card brush = PlayCard("ABrushWithDeathDeath");
            Card sigil = PlayCard("SigiledBlade");
            DecisionDoNotSelectFunction = true;
            GoToStartOfTurn(glyph);
            UsePower(glyph);

            ResetDecisions();
            DecisionYesNo = false;
            DecisionSelectFunction = 2;
            DecisionSelectPower = sigil;
            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(akash);
            DecisionSelectNumber = 1;
            GoToEndOfTurn(glyph);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestABrushWithDeathSkipDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the end of your turn, if you did not play a card, did not use a power, or did not draw a card this turn, or if {Glyph} has 10 or fewer hp, you may draw a card, play a card, or use a power.
            DecisionYesNo = false;
            Card brush = PlayCard("ABrushWithDeathDeath");
            DecisionDoNotSelectFunction = true;
            GoToStartOfTurn(glyph);
            Card sigil = PlayCard("SigiledBlade");
            UsePower(sigil);

            ResetDecisions();
            DecisionYesNo = false;
            DecisionSelectFunction = 0;
            Card sphere = PutOnDeck("SphereOfNalatukha");
            GoToEndOfTurn(glyph);
            AssertInHand(sphere);
        }

        [Test()]
        public void TestABrushWithDeathSkipPower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the end of your turn, if you did not play a card, did not use a power, or did not draw a card this turn, or if {Glyph} has 10 or fewer hp, you may draw a card, play a card, or use a power.
            DecisionYesNo = false;
            DecisionDoNotSelectFunction = true;
            GoToStartOfTurn(glyph);
            Card brush = PlayCard("ABrushWithDeathDeath");
            DrawCard(glyph);

            ResetDecisions();
            DecisionYesNo = false;
            DecisionSelectFunction = 0;
            Card sphere = PutOnDeck("SphereOfNalatukha");
            GoToEndOfTurn(glyph);
            AssertInHand(sphere);
        }

        [Test()]
        public void TestABrushWithDeath10HP()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the end of your turn, if you did not play a card, did not use a power, or did not draw a card this turn, or if {Glyph} has 10 or fewer hp, you may draw a card, play a card, or use a power.
            DecisionYesNo = false;
            Card brush = PlayCard("ABrushWithDeathDeath");
            DecisionDoNotSelectFunction = true;
            GoToStartOfTurn(glyph);
            Card sigil = PlayCard("SigiledBlade");
            UsePower(sigil);
            DrawCard(glyph);
            SetHitPoints(glyph, 10);

            ResetDecisions();
            DecisionYesNo = false;
            DecisionSelectFunction = 0;
            Card sphere = PutOnDeck("SphereOfNalatukha");
            GoToEndOfTurn(glyph);
            AssertInHand(sphere);
        }

        [Test()]
        public void TestABrushWithDeathNoSkip()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the end of your turn, if you did not play a card, did not use a power, or did not draw a card this turn, or if {Glyph} has 10 or fewer hp, you may draw a card, play a card, or use a power.
            DecisionYesNo = false;
            Card brush = PlayCard("ABrushWithDeathDeath");
            DecisionDoNotSelectFunction = true;
            GoToStartOfTurn(glyph);
            Card sigil = PlayCard("SigiledBlade");
            UsePower(sigil);
            DrawCard(glyph);

            ResetDecisions();
            DecisionYesNo = false;
            DecisionSelectFunction = 0;
            Card sphere = PutOnDeck("SphereOfNalatukha");
            GoToEndOfTurn(glyph);
            AssertOnTopOfDeck(sphere);
        }

        [Test()]
        public void TestABrushWithDeathNoSkipFaceDownPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the end of your turn, if you did not play a card, did not use a power, or did not draw a card this turn, or if {Glyph} has 10 or fewer hp, you may draw a card, play a card, or use a power.
            DecisionYesNo = false;
            Card brush = PlayCard("ABrushWithDeathDeath");
            DecisionDoNotSelectFunction = true;
            Card sigil = PlayCard("SigiledBlade");
            GoToStartOfTurn(glyph);

            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            PlayCard("ReturnToSilenceFate");
            
            UsePower(sigil);
            DrawCard(glyph);

            ResetDecisions();
            DecisionYesNo = false;
            DecisionSelectFunction = 0;
            Card sphere = PutOnDeck("SphereOfNalatukha");
            GoToEndOfTurn(glyph);
            AssertOnTopOfDeck(sphere);
        }

        [Test()]
        public void TestABrushWithDeathImpulsionBeamFaceDownPlay()
        {
            SetupGameController("BaronBladeTeam", "ErmineTeam","FrightTrainTeam", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "InsulaPrimalis");
            StartGame();

            //At the end of your turn, if you did not play a card, did not use a power, or did not draw a card this turn, or if {Glyph} has 10 or fewer hp, you may draw a card, play a card, or use a power.
            DecisionYesNo = false;
            DecisionDoNotSelectFunction = true;
            PlayCard("ImpulsionBeam");
            GoToStartOfTurn(glyph);

            DecisionYesNo = false;
            DecisionSelectTurnTaker = glyph.TurnTaker;
            Card brush = PlayCard("ABrushWithDeathDeath");

            DecisionYesNo = false;
            Card sigil = PlayCard("SigiledBlade");
        }

        [Test()]
        public void TestEmbroideredCloakHeal()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //The first time each turn {Glyph} is dealt damage, she regains 1 HP.
            //At the start of your turn, you may discard a card. If you do, {Glyph} is immune to damage, cards in {Glyph}'s play area can only be destroyed by {Glyph}, and cards from other decks cannot be played this turn.
            DecisionYesNo = false;
            Card cloak = PlayCard("EmbroideredCloak");
            SetHitPoints(glyph, 10);

            QuickHPStorage(glyph);
            DealDamage(akash, glyph, 2, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(glyph);
            DealDamage(akash, glyph, 2, DamageType.Melee);
            QuickHPCheck(-2);

            GoToStartOfTurn(legacy);
            QuickHPStorage(glyph);
            DealDamage(akash, glyph, 2, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestEmbroideredCloakStartOfTurn()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //The first time each turn {Glyph} is dealt damage, she regains 1 HP.
            //At the start of your turn, you may discard a card. If you do, {Glyph} is immune to damage, cards in {Glyph}'s play area can only be destroyed by {Glyph}, and cards from other decks cannot be played this turn.
            Card cloak = PlayCard("EmbroideredCloak");
            DecisionDiscardCard = PutInHand("SigiledBlade");
            GoToStartOfTurn(glyph);

            //Check that glyph is immune to damage
            QuickHPStorage(glyph);
            DealDamage(akash, glyph, 10, DamageType.Melee);
            QuickHPCheck(0);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(akash);
            DealDamage(glyph, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that cards in Glyph's play area can only be destroyed by Glyph
            DecisionYesNo = false;
            Card sigil = PlayCard("SigiledBlade");
            DestroyCard(sigil,akash.CharacterCard);
            AssertIsInPlay(sigil);
            DestroyCard(sigil, glyph.CharacterCard);
            AssertInTrash(sigil);

            DecisionYesNo = true;
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card mds = PlayCard(glyph,"MilektithasDarkSummonsFate");
            DestroyCard(mds, akash.CharacterCard);
            AssertInTrash(mds);

            //Check that cards from other decks cannot be played
            Card rockslide = PlayCard("LivingRockslide");
            AssertNotInPlay(rockslide);

            //Check that it expires on the next turn
            GoToStartOfTurn(legacy);
            QuickHPStorage(glyph);
            DealDamage(akash, glyph, 2, DamageType.Melee);
            QuickHPCheck(-1); //Glyph heals 1 due to Cloak

            DecisionYesNo = false;
            PlayCard(sigil);
            DestroyCard(sigil, akash.CharacterCard);
            AssertInTrash(sigil);

            PlayCard(rockslide);
            AssertIsInPlay(rockslide);
        }

        [Test()]
        public void TestEmbroideredCloakStartOfTurnAfterDestroyed()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //The first time each turn {Glyph} is dealt damage, she regains 1 HP.
            //At the start of your turn, you may discard a card. If you do, {Glyph} is immune to damage, cards in {Glyph}'s play area can only be destroyed by {Glyph}, and cards from other decks cannot be played this turn.
            Card cloak = PlayCard("EmbroideredCloak");
            DecisionDiscardCard = PutInHand("SigiledBlade");
            GoToStartOfTurn(glyph);

            //If Embroidered Cloak is destroyed, the effects should persist for this turn
            DestroyCard(cloak, glyph.CharacterCard);

            //Check that glyph is immune to damage
            QuickHPStorage(glyph);
            DealDamage(akash, glyph, 10, DamageType.Melee);
            QuickHPCheck(0);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(akash);
            DealDamage(glyph, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that cards in Glyph's play area can only be destroyed by Glyph
            DecisionYesNo = false;
            Card sigil = PlayCard("SigiledBlade");
            DestroyCard(sigil, akash.CharacterCard);
            AssertIsInPlay(sigil);
            DestroyCard(sigil, glyph.CharacterCard);
            AssertInTrash(sigil);

            DecisionYesNo = true;
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card mds = PlayCard(glyph, "MilektithasDarkSummonsFate");
            DestroyCard(mds, akash.CharacterCard);
            AssertInTrash(mds);

            //Check that cards from other decks cannot be played
            Card rockslide = PlayCard("LivingRockslide");
            AssertNotInPlay(rockslide);

            //Check that it expires on the next turn
            GoToStartOfTurn(legacy);
            QuickHPStorage(glyph);
            DealDamage(akash, glyph, 2, DamageType.Melee);
            QuickHPCheck(-2); //Glyph does not heal 1 due to Cloak being destroyed

            DecisionYesNo = false;
            PlayCard(sigil);
            DestroyCard(sigil, akash.CharacterCard);
            AssertInTrash(sigil);

            PlayCard(rockslide);
            AssertIsInPlay(rockslide);
        }

        [Test()]
        public void TestInscrutableTutelagePlay1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When this card enters play, you may draw a card or play a relic.
            DecisionSelectFunction = 0;
            Card sigil = PutInHand("SigiledBlade");
            DecisionSelectCardToPlay = sigil;
            Card cloak = PutOnDeck("EmbroideredCloak");

            PlayCard("InscrutableTutelageFate");
            AssertInHand(sigil,cloak);
        }

        [Test()]
        public void TestInscrutableTutelagePlay2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When this card enters play, you may draw a card or play a relic.
            DecisionSelectFunction = 1;
            Card sigil = PutInHand("SigiledBlade");
            DecisionSelectCardToPlay = sigil;
            Card cloak = PutOnDeck("EmbroideredCloak");

            PlayCard("InscrutableTutelageFate");
            AssertIsInPlay(sigil);
            AssertOnTopOfDeck(cloak);
        }

        [Test()]
        public void TestInscrutableTutelagePlaySkip()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When this card enters play, you may draw a card or play a relic.
            DecisionDoNotSelectFunction = true;
            Card sigil = PutInHand("SigiledBlade");
            DecisionSelectCardToPlay = sigil;
            Card cloak = PutOnDeck("EmbroideredCloak");

            PlayCard("InscrutableTutelageFate");
            AssertInHand(sigil);
            AssertOnTopOfDeck(cloak);
        }

        [Test()]
        public void TestInscrutableTutelage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //The first time each turn damage would be dealt to any target in an area containing your face-down cards, you may increase or reduce that damage by 1.
            DecisionDoNotSelectFunction = true;
            Card tutelage = PlayCard("InscrutableTutelageFate");

            ResetDecisions();
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            PlayCard(glyph,"SigiledBlade");
            Card rockslide = PlayCard("LivingRockslide");

            //Test increase functionality
            DecisionSelectFunction = 1;
            QuickHPStorage(akash.CharacterCard,rockslide);
            DealDamage(legacy, akash, 2, DamageType.Melee);
            DealDamage(legacy, rockslide, 2, DamageType.Melee);
            QuickHPCheck(-1, -2);

            //Test decrease functionality
            GoToStartOfTurn(glyph);
            DecisionSelectFunction = 0;
            QuickHPStorage(akash.CharacterCard, rockslide);
            DealDamage(legacy, akash, 2, DamageType.Melee);
            DealDamage(legacy, rockslide, 2, DamageType.Melee);
            QuickHPCheck(-3, -2);

            //Test skip
            GoToStartOfTurn(legacy);
            DecisionDoNotSelectFunction = true;
            QuickHPStorage(akash.CharacterCard, rockslide);
            DealDamage(legacy, akash, 2, DamageType.Melee);
            ResetDecisions();
            DecisionSelectFunction = 1;
            DealDamage(legacy, rockslide, 2, DamageType.Melee);
            QuickHPCheck(-2, -2);
        }

        [Test()]
        public void TestMaledictionOfInaala()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Discard up to 3 cards. {Glyph} deals 1 target X times 2 infernal damage where X = the number of cards discarded in this way.
            //You may destroy 1 of your face-down might cards. If you do, you may discard any number of cards instead.
            MoveAllCards(glyph, glyph.HeroTurnTaker.Hand, glyph.TurnTaker.Trash);
            Card cloak = PutInHand("EmbroideredCloak");
            Card inscrutable = PutInHand("InscrutableTutelageFate");
            Card silence = PutInHand("ReturnToSilenceFate");
            Card inscrutable2 = PutInHand("InscrutableTutelageInsight");
            Card silence2 = PutInHand("ReturnToSilenceInsight");

            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(akash);
            PlayCard("MaledictionOfInaalaFate");
            AssertInTrash(cloak, inscrutable, silence);
            AssertInHand(inscrutable2, silence2);
            QuickHPCheck(-6);
        }

        [Test()]
        public void TestMaledictionOfInaalaDestroyMight()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Discard up to 3 cards. {Glyph} deals 1 target X times 2 infernal damage where X = the number of cards discarded in this way.
            //You may destroy 1 of your face-down might cards. If you do, you may discard any number of cards instead.
            MoveAllCards(glyph, glyph.HeroTurnTaker.Hand, glyph.TurnTaker.Trash);
            Card cloak = PutInHand("EmbroideredCloak");
            Card inscrutable = PutInHand("InscrutableTutelageFate");
            Card silence = PutInHand("ReturnToSilenceFate");
            Card inscrutable2 = PutInHand("InscrutableTutelageInsight");
            Card silence2 = PutInHand("ReturnToSilenceInsight");

            DecisionYesNo = true;
            DecisionSelectTurnTaker = glyph.TurnTaker;
            Card sigil = PlayCard(glyph, "SigiledBlade");

            ResetDecisions();
            DecisionDestroyCard = sigil;
            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(akash);
            PlayCard("MaledictionOfInaalaFate");
            AssertInTrash(cloak, inscrutable, silence, inscrutable2, silence2);
            QuickHPCheck(-10);
        }

        [Test()]
        public void TestMilektithasDarkSummons()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //You may destroy 1 of your face-down might cards. If you do, {Glyph} deals each target in that card's play area 2 infernal damage.
            //Destroy any number of your face-down destruction cards. For each card destroyed this way, {Glyph} deals a target in that play area that has not been dealt damage this way 5 cold damage.
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card sigil = PlayCard(glyph,"SigiledBlade");
            Card mal = PlayCard(glyph, "MaledictionOfInaalaDestruction");
            Card minetka = PlayCard(glyph, "MinetkasProclamationDestruction");

            ResetDecisions();

            Card rockslide = PlayCard("LivingRockslide");
            QuickHPStorage(akash.CharacterCard, rockslide);
            Card mds = PlayCard("MilektithasDarkSummonsFate");
            QuickHPCheck(-7, -7);
        }

        [Test()]
        public void TestMinetkasProclamationReduce()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When this card enters play, destroy any number of your face-down fate cards. For each card destroyed this way, increase or reduce damage dealt by a target in that play area by 1 until this card leaves play.
            //At the start of your turn, destroy 1 of your face-down might cards in the play area of a target affected this way or destroy this card.
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card inscrutable = PlayCard("InscrutableTutelageFate");
            Card mal = PlayCard("MaledictionOfInaalaFate");

            DecisionSelectFunction = 1;
            DecisionYesNo = false;
            Card minetka = PlayCard("MinetkasProclamationDeath");
            AssertInTrash(inscrutable, mal);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-1);

            DecisionYesNo = true;
            Card sigil = PlayCard("SigiledBlade");
            GoToStartOfTurn(glyph);
            AssertInTrash(sigil);
            AssertIsInPlay(minetka);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-1);

            GoToPlayCardPhase(glyph);

            DecisionSelectTurnTaker = legacy.TurnTaker;
            PlayCard(sigil);

            GoToStartOfTurn(glyph);
            AssertInTrash(minetka);
            AssertIsInPlay(sigil);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestMinetkasProclamationIncrease()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When this card enters play, destroy any number of your face-down fate cards. For each card destroyed this way, increase or reduce damage dealt by a target in that play area by 1 until this card leaves play.
            //At the start of your turn, destroy 1 of your face-down might cards in the play area of a target affected this way or destroy this card.
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card inscrutable = PlayCard("InscrutableTutelageFate");
            Card mal = PlayCard("MaledictionOfInaalaFate");

            DecisionSelectFunction = 0;
            DecisionYesNo = false;
            Card minetka = PlayCard("MinetkasProclamationDeath");
            AssertInTrash(inscrutable, mal);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(-3);

            DecisionYesNo = true;
            Card sigil = PlayCard("SigiledBlade");
            GoToStartOfTurn(glyph);
            AssertInTrash(sigil);
            AssertIsInPlay(minetka);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(-3);

            GoToPlayCardPhase(glyph);

            DecisionSelectTurnTaker = legacy.TurnTaker;
            PlayCard(sigil);

            GoToStartOfTurn(glyph);
            AssertInTrash(minetka);
            AssertIsInPlay(sigil);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestOccludedMarionettePutOnTop()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //You may put a non-indestructible card on top of its deck.
            //You may destroy 1 of your face-down might cards to put the top card of of a deck in that card's play area into play or shuffle that deck. If a card entered play this way, cards cannot be played from that deck until the start of your next turn.
            DecisionYesNo = false;
            Card rockslide = PlayCard("LivingRockslide");
            DecisionSelectCard = rockslide;
            Card occluded = PlayCard("OccludedMarionetteInsight");
            AssertOnTopOfDeck(rockslide);
        }

        [Test()]
        public void TestOccludedMarionettePutOnTopGlyphFaceDown()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //You may put a non-indestructible card on top of its deck.
            //You may destroy 1 of your face-down might cards to put the top card of of a deck in that card's play area into play or shuffle that deck. If a card entered play this way, cards cannot be played from that deck until the start of your next turn.
            DecisionYesNo = true;
            Card brush = PlayCard("ABrushWithDeathDeath");
            DecisionSelectCard = brush;
            DecisionYesNo = false;
            Card occluded = PlayCard("OccludedMarionetteInsight");
            AssertAtLocation(brush,glyph.TurnTaker.Deck);
        }

        [Test()]
        public void TestOccludedMarionetteDestroyMightPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //You may put a non-indestructible card on top of its deck.
            //You may destroy 1 of your face-down might cards to put the top card of of a deck in that card's play area into play or shuffle that deck. If a card entered play this way, cards cannot be played from that deck until the start of your next turn.
            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card sigil = PlayCard("SigiledBlade");
            Card fortitude = PlayCard("Fortitude");
            Card allies = PutOnDeck("AlliesOfTheEarth");

            DecisionSelectCard = fortitude;
            DecisionSelectFunction = 0;
            DecisionYesNo = false;
            Card occluded = PlayCard("OccludedMarionetteDeath");
            AssertIsInPlay(allies);
            Card rockslide = PlayCard("LivingRockslide");
            AssertNotInPlay(rockslide);
            GoToStartOfTurn(glyph);
            PlayCard(rockslide);
            AssertIsInPlay(rockslide);
        }

        [Test()]
        public void TestOccludedMarionetteDestroyMightShuffle()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //You may put a non-indestructible card on top of its deck.
            //You may destroy 1 of your face-down might cards to put the top card of of a deck in that card's play area into play or shuffle that deck. If a card entered play this way, cards cannot be played from that deck until the start of your next turn.
            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card sigil = PlayCard("SigiledBlade");
            Card fortitude = PlayCard("Fortitude");

            DecisionSelectCard = fortitude;
            DecisionSelectFunction = 1;
            DecisionYesNo = false;
            QuickShuffleStorage(akash);
            Card occluded = PlayCard("OccludedMarionetteInsight");
            QuickShuffleCheck(1);
            Card rockslide = PlayCard("LivingRockslide");
            AssertIsInPlay(rockslide);
        }

        [Test()]
        public void TestReturnToSilenceKeywords()
        {
            SetupGameController("Progeny", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //You may destroy 1 of your face-down might cards. If you do, each non-character, non-target card in that card's play area gains the ongoing keyword this turn.
            //Destroy any number of your face-down death cards. For each card destroyed this way, destroy an environment, ongoing, or equipment card in that card's play area.
            DecisionYesNo = true;
            DecisionSelectTurnTaker = progeny.TurnTaker;
            Card sigil = PlayCard(glyph, "SigiledBlade");

            Card scion = FindCard((Card c) => c.IsScion && c.IsInPlayAndHasGameText);

            ResetDecisions();
            Card silence = PlayCard("ReturnToSilenceFate");
            AssertInTrash(sigil);
            AssertCardHasKeyword(scion, "ongoing", true);

            Card frost = GetCard("ScionOfFrost",0,(Card c) => !c.IsInPlayAndHasGameText);
            PlayCard(frost);
            AssertCardHasKeyword(frost, "ongoing", true);

            DestroyCard(frost);
            AssertCardNoKeyword(frost, "ongoing");

            GoToStartOfTurn(glyph);
            AssertCardNoKeyword(scion, "ongoing");
            AssertCardNoKeyword(frost, "ongoing");
        }

        [Test()]
        public void TestReturnToSilenceDestroy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //You may destroy 1 of your face-down might cards. If you do, each non-character, non-target card in that card's play area gains the ongoing keyword this turn.
            //Destroy any number of your face-down death cards. For each card destroyed this way, destroy an environment, ongoing, or equipment card in that card's play area.
            DecisionYesNo = true;
            DecisionSelectTurnTaker = FindEnvironment().TurnTaker;
            Card mds = PlayCard(glyph, "MilektithasDarkSummonsDeath");
            Card minetka = PlayCard(glyph, "MinetkasProclamationDeath");

            ResetDecisions();
            PutOnDeck("AlliesOfTheEarth");
            Card raptor = PlayCard("VelociraptorPack");
            Card volcano = PlayCard("VolcanicEruption");
            Card plant = PlayCard("PrimordialPlantLife");

            Card silence = PlayCard("ReturnToSilenceFate");
            AssertInTrash(raptor, plant, mds, minetka);
            AssertIsInPlay(volcano);

        }

        [Test()]
        public void TestRitualCircleEndOfTurn()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the end of your turn, you may put a ritual from your hand beneath this card.
            //Power: Play any number of rituals from beneath this card.
            DecisionYesNo = false;
            Card circle = PlayCard("RitualCircleFate");
            Card silence = PutInHand("ReturnToSilenceFate");
            DecisionSelectCard = silence;
            DecisionYesNo = true;

            GoToEndOfTurn(glyph);
            AssertAtLocation(silence, circle.UnderLocation);
        }

        [Test()]
        public void TestRitualCirclePower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the end of your turn, you may put a ritual from your hand beneath this card.
            //Power: Play any number of rituals from beneath this card.
            DecisionYesNo = false;
            Card circle = PlayCard("RitualCircleFate");
            Card mds = GetCard("MilektithasDarkSummonsFate");
            Card minetka = GetCard("MinetkasProclamationDeath");
            Card sigil = GetCard("SigiledBlade");
            MoveCards(glyph, new Card[] {mds,minetka, sigil }, circle.UnderLocation);
            UsePower(circle);
            AssertIsInPlay(minetka);
            AssertInTrash(mds);
            AssertAtLocation(sigil, circle.UnderLocation);
        }

        [Test()]
        public void TestSigiledBlade()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Increase melee damage dealt by {Glyph} by 1.
            //Power: {Glyph} deals 1 target 0 or 1 melee damage. If {Glyph} is dealt damage this way, you may put one of your cards into play face-down in any play area for each point of damage dealt this way, then you may play a ritual.
            DecisionYesNo = false;
            Card sigil = PlayCard("SigiledBlade");

            QuickHPStorage(akash);
            DealDamage(glyph, akash, 1, DamageType.Melee);
            QuickHPCheck(-2);

            QuickHPStorage(akash);
            DealDamage(glyph, akash, 1, DamageType.Fire);
            QuickHPCheck(-1);

            QuickHPStorage(akash);
            DealDamage(legacy, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestSigiledBladePower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Increase melee damage dealt by {Glyph} by 1.
            //Power: {Glyph} deals 1 target 0 or 1 melee damage. If {Glyph} is dealt damage this way, you may put one of your cards into play face-down in any play area for each point of damage dealt this way, then you may play a ritual.
            DecisionYesNo = false;
            Card sigil = PlayCard("SigiledBlade");

            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;

            Card mds = PutInHand("MilektithasDarkSummonsFate");
            Card minetka = PutInHand("MinetkasProclamationDeath");
            Card circle = PutInHand("RitualCircleFate");

            DecisionSelectCards = new Card[] { mds, minetka, circle };
            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(akash);
            DecisionSelectNumber = 0;
            UsePower(sigil);
            QuickHPCheck(-1);
            AssertInHand(new Card[] { mds, minetka, circle }); 
        }

        [Test()]
        public void TestSigiledBladePowerGlyphDamage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Increase melee damage dealt by {Glyph} by 1.
            //Power: {Glyph} deals 1 target 0 or 1 melee damage. If {Glyph} is dealt damage this way, you may put one of your cards into play face-down in any play area for each point of damage dealt this way, then you may play a ritual.
            DecisionYesNo = false;
            Card sigil = PlayCard("SigiledBlade");

            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;
            MoveAllCards(glyph, glyph.HeroTurnTaker.Hand, glyph.TurnTaker.Trash);
            Card mds = PutInHand("MilektithasDarkSummonsFate");
            Card minetka = PutInHand("MinetkasProclamationDeath");
            Card circle = PutInHand("RitualCircleFate");

            //DecisionSelectCards = new Card[] { mds, minetka, circle };
            DecisionSelectTarget = glyph.CharacterCard;
            QuickHPStorage(glyph);
            DecisionSelectNumber = 1;
            UsePower(sigil);
            QuickHPCheck(-2);
            AssertInHand(circle);
            AssertIsInPlay(new Card[] { mds, minetka });
            Assert.IsTrue(mds.IsFaceDownNonCharacter);
            Assert.IsTrue(minetka.IsFaceDownNonCharacter);
        }

        [Test()]
        public void TestSphereOfNalatukhaDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //The first time each turn {Glyph} is dealt damage, you may draw or play a card.
            DecisionYesNo = false;
            Card sphere = PlayCard("SphereOfNalatukha");
            Card circle = PutOnDeck("RitualCircleFate");
            Card occluded = PutOnDeck("OccludedMarionetteDeath");
            Card mds = PutOnDeck("MilektithasDarkSummonsFate");

            DecisionSelectFunction = 1;
            DealDamage(akash, glyph, 0, DamageType.Melee);
            AssertOnTopOfDeck(mds);
            DealDamage(akash, glyph, 1, DamageType.Melee);
            AssertInHand(mds);
            DealDamage(akash, glyph, 1, DamageType.Melee);
            AssertOnTopOfDeck(occluded);

            GoToStartOfTurn(glyph);
            DealDamage(akash, glyph, 1, DamageType.Melee);
            AssertInHand(occluded);
        }

        [Test()]
        public void TestSphereOfNalatukhaPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //The first time each turn {Glyph} is dealt damage, you may draw or play a card.
            DecisionYesNo = false;
            Card sphere = PlayCard("SphereOfNalatukha");
            Card circle = PutInHand("RitualCircleFate");
            DecisionSelectFunction = 0;
            DecisionSelectCardToPlay = circle;
            DealDamage(akash, glyph, 1, DamageType.Melee);
            AssertIsInPlay(circle);
        }

        [Test()]
        public void TestSuntulusCompendium()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Power: Reveal cards from the top of your deck or trash until a ritual is revealed. Put it into your hand. Discard the other revealed cards. If you revealed a card from the top of your deck this way, you may play a card.
            DecisionYesNo = false;
            Card suntulu = PlayCard("SuntulusCompendium");
            Card silence = PutOnDeck("ReturnToSilenceFate");
            Card circle = PutOnDeck("RitualCircleFate");
            Card sigil = PutInHand("SigiledBlade");

            DecisionSelectCardToPlay = sigil;
            UsePower(suntulu);
            AssertInHand(silence);
            AssertIsInPlay(sigil);
            AssertInTrash(circle);
        }

        [Test()]
        public void TestSuntulusCompendiumNoReveal()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Power: Reveal cards from the top of your deck or trash until a ritual is revealed. Put it into your hand. Discard the other revealed cards. If you revealed a card from the top of your deck this way, you may play a card.
            DecisionYesNo = false;
            Card suntulu = PlayCard("SuntulusCompendium");
            Card sigil = PutInHand("SigiledBlade");
            MoveAllCards(glyph, glyph.TurnTaker.Deck, glyph.TurnTaker.Trash);

            DecisionSelectCardToPlay = sigil;
            UsePower(suntulu);
            AssertNotInPlay(sigil);
        }

        [Test()]
        public void TestSuntulusCompendiumNoMatch()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Power: Reveal cards from the top of your deck or trash until a ritual is revealed. Put it into your hand. Discard the other revealed cards. If you revealed a card from the top of your deck this way, you may play a card.
            DecisionYesNo = false;
            Card suntulu = PlayCard("SuntulusCompendium");
            Card sigil = PutInHand("SigiledBlade");
            MoveAllCards(glyph, glyph.TurnTaker.Deck, glyph.TurnTaker.Trash);
            Card circle = PutOnDeck("RitualCircleFate");

            DecisionSelectCardToPlay = sigil;
            UsePower(suntulu);
            AssertIsInPlay(sigil);
            AssertInTrash(circle);
        }

        [Test()]
        public void TestSympatheticCasting()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Power: Select a play area. X on this card = the number of your face-down cards in that area. {Glyph} deals herself and 1 target in that play area X irreducible infernal damage each, then {Glyph} deals that target X fire damage.
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            PlayCard("SigiledBlade");
            PlayCard("MinetkasProclamationDeath");
            PlayCard("MilektithasDarkSummonsFate");

            DecisionSelectTurnTaker = legacy.TurnTaker;
            PlayCard("EmbroideredCloak");

            ResetDecisions();
            DecisionSelectTurnTaker = akash.TurnTaker;

            DecisionYesNo = false;
            Card casting = PlayCard("SympatheticCastingFate");
            QuickHPStorage(akash, glyph);
            UsePower(casting);
            QuickHPCheck(-6, -3);
        }

        [Test()]
        public void TestUnspokenDecreeDeath()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Destroy any number of your face-down cards. For each card destroyed with a matching type, 1 target in that play area:
            //Death: is put on top its deck, if not a character.
            //Destruction: Deals a target 2 melee damage.
            //Fate: Makes its next damage dealt irreducible.
            //Insight: Prevents the next damage it would deal
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card minetka = PlayCard("MinetkasProclamationDeath");

            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card casting = PlayCard("SympatheticCastingDeath");

            ResetDecisions();
            Card rockslide = PlayCard("LivingRockslide");
            PlayCard("UnspokenDecreeInsight");

            AssertOnTopOfDeck(rockslide);
            AssertInTrash(minetka);
            AssertInTrash(casting);
        }

        [Test()]
        public void TestUnspokenDecreeDestruction()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Destroy any number of your face-down cards. For each card destroyed with a matching type, 1 target in that play area:
            //Death: is put on top its deck, if not a character.
            //Destruction: Deals a target 2 melee damage.
            //Fate: Makes its next damage dealt irreducible.
            //Insight: Prevents the next damage it would deal
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card minetka = PlayCard("MinetkasProclamationDestruction");
            DecisionSelectTarget = akash.CharacterCard;
            DecisionYesNo = false;

            QuickHPStorage(akash);
            PlayCard("UnspokenDecreeInsight");
            QuickHPCheck(-2);
            AssertInTrash(minetka);
        }

        [Test()]
        public void TestUnspokenDecreeFate()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Destroy any number of your face-down cards. For each card destroyed with a matching type, 1 target in that play area:
            //Death: is put on top its deck, if not a character.
            //Destruction: Deals a target 2 melee damage.
            //Fate: Makes its next damage dealt irreducible.
            //Insight: Prevents the next damage it would deal
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card inscrutable = PlayCard("InscrutableTutelageFate");
            DecisionSelectTarget = akash.CharacterCard;
            DecisionYesNo = false;

            PlayCard("UnspokenDecreeInsight");
            AssertInTrash(inscrutable);

            PlayCard("Fortitude");
            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 2, DamageType.Melee);
            QuickHPCheck(-2);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 2, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestUnspokenDecreeInsight()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Destroy any number of your face-down cards. For each card destroyed with a matching type, 1 target in that play area:
            //Death: is put on top its deck, if not a character.
            //Destruction: Deals a target 2 melee damage.
            //Fate: Makes its next damage dealt irreducible.
            //Insight: Prevents the next damage it would deal
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card inscrutable = PlayCard("InscrutableTutelageInsight");
            DecisionSelectTarget = akash.CharacterCard;
            DecisionYesNo = false;

            PlayCard("UnspokenDecreeInsight");
            AssertInTrash(inscrutable);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 2, DamageType.Melee);
            QuickHPCheck(0);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 2, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestWalkingTheWebInsight()
        {
            //Destroy any number of your face-down insight cards. For each card destroyed this way, reveal the top 2 cards of a deck in that card's play area and put 1 card on the top and 1 on the bottom of that deck.
            //You may destroy 1 of your face-down might cards to play or discard top card of a deck in that card's play area.
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card inscrutable = PlayCard("InscrutableTutelageInsight");
            Card rockslide = PutOnDeck("LivingRockslide");
            Card entomb = PutOnDeck("Entomb");

            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;

            DecisionYesNo = false;
            DecisionSelectCard = entomb;
            PlayCard("WalkingTheWebDeath");
            AssertOnTopOfDeck(entomb);
            AssertOnBottomOfDeck(rockslide);
            AssertInTrash(inscrutable);
        }

        [Test()]
        public void TestWalkingTheWebMightPlay()
        {
            //Destroy any number of your face-down insight cards. For each card destroyed this way, reveal the top 2 cards of a deck in that card's play area and put 1 card on the top and 1 on the bottom of that deck.
            //You may destroy 1 of your face-down might cards to play or discard top card of a deck in that card's play area.
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card sigil = PlayCard("SigiledBlade");
            Card entomb = PutOnDeck("Entomb");
            DecisionSelectFunction = 0;
            DecisionYesNo = false;
            PlayCard("WalkingTheWebDeath");
            AssertInTrash(sigil);
            AssertIsInPlay(entomb);
        }

        [Test()]
        public void TestWalkingTheWebMightDiscard()
        {
            //Destroy any number of your face-down insight cards. For each card destroyed this way, reveal the top 2 cards of a deck in that card's play area and put 1 card on the top and 1 on the bottom of that deck.
            //You may destroy 1 of your face-down might cards to play or discard top card of a deck in that card's play area.
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card sigil = PlayCard("SigiledBlade");
            Card entomb = PutOnDeck("Entomb");
            DecisionSelectFunction = 1;
            DecisionYesNo = false;
            PlayCard("WalkingTheWebDeath");
            AssertInTrash(sigil);
            AssertInTrash(entomb);
        }

        [Test()]
        public void TestWrittenFutureDeath()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Destroy any number of your face-down cards. For each card destroyed with a matching type, 1 target in that play area:
            //death: is destroyed if it has 3 or fewer hp.
            //destruction: deals itself 2 psychic damage.
            //fate: reduces its next damage dealt by 2.
            //insight: increases its next damage dealt by 2.
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card minetka = PlayCard("MinetkasProclamationDeath");
            Card rockslide = PlayCard("LivingRockslide");
            SetHitPoints(rockslide, 3);
            DecisionYesNo = false;
            PlayCard("WrittenFutureInsight");
            AssertInTrash(rockslide);
        }

        [Test()]
        public void TestWrittenFutureDestruction()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Destroy any number of your face-down cards. For each card destroyed with a matching type, 1 target in that play area:
            //death: is destroyed if it has 3 or fewer hp.
            //destruction: deals itself 2 psychic damage.
            //fate: reduces its next damage dealt by 2.
            //insight: increases its next damage dealt by 2.
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card minetka = PlayCard("MinetkasProclamationDestruction");
            DecisionYesNo = false;

            QuickHPStorage(akash);
            PlayCard("WrittenFutureInsight");
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestWrittenFutureFate()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Destroy any number of your face-down cards. For each card destroyed with a matching type, 1 target in that play area:
            //death: is destroyed if it has 3 or fewer hp.
            //destruction: deals itself 2 psychic damage.
            //fate: reduces its next damage dealt by 2.
            //insight: increases its next damage dealt by 2.
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card inscrutable = PlayCard("InscrutableTutelageFate");
            DecisionYesNo = false;
            PlayCard("WrittenFutureInsight");

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestWrittenFutureInsight()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Destroy any number of your face-down cards. For each card destroyed with a matching type, 1 target in that play area:
            //death: is destroyed if it has 3 or fewer hp.
            //destruction: deals itself 2 psychic damage.
            //fate: reduces its next damage dealt by 2.
            //insight: increases its next damage dealt by 2.
            DecisionYesNo = true;
            DecisionSelectTurnTaker = akash.TurnTaker;
            Card inscrutable = PlayCard("InscrutableTutelageInsight");
            DecisionYesNo = false;
            PlayCard("WrittenFutureInsight");

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(-3);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        //[Test()]
        //public void TestChaosLord()
        //{
        //    SetupGameController("KaargraWarfang", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
        //    StartGame();

        //    Card chaos = PlayCard("TitleChaosLord");
        //    DecisionYesNo = true;
        //    DecisionSelectTurnTaker = glyph.TurnTaker;
        //    PlayCard("OccludedMarionetteInsight");
        //    PlayCard("ABrushWithDeathDeath");
        //    PlayCard("ReturnToSilenceFate");

        //    DecisionYesNo = false;
        //    PlayCard("ABrushWithDeathFate");
        //    PlayCard("SigiledBlade");
        //    PlayCard("EmbroideredCloak");
        //}
    }
}