using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class SymptomCardController:CardController
	{
		public SymptomCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //At the end of your turn, {Peacekeeper} deals himself 1 irreducible toxic damage.
            //AddDealDamageAtEndOfTurnTrigger(this.TurnTaker, this.CharacterCard, (Card c) => c == this.CharacterCard, TargetType.All, 1, DamageType.Toxic, true);
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, (PhaseChangeAction pca) => DealDamage(this.CharacterCard, this.CharacterCard, 1, DamageType.Toxic, true, cardSource: GetCardSource()), TriggerType.DealDamage);
        }
    }
}

