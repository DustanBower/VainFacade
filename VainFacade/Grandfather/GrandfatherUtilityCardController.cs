using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class GrandfatherUtilityCardController : CardController
    {
        public GrandfatherUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        protected const string CovertKeyword = "covert";
        protected const string DesignKeyword = "design";
        protected const string FulcrumKeyword = "fulcrum";
    }
}
