using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class ProclamationCardController:DoomsayerCardUtilities
	{
		public ProclamationCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
		}

        //Doomsayer is indestructible
        public override bool AskIfCardIsIndestructible(Card card)
        {
            if (card == this.CharacterCard)
            {
                return true;
            }
            return base.AskIfCardIsIndestructible(card);
        }
    }
}

