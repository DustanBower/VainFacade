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
    public class BatCardController : CardController
    {
        public BatCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }
    }
}
