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
    public class QuibbleCardController : CardController
    {
        public QuibbleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play, show whether a card has been moved this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(UsedThisTurn, base.Card.Title + " has already moved a card on top of its deck this turn.", base.Card.Title + " has not moved a card on top of its deck this turn.").Condition = () => base.Card.IsInPlayAndHasGameText;
            // If in play and has moved a card, show most recently moved card
            SpecialStringMaker.ShowSpecialString(() => "Last card moved with " + base.Card.Title + ": " + GetCardPropertyJournalEntryCard(CardMoved).Title).Condition = () => base.Card.IsInPlayAndHasGameText && GetCardPropertyJournalEntryCard(CardMoved) != null;
        }

        private const string CardMoved = "MostRecentCardMoved";
        private const string UsedThisTurn = "WasUsedToMoveCardThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Once during a turn, when {TheFuryCharacter} would deal or be dealt damage, you may first put a non-character card in play on top of its deck. If you do, you may play a card or use a power."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(UsedThisTurn) && !dda.IsPretend && dda.Amount > 0 && dda.DamageSource.IsInPlayAndHasGameText && (dda.Target == base.CharacterCard || (dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card == base.CharacterCard)), MoveAndPlayOrPowerResponse, TriggerType.WouldBeDealtDamage, TriggerTiming.Before);
            // "At the end of each turn, if you have put a card on top of a deck in this way, put the top card of that deck into play and destroy this card."
            AddEndOfTurnTrigger((TurnTaker tt) => true, PutDestructResponse, new TriggerType[] { TriggerType.PutIntoPlay, TriggerType.DestroySelf }, additionalCriteria: (PhaseChangeAction pca) => GetCardPropertyJournalEntryCard(CardMoved) != null);
            ResetFlagAfterLeavesPlay(CardMoved);
        }

        private IEnumerator MoveAndPlayOrPowerResponse(DealDamageAction dda)
        {
            // "... you may first put a non-character card in play on top of its deck."
            List<MoveCardAction> moveResults = new List<MoveCardAction>();
            List<SelectCardDecision> choiceResults = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.MoveCardOnDeck, new LinqCardCriteria((Card c) => c.IsInPlay && !c.IsCharacter), choiceResults, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            Card selected = GetSelectedCard(choiceResults);
            if (selected != null)
            {
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, selected, selected.NativeDeck, responsibleTurnTaker: base.TurnTaker, storedResults: moveResults, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
            }
            // "If you do, you may play a card or use a power."
            if (DidMoveCard(moveResults))
            {
                Card moved = moveResults.Where((MoveCardAction mca) => mca.WasCardMoved).FirstOrDefault().CardToMove;
                AddCardPropertyJournalEntry(CardMoved, moved);
                IEnumerable<Function> functionChoices = new Function[2]
                {
                    new Function(DecisionMaker, "Play a card", SelectionType.PlayCard, () => base.GameController.SelectAndPlayCardFromHand(DecisionMaker, true, cardSource: GetCardSource()), onlyDisplayIfTrue: CanPlayCardsFromHand(base.HeroTurnTakerController)),
                    new Function(DecisionMaker, "Use a power", SelectionType.UsePower, () => base.GameController.SelectAndUsePower(DecisionMaker, cardSource: GetCardSource()), onlyDisplayIfTrue: base.GameController.CanUsePowers(FindHeroTurnTakerController(base.HeroTurnTaker), GetCardSource()))
                };
                SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, DecisionMaker, functionChoices, true, cardSource: GetCardSource());
                IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice);
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

        private IEnumerator PutDestructResponse(PhaseChangeAction pca)
        {
            // "... put the top card of that deck into play..."
            Card wasMoved = GetCardPropertyJournalEntryCard(CardMoved);
            Location deck = wasMoved.NativeDeck;
            IEnumerator putCoroutine = base.GameController.PlayTopCardOfLocation(DecisionMaker, deck, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource(), showMessage: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(putCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(putCoroutine);
            }
            // "... and destroy this card."
            IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
        }
    }
}
