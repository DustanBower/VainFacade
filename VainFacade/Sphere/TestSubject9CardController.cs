using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class TestSubject9CardController : SphereUtilityCardController
    {
        public TestSubject9CardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Emanations in Sphere's deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, isEmanation);
        }

        public override IEnumerator Play()
        {
            // "Reveal cards from the top of your deck until 2 Emanations are revealed."
            List<RevealCardsAction> results = new List<RevealCardsAction>();
            IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, base.TurnTaker.Deck, (Card c) => c.DoKeywordsContain(emanationKeyword), 2, results, RevealedCardDisplay.Message, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            // "Put one into play..."
            List<Card> revealedCards = results.SelectMany((RevealCardsAction rca) => rca.RevealedCards).ToList();
            LinqCardCriteria wasRevealedEmanation = new LinqCardCriteria((Card c) => c.DoKeywordsContain(emanationKeyword) && revealedCards.Contains(c));
            IEnumerator playCoroutine = base.GameController.SelectCardFromLocationAndMoveIt(base.HeroTurnTakerController, base.TurnTaker.Revealed, wasRevealedEmanation, new MoveCardDestination[] { new MoveCardDestination(base.TurnTaker.PlayArea) }, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            // "... and one into your hand."
            IEnumerator handCoroutine = base.GameController.SelectCardFromLocationAndMoveIt(base.HeroTurnTakerController, base.TurnTaker.Revealed, wasRevealedEmanation, new MoveCardDestination[] { new MoveCardDestination(base.HeroTurnTaker.Hand) }, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(handCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(handCoroutine);
            }
            // "Discard the other revealed cards."
            List<Location> toClean = new List<Location>();
            toClean.Add(base.TurnTaker.Revealed);
            IEnumerator cleanupCoroutine = CleanupCardsAtLocations(toClean, base.TurnTaker.Trash, isDiscard: true, isReturnedToOriginalLocation: false, cardsInList: revealedCards);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cleanupCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cleanupCoroutine);
            }
            yield break;
        }
    }
}
