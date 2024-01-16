using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class MilektithasDarkSummonsCardController:GlyphCardUtilities
	{
		public MilektithasDarkSummonsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowSpecialString(() => ShowLocationsOfFaceDownCards(IsFaceDownMight,"face-down might cards", true));
            base.SpecialStringMaker.ShowSpecialString(() => ShowLocationsOfFaceDownCards(IsFaceDownFate, "face-down fate cards", true));
        }

        

        public override IEnumerator Play()
        {
            //You may destroy one of your face-down might cards.
            List<DestroyCardAction> destroyMight = new List<DestroyCardAction>();
            List<Location> locationMight = new List<Location>();
            IEnumerator coroutine = SelectAndDestroyCardAndReturnLocation(DecisionMaker, new LinqCardCriteria(IsFaceDownMight, "face-down might"), true, destroyMight, locationMight, CustomDecisionMode.Might, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDestroyCard(destroyMight) && locationMight.FirstOrDefault() != null && locationMight.FirstOrDefault().IsPlayArea)
            {
                //If you do, Glyph deals each target in that card's play area 2 infernal damage.
                Location loc = locationMight.FirstOrDefault();
                coroutine = DealDamage(this.CharacterCard, (Card c) => c.Location.HighestRecursiveLocation == loc, 2, DamageType.Infernal);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }



            //Destroy any number of your face-down destruction cards.
            //Use fate for pilot deck
            List<Card> damagedCards = new List<Card>();

            List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
            List<Location> locationResults = new List<Location>();
            coroutine = SelectAndDestroyCardAndReturnLocation(DecisionMaker, new LinqCardCriteria(IsFaceDownFate, "face-down fate"), true, destroyResults, locationResults, CustomDecisionMode.Fate, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            while (locationResults.FirstOrDefault() != null)
            {
                if (DidDestroyCard(destroyResults) && locationResults.FirstOrDefault() != null && locationResults.FirstOrDefault().IsPlayArea)
                {
                    //For each card destroyed this way, Glyph deals a target in that play area that has not been dealt damage this way 5 cold damage.
                    Location loc = locationResults.FirstOrDefault();
                    List<DealDamageAction> damageResults = new List<DealDamageAction>();
                    coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), 5, DamageType.Cold, 1, false, 1, additionalCriteria: (Card c) => c.Location.HighestRecursiveLocation == loc && !damagedCards.Contains(c), storedResultsDamage: damageResults, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    if (DidDealDamage(damageResults))
                    {
                        damagedCards.Add(damageResults.FirstOrDefault().Target);
                    }
                }

                destroyResults = new List<DestroyCardAction>();
                locationResults = new List<Location>();
                coroutine = SelectAndDestroyCardAndReturnLocation(DecisionMaker, new LinqCardCriteria(IsFaceDownFate, "face-down fate"), true, destroyResults, locationResults, CustomDecisionMode.Fate, cardSource: GetCardSource());
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

