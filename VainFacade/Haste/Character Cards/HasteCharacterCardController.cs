using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VainFacadePlaytest.Peacekeeper;

namespace VainFacadePlaytest.Haste
{
	public class HasteCharacterCardController:HeroCharacterCardController
	{
		public HasteCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowTokenPool(SpeedPool);
		}

        private TokenPool SpeedPool => this.Card.FindTokenPool("SpeedPool");
        

        public override IEnumerator UsePower(int index = 0)
        {
            //Add 3 tokens to your Speed pool.
            int num1 = GetPowerNumeral(0, 3);
            int num2 = GetPowerNumeral(1, 2);
            //IEnumerator coroutine = base.GameController.AddTokensToPool(HasteSpeedPoolUtility.GetSpeedPool(this), num1, GetCardSource());
            IEnumerator coroutine = HasteSpeedPoolUtility.AddSpeedTokens(this, num1, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Until the start of your next turn, when a non-hero card enters play, you may remove 2 tokens from your speed pool. If you do, play a card.
            HastePowerStatusEffect effect = new HastePowerStatusEffect(this.Card, this.TurnTaker, num2);
            effect.UntilStartOfNextTurn(this.TurnTaker);
            effect.UntilTargetLeavesPlay(this.Card);
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

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    //Select a target. Reduce the next damage dealt to that target by 2.
                    List<SelectCardDecision> results = new List<SelectCardDecision>();
                    coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.ReduceNextDamageTaken, new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText, "", false, false, "target", "targets"), results, false, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }


                    if (DidSelectCard(results))
                    {
                        Card selected = GetSelectedCard(results);
                        ReduceDamageStatusEffect effect = new ReduceDamageStatusEffect(2);
                        effect.NumberOfUses = 1;
                        effect.TargetCriteria.IsSpecificCard = selected;
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
                    break;
                case 1:
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
                case 2:
                    //1 hero may use a power.
                    coroutine = base.GameController.SelectHeroToUsePower(DecisionMaker, cardSource: GetCardSource());
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

        public override void AddTriggers()
        {
            //when a non-hero card enters play, you may remove 2 tokens from your speed pool. If you do, play a card.
            AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cep) => ListOfStatusEffectControllers().Count() > 0 && !IsHero(cep.CardEnteringPlay) && HasteSpeedPoolUtility.GetSpeedPool(this) != null && HasteSpeedPoolUtility.GetSpeedPool(this).CurrentValue > 0, HastePowerResponse, new TriggerType[] { TriggerType.ModifyTokens, TriggerType.PlayCard }, TriggerTiming.After);
        }

        private IEnumerable<StatusEffectController> ListOfStatusEffectControllers()
        {
            return base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is HastePowerStatusEffect);
        }

        private IEnumerator HastePowerResponse(CardEntersPlayAction cep)
        {
            List<HastePowerStatusEffect> effects = ListOfStatusEffectControllers().Select((StatusEffectController sec) => sec.StatusEffect as HastePowerStatusEffect).ToList();
            foreach (HastePowerStatusEffect effect in effects)
            {
                int num = effect.Num;
                List<RemoveTokensFromPoolAction> results = new List<RemoveTokensFromPoolAction>();
                CardSource source = new CardSource(FindCardController(effect.Source));
                if (source.Card != this.CardWithoutReplacements)
                {
                    //This is necessary to allow Friday to get the custom decision text
                    source.AddAssociatedCardSource(GetCardSource());
                }
                IEnumerator coroutine = HasteSpeedPoolUtility.RemoveSpeedTokens(FindCardController(effect.Source), num, null, true, results, null,cardSource: source);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidRemoveTokens(results, num))
                {
                    TurnTaker hero = effect.Hero;
                    coroutine = base.GameController.SelectAndPlayCardFromHand(FindHeroTurnTakerController(hero.ToHero()), false, cardSource: source);
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

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decision is YesNoAmountDecision && ((YesNoAmountDecision)decision).Amount.HasValue)
            {
                int amount = ((YesNoAmountDecision)decision).Amount.Value;
                string amountString = amount == 1 ? "a" : amount.ToString();
                string name = decision.DecisionMaker.Name;
                string plural = amount == 1 ? "" : "s";
                return new CustomDecisionText(
                $"Remove {amountString} speed token{plural} to play a card?",
                $"{name} is deciding whether to remove {amountString} speed token{plural} to play a card",
                $"Vote on whether to remove {amountString} speed token{plural} to play a card",
                $"remove {amountString} speed token{plural} to play a card"
                );
            }
            return null;
        }
    }
}

