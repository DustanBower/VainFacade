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
    public class CausalCascadeCardController : GrandfatherUtilityCardController
    {
        public CausalCascadeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override bool DoNotMoveOneShotToTrash
        {
            get
            {
                if (base.Card.Location.IsDeck && base.Card.Location.IsVillain)
                {
                    return true;
                }
                return base.DoNotMoveOneShotToTrash;
            }
        }

        public override IEnumerator Play()
        {
            // "Shuffle the villain trash into the villain deck."
            IEnumerator shuffleCoroutine = base.GameController.ShuffleTrashIntoDeck(base.TurnTakerController, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }
            // "Reveal cards from the top of the villain deck until a Fulcrum is revealed."
            List<Card> matching = new List<Card>();
            List<Card> nonMatching = new List<Card>();
            List<Card> storedResultsCard = new List<Card>();
            List<RevealCardsAction> storedResultsAction = new List<RevealCardsAction>();
            IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, base.TurnTaker.Deck, (Card c) => c.DoKeywordsContain(FulcrumKeyword), 1, storedResultsAction, RevealedCardDisplay.ShowRevealedCards, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            if (storedResultsAction.FirstOrDefault() != null)
            {
                matching.AddRange(storedResultsAction.FirstOrDefault().MatchingCards);
                nonMatching.AddRange(storedResultsAction.FirstOrDefault().NonMatchingCards);
            }
            // "Put it into play."
            string message = "";
            if (matching.Count > 0)
            {
                message = base.Card.Title + " puts a Fulcrum from the villain deck into play...";
            }
            else
            {
                message = "No Fulcrum cards were found in the villain deck.";
            }
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            if (matching.Count > 0)
            {
                IEnumerator putCoroutine = base.GameController.PlayCard(base.TurnTakerController, matching.FirstOrDefault(), isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(putCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(putCoroutine);
                }
            }
            // "Shuffle the other revealed cards and this card into the villain deck."
            List<Card> toShuffle = new List<Card>();
            toShuffle.AddRange(nonMatching);
            toShuffle.Add(base.Card);
            IEnumerator reshuffleCoroutine = base.GameController.ShuffleCardsIntoLocation(DecisionMaker, toShuffle, base.TurnTaker.Deck, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(reshuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(reshuffleCoroutine);
            }
            List<Location> itinerary = new List<Location>();
            itinerary.Add(base.TurnTaker.Revealed);
            IEnumerator cleanupCoroutine = CleanupCardsAtLocations(itinerary, base.TurnTaker.Deck, isReturnedToOriginalLocation: true, cardsInList: matching.Union(nonMatching).ToList());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cleanupCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cleanupCoroutine);
            }
        }
    }
}
