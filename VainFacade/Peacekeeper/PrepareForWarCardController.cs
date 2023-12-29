using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class PrepareForWarCardController:PeacekeeperCardUtilities
	{
		public PrepareForWarCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //Search your deck for a maneuver and put it into play.
            IEnumerator coroutine = SearchForCards(DecisionMaker, true, false, 1, 1, ManeuverCriteria(), true, false, false, shuffleAfterwards: false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //You may put a maneuver from your trash on top of your deck.
            coroutine = base.GameController.SelectCardFromLocationAndMoveIt(DecisionMaker, this.TurnTaker.Trash, ManeuverCriteria(), new MoveCardDestination[] { new MoveCardDestination(this.TurnTaker.Deck) }, optional: true, cardSource:GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Shuffle your deck.
            coroutine = ShuffleDeck(DecisionMaker, this.TurnTaker.Deck);
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

