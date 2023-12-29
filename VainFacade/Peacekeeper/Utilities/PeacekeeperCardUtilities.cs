using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class PeacekeeperCardUtilities:CardController
	{
		public PeacekeeperCardUtilities(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

		public bool IsSymptom(Card c)
		{
			return base.GameController.DoesCardContainKeyword(c, "symptom");
		}

        public bool IsManeuver(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "maneuver");
        }

        public bool IsSerum(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "serum");
        }

        public LinqCardCriteria ManeuverCriteria()
        {
            return new LinqCardCriteria((Card c) => IsManeuver(c), "", false, false, "maneuver", "maneuvers");
        }

        public LinqCardCriteria SymptomCriteria()
        {
            return new LinqCardCriteria((Card c) => IsSymptom(c), "", false, false, "symptom", "symptom");
        }
    }
}

