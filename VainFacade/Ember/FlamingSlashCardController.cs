using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Ember
{
    public class FlamingSlashCardController : CardController
    {
        public FlamingSlashCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "{EmberCharacter} deals up to 3 targets 2 melee and 1 fire damage each."
            List<DealDamageAction> instances = new List<DealDamageAction>();
            DealDamageAction twoMelee = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 2, DamageType.Melee);
            DealDamageAction oneFire = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 1, DamageType.Fire);
            instances.Add(twoMelee);
            instances.Add(oneFire);
            IEnumerator damageCoroutine = SelectTargetsAndDealMultipleInstancesOfDamage(instances, (Card c) => c.IsTarget, minNumberOfTargets: 0, maxNumberOfTargets: 3);
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
