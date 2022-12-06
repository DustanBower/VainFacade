using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Carnaval
{
    public class CarnavalUtilityCardController : CardController
    {
        public CarnavalUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        protected static readonly string MasqueKeyword = "masque";
        protected static readonly string TrapKeyword = "trap";

        protected static LinqCardCriteria MasqueCard = new LinqCardCriteria((Card c) => c.DoKeywordsContain(MasqueKeyword), "Masque");
        protected static LinqCardCriteria TrapCard = new LinqCardCriteria((Card c) => c.DoKeywordsContain(TrapKeyword), "Trap");
        protected static LinqCardCriteria MasqueInPlay = new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain(MasqueKeyword), "Masque", singular: "card in play", plural: "cards in play");
    }
}
