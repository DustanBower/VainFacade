using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class GrandfatherTurnTakerController : TurnTakerController
    {
        public GrandfatherTurnTakerController(TurnTaker turnTaker, GameController gameController) : base(turnTaker, gameController)
        {

        }

        public override IEnumerator StartGame()
        {
            // "Put [i]Arrow of Time[/i] into play."
            Card timeline = base.TurnTaker.GetCardByIdentifier("ArrowOfTime");
            IEnumerator putCoroutine = base.GameController.PlayCard(this, timeline, isPutIntoPlay: true, cardSource: new CardSource(base.CharacterCardController));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(putCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(putCoroutine);
            }
            // "Reveal cards from the top of the villain deck until a Design and Fulcrum are revealed."
            string designKeyword = "design";
            string fulcrumKeyword = "fulcrum";
            List<RevealCardsAction> revealed = new List<RevealCardsAction>();
            Card firstDesign = null;
            Card firstFulcrum = null;
            List<Card> allMatches = new List<Card>();
            List<string> searchKeywords = new List<string>();
            searchKeywords.Add(designKeyword);
            searchKeywords.Add(fulcrumKeyword);
            IEnumerator revealCoroutine;

            while (searchKeywords.Count > 0 && base.TurnTaker.Deck.HasCards)
            {
                // Reveal cards from the villain deck until you find one that has a keyword in searchKeywords (or the deck runs out of cards)
                revealCoroutine = base.GameController.RevealCards(this, base.TurnTaker.Deck, (Card c) => c.DoKeywordsContain(searchKeywords), 1, revealed, cardSource: new CardSource(base.CharacterCardController));
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(revealCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(revealCoroutine);
                }

                List<Card> newMatches = base.CharacterCardController.GetRevealedCards(revealed).Where((Card c) => c.DoKeywordsContain(searchKeywords)).Take(1).ToList();
                if (newMatches.Any())
                {
                    // If the card found has a keyword that's still in searchKeywords, add the card to allMatches and remove the keyword
                    Card firstMatch = newMatches.First();
                    if (firstMatch.DoKeywordsContain(designKeyword) && searchKeywords.Contains(designKeyword))
                    {
                        firstDesign = firstMatch;
                        allMatches.Add(firstMatch);
                        searchKeywords.Remove(designKeyword);
                    }
                    else if (firstMatch.DoKeywordsContain(fulcrumKeyword) && searchKeywords.Contains(fulcrumKeyword))
                    {
                        firstFulcrum = firstMatch;
                        allMatches.Add(firstMatch);
                        searchKeywords.Remove(fulcrumKeyword);
                    }
                }
            }
            // "Put the first revealed Design and Fulcrum into play."
            List<Card> cardsToPlay = new List<Card>();
            cardsToPlay.Add(firstDesign);
            cardsToPlay.Add(firstFulcrum);
            foreach (Card c in cardsToPlay)
            {
                if (c != null)
                {
                    IEnumerator playCoroutine = base.GameController.PlayCard(this, c, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: new CardSource(base.CharacterCardController));
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
            // "Shuffle the rest into the villain deck."
            List<Card> otherRevealed = base.CharacterCardController.GetRevealedCards(revealed).Where((Card c) => !allMatches.Contains(c)).ToList();
            IEnumerator shuffleCoroutine = base.GameController.ShuffleCardsIntoLocation(base.FindDecisionMaker(), otherRevealed, base.TurnTaker.Deck, cardSource: new CardSource(base.CharacterCardController));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }
        }
    }
}
