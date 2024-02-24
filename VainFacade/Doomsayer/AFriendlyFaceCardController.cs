using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace VainFacadePlaytest.Doomsayer
{
	public class AFriendlyFaceCardController:RoleCardController
	{
		public AFriendlyFaceCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
        }
    }
}

