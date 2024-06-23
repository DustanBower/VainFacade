using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.BastionCity
{
	public class GrimSebastianCardController:BastionCityCardController
	{
		public GrimSebastianCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => this.Card.UnderLocation.Cards.Any((Card cc) => cc.Owner == c.Owner) && CanCardBeConsideredHighestHitPoints(c, (Card cc) => cc.Owner == c.Owner), " with the highest HP from their deck", false, true));
		}

        public override void AddTriggers()
        {
            //At the start of the environment turn, discard a card from under this card. This card deals the non-keeper card with the highest HP from that card's deck {H - 2} melee damage.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, StartOfTurnResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DealDamage });

            //At the end of the environment turn, reveal and discard the top card of each deck. Put the revealed card with the most keywords under this card.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.RevealCard, TriggerType.DiscardCard, TriggerType.MoveCard });
        }

        private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
        {
            List<SelectCardDecision> results = new List<SelectCardDecision>();
            IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.DiscardCard, this.Card.UnderLocation.Cards, results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidSelectCard(results))
            {
                Card selected = GetSelectedCard(results);
                List<MoveCardAction> moveResults = new List<MoveCardAction>();
                coroutine = base.GameController.MoveCard(DecisionMaker, selected, base.GameController.FindCardController(selected).GetTrashDestination().Location, storedResults: moveResults, isDiscard: true, responsibleTurnTaker: this.TurnTaker, cardSource:GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidMoveCard(moveResults))
                {
                    Card moved = moveResults.FirstOrDefault().CardToMove;
                    coroutine = DealDamageToHighestHP(this.Card, 1, (Card c) => c.Owner == moved.Owner && !IsKeeper(c), (Card c) => base.H - 2, DamageType.Melee);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }
            }
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<Card> cards = new List<Card>();
            IEnumerator coroutine = base.GameController.SelectLocationsAndDoAction(DecisionMaker, SelectionType.RevealTopCardOfDeck, (Location L) => L.IsDeck && L.IsRealDeck && L.HasCards && base.GameController.IsLocationVisibleToSource(L, GetCardSource()), (Location L) => RevealResponse(L, cards), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (cards.Count() > 0)
            {
                int max = cards.Select((Card c) => base.GameController.GetAllKeywords(c).Count()).Max();
                List<Card> maxCards = cards.Where((Card c) => base.GameController.GetAllKeywords(c).Count() == max).ToList();

                List<SelectCardDecision> results = new List<SelectCardDecision>();
                coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.Custom, maxCards, results, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidSelectCard(results))
                {
                    Card selected = GetSelectedCard(results);
                    coroutine = base.GameController.MoveCard(DecisionMaker, selected, this.Card.UnderLocation, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }
            }
        }

        private IEnumerator RevealResponse(Location deck, List<Card> cards)
        {
            List<Card> results = new List<Card>();
            IEnumerator coroutine = base.GameController.RevealCards(this.TurnTakerController, deck, 1, results,revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            Card revealed = results.FirstOrDefault();
            if (revealed != null && revealed.Location.IsRevealed)
            {
                coroutine = base.GameController.MoveCard(DecisionMaker, revealed, base.GameController.FindCardController(revealed).GetTrashDestination().Location, isDiscard: true, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                cards.Add(revealed);
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText(
            $"Select a card to move under {this.Card.Title}",
            $"The players are selecting a card to move under {this.Card.Title}.",
            $"Vote for a card to move under {this.Card.Title}.",
            $"a card to move under {this.Card.Title}."
            );
        }
    }
}

