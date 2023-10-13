using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class ArctisCardUtilities:CardController
	{
		public ArctisCardUtilities(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

		public bool IsIcework(Card c)
		{
			return base.GameController.DoesCardContainKeyword(c, "icework");
		}
	}
}

