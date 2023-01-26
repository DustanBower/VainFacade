using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Blitz
{
    public class PulseModulatorCardController : CardController
    {
        public PulseModulatorCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show list of One-Shot cards under this card
            SpecialStringMaker.ShowListOfCardsAtLocation(base.Card.UnderLocation, new LinqCardCriteria((Card c) => c.DoKeywordsContain("one-shot", evenIfUnderCard: true), "One-Shot")).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {BlitzCharacter} is dealt lightning damage, move a card from the top of the villain deck beneath this card for each point of damage dealt this way."
            AddTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && dda.DamageType == DamageType.Lightning && dda.DidDealDamage, StockpileFromDeckResponse, TriggerType.MoveCard, TriggerTiming.After);
            // "At the start of the villain turn, shuffle the cards beneath this one and discard cards from under this card until a One-Shot is discarded. If a One-Shot is discarded this way, put it into play."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardPlayOneShotResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PutIntoPlay });
        }

        private IEnumerator StockpileFromDeckResponse(DealDamageAction dda)
        {
            // "... move a card from the top of the villain deck beneath this card for each point of damage dealt."
            int numToMove = dda.Amount;
            string messageText = base.Card.Title + " moves the top " + numToMove.ToString() + " cards of the villain deck under itself.";
            if (numToMove == 1)
            {
                messageText = base.Card.Title + " moves the top card of the villain deck under itself.";
            }
            if (!base.TurnTaker.Deck.HasCards)
            {
                numToMove = 0;
                messageText = "There are no cards in the villain deck for " + base.Card.Title + " to move.";
            }
            else if (base.TurnTaker.Deck.NumberOfCards < dda.Amount)
            {
                numToMove = base.TurnTaker.Deck.NumberOfCards;
                if (numToMove == 1)
                {
                    messageText = "There is only " + base.TurnTaker.Deck.NumberOfCards.ToString() + " card in the villain deck, so " + base.Card.Title + " moves it under itself.";
                }
                else
                {
                    messageText = "There are only " + base.TurnTaker.Deck.NumberOfCards.ToString() + " cards in the villain deck, so " + base.Card.Title + " moves all of them under itself.";
                }
            }
            List<Card> cardsToMove = new List<Card>();
            if (numToMove > 0)
            {
                cardsToMove = base.TurnTaker.Deck.GetTopCards(numToMove).ToList();
            }
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(messageText, Priority.Medium, cardSource: GetCardSource(), associatedCards: cardsToMove, showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            IEnumerator moveCoroutine = base.GameController.MoveCards(base.TurnTakerController, cardsToMove, base.Card.UnderLocation, toBottom: true, playIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
        }

        private IEnumerator DiscardPlayOneShotResponse(PhaseChangeAction pca)
        {
            // "... shuffle the cards beneath this one..."
            IEnumerator shuffleCoroutine = base.GameController.ShuffleLocation(base.Card.UnderLocation, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }
            // "... and discard cards from under this card until a One-Shot is discarded."
            Card oneShotDiscarded = null;
            List<MoveCardAction> results = new List<MoveCardAction>();
            while (base.Card.UnderLocation.HasCards && oneShotDiscarded == null)
            {
                IEnumerator discardCoroutine = base.GameController.DiscardTopCard(base.Card.UnderLocation, results, (Card c) => true, base.TurnTaker, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
                MoveCardAction firstRelevant = results.Where((MoveCardAction mca) => mca.WasCardMoved && mca.CardToMove.IsOneShot).FirstOrDefault();
                if (firstRelevant != null)
                {
                    oneShotDiscarded = firstRelevant.CardToMove;
                }
            }
            // "If a One-Shot is discarded this way, put it into play."
            IEnumerator oneShotCoroutine = base.GameController.SendMessageAction("None of the discarded cards were One-Shots.", Priority.Medium, GetCardSource(), showCardSource: true);
            if (oneShotDiscarded != null)
            {
                oneShotCoroutine = base.GameController.PlayCard(base.TurnTakerController, oneShotDiscarded, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, associateCardSource: true, cardSource: GetCardSource());
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(oneShotCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(oneShotCoroutine);
            }
        }
    }
}
