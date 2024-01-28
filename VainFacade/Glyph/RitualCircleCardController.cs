using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class RitualCircleCardController:GlyphCardUtilities
	{
		public RitualCircleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //At the end of your turn, you may put a ritual from your hand under this card.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker && this.HeroTurnTaker.Hand.Cards.Any((Card c) => IsRitual(c)), EndOfTurnResponse, TriggerType.MoveCard);
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            SelectCardDecision decision = new SelectCardDecision(base.GameController, DecisionMaker, SelectionType.MoveUnderThisCard, FindCardsWhere((Card c) => c.Location == this.HeroTurnTaker.Hand && IsRitual(c)), true, cardSource:GetCardSource());
            IEnumerator coroutine = base.GameController.SelectCardAndDoAction(decision, (SelectCardDecision d) => base.GameController.MoveCard(this.TurnTakerController, d.SelectedCard, this.Card.UnderLocation, cardSource: GetCardSource()));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Play any number of rituals from under this card.
            IEnumerator coroutine = base.GameController.SelectCardsFromLocationAndMoveThem(DecisionMaker, this.Card.UnderLocation, 0, 100, new LinqCardCriteria((Card c) => c.Location == this.Card.UnderLocation && base.GameController.DoesCardContainKeyword(c, "ritual", true)), new MoveCardDestination[] { new MoveCardDestination(this.TurnTaker.PlayArea) },responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
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

