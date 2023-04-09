using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class PickingUpThePiecesCardController : CardController
    {
        public PickingUpThePiecesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "Shuffle the villain trash..."
            IEnumerator shuffleCoroutine = base.GameController.ShuffleLocation(base.TurnTaker.Trash, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }
            // "... and reveal cards from it until a target is revealed."
            List<RevealCardsAction> revealed = new List<RevealCardsAction>();
            IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, base.TurnTaker.Trash, (Card c) => c.IsTarget, 1, revealed, RevealedCardDisplay.None, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            List<PlayCardAction> playResults = new List<PlayCardAction>();
            RevealCardsAction action = revealed.FirstOrDefault();
            if (action != null)
            {
                if (action.FoundMatchingCards)
                {
                    // "Put it into play."
                    IEnumerator putCoroutine = base.GameController.PlayCard(base.TurnTakerController, action.MatchingCards.First(), isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, storedResults: playResults, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(putCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(putCoroutine);
                    }
                }
                if (action.NonMatchingCards.Count() > 0)
                {
                    IEnumerator moveCoroutine = base.GameController.BulkMoveCards(base.TurnTakerController, action.NonMatchingCards, base.TurnTaker.Trash, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(moveCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(moveCoroutine);
                    }
                }
            }
            List<Location> places = new List<Location>();
            places.Add(base.TurnTaker.Revealed);
            List<Card> toCleanup = revealed.SelectMany((RevealCardsAction rca) => rca.RevealedCards).ToList();
            IEnumerator cleanupCoroutine = base.GameController.CleanupCardsAtLocations(base.TurnTakerController, places, base.TurnTaker.Trash, cardsInList: toCleanup);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cleanupCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cleanupCoroutine);
            }
            // "If no targets entered play this way, play the top card of the villain deck."
            if (!playResults.Any((PlayCardAction pca) => pca.WasCardPlayed && pca.CardToPlay.IsTarget))
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction("No targets were played from the villain trash, so " + base.Card.Title + " plays the top card of the villain deck.", Priority.High, cardSource: GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                IEnumerator playVillainCoroutine = base.GameController.PlayTopCard(DecisionMaker, base.TurnTakerController, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playVillainCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playVillainCoroutine);
                }
            }
        }
    }
}
