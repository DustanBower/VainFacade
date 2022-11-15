using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Node
{
    public class NetworkAccessCardController : NodeUtilityCardController
    {
        public NetworkAccessCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Connection cards in Node's deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, isConnection);
        }

        public override IEnumerator Play()
        {
            // "You may shuffle your trash into your deck."
            YesNoDecision shuffleChoice = new YesNoDecision(base.GameController, DecisionMaker, SelectionType.ShuffleTrashIntoDeck, associatedCards: base.TurnTaker.Trash.Cards, cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.MakeDecisionAction(shuffleChoice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            // "Reveal cards from the top of your deck until 2 Connections are revealed."
            List<Card> matchingCards = new List<Card>();
            List<Card> nonMatchingCards = new List<Card>();
            List<Card> storedResultsCard = new List<Card>();
            List<RevealCardsAction> storedResultsAction = new List<RevealCardsAction>();
            IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, base.TurnTaker.Deck, (Card c) => isConnection.Criteria(c), 2, storedResultsAction, RevealedCardDisplay.Message, cardSource: GetCardSource());
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
                matchingCards.AddRange(storedResultsAction.FirstOrDefault().MatchingCards);
                nonMatchingCards.AddRange(storedResultsAction.FirstOrDefault().NonMatchingCards);
            }
            // "Put 1 into your hand and 1 into play."
            if (matchingCards.Count > 1)
            {
                IEnumerator toHandCoroutine = base.GameController.SelectAndMoveCard(DecisionMaker, (Card c) => matchingCards.Contains(c), base.TurnTaker.ToHero().Hand, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(toHandCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(toHandCoroutine);
                }
                IEnumerator toPlayCoroutine = base.GameController.SelectAndPlayCard(DecisionMaker, matchingCards.Where((Card c) => !c.Location.IsHand), isPutIntoPlay: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(toPlayCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(toPlayCoroutine);
                }
            }
            else if (matchingCards.Count > 0)
            {
                IEnumerator toHandCoroutine = base.GameController.MoveCard(base.TurnTakerController, matchingCards.FirstOrDefault(), base.TurnTaker.ToHero().Hand, showMessage: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(toHandCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(toHandCoroutine);
                }
                IEnumerator messageCoroutine = base.GameController.SendMessageAction("There are no more revealed Connection cards.", Priority.High, GetCardSource(), associatedCards: nonMatchingCards, showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            // "Shuffle the other cards into your deck."
            IEnumerator bulkMoveCoroutine = base.GameController.BulkMoveCards(base.TurnTakerController, nonMatchingCards, base.TurnTaker.Deck, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(bulkMoveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(bulkMoveCoroutine);
            }
            IEnumerator shuffleCoroutine = base.GameController.ShuffleLocation(base.TurnTaker.Deck, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }
            List<Location> toClean = new List<Location>();
            toClean.Add(base.TurnTaker.Revealed);
            IEnumerator cleanupCoroutine = CleanupCardsAtLocations(toClean, base.TurnTaker.Deck, shuffleAfterwards: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cleanupCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cleanupCoroutine);
            }
            // "You may draw a card."
            IEnumerator drawCoroutine = DrawCard(base.TurnTaker.ToHero(), optional: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "You may play a card."
            IEnumerator playCoroutine = SelectAndPlayCardFromHand(DecisionMaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }
    }
}
