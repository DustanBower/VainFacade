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
    public class FromTheAshesCardController : EmberUtilityCardController
    {
        public FromTheAshesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Blaze cards in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(BlazeCard);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {EmberCharacter} drops to 0 or fewer HP, restore her to 7 HP, then remove this card from the game."
            AddTrigger((DestroyCardAction dca) => dca.CardToDestroy.Card == base.CharacterCard && base.CharacterCard.HitPoints.HasValue && base.CharacterCard.HitPoints.Value <= 0, PreventDestroyRestoreHPRemoveResponse, TriggerType.CancelAction, TriggerTiming.Before);
        }

        private IEnumerator PreventDestroyRestoreHPRemoveResponse(DestroyCardAction dca)
        {
            IEnumerator preventCoroutine = CancelAction(dca);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(preventCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(preventCoroutine);
            }
            // "... restore her to 7 HP, ..."
            IEnumerator restoreCoroutine = base.GameController.SetHP(base.CharacterCard, Math.Min(7, base.CharacterCard.MaximumHitPoints.Value), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(restoreCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(restoreCoroutine);
            }
            // "... then remove this card from the game."
            IEnumerator removeCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.OutOfGame, showMessage: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(removeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(removeCoroutine);
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int healAmt = GetPowerNumeral(0, 1);
            int increaseAmt = GetPowerNumeral(1, 1);
            // "Until the start of your next turn, whenever {EmberCharacter} is dealt fire damage, she regains 1 HP for each Blaze card in play, then increases the next damage she deals by 1."
            OnDealDamageStatusEffect reaction = new OnDealDamageStatusEffect(CardWithoutReplacements, nameof(FireDamageResponse), "Whenever " + base.CharacterCard.Title + " is dealt fire damage, she regains " + healAmt.ToString() + " HP for each Blaze card in play, then increases the next damage she deals by " + increaseAmt.ToString() + ".", new TriggerType[] { TriggerType.GainHP, TriggerType.CreateStatusEffect }, base.TurnTaker, base.Card, new int[] {healAmt, increaseAmt});
            reaction.TargetCriteria.IsSpecificCard = base.CharacterCard;
            reaction.DamageAmountCriteria.GreaterThan = 0;
            reaction.DamageTypeCriteria.AddType(DamageType.Fire);
            reaction.UntilTargetLeavesPlay(base.CharacterCard);
            reaction.UntilStartOfNextTurn(base.TurnTaker);
            reaction.BeforeOrAfter = BeforeOrAfter.After;
            IEnumerator statusCoroutine = AddStatusEffect(reaction);
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

        public IEnumerator FireDamageResponse(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            int? healAmt = 1;
            int? increaseAmt = 1;
            if (powerNumerals != null)
            {
                healAmt = powerNumerals.ElementAtOrDefault(0);
                increaseAmt = powerNumerals.ElementAtOrDefault(1);
            }
            if (!healAmt.HasValue)
            {
                healAmt = 1;
            }
            if (!increaseAmt.HasValue)
            {
                increaseAmt = 1;
            }
            // "... she regains 1 HP for each Blaze card in play, ..."
            int numberOfHeals = NumBlazeCardsInPlay();
            if (numberOfHeals > 0)
            {
                if (!base.GameController.DoAnyTriggersRespondToAction(new GainHPAction(GetCardSource(), base.CharacterCard, healAmt.Value * numberOfHeals, null)))
                {
                    IEnumerator healCoroutine = base.GameController.GainHP(base.CharacterCard, healAmt.Value * numberOfHeals, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(healCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(healCoroutine);
                    }
                }
                else
                {
                    while (numberOfHeals > 0)
                    {
                        IEnumerator healOneCoroutine = base.GameController.GainHP(base.CharacterCard, healAmt.Value, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(healOneCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(healOneCoroutine);
                        }
                        numberOfHeals -= 1;
                    }
                }
            }
            // "... then increases the next damage she deals by 1."
            IncreaseDamageStatusEffect buff = new IncreaseDamageStatusEffect(increaseAmt.Value);
            buff.SourceCriteria.IsSpecificCard = base.CharacterCard;
            buff.NumberOfUses = 1;
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(buff, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
        }
    }
}
