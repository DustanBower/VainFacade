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
    public class VampiricStrengthCardController : CardController
    {
        public VampiricStrengthCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero target with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP(ranking: 1, numberOfTargets: 1);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase melee and projectile damage dealt by villain targets by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card.IsVillainTarget && (dda.DamageType == DamageType.Melee || dda.DamageType == DamageType.Projectile), 1);
            // "At the end of the villain turn, {TheBaroness} deals the hero target with the highest HP {H} melee damage."
            AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.CharacterCard, (Card c) => c.IsHero, TargetType.HighestHP, H, DamageType.Melee);
        }
    }
}
