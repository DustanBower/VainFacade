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
	public class BrotherMathisCardController:BastionCityCardController
	{
		public BrotherMathisCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowLowestHP();
		}

        public override void AddTriggers()
        {
            //At the end of the environment turn, this card deals the target with the lowest HP {H - 2} infernal damage.
            //If an environment card is dealt damage this way, this card deals each non-environment target 2 infernal damage.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, TriggerType.DealDamage);
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<DealDamageAction> results = new List<DealDamageAction>();
            IEnumerator coroutine = DealDamageToLowestHP(this.Card, 1, (Card c) => true, (Card c) => base.H - 2, DamageType.Infernal, storedResults: results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDealDamage(results) && results.FirstOrDefault().Target.IsEnvironment)
            {
                coroutine = DealDamage(this.Card, (Card c) => c.IsNonEnvironmentTarget, 2, DamageType.Infernal);
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
}

