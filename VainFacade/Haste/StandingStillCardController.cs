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
	public class StandingStillCardController:HasteUtilityCardController
	{
		public StandingStillCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //When {Haste} deals himself damage, he deals up to X targets other than himself 2 melee damage each, where X = the damage dealt this way.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.IsSameCard(this.CharacterCard) && dd.Target == this.CharacterCard && dd.DidDealDamage, (DealDamageAction dd) => base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), 2, DamageType.Melee, dd.Amount, false, 0, additionalCriteria: (Card c) => c != this.CharacterCard, cardSource: GetCardSource()), TriggerType.DealDamage, TriggerTiming.After, ActionDescription.DamageTaken);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Add a token to your speed pool. {Haste} deals a target 2 melee damage.
            IEnumerator coroutine = AddSpeedTokens(1);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            int num = GetPowerNumeral(0, 2);
            coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), num, DamageType.Melee, 1, false, 1, cardSource: GetCardSource());
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

