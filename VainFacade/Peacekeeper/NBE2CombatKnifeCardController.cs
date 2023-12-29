using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace VainFacadePlaytest.Peacekeeper
{
	public class NBE2CombatKnifeCardController:PeacekeeperCardUtilities
	{
		public NBE2CombatKnifeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowSpecialString(ShowDamageTakenSinceStartOfLastTurn);
        }

        private string ShowDamageTakenSinceStartOfLastTurn()
        {
            string text = null;
            IEnumerable<DealDamageJournalEntry> enumerable = from e in Journal.DealDamageEntries()
                                                             where e.TargetCard == this.CharacterCard
                                                             select e;
            enumerable = enumerable.Where(SinceStartOfLastTurn<DealDamageJournalEntry>(this.TurnTaker));
            string text3 = (!enumerable.Any() ? "not " : "");
            string text4 = "damage";
            string text5 = "";
            if (!enumerable.Any())
            {
                text5 = " by targets ";
            }
            else if (enumerable.Any())
            {
                text5 = " by " + (from e in enumerable
                                    where e.SourceCard != null
                                    select e.SourceCard.AlternateTitleOrTitle).Distinct().ToCommaList(useWordAnd: true) + " ";
            }
                        
            string text7 = this.CharacterCard.Title;
            
            string text8 = ((!Journal.PhaseChangeEntries().Where(Journal.SinceLastTurn<PhaseChangeJournalEntry>(this.TurnTaker)).Any()) ? (" since there has not been a previous turn for " + this.TurnTaker.Name) : (" since the start of " + this.TurnTaker.Name + "'s last turn"));

            string text9 = "has";
            
            text = text7 + " " + text9 + " " + text3 + "been dealt " + text4 + text5 + text8 + ".";
            
            return text.TrimExtraSpaces();
        }

        public override void AddTriggers()
        {
            //Increase melee damage dealt by {Peacekeeper} by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && dd.DamageType == DamageType.Melee, 1);

            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.CardSource != null && dd.CardSource.PowerSource != null && dd.CardSource == GetCardSource(), 2);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //{Peacekeeper} deals 1 target 2 melee damage.
            //If that target dealt {Peacekeeper} melee damage since the start of your last turn, increase that damage by 2.
            int num1 = GetPowerNumeral(0, 1);
            int num2 = GetPowerNumeral(1, 2);
            int num3 = GetPowerNumeral(2, 2);

            AddToTemporaryTriggerList(AddTrigger<DealDamageAction>((DealDamageAction dd) => HasTargetDealtDamageToPeacekeeperSinceStartOfLastTurn(dd.Target) && dd.CardSource != null && dd.CardSource.PowerSource != null && dd.CardSource.Card == this.Card, (DealDamageAction dd) => IncreaseDamageResponse(dd,num3), TriggerType.IncreaseDamage, TriggerTiming.Before));

            IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), num2, DamageType.Melee, num1, false, num1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            RemoveTemporaryTriggers();
        }

        private IEnumerator IncreaseDamageResponse(DealDamageAction dd, int amount)
        {
            //RemoveTemporaryTriggers();
            IEnumerator coroutine = base.GameController.IncreaseDamage(dd, amount, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        //private IEnumerable<Card> GetTargetsThatHaveDealtDamageToPeacekeeperSinceStartOfLastTurn()
        //{
        //    return (from e in DealDamageEntriesToTargetSinceStartOfLastTurn(base.CharacterCard, base.TurnTaker)
        //            where e.SourceCard != null && HasTargetDealtDamageToPeacekeeperSinceStartOfLastTurn(e.SourceCard) && e.DamageType == DamageType.Melee
        //            select e.SourceCard).Distinct();
        //}

        private bool HasTargetDealtDamageToPeacekeeperSinceStartOfLastTurn(Card target)
        {
            if (target.IsTarget)
            {
                DealDamageJournalEntry dealDamageJournalEntry = (from d in DealDamageEntriesFromTargetToTargetSinceStartOfLastTurn(target, base.CharacterCard, base.TurnTaker)
                                                                 where d.Amount > 0
                                                                 select d).LastOrDefault();
                if (dealDamageJournalEntry != null && target.IsInPlayAndHasGameText)
                {
                    int? entryIndex = base.GameController.Game.Journal.GetEntryIndex(dealDamageJournalEntry);
                    PlayCardJournalEntry playCardJournalEntry = (from c in base.GameController.Game.Journal.PlayCardEntries()
                                                                 where c.CardPlayed == target
                                                                 select c).LastOrDefault();
                    if (playCardJournalEntry == null)
                    {
                        return true;
                    }
                    if (entryIndex > base.GameController.Game.Journal.GetEntryIndex(playCardJournalEntry))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //private IEnumerable<DealDamageJournalEntry> DealDamageEntriesToTargetSinceStartOfLastTurn(Card targetCard, TurnTaker turnTaker)
        //{
        //    return base.Game.Journal.QueryJournalEntries((DealDamageJournalEntry e) => e.TargetCard.Equals(targetCard)).Where(SinceStartOfLastTurn<DealDamageJournalEntry>(turnTaker));
        //}

        private IEnumerable<DealDamageJournalEntry> DealDamageEntriesFromTargetToTargetSinceStartOfLastTurn(Card sourceCard, Card targetCard, TurnTaker turnTaker)
        {
            return base.Game.Journal.QueryJournalEntries((DealDamageJournalEntry e) => e.SourceCard != null && e.TargetCard != null && TargetEqualityComparer.AreCardsTheSameTarget(e.SourceCard, sourceCard) && TargetEqualityComparer.AreCardsTheSameTarget(e.TargetCard, targetCard)).Where(SinceStartOfLastTurn<DealDamageJournalEntry>(turnTaker));
        }

        private Func<T, bool> SinceStartOfLastTurn<T>(TurnTaker turnTaker) where T : JournalEntry
        {
            if (base.Game.Journal.PhaseChangeEntries().Any((PhaseChangeJournalEntry pcje) => pcje.ToPhase != null && pcje.ToPhase.TurnTaker == turnTaker && pcje.ToPhase.IsEnd))
            {
                //turnTakerIndex = Peacekeeper's turn index
                int? turnTakerIndex = Game.TurnTakers.IndexOf(turnTaker);
                if (turnTakerIndex.HasValue)
                {
                    return delegate (T e)
                    {
                        //num = index of turn when damage happened
                        int num = 0;
                        if (e.TurnPhase != null)
                        {
                            num = Game.TurnTakers.IndexOf(e.TurnPhase.TurnTaker).GetValueOrDefault(0);
                        }

                        //num2 = index of active turn
                        int num2 = 0;
                        if (Game.ActiveTurnPhase != null)
                        {
                            num2 = Game.TurnTakers.IndexOf(Game.ActiveTurnPhase.TurnTaker).GetValueOrDefault(0);
                        }

                        //round = round when damage happened
                        int round = e.Round;

                        //round2 = current round
                        int round2 = Game.Round;

                        if (num2 > turnTakerIndex)
                        {
                            //If the damage took place in the current round and the active turntaker is on or after Peacemaker,
                            //check that it happened on a turn on or after Peacemaker's
                            if (round == round2)
                            {
                                return num >= turnTakerIndex;
                            }
                            return false;
                        }
                        //If the active turntaker is before or on Peacekeeper's turn,
                        //either it must have taken place in the previous round after Peacekeeper's start of turn
                        //or it must have taken place in this round, before or on Peacekeeper's turn
                        return (round == round2 - 1 && num >= turnTakerIndex) || (round == round2 && num <= turnTakerIndex);
                    };
                }
                return (T e) => false;
            }
            return (T e) => false;
        }
    }
}

