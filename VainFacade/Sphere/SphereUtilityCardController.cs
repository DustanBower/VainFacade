using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class SphereUtilityCardController : CardController
    {
        public SphereUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public static readonly string emanationKeyword = "emanation";

        public static LinqCardCriteria isEmanation = new LinqCardCriteria((Card c) => c.DoKeywordsContain(emanationKeyword), "Emanation");
        public static LinqCardCriteria isEmanationInPlay = new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain(emanationKeyword), "Emanation in play", false, false, "Emanation in play", "Emanations in play");
    }
}
