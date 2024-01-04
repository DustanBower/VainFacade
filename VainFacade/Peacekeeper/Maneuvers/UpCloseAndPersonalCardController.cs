using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class UpCloseAndPersonalCardController:ManeuverCardController
	{
		public UpCloseAndPersonalCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddAsPowerContributor();
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamage);
		}

        private string FirstDamage = "FirstDamage";

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            //Play this card next to a target.
            IEnumerator coroutine = SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => c.IsTarget, "", false, false, "target", "targets"), storedResults, isPutIntoPlay, decisionSources);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override IEnumerable<Card> FilterDecisionCardChoices(SelectCardDecision decision)
        {
            if (decision.SelectionType == SelectionType.MoveCardNextToCard && decision.Choices.Where((Card c) => !IsHeroTarget(c)).Count() > 0)
            {
                return decision.Choices.Where((Card c) => IsHeroTarget(c));
            }
            return null;
        }

        public override void AddTriggers()
        {
            //The first time each turn that target would deal damage, redirect that damage to Peacekeeper.
            //AddRedirectDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsTarget && IsThisCardNextToCard(dd.DamageSource.Card) && dd.Target != this.CharacterCard, () => this.CharacterCard);
            AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(FirstDamage) && dd.DamageSource.IsTarget && IsThisCardNextToCard(dd.DamageSource.Card) && !HasDamageOccurredThisTurn(null, (Card c) => IsThisCardNextToCard(c), dd), RedirectResponse, TriggerType.RedirectDamage, TriggerTiming.Before);

            //Reduce non-melee damage that target deals to {Peacekeeper} by 1.
            AddReduceDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsTarget && IsThisCardNextToCard(dd.DamageSource.Card) && dd.Target == this.CharacterCard && dd.DamageType != DamageType.Melee, (DealDamageAction dd) => 1);

            AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(true,false);

            AddTrigger((UsePowerAction up) => up.Power != null && up.Power.IsContributionFromCardSource && up.Power.CopiedFromCardController == this,
                            ReplaceWithActualPower,
                            TriggerType.FirstTrigger,
                            TriggerTiming.Before);
        }

        private IEnumerator RedirectResponse(DealDamageAction dd)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamage);
            if (dd.Target != this.CharacterCard)
            {
                IEnumerator coroutine = base.GameController.RedirectDamage(dd, this.CharacterCard, false, GetCardSource());
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

        public override IEnumerator UsePower(int index = 0)
        {
            // {Peacekeeper} deals the target next to this card 3 melee damage.
            int num = GetPowerNumeral(0, 3);
            if (GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().IsTarget)
            {
                IEnumerator coroutine = DealDamage(this.CharacterCard, GetCardThisCardIsNextTo(), num, DamageType.Melee, cardSource: GetCardSource());
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

        private IEnumerator ReplaceWithActualPower(UsePowerAction up)
        {
            IEnumerator coroutine = CancelAction(up, false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = UsePowerOnOtherCard(this.Card);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }

        public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cardController)
        {
            Power[] powers = null;
            if (cardController.CardWithoutReplacements == CharacterCard)
            {
                if (!HasPowerBeenUsedThisTurn(new Power(DecisionMaker, this, CardWithoutReplacements.AllPowers.FirstOrDefault(), this.UsePower(), 0, null, GetCardSource())))
                {
                    return new Power[]
                    {
                        new Power(DecisionMaker, cardController, "{Peacekeeper} deals the target next to Up Close and Personal 3 melee damage.", this.DoNothing(), 0, this, GetCardSource())
                    };
                }
            }
            return powers;
        }

        private bool HasPowerBeenUsedThisTurn(Power power)
        {
            List<UsePowerJournalEntry> source = Game.Journal.UsePowerEntriesThisTurn().ToList();
            Func<UsePowerJournalEntry, bool> predicate = delegate (UsePowerJournalEntry p)
            {
                bool flag = power.CardController.CardWithoutReplacements == p.CardWithPower;
                if (!flag && power.CardController.CardWithoutReplacements.SharedIdentifier != null && power.IsContributionFromCardSource)
                {
                    flag = power.CardController.CardWithoutReplacements.SharedIdentifier == p.CardWithPower.SharedIdentifier;
                }
                if (flag)
                {
                    flag &= p.NumberOfUses == 0;
                }
                if (flag)
                {
                    flag &= power.Index == p.PowerIndex;
                }
                if (flag)
                {
                    flag &= power.IsContributionFromCardSource == p.IsContributionFromCardSource;
                }
                if (flag)
                {
                    bool flag2 = power.TurnTakerController == null && p.PowerUser == null;
                    bool flag3 = false;
                    if (power.TurnTakerController != null && power.TurnTakerController.IsHero)
                    {
                        flag3 = power.TurnTakerController.ToHero().HeroTurnTaker == p.PowerUser;
                    }
                    flag = flag && (flag2 || flag3);
                }
                if (flag)
                {
                    if (!power.IsContributionFromCardSource)
                    {
                        if (flag && power.CardController.CardWithoutReplacements.PlayIndex.HasValue && p.CardWithPowerPlayIndex.HasValue)
                        {
                            flag &= power.CardController.CardWithoutReplacements.PlayIndex.Value == p.CardWithPowerPlayIndex.Value;
                        }
                    }
                    else
                    {
                        flag &= p.CardSource == power.CardSource.Card;
                        if (power.CardSource != null && power.CardSource.Card.PlayIndex.HasValue && p.CardSourcePlayIndex.HasValue)
                        {
                            flag &= power.CardSource.Card.PlayIndex.Value == p.CardSourcePlayIndex.Value;
                        }
                    }
                }
                return flag;
            };
            int num = source.Where(predicate).Count();
            if (num > 0)
            {
                if (!GameController.StatusEffectManager.AskIfPowerCanBeReused(power, num))
                {
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}

