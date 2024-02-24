using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class SideCharacterCardController:VillainCharacterCardController
	{
		public SideCharacterCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowIfElseSpecialString(() => base.GameController.IsCardIndestructible(this.Card), () => $"{this.Card.Title} is indestructible.", () => $"{this.Card.Title} is not indestructible.").Condition = () => this.Card.IsInPlayAndHasGameText && base.GameController.IsCardIndestructible(this.Card);
        }
    }
}

