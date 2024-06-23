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
	public class BusinessAsUsualCardController:BastionCityCardController
	{
		public BusinessAsUsualCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNonEnvironmentTargetWithLowestHP();
		}

        public override void AddTriggers()
        {
            //At the end of the environment turn, 1 player may discard a card. If they do, select a target. Reduce the next damage dealt to that target by 2. If they do not, deal the non-environment target with the lowest hp 2 melee damage.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.CreateStatusEffect, TriggerType.DealDamage });
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
            IEnumerator coroutine = base.GameController.SelectHeroToDiscardCard(DecisionMaker, true, storedResultsDiscard: discardResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDiscardCards(discardResults))
            {
                List<SelectCardDecision> results = new List<SelectCardDecision>();
                coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.ReduceNextDamageTaken, FindCardsWhere((Card c) => c.IsTarget && c.IsInPlayAndHasGameText), results, cardSource: GetCardSource());
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
                    ReduceDamageStatusEffect effect = new ReduceDamageStatusEffect(2);
                    effect.NumberOfUses = 1;
                    effect.TargetCriteria.IsSpecificCard = selected;
                    effect.UntilTargetLeavesPlay(selected);
                    coroutine = AddStatusEffect(effect);
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
            else
            {
                coroutine = DealDamageToLowestHP(this.Card, 1, (Card c) => c.IsNonEnvironmentTarget, (Card c) => 2, DamageType.Melee);
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