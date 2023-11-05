using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class PushBaseCharacterCardController:HeroCharacterCardController
	{
		public PushBaseCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}


        //Handle damagae prevention tracking for Field Cascade
        //Based on the handling for Unnatural Upheaval on Lifeline's character cards
        public override void AddTriggers()
        {
            base.AddTriggers();
            AddTrigger((CancelAction a) => !a.ActionToCancel.IsPretend && a.ActionToCancel is DealDamageAction && a.IsPreventEffect && ((DealDamageAction)a.ActionToCancel).Amount > 0 && a.CardSource != null && a.CardSource.Card.Owner == this.TurnTaker, CountPrevention,new TriggerType[] { TriggerType.HiddenLast }, TriggerTiming.Before);
            AddPhaseChangeTrigger((TurnTaker tt) => true, (Phase p) => true, (PhaseChangeAction pc) => pc.FromPhase != null && pc.FromPhase.TurnTaker != pc.ToPhase.TurnTaker, ResetPreventionCount, new TriggerType[1]
            {
            TriggerType.Hidden
            }, TriggerTiming.Before, ignoreBattleZone: true);
        }

        private IEnumerator CountPrevention(CancelAction a)
        {
            int value = GetCardPropertyJournalEntryInteger("FieldCascadePreventionCount").GetValueOrDefault(0) + ((DealDamageAction)a.ActionToCancel).Amount;
            Console.WriteLine($"Push prevented {a.ActionToCancel.ToString()} for {((DealDamageAction)a.ActionToCancel).Amount}. Total amount prevented this turn is {value}.");
            SetCardProperty("FieldCascadePreventionCount", value);
            yield return null;
        }

        private IEnumerator ResetPreventionCount(PhaseChangeAction p)
        {
            SetCardProperty("FieldCascadePreventionCount", 0);
            yield return null;
        }
    }
}

