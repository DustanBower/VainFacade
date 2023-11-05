using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class CollateralBonusCardController:PushCardControllerUtilities
	{
		public CollateralBonusCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //When Push deals damage, destroy an environment card. If a card is destroyed this way, discard a card and the environment deals the same target 2 irreducible projectile damage.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && dd.DidDealDamage, DamageResponse, new TriggerType[3] { TriggerType.DestroyCard, TriggerType.DiscardCard, TriggerType.DealDamage }, TriggerTiming.After);

            //At the start of your turn, destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }

        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            Card target = dd.Target;
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            IEnumerator coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), false, results, this.Card, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDestroyCard(results))
            {
                coroutine = SelectAndDiscardCards(DecisionMaker, 1, false, 1);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (target.IsInPlayAndHasGameText && target.IsTarget)
                {
                    coroutine = base.GameController.DealDamageToTarget(new DamageSource(base.GameController, FindEnvironment().TurnTaker), target, 2, DamageType.Projectile, true, cardSource: GetCardSource());
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
}

