using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class CryogenicBloodCardController:ArctisCardUtilities
	{
		public CryogenicBloodCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //Increase damage dealt by {Arctis} by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard, 1);

            //At the end of your turn, {Arctis} may deal himself up to 2 cold damage. If he is dealt no damage this way, destroy this card.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroySelf });
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<SelectNumberDecision> results = new List<SelectNumberDecision>();
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            IEnumerator coroutine = base.GameController.SelectNumber(DecisionMaker, SelectionType.DealDamage, 1, 2, true, storedResults: results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidSelectNumber(results))
            {
                int num = GetSelectedNumber(results).Value;
                coroutine = DealDamage(this.CharacterCard, this.CharacterCard, num, DamageType.Cold, storedResults: damageResults, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }

            if (!DidDealDamage(damageResults) || !damageResults.Any((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DidDealDamage))
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

