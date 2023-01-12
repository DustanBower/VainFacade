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
    public class EmberCharacterCardController : HeroCharacterCardController
    {
        public EmberCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            int numTargets = GetPowerNumeral(0, 1);
            int damageAmt = GetPowerNumeral(1, 1);
            // "{EmberCharacter} deals 1 target 1 fire damage."
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.Card), damageAmt, DamageType.Fire, numTargets, false, numTargets, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "You may draw a card."
            IEnumerator drawCoroutine = DrawCard(base.TurnTaker.ToHero(), true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator incapCoroutine;
            switch (index)
            {
                case 0:
                    incapCoroutine = UseIncapOption1();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 1:
                    incapCoroutine = UseIncapOption2();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 2:
                    incapCoroutine = UseIncapOption3();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
            }
            yield break;
        }

        private IEnumerator UseIncapOption1()
        {
            // "Destroy an environment card."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), false, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
        }

        private IEnumerator UseIncapOption2()
        {
            // "A hero target..."
            List<SelectCardDecision> selected = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.CardToDealDamage, new LinqCardCriteria((Card c) => c.IsHero && c.IsTarget && c.IsInPlayAndHasGameText, "", false, false, "hero target", "hero targets"), selected, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            Card damageDealer = GetSelectedCard(selected);
            if (damageDealer != null)
            {
                // "... deals a target 1 fire damage, ..."
                HeroTurnTakerController selector = FindTurnTakerController(damageDealer.Owner).ToHero();
                IEnumerator firstCoroutine = base.GameController.SelectTargetsAndDealDamage(selector, new DamageSource(base.GameController, damageDealer), 1, DamageType.Fire, 1, false, 1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(firstCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(firstCoroutine);
                }
                // "... then deals a target 1 fire damage."
                IEnumerator secondCoroutine = base.GameController.SelectTargetsAndDealDamage(selector, new DamageSource(base.GameController, damageDealer), 1, DamageType.Fire, 1, false, 1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(secondCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(secondCoroutine);
                }
            }
        }

        private IEnumerator UseIncapOption3()
        {
            // "One target regains 2 HP."
            IEnumerator healCoroutine = base.GameController.SelectAndGainHP(DecisionMaker, 2, requiredDecisions: 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
        }
    }
}
