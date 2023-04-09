using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Blitz
{
    public class CallingAnAudibleCardController : BlitzUtilityCardController
    {
        public CallingAnAudibleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of One-Shot cards in Blitz's deck
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.IsOneShot, "One-Shot"));
            // Show list of Playbook cards in Blitz's deck
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Deck, IsPlaybook);
        }

        public override IEnumerator Play()
        {
            // "Reveal the top {H} cards of the villain deck."
            List<Card> revealedCards = new List<Card>();
            IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, base.TurnTaker.Deck, H, revealedCards, revealedCardDisplay: RevealedCardDisplay.Message, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            // "Put the first revealed One-Shot and the first revealed Playbook into play."
            Card firstOneShot = null;
            Card firstPlaybook = null;
            foreach (Card c in revealedCards)
            {
                if (firstOneShot == null && c.IsOneShot)
                {
                    firstOneShot = c;
                }
                if (firstPlaybook == null && IsPlaybook.Criteria(c))
                {
                    firstPlaybook = c;
                }
            }
            IEnumerator oneshotCoroutine = base.GameController.SendMessageAction("None of the revealed cards were One-Shots.", Priority.Medium, GetCardSource(), revealedCards, showCardSource: true);
            if (firstOneShot != null)
            {
                oneshotCoroutine = base.GameController.PlayCard(base.TurnTakerController, firstOneShot, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(oneshotCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(oneshotCoroutine);
            }
            IEnumerator playbookCoroutine = base.GameController.SendMessageAction("None of the revealed cards were Playbooks.", Priority.Medium, GetCardSource(), revealedCards, showCardSource: true);
            if (firstOneShot != null)
            {
                playbookCoroutine = base.GameController.PlayCard(base.TurnTakerController, firstPlaybook, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playbookCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playbookCoroutine);
            }
            // "Discard the other revealed cards."
            List<Card> toDiscard = revealedCards.Where((Card c) => c != firstOneShot && c != firstPlaybook).ToList();
            IEnumerator discardCoroutine = base.GameController.MoveCards(base.TurnTakerController, toDiscard, base.TurnTaker.Trash, responsibleTurnTaker: base.TurnTaker, isDiscard: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            List<Location> toClean = new List<Location>();
            toClean.Add(base.TurnTaker.Revealed);
            IEnumerator cleanupCoroutine = base.GameController.CleanupCardsAtLocations(base.TurnTakerController, toClean, base.TurnTaker.Deck, cardsInList: revealedCards);
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
