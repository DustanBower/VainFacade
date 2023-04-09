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
    public class FullMoonCardController : EldrenwoodUtilityCardController
    {
        public FullMoonCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            if (card == base.Card)
            {
                // "This card is indestructible while there is at least 1 card under it."
                return base.Card.UnderLocation.HasCards;
            }
            return base.AskIfCardIsIndestructible(card);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the environment turn, discard 1 card from under this card. Then each Werewolf regains 1 HP. Then destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardHealDestructResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.GainHP, TriggerType.DestroySelf });
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, put the top card of 3 hero decks under this card."
            yield return SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => IsHero(tt) && !tt.ToHero().IsIncapacitatedOrOutOfGame && tt.Deck.HasCards, "heroes with cards in their decks"), SelectionType.MoveCardToUnderCard, (TurnTaker tt) => base.GameController.MoveCard(base.TurnTakerController, tt.Deck.TopCard, base.Card.UnderLocation, showMessage: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource()), 3, requiredDecisions: 3, allowAutoDecide: true, numberOfCards: 1, cardSource: GetCardSource());
        }

        // Copied from GameController
        public IEnumerator SelectTurnTakersAndDoAction(HeroTurnTakerController hero, LinqTurnTakerCriteria turnTakerCriteria, SelectionType selectionType, Func<TurnTaker, IEnumerator> actionWithTurnTaker, int? numberOfTurnTakers = null, bool optional = false, int? requiredDecisions = null, List<SelectTurnTakerDecision> storedResults = null, bool allowAutoDecide = false, IEnumerable<DealDamageAction> dealDamageInfo = null, Func<string> extraInfo = null, IEnumerable<Card> associatedCards = null, bool ignoreBattleZone = false, int? numberOfCards = null, CardSource cardSource = null)
        {
            BattleZone battleZone = null;
            if (cardSource != null && !ignoreBattleZone)
            {
                battleZone = cardSource.BattleZone;
            }
            SelectTurnTakersDecision selectTurnTakersDecision = new SelectTurnTakersDecision(base.GameController, hero, turnTakerCriteria, selectionType, numberOfTurnTakers, optional, requiredDecisions, eliminateOptions: true, allowAutoDecide, numberOfCards, dealDamageInfo, extraInfo, cardSource, associatedCards);
            selectTurnTakersDecision.BattleZone = battleZone;
            IEnumerator coroutine = SelectTurnTakersAndDoAction(selectTurnTakersDecision, actionWithTurnTaker, storedResults, cardSource);
            if (UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        // Copied from GameController to fix Choose For Me bug
        public IEnumerator SelectTurnTakersAndDoAction(SelectTurnTakersDecision selectTurnTakersDecision, Func<TurnTaker, IEnumerator> actionWithTurnTaker, List<SelectTurnTakerDecision> storedResults = null, CardSource cardSource = null)
        {
            if (FindTurnTakersWhere((TurnTaker tt) => selectTurnTakersDecision.Criteria.Criteria(tt) && (selectTurnTakersDecision.BattleZone == null || selectTurnTakersDecision.BattleZone == tt.BattleZone)).Count() == 0)
            {
                //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: no viable TurnTakers found");
                if (!string.IsNullOrEmpty(selectTurnTakersDecision.Criteria.Description))
                {
                    IEnumerator coroutine = base.GameController.SendMessageAction("There are no " + selectTurnTakersDecision.Criteria.Description + ".", Priority.High, cardSource, null, showCardSource: true);
                    if (UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }
                yield break;
            }
            YesNoDecision yesNo = null;
            if (selectTurnTakersDecision.IsOptional)
            {
                //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: selectTurnTakersDecision.IsOptional: " + selectTurnTakersDecision.IsOptional.ToString());
                //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: creating YesNoDecision");
                bool upTo = false;
                if (selectTurnTakersDecision.RequiredDecisions.HasValue && selectTurnTakersDecision.RequiredDecisions.Value == 0)
                {
                    upTo = true;
                }
                yesNo = new YesNoAmountDecision(base.GameController, selectTurnTakersDecision.HeroTurnTakerController, selectTurnTakersDecision.SelectionType, selectTurnTakersDecision.NumberOfTurnTakers, upTo, requireUnanimous: false, null, null, cardSource);
                IEnumerator coroutine2 = base.GameController.MakeDecisionAction(yesNo);
                if (UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine2);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine2);
                }
            }
            if (selectTurnTakersDecision.IsOptional && !base.GameController.DidAnswerYes(yesNo))
            {
                //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: player answered No for optional decision");
                yield break;
            }
            while (!selectTurnTakersDecision.WasCancelled && !WasCompleted(selectTurnTakersDecision) && base.GameController.CanPerformAction<GameAction>(base.TurnTakerController, selectTurnTakersDecision.CardSource))
            {
                /*Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: selectTurnTakersDecision.WasCancelled: " + selectTurnTakersDecision.WasCancelled.ToString());
                Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: selectTurnTakersDecision.Completed: " + selectTurnTakersDecision.Completed.ToString());
                Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction:    selectTurnTakersDecision.FinishedSelecting: " + selectTurnTakersDecision.FinishedSelecting.ToString());
                Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction:    selectTurnTakersDecision.SelectCardDecisions.Count: " + selectTurnTakersDecision.SelectCardDecisions.Count().ToString());
                foreach(SelectTurnTakerDecision d in selectTurnTakersDecision.SelectCardDecisions)
                {
                    Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction:        d from selectTurnTakersDecision.SelectCardDecisions: " + d.ToString());
                    Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction:        d.Completed: " + d.Completed.ToString());
                    Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction:        DidSelectTurnTaker(d.ToEnumerable()): " + DidSelectTurnTaker(d.ToEnumerable()).ToString());
                    if (DidSelectTurnTaker(d.ToEnumerable()))
                    {
                        Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction:        d.SelectedTurnTaker.Name: " + d.SelectedTurnTaker.Name);
                    }
                }
                Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction:    selectTurnTakersDecision.NumberOfTurnTakers.Value: " + selectTurnTakersDecision.NumberOfTurnTakers.Value.ToString());
                Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: WasCompleted(selectTurnTakersDecision): " + WasCompleted(selectTurnTakersDecision).ToString());
                Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: base.GameController.CanPerformAction<GameAction>(base.TurnTakerController, selectTurnTakersDecision.CardSource): " + base.GameController.CanPerformAction<GameAction>(base.TurnTakerController, selectTurnTakersDecision.CardSource).ToString());
                Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: getting next SelectTurnTakerDecision");*/
                SelectTurnTakerDecision decision = selectTurnTakersDecision.GetNextSelectTurnTakerDecision();
                storedResults?.Add(decision);
                if (decision.Choices.Count() == 1)
                {
                    if (!decision.IsOptional)
                    {
                        decision.AllowAutoDecide = true;
                        decision.AutoDecide();
                    }
                    else if (!decision.AutoDecided)
                    {
                        decision.AllowAutoDecide = false;
                    }
                }
                //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: passing decision to SelectTurnTakerAndDoAction");
                IEnumerator coroutine3 = base.GameController.SelectTurnTakerAndDoAction(decision, actionWithTurnTaker);
                if (UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine3);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine3);
                }
                if (decision.AllowAutoDecide && decision.AutoDecided)
                {
                    //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: AutoDecide was used");
                    //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: continuing to AutoDecide");
                    selectTurnTakersDecision.AutoDecide();
                }
                if (decision.IsOptional && decision.FinishedSelecting && decision.SelectedTurnTaker == null)
                {
                    //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: decision.FinishedSelecting && decision.SelectedTurnTaker == null");
                    //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: setting selectTurnTakersDecision.FinishedSelecting = true");
                    selectTurnTakersDecision.FinishedSelecting = true;
                }
                if (decision.WasCancelled)
                {
                    //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: decision.WasCancelled = true");
                    //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: setting selectTurnTakersDecision.WasCancelled = true");
                    selectTurnTakersDecision.WasCancelled = true;
                }
                List<TurnTaker> list = decision.Choices.ToList();
                if (selectTurnTakersDecision.SelectedTurnTaker != null)
                {
                    //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: selectTurnTakersDecision.SelectedTurnTaker: " + selectTurnTakersDecision.SelectedTurnTaker.Name);
                    //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: removing " + selectTurnTakersDecision.SelectedTurnTaker.Name + " from list of options");
                    list.Remove(selectTurnTakersDecision.SelectedTurnTaker);
                }
                if (list.Count() == 0)
                {
                    //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: decision.Choices.ToList().Count: " + list.Count.ToString());
                    //Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: setting selectTurnTakersDecision.FinishedSelecting = true");
                    selectTurnTakersDecision.FinishedSelecting = true;
                }
            }
            /*Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: selectTurnTakersDecision.WasCancelled: " + selectTurnTakersDecision.WasCancelled.ToString());
            Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: selectTurnTakersDecision.Completed: " + selectTurnTakersDecision.Completed.ToString());
            Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction:    selectTurnTakersDecision.FinishedSelecting: " + selectTurnTakersDecision.FinishedSelecting.ToString());
            Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction:    selectTurnTakersDecision.SelectCardDecisions.Count: " + selectTurnTakersDecision.SelectCardDecisions.Count().ToString());
            foreach (SelectTurnTakerDecision d in selectTurnTakersDecision.SelectCardDecisions)
            {
                Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction:        d from selectTurnTakersDecision.SelectCardDecisions: " + d.ToString());
                Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction:        d.Completed: " + d.Completed.ToString());
                Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction:        DidSelectTurnTaker(d.ToEnumerable()): " + DidSelectTurnTaker(d.ToEnumerable()).ToString());
                if (DidSelectTurnTaker(d.ToEnumerable()))
                {
                    Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction:        d.SelectedTurnTaker.Name: " + d.SelectedTurnTaker.Name);
                }
            }
            Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction:    selectTurnTakersDecision.NumberOfTurnTakers.Value: " + selectTurnTakersDecision.NumberOfTurnTakers.Value.ToString());
            Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: WasCompleted(selectTurnTakersDecision): " + WasCompleted(selectTurnTakersDecision).ToString());
            Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: base.GameController.CanPerformAction<GameAction>(base.TurnTakerController, selectTurnTakersDecision.CardSource): " + base.GameController.CanPerformAction<GameAction>(base.TurnTakerController, selectTurnTakersDecision.CardSource).ToString());
            Log.Debug("FullMoonCardController.SelectTurnTakersAndDoAction: loop terminated");*/
        }

        // More accurate version of SelectTurnTakersDecision.Completed
        private bool WasCompleted(SelectTurnTakersDecision sttd)
        {
            return sttd.FinishedSelecting || (sttd.NumberOfTurnTakers.HasValue && sttd.SelectCardDecisions.Where((SelectTurnTakerDecision d) => DidSelectTurnTaker(d.ToEnumerable())).Count() == sttd.NumberOfTurnTakers.Value);
        }

        private IEnumerator DiscardHealDestructResponse(PhaseChangeAction pca)
        {
            // "... discard 1 card from under this card."
            List<SelectCardDecision> selected = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.MoveCardToTrash, (from c in FindCardsWhere((Card c) => c.Location == base.Card.UnderLocation) orderby c.Owner.Name select c), selected, allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            SelectCardDecision choice = selected.Where((SelectCardDecision scd) => scd.Completed && scd.SelectedCard != null).FirstOrDefault();
            if (choice != null)
            {
                MoveCardDestination trashDestination = FindCardController(choice.SelectedCard).GetTrashDestination();
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, choice.SelectedCard, trashDestination.Location, responsibleTurnTaker: base.TurnTaker, isDiscard: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
            }
            // "Then each Werewolf regains 1 HP."
            IEnumerator healCoroutine = base.GameController.GainHP(DecisionMaker, (Card c) => base.GameController.DoesCardContainKeyword(c, WerewolfKeyword), 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            // "Then destroy this card."
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
