using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class GlacialShieldCardController:IceworkWithAbilityCardController
	{
		public GlacialShieldCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator ActivateIce()
        {
            //Until the start of your next turn, when a target other than {Arctis} would deal damage to one of your targets, reduce that damage by 1.
            ReduceDamageStatusEffect effect = new GlacialShieldStatusEffect(1);
            effect.UntilStartOfNextTurn(this.TurnTaker);
            effect.SourceCriteria.IsTarget = true;
            effect.SourceCriteria.IsNotSpecificCard = this.CharacterCard;
            effect.TargetCriteria.OwnedBy = this.TurnTaker;
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

