using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class CallousDisregardCardController : CardController
    {
        public CallousDisregardCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of active heroes other than Sphere in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsHeroCharacterCard && c.IsTarget && c != base.CharacterCard, " other than " + base.CharacterCard.Title + " in play", false, false, "active hero", "active heroes"));
        }

        public override IEnumerator Play()
        {
            // "{Sphere} is immune to psychic damage this turn."
            ImmuneToDamageStatusEffect disregard = new ImmuneToDamageStatusEffect();
            disregard.TargetCriteria.IsSpecificCard = base.CharacterCard;
            disregard.DamageTypeCriteria.AddType(DamageType.Psychic);
            disregard.UntilTargetLeavesPlay(base.CharacterCard);
            disregard.UntilThisTurnIsOver(base.Game);
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(disregard, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
            // "{Sphere} deals each hero 1 psychic damage."
            List<DealDamageAction> results = new List<DealDamageAction>();
            IEnumerator damageCoroutine = base.GameController.DealDamage(base.HeroTurnTakerController, base.CharacterCard, (Card c) => c.IsHeroCharacterCard, 1, DamageType.Psychic, storedResults: results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "You may destroy an environment card for each point of damage dealt this way."
            if (DidDealDamage(results))
            {
                int totalDamage = 0;
                foreach(DealDamageAction dda in results)
                {
                    if (dda.DidDealDamage)
                    {
                        totalDamage += dda.Amount;
                    }
                }
                if (totalDamage > 0)
                {
                    IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), totalDamage, optional: false, requiredDecisions: 0, responsibleCard: base.Card, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(destroyCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(destroyCoroutine);
                    }
                }
            }
        }
    }
}
