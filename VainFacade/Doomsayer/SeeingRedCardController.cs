using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class SeeingRedCardController:PatternCardController
	{
		public SeeingRedCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowTokenPool(pool());
        }

        private TokenPool pool()
        {
            return this.Card.FindTokenPool("SeeingRedPool");
        }

        public override void AddTriggers()
        {
            //Increase damage dealt to and by hero targets by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => IsHeroTarget(dd.Target), 1);
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsTarget && IsHeroTarget(dd.DamageSource.Card), 1);

            //When a card is destroyed, add a token to this card. Then, if there are {H} or more tokens on this card, put this card under {CountlessWords}.
            AddTrigger<DestroyCardAction>((DestroyCardAction dc) => dc.WasCardDestroyed, DestroyResponse, new TriggerType[2] { TriggerType.AddTokensToPool, TriggerType.MoveCard }, TriggerTiming.After);

            //Reset token pool when this card is destroyed or moved out of play. Based on Magma's Rage
            AddWhenDestroyedTrigger((DestroyCardAction dc) => ResetTokenValue(), TriggerType.Hidden);
            AddTrigger<MoveCardAction>((MoveCardAction mc) => mc.Origin.IsInPlayAndNotUnderCard && !mc.Destination.IsInPlayAndNotUnderCard && mc.CardToMove == base.Card, (MoveCardAction mc) => ResetTokenValue(), TriggerType.ModifyTokens, TriggerTiming.After, ActionDescription.Unspecified, outOfPlayTrigger: true);
        }

        private IEnumerator ResetTokenValue()
        {
            pool().SetToInitialValue();
            yield return null;
        }

        private IEnumerator DestroyResponse(DestroyCardAction dc)
        {
            IEnumerator coroutine = base.GameController.AddTokensToPool(pool(), 1, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (pool().CurrentValue >= base.H)
            {
                coroutine = base.GameController.MoveCard(this.TurnTakerController, this.Card, countlessWords.UnderLocation, cardSource: GetCardSource());
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

