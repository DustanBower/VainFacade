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
    public class WreathedInFlameCardController : EmberUtilityCardController
    {
        public WreathedInFlameCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Blaze cards in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(BlazeCard);
        }

        private const string FirstDamageThisTurn = "FirstDamageThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase fire damage dealt by {EmberCharacter} by 1 for each Blaze card in play."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource.IsSameCard(base.CharacterCard) && dda.DamageType == DamageType.Fire, (DealDamageAction dda) => NumBlazeCardsInPlay());
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int retaliateAmt = GetPowerNumeral(0, 1);
            // "Until the start of your next turn, the first time each turn {EmberCharacter} is dealt damage, she may deal the source of that damage 1 fire damage."
            OnDealDamageStatusEffect firewall = new OnDealDamageStatusEffect(base.Card, nameof(CounterDamageOncePerTurnResponse), "The first time " + base.CharacterCard.Title + " is dealt damage each turn, she may deal the source of that damage " + retaliateAmt.ToString() + " fire damage.", new TriggerType[] { TriggerType.DealDamage }, base.TurnTaker, base.Card, new int[] { retaliateAmt });
            firewall.BeforeOrAfter = BeforeOrAfter.After;
            firewall.TargetCriteria.IsSpecificCard = base.CharacterCard;
            firewall.UntilStartOfNextTurn(base.TurnTaker);
            firewall.UntilTargetLeavesPlay(base.CharacterCard);
            firewall.DamageAmountCriteria.GreaterThan = 0;
            firewall.DoesDealDamage = true;
            IEnumerator statusCoroutine = AddStatusEffect(firewall);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
            // "You may discard a card to use a power."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = SelectAndDiscardCards(DecisionMaker, 1, requiredDecisions: 0, storedResults: discards, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(discards, 1, true))
            {
                IEnumerator powerCoroutine = base.GameController.SelectAndUsePower(DecisionMaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(powerCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(powerCoroutine);
                }
            }
        }

        public IEnumerator CounterDamageOncePerTurnResponse(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            if (!HasBeenSetToTrueThisTurn(FirstDamageThisTurn))
            {
                SetCardPropertyToTrueIfRealAction(FirstDamageThisTurn);
                // "... she may deal the source of that damage 1 fire damage."
                int? amt = null;
                if (powerNumerals != null)
                {
                    amt = powerNumerals.ElementAtOrDefault(0);
                }
                if (!amt.HasValue)
                {
                    amt = 1;
                }

                if (dda.DamageSource.IsCard)
                {
                    IEnumerator damageCoroutine = DealDamage(base.CharacterCard, dda.DamageSource.Card, amt.Value, DamageType.Fire, optional: true, isCounterDamage: true, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(damageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(damageCoroutine);
                    }
                }
            }
        }
    }
}
