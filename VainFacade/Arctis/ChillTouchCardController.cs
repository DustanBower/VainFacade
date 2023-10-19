using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class ChillTouchCardController:ArctisCardUtilities
	{
		public ChillTouchCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //{Arctis} deals 1 target 1 melee and 1 cold damage.
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            List<DealDamageAction> damage = new List<DealDamageAction>();
            damage.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, this.CharacterCard), null, 1, DamageType.Melee));
            damage.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, this.CharacterCard), null, 1, DamageType.Cold));
            IEnumerator coroutine = SelectTargetAndDealMultipleInstancesOfDamageEx(damage, (Card c) => c.IsTarget, damageResults, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            IEnumerable<Card> targets = damageResults.Where((DealDamageAction dd) => dd.DidDealDamage && dd.DamageType == DamageType.Cold && dd.Target.IsInPlayAndHasGameText).Select((DealDamageAction dd) => dd.Target).Distinct();
            //Destroy an ongoing or reduce damage dealt by a target dealt cold damage this way by 2 until the start of your next turn.
            string reduceMessage = "";
            if (targets.Count() > 0)
            {
                reduceMessage = $"Reduce damage dealt by {(targets.Count() > 1 ? "a target dealt cold damage this way" : targets.FirstOrDefault().Title)} by 2 until the start of your next turn";
            }
            else
            {
                reduceMessage = "Reduce damage to no effect";
            }
            IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(base.HeroTurnTakerController, "Destroy an ongoing", SelectionType.DestroyCard, () => base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsOngoing(c)), false, cardSource: GetCardSource()), FindCardsWhere((Card c) => IsOngoing(c) && c.IsInPlayAndHasGameText).Any()),
                new Function(base.HeroTurnTakerController, reduceMessage, SelectionType.ReduceDamageDealt, () => AddReduceStatus(targets),null, "There are no ongoings in play")
                };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, false, null, null, null, GetCardSource());
            IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(choose);
            }
            else
            {
                base.GameController.ExhaustCoroutine(choose);
            }
        }

        private IEnumerator AddReduceStatus(IEnumerable<Card> targets)
        {
            List<SelectCardDecision> results = new List<SelectCardDecision>();
            IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.ReduceDamageDealt, new LinqCardCriteria((Card c) => targets.Contains(c) && c.IsInPlayAndHasGameText,"dealt cold damage this way", false, true), results, false, cardSource: GetCardSource());
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
                Card target = GetSelectedCard(results);
                ReduceDamageStatusEffect effect = new ReduceDamageStatusEffect(2);
                effect.UntilStartOfNextTurn(this.TurnTaker);
                effect.SourceCriteria.IsSpecificCard = target;
                effect.UntilTargetLeavesPlay(target);
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
    }
}

