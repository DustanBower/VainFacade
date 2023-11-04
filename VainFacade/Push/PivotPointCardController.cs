using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class PivotPointCardController:PushCardControllerUtilities
	{
		public PivotPointCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowListOfCardsAtLocation(this.TurnTaker.Deck, AnchorCriteria());
		}

        public override IEnumerator Play()
        {
            //Reveal cards from the top of your deck until an anchor is revealed. Put it into your hand or into play. Discard the other revealed cards.
            IEnumerator coroutine = RevealCards_SelectSome_MoveThem_DiscardTheRest(DecisionMaker, this.TurnTakerController, this.TurnTaker.Deck, IsAnchor, 1, 1, true, true, true, "anchor", this.TurnTaker);
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

