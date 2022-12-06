using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Carnaval
{
    public class CombatOperativeCardController : CardController
    {
        public CombatOperativeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "{CarnavalCharacter} deals up to 3 targets 1 melee and 1 psychic damage each."
            List<DealDamageAction> instances = new List<DealDamageAction>();
            DealDamageAction oneMelee = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 1, DamageType.Melee);
            DealDamageAction onePsychic = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 1, DamageType.Psychic);
            instances.Add(oneMelee);
            instances.Add(onePsychic);
            IEnumerator damageCoroutine = SelectTargetsAndDealMultipleInstancesOfDamage(instances, (Card c) => c.IsTarget, minNumberOfTargets: 0, maxNumberOfTargets: 3);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            yield break;
        }
    }
}
