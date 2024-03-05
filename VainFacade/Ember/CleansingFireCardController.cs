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
    public class CleansingFireCardController : CardController
    {
        public CleansingFireCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
            // If in play: show number of times targets have healed this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => TimesHealedThisTurn() > 0, () => "HP has been regained " + TimesHealedThisTurn().ToString() + " " + TimesHealedThisTurn().ToString_SingularOrPlural("time", "times") + " with " + base.Card.Title + " this turn.", () => "HP has not been regained with " + base.Card.Title + " this turn.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {EmberCharacter} would deal a target fire damage, that target may regain that much HP instead. Reduce HP gained this way by 1 for each time HP has been regained this way this turn, to a minimum of 1."
            AddOptionalPreventDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsSameCard(base.CharacterCard) && dda.DamageType == DamageType.Fire && dda.Amount > 0, GainHPBasedOnDamagePrevented, new TriggerType[] { TriggerType.GainHP }, false);
        }

        public int TimesHealedThisTurn()
        {
            return base.GameController.Game.Journal.GainHPEntriesThisTurn().Where((GainHPJournalEntry g) => g.SourceCard != null && g.SourceCard.Identifier == Identifier).Count();
        }

        // Further code copied from CauterizeCardController from the Cauldron, with modifications to determine the appropriate amount of healing

        public static readonly string Identifier = "CleansingFire";

        private bool? DecisionShouldHeal = null;
        private Card RememberedTarget
        {
            get;
            set;
        }

        private IEnumerator GainHPBasedOnDamagePrevented(DealDamageAction dd)
        {
            // "... that target may regain that much HP instead. Reduce HP gained this way by 1 for each time HP has been regained this way this turn, to a minimum of 1."
            int healAmt = 1;
            if (dd.Amount - TimesHealedThisTurn() > 1)
            {
                healAmt = dd.Amount - TimesHealedThisTurn();
            }
            IEnumerator coroutine = GameController.GainHP(dd.Target, healAmt, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }

        private ITrigger AddOptionalPreventDamageTrigger(Func<DealDamageAction, bool> damageCriteria, Func<DealDamageAction, IEnumerator> followUpResponse = null, IEnumerable<TriggerType> followUpTriggerTypes = null, bool isPreventEffect = false)
        {
            DealDamageAction preventedDamage = null;
            List<CancelAction> cancelDamage = null;
            Func<DealDamageAction, IEnumerator> response = delegate (DealDamageAction dd)
            {
                preventedDamage = dd;
                cancelDamage = new List<CancelAction>();
                IEnumerator enumerator2 = AskAndMaybeCancelAction(dd, showOutput: true, cancelFutureRelatedDecisions: true, cancelDamage, isPreventEffect);
                if (UseUnityCoroutines)
                {
                    return enumerator2;
                }
                GameController.ExhaustCoroutine(enumerator2);
                return DoNothing();
            };
            Func<DealDamageAction, IEnumerator> response2 = delegate (DealDamageAction dd)
            {
                preventedDamage = null;
                cancelDamage = null;
                IEnumerator enumerator = followUpResponse(dd);
                if (UseUnityCoroutines)
                {
                    return enumerator;
                }
                GameController.ExhaustCoroutine(enumerator);
                return DoNothing();
            };
            ITrigger result = AddTrigger((DealDamageAction dd) => damageCriteria(dd) && dd.Amount > 0 && dd.CanDealDamage, response, TriggerType.WouldBeDealtDamage, TriggerTiming.Before, isActionOptional: true);
            if (followUpResponse != null && followUpTriggerTypes != null)
            {
                AddTrigger((DealDamageAction dd) => dd == preventedDamage && !dd.IsSuccessful && cancelDamage != null && cancelDamage.FirstOrDefault() != null && cancelDamage.First().CardSource.Card == Card, response2, followUpTriggerTypes, TriggerTiming.After, null, isConditional: false, requireActionSuccess: false);
            }
            return result;
        }

        private IEnumerator AskAndMaybeCancelAction(DealDamageAction ga, bool showOutput = true, bool cancelFutureRelatedDecisions = true, List<CancelAction> storedResults = null, bool isPreventEffect = false)
        {
            //ask and set 
            if (GameController.PretendMode || ga.Target != RememberedTarget)
            {
                List<YesNoCardDecision> storedYesNo = new List<YesNoCardDecision>();
                IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(this.DecisionMaker,
                    SelectionType.Custom, ga.Target, action: ga, storedResults: storedYesNo,
                    associatedCards: new[] { ga.Target, ga.Target },
                    cardSource: base.GetCardSource());

                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                RememberedTarget = ga.Target;
                if (DidPlayerAnswerYes(storedYesNo))
                {
                    DecisionShouldHeal = true;
                }
                else
                {
                    DecisionShouldHeal = false;
                }
            }
            if (DecisionShouldHeal == true)
            {
                IEnumerator coroutine = base.CancelAction(ga, showOutput: showOutput, cancelFutureRelatedDecisions: cancelFutureRelatedDecisions, storedResults: storedResults, isPreventEffect: isPreventEffect);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }

            if (IsRealAction(ga))
            {
                RememberedTarget = null;
                DecisionShouldHeal = null;
            }
            yield break;
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            string target = "the target";
            string someHitPoints = "1 HP";
            if (decision is YesNoCardDecision yncd && yncd.GameAction is DealDamageAction dd)
            {
                target = dd.Target.Title;
                if (dd.Amount - TimesHealedThisTurn() > 1)
                {
                    someHitPoints = $"{dd.Amount - TimesHealedThisTurn()} HP";
                }
            }
            return new CustomDecisionText($"Do you want {target} to regain {someHitPoints} instead of dealing damage?", $"{decision.DecisionMaker.Name} is deciding whether to cause HP gain instead of damage...", $"Should {target} regain {someHitPoints} instead of taking damage?", "cause HP regain instead of damage");

        }
    }
}
