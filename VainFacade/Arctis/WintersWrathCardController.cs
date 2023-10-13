using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class WintersWrathCardController:ArctisCardUtilities
	{
		public WintersWrathCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
		}

        public override void AddTriggers()
        {
            //When an icework would be dealt damage that would reduce its hp below 1, destroy it instead.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => IsIcework(dd.Target) && dd.Amount >= dd.Target.HitPoints, DestroyIceworkResponse, new TriggerType[3] { TriggerType.WouldBeDealtDamage, TriggerType.CancelAction, TriggerType.DestroyCard }, TriggerTiming.Before);

            //When an icework is destroyed, {Arctis} deals 1 target X cold damage, where X = that card's hp before it was destroyed.
            AddTrigger<DestroyCardAction>((DestroyCardAction dc) => dc.WasCardDestroyed && IsIcework(dc.CardToDestroy.Card) && dc.HitPointsOfCardBeforeItWasDestroyed.HasValue, (DestroyCardAction dc) => base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), dc.HitPointsOfCardBeforeItWasDestroyed.Value, DamageType.Cold, 1, false, 1, cardSource: GetCardSource()), TriggerType.DealDamage, TriggerTiming.After);
        }

        private IEnumerator DestroyIceworkResponse(DealDamageAction dd)
        {
            Card icework = dd.Target;
            IEnumerator coroutine = CancelAction(dd);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (!dd.IsPretend)
            {
                coroutine = base.GameController.DestroyCard(DecisionMaker, icework, cardSource: GetCardSource());
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

