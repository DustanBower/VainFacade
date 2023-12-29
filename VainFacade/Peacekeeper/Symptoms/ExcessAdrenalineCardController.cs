using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class ExcessAdrenalineCardController:SymptomCardController
	{
		public ExcessAdrenalineCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //At the end of your turn, you may draw or play a card.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, (PhaseChangeAction pca) => DrawACardOrPlayACard(DecisionMaker, true), new TriggerType[] { TriggerType.DrawCard, TriggerType.PlayCard });

            base.AddTriggers();
        }
    }
}

