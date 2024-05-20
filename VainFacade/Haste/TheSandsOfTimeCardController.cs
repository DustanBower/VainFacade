using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Haste
{
	public class TheSandsOfTimeCardController:HasteUtilityCardController
	{
		public TheSandsOfTimeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //Increase damage dealt by {Haste} by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.IsSameCard(this.CharacterCard), 1);

            //At the end of your turn, {Haste} may deal himself 2 energy damage. If he takes no damage this way, destroy this card.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse,new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroySelf });
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<DealDamageAction> results = new List<DealDamageAction>();
            IEnumerator coroutine = DealDamage(this.CharacterCard, this.CharacterCard, 2, DamageType.Energy, optional: true, storedResults: results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (!DidDealDamage(results,this.CharacterCard))
            {
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
}

