using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra;

namespace VainFacadePlaytest.Push
{
	public class ObjectsInMotionCardController:PushCardControllerUtilities
	{
		public ObjectsInMotionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //When you discard a card from your hand, put it under this card.
            //Based on Inferno Missile Pod
            AddTrigger((MoveCardAction mc) => mc.Origin == base.HeroTurnTaker.Hand && mc.IsDiscard && mc.CanChangeDestination, PutUnderThisCardResponse, TriggerType.MoveCard, TriggerTiming.Before);
            AddBeforeLeavesPlayActions(ReturnCardsToOwnersTrashResponse);

            //At the end of your turn, discard all cards beneath this card. Push deals 1 target 2 projectile damage for each card discarded this way.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, EndOfTurnResponse, new TriggerType[1] { TriggerType.DealDamage });
        }

        private IEnumerator PutUnderThisCardResponse(MoveCardAction mc)
        {
            mc.SetDestination(base.Card.UnderLocation);
            yield return null;
        }

        private IEnumerator ReturnCardsToOwnersTrashResponse(GameAction gameAction)
        {
            while (base.Card.UnderLocation.Cards.Count() > 0)
            {
                Card topCard = base.Card.UnderLocation.TopCard;
                MoveCardDestination trashDestination = FindCardController(topCard).GetTrashDestination();
                GameController gameController = base.GameController;
                TurnTakerController turnTakerController = base.TurnTakerController;
                Location location = trashDestination.Location;
                bool toBottom = trashDestination.ToBottom;
                CardSource cardSource = GetCardSource();
                IEnumerator coroutine = gameController.MoveCard(turnTakerController, topCard, location, toBottom, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, null, null, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
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

        private IEnumerator EndOfTurnResponse(PhaseChangeAction p)
        {
            SelectCardsDecision decision = new SelectCardsDecision(base.GameController, DecisionMaker, (Card c) => this.Card.UnderLocation.Cards.Contains(c), SelectionType.DiscardCard, null, false, null, true, true, cardSource: GetCardSource());
            IEnumerator coroutine = base.GameController.SelectCardsAndDoAction(decision, DiscardAndDamage, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator DiscardAndDamage(SelectCardDecision scd)
        {
            Card card = scd.SelectedCard;
            MoveCardDestination trashDestination = FindCardController(card).GetTrashDestination();
            List<MoveCardAction> results = new List<MoveCardAction>();
            IEnumerator coroutine = base.GameController.MoveCard(this.TurnTakerController, card, trashDestination.Location,storedResults: results, responsibleTurnTaker: this.TurnTaker, isDiscard: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidMoveCard(results) && results.FirstOrDefault().IsDiscard)
            {
                coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), 2, DamageType.Projectile, 1, false, 1, cardSource: GetCardSource());
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
    }
}

