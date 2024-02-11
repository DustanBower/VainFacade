using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class CountlessWordsCardController:CardController
	{
		public CountlessWordsCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
		}
	}
}

