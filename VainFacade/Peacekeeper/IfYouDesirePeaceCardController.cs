using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class IfYouDesirePeaceCardController:PeacekeeperCardUtilities
	{
		public IfYouDesirePeaceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => IsEquipment(c), "equipment"));
        }

        public override IEnumerator Play()
        {
            //Reveal cards from the top of your deck until 2 equipment cards are revealed. Put one into your hand or into play. Shuffle the other revealed cards into your deck.
            IEnumerator coroutine = RevealCards_SelectSome_MoveThem_ReturnTheRestEx(DecisionMaker, this.TurnTakerController, this.TurnTaker.Deck, (Card c) => IsEquipment(c), 2, 1, true, true, true, false, "equipment cards");
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

