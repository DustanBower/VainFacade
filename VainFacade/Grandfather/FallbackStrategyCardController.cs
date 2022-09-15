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
    public class FallbackStrategyCardController : GrandfatherUtilityCardController
    {
        public FallbackStrategyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "Reveal cards from the top of the villain deck until a Design is revealed. Put it into play. Discard the rest."
            IEnumerator findCoroutine = RevealMoveDiscardDontShuffle(DecisionMaker, base.TurnTakerController, base.TurnTaker.Deck, (Card c) => c.DoKeywordsContain(DesignKeyword), 1, 1, false, true, true, "Design", responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
        }
    }
}
