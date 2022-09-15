using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class AcceleratedTimetableCardController : CardController
    {
        public AcceleratedTimetableCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the villain turn, play the top 2 cards of the villain deck and destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, Play2DestructResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.DestroySelf });
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, play the top card of the villain deck."
            IEnumerator playCoroutine = base.GameController.PlayTopCard(DecisionMaker, base.TurnTakerController, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }

        private IEnumerator Play2DestructResponse(PhaseChangeAction pca)
        {
            // "... play the top 2 cards of the villain deck..."
            IEnumerator playCoroutine = base.GameController.PlayTopCard(DecisionMaker, base.TurnTakerController, numberOfCards: 2, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            // "... and destroy this card."
            IEnumerator destructCoroutine = DestroyThisCardResponse(null);
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
