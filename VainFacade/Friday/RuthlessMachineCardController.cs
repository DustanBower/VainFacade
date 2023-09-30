using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Friday
{
	public class RuthlessMachineCardController:CardController
	{
		public RuthlessMachineCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public Guid? PerformDiscardForDamage { get; set; }

        public Card CardToDiscard { get; set; }

        public override void AddTriggers()
        {
            //When a card from another deck would cause {Friday} to deal herself damage, you may discard a card from your hand to prevent that damage.
            AddTrigger((DealDamageAction dd) => dd.Target == base.CharacterCard && dd.Amount > 0 && dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && dd.CardSource.Card.Owner != this.Card.Owner, DamageResponse, new TriggerType[3]
            {
                TriggerType.WouldBeDealtDamage,
                TriggerType.DiscardCard,
                TriggerType.CancelAction
            }, TriggerTiming.Before, null, isConditional: false, requireActionSuccess: true, true);
        }

        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            //Copied from The Shadow Cloak
            if (!PerformDiscardForDamage.HasValue || PerformDiscardForDamage.Value != dd.InstanceIdentifier)
            {
                List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
                List<DealDamageAction> list = new List<DealDamageAction>();
                list.Add(dd);
                IEnumerator coroutine = base.GameController.SelectAndDiscardCard(DecisionMaker, optional: true, null, storedResults, SelectionType.DiscardCard, list, null, ignoreBattleZone: false, null, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                if (DidDiscardCards(storedResults))
                {
                    PerformDiscardForDamage = dd.InstanceIdentifier;
                    if (storedResults.Any((DiscardCardAction dc) => dc.IsPretend))
                    {
                        CardToDiscard = storedResults.First().CardToDiscard;
                    }
                }
            }
            if (CardToDiscard != null)
            {
                IEnumerator coroutine2 = base.GameController.DiscardCard(DecisionMaker, CardToDiscard, null, base.TurnTaker, null, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine2);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine2);
                }
            }
            if (PerformDiscardForDamage.HasValue && PerformDiscardForDamage.Value == dd.InstanceIdentifier)
            {
                IEnumerator coroutine3 = CancelAction(dd, showOutput: true, cancelFutureRelatedDecisions: true, null, isPreventEffect: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine3);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine3);
                }
            }
            if (IsRealAction(dd))
            {
                PerformDiscardForDamage = null;
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //{Friday} deals 1 Target 2 melee or 2 projectile damage. Draw a card.
            int num1 = GetPowerNumeral(0, 1);
            int num2 = GetPowerNumeral(1, 2);
            int num3 = GetPowerNumeral(2, 2);
            List<SelectDamageTypeDecision> results = new List<SelectDamageTypeDecision>();
            IEnumerator coroutine = base.GameController.SelectDamageType(DecisionMaker, results, new DamageType[2] {DamageType.Melee, DamageType.Projectile }, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            DamageType? type = GetSelectedDamageType(results);

            if (type.HasValue)
            {
                int amount = 2;
                if (type.Value == DamageType.Melee)
                {
                    amount = num2;
                }
                else if (type.Value == DamageType.Projectile)
                {
                    amount = num3;
                }
                coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), (Card c) => amount, type.Value, () => num1, false, num1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }

            coroutine = DrawCard();
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

