using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.BastionCity
{
	public class RitesOfTheBlackThornCardController:BastionCityCardController
	{
		public RitesOfTheBlackThornCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDestroyedByInfernal);
		}

        private string FirstDestroyedByInfernal = "FirstDestroyedByInfernal";

        public override void AddTriggers()
        {
            //Infernal damage is increased by 1 and irreducible.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageType == DamageType.Infernal, 1);
            AddMakeDamageIrreducibleTrigger((DealDamageAction dd) => dd.DamageType == DamageType.Infernal);

            //The first time each turn any target is destroyed by infernal damage, play the top card of the environment deck.
            AddTrigger<DestroyCardAction>((DestroyCardAction dc) => dc.WasCardDestroyed && dc.DealDamageAction != null && dc.DealDamageAction.DamageType == DamageType.Infernal && !IsPropertyTrue(FirstDestroyedByInfernal), DestroyResponse, TriggerType.PlayCard, TriggerTiming.After);
            AddAfterLeavesPlayAction(() => ResetFlagAfterLeavesPlay(FirstDestroyedByInfernal));
        }

        private IEnumerator DestroyResponse(DestroyCardAction dc)
        {
            SetCardPropertyToTrueIfRealAction(FirstDestroyedByInfernal);
            IEnumerator coroutine = PlayTheTopCardOfTheEnvironmentDeckWithMessageResponse(dc);
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

