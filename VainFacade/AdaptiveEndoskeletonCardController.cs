using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Friday
{
	public class AdaptiveEndoskeletonCardController:CardController
	{
		public AdaptiveEndoskeletonCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //Increase lightning damage dealt to {Friday} by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DamageType == DamageType.Lightning, (DealDamageAction dd) => 1);

            //When {Friday} would be dealt damage by a source other than {Friday}, you may attune this card to that damage's type.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => (!dd.DamageSource.IsCard || dd.DamageSource.Card != this.CharacterCard) && dd.Target == this.CharacterCard && dd.Amount > 0 && !IsAttuned(dd.DamageType), PreDamageResponse, TriggerType.WouldBeDealtDamage, TriggerTiming.Before);

            //Each time {Friday} is dealt damage of a type this card is attuned to, reduce damage of that type dealt to {Friday} by 1 until this card is attuned to another type or this card leaves play.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.Target == this.CharacterCard && IsAttuned(dd.DamageType) && dd.DidDealDamage, ReduceResponse, TriggerType.CreateStatusEffect, TriggerTiming.After);
        }

        private bool IsAttuned(DamageType type)
        {
            StatusEffectController effectController =  base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is AttuneStatusEffect && ((AttuneStatusEffect)sec.StatusEffect).Source == this.Card && ((AttuneStatusEffect)sec.StatusEffect).Target == this.CharacterCard).FirstOrDefault();
            if (effectController != null)
            {
                AttuneStatusEffect effect = (AttuneStatusEffect)effectController.StatusEffect;
                return effect.Type == type;
            }
            return false;
        }

        private DamageType? GetAttunedType()
        {
            StatusEffectController effectController = base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is AttuneStatusEffect && ((AttuneStatusEffect)sec.StatusEffect).Source == this.Card && ((AttuneStatusEffect)sec.StatusEffect).Target == this.CharacterCard).FirstOrDefault();
            if (effectController != null)
            {
                AttuneStatusEffect effect = (AttuneStatusEffect)effectController.StatusEffect;
                return effect.Type;
            }
            return null;
        }

        private IEnumerator PreDamageResponse(DealDamageAction dd)
        {
            List<YesNoCardDecision> results = new List<YesNoCardDecision>();
            List<Card> relevant = new List<Card>();
            relevant.Add(base.Card);
            IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.Custom, base.CharacterCard, dd, results, relevant, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidPlayerAnswerYes(results))
            {
                coroutine = Attune(dd);
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

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decision is YesNoCardDecision && ((YesNoCardDecision)decision).GameAction is DealDamageAction)
            {
                DamageType type = ((DealDamageAction)((YesNoCardDecision)decision).GameAction).DamageType;
                DamageType? currentType = GetAttunedType();
                string extraString = (currentType != null) ? ($" Currently attuned to {currentType.Value.ToString()}.") : ("");
                return new CustomDecisionText(
                $"Do you want to attune to {type} damage?{extraString}",
                $"{decision.DecisionMaker.Name} is deciding whether to attune to {type} damage.{extraString}",
                $"Vote for if {this.TurnTaker.Name} should attune to {type} damage.{extraString}",
                $"whether {this.TurnTaker.Name} should attune to {type} damage.{extraString}"
                );
            }
            return null;

        }

        private IEnumerator Attune(DealDamageAction dd)
        {
            IEnumerator coroutine;

            //Remove existing attune status effect
            StatusEffectController removeEffect1 = base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is AttuneStatusEffect && ((AttuneStatusEffect)sec.StatusEffect).Source == this.Card).FirstOrDefault();
            if (removeEffect1 != null)
            {
                //base.GameController.StatusEffectManager.RemoveStatusEffect(removeEffect.StatusEffect);
                coroutine = base.GameController.ExpireStatusEffect(removeEffect1.StatusEffect, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }

            //Remove existing damage reduction effects associated with the attunement
            StatusEffectController removeEffect2 = base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is ReduceDamageStatusEffect && ((ReduceDamageStatusEffect)sec.StatusEffect).TargetCriteria.IsSpecificCard == this.CharacterCard && ((ReduceDamageStatusEffect)sec.StatusEffect).CardSource == this.Card).FirstOrDefault();
            if (removeEffect2 != null)
            {
                coroutine = base.GameController.ExpireStatusEffect(removeEffect2.StatusEffect, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }

            //Add new attune status effect
            AttuneStatusEffect effect = new AttuneStatusEffect(this.TurnTaker, this.CharacterCard, this.Card, dd.DamageType);
            effect.UntilCardLeavesPlay(this.Card);
            effect.UntilTargetLeavesPlay(this.CharacterCard);
            coroutine = AddStatusEffect(effect);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator ReduceResponse(DealDamageAction dd)
        {
            ReduceDamageStatusEffect effect = new ReduceDamageStatusEffect(1);
            effect.DamageTypeCriteria.AddType(dd.DamageType);
            effect.UntilTargetLeavesPlay(this.CharacterCard);
            effect.UntilCardLeavesPlay(this.Card);
            effect.TargetCriteria.IsSpecificCard = this.CharacterCard;
            IEnumerator coroutine = AddStatusEffect(effect);
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

