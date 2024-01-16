using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class GlyphTurnTakerController:HeroTurnTakerController
	{
		public GlyphTurnTakerController(TurnTaker turnTaker, GameController gameController): base(turnTaker, gameController)
		{
		}

        private Location FateDeck;

        private Location InsightDeck;

        public Location GetFateDeck
        {
            get
            {
                if (FateDeck == null)
                {
                    FateDeck = base.TurnTaker.FindSubDeck("GlyphFateDeck");
                }
                return FateDeck;
            }
        }

        public Location GetInsightDeck
        {
            get
            {
                if (InsightDeck == null)
                {
                    InsightDeck = base.TurnTaker.FindSubDeck("GlyphInsightDeck");
                }
                return InsightDeck;
            }
        }

        //public override IEnumerator StartGame()
        //{
        //    //Move all of Glyph's cards to the main deck
        //    IEnumerator coroutine = base.GameController.BulkMoveCards(this, FindCardsWhere((Card c) => (c.Location.IsSubDeck || c.Location.IsHand) && c.Location.OwnerTurnTaker == this.TurnTaker), this.TurnTaker.Deck, cardSource: new CardSource(this.CharacterCardController));
        //    if (base.UseUnityCoroutines)
        //    {
        //        yield return base.GameController.StartCoroutine(coroutine);
        //    }
        //    else
        //    {
        //        base.GameController.ExhaustCoroutine(coroutine);
        //    }

        //    coroutine = base.GameController.ShuffleLocation(this.TurnTaker.Deck, cardSource: new CardSource(this.CharacterCardController));
        //    if (base.UseUnityCoroutines)
        //    {
        //        yield return base.GameController.StartCoroutine(coroutine);
        //    }
        //    else
        //    {
        //        base.GameController.ExhaustCoroutine(coroutine);
        //    }

        //    coroutine = base.GameController.BulkMoveCards(this, this.TurnTaker.Deck.GetTopCards(4), this.HeroTurnTaker.Hand, cardSource: new CardSource(this.CharacterCardController));
        //    if (base.UseUnityCoroutines)
        //    {
        //        yield return base.GameController.StartCoroutine(coroutine);
        //    }
        //    else
        //    {
        //        base.GameController.ExhaustCoroutine(coroutine);
        //    }
        //}

        public void HandleSubDecks()
        {
            List<Location> subdecks = TurnTaker.SubDecks;
            List<Card> cards = FindCardsWhere((Card c) => (c.Location.IsSubDeck || c.Location.IsHand) && c.Location.OwnerTurnTaker == this.TurnTaker).ToList();
            foreach (Location L in subdecks)
            {
                this.TurnTaker.MoveAllCards(L, this.TurnTaker.Deck);
            }

            this.TurnTaker.MoveAllCards(this.HeroTurnTaker.Hand, this.TurnTaker.Deck);
            this.TurnTaker.Deck.ShuffleCards();
            this.TurnTaker.MoveCard(this.TurnTaker.Deck.TopCard,this.HeroTurnTaker.Hand);
            this.TurnTaker.MoveCard(this.TurnTaker.Deck.TopCard, this.HeroTurnTaker.Hand);
            this.TurnTaker.MoveCard(this.TurnTaker.Deck.TopCard, this.HeroTurnTaker.Hand);
            this.TurnTaker.MoveCard(this.TurnTaker.Deck.TopCard, this.HeroTurnTaker.Hand);
            this.TurnTaker.Deck.ShuffleCards();

            foreach (Card item in this.TurnTaker.Deck.Cards)
            {
                item.SetIsPositionKnown(false);
            }
        }
    }
}

