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
    public class ScorchingShriekCardController : CardController
    {
        public ScorchingShriekCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "{EmberCharacter} deals each non-hero target 1 sonic damage and 1 fire damage."
            List<DealDamageAction> instances = new List<DealDamageAction>();
            DealDamageAction oneSonic = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 1, DamageType.Sonic);
            DealDamageAction oneFire = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 1, DamageType.Fire);
            instances.Add(oneSonic);
            instances.Add(oneFire);
            IEnumerator damageCoroutine = DealMultipleInstancesOfDamage(instances, (Card c) => c.IsTarget && !IsHeroTarget(c));
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
