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
    public class UpTheMiddleCardController : PlaybookCardController
    {
        public UpTheMiddleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero target with the second highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP(2);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, {BlitzCharacter} deals the hero target with the second highest HP {H - 2} melee damage and {H - 2} lightning damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, HitZapResponse, TriggerType.DealDamage);
        }

        private IEnumerator HitZapResponse(PhaseChangeAction pca)
        {
            // "... {BlitzCharacter} deals the hero target with the second highest HP {H - 2} melee damage and {H - 2} lightning damage."
            List<DealDamageAction> instances = new List<DealDamageAction>();
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, H - 2, DamageType.Melee));
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, H - 2, DamageType.Lightning));
            IEnumerator damageCoroutine = DealMultipleInstancesOfDamageToHighestLowestHP(instances, (Card c) => c.IsHero && c.IsTarget, HighestLowestHP.HighestHP, 2, 1);
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
