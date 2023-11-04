using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class ShockwaveCardController:AnchorCardController
	{
		public ShockwaveCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            base.AddTriggers();

            //At the start of your turn, Push deals each non-hero target 2 projectile damage. Then destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, StartOfTurnResponse, new TriggerType[2] { TriggerType.DealDamage, TriggerType.DestroySelf });
        }

        private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
        {
            IEnumerator coroutine = DealDamage(this.CharacterCard, (Card c) => !IsHeroTarget(c), 2, DamageType.Projectile);
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

