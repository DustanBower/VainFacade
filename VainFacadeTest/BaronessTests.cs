using NUnit.Framework;
using System;
using VainFacadePlaytest;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using System.Collections.Generic;
using VainFacadePlaytest.TheBaroness;
using System.Runtime.Serialization;
using System.Security.Policy;
using VainFacadePlaytest.Arctis;

namespace VainFacadeTest
{
    [TestFixture()]
    public class BaronessTests:BaseTest
	{
        protected TurnTakerController baroness { get { return FindVillain("TheBaroness"); } }
        protected Card JustBusiness { get { return GetCard("JustBusiness"); } }
        protected Card PoliticsAsUsual { get { return GetCard("PoliticsAsUsual"); } }
        protected Card SecretSocieties { get { return GetCard("SecretSocieties"); } }


        [Test()]
        public void TestVariantSetup()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "Megalopolis");
            StartGame();

            //Put {TheBaroness}'s character card, {JustBusiness}, {PoliticsAsUsual}, and {SecretSocieties} into play in the environment play area, “Caught in the Web” side up. Put the card {Vampirism} into play.
            AssertIsInPlay(baroness.CharacterCard);
            Assert.IsInstanceOf(typeof(TheBaronessSpiderCharacterCardController), baroness.CharacterCardController);
            AssertNotTarget(baroness.CharacterCard);
            AssertIsInPlay("JustBusiness");
            AssertIsInPlay("PoliticsAsUsual");
            AssertIsInPlay("SecretSocieties");
            AssertIsInPlay("Vampirism");
        }

        [Test()]
        public void TestNormalSetup()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness", "Tempest", "Legacy", "Bunker", "Ra", "Megalopolis");
            StartGame();

            //Put {TheBaroness}'s character card, {JustBusiness}, {PoliticsAsUsual}, and {SecretSocieties} into play in the environment play area, “Caught in the Web” side up. Put the card {Vampirism} into play.
            AssertIsInPlay(baroness.CharacterCard);
            Assert.IsNotInstanceOf(typeof(TheBaronessSpiderCharacterCardController), baroness.CharacterCardController);
            AssertIsTarget(baroness.CharacterCard);
            AssertNotInPlay("JustBusiness");
            AssertNotInPlay("PoliticsAsUsual");
            AssertNotInPlay("SecretSocieties");
            AssertIsInPlay("Vampirism");
        }

        [Test()]
        public void TestVariantRemoveWinFront()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "Megalopolis");
            StartGame();

            MoveCard(baroness, baroness.CharacterCard, baroness.TurnTaker.OutOfGame);
            AssertGameOver(EndingResult.VillainDestroyedVictory);
        }

        [Test()]
        public void TestVariantWinBack()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "Megalopolis");
            StartGame();

            SetHitPoints(JustBusiness, 1);
            SetHitPoints(PoliticsAsUsual, 1);
            SetHitPoints(SecretSocieties, 1);

            DrawCard(tempest, 3);

            GoToStartOfTurn(baroness);
            AssertFlipped(baroness);
            DealDamage(legacy, baroness, 200, DamageType.Melee);
            AssertGameOver(EndingResult.VillainDestroyedVictory);
        }

        [Test()]
        public void TestVariantRemoveWinBack()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "Megalopolis");
            StartGame();

            SetHitPoints(JustBusiness, 1);
            SetHitPoints(PoliticsAsUsual, 1);
            SetHitPoints(SecretSocieties, 1);

            DrawCard(tempest, 3);

            GoToStartOfTurn(baroness);
            AssertFlipped(baroness);
            MoveCard(baroness, baroness.CharacterCard, baroness.TurnTaker.OutOfGame);
            AssertGameOver(EndingResult.VillainDestroyedVictory);
        }

        [Test()]
        public void TestNoCardPlays()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "Megalopolis");
            StartGame();

            Card arcane = PutOnDeck("ArcaneVeins");

            PlayCard(arcane);
            AssertOnTopOfDeck(arcane);
        }

        [Test()]
        public void TestNoCardPutIntoPlay()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "Megalopolis");
            StartGame();

            Card arcane = PutOnDeck("ArcaneVeins");

            PlayCard(arcane, true);
            AssertOnTopOfDeck(arcane);
        }

        [Test()]
        public void TestNoCardPutIntoPlayOmnitronX()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "OmnitronX", "Megalopolis");
            StartGame();

            Card arcane = PutOnDeck("ArcaneVeins");

            DecisionSelectTurnTaker = baroness.TurnTaker;
            UsePower(omnix);
            AssertInTrash(arcane);
        }

        [Test()]
        public void TestChallengeFront()
        {
            SetupGameController(new string[] { "VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "Megalopolis" }, challenge: true);
            StartGame();

            //At the end of the villain turn, put the top card of each hero deck face down in the villain play area.
            SetHitPoints(JustBusiness, 1);
            SetHitPoints(PoliticsAsUsual, 1);
            SetHitPoints(SecretSocieties, 1);

            Card chain = PutOnDeck("ChainLightning");
            Card fortitude = PutOnDeck("Fortitude");
            Card flak = PutOnDeck("FlakCannon");
            Card tornado = PutOnDeck("BlazingTornado");

            GoToEndOfTurn(baroness);
            AssertAtLocation(new Card[] { chain, fortitude, flak, tornado }, baroness.TurnTaker.PlayArea);
            AssertNumberOfCardsAtLocation(baroness.TurnTaker.PlayArea, 4, (Card c) => c.IsHero);
        }

        [Test()]
        public void TestChallengeBack()
        {
            SetupGameController(new string[] { "VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "Megalopolis" }, challenge: true);
            StartGame();

            //At the end of the villain turn, put the top card of each hero deck face down in the villain play area.
            DestroyCard(JustBusiness);
            DestroyCard(PoliticsAsUsual);
            DestroyCard(SecretSocieties);

            Card chain = PutOnDeck("ChainLightning");
            Card fortitude = PutOnDeck("Fortitude");
            Card flak = PutOnDeck("FlakCannon");
            Card tornado = PutOnDeck("BlazingTornado");

            PutOnDeck("ArcaneVeins");

            GoToEndOfTurn(baroness);
            AssertAtLocation(new Card[] { chain, fortitude, flak, tornado }, baroness.TurnTaker.PlayArea);
            AssertNumberOfCardsAtLocation(baroness.TurnTaker.PlayArea, 4, (Card c) => c.IsHero);
        }

        [Test()]
        public void TestGlyphFaceDownCards()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "VainFacadePlaytest.Glyph", "Legacy", "Bunker", "OmnitronX", "Megalopolis");
            StartGame();

            FlipCard(baroness);
            SetHitPoints(baroness, 50);

            QuickHPStorage(baroness);
            DecisionYesNo = true;
            DecisionSelectTurnTaker = baroness.TurnTaker;
            Card ritual = PutInHand("RitualCircleDeath");
            PlayCard(ritual);
            QuickHPCheck(1);
        }

        [Test()]
        public void TestVampirismHeal()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "OmnitronX", "Megalopolis");
            StartGame();

            FlipCard(baroness);
            SetHitPoints(baroness, 50);

            QuickHPStorage(baroness);
            DealDamage(baroness, tempest, 1, DamageType.Melee);
            QuickHPCheck(1);
        }

        #region Test Baroness flip conditions
        [Test()]
        public void TestFlipCondition_Hand()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //At the start of the villain turn, remove each villain web from the game and flip {TheBaroness}'s character cards if any of the following conditions are met:
            //{Any player has 7 or more cards in their hand.
            //{Any player has 7 or more cards in their trash.
            //{Any player has 3 or more non-character cards in play.
            //{There are no villain web targets in play.

            SetHitPoints(JustBusiness, 1);
            SetHitPoints(PoliticsAsUsual, 1);
            SetHitPoints(SecretSocieties, 1);

            DrawCard(tempest, 3);

            GoToStartOfTurn(baroness);

            AssertOutOfGame(new Card[] { JustBusiness, PoliticsAsUsual, SecretSocieties });
            AssertFlipped(baroness);
            AssertIsTarget(baroness.CharacterCard);
            AssertHitPoints(baroness.CharacterCard, 91);
        }

        [Test()]
        public void TestFlipCondition_Trash()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //At the start of the villain turn, remove each villain web from the game and flip {TheBaroness}'s character cards if any of the following conditions are met:
            //{Any player has 7 or more cards in their hand.
            //{Any player has 7 or more cards in their trash.
            //{Any player has 3 or more non-character cards in play.
            //{There are no villain web targets in play.

            SetHitPoints(JustBusiness, 1);
            SetHitPoints(PoliticsAsUsual, 1);
            SetHitPoints(SecretSocieties, 1);
            PutOnDeck("ObsidianField");

            DiscardTopCards(tempest, 7);

            GoToStartOfTurn(baroness);

            AssertOutOfGame(new Card[] { JustBusiness, PoliticsAsUsual, SecretSocieties });
            AssertFlipped(baroness);
            AssertIsTarget(baroness.CharacterCard);
            AssertHitPoints(baroness.CharacterCard, 91);
        }

        [Test()]
        public void TestFlipCondition_Play()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //At the start of the villain turn, remove each villain web from the game and flip {TheBaroness}'s character cards if any of the following conditions are met:
            //{Any player has 7 or more cards in their hand.
            //{Any player has 7 or more cards in their trash.
            //{Any player has 3 or more non-character cards in play.
            //{There are no villain web targets in play.

            SetHitPoints(JustBusiness, 1);
            SetHitPoints(PoliticsAsUsual, 1);
            SetHitPoints(SecretSocieties, 1);
            PutOnDeck("ObsidianField");

            PlayCard("Fortitude");
            PlayCard("InspiringPresence");
            PlayCard("TheLegacyRing");

            GoToStartOfTurn(baroness);

            AssertOutOfGame(new Card[] { JustBusiness, PoliticsAsUsual, SecretSocieties });
            AssertFlipped(baroness);
            AssertIsTarget(baroness.CharacterCard);
            AssertHitPoints(baroness.CharacterCard, 91);
        }

        [Test()]
        public void TestFlipCondition_Webs()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //At the start of the villain turn, remove each villain web from the game and flip {TheBaroness}'s character cards if any of the following conditions are met:
            //{Any player has 7 or more cards in their hand.
            //{Any player has 7 or more cards in their trash.
            //{Any player has 3 or more non-character cards in play.
            //{There are no villain web targets in play.

            DestroyCard(JustBusiness);
            DestroyCard(PoliticsAsUsual);
            DestroyCard(SecretSocieties);
            PutOnDeck("ObsidianField");

            DecisionDoNotSelectTurnTaker = true;

            GoToStartOfTurn(baroness);

            AssertOutOfGame(new Card[] { JustBusiness, PoliticsAsUsual, SecretSocieties });
            AssertFlipped(baroness);
            AssertIsTarget(baroness.CharacterCard);
            AssertHitPoints(baroness.CharacterCard, 91);
        }
        #endregion

        #region Test Webs Generic
        [Test()]
        public void TestJustBusiness9HP()
        {
            //* on this card = 3 times {H}
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            AssertIsInPlay(JustBusiness);
            AssertHitPoints(JustBusiness, 9);
        }

        [Test()]
        public void TestJustBusiness12HP()
        {
            //* on this card = 3 times {H}
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter","Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            AssertIsInPlay(JustBusiness);
            AssertHitPoints(JustBusiness, 12);
        }

        [Test()]
        public void TestJustBusiness15HP()
        {
            //* on this card = 3 times {H}
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tachyon", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            AssertIsInPlay(JustBusiness);
            AssertHitPoints(JustBusiness, 15);
        }

        [Test()]
        public void TestPolitics9HP()
        {
            //* on this card = 3 times {H}
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            AssertIsInPlay(PoliticsAsUsual);
            AssertHitPoints(PoliticsAsUsual, 9);
        }

        [Test()]
        public void TestPolitics12HP()
        {
            //* on this card = 3 times {H}
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            AssertIsInPlay(PoliticsAsUsual);
            AssertHitPoints(PoliticsAsUsual, 12);
        }

        [Test()]
        public void TestPolitics15HP()
        {
            //* on this card = 3 times {H}
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tachyon", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            AssertIsInPlay(PoliticsAsUsual);
            AssertHitPoints(PoliticsAsUsual, 15);
        }

        [Test()]
        public void TestSocieties9HP()
        {
            //* on this card = 3 times {H}
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            AssertIsInPlay(SecretSocieties);
            AssertHitPoints(SecretSocieties, 9);
        }

        [Test()]
        public void TestSocieties12HP()
        {
            //* on this card = 3 times {H}
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            AssertIsInPlay(SecretSocieties);
            AssertHitPoints(SecretSocieties, 12);
        }

        [Test()]
        public void TestSocieties15HP()
        {
            //* on this card = 3 times {H}
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tachyon", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            AssertIsInPlay(SecretSocieties);
            AssertHitPoints(SecretSocieties, 15);
        }

        [Test()]
        public void TestJustBusinessFlip()
        {
            //When this card would be destroyed, flip it instead.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            DealDamage(legacy, JustBusiness, 100, DamageType.Melee);
            AssertIsInPlay(JustBusiness);
            AssertFlipped(JustBusiness);
        }

        [Test()]
        public void TestPoliticsFlip()
        {
            //When this card would be destroyed, flip it instead.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            DealDamage(legacy, PoliticsAsUsual, 100, DamageType.Melee);
            AssertIsInPlay(PoliticsAsUsual);
            AssertFlipped(PoliticsAsUsual);
        }

        [Test()]
        public void TestSocietiesFlip()
        {
            //When this card would be destroyed, flip it instead.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            DealDamage(legacy, SecretSocieties, 100, DamageType.Melee);
            AssertIsInPlay(SecretSocieties);
            AssertFlipped(SecretSocieties);
        }
        #endregion

        #region Just Business
        [Test()]
        public void TestJustBusinessFront()
        {
            //At the end of the environment turn, for every 3 HP this card possesses, a player discards a card.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            SetHitPoints(PoliticsAsUsual, 1);
            SetHitPoints(SecretSocieties, 1);
            PutOnDeck("ObsidianField");

            DecisionSelectTurnTaker = tempest.TurnTaker;
            QuickHandStorage(tempest);
            GoToEndOfTurn(FindEnvironment());
            AssertNumberOfCardsInTrash(tempest, 4);
            QuickHandCheck(-4);
        }
        
        [Test()]
        public void TestJustBusinessBack()
        {
            //At the end of the environment turn, each player may play up to 2 cards.
            //If a card entered play this way, destroy this card.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            SetHitPoints(PoliticsAsUsual, 1);
            SetHitPoints(SecretSocieties, 1);
            PutOnDeck("ObsidianField");

            DestroyCard(JustBusiness);

            MoveAllCardsFromHandToDeck(tempest);
            MoveAllCardsFromHandToDeck(legacy);
            MoveAllCardsFromHandToDeck(bunker);
            MoveAllCardsFromHandToDeck(ra);

            PutInHand("CleansingDownpour");
            PutInHand("OtherworldlyResilience");
            PutInHand("Fortitude");
            PutInHand("NextEvolution");
            PutInHand("FlakCannon");
            PutInHand("GrenadeLauncher");
            PutInHand("BlazingTornado");
            PutInHand("FleshOfTheSunGod");

            QuickHandStorage(tempest, legacy, bunker, ra);
            GoToEndOfTurn(FindEnvironment());
            QuickHandCheck(-2, -2, -2, -2);
            AssertIsInPlay(new string[] { "CleansingDownpour", "OtherworldlyResilience", "Fortitude", "NextEvolution", "FlakCannon", "GrenadeLauncher", "BlazingTornado", "FleshOfTheSunGod" });
            AssertOutOfGame(JustBusiness);
        }

        [Test()]
        public void TestJustBusinessBackNoPlay()
        {
            //At the end of the environment turn, each player may play up to 2 cards.
            //If a card entered play this way, destroy this card.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            SetHitPoints(PoliticsAsUsual, 1);
            SetHitPoints(SecretSocieties, 1);

            DestroyCard(JustBusiness);

            MoveAllCardsFromHandToDeck(tempest);
            MoveAllCardsFromHandToDeck(legacy);
            MoveAllCardsFromHandToDeck(bunker);
            MoveAllCardsFromHandToDeck(ra);

            PutInHand("CleansingDownpour");
            PutInHand("OtherworldlyResilience");
            PutInHand("Fortitude");
            PutInHand("NextEvolution");
            PutInHand("FlakCannon");
            PutInHand("GrenadeLauncher");
            PutInHand("BlazingTornado");
            PutInHand("FleshOfTheSunGod");

            QuickHandStorage(tempest, legacy, bunker, ra);
            DecisionDoNotSelectTurnTaker = true;
            GoToEndOfTurn(FindEnvironment());
            QuickHandCheckZero();
            AssertInHand(new string[] { "CleansingDownpour", "OtherworldlyResilience", "Fortitude", "NextEvolution", "FlakCannon", "GrenadeLauncher", "BlazingTornado", "FleshOfTheSunGod" });
            AssertIsInPlay(JustBusiness);
        }

        [Test()]
        public void TestJustBusinessBackDestroy()
        {
            //At the end of the environment turn, each player may play up to 2 cards.
            //If a card entered play this way, destroy this card.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            SetHitPoints(PoliticsAsUsual, 1);
            SetHitPoints(SecretSocieties, 1);

            DestroyCard(JustBusiness);

            AssertIsInPlay(JustBusiness);
            AssertFlipped(JustBusiness);

            DestroyCard(JustBusiness);

            AssertOutOfGame(JustBusiness);
        }
        #endregion

        #region Politics as Usual
        [Test()]
        public void TestPoliticsFront()
        {
            //At the end of the environment turn, for every 3 HP this card possesses, play the top card of the environment deck
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            SetHitPoints(JustBusiness, 1);
            SetHitPoints(SecretSocieties, 1);
            Card obs = PutOnDeck("ObsidianField");
            Card lava = PutOnDeck("RiverOfLava");
            Card trex = PutOnDeck("EnragedTRex");
            Card thief = PutOnDeck("PterodactylThief");
            Card pack = PutOnDeck("VelociraptorPack");

            GoToEndOfTurn(FindEnvironment());
            AssertIsInPlay(new Card[] {lava, trex, thief, pack });
            AssertOnTopOfDeck(obs);
        }

        [Test()]
        public void TestPoliticsBack()
        {
            //At the end of the environment turn, At the end of the environment turn, each player may discard a card to draw 2 cards.
            //If a card is discarded this way, destroy this card.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            SetHitPoints(JustBusiness, 1);
            SetHitPoints(SecretSocieties, 1);

            DestroyCard(PoliticsAsUsual);

            QuickHandStorage(tempest, legacy, bunker, ra);
            GoToEndOfTurn(FindEnvironment());
            QuickHandCheck(1, 1, 1, 1);
            AssertNumberOfCardsInTrash(tempest, 1);
            AssertNumberOfCardsInTrash(legacy, 1);
            AssertNumberOfCardsInTrash(bunker, 1);
            AssertNumberOfCardsInTrash(ra, 1);
            AssertOutOfGame(PoliticsAsUsual);
        }

        [Test()]
        public void TestJustBusinessBackNoDiscard()
        {
            //At the end of the environment turn, At the end of the environment turn, each player may discard a card to draw 2 cards.
            //If a card is discarded this way, destroy this card.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            SetHitPoints(PoliticsAsUsual, 1);
            SetHitPoints(SecretSocieties, 1);

            DestroyCard(JustBusiness);

            DecisionDoNotSelectTurnTaker = true;

            QuickHandStorage(tempest, legacy, bunker, ra);
            GoToEndOfTurn(FindEnvironment());
            QuickHandCheckZero();
            AssertNumberOfCardsInTrash(tempest, 0);
            AssertNumberOfCardsInTrash(legacy, 0);
            AssertNumberOfCardsInTrash(bunker, 0);
            AssertNumberOfCardsInTrash(ra, 0);
            AssertIsInPlay(PoliticsAsUsual);
        }

        [Test()]
        public void TestPoliticsBackDestroy()
        {
            //At the end of the environment turn, each player may play up to 2 cards.
            //If a card entered play this way, destroy this card.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            SetHitPoints(JustBusiness, 1);
            SetHitPoints(SecretSocieties, 1);

            DestroyCard(PoliticsAsUsual);

            AssertIsInPlay(PoliticsAsUsual);
            AssertFlipped(PoliticsAsUsual);

            DestroyCard(PoliticsAsUsual);

            AssertOutOfGame(PoliticsAsUsual);
        }
        #endregion

        #region Secret Societies
        [Test()]
        public void TestSocietiesFront()
        {
            //At the end of the environment turn, for every 3 HP this card possesses, increase the next damage dealt by {TheBaroness} by 1.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            SetHitPoints(JustBusiness, 1);
            SetHitPoints(PoliticsAsUsual, 1);

            QuickHPStorage(legacy);
            DealDamage(baroness, legacy, 1, DamageType.Radiant);
            QuickHPCheck(-1);

            GoToEndOfTurn(FindEnvironment());

            QuickHPStorage(legacy);
            DealDamage(baroness, legacy, 1, DamageType.Radiant);
            QuickHPCheck(-5);

            QuickHPStorage(legacy);
            DealDamage(baroness, legacy, 1, DamageType.Radiant);
            QuickHPCheck(-1);

        }

        [Test()]
        public void TestSocietiesBack()
        {
            //At the end of the environment turn, each player may play a card and use a power.
            //If a card enters play or a power is used this way, destroy this card.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            SetHitPoints(JustBusiness, 1);
            SetHitPoints(PoliticsAsUsual, 1);
            Card pack = PlayCard("VelociraptorPack");

            DestroyCard(SecretSocieties);

            DecisionSelectTurnTakers = new TurnTaker[] { ra.TurnTaker, null };
            Card flesh = PutInHand("FleshOfTheSunGod");
            DecisionSelectCard = flesh;
            DecisionSelectTarget = pack;

            QuickHPStorage(pack);
            GoToEndOfTurn(FindEnvironment());
            QuickHPCheck(-2);
            AssertIsInPlay(flesh);
            AssertOutOfGame(SecretSocieties);

        }

        [Test()]
        public void TestSocietiesBackNoPlay()
        {
            //At the end of the environment turn, each player may play a card and use a power.
            //If a card enters play or a power is used this way, destroy this card.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            SetHitPoints(JustBusiness, 1);
            SetHitPoints(PoliticsAsUsual, 1);
            Card pack = PlayCard("VelociraptorPack");

            DestroyCard(SecretSocieties);

            DecisionSelectTurnTakers = new TurnTaker[] { ra.TurnTaker, null };
            DecisionDoNotSelectCard = SelectionType.PlayCard;
            DecisionSelectTarget = pack;

            QuickHPStorage(pack);
            QuickHandStorage(ra);
            GoToEndOfTurn(FindEnvironment());
            QuickHPCheck(-2);
            QuickHandCheckZero();
            AssertOutOfGame(SecretSocieties);

        }

        [Test()]
        public void TestSocietiesBackNoPower()
        {
            //At the end of the environment turn, each player may play a card and use a power.
            //If a card enters play or a power is used this way, destroy this card.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "Megalopolis");
            StartGame();

            SetHitPoints(JustBusiness, 1);
            SetHitPoints(PoliticsAsUsual, 1);
            Card train = PlayCard("PlummetingMonorail");
            GoToPlayCardPhase(FindEnvironment());
            PlayCard("PaparazziOnTheScene");


            Card flesh = PutInHand("FleshOfTheSunGod");

            DestroyCard(SecretSocieties);

            DecisionSelectTurnTakers = new TurnTaker[] { ra.TurnTaker, null };
            DecisionSelectCardToPlay = flesh;
            DecisionSelectTarget = train;

            QuickHPStorage(train);
            GoToEndOfTurn(FindEnvironment());
            QuickHPCheckZero();
            AssertIsInPlay(flesh);
            AssertOutOfGame(SecretSocieties);

        }

        [Test()]
        public void TestSocietiesBackNoPlayOrPower()
        {
            //At the end of the environment turn, At the end of the environment turn, each player may discard a card to draw 2 cards.
            //If a card is discarded this way, destroy this card.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            SetHitPoints(JustBusiness, 1);
            SetHitPoints(PoliticsAsUsual, 1);
            PutOnDeck("VelociraptorPack");

            DestroyCard(SecretSocieties);

            DecisionDoNotSelectTurnTaker = true;

            QuickHandStorage(tempest,legacy,bunker,ra);
            GoToEndOfTurn(FindEnvironment());
            QuickHandCheckZero();
            AssertIsInPlay(SecretSocieties);
        }

        [Test()]
        public void TestSocietiesBackDestroy()
        {
            //At the end of the environment turn, each player may play up to 2 cards.
            //If a card entered play this way, destroy this card.
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            SetHitPoints(JustBusiness, 1);
            SetHitPoints(PoliticsAsUsual, 1);
            PutOnDeck("ObsidianField");

            DestroyCard(SecretSocieties);

            AssertIsInPlay(SecretSocieties);
            AssertFlipped(SecretSocieties);

            DestroyCard(SecretSocieties);

            AssertOutOfGame(SecretSocieties);
        }
        #endregion

        #region Flip Side
        [Test()]
        public void TestRadiantIrreducible()
        {
            SetupGameController( "VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "TheBlock" );
            StartGame();

            //Radiant damage dealt to villain targets is irreducible
            FlipCard(baroness);
            PlayCard("DefensiveDisplacement");

            QuickHPStorage(baroness);
            DealDamage(legacy, baroness, 3, DamageType.Radiant);
            QuickHPCheck(-3);

            QuickHPStorage(legacy);
            DealDamage(baroness, legacy, 3, DamageType.Radiant);
            QuickHPCheck(-1);
        }

        [Test()]
        public void Test5DamageStealCard()
        {
            SetupGameController( "VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "TheBlock" );
            StartGame();

            //The first time each turn The Baroness is dealt 5 or more damage at once, put a non-character hero card in play face down in the villain play area.
            FlipCard(baroness);
            Card fortitude = PlayCard("Fortitude");
            SetHitPoints(baroness, 50);

            QuickHPStorage(baroness);
            DealDamage(legacy, baroness, 5, DamageType.Melee);
            AssertFlipped(fortitude);
            AssertInPlayArea(baroness, fortitude);

            //Check that Baroness healed 1 HP after being dealt 5
            QuickHPCheck(-4);

            Card evo = PlayCard("NextEvolution");
            
            DealDamage(legacy, baroness, 5, DamageType.Melee);
            AssertNotFlipped(evo);
            AssertInPlayArea(legacy, evo);
        }

        [Test()]
        public void TestSchemeEntersPlay()
        {
            SetupGameController( "VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "TheBlock" );
            StartGame();

            //The first time each turn a scheme enters or leaves play, play the top card of the villain deck.

            FlipCard(baroness);
            Card eternal = PutOnDeck("EternalSilence");
            Card fang = PutOnDeck("FangAndClaw");
            PlayCard("ArcaneVeins");
            AssertInTrash(fang);
            PlayCard("VampiricStrength");
            AssertOnTopOfDeck(eternal);
        }

        [Test()]
        public void TestSchemeLeavesPlay()
        {
            SetupGameController( "VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "TheBlock");
            StartGame();

            //The first time each turn a scheme enters or leaves play, play the top card of the villain deck.
            GoToStartOfTurn(tempest);
            FlipCard(baroness);
            Card eternal = PutOnDeck("EternalSilence");
            Card fang = PutOnDeck("FangAndClaw");
            Card arcane = PlayCard("ArcaneVeins");
            Card strength = PlayCard("VampiricStrength");
            PutOnDeck(baroness,eternal);
            PutOnDeck(baroness,fang);

            GoToStartOfTurn(legacy);
            DestroyCard(arcane);
            AssertInTrash(fang);
            DestroyCard(strength);
            AssertOnTopOfDeck(eternal);
        }

        [Test()]
        public void TestSchemeEntersThenLeavesPlay()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "TheBlock");
            StartGame();

            //The first time each turn a scheme enters or leaves play, play the top card of the villain deck.

            FlipCard(baroness);
            Card eternal = PutOnDeck("EternalSilence");
            Card fang = PutOnDeck("FangAndClaw");
            Card arcane = PlayCard("ArcaneVeins");
            AssertInTrash(fang);
            DestroyCard(arcane);
            AssertOnTopOfDeck(eternal);
        }

        [Test()]
        public void TestCardDrawDamageIncrease()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "TheBlock" );
            StartGame();

            //The first time each turn each player draws a card, increase the next damage dealt by {TheBaroness} by 1.

            GoToStartOfTurn(tempest);
            FlipCard(baroness);

            //Check that next damage is increased
            DrawCard(tempest);
            QuickHPStorage(tempest);
            DealDamage(baroness, tempest, 1, DamageType.Radiant);
            QuickHPCheck(-2);

            QuickHPStorage(tempest);
            DealDamage(baroness, tempest, 1, DamageType.Radiant);
            QuickHPCheck(-1);

            //Check that damage is not increased on the next draw
            DrawCard(tempest);
            QuickHPStorage(tempest);
            DealDamage(baroness, tempest, 1, DamageType.Radiant);
            QuickHPCheck(-1);

            //Check that damage is increased if a different hero draws
            DrawCard(legacy);
            QuickHPStorage(tempest);
            DealDamage(baroness, tempest, 1, DamageType.Radiant);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestEndOfTurn()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness/TheBaronessSpiderCharacter", "Tempest", "Legacy", "Bunker", "Ra", "TheBlock");
            StartGame();

            //At the end of the villain turn, play the top card of the villain deck. Then, {TheBaroness} deals the hero target with the highest HP {H} melee and {H} infernal damage.
            FlipCard(baroness);
            Card thirst = PutOnDeck("EndlessThirst");
            Card undying = PutOnDeck("UndyingEssence");

            QuickHPStorage(tempest,legacy, bunker, ra);
            
            GoToEndOfTurn(baroness);

            AssertIsInPlay(undying);
            AssertIsInPlay(thirst);
            QuickHPCheck(0, -8, 0, 0);
        }
        #endregion
    }
}

