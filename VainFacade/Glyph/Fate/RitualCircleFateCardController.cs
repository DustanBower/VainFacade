using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class RitualCircleFateCardController:RitualCircleCardController
	{
		public RitualCircleFateCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}
	}
}

