using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.EldrenwoodVillage
{
    public class EldrenwoodUtilityCardController : CardController
    {
        public EldrenwoodUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        protected const string HowlsKey = "HowlsEffectKey";
        protected const string QuaintKey = "QuaintEffectKey";
    }
}
