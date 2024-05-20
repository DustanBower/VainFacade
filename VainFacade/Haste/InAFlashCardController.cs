using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Haste
{
	public class InAFlashCardController:HasteUtilityCardController
	{
		public InAFlashCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //Add 2 tokens to your speed pool.
            IEnumerator coroutine = AddSpeedTokens(2);
            //IEnumerator coroutine = base.GameController.AddTokensToPool(SpeedPool, 2, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //You may draw a card.
            coroutine = DrawCard(this.HeroTurnTaker, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Destroy any number of hero ongoing or equipment cards. For each card destroyed this way, you may destroy an environment or ongoing card.
            coroutine = base.GameController.SelectCardsAndDoAction(DecisionMaker, new LinqCardCriteria((Card c) => IsHero(c) && (IsOngoing(c) || IsEquipment(c)) && c.IsInPlayAndHasGameText, "hero ongoing or equipment"), SelectionType.DestroyCard, DestroyResponse, requiredDecisions: 0, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator DestroyResponse(Card selected)
        {
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            IEnumerator coroutine = base.GameController.DestroyCard(DecisionMaker, selected, false, results, responsibleCard: this.Card, cardSource: GetCardSource());
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
                coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && (IsOngoing(c) || c.IsEnvironment), "environment or ongoing"), true, responsibleCard: this.Card, cardSource: GetCardSource());
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

