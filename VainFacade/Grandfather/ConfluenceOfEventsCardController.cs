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
    public class ConfluenceOfEventsCardController : GrandfatherUtilityCardController
    {
        public ConfluenceOfEventsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "Play the top card of the environment deck."
            IEnumerator playEnvCoroutine = PlayTheTopCardOfTheEnvironmentDeckResponse(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playEnvCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playEnvCoroutine);
            }
            // "Reveal cards from the top of the villain deck until a target is revealed. Put it into play. Discard the rest."
            IEnumerator revealCoroutine = RevealMoveDiscardDontShuffle(DecisionMaker, base.TurnTakerController, base.TurnTaker.Deck, (Card c) => c.IsTarget, 1, 1, false, true, true, "target", responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
        }
    }
}
