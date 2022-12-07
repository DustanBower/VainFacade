using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Carnaval
{
    public class RisingActionCardController : CardController
    {
        public RisingActionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When this card is destroyed, you may draw and play a card."
            AddWhenDestroyedTrigger(DrawPlayResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.PlayCard });
        }

        private IEnumerator DrawPlayResponse(DestroyCardAction dca)
        {
            // "... you may draw and play a card."
            IEnumerator drawCoroutine = DrawCard(base.TurnTaker.ToHero(), optional: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
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

        public override IEnumerator UsePower(int index = 0)
        {
            int numDecks = GetPowerNumeral(0, 2);
            // "Reveal the top card of 2 hero decks."
            List<SelectLocationDecision> storedDecks = new List<SelectLocationDecision>();
            IEnumerator selectCoroutine = SelectDecks(DecisionMaker, numDecks, SelectionType.RevealTopCardOfDeck, (Location l) => l.IsHero, storedDecks);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            IEnumerable<Location> decks = from l in storedDecks where l.Completed && l.SelectedLocation.Location != null select l.SelectedLocation.Location;
            List<Card> storedCards = new List<Card>();
            foreach (Location item in decks)
            {
                IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, item, 1, storedCards, revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(revealCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(revealCoroutine);
                }
            }
            // "You may play a card revealed this way."
            IEnumerator playCoroutine = base.GameController.SelectAndPlayCard(DecisionMaker, storedCards, optional: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            // "Replace the other revealed cards."
            foreach (Location item in decks)
            {
                TurnTaker tt = item.OwnerTurnTaker;
                if (tt.Revealed.HasCards)
                {
                    IEnumerator replaceCoroutine = base.GameController.MoveCards(base.TurnTakerController, tt.Revealed.Cards.Where((Card c) => storedCards.Contains(c)), (Card c) => new MoveCardDestination(item), responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
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
        }
    }
}
