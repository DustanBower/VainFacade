using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class PushTurnTakerController:HeroTurnTakerController
	{
		public PushTurnTakerController(TurnTaker turnTaker, GameController gameController)
            : base(turnTaker, gameController)
        {
		}

        public override bool AlwaysAskForDraw => FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && base.GameController.DoesCardContainKeyword(c, "anchor") && c.Owner == this.TurnTaker).Any();
    }
}

