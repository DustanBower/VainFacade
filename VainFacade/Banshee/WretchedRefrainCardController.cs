using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class WretchedRefrainCardController:DirgeCardController
	{
		public WretchedRefrainCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
            RunModifyDamageAmountSimulationForThisCard = false;
        }

        private DealDamageAction DealDamageAction
        {
            get;
            set;
        }

        private bool? SelectFunctionDecision
        {
            get;
            set;
        }

        private Card Target
        {
            get;
            set;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();

            //When that target would be dealt damage, you may increase that damage by 1 and change the damage type to a type of your choice.
            AddTrigger(new ChangeDamageTypeTrigger(base.GameController, (DealDamageAction dd) => IsThisCardNextToCard(dd.Target), base.SelectDamageTypeForDealDamageAction, new TriggerType[1]
            {
                TriggerType.ChangeDamageType
            }, null, GetCardSource()));
            AddTrigger<DealDamageAction>((DealDamageAction dealDamage) => IsThisCardNextToCard(dealDamage.Target), ModifyDamageAmountResponse, TriggerType.ModifyDamageAmount, TriggerTiming.Before);
        }

        private IEnumerator ModifyDamageAmountResponse(DealDamageAction dealDamage)
        {
            DealDamageAction = dealDamage;
            if (base.GameController.PretendMode || dealDamage.Target != Target)
            {
                List<YesNoCardDecision> yesno = new List<YesNoCardDecision>();
                IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.Custom, this.Card, dealDamage, yesno, new Card[] { dealDamage.Target }, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                if (DidPlayerAnswerYes(yesno))
                {
                    SelectFunctionDecision = true;
                }
                Target = dealDamage.Target;
            }
            else if (SelectFunctionDecision.HasValue && SelectFunctionDecision.Value)
            {
                IEnumerator coroutine2 = IncreaseFunction();
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine2);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine2);
                }
            }
            if (!base.GameController.PretendMode)
            {
                SelectFunctionDecision = null;
                DealDamageAction = null;
            }
        }

        private IEnumerator IncreaseFunction()
        {
            IEnumerator coroutine = base.GameController.IncreaseDamage(DealDamageAction, 1, isNemesisEffect: false, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decision is YesNoCardDecision)
            {
                string text = $"increase this damage by 1";
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

