using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class DoomsayerTurnTakerController:TurnTakerController
	{
		public DoomsayerTurnTakerController(TurnTaker turnTaker, GameController gameController) : base(turnTaker, gameController)
        {
		}

        public override IEnumerator StartGame()
        {
            //Put Countless Words into play.
            Card words = base.TurnTaker.GetCardByIdentifier("CountlessWords");
            IEnumerator putCoroutine = base.GameController.PlayCard(this, words, isPutIntoPlay: true, cardSource: new CardSource(base.CharacterCardController));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(putCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(putCoroutine);
            }

            //Shuffle Acolytes of the Black Thorn, Bladewight, and Ingini into the villain deck.
            IEnumerator coroutine = base.GameController.ShuffleLocation(this.TurnTaker.Deck);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Reveal cards from the top of the villain deck until a target and proclamation are revealed.
            //Put the first revealed card of each type into play. Shuffle the rest into the villain deck.
            Card first = TurnTaker.Deck.Cards.Where((Card c) => c.IsTarget || IsProclamation(c)).Take(1).FirstOrDefault();
            Card second = TurnTaker.Deck.Cards.Where((Card c) => IsProclamation(first) ? c.IsTarget : IsProclamation(c)).Take(1).FirstOrDefault();

            coroutine = base.GameController.PlayCard(this, first, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.GameController.PlayCard(this, second, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.GameController.ShuffleLocation(this.TurnTaker.Deck);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private bool IsProclamation(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "proclamation");
        }
    }
}

