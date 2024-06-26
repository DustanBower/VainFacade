using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.BastionCity
{
	public class DarkTitheCardController:BastionCityCardController
	{
		public DarkTitheCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //At the start of the environment turn, discard the top {H - 2} cards of the environment deck. If {MrGoodman} is discarded this way, put him into play.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, StartOfTurnResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PutIntoPlay });

            //At the end of the environment turn, a player may discard a card. If a card is discarded this way, then 1 player may play a card.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PlayCard });
        }

        private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
        {
            List<MoveCardAction> results = new List<MoveCardAction>();
            IEnumerator coroutine = DiscardCardsFromTopOfDeck(this.TurnTakerController, base.H - 2, false, results, false, this.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidMoveCard(results) && results.Any((MoveCardAction mc) => mc.CardToMove.Identifier == "MrGoodman"))
            {
                Card goodman = FindCard("MrGoodman");
                coroutine = base.GameController.PlayCard(this.TurnTakerController, goodman, true, cardSource: GetCardSource());
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

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator coroutine = base.GameController.SelectHeroToDiscardCard(DecisionMaker, true, storedResultsDiscard: results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDiscardCards(results))
            {
                coroutine = base.GameController.SelectHeroToPlayCard(DecisionMaker, cardSource: GetCardSource());
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

