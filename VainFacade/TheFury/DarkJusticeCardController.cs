using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheFury
{
    public class DarkJusticeCardController : TheFuryUtilityCardController
    {
        public DarkJusticeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {TheFuryCharacter} deals herself damage, select a target. Increase the next damage dealt to that target by 1."
            AddTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card == base.CharacterCard && dda.DidDealDamage, (DealDamageAction dda) => SelectTargetAndIncreaseNextDamageTo(new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText, "targets in play", false), 1, false, GetCardSource()), TriggerType.CreateStatusEffect, TriggerTiming.After);
            // "When a source other than {TheFuryCharacter} deals {TheFuryCharacter} damage, you may increase the next damage dealt to {TheFuryCharacter} and the source of that damage by 1 for every 2 points of damage dealt this way."
            AddTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && (dda.DamageSource == null || !dda.DamageSource.IsCard || dda.DamageSource.Card != base.CharacterCard), IncreaseNextDamageByXOver2Response, TriggerType.CreateStatusEffect, TriggerTiming.After);
        }

        protected bool sourceIsTarget;

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decision is YesNoAmountDecision yn)
            {
                int dAmt = yn.Amount.Value;
                if (sourceIsTarget)
                {
                    Card source = yn.AssociatedCards.FirstOrDefault();
                    string sourceTitle = source.Title;
                    return new CustomDecisionText("Do you want to increase the next damage dealt to " + base.CharacterCard.Title + " and to " + sourceTitle + " by " + dAmt.ToString() + "?", "choosing whether to increase the next damage dealt to " + base.CharacterCard.Title + " and to " + sourceTitle, "Vote for whether to increase the next damage dealt to " + base.CharacterCard.Title + " and to " + sourceTitle + " by " + dAmt.ToString(), "increase damage dealt to " + base.CharacterCard.Title + " and to the source of damage");
                }
                else
                {
                    return new CustomDecisionText("Do you want to increase the next damage dealt to " + base.CharacterCard.Title + " by " + dAmt.ToString() + "?", "choosing whether to increase the next damage dealt to " + base.CharacterCard.Title, "Vote for whether to increase the next damage dealt to " + base.CharacterCard.Title + " by " + dAmt.ToString(), "increase damage dealt to " + base.CharacterCard.Title);
                }
            }
            return base.GetCustomDecisionText(decision);
        }

        private IEnumerator IncreaseNextDamageByXOver2Response(DealDamageAction dda)
        {
            // "... you may increase the next damage dealt to {TheFuryCharacter} and the source of that damage by 1 for every 2 points of damage dealt this way."
            int increaseAmt = dda.Amount / 2;
            sourceIsTarget = false;
            YesNoAmountDecision choice = new YesNoAmountDecision(base.GameController, DecisionMaker, SelectionType.Custom, increaseAmt, cardSource: GetCardSource());
            Card source = null;
            if (dda.DamageSource != null)
            {
                if (dda.DamageSource.IsTarget)
                {
                    sourceIsTarget = true;
                    source = dda.DamageSource.Card;
                    choice = new YesNoAmountDecision(base.GameController, DecisionMaker, SelectionType.Custom, increaseAmt, associatedCards: source.ToEnumerable(), cardSource: GetCardSource());
                }
            }
            IEnumerator chooseCoroutine = base.GameController.MakeDecisionAction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (DidPlayerAnswerYes(choice))
            {
                // Increase next damage dealt to the Fury
                IEnumerator increaseFuryCoroutine = IncreaseNextDamageTo(base.CharacterCard, increaseAmt, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(increaseFuryCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(increaseFuryCoroutine);
                }
                if (sourceIsTarget)
                {
                    IEnumerator increaseSourceCoroutine = IncreaseNextDamageTo(dda.DamageSource.Card, increaseAmt, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(increaseSourceCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(increaseSourceCoroutine);
                    }
                }
            }
        }
    }
}
