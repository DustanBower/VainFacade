using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class ToxicBloodCardController:SymptomCardController
	{
		public ToxicBloodCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //Increase damage dealt by {Peacekeeper} to other targets by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && dd.Target != this.CharacterCard, 1);

            base.AddTriggers();
        }
    }
}

