using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class PeacekeeperCharacterCardController:HeroCharacterCardController
	{
		public PeacekeeperCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator UsePower(int index = 0)
        {
            //Peaecekeeper deals 1 target 1 melee damage.
            int num = GetPowerNumeral(0, 1);
            IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.Card), num, DamageType.Melee, 1, false, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //You may draw a card.
            coroutine = DrawCard(this.HeroTurnTaker, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    //The next time a hero uses a power, that hero deals 1 target 1 projectile damage.
                    PeacekeeperIncapStatusEffect effect = new PeacekeeperIncapStatusEffect(this.Card);
                    coroutine = AddStatusEffect(effect);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    break;
                case 1:
                    //Select a hero target. Reduce the next damage dealt to that target by 2.
                    List<SelectCardDecision> results = new List<SelectCardDecision>();
                    coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.ReduceNextDamageTaken, new LinqCardCriteria((Card c) => IsHeroTarget(c) && c.IsInPlayAndHasGameText), results, false, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    Card selected = GetSelectedCard(results);
                    if (selected != null)
                    {
                        ReduceDamageStatusEffect effect2 = new ReduceDamageStatusEffect(2);
                        effect2.TargetCriteria.IsSpecificCard = selected;
                        effect2.NumberOfUses = 1;
                        effect2.UntilTargetLeavesPlay(selected);
                        coroutine = AddStatusEffect(effect2);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                    }
                    break;
                case 2:
                    //1 player may play a card.
                    coroutine = SelectHeroToPlayCard(DecisionMaker);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    break;
            }
        }

        public override void AddSideTriggers()
        {
            AddTrigger<UsePowerAction>((UsePowerAction up) => base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is PeacekeeperIncapStatusEffect && ((PeacekeeperIncapStatusEffect)sec.StatusEffect).Source == this.Card).Count() > 0, Incap1Response, TriggerType.DealDamage, TriggerTiming.After);
        }

        private IEnumerator Incap1Response(UsePowerAction up)
        {
            //...that hero deals 1 target 1 projectile damage.
            Card hero = up.HeroCharacterCardUsingPower.Card;
            HeroTurnTakerController httc = up.HeroUsingPower;
            List<StatusEffectController> removeEffects = base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is PeacekeeperIncapStatusEffect && ((PeacekeeperIncapStatusEffect)sec.StatusEffect).Source == this.Card).ToList();
            for (int i = 0; i < removeEffects.Count(); i++)
            {
                IEnumerator remove = base.GameController.ExpireStatusEffect(removeEffects[i].StatusEffect,GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(remove);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(remove);
                }

                IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(httc, new DamageSource(base.GameController, hero), 1, DamageType.Projectile, 1, false, 1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
        }
    }
}

