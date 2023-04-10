using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class GiselaCaroCardController : CardController
    {
        public GiselaCaroCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.ModifiesDeckKind);
            // Show number of cards in the environment trash
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Trash);
        }

        public override bool? AskIfIsHero(Card card, CardSource cardSource)
        {
            if (card == base.Card)
                return true;
            return base.AskIfIsHero(card, cardSource);
        }

        public override bool? AskIfIsHeroTarget(Card card, CardSource cardSource)
        {
            if (card == base.Card)
                return true;
            return base.AskIfIsHeroTarget(card, cardSource);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt to this card by 3."
            AddReduceDamageTrigger((Card c) => c == base.Card, 3);
            // "Increase damage dealt to this card by 1 for each card in the environment trash."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.Target == base.Card, (DealDamageAction dda) => base.TurnTaker.Trash.NumberOfCards);
            // "When this card is destroyed, remove it from the game."
            AddAfterDestroyedAction((GameAction ga) => base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.OutOfGame, showMessage: true, cardSource: GetCardSource()));
            // "At the end of the environment turn, you may reveal the top card of a deck. Discard it or replace it."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, ReplaceDiscardResponse, TriggerType.RevealCard);
        }

        public IEnumerator ReplaceDiscardResponse(PhaseChangeAction pca)
        {
            // "... you may reveal the top card of a deck."
            List<SelectLocationDecision> storedLocations = new List<SelectLocationDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.RevealTopCardOfDeck, (Location l) => true, storedLocations, optional: true, null, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            Location deck = storedLocations.FirstOrDefault().SelectedLocation.Location;
            if (deck == null)
            {
                yield break;
            }
            List<Card> cards = new List<Card>();
            IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, deck, 1, cards, fromBottom: false, RevealedCardDisplay.None, null, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            Card revealedCard = GetRevealedCard(cards);
            if (revealedCard == null)
            {
                yield break;
            }
            // "Discard it or replace it."
            YesNoDecision yesNo = new YesNoCardDecision(base.GameController, DecisionMaker, SelectionType.DiscardCard, revealedCard, null, null, GetCardSource());
            List<IDecision> decisionSources = new List<IDecision> { yesNo };
            IEnumerator decideCoroutine = base.GameController.MakeDecisionAction(yesNo);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(decideCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(decideCoroutine);
            }
            //Log.Debug("GiselaCaroCardController.ReplaceDiscardResponse: DidPlayerAnswerYes: " + DidPlayerAnswerYes(yesNo).ToString());
            if (DidPlayerAnswerYes(yesNo))
            {
                //Log.Debug("GiselaCaroCardController.ReplaceDiscardResponse: running MoveCard");
                IEnumerator discardCoroutine = base.GameController.MoveCard(base.TurnTakerController, revealedCard, base.TurnTaker.Trash, responsibleTurnTaker: base.TurnTaker, isDiscard: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
            }
            /*Log.Debug("GiselaCaroCardController.ReplaceDiscardResponse: yesNo == null: " + (yesNo == null).ToString());
            if (yesNo != null)
            {
                Log.Debug("GiselaCaroCardController.ReplaceDiscardResponse: yesNo.Completed: " + yesNo.Completed.ToString());
                Log.Debug("GiselaCaroCardController.ReplaceDiscardResponse: yesNo.Answer: " + yesNo.Answer.ToString());
                Log.Debug("GiselaCaroCardController.ReplaceDiscardResponse: yesNo.Answer.HasValue: " + yesNo.Answer.HasValue.ToString());
            }*/
            if (yesNo != null && yesNo.Completed && yesNo.Answer.HasValue)
            {
                decisionSources.Add(yesNo);
                if (!yesNo.Answer.Value)
                {
                    //Log.Debug("GiselaCaroCardController.ReplaceDiscardResponse: running MoveCard to replace revealedCard on top of deck");
                    IEnumerator replaceCoroutine = base.GameController.MoveCard(base.TurnTakerController, revealedCard, deck, toBottom: false, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, decisionSources, base.TurnTaker, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, GetCardSource());
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
            List<Location> list = new List<Location> { deck.OwnerTurnTaker.Revealed };
            //Log.Debug("GiselaCaroCardController.ReplaceDiscardResponse: running CleanupCardsAtLocations");
            IEnumerator cleanupCoroutine = base.GameController.CleanupCardsAtLocations(base.TurnTakerController, list, deck, toBottom: false, addInhibitorException: true, shuffleAfterwards: false, sendMessage: false, isDiscard: false, isReturnedToOriginalLocation: true, cards);
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
