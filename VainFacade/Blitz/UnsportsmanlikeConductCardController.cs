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
    public class UnsportsmanlikeConductCardController : CardController
    {
        public UnsportsmanlikeConductCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero target with lowest HP
            SpecialStringMaker.ShowHeroTargetWithLowestHP();
        }

        public override IEnumerator Play()
        {
            // "{BlitzCharacter} deals the hero target with the lowest HP {H - 1} melee damage and {H - 1} lightning damage."
            List<DealDamageAction> instances = new List<DealDamageAction>();
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, H - 1, DamageType.Melee));
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, H - 1, DamageType.Lightning));
            IEnumerator damageCoroutine = DealMultipleInstancesOfDamageToHighestLowestHP(instances, (Card c) => IsHeroTarget(c), HighestLowestHP.LowestHP);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
        }
    }
}
