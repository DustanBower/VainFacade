using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class LeaderCardController : CardController
    {
        public LeaderCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero target with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP(ranking: 1);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt by hero targets by 1."
            AddReduceDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsTarget && IsHeroTarget(dda.DamageSource.Card), (DealDamageAction dda) => 1);
            // "At the end of the villain turn, this card deals the hero target with the highest HP {H - 2} projectile damage."
            AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => IsHeroTarget(c), TargetType.HighestHP, H - 2, DamageType.Projectile);
        }
    }
}
