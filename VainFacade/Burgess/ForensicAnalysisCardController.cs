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
    public class ForensicAnalysisCardController : CardController
    {
        public ForensicAnalysisCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether any villain targets have dealt damage this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstVillainDamageThisTurn, "A villain target has already dealt damage this turn since " + base.Card.Title + " entered play.", "No villain targets have dealt damage this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected readonly string FirstVillainDamageThisTurn = "FirstVillainDamageThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn a villain target deals damage, you may draw a card."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(FirstVillainDamageThisTurn) && dda.DamageSource != null && dda.DamageSource.Card.IsVillainTarget && dda.DidDealDamage, DrawResponse, TriggerType.DrawCard, TriggerTiming.After);
        }

        private IEnumerator DrawResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(FirstVillainDamageThisTurn);
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
