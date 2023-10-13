using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace VainFacadePlaytest.Arctis
{
	public class ColdFrontCardController:ArctisCardUtilities
	{
		public ColdFrontCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
		}
        private ITrigger _modifyDamageAmount;
        private ITrigger trigger;
        private DealDamageAction DamageAction;
        private Card Target;
        private SelectFunctionDecision SelectFunctionDecision;

        public override void AddTriggers()
        {
            //When cold damage would be dealt to a target, you may reduce that damage by 1 or 2 to put an icework into play from your hand.
            //If Defensive Combat and Crystal Armor are in play, this prompts first to reduce the damage, then asks whether Crystal Armor or Defensive Combat should take effect first.
            //After choosing a card, if you select not to redirect or not to prvent, it prompts to reduce damage again
            //This seems wrong, but Twist the Ether does it as well in the same scenario, so it's not a problem with this card.
            _modifyDamageAmount = AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.DamageType == DamageType.Cold, ModifyDamageAmountResponse, TriggerType.ReduceDamage , TriggerTiming.Before);
        }

        //private IEnumerator DamageResponse(DealDamageAction dda)
        //{
        //    IEnumerable<Function> functionChoices = new Function[2]
        //        {
        //        new Function(base.HeroTurnTakerController, "Reduce this damage by 1 and put an icework into play", SelectionType.ReduceDamageDealt, () => ReduceResponse(dda, 1)),
        //        new Function(base.HeroTurnTakerController, "Reduce this damage by 2 and put an icework into play", SelectionType.ReduceDamageDealt, () => ReduceResponse(dda, 2))
        //        };

        //    SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, true, dda, null, null, GetCardSource());
        //    List<SelectFunctionDecision> storedResults = new List<SelectFunctionDecision>();
        //    IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction, storedResults);
        //    if (base.UseUnityCoroutines)
        //    {
        //        yield return base.GameController.StartCoroutine(choose);
        //    }
        //    else
        //    {
        //        base.GameController.ExhaustCoroutine(choose);
        //    }

        //    if (DidSelectFunction(storedResults, DecisionMaker))
        //    {
        //        //int num = GetSelectedNumber(storedResults).Value;
        //        //coroutine = base.GameController.ReduceDamage(dda, num, null, GetCardSource());
        //        //if (base.UseUnityCoroutines)
        //        //{
        //        //    yield return base.GameController.StartCoroutine(coroutine);
        //        //}
        //        //else
        //        //{
        //        //    base.GameController.ExhaustCoroutine(coroutine);
        //        //}
        //        Console.WriteLine("Creating trigger for Cold Front");
        //        trigger = AddTrigger<DealDamageAction>((DealDamageAction dd) => true, PlayIcework, TriggerType.PutIntoPlay, TriggerTiming.After);
        //    }
        //}

        private IEnumerator ModifyDamageAmountResponse(DealDamageAction dealDamage)
        {
            DamageAction = dealDamage;
            if (base.GameController.PretendMode || dealDamage.Target != Target)
            {
                IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(base.HeroTurnTakerController, "Reduce by 1 and put an icework into play", SelectionType.ReduceDamageTaken, ReduceBy1),
                new Function(base.HeroTurnTakerController, "Reduce by 2 and put an icework into play", SelectionType.ReduceDamageTaken, ReduceBy2)
                };
                List<SelectFunctionDecision> selectFunction = new List<SelectFunctionDecision>();
                SelectFunctionDecision selectFunction2 = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, optional: true, dealDamage, null, null, GetCardSource());
                IEnumerator coroutine = base.GameController.SelectAndPerformFunction(selectFunction2, selectFunction);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                if (Enumerable.Count<SelectFunctionDecision>((IEnumerable<SelectFunctionDecision>)selectFunction) > 0)
                {
                    SelectFunctionDecision = Enumerable.FirstOrDefault<SelectFunctionDecision>((IEnumerable<SelectFunctionDecision>)selectFunction);
                }
                Target = dealDamage.Target;
            }
            else if (SelectFunctionDecision.SelectedFunction != null)
            {
                IEnumerator coroutine2 = SelectFunctionDecision.SelectedFunction.FunctionToExecute.Invoke();
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
                if (SelectFunctionDecision.SelectedFunction != null)
                {
                    Console.WriteLine("Creating trigger for Cold Front");
                    trigger = AddTrigger<DealDamageAction>((DealDamageAction dd) => dd == dealDamage, PlayIcework, TriggerType.PutIntoPlay, TriggerTiming.After, requireActionSuccess: false);
                }
                SelectFunctionDecision = null;
                DamageAction = null;
            }
        }

        private IEnumerator ReduceBy1()
        {
            IEnumerator coroutine = base.GameController.ReduceDamage(DamageAction, 1, _modifyDamageAmount, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator ReduceBy2()
        {
            IEnumerator coroutine = base.GameController.ReduceDamage(DamageAction, 2, _modifyDamageAmount, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        //private IEnumerator ReduceResponse(DealDamageAction dda, int num)
        //{
        //    Console.WriteLine("Reducing damage: Cold Front");
        //    IEnumerator coroutine = base.GameController.ReduceDamage(dda, num, null, GetCardSource());
        //    if (base.UseUnityCoroutines)
        //    {
        //        yield return base.GameController.StartCoroutine(coroutine);
        //    }
        //    else
        //    {
        //        base.GameController.ExhaustCoroutine(coroutine);
        //    }

        //    //trigger = AddTrigger<DealDamageAction>((DealDamageAction dd) => dd == dda, PlayIcework, TriggerType.PutIntoPlay, TriggerTiming.After);
        //}

        private IEnumerator PlayIcework(DealDamageAction dd)
        {
            //Console.WriteLine("Playing icework from Cold Front");
            RemoveTrigger(trigger);
            IEnumerator coroutine = base.GameController.SelectAndPlayCardFromHand(DecisionMaker, false, null, new LinqCardCriteria((Card c) => IsIcework(c), "", false, false, "icework", "iceworks"), true, cardSource: GetCardSource());
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

