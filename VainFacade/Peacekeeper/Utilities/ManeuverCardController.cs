using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class ManeuverCardController:PeacekeeperCardUtilities
	{
		public ManeuverCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //When this card enters play, return your other maneuvers in play to your hand.
            IEnumerator coroutine = base.GameController.MoveCards(this.TurnTakerController, FindCardsWhere((Card c) => IsManeuver(c) && c.IsInPlayAndHasGameText && c.Owner == this.TurnTaker && c != this.Card), this.HeroTurnTaker.Hand, cardSource: GetCardSource());
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

