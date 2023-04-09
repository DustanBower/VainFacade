using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Ember
{
    public class OffThePathCardController : CardController
    {
        public OffThePathCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether Ember has been dealt damage of a relevant type this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstRelevantDamageThisTurn, base.CharacterCard.Title + " has already been dealt toxic, psychic, and/or infernal damage this turn since " + base.Card.Title + " entered play.", base.CharacterCard.Title + " has not been dealt toxic, psychic, or infernal damage this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        private readonly string FirstRelevantDamageThisTurn = "FirstRelevantDamageThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn {EmberCharacter} is dealt toxic, psychic, or infernal damage, you may draw or play a card."
            AddTrigger((DealDamageAction dda) => dda.DidDealDamage && dda.Target == base.CharacterCard && (dda.DamageType == DamageType.Toxic || dda.DamageType == DamageType.Psychic || dda.DamageType == DamageType.Infernal) && !HasBeenSetToTrueThisTurn(FirstRelevantDamageThisTurn), FirstRelevantDamageResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.PlayCard }, TriggerTiming.After);
        }

        private IEnumerator FirstRelevantDamageResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(FirstRelevantDamageThisTurn);
            // "... you may draw or play a card."
            IEnumerator drawPlayCoroutine = DrawACardOrPlayACard(DecisionMaker, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawPlayCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawPlayCoroutine);
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Reveal the top 2 cards of a deck. Put one on the top and one on the bottom."
            int numToReveal = GetPowerNumeral(0, 2);
            List<SelectLocationDecision> results = new List<SelectLocationDecision>();
            IEnumerator chooseCoroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.RevealCardsFromDeck, (Location l) => l.IsDeck, results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            Location deck = GetSelectedLocation(results);
            List<Card> storedCards = new List<Card>();
            if (deck != null)
            {
                IEnumerator revealCoroutine = RevealCardsFromTopOfDeck_PutOnTopAndOnBottom(DecisionMaker, base.TurnTakerController, deck, numToReveal, storedResults: storedCards);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(revealCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(revealCoroutine);
                }

                List<Location> locations = new List<Location>();
                locations.Add(deck.OwnerTurnTaker.Revealed);
                IEnumerator cleanupCoroutine = base.GameController.CleanupCardsAtLocations(base.TurnTakerController, locations, deck, cardsInList: storedCards);
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
}
