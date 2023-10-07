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
    public class InfernalTwinCardController : CardController
    {
        public InfernalTwinCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            //AddThisCardControllerToList(CardControllerListType.CanCauseDamageOutOfPlay);
            //base.SpecialStringMaker.ShowSpecialString(() => played.Select((Card[] C) => C.ElementAt(0).Title + ";" + C.ElementAt(1).Title).ToCommaList()).Condition = () => played != null;
        }

        //played = <this card, one-shot played>
        //Needs to log this card so that Grim Reflection copies it correctly
        private List<Card[]> played;

        private const string IsReacting = "IsReactingToOneShot";

        public override void AddTriggers()
        {
            base.AddTriggers();
            played = new List<Card[]>();
            // "When a One-Shot enters play, you may destroy this card. If you do, resolve the text of that One-Shot a second time, then the character from that card's deck with the lowest HP deals itself 2 irreducible infernal damage."
            //AddTrigger((CardEntersPlayAction cepa) => cepa.CardEnteringPlay.IsOneShot && !HasBeenSetToTrueThisTurn(IsReacting) && !base.Card.IsBeingDestroyed, OneShotResponse, new TriggerType[] { TriggerType.DestroySelf, TriggerType.PlayCard, TriggerType.DealDamage }, TriggerTiming.After);
            AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cepa) => cepa.CardEnteringPlay.IsOneShot && !HasBeenSetToTrueThisTurn(IsReacting) && !base.Card.IsBeingDestroyed, LogResponse, TriggerType.Hidden, TriggerTiming.After);
            AddTrigger((MoveCardAction mca) => mca.CardToMove.IsOneShot && played != null && played.Any((Card[] C) => C.ElementAt(0) == this.Card && C.ElementAt(1) == mca.CardToMove) && !mca.Destination.IsPlayArea && !HasBeenSetToTrueThisTurn(IsReacting) && !base.Card.IsBeingDestroyed, OneShotResponse, new TriggerType[] { TriggerType.DestroySelf, TriggerType.PlayCard, TriggerType.DealDamage }, TriggerTiming.Before);
            ResetFlagAfterLeavesPlay(IsReacting);
        }

        private IEnumerator LogResponse(CardEntersPlayAction cepa)
        {
            //Console.WriteLine("Infernal Twin logging " + cepa.CardEnteringPlay.Title);
            if (played == null)
            {
                played = new List<Card[]>();
            }
            played.Add(new Card[] {this.Card, cepa.CardEnteringPlay });
            yield return null;
        }

        private IEnumerator OneShotResponse(MoveCardAction mca)
        {
            // "...you may destroy this card."
            //Func<GameAction, IEnumerator> action = AddBeforeDestroyAction((GameAction ga) => PlayAgainInfernalResponse(mca));
            //List<DestroyCardAction> destructionResults = new List<DestroyCardAction>();
            //IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, optional: true, storedResults: destructionResults, responsibleCard: base.Card, cardSource: GetCardSource());
            //if (base.UseUnityCoroutines)
            //{
            //    yield return base.GameController.StartCoroutine(destructCoroutine);
            //}
            //else
            //{
            //    base.GameController.ExhaustCoroutine(destructCoroutine);
            //}
            //RemoveDestroyAction(BeforeOrAfter.Before, action);

            played.RemoveAll((Card[] C) => C.ElementAt(0) == this.Card && C.ElementAt(1) == mca.CardToMove);

            List<YesNoCardDecision> results = new List<YesNoCardDecision>();
            IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.DestroyCard, this.Card, null, results, new Card[] { mca.CardToMove }, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidPlayerAnswerYes(results))
            {
                Func<GameAction, IEnumerator> action = AddBeforeDestroyAction((GameAction ga) => PlayAgainInfernalResponse(mca));
                //coroutine = base.GameController.DestroyCard(DecisionMaker, this.Card, postDestroyAction: () => PlayAgainInfernalResponse(mca), cardSource: GetCardSource());
                coroutine = base.GameController.DestroyCard(DecisionMaker, this.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                RemoveDestroyAction(BeforeOrAfter.Before, action);
            }
        }

        private IEnumerator PlayAgainInfernalResponse(MoveCardAction mca)
        {
            // "If you do, resolve the text of that One-Shot a second time, ..."
            SetCardPropertyToTrueIfRealAction(IsReacting);
            played.RemoveAll((Card[] C) => C.ElementAt(0) == this.Card);
            //if (this.Card == this.CardWithoutReplacements)
            //{
            //    //If this is the response for the real Infernal Twin and not Grim Reflection, clear all logged one-shots
            //    played.Clear();
            //}
            Card oneshot = mca.CardToMove;
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.Card.Title + " resolves the text of " + oneshot.Title + " an additional time...", Priority.High, GetCardSource(), associatedCards: oneshot.ToEnumerable());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            IEnumerator playAgainCoroutine = FindCardController(oneshot).Play();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playAgainCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playAgainCoroutine);
            }
            // "... then the character from that card's deck with the lowest HP deals itself 2 irreducible infernal damage."
            List<Card> lowestResults = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithLowestHitPoints(1, (Card c) => c.Owner == oneshot.Owner && c.IsCharacter && c.IsInPlayAndHasGameText, lowestResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            Card lowest = lowestResults.FirstOrDefault();
            if (lowest != null)
            {
                IEnumerator infernalCoroutine = base.GameController.DealDamageToSelf(DecisionMaker, (Card c) => c == lowest, 2, DamageType.Infernal, isIrreducible: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(infernalCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(infernalCoroutine);
                }
            }
        }
    }
}
