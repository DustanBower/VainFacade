using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class SympatheticCastingDestructionCardController:SympatheticCastingCardController
	{
		public SympatheticCastingDestructionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}
	}
}

