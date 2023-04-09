using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Blitz
{
    public class EMChargeStabilizerCardController : CardController
    {
        public EMChargeStabilizerCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero target with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase lightning damage dealt by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageType == DamageType.Lightning, (DealDamageAction dda) => 1);
            // "When {BlitzCharacter} is dealt lightning damage, he regains 3 HP."
            AddTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && dda.DamageType == DamageType.Lightning && dda.DidDealDamage, (DealDamageAction dda) => base.GameController.GainHP(base.CharacterCard, 3, cardSource: GetCardSource()), TriggerType.GainHP, TriggerTiming.After);
            // "At the end of the villain turn, {BlitzCharacter} deals the hero target with the highest HP 1 lightning damage."
            AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.CharacterCard, (Card c) => IsHeroTarget(c), TargetType.HighestHP, 1, DamageType.Lightning);
        }
    }
}
