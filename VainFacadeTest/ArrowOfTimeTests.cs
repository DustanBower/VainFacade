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
    public class ArrowOfTimeTests:BaseTest
	{
        protected TurnTakerController grandpa { get { return FindVillain("Grandfather"); } }

        [Test()]
        public void TestArrowOfTimeIncapNoDeck()
        {
            SetupGameController("VainFacadePlaytest.Grandfather", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //This card is indestructible.
            //The first time each turn each villain card discards a card from each hero's deck, that hero may destroy 1 of their Ongoing or Equipment cards. If no card is destroyed this way, add a token to this card.
            //When a hero deck becomes empty, add a token to this card and put the bottom 5 cards of that deck's trash on top of the deck.
            Card arrow = GetCard("ArrowOfTime");
            MoveAllCards(legacy, legacy.TurnTaker.Deck, legacy.HeroTurnTaker.Hand, false, 0);
            QuickTokenPoolStorage(arrow.TokenPools);
            DestroyCard(legacy);
            QuickTokenPoolCheck(0);
        }

        [Test()]
        public void TestArrowOfTimeIncap()
        {
            SetupGameController("VainFacadePlaytest.Grandfather", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //This card is indestructible.
            //The first time each turn each villain card discards a card from each hero's deck, that hero may destroy 1 of their Ongoing or Equipment cards. If no card is destroyed this way, add a token to this card.
            //When a hero deck becomes empty, add a token to this card and put the bottom 5 cards of that deck's trash on top of the deck.
            Card arrow = GetCard("ArrowOfTime");
            QuickTokenPoolStorage(arrow.TokenPools);
            DestroyCard(legacy);
            QuickTokenPoolCheck(0);
        }

        [Test()]
        public void TestArrowOfTimeDraw()
        {
            SetupGameController("VainFacadePlaytest.Grandfather", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //This card is indestructible.
            //The first time each turn each villain card discards a card from each hero's deck, that hero may destroy 1 of their Ongoing or Equipment cards. If no card is destroyed this way, add a token to this card.
            //When a hero deck becomes empty, add a token to this card and put the bottom 5 cards of that deck's trash on top of the deck.
            Card arrow = GetCard("ArrowOfTime");
            MoveAllCards(legacy, legacy.TurnTaker.Deck, legacy.HeroTurnTaker.Trash, false, 1);
            QuickTokenPoolStorage(arrow.TokenPools);
            DrawCard(legacy);
            QuickTokenPoolCheck(1);
        }

        [Test()]
        public void TestArrowOfTimeMove()
        {
            SetupGameController("VainFacadePlaytest.Grandfather", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //This card is indestructible.
            //The first time each turn each villain card discards a card from each hero's deck, that hero may destroy 1 of their Ongoing or Equipment cards. If no card is destroyed this way, add a token to this card.
            //When a hero deck becomes empty, add a token to this card and put the bottom 5 cards of that deck's trash on top of the deck.
            Card arrow = GetCard("ArrowOfTime");
            MoveAllCards(legacy, legacy.TurnTaker.Deck, legacy.HeroTurnTaker.Trash, false, 1);
            QuickTokenPoolStorage(arrow.TokenPools);
            Card top = GetTopCardOfDeck(legacy);
            MoveCard(legacy, top, legacy.HeroTurnTaker.Hand);
            QuickTokenPoolCheck(1);
        }

        [Test()]
        public void TestArrowOfTimeDiscard()
        {
            SetupGameController("VainFacadePlaytest.Grandfather", "Legacy", "Bunker", "Lifeline", "Megalopolis");
            StartGame();

            //This card is indestructible.
            //The first time each turn each villain card discards a card from each hero's deck, that hero may destroy 1 of their Ongoing or Equipment cards. If no card is destroyed this way, add a token to this card.
            //When a hero deck becomes empty, add a token to this card and put the bottom 5 cards of that deck's trash on top of the deck.
            Card arrow = GetCard("ArrowOfTime");
            MoveAllCards(legacy, legacy.TurnTaker.Deck, legacy.HeroTurnTaker.Hand, false, 1);
            Card ammo = PutOnDeck("AmmoDrop");
            DecisionSelectCard = ammo;
            QuickTokenPoolStorage(arrow.TokenPools);
            PlayCard("LeyLineShift");
            QuickTokenPoolCheck(1);
        }

        [Test()]
        public void TestArrowOfTimeBulkMove()
        {
            SetupGameController("VainFacadePlaytest.Grandfather", "Legacy", "Bunker/TermiNationBunkerCharacter", "TheScholar", "Megalopolis");
            StartGame();

            //This card is indestructible.
            //The first time each turn each villain card discards a card from each hero's deck, that hero may destroy 1 of their Ongoing or Equipment cards. If no card is destroyed this way, add a token to this card.
            //When a hero deck becomes empty, add a token to this card and put the bottom 5 cards of that deck's trash on top of the deck.
            Card arrow = GetCard("ArrowOfTime");
            DestroyCard(bunker);
            DecisionSelectLocation = new LocationChoice(legacy.TurnTaker.Deck);

            QuickTokenPoolStorage(arrow.TokenPools);
            UseIncapacitatedAbility(bunker, 0);
            QuickTokenPoolCheck(1);
        }

        [Test()]
        public void TestArrowOfTimeReveal()
        {
            SetupGameController("VainFacadePlaytest.Grandfather", "Legacy", "Bunker", "TheVisionary/DarkVisionaryCharacter", "Megalopolis");
            StartGame();

            //This card is indestructible.
            //The first time each turn each villain card discards a card from each hero's deck, that hero may destroy 1 of their Ongoing or Equipment cards. If no card is destroyed this way, add a token to this card.
            //When a hero deck becomes empty, add a token to this card and put the bottom 5 cards of that deck's trash on top of the deck.
            Card arrow = GetCard("ArrowOfTime");
            MoveAllCards(legacy, legacy.TurnTaker.Deck, legacy.HeroTurnTaker.Trash, false, 2);
            DecisionSelectLocation = new LocationChoice(legacy.TurnTaker.Deck);

            QuickTokenPoolStorage(arrow.TokenPools);
            UsePower(visionary);
            QuickTokenPoolCheck(1);
        }

        [Test()]
        public void TestArrowOfTimeEmptyTrashDraw()
        {
            SetupGameController("VainFacadePlaytest.Grandfather", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //This card is indestructible.
            //The first time each turn each villain card discards a card from each hero's deck, that hero may destroy 1 of their Ongoing or Equipment cards. If no card is destroyed this way, add a token to this card.
            //When a hero deck becomes empty, add a token to this card and put the bottom 5 cards of that deck's trash on top of the deck.
            Card arrow = GetCard("ArrowOfTime");
            MoveAllCards(legacy, legacy.TurnTaker.Deck, legacy.HeroTurnTaker.Hand, false, 1);
            QuickTokenPoolStorage(arrow.TokenPools);
            DrawCard(legacy);
            QuickTokenPoolCheck(1);
        }

        [Test()]
        public void TestArrowOfTimeEmptyTrashMove()
        {
            SetupGameController("VainFacadePlaytest.Grandfather", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //This card is indestructible.
            //The first time each turn each villain card discards a card from each hero's deck, that hero may destroy 1 of their Ongoing or Equipment cards. If no card is destroyed this way, add a token to this card.
            //When a hero deck becomes empty, add a token to this card and put the bottom 5 cards of that deck's trash on top of the deck.
            Card arrow = GetCard("ArrowOfTime");
            MoveAllCards(legacy, legacy.TurnTaker.Deck, legacy.HeroTurnTaker.Hand, false, 1);
            Card top = GetTopCardOfDeck(legacy);
            QuickTokenPoolStorage(arrow.TokenPools);
            MoveCard(legacy, top, legacy.HeroTurnTaker.Hand);
            QuickTokenPoolCheck(1);
        }

        [Test()]
        public void TestArrowOfTimeEmptyTrashReveal()
        {
            SetupGameController("VainFacadePlaytest.Grandfather", "Legacy", "Bunker", "TheVisionary/DarkVisionaryCharacter", "Megalopolis");
            StartGame();

            //This card is indestructible.
            //The first time each turn each villain card discards a card from each hero's deck, that hero may destroy 1 of their Ongoing or Equipment cards. If no card is destroyed this way, add a token to this card.
            //When a hero deck becomes empty, add a token to this card and put the bottom 5 cards of that deck's trash on top of the deck.
            Card arrow = GetCard("ArrowOfTime");
            MoveAllCards(legacy, legacy.TurnTaker.Deck, legacy.HeroTurnTaker.Hand, false, 2);
            DecisionSelectLocation = new LocationChoice(legacy.TurnTaker.Deck);

            QuickTokenPoolStorage(arrow.TokenPools);
            UsePower(visionary);
            QuickTokenPoolCheck(1);
        }

        [Test()]
        public void TestVillainShowdownNemesis()
        {
            SetupGameController("VainFacadePlaytest.Grandfather", "Legacy", "Bunker", "TheVisionary/DarkVisionaryCharacter", "Megalopolis");
            Card showdown = PutOnDeck("VillainShowdown");
            StartGame();
            //MoveCards(grandpa, FindCardsWhere((Card c) => c.Owner == grandpa.TurnTaker && c.IsInPlay && !c.IsCharacter), grandpa.TurnTaker.Trash, overrideIndestructible: true);
            //DecisionSelectCard = legacy.CharacterCard;
            //Card showdown = PlayCard("VillainShowdown");

            Assert.IsTrue(FindCardController(showdown).NemesisTrigger != null, "Villain Showdown does not have a nemesis trigger");

            QuickHPStorage(legacy.CharacterCard, showdown);
            DealDamage(legacy.CharacterCard, showdown, 1, DamageType.Melee);
            DealDamage(showdown, legacy.CharacterCard, 1, DamageType.Melee);
            QuickHPCheck(-2, -2);

            QuickHPStorage(bunker.CharacterCard, showdown);
            DealDamage(bunker.CharacterCard, showdown, 1, DamageType.Melee);
            DealDamage(showdown, bunker.CharacterCard, 1, DamageType.Melee);
            QuickHPCheck(-1, -1);
        }
    }
}

