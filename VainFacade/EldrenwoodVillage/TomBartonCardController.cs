using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.EldrenwoodVillage
{
    public class TomBartonCardController : EldrenwoodTargetCardController
    {
        public TomBartonCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
            SpecialStringMaker.ShowHasBeenUsedThisTurn(TakenShelterThisTurn, base.Card.Title + " has already prevented damage dealt by a Werewolf this turn.", base.Card.Title + " has not prevented damage dealt by a Werewolf this turn.");
        }

        protected readonly string TakenShelterThisTurn = "TakenShelterThisTurn";

        public Guid? MoveCardsForDamage { get; set; }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Once per turn, when a Werewolf would deal damage, each player may put a card in their hand on the bottom of their deck. If {H - 2} cards are moved this way, prevent that damage."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(TakenShelterThisTurn) && dda.DamageSource != null && dda.DamageSource.IsCard && base.GameController.DoesCardContainKeyword(dda.DamageSource.Card, WerewolfKeyword) && dda.Amount > 0 && dda.CanDealDamage, MoveCardsToPreventDamageResponse, TriggerType.WouldBeDealtDamage, TriggerTiming.Before);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(TakenShelterThisTurn), TriggerType.Hidden);
        }

        private IEnumerator MoveCardsToPreventDamageResponse(DealDamageAction dda)
        {
            if (!MoveCardsForDamage.HasValue || MoveCardsForDamage.Value != dda.InstanceIdentifier)
            {
                if (!HasBeenSetToTrueThisTurn(TakenShelterThisTurn))
                {
                    YesNoDecision choice = new YesNoDecision(base.GameController, DecisionMaker, SelectionType.Custom, gameAction: dda, associatedCards: dda.Target.ToEnumerable(), cardSource: GetCardSource());
                    IEnumerator decideCoroutine = base.GameController.MakeDecisionAction(choice);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(decideCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(decideCoroutine);
                    }
                    if (DidPlayerAnswerYes(choice))
                    {
                        MoveCardsForDamage = dda.InstanceIdentifier;
                    }
                }
            }
            if (MoveCardsForDamage.HasValue && MoveCardsForDamage.Value == dda.InstanceIdentifier && IsRealAction(dda))
            {
                SetCardPropertyToTrueIfRealAction(TakenShelterThisTurn);
                // "... each player may put a card in their hand on the bottom of their deck."
                List<MoveCardAction> moved = new List<MoveCardAction>();
                IEnumerator selectCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.ToHero().IsIncapacitatedOrOutOfGame && tt.ToHero().HasCardsInHand), SelectionType.MoveCardOnBottomOfDeck, (TurnTaker tt) => PlayerMovesCardResponse(tt, moved), requiredDecisions: 0, dealDamageInfo: dda.ToEnumerable(), cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selectCoroutine);
                }
                foreach (MoveCardAction mca in moved)
                {
                    Log.Debug("TomBartonCardController.MoveCardsToPreventDamageResponse: mca.Origin: " + mca.Origin.GetFriendlyName());
                    Log.Debug("TomBartonCardController.MoveCardsToPreventDamageResponse: mca.CardToMove: " + mca.CardToMove.Title);
                    Log.Debug("TomBartonCardController.MoveCardsToPreventDamageResponse: mca.WasCardMoved: " + mca.WasCardMoved.ToString());
                }
                if (moved.Where((MoveCardAction mca) => mca.WasCardMoved).Count() >= H - 2)
                {
                    Log.Debug("TomBartonCardController.MoveCardsToPreventDamageResponse: preventing damage");
                    // "If {H - 2} cards are moved this way, prevent that damage."
                    IEnumerator preventCoroutine = CancelAction(dda, isPreventEffect: true);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(preventCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(preventCoroutine);
                    }
                }
            }
            if (IsRealAction(dda))
            {
                MoveCardsForDamage = null;
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Do you want to move cards from hand on the bottom of their decks?", "choosing whether to move cards from hand on the bottom of their decks", "Vote for whether to move cards from hand on the bottom of their decks", "move cards from hand on the bottom of their decks");
        }

        private IEnumerator PlayerMovesCardResponse(TurnTaker tt, List<MoveCardAction> moves)
        {
            // "... may put a card in their hand on the bottom of their deck."
            IEnumerator moveCoroutine = base.GameController.SelectCardFromLocationAndMoveIt(base.GameController.FindHeroTurnTakerController(tt.ToHero()), tt.ToHero().Hand, new LinqCardCriteria((Card c) => true), new MoveCardDestination[] { new MoveCardDestination(tt.Deck, toBottom: true) }, optional: true, responsibleTurnTaker: tt, storedResultsMove: moves, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            /*List<SelectCardDecision> decisions = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(base.GameController.FindHeroTurnTakerController(tt.ToHero()), SelectionType.MoveCardOnBottomOfDeck, new LinqCardCriteria((Card c) => c.Location == tt.ToHero().Hand), decisions, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            foreach(SelectCardDecision scd in decisions)
            {
                Log.Debug("TomBartonCardController.PlayerMovesCardResponse: scd: " + scd.ToString());
                Log.Debug("TomBartonCardController.PlayerMovesCardResponse: scd.SelectedCard: " + scd.SelectedCard.Title);
                Log.Debug("TomBartonCardController.PlayerMovesCardResponse: scd.Completed: " + scd.Completed.ToString());
            }
            SelectCardDecision choice = decisions.Where((SelectCardDecision scd) => scd.Completed && scd.SelectedCard != null).FirstOrDefault();
            Log.Debug("TomBartonCardController.PlayerMovesCardResponse: choice: " + choice.ToString());
            if (choice != null)
            {
                Log.Debug("TomBartonCardController.PlayerMovesCardResponse: moving " + choice.SelectedCard.Title + " to the bottom of " + tt.Deck.GetFriendlyName());
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.GameController.FindTurnTakerController(tt), choice.SelectedCard, tt.Deck, toBottom: true, decisionSources: decisions.CastEnumerable<SelectCardDecision, IDecision>(), responsibleTurnTaker: tt, storedResults: moves, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
            }*/
        }

        public override IEnumerator ReducedToZeroResponse()
        {
            // "... each player discards a card."
            yield return base.GameController.EachPlayerDiscardsCards(1, 1, cardSource: GetCardSource());
        }
    }
}
