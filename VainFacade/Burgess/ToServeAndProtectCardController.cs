using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Burgess
{
    public class ToServeAndProtectCardController : CardController
    {
        public ToServeAndProtectCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If this card is in play, make sure the player can tell where it is...
            SpecialStringMaker.ShowSpecialString(() => "This card is in " + base.Card.Location.GetFriendlyName() + ".").Condition = () => base.Card.IsInPlayAndHasGameText;
            // ... and who it's protecting
            SpecialStringMaker.ShowListOfCardsAtLocationOfCard(base.Card, new LinqCardCriteria((Card c) => c.IsTarget, "target", useCardsSuffix: false, false, "target", "targets")).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a target in this play area would take damage, you may redirect that damage to a target in your play area."
            AddTrigger((DealDamageAction dda) => dda.Target.IsAtLocationRecursive(base.Card.Location), (DealDamageAction dda) => base.GameController.SelectTargetAndRedirectDamage(base.HeroTurnTakerController, (Card c) => c.IsTarget && c.IsAtLocationRecursive(base.TurnTaker.PlayArea), dda, optional: true, cardSource: GetCardSource()), new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.RedirectDamage }, TriggerTiming.Before);
            // "At the start of your turn, if you redirected damage this way since the start of your last turn, discard 3 cards or destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker && base.GameController.Game.Journal.QueryJournalEntries((RedirectDamageJournalEntry r) => r.CardSource == base.Card && r.AmoutRedirected > 0).Where(base.GameController.Game.Journal.SinceLastTurn<RedirectDamageJournalEntry>(base.TurnTaker)).Any(), DiscardOrSelfDestructResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DestroySelf });
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> destination, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card in another play area."
            // Adapted from SergeantSteelTeam.MissionObjectiveCardController
            List<SelectTurnTakerDecision> storedResults = new List<SelectTurnTakerDecision>();
            IEnumerator coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.MoveCardToPlayArea, storedResults, optional: false, allowAutoDecide: false, (TurnTaker tt) => tt != base.TurnTaker, null, null, checkExtraTurnTakersInstead: false, canBeCancelled: true, ignoreBattleZone: false, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            SelectTurnTakerDecision selectTurnTakerDecision = storedResults.FirstOrDefault();
            if (selectTurnTakerDecision != null && selectTurnTakerDecision.SelectedTurnTaker != null && destination != null)
            {
                destination.Add(new MoveCardDestination(selectTurnTakerDecision.SelectedTurnTaker.PlayArea, toBottom: false, showMessage: true));
                yield break;
            }
            coroutine = base.GameController.SendMessageAction("No viable play locations. Putting this card in the trash", Priority.Low, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            destination.Add(new MoveCardDestination(base.TurnTaker.Trash, toBottom: false, showMessage: true));
        }

        private IEnumerator DiscardOrSelfDestructResponse(PhaseChangeAction pca)
        {
            // "... discard 3 cards or destroy this card."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            List<Function> choices = new List<Function>();
            choices.Add(new Function(base.HeroTurnTakerController, "Discard 3 cards", SelectionType.DiscardCard, () => SelectAndDiscardCards(base.HeroTurnTakerController, 3, requiredDecisions: 3, storedResults: discards, allowAutoDecide: base.HeroTurnTaker.NumberOfCardsInHand == 3, responsibleTurnTaker: base.TurnTaker), onlyDisplayIfTrue: base.HeroTurnTaker.NumberOfCardsInHand >= 3, forcedActionMessage: base.Card.Title + " cannot be destroyed, so " + base.TurnTaker.Name + " must discard 3 cards."));
            choices.Add(new Function(base.HeroTurnTakerController, "Destroy " + base.Card.Title, SelectionType.DestroySelf, () => DestroyThisCardResponse(pca), onlyDisplayIfTrue: !AskIfCardIsIndestructible(base.Card), forcedActionMessage: base.TurnTaker.Name + " does not have at least 3 cards in hand, so " + base.Card.Title + " must be destroyed."));
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, choices, false, noSelectableFunctionMessage: base.TurnTaker.Name + " can neither discard 3 cards nor destroy " + base.Card.Title + ".", cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }
    }
}
