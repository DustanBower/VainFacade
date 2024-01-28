using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class ABrushWithDeathCardController:GlyphLimitedCard
	{
		public ABrushWithDeathCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //At the end of your turn, if you did not play a card, did not use a power, or did not draw a card this turn, or {Glyph} has fewer than 10 hp, you may draw a card, play a card, or use a power.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker && (this.CharacterCard.HitPoints < 10 || GetNumberOfCardsPlayedThisTurn(this.TurnTakerController) == 0 || GetNumberOfPowersUsedThisTurn(this.HeroTurnTakerController) == 0 || GetNumberOfCardsDrawnThisTurn(this.HeroTurnTaker) == 0), EndOfTurnResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.PlayCard, TriggerType.UsePower });
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            IEnumerable<Function> functionChoices = new Function[3]
                {
                new Function(base.HeroTurnTakerController, "Draw a card", SelectionType.DrawCard, () => DrawCard(), base.GameController.CanDrawCards(DecisionMaker, GetCardSource())),
                new Function(base.HeroTurnTakerController, "Play a card", SelectionType.PlayCard,() => SelectAndPlayCardFromHand(DecisionMaker), base.GameController.CanPlayCards(this.TurnTakerController, GetCardSource()) && this.HeroTurnTaker.HasCardsInHand),
                new Function(base.HeroTurnTakerController, "Use a power", SelectionType.UsePower,() => base.GameController.SelectAndUsePower(DecisionMaker, cardSource:GetCardSource()), base.GameController.CanUsePowers(DecisionMaker,GetCardSource()))
                };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, true, null, $"{this.TurnTaker.Name} cannot draw cards, {(this.HeroTurnTaker.HasCardsInHand ? "cannot play cards" : "has no cards in hand")}, and cannot use powers.", null, GetCardSource());
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

        private int GetNumberOfCardsDrawnThisTurn(HeroTurnTaker htt)
        {
            return (from p in Journal.DrawCardEntriesThisTurn()
                    where p.Hero == htt
                    select p).Count();
        }
    }
}

