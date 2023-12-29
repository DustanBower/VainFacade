using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class S399ModifiedAssaultRifleCardController:PeacekeeperCardUtilities
	{
		public S399ModifiedAssaultRifleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //Increase projectile damage dealt by {Peacekeeper} by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && dd.DamageType == DamageType.Projectile, 1);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //{Peacekeeper} deals 1 target 2 projectile damage, then deals 1 target 2 projectile damage.
            int num1 = GetPowerNumeral(0, 1);
            int num2 = GetPowerNumeral(1, 2);
            int num3 = GetPowerNumeral(2, 1);
            int num4 = GetPowerNumeral(3, 2);

            IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), num2, DamageType.Projectile, num1, false, num1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            IEnumerator coroutine2 = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), num4, DamageType.Projectile, num3, false, num3, cardSource: GetCardSource());
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

