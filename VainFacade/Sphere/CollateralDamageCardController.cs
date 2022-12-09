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
    public class CollateralDamageCardController : CardController
    {
        public CollateralDamageCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of environment cards in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"));
        }

        public override IEnumerator Play()
        {
            // "Destroy all environment cards."
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = base.GameController.DestroyCards(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), storedResults: results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "{Sphere} deals himself X projectile damage, then another target X times 2 projectile damage, where X is the number of environment cards destroyed in this way."
            int x = GetNumberOfCardsDestroyed(results);
            IEnumerator selfDamageCoroutine = base.GameController.DealDamageToSelf(base.HeroTurnTakerController, (Card c) => c == base.CharacterCard, x, DamageType.Projectile, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selfDamageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selfDamageCoroutine);
            }
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 2 * x, DamageType.Projectile, 1, false, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
        }
    }
}
