using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class ParadiseIsleUtilityCardController : CardController
    {
        public ParadiseIsleUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public static readonly string ConspiratorKeyword = "conspirator";

        public static LinqCardCriteria isConspirator = new LinqCardCriteria((Card c) => c.DoKeywordsContain(ConspiratorKeyword), "", false, false, "Conspirator", "Conspirators");
        public static LinqCardCriteria isConspiratorInPlay = new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain(ConspiratorKeyword), "Conspirator in play", false, false, "Conspirator in play", "Conspirators in play");
    }
}
