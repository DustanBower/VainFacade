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
    public class DoomsayerTests : BaseTest
    {
        protected TurnTakerController doomsayer { get { return FindVillain("Doomsayer"); } }
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


        [Test()]
        public void TestLoadDoomsayer()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(doomsayer);
            Assert.IsInstanceOf(typeof(DoomsayerCharacterCardController), doomsayer.CharacterCardController);

            Assert.AreEqual(13, doomsayer.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestSetup()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //"Put {CountlessWords} into play.
            //Shuffle {AcolytesOfTheBlackThorn}, {Bladewight}, and {Ingini} into the villain deck.
            //Reveal cards from the top of the villain deck until a target and a proclamation are revealed.
            //Put the first revealed card of each type into play. Shuffle the rest into the villain deck."
            AssertIsInPlay(countless);
            Assert.IsTrue(bladewight.IsInPlayAndHasGameText || acolytes.IsInPlayAndHasGameText || ingini.IsInPlayAndHasGameText);
            if (bladewight.IsInPlayAndHasGameText)
            {
                AssertAtLocation(new Card[] { acolytes, ingini },doomsayer.TurnTaker.Deck);
            }
            if (acolytes.IsInPlayAndHasGameText)
            {
                AssertAtLocation(new Card[] { bladewight, ingini }, doomsayer.TurnTaker.Deck);
            }
            if (ingini.IsInPlayAndHasGameText)
            {
                AssertAtLocation(new Card[] { acolytes, bladewight }, doomsayer.TurnTaker.Deck);
            }

            Card TheyOweYou = GetCard("TheyOweYou");
            Card alone = GetCard("YouAreAlone");
            Card broken = GetCard("YouAreBroken");
            Card running = GetCard("YouAreRunningOutOfTime");
            Card change = GetCard("YouCantChange");
            Card choice = GetCard("YouHaveNoChoice");
            Assert.IsTrue(TheyOweYou.IsInPlayAndHasGameText || alone.IsInPlayAndHasGameText || broken.IsInPlayAndHasGameText || running.IsInPlayAndHasGameText || change.IsInPlayAndHasGameText || choice.IsInPlayAndHasGameText);
        }

        [Test()]
        public void TestRemoveCharacterFromGame()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When a villain character is destroyed, remove it from the game.
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);

            PlayCard(ingini);
            SetHitPoints(ingini, 1);
            DealDamage(legacy, ingini, 2, DamageType.Melee);
            AssertOutOfGame(ingini);
        }

        [Test()]
        public void TestFlipRemoveCharacterFromGame()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When a villain character is destroyed, remove it from the game.
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);

            FlipCard(doomsayer.CharacterCard);

            PlayCard(ingini);
            SetHitPoints(ingini, 1);
            DealDamage(legacy, ingini, 2, DamageType.Melee);
            AssertOutOfGame(ingini);
        }

        [Test()]
        public void TestDoomsayerDestroyedWin()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When {Doomsayer} is destroyed, the heroes win.
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            MoveCards(doomsayer, FindCardsWhere((Card c) => c.DoKeywordsContain("proclamation")), doomsayer.TurnTaker.Trash);

            SetHitPoints(doomsayer, 1);
            DealDamage(legacy, doomsayer, 5, DamageType.Melee);
            AssertGameOver(EndingResult.VillainDestroyedVictory);
        }

        [Test()]
        public void TestDoomsayerFlipDestroyedWin()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When {Doomsayer} is destroyed, the heroes win.
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            MoveCards(doomsayer, FindCardsWhere((Card c) => c.DoKeywordsContain("proclamation")), doomsayer.TurnTaker.Trash);

            FlipCard(doomsayer.CharacterCard);

            SetHitPoints(doomsayer, 1);
            DealDamage(legacy, doomsayer, 5, DamageType.Melee);
            AssertGameOver(EndingResult.VillainDestroyedVictory);
        }

        [Test()]
        public void TestDoomsayerProclamationDestroyedWin()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When {Doomsayer} is destroyed, the heroes win.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);

            SetHitPoints(doomsayer, 1);
            Card owe = PlayCard("TheyOweYou");
            DealDamage(legacy, doomsayer, 5, DamageType.Melee);
            DestroyCard(owe);
            AssertGameOver(EndingResult.VillainDestroyedVictory);
        }

        [Test()]
        public void TestDoomsayerRemovedFromGameWin()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //When {Doomsayer} is destroyed, the heroes win.
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            MoveCards(doomsayer, FindCardsWhere((Card c) => c.DoKeywordsContain("proclamation")), doomsayer.TurnTaker.Trash);

            SetHitPoints(doomsayer, 1);
            Card worm = PlayCard("MongolianDeathWorm");
            PlayCard("UnforgivingWasteland");
            DecisionYesNo = true;
            DealDamage(worm, doomsayer, 5, DamageType.Melee);
            AssertGameOver(EndingResult.VillainDestroyedVictory);
        }

        [Test()]
        public void TestDoomsayerFlipRemovedFromGameWin()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //When {Doomsayer} is destroyed, the heroes win.
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            MoveCards(doomsayer, FindCardsWhere((Card c) => c.DoKeywordsContain("proclamation")), doomsayer.TurnTaker.Trash);

            FlipCard(doomsayer.CharacterCard);

            SetHitPoints(doomsayer, 1);
            Card worm = PlayCard("MongolianDeathWorm");
            PlayCard("UnforgivingWasteland");
            DecisionYesNo = true;
            DealDamage(worm, doomsayer, 5, DamageType.Melee);
            AssertGameOver(EndingResult.VillainDestroyedVictory);
        }

        [Test()]
        public void TestDoomsayerDamageReduction()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //Reduce damage dealt to {Doomsayer} by 1 for each other villain target in play.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);

            PlayCard(ingini);
            PlayCard(bladewight);
            PlayCard(acolytes);

            QuickHPStorage(doomsayer);
            DealDamage(legacy, doomsayer, 5, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestDoomsayerFlipDamageReduction()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //Reduce damage dealt to {Doomsayer} by 1 for each other villain target in play.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);

            PlayCard(ingini);
            PlayCard(bladewight);
            PlayCard(acolytes);

            FlipCard(doomsayer.CharacterCard);

            QuickHPStorage(doomsayer);
            DealDamage(legacy, doomsayer, 5, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestDoomsayerHeal()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //At the start of the villain turn, Doomsayer regains 13 HP
            GoToEndOfTurn(doomsayer);
            ClearInitialCards();
            SetHitPoints(doomsayer.CharacterCard, -30);
            QuickHPStorage(doomsayer);

            GoToStartOfTurn(doomsayer);
            QuickHPCheck(13);
        }

        [Test()]
        public void TestDoomsayerFlipHeal()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //At the start of the villain turn, Doomsayer regains 13 HP
            GoToEndOfTurn(doomsayer);

            ClearInitialCards();
            SetHitPoints(doomsayer.CharacterCard, -30);
            QuickHPStorage(doomsayer);
            GoToEndOfTurn(FindEnvironment());
            FlipCard(doomsayer.CharacterCard);
            GoToStartOfTurn(doomsayer);
            QuickHPCheck(13);
        }

        [Test()]
        public void TestDoomsayerEndOfTurn()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //At the end of the villain turn, shuffle all patterns from the villain trash into the villain deck,
            //then reveal cards from the top of the villain deck until a pattern is revealed. Put it into play.
            //Shuffle the rest into the villain deck. Then flip this card.

            //At the end of the villain turn, {Doomsayer} deals each hero target 1 infernal damage. If no damage is dealt this way, play the top card of the environment deck.

            ClearInitialCards();
            StackDeckAfterShuffle(doomsayer,new string[] {"SilverTonguedDevil","FutileEfforts" });
            Card nothing = PutInTrash("NothingToSeeHere");
            Card silver = GetCard("SilverTonguedDevil");
            Card futile = GetCard("FutileEfforts");
            Card worm = PlayCard("MongolianDeathWorm");

            DecisionDoNotSelectCard = SelectionType.DiscardCard;
            QuickHPStorage(ra.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, scholar.CharacterCard, worm);
            QuickShuffleStorage(doomsayer);
            GoToEndOfTurn(doomsayer);
            AssertIsInPlay(futile);
            AssertInDeck(nothing);
            AssertInDeck(silver);
            QuickShuffleCheck(2);
            QuickHPCheck(-1, -1, -1, -1, 0);
            AssertFlipped(doomsayer.CharacterCard);
        }

        [Test()]
        public void TestDoomsayerFlippedEndOfTurnNoDamage()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //At the end of the villain turn, {Doomsayer} deals each hero target 1 infernal damage. If no damage is dealt this way, play the top card of the environment deck.
            ClearInitialCards();
            FlipCard(doomsayer.CharacterCard);
            PlayCard("Fortitude");
            PlayCard("HeroicInterception");
            Card worm = PutOnDeck("MongolianDeathWorm");

            GoToEndOfTurn(doomsayer);
            AssertIsInPlay(worm);
        }

        [Test()]
        public void TestDoomsayerBackFlip()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //When a villain pattern is moved under {CountlessWords}, flip this card.
            FlipCard(doomsayer.CharacterCard);
            ClearInitialCards();

            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            AssertNotFlipped(doomsayer.CharacterCard);
        }

        [Test()]
        public void TestDoomsayerAdvancedFront()
        {
            SetupGameController(new string [] {"VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland"}, advanced: true);
            StartGame();

            //Increase damage dealt to hero targets by 1.
            ClearInitialCards();
            QuickHPStorage(ra);
            DealDamage(legacy, ra, 1, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestDoomsayerAdvancedBack()
        {
            SetupGameController(new string[] { "VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland" }, advanced: true);
            StartGame();

            //At the start of the villain turn, play the top card of the villain deck.
            GoToEndOfTurn(FindEnvironment());
            ClearInitialCards();
            PutOnDeck("FutileEfforts");
            if (!doomsayer.CharacterCard.IsFlipped)
            {
                FlipCard(doomsayer.CharacterCard);
            }

            GoToStartOfTurn(doomsayer);
            AssertIsInPlay("FutileEfforts");
        }

        [Test()]
        public void TestDoomsayerChallenge()
        {
            SetupGameController(new string[] { "VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland" }, challenge: true);
            StartGame();

            //When damage would be dealt to a hero target, increase that damage by 1 for each proclamation in that target's play area.
            ClearInitialCards();
            DecisionSelectTurnTaker = ra.TurnTaker;
            
            QuickHPStorage(ra);
            DealDamage(legacy, ra, 1, DamageType.Melee);
            QuickHPCheck(-1);

            Card choice = PlayCard("YouHaveNoChoice");
            QuickHPStorage(ra);
            DealDamage(legacy, ra, 1, DamageType.Melee);
            QuickHPCheck(-2);

            MoveAllCards(ra, ra.HeroTurnTaker.Hand, ra.TurnTaker.Trash);
            PlayCard("YouAreBroken");
            QuickHPStorage(ra);
            DealDamage(legacy, ra, 1, DamageType.Melee);
            QuickHPCheck(-3);

            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            DestroyCard(choice);
            QuickHPStorage(ra);
            DealDamage(legacy, ra, 1, DamageType.Melee);
            QuickHPCheck(-2);

            DecisionSelectTurnTaker = legacy.TurnTaker;
            PlayCard("YouCantChange");
            QuickHPStorage(ra);
            DealDamage(legacy, ra, 1, DamageType.Melee);
            QuickHPCheck(-2);

            FlipCard(doomsayer);
            QuickHPStorage(ra);
            DealDamage(legacy, ra, 1, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestCountlessWordsNoCards()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //When there are no cards under this card, villain cards are indestructible and {Doomsayer} is immune to damage.
            //When there are 2 or fewer cards under this card, {Doomsayer} and proclamations are indestructible.
            ClearInitialCards();
            PlayCard(ingini);
            Card silver = PlayCard("SilverTonguedDevil");
            DestroyCard(silver);
            SetHitPoints(ingini,1);
            DealDamage(legacy, ingini, 5, DamageType.Melee);
            AssertIsInPlay(new Card[] { ingini, silver });

            QuickHPStorage(doomsayer);
            DealDamage(legacy, doomsayer, 5, DamageType.Melee);
            QuickHPCheck(0);

            //Check that if a card is moved under Countless Words while a villain target has less than 0 HP, it gets destroyed
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            AssertOutOfGame(ingini);
        }

        [Test()]
        public void TestCountlessWords1Card()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //When there are no cards under this card, villain cards are indestructible and {Doomsayer} is immune to damage.
            //When there are 2 or fewer cards under this card, {Doomsayer} and proclamations are indestructible.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            PlayCard(ingini);
            Card silver = PlayCard("SilverTonguedDevil");

            //Check that regular ongoings and targets are no longer indestructible
            DestroyCard(silver);
            SetHitPoints(ingini, 1);
            DealDamage(legacy, ingini, 5, DamageType.Melee);
            AssertInTrash(silver);
            AssertOutOfGame(ingini);

            //Check that Doomsayer can be damaged but is indestructible
            QuickHPStorage(doomsayer);
            DealDamage(legacy, doomsayer, 20, DamageType.Melee);
            QuickHPCheck(-20);
            AssertIsInPlay(doomsayer.CharacterCard);

            //Check that proclamations are indestructible
            Card broken = PlayCard("YouAreBroken");
            DestroyCard(broken);
            AssertIsInPlay(broken);
        }

        [Test()]
        public void TestCountlessWords2Cards()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //When there are no cards under this card, villain cards are indestructible and {Doomsayer} is immune to damage.
            //When there are 2 or fewer cards under this card, {Doomsayer} and proclamations are indestructible.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            PlayCard(ingini);
            Card silver = PlayCard("SilverTonguedDevil");

            //Check that regular ongoings and targets are no longer indestructible
            DestroyCard(silver);
            SetHitPoints(ingini, 1);
            DealDamage(legacy, ingini, 5, DamageType.Melee);
            AssertInTrash(silver);
            AssertOutOfGame(ingini);

            //Check that Doomsayer can be damaged but is indestructible
            QuickHPStorage(doomsayer);
            DealDamage(legacy, doomsayer, 20, DamageType.Melee);
            QuickHPCheck(-20);
            AssertIsInPlay(doomsayer.CharacterCard);
            AssertNotGameOver();

            //Check that proclamations are indestructible
            Card broken = PlayCard("YouAreBroken");
            DestroyCard(broken);
            AssertIsInPlay(broken);

        }

        [Test()]
        public void TestCountlessWordsMoveCardUnderWin()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //When there are no cards under this card, villain cards are indestructible and {Doomsayer} is immune to damage.
            //When there are 2 or fewer cards under this card, {Doomsayer} and proclamations are indestructible.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            PlayCard(ingini);
            Card silver = PlayCard("SilverTonguedDevil");

            //Check that regular ongoings and targets are no longer indestructible
            DestroyCard(silver);
            SetHitPoints(ingini, 1);
            DealDamage(legacy, ingini, 5, DamageType.Melee);
            AssertInTrash(silver);
            AssertOutOfGame(ingini);

            //Check that Doomsayer can be damaged but is indestructible
            QuickHPStorage(doomsayer);
            DealDamage(legacy, doomsayer, 20, DamageType.Melee);
            QuickHPCheck(-20);
            AssertIsInPlay(doomsayer.CharacterCard);
            AssertNotGameOver();

            //Check that if a third card is moved under Countless Words while Doomsayer has less than 0 HP, the heroes win
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            AssertGameOver(EndingResult.VillainDestroyedVictory);
        }

        [Test()]
        public void TestCountlessWords3Cards()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //When there are no cards under this card, villain cards are indestructible and {Doomsayer} is immune to damage.
            //When there are 2 or fewer cards under this card, {Doomsayer} and proclamations are indestructible.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            PlayCard(ingini);
            Card silver = PlayCard("SilverTonguedDevil");

            //Check that regular ongoings and targets are no longer indestructible
            DestroyCard(silver);
            SetHitPoints(ingini, 1);
            DealDamage(legacy, ingini, 5, DamageType.Melee);
            AssertInTrash(silver);
            AssertOutOfGame(ingini);

            //Check that proclamations are no longer indestructible
            Card broken = PlayCard("YouAreBroken");
            DestroyCard(broken);
            AssertInTrash(broken);

            //Check that Doomsayer can be damaged and is no longer indestructible
            QuickHPStorage(doomsayer);
            DealDamage(legacy, doomsayer, 20, DamageType.Melee);
            QuickHPCheck(-20);
            AssertGameOver(EndingResult.VillainDestroyedVictory);
        }

        [Test()]
        public void TestCountlessWordsFlip()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //When there are 4 cards under this card, flip this card.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            AssertFlipped(countless);
        }

        [Test()]
        public void TestTheSilentBalanceReduction()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Luminary", "TheScholar", "TheFinalWasteland");
            StartGame();

            //Reduce damage dealt to hero targets by 1.
            ClearInitialCards();
            FlipCard(countless);
            Card turret = PlayCard("RegressionTurret");
            QuickHPStorage(ra.CharacterCard, legacy.CharacterCard, luminary.CharacterCard, scholar.CharacterCard, turret);
            DealDamage(doomsayer, (Card c) => c.IsHero, 2, DamageType.Melee);
            QuickHPCheck(-1, -1, -1, -1, -1);
        }

        [Test()]
        public void TestTheSilentBalancePower1()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //Each hero gains the following power:
            //Power: Destroy an ongoing or increase the next damage dealt to {Doomsayer} by 3.
            ClearInitialCards();
            FlipCard(countless);
            Card broken = PlayCard("YouAreBroken");
            Card fortitude = PlayCard("Fortitude");
            Card flak = PlayCard("FlakCannon");
            Card red = PlayCard("SeeingRed");
            DecisionSelectCard = broken;
            DecisionSelectFunction = 0;
            AssertNextDecisionChoices(new Card[] { broken, fortitude }, new Card[] { flak, red, countless });
            UsePower(bunker, 1);
            AssertInTrash(broken);
        }

        [Test()]
        public void TestTheSilentBalancePower2()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //Each hero gains the following power:
            //Power: Destroy an ongoing or increase the next damage dealt to {Doomsayer} by 3.
            ClearInitialCards();
            FlipCard(countless);
            DecisionSelectFunction = 1;
            UsePower(bunker, 1);

            QuickHPStorage(doomsayer);
            DealDamage(legacy, doomsayer, 1, DamageType.Melee);
            QuickHPCheck(-4);

            QuickHPStorage(doomsayer);
            DealDamage(legacy, doomsayer, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestAcolytesDamage()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //The first time each turn this card is dealt damage, reveal and replace the top card of the villain deck, then 1 player may play a card.
            //At the end of the villain turn, if there is an ongoing on top the villain trash, this card deals the hero target with the second highest HP {H} infernal damage. Otherwise this card deals the {H - 1} hero targets with the lowest HP 1 melee and 1 infernal damage each.
            ClearInitialCards();
            PlayCard(acolytes);

            Card fortitude = PutInHand("Fortitude");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionSelectCardToPlay = fortitude;

            DealDamage(legacy, acolytes, 1, DamageType.Melee);
            AssertIsInPlay(fortitude);

            Card danger = PutInHand("DangerSense");
            DecisionSelectCardToPlay = danger;

            DealDamage(legacy, acolytes, 1, DamageType.Melee);
            AssertInHand(danger);
        }

        [Test()]
        public void TestAcolytesEndOfTurnNoTrash()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //The first time each turn this card is dealt damage, reveal and replace the top card of the villain deck, then 1 player may play a card.
            //At the end of the villain turn, if there is an ongoing on top the villain trash, this card deals the hero target with the second highest HP {H} infernal damage. Otherwise this card deals the {H - 1} hero targets with the lowest HP 1 melee and 1 infernal damage each.
            ClearInitialCards();
            PlayCard(acolytes);

            MoveCards(doomsayer, FindCardsWhere((Card c) => c.Location == doomsayer.TurnTaker.Trash), doomsayer.TurnTaker.Deck,overrideIndestructible: true);
            StackDeckAfterShuffle(doomsayer,new string[] { "LookingForALoophole" });

            PlayCard("HeavyPlating");
            PlayCard("AFriendlyFace");
            PutOnDeck("MongolianDeathWorm");

            QuickHPStorage(ra, legacy, bunker, scholar);

            GoToEndOfTurn(doomsayer);

            QuickHPCheck(-2, 0, 0, -2);
        }

        [Test()]
        public void TestAcolytesEndOfTurnNonOngoing()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //The first time each turn this card is dealt damage, reveal and replace the top card of the villain deck, then 1 player may play a card.
            //At the end of the villain turn, if there is an ongoing on top the villain trash, this card deals the hero target with the second highest HP {H} infernal damage. Otherwise this card deals the {H - 1} hero targets with the lowest HP 1 melee and 1 infernal damage each.
            ClearInitialCards();
            PlayCard(acolytes);

            MoveCards(doomsayer, FindCardsWhere((Card c) => c.Location == doomsayer.TurnTaker.Trash), doomsayer.TurnTaker.Deck,overrideIndestructible: true);

            PlayCard("HeavyPlating");
            PlayCard("AFriendlyFace");
            PlayCard("LookingForALoophole");
            FlipCard(doomsayer.CharacterCard);
            PutInTrash("DifferentTimesDifferentFaces");
            PutOnDeck("MongolianDeathWorm");

            QuickHPStorage(ra, legacy, bunker, scholar);

            GoToEndOfTurn(doomsayer);

            QuickHPCheck(-2, 0, 0, -2);
        }

        [Test()]
        public void TestAcolytesEndOfTurnOngoing()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //The first time each turn this card is dealt damage, reveal and replace the top card of the villain deck, then 1 player may play a card.
            //At the end of the villain turn, if there is an ongoing on top the villain trash, this card deals the hero target with the second highest HP {H} infernal damage. Otherwise this card deals the {H - 1} hero targets with the lowest HP 1 melee and 1 infernal damage each.
            ClearInitialCards();
            PlayCard(acolytes);

            MoveCards(doomsayer, FindCardsWhere((Card c) => c.Location == doomsayer.TurnTaker.Trash), doomsayer.TurnTaker.Deck);
            StackDeckAfterShuffle(doomsayer, new string[] { "LookingForALoophole" });

            PlayCard("AFriendlyFace");
            PutInTrash("DictatingDestiny");
            PutOnDeck("MongolianDeathWorm");

            QuickHPStorage(ra, legacy, bunker, scholar);

            GoToEndOfTurn(doomsayer);

            QuickHPCheck(-4, 0, 0, 0);
        }

        [Test()]
        public void TestInginiDamage()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //The first time each turn this card is dealt damage, 1 hero may use a power.
            ClearInitialCards();
            PlayCard(ingini);

            DecisionSelectTurnTaker = ra.TurnTaker;
            DecisionSelectTarget = legacy.CharacterCard;
            QuickHPStorage(legacy);
            DealDamage(bunker, ingini, 1, DamageType.Melee);
            QuickHPCheck(-2);

            DecisionSelectTurnTaker = scholar.TurnTaker;
            SetHitPoints(scholar, 10);
            QuickHPStorage(scholar);
            DealDamage(bunker, ingini, 1, DamageType.Melee);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestInginiEndOfTurn()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //At the end of the villain turn, {Ingini} deals the hero target with the highest HP {H - 3} irreducible infernal damage. Increase damage dealt by {Ingini} by 1 until this card leaves play.
            ClearInitialCards();
            PlayCard(ingini);

            MoveCards(doomsayer, FindCardsWhere((Card c) => c.Location == doomsayer.TurnTaker.Trash), doomsayer.TurnTaker.Deck);
            StackDeckAfterShuffle(doomsayer, new string[] { "LookingForALoophole" });

            PlayCard("AFriendlyFace");
            PutOnDeck("MongolianDeathWorm");
            PlayCard("Fortitude");

            QuickHPStorage(ra, legacy, bunker, scholar);
            GoToEndOfTurn(doomsayer);
            QuickHPCheck(0, -1, 0, 0);

            QuickHPStorage(ra);
            DealDamage(ingini, ra, 1, DamageType.Melee);
            QuickHPCheck(-2);

            PutOnDeck("MongolianDeathWorm");

            MoveCards(doomsayer, FindCardsWhere((Card c) => c.Location.IsHand), (Card c) => c.NativeTrash);

            QuickHPStorage(ra, legacy, bunker, scholar);
            GoToEndOfTurn(doomsayer);
            QuickHPCheck(0, -2, 0, 0);

            QuickHPStorage(ra);
            DealDamage(ingini, ra, 1, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestBladewightDamage()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //When this card would be dealt 5 or fewer damage, reduce that damage to 1.
            //When {Bladewight} is dealt damage, increase the next damage dealt by {Bladewight} by 1, then 1 player may draw a card.
            ClearInitialCards();
            PlayCard(bladewight);

            //Check that dealing Bladewight 1 damage deals him 1 damage, and lets a hero draw
            QuickHPStorage(bladewight);
            DecisionSelectTurnTaker = ra.TurnTaker;
            QuickHandStorage(ra, legacy, bunker, scholar);
            DealDamage(legacy, bladewight, 1, DamageType.Melee);
            QuickHPCheck(-1);
            QuickHandCheck(1, 0, 0, 0);

            //Check that the next damage dealt by Bladewight is increased by 1
            QuickHPStorage(ra);
            DealDamage(bladewight, ra, 1, DamageType.Melee);
            QuickHPCheck(-2);

            QuickHPStorage(ra);
            DealDamage(bladewight, ra, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that dealing Bladewight 5 damage is reduced to 1 damage
            QuickHPStorage(bladewight);
            DecisionSelectTurnTaker = ra.TurnTaker;
            QuickHandStorage(ra, legacy, bunker, scholar);
            DealDamage(legacy, bladewight, 5, DamageType.Melee);
            QuickHPCheck(-1);
            QuickHandCheck(1, 0, 0, 0);

            //Check that dealing Bladewight 6 damage is not reduced
            QuickHPStorage(bladewight);
            DecisionSelectTurnTaker = ra.TurnTaker;
            QuickHandStorage(ra, legacy, bunker, scholar);
            DealDamage(legacy, bladewight, 6, DamageType.Melee);
            QuickHPCheck(-6);
            QuickHandCheck(1, 0, 0, 0);

            //Check that the next damage dealt by Bladewight is increased by 2
            QuickHPStorage(ra);
            DealDamage(bladewight, ra, 1, DamageType.Melee);
            QuickHPCheck(-3);

            QuickHPStorage(ra);
            DealDamage(bladewight, ra, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestBladewightEndOfTurn()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //At the end of the villain turn, this card deals the {H - 1} hero targets with the highest HP 2 melee damage each.
            ClearInitialCards();
            PlayCard(bladewight);

            MoveCards(doomsayer, FindCardsWhere((Card c) => c.Location == doomsayer.TurnTaker.Trash), doomsayer.TurnTaker.Deck);
            StackDeckAfterShuffle(doomsayer, new string[] { "LookingForALoophole" });

            PlayCard("AFriendlyFace");
            PutOnDeck("MongolianDeathWorm");

            QuickHPStorage(ra, legacy, bunker, scholar);
            GoToEndOfTurn(doomsayer);
            QuickHPCheck(-2, -2, 0, -2);
        }

        [Test()]
        public void TestACertainFinalityPlayTarget()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //When this card enters play, discard cards from the top of the villain deck until a target or proclamation is discarded. Put it into play.
            ClearInitialCards();
            Card alone = PutOnDeck("YouAreAlone");
            PutOnDeck("Ingini");
            Card face = PutOnDeck("AFriendlyFace");
            Card fate = PutOnDeck("BloodBoundFate");
            Card certain = PlayCard("ACertainFinality");

            AssertIsInPlay(ingini);
            AssertInTrash(new Card[] { face, fate });
            AssertIsInPlay(certain);
            AssertInDeck(alone);
        }

        [Test()]
        public void TestACertainFinalityPlayProclamation()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //When this card enters play, discard cards from the top of the villain deck until a target or proclamation is discarded. Put it into play.
            ClearInitialCards();
            PutOnDeck("Ingini");
            Card alone = PutOnDeck("YouAreAlone");
            Card face = PutOnDeck("AFriendlyFace");
            Card fate = PutOnDeck("BloodBoundFate");
            Card certain = PlayCard("ACertainFinality");

            AssertIsInPlay(alone);
            AssertInTrash(new Card[] { face, fate });
            AssertIsInPlay(certain);
            AssertInDeck(ingini);
        }

        [Test()]
        public void TestACertainFinalityReduce()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "TheFinalWasteland");
            StartGame();

            //When damage would be dealt to a hero target, that damage cannot be reduced, redirected, or prevented by non-villain cards.
            ClearInitialCards();
            PutOnDeck("Ingini");
            Card certain = PlayCard("ACertainFinality");
            PlayCard("Fortitude");
            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestACertainFinalityEnvironmentReduce()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "PikeIndustrialComplex");
            StartGame();

            //When damage would be dealt to a hero target, that damage cannot be reduced, redirected, or prevented by non-villain cards.
            ClearInitialCards();
            PutOnDeck("Ingini");
            Card certain = PlayCard("ACertainFinality");
            PlayCard("BiomemeticPlasmaVat");
            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 2, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestACertainFinalityVillainReduce()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When damage would be dealt to a hero target, that damage cannot be reduced, redirected, or prevented by non-villain cards.
            ClearInitialCards();
            PutOnDeck("Ingini");
            Card certain = PlayCard("ACertainFinality");
            FlipCard(countless);

            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 2, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestACertainFinalityRedirect()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When damage would be dealt to a hero target, that damage cannot be reduced, redirected, or prevented by non-villain cards.
            ClearInitialCards();
            PutOnDeck("Ingini");
            Card certain = PlayCard("ACertainFinality");
            Card lead = PlayCard("LeadFromTheFront");
            DecisionYesNo = true;

            QuickHPStorage(ra, legacy);
            DealDamage(ingini, ra, 1, DamageType.Melee);
            QuickHPCheck(-1, 0);
        }

        [Test()]
        public void TestACertainFinalityPrevent()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "VoidGuardWrithe/VoidGuardWritheCosmicInventor");
            StartGame();

            //When damage would be dealt to a hero target, that damage cannot be reduced, redirected, or prevented by non-villain cards.
            ClearInitialCards();
            PutOnDeck("Ingini");
            Card certain = PlayCard("ACertainFinality");
            DecisionSelectCard = ra.CharacterCard;
            UsePower(voidWrithe);

            QuickHPStorage(ra);
            DealDamage(legacy, ra, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestAFriendlyFaceIndestructible()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //{Doomsayer} is indestructible, immune to damage, and can’t deal damage.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            PlayCard("AFriendlyFace");

            DestroyCard(doomsayer.CharacterCard);
            AssertIsInPlay(doomsayer.CharacterCard);
        }

        [Test()]
        public void TestAFriendlyFaceImmune()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //{Doomsayer} is indestructible, immune to damage, and can’t deal damage.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            PlayCard("AFriendlyFace");

            QuickHPStorage(doomsayer);
            DealDamage(legacy,doomsayer,10,DamageType.Melee);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestAFriendlyFaceNoDamage()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //{Doomsayer} is indestructible, immune to damage, and can’t deal damage.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            PlayCard("AFriendlyFace");

            QuickHPStorage(legacy);
            DealDamage(doomsayer, legacy, 10, DamageType.Melee);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestAFriendlyFaceSkipPlay()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the start of each player’s turn, that player may skip their play, power, or draw phase to add a token to this card.
            ClearInitialCards();
            StackDeckAfterShuffle(doomsayer,new string[] { "SeeingRed" });
            Card role = PlayCard("AFriendlyFace");

            AssertDecisionIsOptional(SelectionType.SelectFunction);
            DecisionSelectFunction = 0;

            GoToStartOfTurn(ra);
            AssertTokenPoolCount(role.FindTokenPool("RolePool"),1);

            GoToPlayCardPhase(ra);
            AssertPhaseActionCount(null);
        }

        [Test()]
        public void TestAFriendlyFaceSkipPower()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the start of each player’s turn, that player may skip their play, power, or draw phase to add a token to this card.
            ClearInitialCards();
            StackDeckAfterShuffle(doomsayer, new string[] { "SeeingRed" });
            Card role = PlayCard("AFriendlyFace");

            AssertDecisionIsOptional(SelectionType.SelectFunction);
            DecisionSelectFunction = 1;

            GoToStartOfTurn(ra);
            AssertTokenPoolCount(role.FindTokenPool("RolePool"), 1);

            GoToUsePowerPhase(ra);
            AssertPhaseActionCount(null);
        }

        [Test()]
        public void TestAFriendlyFaceSkipDraw()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the start of each player’s turn, that player may skip their play, power, or draw phase to add a token to this card.
            ClearInitialCards();
            StackDeckAfterShuffle(doomsayer, new string[] { "SeeingRed" });
            Card role = PlayCard("AFriendlyFace");

            AssertDecisionIsOptional(SelectionType.SelectFunction);
            DecisionSelectFunction = 2;

            GoToStartOfTurn(ra);
            AssertTokenPoolCount(role.FindTokenPool("RolePool"), 1);

            GoToDrawCardPhase(ra);
            AssertPhaseActionCount(null);
        }

        [Test()]
        public void TestAFriendlyFaceRemove()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Then, if there are more tokens on this card than active heroes, each hero deals itself 1 psychic damage, then remove this card from the game.
            ClearInitialCards();
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            StackDeckAfterShuffle(doomsayer, new string[] { "LookingForALoophole" });
            Card role = PlayCard("AFriendlyFace");
            AddTokensToPool(role.FindTokenPool("RolePool"), 4);
            DecisionSelectFunction = 0;
            PutOnDeck("VelociraptorPack");

            QuickHPStorage(ra, legacy, bunker, scholar);
            GoToStartOfTurn(ra);
            QuickHPCheck(-1, -1, -1, -1);
            AssertOutOfGame(role);
        }

        [Test()]
        public void TestSomeoneInNeedIndestructible()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //{Doomsayer} is indestructible, immune to damage, and can’t deal damage.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            PlayCard("SomeoneInNeed");

            DestroyCard(doomsayer.CharacterCard);
            AssertIsInPlay(doomsayer.CharacterCard);
        }

        [Test()]
        public void TestSomeoneInNeedImmune()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //{Doomsayer} is indestructible, immune to damage, and can’t deal damage.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            PlayCard("SomeoneInNeed");

            QuickHPStorage(doomsayer);
            DealDamage(legacy, doomsayer, 10, DamageType.Melee);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestSomeoneInNeedNoDamage()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //{Doomsayer} is indestructible, immune to damage, and can’t deal damage.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            PlayCard("SomeoneInNeed");

            QuickHPStorage(legacy);
            DealDamage(doomsayer, legacy, 10, DamageType.Melee);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestSomeoneInNeedSkipPlay()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the start of each player’s turn, that player may skip their play, power, or draw phase to add a token to this card.
            ClearInitialCards();
            StackDeckAfterShuffle(doomsayer, new string[] { "SeeingRed" });
            Card role = PlayCard("SomeoneInNeed");

            AssertDecisionIsOptional(SelectionType.SelectFunction);
            DecisionSelectFunction = 0;

            GoToStartOfTurn(ra);
            AssertTokenPoolCount(role.FindTokenPool("RolePool"), 1);

            GoToPlayCardPhase(ra);
            AssertPhaseActionCount(null);
        }

        [Test()]
        public void TestSomeoneInNeedSkipPower()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the start of each player’s turn, that player may skip their play, power, or draw phase to add a token to this card.
            ClearInitialCards();
            StackDeckAfterShuffle(doomsayer, new string[] { "SeeingRed" });
            Card role = PlayCard("SomeoneInNeed");

            AssertDecisionIsOptional(SelectionType.SelectFunction);
            DecisionSelectFunction = 1;

            GoToStartOfTurn(ra);
            AssertTokenPoolCount(role.FindTokenPool("RolePool"), 1);

            GoToUsePowerPhase(ra);
            AssertPhaseActionCount(null);
        }

        [Test()]
        public void TestSomeoneInNeedSkipDraw()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //At the start of each player’s turn, that player may skip their play, power, or draw phase to add a token to this card.
            ClearInitialCards();
            StackDeckAfterShuffle(doomsayer, new string[] { "SeeingRed" });
            Card role = PlayCard("SomeoneInNeed");

            AssertDecisionIsOptional(SelectionType.SelectFunction);
            DecisionSelectFunction = 2;

            GoToStartOfTurn(ra);
            AssertTokenPoolCount(role.FindTokenPool("RolePool"), 1);

            GoToDrawCardPhase(ra);
            AssertPhaseActionCount(null);
        }

        [Test()]
        public void TestSomeoneInNeedRemove()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Then, if there are more tokens on this card than active heroes, each hero deals itself 1 psychic damage, then remove this card from the game.
            ClearInitialCards();
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            StackDeckAfterShuffle(doomsayer, new string[] { "LookingForALoophole" });
            Card role = PlayCard("SomeoneInNeed");
            AddTokensToPool(role.FindTokenPool("RolePool"), 4);
            DecisionSelectFunction = 0;
            PutOnDeck("VelociraptorPack");

            QuickHPStorage(ra, legacy, bunker, scholar);
            GoToStartOfTurn(ra);
            QuickHPCheck(-1, -1, -1, -1);
            AssertOutOfGame(role);
        }

        [Test()]
        public void TestBloodBoundFatePlay()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //When this card enters play, a player may move a card from their trash to their hand.
            ClearInitialCards();
            Card fortitude = PutInTrash("Fortitude");
            Card blazing = PutInTrash("BlazingTornado");
            Card ammo = PutInTrash("AmmoDrop");
            Card knives = PutInTrash("ThrowingKnives");

            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card blood = PlayCard("BloodBoundFate");
            AssertInHand(fortitude);
            AssertInTrash(new Card[] { blazing, ammo, knives });
            AssertIsInPlay(blood);
        }

        [Test()]
        public void TestBloodBoundFatePreventDamage()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //When a hero would deal damage to {Doomsayer}, they may first deal themselves that same damage.
            ClearInitialCards();
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            Card blood = PlayCard("BloodBoundFate");
            DecisionYesNo = false;

            QuickHPStorage(legacy, doomsayer);
            DealDamage(legacy, doomsayer, 1, DamageType.Melee);
            QuickHPCheck(0, 0);

            DecisionYesNo = true;

            QuickHPStorage(legacy, doomsayer);
            DealDamage(legacy, doomsayer, 1, DamageType.Melee);
            QuickHPCheck(-1, -1);
        }

        [Test()]
        public void TestBloodBoundFatePreviousDamage()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //When a hero would deal damage to {Doomsayer}, they may first deal themselves that same damage.
            //When a hero would deal damage to {Doomsayer}, prevent that damage if that hero has not dealt themselves damage this turn.
            ClearInitialCards();
            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            Card blood = PlayCard("BloodBoundFate");
            DecisionYesNo = false;

            DealDamage(legacy, legacy, 1, DamageType.Melee);

            QuickHPStorage(legacy, doomsayer);
            DealDamage(legacy, doomsayer, 1, DamageType.Melee);
            QuickHPCheck(0, -1);
        }

        [Test()]
        public void TestDarkestBeforeTheDawnIncrease()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //The first time each turn any hero target would be dealt damage, increase that damage by 3.
            ClearInitialCards();
            Card darkest = PlayCard("DarkestBeforeTheDawn");
            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 1, DamageType.Melee);
            QuickHPCheck(-4);

            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(wraith);
            DealDamage(ra, wraith, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestDarkestBeforeTheDawnHPGain()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //When {Doomsayer} regains 13 or more hp at once, destroy an ongoing.
            ClearInitialCards();
            Card darkest = PlayCard("DarkestBeforeTheDawn");
            SetHitPoints(doomsayer.CharacterCard, -13);
            Card fortitude = PlayCard("Fortitude");
            Card knives = PlayCard("ThrowingKnives");

            AssertNextDecisionChoices(new Card[] { fortitude, darkest }, new Card[] { knives });
            DecisionSelectCard = fortitude;
            GainHP(doomsayer.CharacterCard, 13);
            AssertInTrash(fortitude);
        }

        [Test()]
        public void TestDarkestBeforeTheDawnEndOfTurn()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //At the end of each turn, if {Doomsayer} has -13 or fewer hp, he regains 13 HP.
            ClearInitialCards();
            StackDeckAfterShuffle(doomsayer,new string[] { "SeeingRed" });
            SetHitPoints(doomsayer.CharacterCard, -13);
            GoToStartOfTurn(ra);

            PlayCard("DarkestBeforeTheDawn");
            QuickHPStorage(doomsayer);
            GoToEndOfTurn(ra);
            QuickHPCheck(13);

            QuickHPStorage(doomsayer);
            GoToEndOfTurn(legacy);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestDictatingDestiny()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += MakeDecisions2;

            //When a villain target would be dealt damage, redirect that damage to the non-villain target with the lowest hp.
            //When this card enters play, play the top card of each hero deck in turn order. The first {H - 2} times a non-one-shot enters play this way, play the top card of the villain deck.
            //At the end of the villain turn, 1 player may discard 2 cards to destroy this card.
            ClearInitialCards();
            PlayCard(bladewight);
            Card rex = PlayCard("EnragedTRex");

            Card summon = PutOnDeck("SummonStaff");
            Card fortitude = PutOnDeck("Fortitude");
            Card ammo = PutOnDeck("AmmoDrop");
            Card knives = PutOnDeck("ThrowingKnives");
            Card face = PutOnDeck("AFriendlyFace");
            Card looking = PutOnDeck("LookingForALoophole");
            Card nothing = PutOnDeck("NothingToSeeHere");

            DecisionDoNotSelectCard = SelectionType.PlayCard;
            Card destiny = PlayCard("DictatingDestiny");

            //Summon Staff, Fortitude, Ammo Drop, Throwing Knives, Nothing to See Here, and Looking for a Loophole should all have been played
            //A Friendly Face should still be in the villain deck since it only plays for the first 2 non-one-shot cards
            AssertInTrash(summon);
            AssertIsInPlay(new Card[] { fortitude, ammo, knives, nothing, looking });
            AssertInDeck(face);

            //Check that damage to Bladewight is redirected to T-Rex
            QuickHPStorage(bladewight, rex);
            DealDamage(ra, bladewight, 2, DamageType.Melee);
            QuickHPCheck(0, -2);

            //Check that damage to Legacy is not redirected
            QuickHPStorage(legacy.CharacterCard, rex);
            DealDamage(ra, legacy, 2, DamageType.Melee);
            QuickHPCheck(-1, 0);

            //Check the end-of-turn discard effect
            MoveAllCards(ra, ra.HeroTurnTaker.Hand, ra.TurnTaker.Trash);
            Card blazing = PutInHand("BlazingTornado");
            Card blast = PutInHand("FireBlast");
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            FlipCard(doomsayer);
            DecisionSelectTurnTaker = ra.TurnTaker;
            GoToEndOfTurn(doomsayer);
            AssertInTrash(new Card[] { blazing, blast, destiny });
        }

        [Test()]
        public void TestDifferentTimesDifferentFaces()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //Reveal cards from the top of the villain deck until a proclamation and a role are revealed.
            //Put the first revealed card of each type into play. Shuffle the other cards into the villain deck.
            //Each player discards a random card from their hand.
            //Play the top card of the villain deck.
            ClearInitialCards();
            Card face = PutOnDeck("AFriendlyFace");
            Card need = PutOnDeck("SomeoneInNeed");
            Card broken = PutOnDeck("YouAreBroken");
            Card change = PutOnDeck("YouCantChange");
            Card destiny = PutOnDeck("DictatingDestiny");

            Card red = GetCard("SeeingRed");
            StackDeckAfterShuffle(doomsayer, new Card[] { red });

            //You Can't Change and Someone in Need should be put into play.
            //Then, Seeing Red should be played from the villain deck.
            //Dictating Destiny, You Are Broken, and A Friendly Face should be shuffled into the villain deck.
            QuickHandStorage(ra, legacy, bunker, wraith);
            QuickShuffleStorage(doomsayer);
            Card diff = PlayCard("DifferentTimesDifferentFaces");
            AssertIsInPlay(new Card[] { change, need, red });
            AssertInTrash(diff);
            AssertInDeck(new Card[] { destiny, broken, face });
            QuickHandCheck(-1, -1, -1, -1);
            QuickShuffleCheck(1);
        }

        [Test()]
        public void TestDifferentTimesDifferentFaces2()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //Reveal cards from the top of the villain deck until a proclamation and a role are revealed.
            //Put the first revealed card of each type into play. Shuffle the other cards into the villain deck.
            //Each player discards a random card from their hand.
            //Play the top card of the villain deck.
            ClearInitialCards();
            Card broken = PutOnDeck("YouAreBroken");
            Card change = PutOnDeck("YouCantChange");
            Card face = PutOnDeck("AFriendlyFace");
            Card need = PutOnDeck("SomeoneInNeed");
            Card destiny = PutOnDeck("DictatingDestiny");

            Card red = GetCard("SeeingRed");
            StackDeckAfterShuffle(doomsayer, new Card[] { red });

            //You Can't Change and Someone in Need should be put into play.
            //Then, Seeing Red should be played from the villain deck.
            //Dictating Destiny, You Are Broken, and A Friendly Face should be shuffled into the villain deck.
            QuickHandStorage(ra, legacy, bunker, wraith);
            QuickShuffleStorage(doomsayer);
            Card diff = PlayCard("DifferentTimesDifferentFaces");
            AssertIsInPlay(new Card[] { change, need, red });
            AssertInTrash(diff);
            AssertInDeck(new Card[] { destiny, broken, face });
            QuickHandCheck(-1, -1, -1, -1);
            QuickShuffleCheck(1);
        }

        [Test()]
        public void TestDifferentTimesDifferentFacesNoProc()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //Reveal cards from the top of the villain deck until a proclamation and a role are revealed.
            //Put the first revealed card of each type into play. Shuffle the other cards into the villain deck.
            //Each player discards a random card from their hand.
            //Play the top card of the villain deck.
            ClearInitialCards();
            MoveCards(doomsayer, FindCardsWhere((Card c) => c.DoKeywordsContain("proclamation")), doomsayer.TurnTaker.Trash);

            Card face = PutOnDeck("AFriendlyFace");
            Card need = PutOnDeck("SomeoneInNeed");
            Card destiny = PutOnDeck("DictatingDestiny");

            Card red = GetCard("SeeingRed");
            StackDeckAfterShuffle(doomsayer, new Card[] { red });

            //You Can't Change and Someone in Need should be put into play.
            //Then, Seeing Red should be played from the villain deck.
            //Dictating Destiny, You Are Broken, and A Friendly Face should be shuffled into the villain deck.
            QuickHandStorage(ra, legacy, bunker, wraith);
            QuickShuffleStorage(doomsayer);
            Card diff = PlayCard("DifferentTimesDifferentFaces");
            AssertIsInPlay(new Card[] { need, red });
            AssertInTrash(diff);
            AssertInDeck(new Card[] { destiny, face });
            Assert.IsTrue(!FindCardsWhere((Card c) => c.DoKeywordsContain("proclamation") && c.Location != doomsayer.TurnTaker.Trash).Any(), "A proclamation was moved from Doomsayer's trash");
            QuickHandCheck(-1, -1, -1, -1);
            QuickShuffleCheck(1);
        }

        [Test()]
        public void TestDifferentTimesDifferentFacesNoRole()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //Reveal cards from the top of the villain deck until a proclamation and a role are revealed.
            //Put the first revealed card of each type into play. Shuffle the other cards into the villain deck.
            //Each player discards a random card from their hand.
            //Play the top card of the villain deck.
            ClearInitialCards();
            MoveCards(doomsayer, FindCardsWhere((Card c) => c.DoKeywordsContain("role")), doomsayer.TurnTaker.Trash);

            Card broken = PutOnDeck("YouAreBroken");
            Card change = PutOnDeck("YouCantChange");
            Card destiny = PutOnDeck("DictatingDestiny");

            Card red = GetCard("SeeingRed");
            StackDeckAfterShuffle(doomsayer, new Card[] { red });

            //You Can't Change and Someone in Need should be put into play.
            //Then, Seeing Red should be played from the villain deck.
            //Dictating Destiny, You Are Broken, and A Friendly Face should be shuffled into the villain deck.
            QuickHandStorage(ra, legacy, bunker, wraith);
            QuickShuffleStorage(doomsayer);
            Card diff = PlayCard("DifferentTimesDifferentFaces");
            AssertIsInPlay(new Card[] { change, red });
            AssertInTrash(diff);
            AssertInDeck(new Card[] { destiny, broken });
            Assert.IsTrue(!FindCardsWhere((Card c) => c.DoKeywordsContain("role") && c.Location != doomsayer.TurnTaker.Trash).Any(), "A role was moved from Doomsayer's trash");
            QuickHandCheck(-1, -1, -1, -1);
            QuickShuffleCheck(1);
        }

        [Test()]
        public void TestDifferentTimesDifferentFacesNoCards()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //Reveal cards from the top of the villain deck until a proclamation and a role are revealed.
            //Put the first revealed card of each type into play. Shuffle the other cards into the villain deck.
            //Each player discards a random card from their hand.
            //Play the top card of the villain deck.
            ClearInitialCards();
            MoveCards(doomsayer, FindCardsWhere((Card c) => c.DoKeywordsContain("role") || c.DoKeywordsContain("proclamation")), doomsayer.TurnTaker.Trash);

            Card destiny = PutOnDeck("DictatingDestiny");

            Card red = GetCard("SeeingRed");
            StackDeckAfterShuffle(doomsayer, new Card[] { red });

            //You Can't Change and Someone in Need should be put into play.
            //Then, Seeing Red should be played from the villain deck.
            //Dictating Destiny, You Are Broken, and A Friendly Face should be shuffled into the villain deck.
            QuickHandStorage(ra, legacy, bunker, wraith);
            QuickShuffleStorage(doomsayer);
            Card diff = PlayCard("DifferentTimesDifferentFaces");
            AssertIsInPlay(new Card[] { red });
            AssertInTrash(diff);
            AssertInDeck(new Card[] { destiny });
            Assert.IsTrue(!FindCardsWhere((Card c) => (c.DoKeywordsContain("role") || c.DoKeywordsContain("proclamation"))&& c.Location != doomsayer.TurnTaker.Trash).Any(), "A role or proclamation was moved from Doomsayer's trash");
            QuickHandCheck(-1, -1, -1, -1);
            QuickShuffleCheck(1);
        }

        [Test()]
        public void TestFutileEffortsReduce()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //Reduce hero damage by 1.
            ClearInitialCards();
            Card futile = PlayCard("FutileEfforts");
            QuickHPStorage(ra);
            DealDamage(legacy, ra, 2,DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestFutileEffortsTokens()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //At the end of the villain turn, each player may discard any number of cards. Add a token to this card for each card discarded this way.
            //Then, if there are {H * 3} or more tokens on this card, put this card under {CountlessWords}.
            ClearInitialCards();
            Card futile = PlayCard("FutileEfforts");
            FlipCard(doomsayer);
            MoveAllCards(wraith, wraith.HeroTurnTaker.Hand, wraith.TurnTaker.Trash, leaveSomeCards: 1);
            MoveAllCards(ra, ra.HeroTurnTaker.Hand, ra.TurnTaker.Trash, leaveSomeCards: 1);
            QuickHandStorage(ra, legacy, bunker, wraith);
            GoToEndOfTurn(doomsayer);
            QuickHandCheck(-1, -4, -4, -1);
            AssertTokenPoolCount(futile.FindTokenPool("FutileEffortsPool"), 10);
            AssertIsInPlay(futile);
        }

        [Test()]
        public void TestFutileEffortsMoveUnder()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //At the end of the villain turn, each player may discard any number of cards. Add a token to this card for each card discarded this way.
            //Then, if there are {H * 3} or more tokens on this card, put this card under {CountlessWords}.
            ClearInitialCards();
            Card futile = PlayCard("FutileEfforts");
            FlipCard(doomsayer);
            MoveAllCards(wraith, wraith.HeroTurnTaker.Hand, wraith.TurnTaker.Trash, leaveSomeCards: 1);
            MoveAllCards(ra, ra.HeroTurnTaker.Hand, ra.TurnTaker.Trash, leaveSomeCards: 3);
            QuickHandStorage(ra, legacy, bunker, wraith);
            GoToEndOfTurn(doomsayer);
            QuickHandCheck(-3, -4, -4, -1);
            AssertAtLocation(futile, countless.UnderLocation);
        }

        [Test()]
        public void TestLookingForALoopholeEndOfTurnDiscard()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();
            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += MakeDecisions2;

            //At the end of each hero turn, that hero’s player discards 1 card or destroys one of their ongoing or equipment cards, then may play a card.
            //Then they may discard a card to put a token on this card. Then, if there are {H} or more tokens on this card, put this card under {CountlessWords}.
            ClearInitialCards();
            Card looking = GetCard("LookingForALoophole");
            StackDeckAfterShuffle(doomsayer,new Card[] { looking });
            MoveAllCards(ra, ra.HeroTurnTaker.Hand, ra.TurnTaker.Trash);
            Card blazing = PutInHand("BlazingTornado");
            Card flesh = PutInHand("FleshOfTheSunGod");
            Card gaze = PutInHand("WrathfulGaze");
            DecisionSelectCard = blazing;
            DecisionSelectCardToPlay = flesh;

            GoToEndOfTurn(ra);
            AssertInTrash(new[] { blazing, gaze });
            AssertIsInPlay(flesh);
            AssertTokenPoolCount(looking.FindTokenPool("LookingForALoopholePool"), 1);
        }

        [Test()]
        public void TestLookingForALoopholeEndOfTurnNoDiscard()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();
            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += MakeDecisions2;

            //At the end of each hero turn, that hero’s player discards 1 card or destroys one of their ongoing or equipment cards, then may play a card.
            //Then they may discard a card to put a token on this card. Then, if there are {H} or more tokens on this card, put this card under {CountlessWords}.
            ClearInitialCards();
            Card looking = GetCard("LookingForALoophole");
            StackDeckAfterShuffle(doomsayer, new Card[] { looking });
            MoveAllCards(ra, ra.HeroTurnTaker.Hand, ra.TurnTaker.Trash);
            Card blazing = PutInHand("BlazingTornado");
            Card flesh = PutInHand("FleshOfTheSunGod");
            DecisionSelectCard = blazing;
            DecisionSelectCardToPlay = flesh;

            GoToEndOfTurn(ra);
            AssertInTrash(new[] { blazing});
            AssertIsInPlay(flesh);
            AssertTokenPoolCount(looking.FindTokenPool("LookingForALoopholePool"), 0);
        }

        [Test()]
        public void TestLookingForALoopholeEndOfTurnDestroy()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();
            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += MakeDecisions2;

            //At the end of each hero turn, that hero’s player discards 1 card or destroys one of their ongoing or equipment cards, then may play a card.
            //Then they may discard a card to put a token on this card. Then, if there are {H} or more tokens on this card, put this card under {CountlessWords}.
            ClearInitialCards();
            Card looking = GetCard("LookingForALoophole");
            StackDeckAfterShuffle(doomsayer, new Card[] { looking });
            MoveAllCards(ra, ra.HeroTurnTaker.Hand, ra.TurnTaker.Trash);
            Card blazing = PlayCard("BlazingTornado");
            Card flesh = PutInHand("FleshOfTheSunGod");
            Card gaze = PutInHand("WrathfulGaze");
            DecisionSelectCard = blazing;
            DecisionSelectCardToPlay = flesh;
            DecisionSelectFunction = 1;

            GoToEndOfTurn(ra);
            AssertInTrash(new[] { blazing, gaze });
            AssertIsInPlay(flesh);
            AssertTokenPoolCount(looking.FindTokenPool("LookingForALoopholePool"), 1);
        }

        [Test()]
        public void TestLookingForALoopholeMove()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();
            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += MakeDecisions2;

            //At the end of each hero turn, that hero’s player discards 1 card or destroys one of their ongoing or equipment cards, then may play a card.
            //Then they may discard a card to put a token on this card. Then, if there are {H} or more tokens on this card, put this card under {CountlessWords}.
            ClearInitialCards();
            Card looking = GetCard("LookingForALoophole");
            StackDeckAfterShuffle(doomsayer, new Card[] { looking });
            MoveAllCards(ra, ra.HeroTurnTaker.Hand, ra.TurnTaker.Trash);
            MoveAllCards(legacy, legacy.HeroTurnTaker.Hand, legacy.TurnTaker.Trash);
            MoveAllCards(bunker, bunker.HeroTurnTaker.Hand, bunker.TurnTaker.Trash);
            MoveAllCards(wraith, wraith.HeroTurnTaker.Hand, wraith.TurnTaker.Trash);
            Card blazing = PutInHand("BlazingTornado");
            Card flesh = PutInHand("FleshOfTheSunGod");
            Card fortitude = PutInHand("Fortitude");
            Card thokk = PutInHand("Thokk");
            Card ammo = PutInHand("AmmoDrop");
            Card flak = PutInHand("FlakCannon");
            Card knives = PutInHand("ThrowingKnives");
            Card razor = PutInHand("RazorOrdnance");
            DecisionSelectFunction = 0;
            DecisionDoNotSelectCard = SelectionType.PlayCard;

            GoToEndOfTurn(ra);
            AssertInTrash(new[] { blazing, flesh });
            AssertTokenPoolCount(looking.FindTokenPool("LookingForALoopholePool"), 1);

            GoToEndOfTurn(legacy);
            AssertInTrash(new[] { fortitude, thokk });
            AssertTokenPoolCount(looking.FindTokenPool("LookingForALoopholePool"), 2);

            GoToEndOfTurn(bunker);
            AssertInTrash(new[] { ammo, flak });
            AssertTokenPoolCount(looking.FindTokenPool("LookingForALoopholePool"), 3);

            GoToEndOfTurn(wraith);
            AssertInTrash(new[] { knives, razor });
            AssertAtLocation(looking, countless.UnderLocation);
        }

        [Test()]
        public void TestNothingToSeeHereToken()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //The first time each turn each hero is dealt damage by any non-hero card other than {Doomsayer},
            //put a token on this card. Then if there are {H} or more tokens on this card, put this card under {CountlessWords}.
            ClearInitialCards();
            Card nothing = PlayCard("NothingToSeeHere");
            Card rex = PlayCard("EnragedTRex");
            PlayCard(ingini);

            TokenPool pool = nothing.FindTokenPool("NothingToSeeHerePool");

            DealDamage(doomsayer, legacy, 1, DamageType.Melee);
            AssertTokenPoolCount(pool, 0);

            DealDamage(ingini, legacy, 1, DamageType.Melee);
            AssertTokenPoolCount(pool, 1);

            DealDamage(ingini, legacy, 1, DamageType.Melee);
            AssertTokenPoolCount(pool, 1);

            DealDamage(rex, legacy, 1, DamageType.Melee);
            AssertTokenPoolCount(pool, 1);

            DealDamage(ingini, bunker, 1, DamageType.Melee);
            AssertTokenPoolCount(pool, 2);

            DealDamage(rex, wraith, 1, DamageType.Melee);
            AssertTokenPoolCount(pool, 3);

            DealDamage(ingini, ra, 1, DamageType.Melee);
            AssertAtLocation(nothing, countless.UnderLocation);
        }

        [Test()]
        public void TestNothingToSeeHereEndOfTurn()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //At the end of the villain turn, play the top card of the villain deck.
            ClearInitialCards();
            Card nothing = PlayCard("NothingToSeeHere");
            Card role = PutOnDeck("AFriendlyFace");
            FlipCard(doomsayer);
            GoToEndOfTurn(doomsayer);
            AssertIsInPlay(role);
        }

        [Test()]
        public void TestSeeingRedDamage()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //Increase damage dealt to and by hero targets by 1.
            ClearInitialCards();
            Card red = PlayCard("SeeingRed");
            Card rex = PlayCard("EnragedTRex");

            QuickHPStorage(ra);
            DealDamage(doomsayer, ra, 1, DamageType.Melee);
            QuickHPCheck(-2);

            QuickHPStorage(ra);
            DealDamage(rex, ra, 1, DamageType.Melee);
            QuickHPCheck(-2);

            QuickHPStorage(rex);
            DealDamage(ra, rex, 1, DamageType.Melee);
            QuickHPCheck(-2);

            QuickHPStorage(ra);
            DealDamage(ra, ra, 1, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestSeeingRedTokens()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //When a card is destroyed, add a token to this card. Then, if there are {H} or more tokens on this card, put this card under {CountlessWords}.
            Card red = PlayCard("SeeingRed");
            TokenPool pool = red.FindTokenPool("SeeingRedPool");

            Card rex = PlayCard("EnragedTRex");
            DestroyCard(rex);
            AssertTokenPoolCount(pool, 1);

            Card fortitude = PlayCard("Fortitude");
            DestroyCard(fortitude);
            AssertTokenPoolCount(pool, 2);

            MoveCard(doomsayer, "NothingToSeeHere", countless.UnderLocation);
            PlayCard(ingini);
            SetHitPoints(ingini, 1);
            DealDamage(legacy, ingini, 5, DamageType.Melee);
            AssertTokenPoolCount(pool, 3);

            Card dark = PlayCard("DarkestBeforeTheDawn");
            DestroyCard(dark);
            AssertAtLocation(red, countless.UnderLocation);
        }

        [Test()]
        public void TestSilverTonguedDevilPlay()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //When this card enters play, shuffle all one-shots and targets from the villain trash into the villain deck.
            ClearInitialCards();
            Card diff = PutInTrash("DifferentTimesDifferentFaces");
            PutInTrash(ingini);
            PutInTrash(bladewight);
            PutInTrash(acolytes);
            Card destiny = PutInTrash("DictatingDestiny");
            Card futile = PutInTrash("FutileEfforts");
            QuickShuffleStorage(doomsayer);
            PlayCard("SilverTonguedDevil");
            AssertInDeck(new Card[] {diff,ingini,bladewight,acolytes });
            AssertInTrash(new Card[] { destiny, futile });
        }

        [Test()]
        public void TestSilverTonguedDevilVillainPlayIncrease()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //When a villain card enters play, the hero with the highest hp may increase the next damage dealt to them by 3. Otherwise destroy a hero ongoing or equipment.
            //Then 1 player may discard their hand to destroy an ongoing card.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            Card silver = PlayCard("SilverTonguedDevil");
            DecisionYesNo = true;
            DecisionSelectTurnTaker = ra.TurnTaker;
            QuickHandStorage(ra);
            Card dark = GetCard("DarkestBeforeTheDawn");
            DecisionSelectCard = dark;
            Card gaze = PlayCard("WrathfulGaze");
            PlayCard(dark);

            //The next damage to Legacy should be increased by 3, and Wrathful Gaze should still be in play,
            //and Ra should have discarded his hand, and Darkest Before the Dawn should be destroyed
            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 1, DamageType.Melee);
            QuickHPCheck(-4);

            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);
            AssertInTrash(dark);
            AssertIsInPlay(gaze);
            QuickHandCheck(-4);
        }

        [Test()]
        public void TestSilverTonguedDevilVillainPlayDestroy()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //When a villain card enters play, the hero with the highest hp may increase the next damage dealt to them by 3. Otherwise destroy a hero ongoing or equipment.
            //Then 1 player may discard their hand to destroy an ongoing card.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            Card silver = PlayCard("SilverTonguedDevil");
            DecisionYesNo = false;
            DecisionDoNotSelectTurnTaker = true;
            QuickHandStorage(ra);
            Card dark = GetCard("DarkestBeforeTheDawn");
            DecisionSelectCard = dark;
            Card gaze = PlayCard("WrathfulGaze");
            PlayCard(dark);

            //The next damage to Legacy should not be increased by 3, and Wrathful Gaze should still be destroyed,
            //and Ra should not have discarded his hand, and Darkest Before the Dawn should be in play
            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 1, DamageType.Melee);
            QuickHPCheck(-4);
            AssertIsInPlay(dark);
            AssertInTrash(gaze);
            QuickHandCheck(0);
        }

        [Test()]
        public void TestSilverTonguedDevilEmptyHandDiscard()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //When a villain card enters play, the hero with the highest hp may increase the next damage dealt to them by 3. Otherwise destroy a hero ongoing or equipment.
            //Then 1 player may discard their hand to destroy an ongoing card.
            ClearInitialCards();
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            Card silver = PlayCard("SilverTonguedDevil");
            DecisionYesNo = true;
            DecisionSelectTurnTaker = ra.TurnTaker;
            MoveAllCards(ra, ra.HeroTurnTaker.Hand, ra.TurnTaker.Trash);
            Card dark = GetCard("DarkestBeforeTheDawn");
            DecisionSelectCard = dark;
            Card gaze = PlayCard("WrathfulGaze");
            PlayCard(dark);

            //The next damage to Legacy should be increased by 3, and Wrathful Gaze should still be in play,
            //and Ra should have discarded his empty hand, and Darkest Before the Dawn should be destroyed
            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 1, DamageType.Melee);
            QuickHPCheck(-4);

            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);
            AssertInTrash(dark);
            AssertIsInPlay(gaze);
        }

        [Test()]
        public void TestTheyOweYou()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //Play this card next to the hero with the fewest cards in play.
            //{Doomsayer} is indestructible.
            //At the end of that hero’s turn, that hero regains 1 hp, then deals each other hero target 1 psychic damage.
            ClearInitialCards();
            PlayCard("WrathfulGaze");
            PlayCard("MotivationalCharge");
            PlayCard("ThrowingKnives");
            SetHitPoints(bunker, 10);
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            FlipCard(doomsayer);

            Card owe = PlayCard("TheyOweYou");
            AssertAtLocation(owe, bunker.CharacterCard.NextToLocation);

            //Check that Doomsayer is indestructible
            SetHitPoints(doomsayer, 1);
            DealDamage(legacy, doomsayer, 5, DamageType.Melee);
            AssertNotGameOver();
            AssertIsInPlay(doomsayer.CharacterCard);

            //Check end of turn effect
            GoToStartOfTurn(bunker);
            QuickHPStorage(ra, legacy, bunker, wraith);
            GoToEndOfTurn(bunker);
            QuickHPCheck(-1, -1, 1, -1);
        }

        [Test()]
        public void TestYouAreBroken()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //Play this card next to the hero whose player has the fewest cards in their hand.
            //Targets in this play area cannot regain hp or have their hp increased.
            //{Doomsayer} is indestructible.
            ClearInitialCards();
            MoveAllCards(bunker, bunker.HeroTurnTaker.Hand, bunker.TurnTaker.Trash,true,3);
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            FlipCard(doomsayer);

            Card broken = PlayCard("YouAreBroken");
            AssertAtLocation(broken, bunker.CharacterCard.NextToLocation);

            //Check that Doomsayer is indestructible
            SetHitPoints(doomsayer, 1);
            DealDamage(legacy, doomsayer, 5, DamageType.Melee);
            AssertNotGameOver();
            AssertIsInPlay(doomsayer.CharacterCard);

            //Check that bunker cannot gain HP
            SetHitPoints(bunker, 10);
            SetHitPoints(legacy, 10);
            Card charge = PlayCard("MotivationalCharge");
            QuickHPStorage(legacy,bunker);
            UsePower(charge);
            QuickHPCheck(1, 0);

            QuickHPStorage(bunker);
            SetHitPoints(bunker, 15);
            QuickHPCheck(0);

            QuickHPStorage(bunker);
            SetHitPoints(bunker, 5);
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestYouAreRunningOutOfTime()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //Play this card in a hero play area.
            //The first time each turn that hero plays a card, put a random card from their hand into play.
            //{Doomsayer} is indestructible.
            ClearInitialCards();
            DecisionSelectTurnTaker = bunker.TurnTaker;
            AssertNextDecisionChoices(new TurnTaker[] { ra.TurnTaker, legacy.TurnTaker, bunker.TurnTaker, wraith.TurnTaker }, new TurnTaker[] { doomsayer.TurnTaker, FindEnvironment().TurnTaker });
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            FlipCard(doomsayer);

            Card time = PlayCard("YouAreRunningOutOfTime");
            AssertAtLocation(time, bunker.TurnTaker.PlayArea);

            //Check that Doomsayer is indestructible
            SetHitPoints(doomsayer, 1);
            DealDamage(legacy, doomsayer, 5, DamageType.Melee);
            AssertNotGameOver();
            AssertIsInPlay(doomsayer.CharacterCard);

            //Check random play
            MoveAllCards(bunker, bunker.HeroTurnTaker.Hand, bunker.TurnTaker.Trash);
            Card flak = PutInHand("FlakCannon");
            Card ammo = PutInHand("AmmoDrop");

            //Check that putting a card into play does not trigger it
            PlayCard(bunker, flak, true);
            AssertIsInPlay(flak);
            AssertInHand(ammo);

            //Check that a different turntaker playing a card belonging to the affected hero does not trigger it
            PutInHand(flak);
            RunCoroutine(this.GameController.PlayCard(legacy, flak, false, responsibleTurnTaker: legacy.TurnTaker));
            AssertIsInPlay(flak);
            AssertInHand(ammo);

            //Check that playing a card does trigger it
            PutInHand(flak);
            PlayCard(bunker,flak);
            AssertIsInPlay(new Card[] { flak, ammo });

            //Check that it only happens once per turn
            PutInHand(flak);
            PutInHand(ammo);
            PlayCard(bunker, flak);
            AssertIsInPlay(flak);
            AssertInHand(ammo);
        }

        [Test()]
        public void TestYouCantChange()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //Play this card in a hero play area.
            //When a power is used on a non-character card in that play area, destroy that card.
            //{Doomsayer} is indestructible.
            ClearInitialCards();
            DecisionSelectTurnTaker = bunker.TurnTaker;
            AssertNextDecisionChoices(new TurnTaker[] { ra.TurnTaker, legacy.TurnTaker, bunker.TurnTaker, wraith.TurnTaker }, new TurnTaker[] { doomsayer.TurnTaker, FindEnvironment().TurnTaker });
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            FlipCard(doomsayer);

            Card change = PlayCard("YouCantChange");
            AssertAtLocation(change, bunker.TurnTaker.PlayArea);

            //Check that Doomsayer is indestructible
            SetHitPoints(doomsayer, 1);
            DealDamage(legacy, doomsayer, 5, DamageType.Melee);
            AssertNotGameOver();
            AssertIsInPlay(doomsayer.CharacterCard);

            //Check destruction effect
            Card flak = PlayCard("FlakCannon");
            Card charge = PlayCard("MotivationalCharge");
            UsePower(charge);
            AssertIsInPlay(charge);
            UsePower(flak);
            AssertInTrash(flak);
            UsePower(bunker);
            AssertNotIncapacitatedOrOutOfGame(bunker);
        }

        [Test()]
        public void TestYouHaveNoChoice()
        {
            SetupGameController("VainFacadePlaytest.Doomsayer", "Ra", "Legacy", "Bunker", "TheWraith", "InsulaPrimalis");
            StartGame();

            //Play this card in a hero play area.
            //When that hero’s player would play a card or that hero would use a power, that hero’s player may discard a card.
            //If they do not, play the bottom card of that hero’s deck instead.
            //{Doomsayer} is indestructible.
            ClearInitialCards();
            DecisionSelectTurnTaker = bunker.TurnTaker;
            AssertNextDecisionChoices(new TurnTaker[] { ra.TurnTaker, legacy.TurnTaker, bunker.TurnTaker, wraith.TurnTaker }, new TurnTaker[] { doomsayer.TurnTaker, FindEnvironment().TurnTaker });
            MoveCard(doomsayer, "FutileEfforts", countless.UnderLocation);
            MoveCard(doomsayer, "SeeingRed", countless.UnderLocation);
            MoveCard(doomsayer, "LookingForALoophole", countless.UnderLocation);
            FlipCard(doomsayer);

            Card choice = PlayCard("YouHaveNoChoice");
            AssertAtLocation(choice, bunker.TurnTaker.PlayArea);

            //Check that Doomsayer is indestructible
            SetHitPoints(doomsayer, 1);
            DealDamage(legacy, doomsayer, 5, DamageType.Melee);
            AssertNotGameOver();
            AssertIsInPlay(doomsayer.CharacterCard);

            //Check card effect
            Card flak = PutOnDeck("FlakCannon", true);
            Card ammo = PutInHand("AmmoDrop");
            Card maint = PutInHand("MaintenanceUnit");
            Card grenade = PutOnDeck("GrenadeLauncher");

            //Check that it plays the bottom card instead of the actual play
            DecisionDoNotSelectCard = SelectionType.DiscardCard;
            PlayCard(ammo);
            AssertIsInPlay(flak);
            AssertInHand(ammo);

            //Check that it plays the bottom card instead of using power
            PutOnDeck(bunker,flak, true);
            UsePower(bunker);
            AssertIsInPlay(flak);
            AssertOnTopOfDeck(grenade);

            //Check that discarding a card lets you play the intended card
            PutOnDeck(bunker, flak, true);
            ResetDecisions();
            DecisionSelectCard = maint;
            PlayCard(ammo);
            AssertIsInPlay(ammo);
            AssertOnBottomOfDeck(flak);
            AssertInTrash(maint);

            //Check that discarding a card lets you use a power
            PutInHand(ammo);
            PutInHand(maint);
            UsePower(bunker);
            AssertInHand(grenade);
            AssertInTrash(maint);
            AssertOnBottomOfDeck(flak);

            //Check that putting a card into play does not trigger the card
            PutOnDeck(bunker, grenade);
            PutInHand(ammo);
            PutInHand(maint);
            ResetDecisions();
            DecisionDoNotSelectCard = SelectionType.DiscardCard;
            PlayCard(bunker, ammo, true);
            AssertIsInPlay(ammo);
            AssertInHand(maint);
            AssertOnBottomOfDeck(flak);

            //Check that another hero playing one of the affected hero's cards does not trigger the card
            PutInHand(ammo);
            RunCoroutine(this.GameController.PlayCard(legacy, ammo, false, responsibleTurnTaker: legacy.TurnTaker));
            AssertIsInPlay(ammo);
            AssertInHand(maint);
            AssertOnBottomOfDeck(flak);
        }
    }
}