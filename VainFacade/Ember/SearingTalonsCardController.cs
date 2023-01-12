using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Ember
{
    public class SearingTalonsCardController : EmberUtilityCardController
    {
        public SearingTalonsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Blaze cards in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(BlazeCard);
        }

        private readonly string FirstBurnThisTurn = "FirstBurnThisTurn";
        private Card relevant;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn {EmberCharacter} deals each target fire damage, you may reduce the next damage dealt by that target by 1 for each Blaze card in play."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card == base.CharacterCard && dda.DidDealDamage && dda.DamageType == DamageType.Fire && !IsPropertyTrue(GeneratePerTargetKey(FirstBurnThisTurn, dda.Target)), BlazeStunResponse, TriggerType.CreateStatusEffect, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagsAfterLeavesPlay(FirstBurnThisTurn), TriggerType.Hidden);
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            string target = "this target";
            if (relevant != null)
            {
                target = relevant.Title;
            }
            return new CustomDecisionText("Do you want to reduce the next damage dealt by " + target + " by " + NumBlazeCardsInPlay().ToString() + "? ", "deciding whether to reduce the next damage dealt by " + target + " by " + NumBlazeCardsInPlay().ToString(), "Vote for whether to reduce the next damage dealt by " + target + " by " + NumBlazeCardsInPlay().ToString(), "whether to reduce the next damage dealt by " + target + " by " + NumBlazeCardsInPlay().ToString());
        }

        private IEnumerator BlazeStunResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(GeneratePerTargetKey(FirstBurnThisTurn, dda.Target));
            // "... you may reduce the next damage dealt by that target by 1 for each Blaze card in play."
            int blazeCount = NumBlazeCardsInPlay();
            relevant = dda.Target;
            List<YesNoCardDecision> choices = new List<YesNoCardDecision>();
            IEnumerator chooseCoroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.Custom, dda.Target, storedResults: choices, associatedCards: dda.Target.ToEnumerable(), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (DidPlayerAnswerYes(choices))
            {
                ReduceDamageStatusEffect burn = new ReduceDamageStatusEffect(blazeCount);
                burn.SourceCriteria.IsSpecificCard = dda.Target;
                burn.NumberOfUses = 1;
                IEnumerator statusCoroutine = base.GameController.AddStatusEffect(burn, true, GetCardSource());
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

        public override IEnumerator UsePower(int index = 0)
        {
            int meleeAmt = GetPowerNumeral(0, 1);
            int fireAmt = GetPowerNumeral(1, 1);
            // "You may discard a card to play a card."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = SelectAndDiscardCards(DecisionMaker, 1, requiredDecisions: 0, storedResults: discards, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(discards, 1, true))
            {
                IEnumerator playCoroutine = base.GameController.SelectAndPlayCardFromHand(DecisionMaker, true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
            // "Then {EmberCharacter} deals a target 1 melee and 1 fire damage."
            List<DealDamageAction> instances = new List<DealDamageAction>();
            DealDamageAction oneMelee = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, meleeAmt, DamageType.Melee);
            DealDamageAction oneFire = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, fireAmt, DamageType.Fire);
            instances.Add(oneMelee);
            instances.Add(oneFire);
            IEnumerator damageCoroutine = SelectTargetsAndDealMultipleInstancesOfDamage(instances, (Card c) => c.IsTarget, minNumberOfTargets: 1, maxNumberOfTargets: 1);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
        }
    }
}
