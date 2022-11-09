using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Node
{
    public class ResourceAllocationCardController : NodeUtilityCardController
    {
        public ResourceAllocationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Connected hero targets
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => IsConnected(c) && c.IsHero && c.IsTarget, "Connected hero targets", false, false, "target", "targets"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a [i]Connected[/i] hero target would deal damage, you may select another [i]Connected[/i] hero target. Increase the damage dealt by the first target by 1. Reduce the next damage dealt by the second target by 1."
            AddTrigger((DealDamageAction dda) => dda.CanDealDamage && dda.Amount > 0 && dda.DamageSource != null && dda.DamageSource.Card != null && IsConnected(dda.DamageSource.Card) && dda.DamageSource.Card.IsHero && dda.DamageSource.Card.IsTarget, IncreaseReduceResponse, new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.IncreaseDamage, TriggerType.CreateStatusEffect }, TriggerTiming.Before);
        }

        private IEnumerator IncreaseReduceResponse(DealDamageAction dda)
        {
            // "... you may select another [i]Connected[/i] hero target."
            Card firstTarget = dda.DamageSource.Card;
            List<SelectCardDecision> choices = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.SelectTargetNoDamage, new LinqCardCriteria((Card c) => IsConnected(c) && c.IsHero && c.IsTarget && c != firstTarget, "Connected hero", false, false, "target other than " + firstTarget.Title, "targets other than " + firstTarget.Title), choices, true, gameAction: dda, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            if (DidSelectCard(choices) && GetSelectedCard(choices) != null)
            {
                Card secondTarget = GetSelectedCard(choices);
                // "Increase the damage dealt by the first target by 1."
                IEnumerator increaseCoroutine = base.GameController.IncreaseDamage(dda, 1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(increaseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(increaseCoroutine);
                }
                // "Reduce the next damage dealt by the second target by 1."
                ReduceDamageStatusEffect spent = new ReduceDamageStatusEffect(1);
                spent.SourceCriteria.IsSpecificCard = secondTarget;
                spent.UntilTargetLeavesPlay(secondTarget);
                spent.NumberOfUses = 1;
                IEnumerator statusCoroutine = base.GameController.AddStatusEffect(spent, true, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
            yield break;
        }
    }
}
