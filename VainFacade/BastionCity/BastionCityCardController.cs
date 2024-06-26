using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.BastionCity
{
	public class BastionCityCardController:CardController
	{
		public BastionCityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
		{
		}

		public bool IsMachination(Card c)
		{
			return base.GameController.DoesCardContainKeyword(c, "machination");
		}

        public LinqCardCriteria MachinationCriteria()
        {
            return new LinqCardCriteria((Card c) => IsMachination(c), "machination");
        }

		public bool IsCoalition(Card c)
		{
            return base.GameController.DoesCardContainKeyword(c, "coalition");
        }

        public bool IsKeeper(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "keeper");
        }

        public bool IsCivilian(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "civilian");
        }
    }
}

