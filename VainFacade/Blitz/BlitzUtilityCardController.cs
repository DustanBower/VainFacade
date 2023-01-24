using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Blitz
{
    public class BlitzUtilityCardController : CardController
    {
        public BlitzUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        protected const string CircuitKeyword = "circuit";
        protected const string PlaybookKeyword = "playbook";

        public LinqCardCriteria IsCircuit = new LinqCardCriteria((Card c) => c.DoKeywordsContain(CircuitKeyword), "Circuit");
    }
}
