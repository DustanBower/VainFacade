using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class DirgeCardController:CardController
	{
		public DirgeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => IsDirge(c), "dirges", false, false, "dirge", "dirge")).Condition = () => !this.Card.IsInPlay;
		}

        public override IEnumerator Play()
        {
            //When this card enters play, return all of your other dirges in play to your hand.
            IEnumerator coroutine = base.GameController.MoveCards(this.TurnTakerController, FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsDirge(c) && c != this.Card), this.HeroTurnTaker.Hand, playIfMovingToPlayArea: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            //Play this card next to a target.
            IEnumerator coroutine = SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => c.IsTarget, "",false,false,"target","targets"), storedResults, isPutIntoPlay, decisionSources);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override void AddTriggers()
        {
            //At the end of that target's turn, if this card did not enter play this turn, play up to 1 card and return this card to your hand.
            AddEndOfTurnTrigger((TurnTaker tt) => GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().Owner == tt && !DidThisCardEnterPlayThisTurn(), EndOfTurnResponse, new TriggerType[2] { TriggerType.PlayCard, TriggerType.MoveCard });
            AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(false, GetCardThisCardIsNextTo() != null && !GetCardThisCardIsNextTo().IsCharacter);
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            IEnumerator coroutine = SelectAndPlayCardFromHand(DecisionMaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (this.Card.IsInPlayAndHasGameText)
            {
                coroutine = base.GameController.MoveCard(this.TurnTakerController, this.Card, this.HeroTurnTaker.Hand, playCardIfMovingToPlayArea: false, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
        }

        private bool IsDirge(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "dirge");
        }

        private bool DidThisCardEnterPlayThisTurn()
        {
            return base.GameController.Game.Journal.QueryJournalEntries((CardEntersPlayJournalEntry e) => e.Card == this.Card).Where(base.GameController.Game.Journal.ThisTurn<CardEntersPlayJournalEntry>()).Any();
        }

        public override IEnumerable<Card> FilterDecisionCardChoices(SelectCardDecision decision)
        {
            if (decision.SelectionType == SelectionType.MoveCardNextToCard && decision.Choices.Where((Card c) => !IsHeroTarget(c)).Count() > 0)
            {
                return decision.Choices.Where((Card c) => IsHeroTarget(c));
            }
            return null;
        }
    }
}

