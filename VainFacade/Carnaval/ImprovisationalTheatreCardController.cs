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
    public class ImprovisationalTheatreCardController : CardController
    {
        public ImprovisationalTheatreCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When this card is destroyed, you may draw a card, play a card, and use a power."
            AddWhenDestroyedTrigger(DrawPlayPowerResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.PlayCard, TriggerType.UsePower });
            // "At the end of your turn, you may discard a card to put the top card of a deck into play."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardToPutTopCardIntoPlayResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PutIntoPlay });
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Draw a card."
            IEnumerator drawCoroutine = DrawCard(base.TurnTaker.ToHero());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "Destroy this card."
            IEnumerator destructCoroutine = DestroyThisCardResponse(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
        }

        private IEnumerator DrawPlayPowerResponse(DestroyCardAction dca)
        {
            // "... you may draw a card, ..."
            IEnumerator drawCoroutine = DrawCard(base.TurnTaker.ToHero(), optional: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "... play a card, ..."
            IEnumerator playCoroutine = SelectAndPlayCardFromHand(base.TurnTakerController.ToHero());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            // "... and use a power."
            IEnumerator powerCoroutine = base.GameController.SelectAndUsePower(DecisionMaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(powerCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(powerCoroutine);
            }
        }

        private IEnumerator DiscardToPutTopCardIntoPlayResponse(PhaseChangeAction pca)
        {
            // "... you may discard a card..."
            List<DiscardCardAction> discarded = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = SelectAndDiscardCards(DecisionMaker, 1, true, 0, discarded, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(discarded))
            {
                // "... to put the top card of a deck into play."
                List<SelectLocationDecision> decks = new List<SelectLocationDecision>();
                IEnumerator selectCoroutine = SelectDecks(DecisionMaker, 1, SelectionType.PlayTopCard, (Location l) => true, decks);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selectCoroutine);
                }
                SelectLocationDecision choice = decks.FirstOrDefault();
                if (choice != null && choice.SelectedLocation.Location != null)
                {
                    IEnumerator playCoroutine = base.GameController.PlayTopCardOfLocation(base.TurnTakerController, choice.SelectedLocation.Location, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
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
    }
}
