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
	public class CleanStreetsDirtyHandsCardController:BastionCityCardController
	{
		public CleanStreetsDirtyHandsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //At the end of the environment turn, discard the top {H - 1} cards of the environment deck. When a non-machination card is discarded this way with a keyword matching a card in play, put it into play.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PutIntoPlay });
        }

        //private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        //{
        //    List<MoveCardAction> results = new List<MoveCardAction>();
        //    IEnumerator coroutine = DiscardCardsFromTopOfDeck(this.TurnTakerController, base.H - 1, storedResults: results, responsibleTurnTaker: this.TurnTaker);
        //    if (base.UseUnityCoroutines)
        //    {
        //        yield return base.GameController.StartCoroutine(coroutine);
        //    }
        //    else
        //    {
        //        base.GameController.ExhaustCoroutine(coroutine);
        //    }

        //    if (DidMoveCard(results))
        //    {
        //        List<Card> cards = results.Where((MoveCardAction mc) => mc.WasCardMoved).Select((MoveCardAction mc) => mc.CardToMove).ToList();
        //        List<Card> cardsToPlay = cards.Where((Card c) => !IsMachination(c) && FindCardsWhere((Card cc) => cc.IsInPlayAndHasGameText && base.GameController.GetAllKeywords(cc).Any((string s) => base.GameController.DoesCardContainKeyword(c, s))).Any()).ToList();
        //        coroutine = base.GameController.PlayCards(DecisionMaker, (Card c) => cardsToPlay.Contains(c), false, true, cardSource: GetCardSource());
        //        if (base.UseUnityCoroutines)
        //        {
        //            yield return base.GameController.StartCoroutine(coroutine);
        //        }
        //        else
        //        {
        //            base.GameController.ExhaustCoroutine(coroutine);
        //        }
        //    }
        //}

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            IEnumerator coroutine;
            for (int i = 0; i < base.H - 1; i++)
            {
                List<MoveCardAction> results = new List<MoveCardAction>();
                coroutine = DiscardCardsFromTopOfDeck(this.TurnTakerController, 1, storedResults: results, responsibleTurnTaker: this.TurnTaker);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidMoveCard(results))
                {
                    Card moved = results.FirstOrDefault().CardToMove;
                    if (moved.Location == this.TurnTaker.Trash && !IsMachination(moved) && FindCardsWhere((Card cc) => cc.IsInPlayAndHasGameText && base.GameController.GetAllKeywords(cc).Any((string s) => base.GameController.DoesCardContainKeyword(moved, s))).Any())
                    {
                        coroutine = base.GameController.PlayCard(this.TurnTakerController, moved, true, cardSource: GetCardSource());
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
    }
}

