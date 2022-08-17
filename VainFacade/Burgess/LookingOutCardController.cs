using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Burgess
{
    public class LookingOutCardController : CardController
    {
        public LookingOutCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether Burgess has already drawn a card this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDrawThisTurn, base.TurnTaker.Name + " has already drawn a card this turn since " + base.Card.Title + " entered play.", base.TurnTaker.Name + " has not drawn a card this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected readonly string FirstDrawThisTurn = "FirstDrawThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn you draw a card, another player may draw a card."
            AddTrigger((DrawCardAction dca) => !HasBeenSetToTrueThisTurn(FirstDrawThisTurn) && dca.HeroTurnTaker == base.HeroTurnTaker && dca.DidDrawCard, AnotherPlayerDrawsResponse, TriggerType.DrawCard, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstDrawThisTurn), TriggerType.Hidden);
        }

        private IEnumerator AnotherPlayerDrawsResponse(DrawCardAction dca)
        {
            SetCardPropertyToTrueIfRealAction(FirstDrawThisTurn);
            // "... another player may draw a card."
            IEnumerator drawCoroutine = base.GameController.SelectHeroToDrawCard(base.HeroTurnTakerController, optionalDrawCard: true, additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt != base.TurnTaker), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
        }
    }
}
