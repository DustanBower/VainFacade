using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class GlyphLimitedCard:GlyphCardUtilities
	{
		public GlyphLimitedCard(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override bool CanBePlayedFromLocation()
        {
            if (FindCardsWhere((Card cc) => cc.Owner == this.TurnTaker && cc.Title == this.Card.Title && cc.IsInPlayAndHasGameText, ignoreBattleZone: true).Any())
            {
                return false;
            }
            return base.CanBePlayedFromLocation();
        }
    }
}