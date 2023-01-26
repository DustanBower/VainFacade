using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Ember
{
    public class ReasonableParanoiaCardController : CardController
    {
        public ReasonableParanoiaCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt to {EmberCharacter} by 1."
            AddReduceDamageTrigger((Card c) => c == base.CharacterCard, 1);
            // "At the start of your turn, draw a card, play a card, and destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DrawPlayDestructResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.PlayCard, TriggerType.DestroySelf });
        }

        private IEnumerator DrawPlayDestructResponse(PhaseChangeAction pca)
        {
            // "... draw a card, ..."
            IEnumerator drawCoroutine = DrawCard(base.TurnTaker.ToHero());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "... play a card, ..."
            IEnumerator playCoroutine = SelectAndPlayCardFromHand(DecisionMaker, optional: false);
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
