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
            SpecialStringMaker.ShowHasBeenUsedThisTurn(TakenShelterThisTurn, base.Card.Title + " has already prevented damage dealt by a Werewolf this turn.", base.Card.Title + " has not prevented damage dealt by a Werewolf this turn.");
        }

        protected readonly string TakenShelterThisTurn = "TakenShelterThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Once per turn, when a Werewolf would deal damage, each player may put a card in their hand on the bottom of their deck. If {H - 2} cards are moved this way, prevent that damage."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(TakenShelterThisTurn) && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card.DoKeywordsContain(WerewolfKeyword) && dda.Amount > 0 && dda.CanDealDamage, MoveCardsToPreventDamageResponse, TriggerType.WouldBeDealtDamage, TriggerTiming.Before);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(TakenShelterThisTurn), TriggerType.Hidden);
        }

        private IEnumerator MoveCardsToPreventDamageResponse(DealDamageAction dda)
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
                SetCardPropertyToTrueIfRealAction(TakenShelterThisTurn);
                // "... each player may put a card in their hand on the bottom of their deck."
                List<MoveCardAction> moved = new List<MoveCardAction>();
                IEnumerator selectCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.ToHero().IsIncapacitatedOrOutOfGame && tt.ToHero().HasCardsInHand), SelectionType.MoveCardOnBottomOfDeck, (TurnTaker tt) => PlayerMovesCardResponse(tt, moved, dda), dealDamageInfo: dda.ToEnumerable(), cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selectCoroutine);
                }
                if (moved.Where((MoveCardAction mca) => mca.WasCardMoved).Count() >= H - 2)
                {
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
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Select a player to move a card from hand on the bottom of their deck", "choosing a player to move a card from hand on the bottom of their deck", "Vote for a player to move a card from hand on the bottom of their deck", "player to move a card from hand on the bottom of their deck");
        }

        private IEnumerator PlayerMovesCardResponse(TurnTaker tt, List<MoveCardAction> moves, DealDamageAction dda)
        {
            // "... may put a card in their hand on the bottom of their deck."
            List<SelectCardDecision> decisions = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(base.GameController.FindHeroTurnTakerController(tt.ToHero()), SelectionType.MoveCardOnBottomOfDeck, new LinqCardCriteria((Card c) => c.Location == tt.ToHero().Hand), decisions, false, gameAction: dda, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            SelectCardDecision choice = decisions.Where((SelectCardDecision scd) => scd.Completed && scd.SelectedCard != null).FirstOrDefault();
            if (choice != null)
            {
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.GameController.FindTurnTakerController(tt), choice.SelectedCard, tt.Deck, toBottom: true, responsibleTurnTaker: tt, storedResults: moves, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
            }
        }

        public override IEnumerator ReducedToZeroResponse()
        {
            // "... each player discards a card."
            yield return base.GameController.EachPlayerDiscardsCards(1, 1, cardSource: GetCardSource());
        }
    }
}
