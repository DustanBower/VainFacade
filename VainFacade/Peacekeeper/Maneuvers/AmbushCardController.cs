using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class AmbushCardController:ManeuverCardController
	{
		public AmbushCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageKey);
		}

        private string FirstDamageKey = "FirstDamageKey";

        public override void AddTriggers()
        {
            //Prevent the first damage dealt to {Peacekeeper} by non-hero targets each turn.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(FirstDamageKey) && dd.Target == this.CharacterCard && dd.DamageSource.IsTarget && !IsHeroTarget(dd.DamageSource.Card), PreventResponse, TriggerType.CancelAction, TriggerTiming.Before);

            //When {Peacekeeper} deals or is dealt damage by another target, destroy this card.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.DidDealDamage && dd.DamageSource.IsTarget && ((dd.DamageSource.Card != this.CharacterCard && dd.Target == this.CharacterCard) || (dd.DamageSource.Card == this.CharacterCard && dd.Target != this.CharacterCard)), DestroyThisCardResponse, TriggerType.DestroySelf, TriggerTiming.After);

            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageKey), TriggerType.Hidden);
        }

        private IEnumerator PreventResponse(DealDamageAction dd)
        {
            Console.WriteLine("Preventing damage");
            SetCardPropertyToTrueIfRealAction(FirstDamageKey);

            IEnumerator coroutine = base.GameController.CancelAction(dd, isPreventEffect: true, cardSource:GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Increase damage {Peacekeeper} deals to non-hero targets by 2 until this card leaves play.
            int num = GetPowerNumeral(0, 2);
            IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(num);
            effect.SourceCriteria.IsSpecificCard = this.CharacterCard;
            effect.TargetCriteria.IsHero = false;
            effect.UntilCardLeavesPlay(this.Card);
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

