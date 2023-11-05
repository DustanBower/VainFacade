using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class PhaseAnalyzerCardController:PushCardControllerUtilities
	{
		public PhaseAnalyzerCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstTimeDraw);
		}

        private string FirstTimeDraw = "FirstTimeDraw";

        public override void AddTriggers()
        {
            //The first time each turn that you draw a card, you may draw a second card or discard a card.
            AddTrigger<DrawCardAction>((DrawCardAction dc) => dc.HeroTurnTaker == this.HeroTurnTaker && dc.DidDrawCard && !IsPropertyTrue(FirstTimeDraw), DrawResponse, new TriggerType[2] { TriggerType.DrawCard, TriggerType.DiscardCard }, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstTimeDraw), TriggerType.Hidden);
        }

        public IEnumerator DrawResponse(DrawCardAction dc)
        {
            SetCardProperty(FirstTimeDraw, true);
            IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(base.HeroTurnTakerController, "Draw a card", SelectionType.DrawCard, () => DrawCard()),
                new Function(base.HeroTurnTakerController, "Discard a card", SelectionType.DiscardCard, () => SelectAndDiscardCards(DecisionMaker, 1, false, 0), this.HeroTurnTaker.Hand.HasCards)
                };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, true, null, null, null, GetCardSource());
            IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(choose);
            }
            else
            {
                base.GameController.ExhaustCoroutine(choose);
            }
        }
    }
}

