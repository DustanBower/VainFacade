using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheMidnightBazaar
{
    public abstract class ThreenCardController : TheMidnightBazaarUtilityCardController
    {
        public ThreenCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt by this card to 0."
            AddReduceDamageToSetAmountTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card == base.Card, 0);
        }
    }
}
