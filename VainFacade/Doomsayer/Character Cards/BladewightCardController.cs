using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class BladewightCardController:SideCharacterCardController
	{
		public BladewightCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHeroTargetWithHighestHP(1, base.H - 1);
		}

        public override void AddTriggers()
        {
            //When this card would be dealt 5 or fewer damage, reduce that damage to 1.
            AddReduceDamageToSetAmountTrigger((DealDamageAction dd) => dd.Amount <= 5 && dd.Target == this.Card, 1);

            //When {Bladewight} is dealt damage, increase the next damage dealt by {Bladewight} by 1, then 1 player may draw a card.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.Target == this.Card && dd.DidDealDamage, DamageResponse, new TriggerType[2] { TriggerType.CreateStatusEffect, TriggerType.DrawCard }, TriggerTiming.After);

            //At the end of the villain turn, this card deals the H-1 hero targets with the highest HP 2 melee damage each.
            AddDealDamageAtEndOfTurnTrigger(this.TurnTaker, this.Card, (Card c) => IsHeroTarget(c), TargetType.HighestHP, 2, DamageType.Melee, numberOfTargets: base.H - 1);
        }

        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(1);
            effect.NumberOfUses = 1;
            effect.UntilTargetLeavesPlay(this.Card);
            effect.SourceCriteria.IsSpecificCard = this.Card;
            IEnumerator coroutine = AddStatusEffect(effect);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.GameController.SelectHeroToDrawCard(DecisionMaker, true, cardSource: GetCardSource());
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

