using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
    public class BatCardController : CardController
    {
        public BatCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // When this card is destroyed, destroy the card it's next to (if any) and move it to OffToTheSide
            AddAfterDestroyedAction(CleanUpResponse);
            // If the card this card is next to leaves play, destroy this card
            Func<BulkMoveCardsAction, IEnumerator> response2 = delegate (BulkMoveCardsAction m)
            {
                Card card = m.CardsToMove.First((Card c) => IsThisCardNextToCard(c));
                return DestroyThisCardResponse(m);
            };
            AddTrigger((BulkMoveCardsAction m) => m.CardsToMove.Any((Card c) => IsThisCardNextToCard(c)) && !m.Destination.IsInPlayAndNotUnderCard, response2, TriggerType.MoveCard, TriggerTiming.After);
            AddTrigger((MoveCardAction d) => IsThisCardNextToCard(d.CardToMove) && !d.CardToMove.Location.IsInPlayAndNotUnderCard, (MoveCardAction m) => DestroyThisCardResponse(m), TriggerType.MoveCard, TriggerTiming.After);
        }

        private IEnumerator CleanUpResponse(GameAction ga)
        {
            DestroyCardAction d = ga as DestroyCardAction;
            // ... destroy the card it's next to (if any)...
            Card anchor = GetCardThisCardIsNextTo();
            if (anchor != null && anchor.IsInPlay)
            {
                IEnumerator destroyCoroutine = base.GameController.DestroyCard(DecisionMaker, anchor, actionSource: d);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            // ... and move it to OffToTheSide
            d.PreventMoveToTrash = true;
            IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.OffToTheSide, actionSource: d);
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
}
