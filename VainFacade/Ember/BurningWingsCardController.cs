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
    public class BurningWingsCardController : EmberUtilityCardController
    {
        public BurningWingsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Blaze cards in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(BlazeCard);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each instance of damage dealt by {EmberCharacter} would be reduced, increase that damage by 1."
            AddTrigger((ReduceDamageAction rda) => rda.DealDamageAction.DamageSource.IsSameCard(base.CharacterCard) && !rda.DealDamageAction.DamageModifiers.Any((ModifyDealDamageAction mdda) => mdda is ReduceDamageAction && mdda != rda), (ReduceDamageAction rda) => base.GameController.IncreaseDamage(rda.DealDamageAction, 1, cardSource: GetCardSource()), TriggerType.IncreaseDamage, TriggerTiming.Before);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "For each Blaze card in play, reveal and replace the top card of a deck."
            int numReveals = NumBlazeCardsInPlay();
            for (int i = 0; i < numReveals; i++)
            {
                List<SelectLocationDecision> selectResults = new List<SelectLocationDecision>();
                IEnumerator selectCoroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.RevealTopCardOfDeck, (Location l) => true, selectResults, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selectCoroutine);
                }
                Location deck = GetSelectedLocation(selectResults);
                if (deck != null)
                {
                    IEnumerator revealReplaceCoroutine = RevealCards_MoveMatching_ReturnNonMatchingCards(base.TurnTakerController, deck, false, false, false, new LinqCardCriteria((Card c) => false), 1, 1, revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(revealReplaceCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(revealReplaceCoroutine);
                    }
                }
            }
            // "You may discard the top card of a deck."
            List<SelectLocationDecision> selectDiscardResults = new List<SelectLocationDecision>();
            IEnumerator selectDiscardCoroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.DiscardFromDeck, (Location l) => true, selectDiscardResults, optional: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectDiscardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectDiscardCoroutine);
            }
            Location deckDiscard = GetSelectedLocation(selectDiscardResults);
            if (deckDiscard != null)
            {
                IEnumerator discardCoroutine = base.GameController.DiscardTopCard(deckDiscard, null, showCard: (Card c) => true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
            }
            // "You may play a card or you may discard a card to use a power."
            List<Function> options = new List<Function>();
            options.Add(new Function(DecisionMaker, "Play a card", SelectionType.PlayCard, () => SelectAndPlayCardFromHand(DecisionMaker, optional: true), CanPlayCardsFromHand(DecisionMaker), base.TurnTaker.Name + " cannot use any powers, so they must play a card."));
            options.Add(new Function(DecisionMaker, "Discard a card to use a power", SelectionType.DiscardCard, () => DiscardToUsePowerResponse(), base.TurnTaker.ToHero().HasCardsInHand));
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, DecisionMaker, options, true, noSelectableFunctionMessage: base.TurnTaker.Name + " cannot currently play cards or discard cards.", cardSource: GetCardSource());
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

        private IEnumerator DiscardToUsePowerResponse()
        {
            // "... you may discard a card to use a power."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = SelectAndDiscardCards(DecisionMaker, 1, requiredDecisions: 0, storedResults: discards, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(discards, 1, true))
            {
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
        }
    }
}
