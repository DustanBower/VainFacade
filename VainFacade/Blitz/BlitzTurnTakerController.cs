using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Blitz
{
    public class BlitzTurnTakerController : TurnTakerController
    {
        public BlitzTurnTakerController(TurnTaker turnTaker, GameController gameController) : base(turnTaker, gameController)
        {

        }

        protected const string CircuitKeyword = "circuit";
        public LinqCardCriteria IsCircuit = new LinqCardCriteria((Card c) => c.DoKeywordsContain(CircuitKeyword), "Circuit");

        public override IEnumerator StartGame()
        {
            if (base.CharacterCardController is BlitzCharacterCardController)
            {
                // "Shuffle the villain deck."
                IEnumerator shuffleCoroutine = base.GameController.ShuffleLocation(base.TurnTaker.Deck, cardSource: new CardSource(base.CharacterCardController));
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shuffleCoroutine);
                }
                // "Reveal cards from the top of the villain deck until a Circuit is revealed. Put it into play. Shuffle the remaining cards into the deck."
                IEnumerator circuitCoroutine = PutCardsIntoPlay(IsCircuit, 1, true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(circuitCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(circuitCoroutine);
                }
            }
            yield break;
        }
    }
}
