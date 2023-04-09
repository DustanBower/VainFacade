using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
    public class EndlessThirstCardController : BaronessUtilityCardController
    {
        public EndlessThirstCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Each time {TheBaroness} deals a hero target melee damage, put the top card of that target's deck face-down in the villain play area."
            AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card == base.CharacterCard && dda.DamageType == DamageType.Melee && dda.DidDealDamage && IsHeroTarget(dda.Target), (DealDamageAction dda) => MoveFaceDownToVillainPlayArea(GetNativeDeck(dda.Target).TopCard), TriggerType.MoveCard, TriggerTiming.After);
        }
    }
}
