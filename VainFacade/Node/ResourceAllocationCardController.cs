﻿using Handelabra;
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
            AllowFastCoroutinesDuringPretend = false;
            RunModifyDamageAmountSimulationForThisCard = false;
            // Show list of Connected hero targets
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => IsConnected(c) && IsHeroTarget(c), "Connected hero", true, false, "target", "targets"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a [i]Connected[/i] hero target would deal damage, you may select another [i]Connected[/i] hero target. Increase the damage dealt by the first target by 1. Reduce the next damage dealt by the second target by 1."
            AddTrigger((DealDamageAction dda) => dda.CanDealDamage && dda.Amount > 0 && dda.DamageSource != null && dda.DamageSource.Card != null && IsConnected(dda.DamageSource.Card) && IsHeroTarget(dda.DamageSource.Card) && !dda.IsPretend, IncreaseReduceResponse, new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.IncreaseDamage, TriggerType.CreateStatusEffect }, TriggerTiming.Before);
        }

        private IEnumerator IncreaseReduceResponse(DealDamageAction dda)
        {
            /*Log.Debug("ResourceAllocationCardController.IncreaseReduceResponse: dda: " + dda.ToString());
            Log.Debug("ResourceAllocationCardController.IncreaseReduceResponse: dda.CanDealDamage: " + dda.CanDealDamage.ToString());
            Log.Debug("ResourceAllocationCardController.IncreaseReduceResponse: dda.Amount: " + dda.Amount.ToString());
            Log.Debug("ResourceAllocationCardController.IncreaseReduceResponse: dda.DamageSource == null: " + (dda.DamageSource == null).ToString());
            Log.Debug("ResourceAllocationCardController.IncreaseReduceResponse: dda.DamageSource.Card == null: " + (dda.DamageSource.Card == null).ToString());
            Log.Debug("ResourceAllocationCardController.IncreaseReduceResponse: IsConnected(dda.DamageSource.Card): " + IsConnected(dda.DamageSource.Card).ToString());
            Log.Debug("ResourceAllocationCardController.IncreaseReduceResponse: dda.DamageSource.Card.IsHero: " + dda.DamageSource.Card.IsHero.ToString());
            Log.Debug("ResourceAllocationCardController.IncreaseReduceResponse: dda.DamageSource.Card.IsTarget: " + dda.DamageSource.Card.IsTarget.ToString());
            Log.Debug("ResourceAllocationCardController.IncreaseReduceResponse: dda.Target: " + dda.Target.Title);
            Log.Debug("ResourceAllocationCardController.IncreaseReduceResponse: dda.IsPretend: " + dda.IsPretend.ToString());*/

            //Log.Debug("ResourceAllocationCardController.IncreaseReduceResponse:");
            // "... you may select another [i]Connected[/i] hero target."
            Card firstTarget = dda.DamageSource.Card;
            List<SelectCardDecision> choices = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.Custom, new LinqCardCriteria((Card c) => IsConnected(c) && IsHeroTarget(c) && c != firstTarget, "Connected hero", false, false, "target other than " + firstTarget.Title, "targets other than " + firstTarget.Title), choices, true, cardSource: GetCardSource());
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
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            string choosing = "a target to reduce the next damage they deal";
            return new CustomDecisionText("Select " + choosing, base.TurnTaker.Name + " is selecting " + choosing, "Vote for " + choosing, choosing);
        }
    }
}
