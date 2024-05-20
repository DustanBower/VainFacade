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
	public class KillingTimeCardController:HasteUtilityCardController
	{
		public KillingTimeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
			base.SpecialStringMaker.ShowTokenPool(SpeedPool);
		}

        private string key = "KillingTimeKey";

        public override IEnumerator Play()
        {
            SetKey(null);
            return base.Play();
        }

        public override void AddTriggers()
        {
            //When a card not named {KillingTime} adds tokens to your speed pool, add a token to your speed pool.
            //This causes an infinite loop if copied by Uh Yeah
            //AddTrigger<AddTokensToPoolAction>((AddTokensToPoolAction tp) => HasteSpeedPoolUtility.GetSpeedPool(this) != null && tp.TokenPool == HasteSpeedPoolUtility.GetSpeedPool(this) && (tp.CardSource == null || tp.CardSource.Card.Title != "Killing Time"), (AddTokensToPoolAction tp) => AddSpeedTokens(1), TriggerType.AddTokensToPool, TriggerTiming.After);

            AddTrigger<GameAction>((GameAction ga) => !IsPropertyCurrentlyTrue(key), SetKey, TriggerType.Hidden, TriggerTiming.Before);
            AddAfterLeavesPlayAction(ResetKey, TriggerType.HiddenLast);
            AddBeforeLeavesPlayActions(ResetKey);

            //At the start of your turn, increase the next damage dealt to {Haste} by 1 for every 10 tokens in your speed pool. Then {Haste} deals himself 0 irreducible toxic damage.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, StartOfTurnResponse, new TriggerType[] { TriggerType.CreateStatusEffect, TriggerType.DealDamage });
        }

        private IEnumerator SetKey(GameAction ga)
        {
            SetCardProperty(key, true);
            yield return null;
        }

        private IEnumerator ResetKey(GameAction ga)
        {
            if (this.Card != this.CardWithoutReplacements)
            {
                //If Uh Yeah is destroyed while copying this card, it should only reset its own properties
                SetCardProperty(key, false);
            }
            else
            {
                //If this card is destroyed, reset all cards' properties so that Uh Yeah no longer copies this card
                ResetAllCardProperties(key);
            }
            yield return null;
        }

        private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
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
            int num = pool.CurrentValue / 10;
            IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(num);
            effect.NumberOfUses = 1;
            effect.UntilTargetLeavesPlay(this.CharacterCard);
            effect.TargetCriteria.IsSpecificCard = this.CharacterCard;
            coroutine = AddStatusEffect(effect);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = DealDamage(this.CharacterCard, this.CharacterCard, 0, DamageType.Toxic, true, cardSource: GetCardSource());
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

