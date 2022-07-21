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
    public class AlienHeartCardController : CardController
    {
        public AlienHeartCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show whether Sphere has been prevented from playing a card this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(FirstPreventedPlayThisTurn), () => base.TurnTaker.Name + " has already been prevented from playing a card this turn.", () => base.TurnTaker.Name + " has not been prevented from playing a card this turn.");
            // Show whether Sphere has been prevented from drawing a card this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(FirstPreventedDrawThisTurn), () => base.TurnTaker.Name + " has already been prevented from drawing a card this turn.", () => base.TurnTaker.Name + " has not been prevented from drawing a card this turn.");
            // Show whether Sphere has been prevented from using a power this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(FirstPreventedPowerThisTurn), () => base.TurnTaker.Name + " has already been prevented from using a power this turn.", () => base.TurnTaker.Name + " has not been prevented from using a power this turn.");
        }

        protected const string FirstPreventedPlayThisTurn = "FirstPreventedPlayThisTurn";
        protected const string FirstPreventedDrawThisTurn = "FirstPreventedDrawThisTurn";
        protected const string FirstPreventedPowerThisTurn = "FirstPreventedPowerThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn a non-hero card prevents you from playing a card, you may use a power."
            AddTrigger((CancelAction ca) => ca.CardSource != null && !ca.CardSource.Card.IsHero && ca.ActionToCancel is PlayCardAction && (ca.ActionToCancel as PlayCardAction).ResponsibleTurnTaker == base.TurnTaker && !HasBeenSetToTrueThisTurn(FirstPreventedPlayThisTurn), PreventedPlayResponse, TriggerType.UsePower, TriggerTiming.After);
            AddTrigger((CancelAction ca) => ca.CardSource != null && !ca.CardSource.Card.IsHero && ca.ActionToCancel is MakeDecisionAction && (ca.ActionToCancel as MakeDecisionAction).DecisionMaker == base.HeroTurnTakerController && (ca.ActionToCancel as MakeDecisionAction).Decision.SelectionType == SelectionType.PlayCard && !HasBeenSetToTrueThisTurn(FirstPreventedPlayThisTurn), PreventedPlayResponse, TriggerType.UsePower, TriggerTiming.After);
            // ...
            // "The first time each turn a non-hero card prevents you from drawing a card, you may play a card."
            AddTrigger((CancelAction ca) => ca.CardSource != null && !ca.CardSource.Card.IsHero && ca.ActionToCancel is DrawCardAction && (ca.ActionToCancel as DrawCardAction).HeroTurnTaker == base.HeroTurnTaker && !HasBeenSetToTrueThisTurn(FirstPreventedDrawThisTurn), PreventedDrawResponse, TriggerType.PlayCard, TriggerTiming.After);
            // ...
            // "The first time each turn a non-hero card prevents you from using a power, you may draw a card."
            AddTrigger((CancelAction ca) => ca.CardSource != null && !ca.CardSource.Card.IsHero && ca.ActionToCancel is UsePowerAction && (ca.ActionToCancel as UsePowerAction).HeroUsingPower == base.HeroTurnTakerController && !HasBeenSetToTrueThisTurn(FirstPreventedPowerThisTurn), PreventedPowerResponse, TriggerType.DrawCard, TriggerTiming.After);
            AddTrigger((CancelAction ca) => ca.CardSource != null && !ca.CardSource.Card.IsHero && ca.ActionToCancel is MakeDecisionAction && (ca.ActionToCancel as MakeDecisionAction).DecisionMaker == base.HeroTurnTakerController && (ca.ActionToCancel as MakeDecisionAction).Decision.SelectionType == SelectionType.UsePower && !HasBeenSetToTrueThisTurn(FirstPreventedPowerThisTurn), PreventedPowerResponse, TriggerType.DrawCard, TriggerTiming.After);
            // ...
            AddAfterLeavesPlayAction(() => ResetFlagAfterLeavesPlay(FirstPreventedPlayThisTurn));
            AddAfterLeavesPlayAction(() => ResetFlagAfterLeavesPlay(FirstPreventedDrawThisTurn));
            AddAfterLeavesPlayAction(() => ResetFlagAfterLeavesPlay(FirstPreventedPowerThisTurn));
        }

        private IEnumerator PreventedPlayResponse(GameAction ga)
        {
            // "... you may use a power."
            if (!HasBeenSetToTrueThisTurn(FirstPreventedPlayThisTurn))
            {
                SetCardPropertyToTrueIfRealAction(FirstPreventedPlayThisTurn);
                IEnumerator powerCoroutine = base.GameController.SelectAndUsePower(base.HeroTurnTakerController, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(powerCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(powerCoroutine);
                }
            }
            yield break;
        }

        private IEnumerator PreventedDrawResponse(GameAction ga)
        {
            // "... you may play a card."
            if (!HasBeenSetToTrueThisTurn(FirstPreventedDrawThisTurn))
            {
                SetCardPropertyToTrueIfRealAction(FirstPreventedDrawThisTurn);
                IEnumerator playCoroutine = base.GameController.SelectAndPlayCardFromHand(base.HeroTurnTakerController, true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
            yield break;
        }

        private IEnumerator PreventedPowerResponse(GameAction ga)
        {
            // "... you may draw a card."
            if (!HasBeenSetToTrueThisTurn(FirstPreventedPowerThisTurn))
            {
                SetCardPropertyToTrueIfRealAction(FirstPreventedPowerThisTurn);
                IEnumerator drawCoroutine = base.GameController.DrawCard(base.HeroTurnTaker, optional: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawCoroutine);
                }
            }
            yield break;
        }
    }
}
