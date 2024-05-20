using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Haste
{
	public class QuickReactionsCardController:HasteUtilityCardController
	{
		public QuickReactionsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowTokenPool(SpeedPool);
		}

        public override IEnumerator Play()
        {
            //When this card enters play, add 3 tokens to your speed pool.
            return AddSpeedTokens(3);
        }

        public override void AddTriggers()
        {
            //When a hero ongoing or equipment would be destroyed, you may remove 3 tokens from your speed pool. If you do, put that card on top of its deck. Otherwise, if it is one of your cards, add 2 tokens to your speed pool.
            AddTrigger<DestroyCardAction>((DestroyCardAction dc) => HasteSpeedPoolUtility.GetSpeedPool(this) != null && HasteSpeedPoolUtility.GetSpeedPool(this).CurrentValue > 0 && IsHero(dc.CardToDestroy.Card) && (IsOngoing(dc.CardToDestroy.Card) || IsEquipment(dc.CardToDestroy.Card)), DestroyResponse, new TriggerType[] { TriggerType.ModifyTokens, TriggerType.PutOnDeck, TriggerType.AddTokensToPool, TriggerType.CancelAction }, TriggerTiming.Before);
        }

        public override string DecisionAction => " to put it on top of the deck instead";

        private IEnumerator DestroyResponse(DestroyCardAction dc)
        {
            List<RemoveTokensFromPoolAction> results = new List<RemoveTokensFromPoolAction>();
            IEnumerator coroutine = RemoveSpeedTokens(3, dc, true, results, new Card[] {dc.CardToDestroy.Card});
            //IEnumerator coroutine = base.GameController.RemoveTokensFromPool(SpeedPool, 3, results, true, dc, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidRemoveTokens(results, 3))
            {
                coroutine = CancelAction(dc,showOutput: false);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                coroutine = base.GameController.MoveCard(DecisionMaker, dc.CardToDestroy.Card, GetNativeDeck(dc.CardToDestroy.Card), responsibleTurnTaker: this.TurnTaker, cardSource:GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
            else if (dc.CardToDestroy.Card.Owner == this.TurnTaker)
            {
                coroutine = AddSpeedTokens(2);
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

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decision is YesNoAmountDecision && ((YesNoAmountDecision)decision).Amount.HasValue && decision.AssociatedCards.FirstOrDefault() != null)
            {
                string cardName = decision.AssociatedCards.First().Title;
                string text = $" to put {cardName} on top of the deck instead";
                int amount = ((YesNoAmountDecision)decision).Amount.Value;
                string amountString = amount == 1 ? "a" : amount.ToString();
                string name = decision.DecisionMaker.Name;
                string plural = amount == 1 ? "" : "s";
                return new CustomDecisionText(
                $"Remove {amountString} speed token{plural}{text}?",
                $"{name} is deciding whether to remove {amountString} speed token{plural}{text}",
                $"Vote on whether to remove {amountString} speed token{plural}{text}",
                $"remove {amountString} speed token{plural}{text}"
                );
            }
            return null;
        }
    }
}

