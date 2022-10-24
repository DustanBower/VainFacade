using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheFury
{
    public class CraftingTheFutureCardController : CardController
    {
        public CraftingTheFutureCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Select a deck to put its top card into play", "choosing a deck to put the top card into play", "Vote for a deck to put its top card into play", "deck to put its top card into play");
        }

        public override IEnumerator Play()
        {
            // "Reveal and replace the top card of a deck."
            List<SelectLocationDecision> revealChoice = new List<SelectLocationDecision>();
            Location revealedDeck = null;
            IEnumerator chooseRevealCoroutine = base.GameController.SelectADeck(base.HeroTurnTakerController, SelectionType.RevealTopCardOfDeck, (Location l) => l.HasCards, revealChoice, noValidLocationsMessage: "There are no decks with a top card to reveal.", cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseRevealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseRevealCoroutine);
            }
            if (DidSelectLocation(revealChoice))
            {
                revealedDeck = GetSelectedLocation(revealChoice);
                if (revealedDeck != null)
                {
                    List<Card> revealed = new List<Card>();
                    IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, revealedDeck, 1, revealed, revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(revealCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(revealCoroutine);
                    }
                    IEnumerator replaceCoroutine = base.CleanupRevealedCards(revealedDeck.OwnerTurnTaker.Revealed, revealedDeck);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(replaceCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(replaceCoroutine);
                    }
                }
            }
            // "Put the top card of a deck into play."
            List<SelectLocationDecision> putChoice = new List<SelectLocationDecision>();
            IEnumerator choosePutCoroutine = SelectDecks(DecisionMaker, 1, SelectionType.Custom, (Location l) => l.IsInGame, putChoice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(choosePutCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(choosePutCoroutine);
            }
            if (DidSelectLocation(putChoice))
            {
                Location putDeck = GetSelectedLocation(putChoice);
                if (putDeck != null)
                {
                    IEnumerator putCoroutine = base.GameController.PlayTopCardOfLocation(base.TurnTakerController, putDeck, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(putCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(putCoroutine);
                    }
                }
            }
            // "Draw a card."
            IEnumerator drawCoroutine = DrawCard();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "You may play a card or use a power."
            List<Function> options = new List<Function>();
            options.Add(new Function(DecisionMaker, "Play a card", SelectionType.PlayCard, () => SelectAndPlayCardFromHand(DecisionMaker, optional: true), CanPlayCardsFromHand(DecisionMaker), base.TurnTaker.Name + " cannot use any powers, so they must play a card."));
            options.Add(new Function(DecisionMaker, "Use a power", SelectionType.UsePower, () => base.GameController.SelectAndUsePower(DecisionMaker, optional: true, cardSource: GetCardSource()), base.GameController.CanUsePowers(DecisionMaker, GetCardSource()), base.TurnTaker.Name + " cannot play any cards, so they must use a power."));
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, DecisionMaker, options, true, noSelectableFunctionMessage: base.TurnTaker.Name + " cannot currently play cards or use powers.", cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice, associatedCards: base.Card.ToEnumerable());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
        }
    }
}
