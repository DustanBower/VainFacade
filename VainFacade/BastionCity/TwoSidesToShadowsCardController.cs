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
	public class TwoSidesToShadowsCardController:BastionCityCardController
	{
		public TwoSidesToShadowsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //Reduce damage dealt to keepers and civilians by 1.
            AddReduceDamageTrigger((Card c) => IsKeeper(c) || IsCivilian(c), 1);

            //At the end of the environment turn, destroy an ongoing card. Then play the top card of that card's deck.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.PlayCard });
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            IEnumerator coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsOngoing(c), "ongoing"), false, results, this.Card, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDestroyCard(results))
            {
                Location deck = GetNativeDeck(GetDestroyedCards(results).FirstOrDefault());
                coroutine = base.GameController.PlayTopCardOfLocation(this.TurnTakerController, deck, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
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

