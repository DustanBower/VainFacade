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
    public class RechargeCardController : BlitzUtilityCardController
    {
        public RechargeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show the hero target with the highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {BlitzCharacter} is dealt lightning damage, he regains the same amount of HP.{BR}Then, he regains 5 HP. If he did not regain HP this way, increase the next lightning damage dealt by {BlitzCharacter} by 1."
            AddTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && dda.DamageType == DamageType.Lightning && dda.DidDealDamage, HealHealOrBoostResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.CreateStatusEffect }, TriggerTiming.After);
            // "At the end of the villain turn, {BlitzCharacter} deals the hero target with the highest HP 1 irreducible lightning damage. If no damage is dealt this way, each player destroys 1 of their Ongoings."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DealOrDestroyResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroyCard });
        }

        private IEnumerator HealHealOrBoostResponse(DealDamageAction dda)
        {
            // "... he regains the same amount of HP."
            IEnumerator healXCoroutine = base.GameController.GainHP(base.CharacterCard, dda.Amount, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healXCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healXCoroutine);
            }
            // "Then, he regains 5 HP."
            List<GainHPAction> results = new List<GainHPAction>();
            IEnumerator healFiveCoroutine = base.GameController.GainHP(base.CharacterCard, 5, storedResults: results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healFiveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healFiveCoroutine);
            }
            // "If he did not regain HP this way, increase the next lightning damage dealt by {BlitzCharacter} by 1."
            bool didGainHP = false;
            foreach (GainHPAction hpa in results)
            {
                if (hpa.AmountActuallyGained > 0)
                {
                    didGainHP = true;
                }
            }
            if (!didGainHP)
            {
                IEnumerator boostCoroutine = IncreaseNextLightningDamage(cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(boostCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(boostCoroutine);
                }
            }
        }

        private IEnumerator DealOrDestroyResponse(PhaseChangeAction pca)
        {
            // "... {BlitzCharacter} deals the hero target with the highest HP 1 irreducible lightning damage."
            List<DealDamageAction> results = new List<DealDamageAction>();
            IEnumerator damageCoroutine = DealDamageToHighestHP(base.CharacterCard, 1, (Card c) => c.IsHero && c.IsTarget, (Card c) => 1, DamageType.Lightning, isIrreducible: true, storedResults: results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "If no damage is dealt this way, each player destroys 1 of their Ongoings."
            if (!DidDealDamage(results, fromDamageSource: base.CharacterCard))
            {
                IEnumerator destroyCoroutine = EachPlayerDestroysTheirCards(new LinqTurnTakerCriteria((TurnTaker tt) => true, "heroes with Ongoing cards in play"), new LinqCardCriteria((Card c) => c.IsHero && IsOngoing(c), "hero Ongoing"));
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
        }
    }
}
