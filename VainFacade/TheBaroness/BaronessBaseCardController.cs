using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
	public class BaronessBaseCardController:CardController
	{
		public BaronessBaseCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override bool CanBePlayedFromLocation()
        {
            if (base.FindCardController(base.CharacterCard) is TheBaronessSpiderCharacterCardController && !this.CharacterCard.IsFlipped && this.Card.Location == this.TurnTaker.Deck && this.Card.Identifier != "Vampirism")
            {
                return false;
            }
            return base.CanBePlayedFromLocation();
        }
    }
}

