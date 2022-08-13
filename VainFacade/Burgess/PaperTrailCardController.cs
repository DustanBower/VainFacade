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
    public class PaperTrailCardController : CardController
    {
        public PaperTrailCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether a villain card has already entered play this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstVillainCardThisTurn, "A villain card has already entered play this turn since " + base.Card.Title + " entered play.", "No villain cards have entered play this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected readonly string FirstVillainCardThisTurn = "FirstVillainCardThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time a villain card enters play each turn, you may draw a card."
            AddTrigger((CardEntersPlayAction cepa) => !HasBeenSetToTrueThisTurn(FirstVillainCardThisTurn) && cepa.CardEnteringPlay.IsVillain, DrawResponse, TriggerType.DrawCard, TriggerTiming.After);
        }

        private IEnumerator DrawResponse(CardEntersPlayAction cepa)
        {
            SetCardPropertyToTrueIfRealAction(FirstVillainCardThisTurn);
            // "... you may draw a card."
            IEnumerator drawCoroutine = DrawCard(base.HeroTurnTaker, optional: true);
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
