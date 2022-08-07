using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
    public class TheBaronessTurnTakerController : TurnTakerController
    {
        public TheBaronessTurnTakerController(TurnTaker turnTaker, GameController gameController): base(turnTaker, gameController)
        {

        }

        public override IEnumerator StartGame()
        {
            // "Put the card Vampirism into play."
            Card vampirism = base.TurnTaker.GetCardByIdentifier("Vampirism");
            IEnumerator putCoroutine = base.GameController.PlayCard(this, vampirism, isPutIntoPlay: true, cardSource: new CardSource(base.CharacterCardController));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(putCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(putCoroutine);
            }
            IEnumerator shuffleCoroutine = base.GameController.ShuffleLocation(base.TurnTaker.Deck, cardSource: new CardSource(base.CharacterCardController));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }
            /*// "Flip {TheBaroness}'s character card."
            IEnumerator flipcoroutine = base.GameController.FlipCard(base.CharacterCardController, cardSource: new CardSource(base.CharacterCardController));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(flipcoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(flipcoroutine);
            }*/
        }
    }
}
