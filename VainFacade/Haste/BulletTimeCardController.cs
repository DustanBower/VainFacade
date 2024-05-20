using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Haste
{
	public class BulletTimeCardController:HasteUtilityCardController
	{
		public BulletTimeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowTokenPool(SpeedPool);
            AllowFastCoroutinesDuringPretend = false;
		}

        private int decisionAmount{ get; set; }

        public Guid? RemoveTokensForDamage { get; set; }

        private SelectNumberDecision selectNumber
        {
            get;
            set;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Add 3 tokens to your speed pool.
            int num = GetPowerNumeral(0, 3);
            IEnumerator coroutine = AddSpeedTokens(num);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Until the start of your next turn, when {Haste} would be dealt damage by a source other than {Haste}, remove any number of tokens from your speed pool.
            //If the number of tokens equals or exceeds that damage, prevent that damage.
            OnDealDamageStatusEffect effect = new OnDealDamageStatusEffect(this.CardWithoutReplacements, "BulletTimeResponse", $"When {this.CharacterCard.Title} would be dealt damage by a source other than {this.CharacterCard.Title}, they may remove tokens from their speed pool to prevent that damage.", new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.CancelAction }, this.TurnTaker, this.Card);
            effect.UntilStartOfNextTurn(this.TurnTaker);
            effect.SourceCriteria.IsNotSpecificCard = this.CharacterCard;
            effect.TargetCriteria.IsSpecificCard = this.CharacterCard;
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

        //Based on The Shadow Cloak
        public IEnumerator BulletTimeResponse(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            if (!RemoveTokensForDamage.HasValue || RemoveTokensForDamage.Value != dda.InstanceIdentifier)
            {
                List<SelectNumberDecision> storedResults = new List<SelectNumberDecision>();
                decisionAmount = dda.Amount;

                IEnumerator coroutine = RemoveAnyNumberOfTokensFromTokenPool_Modified(storedResults , dda);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                if (DidSelectNumber(storedResults))
                {
                    RemoveTokensForDamage = dda.InstanceIdentifier;
                    selectNumber = storedResults.FirstOrDefault();
                }
            }
            if (RemoveTokensForDamage.HasValue && RemoveTokensForDamage.Value == dda.InstanceIdentifier && selectNumber.SelectedNumber >= dda.Amount)
            {
                IEnumerator coroutine3 = CancelAction(dda, showOutput: true, cancelFutureRelatedDecisions: true, null, isPreventEffect: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine3);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine3);
                }
            }
            if (IsRealAction(dda))
            {
                if (selectNumber != null)
                {
                    IEnumerator remove = RemoveSpeedTokens(selectNumber.SelectedNumber.Value, dda, false);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(remove);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(remove);
                    }
                }
                

                RemoveTokensForDamage = null;
            }
        }

        //Modified to show the damage while deciding how many tokens to remove
        private IEnumerator RemoveAnyNumberOfTokensFromTokenPool_Modified(List<SelectNumberDecision> storedResults, DealDamageAction dd)
        {
            IEnumerator coroutine;
            if (HasteSpeedPoolUtility.GetSpeedPool(this) == null)
            {
                coroutine = HasteSpeedPoolUtility.SpeedPoolErrorMessage(this);
                if (this.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(coroutine);
                }
                yield break;
            }

            TokenPool pool = HasteSpeedPoolUtility.GetSpeedPool(this);
            if (pool.CurrentValue > 0)
            {
                coroutine = GameController.SelectNumber(DecisionMaker, SelectionType.Custom, 0, pool.CurrentValue, optional: false, allowAutoDecide: false, null, storedResults, GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }
                //if (selectNumberDecision != null && selectNumberDecision.SelectedNumber.HasValue && storedResults != null)
                //{
                //    int value = selectNumberDecision.SelectedNumber.Value;
                //    storedResults.Add(value);
                //    coroutine = GameController.RemoveTokensFromPool(pool, value, null, optional: false, null, GetCardSource());
                //    if (UseUnityCoroutines)
                //    {
                //        yield return GameController.StartCoroutine(coroutine);
                //    }
                //    else
                //    {
                //        GameController.ExhaustCoroutine(coroutine);
                //    }
                //}
            }
            else
            {
                IEnumerator coroutine2 = GameController.SendMessageAction("There are no tokens in " + pool.Name + " to remove.", Priority.High, GetCardSource(), null, showCardSource: true);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine2);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine2);
                }
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            string extra = $" Remove at least {decisionAmount} to prevent the damage.";
            return new CustomDecisionText(
            $"How many tokens would you like to remove?{extra}",
            $"{decision.DecisionMaker.Name} is choosing how many tokens to remove.{extra}",
            $"Vote for how many tokens to remove.{extra}",
            $"how many tokens to remove.{extra}"
            );
        }
    }
}

