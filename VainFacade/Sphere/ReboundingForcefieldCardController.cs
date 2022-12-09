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
    public class ReboundingForcefieldCardController : EmanationCardController
    {
        public ReboundingForcefieldCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override bool CanOrderAffectOutcome(GameAction action)
        {
            if (action is DealDamageAction)
            {
                return (action as DealDamageAction).Target == base.CharacterCard;
            }
            return false;
        }

        private int X()
        {
            return 2 * base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.Title == base.Card.Title && c.Owner == base.Card.Owner), visibleToCard: GetCardSource()).Count();
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {Sphere} would be dealt damage, if that damage is less than or equal to X, you may discard a card to redirect that damage to the non-hero target with the lowest HP."
            AddTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && dda.Amount <= X(), DiscardToRedirectResponse, TriggerType.RedirectDamage, TriggerTiming.Before);
        }

        private IEnumerator DiscardToRedirectResponse(DealDamageAction dda)
        {
            // "... you may discard a card..."
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = base.GameController.SelectAndDiscardCard(base.HeroTurnTakerController, optional: true, storedResults: results, dealDamageInfo: dda.ToEnumerable(), responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(results))
            {
                // "... to redirect that damage to the non-hero target with the lowest HP."
                List<Card> targets = new List<Card>();
                IEnumerator findCoroutine = base.GameController.FindTargetWithLowestHitPoints(1, (Card c) => !c.IsHero, targets, dealDamageInfo: dda.ToEnumerable(), evenIfCannotDealDamage: true, optional: false, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(findCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(findCoroutine);
                }
                Card target = targets.FirstOrDefault();
                if (target != null)
                {
                    IEnumerator redirectCoroutine = base.GameController.RedirectDamage(dda, target, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(redirectCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(redirectCoroutine);
                    }
                }
            }
        }
    }
}
