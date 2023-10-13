using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class FreezingVeinsCardController:ArctisCardUtilities
	{
		public FreezingVeinsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        ITrigger trigger;

        public override void AddTriggers()
        {
            //Increase cold damage dealt by {Arctis} by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && dd.DamageType == DamageType.Cold, 1);

            //When {Arctis} deals non-cold damage to a target, he then deals that target 1 cold damage. The type of this damage cannot be changed."
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && dd.DamageType != DamageType.Cold && dd.DidDealDamage, NonColdDamageResponse, TriggerType.DealDamage, TriggerTiming.After);

            //At the end of your turn, {Arctis} deals himself 1 cold damage.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, (PhaseChangeAction pca) => DealDamage(this.CharacterCard, this.CharacterCard, 1, DamageType.Cold, cardSource: GetCardSource()),TriggerType.DealDamage);
        }

        private IEnumerator NonColdDamageResponse(DealDamageAction dd)
        {
            if (dd.Target.IsInPlayAndHasGameText && dd.Target.IsTarget)
            {
                //IEnumerator coroutine = DealDamage(this.CharacterCard, dd.Target, 1, DamageType.Cold, cardSource:GetCardSource());
                //if (base.UseUnityCoroutines)
                //{
                //    yield return base.GameController.StartCoroutine(coroutine);
                //}
                //else
                //{
                //    base.GameController.ExhaustCoroutine(coroutine);
                //}

                //Need to check what this does with Imbued Fire
                //Seems to work fine with Imbued Fire and Twist the Ether
                DealDamageAction action = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, this.CharacterCard), dd.Target, 1, DamageType.Cold);
                trigger = AddTrigger<ChangeDamageTypeAction>((ChangeDamageTypeAction a) => a.DealDamageAction == action, PreventTypeChange, TriggerType.CancelAction, TriggerTiming.Before);
                //trigger = AddChangeDamageTypeTrigger((DealDamageAction dda) => dda == action, DamageType.Cold);
                IEnumerator coroutine = DoAction(action);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                RemoveTrigger(trigger);
            }
        }

        private IEnumerator PreventTypeChange(ChangeDamageTypeAction a)
        {
            IEnumerator coroutine = base.GameController.SendMessageAction($"{this.Card.Title} prevented the damage type from being changed{(a.CardSource != null ? $" by {a.CardSource.Card.Title}" : "")}", Priority.Low, GetCardSource(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = CancelAction(a);
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

