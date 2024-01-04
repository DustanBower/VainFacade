using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class ReboundPulseCardController:PushCardControllerUtilities
	{
		public ReboundPulseCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //When Push deals melee or projectile damage, draw a card
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && (dd.DamageType == DamageType.Melee || dd.DamageType == DamageType.Projectile) && dd.DidDealDamage, (DealDamageAction dd) => DrawCard(), TriggerType.DrawCard, TriggerTiming.After);

            //At the start of your turn, destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }
    }
}

