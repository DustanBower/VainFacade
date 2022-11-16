using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Node
{
    abstract public class ConnectionCardController : NodeUtilityCardController
    {
        public ConnectionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show location of this card
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria(base.Card), specifyPlayAreas: true).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        abstract public bool IsValidPlayArea(TurnTaker tt);
    }
}
