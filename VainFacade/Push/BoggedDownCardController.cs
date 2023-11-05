using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class BoggedDownCardController:PushCardControllerUtilities
	{
		public BoggedDownCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //When Push deals a target damage, reduce the next damage dealt by that target by 2.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && dd.DidDealDamage && dd.Target.IsInPlayAndHasGameText, ReduceDamageResponse, TriggerType.ReduceDamageOneUse, TriggerTiming.After);

            //At the start of your turn, destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }

        private IEnumerator ReduceDamageResponse(DealDamageAction dd)
        {
            ReduceDamageStatusEffect effect = new ReduceDamageStatusEffect(2);
            effect.SourceCriteria.IsSpecificCard = dd.Target;
            effect.UntilTargetLeavesPlay(dd.Target);
            effect.NumberOfUses = 1;
            IEnumerator coroutine = AddStatusEffect(effect);
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

