using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class IceAxeCardController:IceworkWithAbilityCardController
	{
		public IceAxeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator ActivateIce()
        {
            //{Arctis} deals 1 target 3 melee damage.
            IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), 3, DamageType.Melee, 1, false, 1, cardSource: GetCardSource());
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

