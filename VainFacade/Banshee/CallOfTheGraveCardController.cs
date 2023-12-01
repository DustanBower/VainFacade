using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class CallOfTheGraveCardController:CardController
	{
		public CallOfTheGraveCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //Discard any number of cards.
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator coroutine = SelectAndDiscardCards(DecisionMaker, null, false, 0, results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //{Banshee} deals herself 1 irreducible infernal damage for each card discarded this way.
            if (DidDiscardCards(results))
            {
                int num1 = GetNumberOfCardsDiscarded(results);
                List<DealDamageAction> damageResults = new List<DealDamageAction>();

                for (int i = 0; i < num1; i++)
                {
                    coroutine = DealDamage(this.CharacterCard, this.CharacterCard, 1, DamageType.Infernal, true, storedResults: damageResults, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }

                //coroutine = DealDamage(this.CharacterCard, this.CharacterCard, num1, DamageType.Infernal, true, storedResults: damageResults, cardSource: GetCardSource());
                //if (base.UseUnityCoroutines)
                //{
                //    yield return base.GameController.StartCoroutine(coroutine);
                //}
                //else
                //{
                //    base.GameController.ExhaustCoroutine(coroutine);
                //}

                //{Banshee} deals a target X irreducible infernal damage, where X = 2 times the damage she took this way.
                int num2 = 0;
                if (DidDealDamage(damageResults))
                {
                    num2 = damageResults.Select((DealDamageAction dd) => (dd.Target == this.CharacterCard ? dd.Amount : 0)).Sum();
                }

                coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), num2 * 2, DamageType.Infernal, 1, false, 1, true, cardSource: GetCardSource());
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

