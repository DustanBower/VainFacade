using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class SomeoneInNeedCardController:RoleCardController
	{
		public SomeoneInNeedCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
		}
	}
}

