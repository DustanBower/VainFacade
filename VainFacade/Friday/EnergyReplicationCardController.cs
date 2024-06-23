using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace VainFacadePlaytest.Friday
{
	public class EnergyReplicationCardController:CardController
	{
		public EnergyReplicationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisRound(OneTimePerRoundKey), () => $"{this.Card.Title} has been used this round", () => $"{this.Card.Title} has not been used this round").Condition = () => this.Card.IsInPlayAndHasGameText;
		}

        private string OneTimePerRoundKey = "OneTimePerRoundKey";

        //private bool HasBeenUsedThisRound()
        //{
        //    return base.GameController.Game.Journal.QueryJournalEntries((CardPropertiesJournalEntry e) => e.Key == OneTimePerRoundKey).Where(base.GameController.Game.Journal.ThisRound<CardPropertiesJournalEntry>()).Any();
        //}

        public override void AddTriggers()
        {
            //Increase Lightning Damage dealt to {Friday} by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DamageType == DamageType.Lightning, (DealDamageAction dd) => 1);

            //Once per round, when another target deals damage, {Friday} may deal 1 target X damage of the same type, where X = the amount of damage dealt or 3, whichever is lower.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => !HasBeenSetToTrueThisRound(OneTimePerRoundKey) && (dd.DamageSource.IsTarget || dd.DamageSource.IsHeroCharacterCard) && dd.DamageSource.Card != this.CharacterCard && dd.DidDealDamage, DamageResponse, TriggerType.DealDamage, TriggerTiming.After);
            AddAfterLeavesPlayAction(ResetFlags, TriggerType.Hidden);
        }
        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            int amount = Math.Min(3, dd.Amount);

            List<YesNoCardDecision> storedResults = new List<YesNoCardDecision>();
            IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.DealDamage, this.Card, new DealDamageAction(base.GameController, new DamageSource(base.GameController, this.CharacterCard), null, amount, dd.DamageType), storedResults, null, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidPlayerAnswerYes(storedResults))
            {
                SetCardPropertyToTrueIfRealAction(OneTimePerRoundKey);
                coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), amount, dd.DamageType, 1, false, 0, cardSource: GetCardSource());
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

        private IEnumerator ResetFlags(GameAction ga)
        {
            IEnumerator coroutine = ResetFlagAfterLeavesPlay(OneTimePerRoundKey);
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

