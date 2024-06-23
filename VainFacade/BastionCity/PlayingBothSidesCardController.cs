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
	public class PlayingBothSidesCardController:BastionCityCardController
	{
		public PlayingBothSidesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //When this card enters play, put the top card of each deck under it.
            IEnumerator coroutine = base.GameController.MoveCards(this.TurnTakerController, FindLocationsWhere((Location L) => L.IsDeck && L.HasCards && L.IsRealDeck && base.GameController.IsLocationVisibleToSource(L, GetCardSource())).Select((Location L) => L.TopCard), this.Card.UnderLocation, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override void AddTriggers()
        {
            //At the end of the environment turn, a player may discard 2 cards. If they do not, put the top card of the villain deck under this card.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.MoveCard });

            //At the start of the environment turn, put a random card from under this card into play. If a one-shot or environment card is played this way, destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, StartOfTurnResponse, new TriggerType[] { TriggerType.PutIntoPlay, TriggerType.DestroySelf });
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator coroutine = base.GameController.SelectHeroToDiscardCards(DecisionMaker, 2, 2, true, false, storedResultsDiscard: results, cardSource:GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (!DidDiscardCards(results, 2))
            {
                List<SelectLocationDecision> deckResults = new List<SelectLocationDecision>();
                coroutine = FindVillainDeck(DecisionMaker, SelectionType.MoveCardToUnderCard, deckResults, null);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidSelectLocation(deckResults))
                {
                    Location villainDeck = GetSelectedLocation(deckResults);

                    if (villainDeck.HasCards)
                    {
                        Card top = villainDeck.TopCard;
                        coroutine = base.GameController.MoveCard(this.TurnTakerController, top, this.Card.UnderLocation, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
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
        }

        private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
        {
            if (this.Card.UnderLocation.HasCards)
            {
                Card card = this.Card.UnderLocation.Cards.TakeRandom(1, base.Game.RNG).FirstOrDefault();
                List<PlayCardAction> results = new List<PlayCardAction>();
                IEnumerator message = base.GameController.SendMessageAction($"{card.Title} is put into play.", Priority.Medium, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(message);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(message);
                }

                IEnumerator coroutine = base.GameController.PlayCard(this.TurnTakerController, card, true, storedResults: results, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidPlayCards(results))
                {
                    Card played = results.FirstOrDefault().CardToPlay;
                    if (played.IsEnvironment || played.IsOneShot)
                    {
                        coroutine = DestroyThisCardResponse(null);
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
        }
    }
}

