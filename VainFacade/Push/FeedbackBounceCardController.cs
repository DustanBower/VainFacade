using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class FeedbackBounceCardController:PushCardControllerUtilities
	{
		public FeedbackBounceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => IsOngoing(c) && c.Owner != this.TurnTaker, "ongoing"), null,null," belonging to other heroes",false);
            base.SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => IsOngoing(c) && c != this.Card, "other ongoing"));
		}

		public override void AddTriggers()
		{
			//When Push deals damage, you may discard 2 cards.
            //If 2 cards were discarded this way, destroy this card or put an ongoing from in play under this card.
            //Otherwise, put another hero's ongoing from in play under this card.
			AddTrigger<DealDamageAction>((DealDamageAction dd) => !this.Card.IsBeingDestroyed && dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard, DamageResponse, new TriggerType[3] { TriggerType.MoveCard, TriggerType.DiscardCard, TriggerType.DestroySelf }, TriggerTiming.After);

			//When this card is destroyed, Push deals X targets 4 projectile damage each, where X = the number of cards beneath this one.
			AddWhenDestroyedTrigger(DestroyedResponse, TriggerType.DealDamage);

			//At the start of your turn, destroy this card.
			AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
		}

		private IEnumerator DestroyedResponse(DestroyCardAction dc)
		{
			IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), 4, DamageType.Projectile, this.Card.UnderLocation.NumberOfCards, false, this.Card.UnderLocation.Cards.Count(), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

		private IEnumerator DamageResponse(DealDamageAction dd)
		{
            //When Push deals damage, you may discard 2 cards.
            List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
            IEnumerator coroutine;
            if (this.HeroTurnTaker.Hand.NumberOfCards > 0)
            {
                coroutine = SelectAndDiscardCards(DecisionMaker, 2, true, 2, discardResults);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }

            if (DidDiscardCards(discardResults, 2))
            {
                //If 2 cards were discarded this way, destroy this card or put an ongoing from in play under this card.
                IEnumerable<Function> functionChoices = new Function[2]
                {
                    new Function(base.HeroTurnTakerController, "Destroy this card", SelectionType.DestroySelf, () => DestroyThisCardResponse(null)),
                    new Function(base.HeroTurnTakerController, "Put an ongoing from in play under this card", SelectionType.MoveCard, () => base.GameController.SelectAndMoveCard(DecisionMaker, (Card c) => IsOngoing(c) && c.IsInPlayAndHasGameText && c != this.Card, this.Card.UnderLocation, playIfMovingToPlayArea: false, cardSource: GetCardSource()))
                };

                SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, false, null, null, null, GetCardSource());
                IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(choose);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(choose);
                }
            }
            else
            {
                //Otherwise, put another hero's ongoing from in play under this card.
                coroutine = base.GameController.SendMessageAction("Less than 2 cards were discarded, so " + this.TurnTaker.Name + " puts another hero's ongoing from in play under this card.", Priority.Low, GetCardSource(),showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                coroutine = base.GameController.SelectAndMoveCard(DecisionMaker, (Card c) => IsOngoing(c) && c.IsInPlayAndHasGameText && c.Owner.IsPlayer && c.Owner != this.TurnTaker, this.Card.UnderLocation, playIfMovingToPlayArea: false, cardSource: GetCardSource());
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

