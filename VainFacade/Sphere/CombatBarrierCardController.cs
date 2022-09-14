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
    public class CombatBarrierCardController : EmanationCardController
    {
        public CombatBarrierCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {Sphere} is dealt damage, you may discard a card."
            // "When you discard a card for any copy of this card, {Sphere} deals the source of that damage 1 energy damage."
            AddTrigger((DealDamageAction dda) => dda.DidDealDamage && dda.Target == base.CharacterCard, DiscardMassRetaliateResponse, TriggerType.DiscardCard, TriggerTiming.After);
        }

        private IEnumerator DiscardMassRetaliateResponse(DealDamageAction dda)
        {
            // "... you may discard a card."
            List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = SelectAndDiscardCards(base.HeroTurnTakerController, 1, optional: true, requiredDecisions: 0, storedResults: storedResults, gameAction: dda, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "When you discard a card for any copy of this card, {Sphere} deals the source of that damage 1 energy damage."
            if (DidDiscardCards(storedResults))
            {
                int discarded = GetNumberOfCardsDiscarded(storedResults);
                for (int i = 0; i < discarded; i++)
                {
                    // Get the list of copies of this card in play
                    List<Card> barriers = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.Title == base.Card.Title && c.Owner == base.Card.Owner && c.IsInPlayAndHasGameText, " of " + base.Card.Title + " in play", false, false, "copy", "copies"), visibleToCard: GetCardSource()).ToList();
                    foreach (Card copy in barriers)
                    {
                        // Fire this copy's RetaliateResponse for dda
                        CardController copyController = base.GameController.FindCardController(copy);
                        if (copyController is CombatBarrierCardController)
                        {
                            IEnumerator retaliateCoroutine = (copyController as CombatBarrierCardController).RetaliateResponse(dda);
                            if (base.UseUnityCoroutines)
                            {
                                yield return base.GameController.StartCoroutine(retaliateCoroutine);
                            }
                            else
                            {
                                base.GameController.ExhaustCoroutine(retaliateCoroutine);
                            }
                        }
                    }
                }
            }
            yield break;
        }

        private IEnumerator RetaliateResponse(DealDamageAction dda)
        {
            // "... {Sphere} deals the source of that damage 1 energy damage."
            if (dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card.IsTarget)
            {
                Card card = dda.DamageSource.Card;
                IEnumerator damageCoroutine = base.GameController.DealDamageToTarget(new DamageSource(base.GameController, base.CharacterCard), card, 1, DamageType.Energy, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            yield break;
        }
    }
}
