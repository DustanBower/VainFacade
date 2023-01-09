using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Ember
{
    public class EmberUtilityCardController : CardController
    {
        public EmberUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        protected static readonly string BlazeKeyword = "blaze";

        protected static LinqCardCriteria BlazeCard = new LinqCardCriteria((Card c) => c.DoKeywordsContain(BlazeKeyword), "Blaze");
        protected static LinqCardCriteria BlazeCardInPlay = new LinqCardCriteria((Card c) => c.DoKeywordsContain(BlazeKeyword) && c.IsInPlayAndHasGameText, "Blaze", singular: "card in play", plural: "cards in play");

        protected int NumBlazeCardsInPlay()
        {
            return base.GameController.FindCardsWhere(BlazeCardInPlay, visibleToCard: GetCardSource()).Count();
        }
    }
}
