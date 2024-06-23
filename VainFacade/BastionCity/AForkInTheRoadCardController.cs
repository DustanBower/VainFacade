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
	public class AForkInTheRoadCardController:BastionCityCardController
	{
		public AForkInTheRoadCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowListOfCardsInPlay(MachinationCriteria());
		}

        public override void AddTriggers()
        {
            //At the end of the environment turn, destroy a machination and this card.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.DestroySelf });

            //When this card is destroyed, discard the top card of the environment deck. If that card is not a target, play the top card of the villain deck. Otherwise a player may play a card.
            AddWhenDestroyedTrigger(DestroyResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PlayCard });
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            IEnumerator coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, MachinationCriteria(), false, responsibleCard: this.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

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

        private IEnumerator DestroyResponse(DestroyCardAction dc)
        {
            List<MoveCardAction> results = new List<MoveCardAction>();
            IEnumerator coroutine = DiscardCardsFromTopOfDeck(this.TurnTakerController, 1, false, results, responsibleTurnTaker: this.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidMoveCard(results))
            {
                Card moved = results.FirstOrDefault().CardToMove;
                if (!moved.IsTarget)
                {
                    coroutine = PlayTheTopCardOfTheVillainDeckWithMessageResponse(null);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }
                else
                {
                    coroutine = SelectHeroToPlayCard(DecisionMaker, true, true);
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

