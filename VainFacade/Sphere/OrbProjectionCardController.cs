using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class OrbProjectionCardController : EmanationCardController
    {
        public OrbProjectionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            int numTargets = GetPowerNumeral(0, 1);
            int damageAmt = GetPowerNumeral(1, 1);
            int increaseAmt = GetPowerNumeral(2, 1);
            // "{Sphere} deals 1 target 1 energy damage or increase energy damage dealt by {Sphere} by 1 until the start of your next turn."
            List<Function> options = new List<Function>();
            options.Add(new Function(base.HeroTurnTakerController, "{Sphere} deals " + numTargets + " target " + damageAmt + " energy damage", SelectionType.DealDamage, () => base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), damageAmt, DamageType.Energy, numTargets, false, numTargets, cardSource: GetCardSource())));
            options.Add(new Function(base.HeroTurnTakerController, "Increase energy damage dealt by {Sphere} by " + increaseAmt + " until the start of your next turn", SelectionType.IncreaseDamage, () => IncreaseEnergyDamageDealt(increaseAmt)));
            SelectFunctionDecision select = new SelectFunctionDecision(GameController, base.HeroTurnTakerController, options, false, cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectAndPerformFunction(select);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            // "You may use a power."
            IEnumerator powerCoroutine = base.GameController.SelectAndUsePower(base.HeroTurnTakerController, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(powerCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(powerCoroutine);
            }
            yield break;
        }

        private IEnumerator IncreaseEnergyDamageDealt(int amt)
        {
            // "... increase energy damage dealt by {Sphere} by 1 until the start of your next turn."
            IncreaseDamageStatusEffect buff = new IncreaseDamageStatusEffect(amt);
            buff.SourceCriteria.IsSpecificCard = base.CharacterCard;
            buff.DamageTypeCriteria.AddType(DamageType.Energy);
            buff.UntilStartOfNextTurn(base.TurnTaker);
            buff.UntilTargetLeavesPlay(base.CharacterCard);
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(buff, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
            yield break;
        }
    }
}
