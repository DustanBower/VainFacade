using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Node
{
    public class HealthyConnectionsCardController : NodeUtilityCardController
    {
        public HealthyConnectionsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Connected hero targets
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => IsConnected(c) && IsHeroTarget(c), "Connected hero", false, false, "target", "targets"));
            // If in play: show whether this card has healed anyone this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(HealedThisTurn, "A Connected hero target has already regained HP this turn since " + base.Card.Title + " entered play.", "No Connected hero targets have regained HP this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        private readonly string HealedThisTurn = "FirstTimeHealedThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn any [i]Connected[/i] hero target gains HP, another [i]Connected[/i] hero target gains 1 HP."
            AddTrigger((GainHPAction gha) => !HasBeenSetToTrueThisTurn(HealedThisTurn) && IsHero(gha.HpGainer) && IsConnected(gha.HpGainer), HealOtherConnectedTargetResponse, TriggerType.GainHP, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(HealedThisTurn), TriggerType.Hidden);
        }

        private IEnumerator HealOtherConnectedTargetResponse(GainHPAction gha)
        {
            SetCardPropertyToTrueIfRealAction(HealedThisTurn);
            // "... another [i]Connected[/i] hero target gains 1 HP."
            Card healed = gha.HpGainer;
            IEnumerator healCoroutine = base.GameController.SelectAndGainHP(DecisionMaker, 1, additionalCriteria: (Card c) => IsConnected(c) && IsHeroTarget(c) && c != healed, requiredDecisions: 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
        }
    }
}
