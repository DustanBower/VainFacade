using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace VainFacadePlaytest.Arctis
{
	public class DefensiveCombatCardController:ArctisCardUtilities
	{
		public DefensiveCombatCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
		}

        private Guid? PreventAndHeal { get; set; }
        private Guid? DoNotPreventAndHeal { get; set; }
        private int? AmountToHeal;

        public override void AddTriggers()
        {
            //When {Arctis} would be dealt cold damage, you may have an icework regain that much hp instead.
            //When {Arctis} would be dealt non-cold damage, you may increase that damage by 1 and redirect it to an icework.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DamageType != DamageType.Cold && dd.Amount > 0 && FindCardsWhere((Card c) => IsIcework(c) && c.IsInPlayAndHasGameText).Any(), NonColdDamageResponse, new TriggerType[] { TriggerType.RedirectDamage, TriggerType.IncreaseDamage}, TriggerTiming.Before);
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DamageType == DamageType.Cold && dd.Amount > 0 && FindCardsWhere((Card c) => IsIcework(c) && c.IsInPlayAndHasGameText).Any(), ColdDamageResponse, TriggerType.WouldBeDealtDamage, TriggerTiming.Before);
        }

        private IEnumerator ColdDamageResponse(DealDamageAction dd)
        {
            //...you may have an icework regain that much hp instead.
            //based on Planquez Vous
            if (!DoNotPreventAndHeal.HasValue || DoNotPreventAndHeal != dd.InstanceIdentifier)
            {
                if (!PreventAndHeal.HasValue || PreventAndHeal.Value != dd.InstanceIdentifier)
                {
                    List<YesNoCardDecision> storedResults = new List<YesNoCardDecision>();
                    IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.Custom, base.Card, dd, storedResults, null, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    if (DidPlayerAnswerYes(storedResults))
                    {
                        PreventAndHeal = dd.InstanceIdentifier;
                        AmountToHeal = dd.Amount;
                    }
                    else
                    {
                        DoNotPreventAndHeal = dd.InstanceIdentifier;
                    }
                }
                if (PreventAndHeal.HasValue && PreventAndHeal.Value == dd.InstanceIdentifier)
                {
                    IEnumerator coroutine2 = CancelAction(dd, showOutput: true, cancelFutureRelatedDecisions: true, null, isPreventEffect: true);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine2);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine2);
                    }
                    if (IsRealAction(dd) && AmountToHeal.HasValue)
                    {
                        GameController gameController = base.GameController;
                        HeroTurnTakerController decisionMaker = DecisionMaker;
                        Card card = base.Card;
                        CardSource cardSource = GetCardSource();
                        coroutine2 = gameController.SelectAndGainHP(DecisionMaker, AmountToHeal.Value, false, IsIcework, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine2);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine2);
                        }
                    }
                }
            }
            if (IsRealAction(dd))
            {
                PreventAndHeal = null;
                AmountToHeal = null;
                DoNotPreventAndHeal = null;
            }
        }

        private IEnumerator NonColdDamageResponse(DealDamageAction dd)
        {
            //...you may increase that damage by 1 and redirect it to an icework.
            List<YesNoCardDecision> results2 = new List<YesNoCardDecision>();
            IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.Custom, this.Card, dd, results2, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //based on Smoke Bombs
            if (DidPlayerAnswerYes(results2))
            {
                IEnumerator redirect = base.GameController.SelectTargetAndRedirectDamage(DecisionMaker, (Card c) => IsIcework(c), dd, false, cardSource: GetCardSource());
                IEnumerator increase = base.GameController.IncreaseDamage(dd, 1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(redirect);
                    yield return base.GameController.StartCoroutine(increase);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(redirect);
                    base.GameController.ExhaustCoroutine(increase);
                }
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decision is YesNoCardDecision && ((YesNoCardDecision)decision).GameAction is DealDamageAction)
            {
                string text = "";
                if (((DealDamageAction)((YesNoCardDecision)decision).GameAction).DamageType == DamageType.Cold)
                {
                    int amount = ((DealDamageAction)((YesNoCardDecision)decision).GameAction).Amount;
                    text = $"prevent this damage and have an icework regain {amount} HP";
                }
                else
                {
                    text = "increase this damage by 1 and redirect it to an icework";
                }
                return new CustomDecisionText(
                $"Do you want to {text}?",
                $"{decision.DecisionMaker.Name} is deciding whether to {text}.",
                $"Vote for whether to {text}.",
                $"whether to {text}."
                );
            }
            return null;
        }
    }
}

