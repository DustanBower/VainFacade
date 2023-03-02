using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class LastCallCardController : CardController
    {
        public LastCallCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of environment cards in play other than this one
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsEnvironment && c != base.Card, "other than " + base.Card.Title, false, true, "environment card", "environment cards"));
        }

        private readonly string TokenPoolIdentifier = "LastCallPool";

        private TokenPool LastCallTokenPool()
        {
            return base.Card.FindTokenPool(TokenPoolIdentifier);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When this card is destroyed, destroy all environment cards, then deal each target X fire damage and X projectile damage, where X = 1 plus the number of cards destroyed this way."
            AddWhenDestroyedTrigger(ExplodeResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.DealDamage });
            // "When this card has no tokens on it, destroy it."
            AddTrigger((ModifyTokensAction mta) => LastCallTokenPool().CurrentValue == 0 && !base.Card.IsBeingDestroyed, DestroyThisCardResponse, TriggerType.DestroySelf, TriggerTiming.After);
            // "At the end of the environment turn, remove a token from this card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => base.GameController.RemoveTokensFromPool(LastCallTokenPool(), 1, cardSource: GetCardSource()), TriggerType.ModifyTokens);
            // When destroyed: reset number of tokens
            AddWhenDestroyedTrigger(ResetTokensResponse, TriggerType.Hidden);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, put 3 tokens on it."
            IEnumerator addCoroutine = base.GameController.AddTokensToPool(LastCallTokenPool(), 3, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(addCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(addCoroutine);
            }
        }

        private IEnumerator ExplodeResponse(DestroyCardAction dca)
        {
            // "... destroy all environment cards, ..."
            List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = base.GameController.DestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), storedResults: destroyResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "... then deal each target X fire damage and X projectile damage, where X = 1 plus the number of cards destroyed this way."
            int x = 1 + GetNumberOfCardsDestroyed(destroyResults);
            List<DealDamageAction> instances = new List<DealDamageAction>();
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, x, DamageType.Fire));
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, x, DamageType.Projectile));
            IEnumerator damageCoroutine = DealMultipleInstancesOfDamage(instances, (Card c) => true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
        }

        private IEnumerator ResetTokensResponse(GameAction ga)
        {
            LastCallTokenPool().SetToInitialValue();
            yield return null;
        }
    }
}
