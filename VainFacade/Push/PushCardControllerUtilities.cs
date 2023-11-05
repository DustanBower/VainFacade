using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class PushCardControllerUtilities:CardController
	{
		public PushCardControllerUtilities(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

		public bool IsAlteration(Card c)
		{
			return base.GameController.DoesCardContainKeyword(c, "alteration");
		}

		public bool IsAnchor(Card c)
		{
            return base.GameController.DoesCardContainKeyword(c, "anchor");
        }

		public LinqCardCriteria AlterationCriteria(Func<Card, bool> criteria = null)
		{
			return new LinqCardCriteria((Card c) => (criteria != null ? criteria(c) : true) && IsAlteration(c), "alteration", false, false, "alteration", "alterations");
		}

        public LinqCardCriteria AnchorCriteria(Func<Card, bool> criteria = null)
        {
            return new LinqCardCriteria((Card c) => (criteria != null ? criteria(c) : true) && IsAnchor(c), "anchor", false, false, "anchor", "anchors");
        }
    }
}

