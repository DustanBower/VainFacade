using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class FoldingForcesCardController:PushCardControllerUtilities
	{
		public FoldingForcesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //Increase projectile damage dealt by Push by 2.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && dd.DamageType == DamageType.Projectile, 2);

            //At the start of your turn, destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }
    }
}

