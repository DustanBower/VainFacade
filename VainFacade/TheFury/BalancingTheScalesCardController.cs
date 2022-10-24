using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheFury
{
    public class BalancingTheScalesCardController : TheFuryUtilityCardController
    {
        public BalancingTheScalesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "{TheFuryCharacter} deals 1 target 3 infernal damage."
            List<DealDamageAction> storedDamage = new List<DealDamageAction>();
            List<Card> alreadyChosen = new List<Card>();
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), 3, DamageType.Infernal, 1, false, 1, storedResultsDamage: storedDamage, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            alreadyChosen.AddRange(storedDamage.Select((DealDamageAction dda) => dda.Target));
            // "A second target regains 3 HP."
            List<GainHPAction> storedHeals = new List<GainHPAction>();
            IEnumerator healCoroutine = base.GameController.SelectAndGainHP(DecisionMaker, 3, additionalCriteria: (Card c) => !alreadyChosen.Contains(c), storedResults: storedHeals, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            alreadyChosen.AddRange(storedHeals.Select((GainHPAction gha) => gha.HpGainer));
            // "Increase the next damage dealt to a third target by 3."
            IEnumerator increaseCoroutine = SelectTargetAndIncreaseNextDamageTo(new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && !alreadyChosen.Contains(c)), 3, false, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(increaseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(increaseCoroutine);
            }
        }
    }
}
