using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class IcicleVolleyCardController:IceworkWithAbilityCardController
	{
		public IcicleVolleyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator ActivateIce()
        {
            //{Arctis} deals up to 3 targets 2 projectile each. Destroy this card.
            IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), 2, DamageType.Projectile, 3, false, 0, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = DestroyThisCardResponse(null);
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

