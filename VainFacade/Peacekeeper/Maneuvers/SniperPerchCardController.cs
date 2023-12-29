using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class SniperPerchCardController:ManeuverCardController
	{
		public SniperPerchCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //Reduce damage dealt to {Peacekeeper} by 2.
            AddReduceDamageTrigger((Card c) => c == this.CharacterCard, 2);

            //When {Peacekeeper} is dealt damage by another target or you use a power on another card, destroy this card.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DamageSource.IsTarget && dd.DamageSource.Card != this.CharacterCard && dd.DidDealDamage, DestroyThisCardResponse, TriggerType.DestroySelf, TriggerTiming.After);
            AddTrigger<UsePowerAction>((UsePowerAction up) => up.HeroUsingPower == this.HeroTurnTakerController && up.Power.CardSource.Card != this.Card, DestroyThisCardResponse, TriggerType.DestroySelf, TriggerTiming.After);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Deal 1 target 3 irreducible projectile damage.
            int num1 = GetPowerNumeral(0, 1);
            int num2 = GetPowerNumeral(0, 3);
            IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), num2, DamageType.Projectile, num1, false, num1, true, cardSource: GetCardSource());
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

