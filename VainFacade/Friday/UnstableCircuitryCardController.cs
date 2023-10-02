using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Friday
{
	public class UnstableCircuitryCardController:CardController
	{
		public UnstableCircuitryCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.EnteringGameCheck);
            AddInhibitorException((GameAction ga) => ga is PlayCardAction && base.Card.Location.IsHand);
        }

        //When this card enters your hand, put it into play.
        //Copied from Monster of Id
        private IEnumerator PlayFromHandResponse()
        {
            IEnumerator coroutine = base.GameController.SendMessageAction(base.Card.Title + " puts itself into play.", Priority.High, GetCardSource(), null, showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            GameController gameController = base.GameController;
            TurnTakerController turnTakerController = base.TurnTakerController;
            Card card = base.Card;
            CardSource cardSource = GetCardSource();
            coroutine = gameController.PlayCard(turnTakerController, card, isPutIntoPlay: true, null, optional: false, null, null, evenIfAlreadyInPlay: false, null, null, null, associateCardSource: false, fromBottom: false, canBeCancelled: true, cardSource);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override IEnumerator PerformEnteringGameResponse()
        {
            IEnumerator coroutine = ((!base.Card.IsInHand) ? base.PerformEnteringGameResponse() : PlayFromHandResponse());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override void AddStartOfGameTriggers()
        {
            AddTrigger((DrawCardAction d) => d.DrawnCard == base.Card, (DrawCardAction d) => PlayFromHandResponse(), new TriggerType[2]
            {
            TriggerType.PutIntoPlay,
            TriggerType.Hidden
            }, TriggerTiming.After, null, isConditional: false, requireActionSuccess: true, null, outOfPlayTrigger: true);
            AddTrigger((MoveCardAction m) => m.Destination == base.HeroTurnTaker.Hand && m.CardToMove == base.Card, (MoveCardAction m) => PlayFromHandResponse(), new TriggerType[2]
            {
            TriggerType.PutIntoPlay,
            TriggerType.Hidden
            }, TriggerTiming.After, null, isConditional: false, requireActionSuccess: true, null, outOfPlayTrigger: true);
        }

        public override IEnumerator Play()
        {
            //{Friday} deals herself 1 irreducible lightning damage.
            IEnumerator coroutine = DealDamage(this.CharacterCard, this.CharacterCard, 1, DamageType.Lightning, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //For some reason, if the damage incaps Friday, it still asks to draw 2 cards unless this check is here.
            if (!this.TurnTaker.IsIncapacitatedOrOutOfGame)
            {
                //You may draw 2 cards. If you do, shuffle this card into your deck.
                List<DrawCardAction> results = new List<DrawCardAction>();
                coroutine = DrawCards(DecisionMaker, 2, true, false, results, false);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidDrawCards(results))
                {
                    coroutine = base.GameController.ShuffleCardIntoLocation(DecisionMaker, this.Card, this.TurnTaker.Deck, false, cardSource: GetCardSource());
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


        public override bool DoNotMoveOneShotToTrash
        {
            get
            {
                if (base.Card.Location == this.TurnTaker.Deck)
                {
                    return true;
                }
                return base.DoNotMoveOneShotToTrash;
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText(
            $"Do you want to draw 2 cards?",
            $"{decision.DecisionMaker.Name} is deciding whether to draw 2 cards.",
            $"Vote for whether " + this.TurnTaker.Name + " will draw 2 cards.",
            $"whether to draw 2 cards."
            );
        }
    }
}

