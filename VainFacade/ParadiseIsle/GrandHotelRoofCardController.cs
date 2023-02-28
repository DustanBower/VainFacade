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
    public class GrandHotelRoofCardController : AreaCardController
    {
        public GrandHotelRoofCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase damage dealt by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => true, 1);
            // "At the start of the environment turn, each player may discard a card to destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => EachPlayerMayDiscardOneCardToPerformAction(pca, base.GameController.DestroyCard(DecisionMaker, base.Card, responsibleCard: base.Card, cardSource: GetCardSource()), "destroy " + base.Card.Title), new TriggerType[] { TriggerType.DiscardCard, TriggerType.DestroyCard });
            // "At the end of the environment turn, 1 player discards a card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => base.GameController.SelectHeroToDiscardCard(DecisionMaker, optionalDiscardCard: false, cardSource: GetCardSource()), TriggerType.DiscardCard);
        }
    }
}
