using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Friday
{
	public class DracimpR9A3ArmCannonCardController:CardController
	{
		public DracimpR9A3ArmCannonCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            //Show all damage types that have been dealt since the end of your last turn
            AllowFastCoroutinesDuringPretend = false;
            base.SpecialStringMaker.ShowSpecialString(() => HadLastTurn() ? SpecialStringText() : TextIfNoLastTurn());
        }

        private bool HadLastTurn()
        {
            return base.Game.Journal.PhaseChangeEntries().Where(base.Game.Journal.SinceLastTurn<PhaseChangeJournalEntry>(this.TurnTaker)).Any();
        }

        private string SpecialStringText()
        {
            return $"Damage types dealt by sources other than {this.CharacterCard.Title} since the end of {this.TurnTaker.Name}'s last turn: {GetDamageTypesSinceLastTurn().Select((DamageType t) => t.ToString()).ToCommaList()}";
        }

        private string TextIfNoLastTurn()
        {
            return $"There has not been a previous turn for {this.TurnTaker.Name}";
        }

        public Guid? DecidedIncrease { get; set; }

        private IEnumerable<DamageType> GetDamageTypesSinceLastTurn()
        {
            //"Since the end of Friday's last turn" does not include damage dealt during Friday's end-of-turn phase
            //In OblivAeon mode, this sees all damage that was dealt in both battle zones since Friday's last turn
            return base.GameController.Game.Journal.QueryJournalEntries((DealDamageJournalEntry e) => e.Amount > 0 && e.SourceCard != this.CharacterCard).Where(base.GameController.Game.Journal.SinceLastTurn<DealDamageJournalEntry>(this.TurnTaker)).Select((DealDamageJournalEntry dde) => dde.DamageType).Distinct();
        }

        public override void AddTriggers()
        {
            //When {Friday} would deal a type of damage that has been dealt by a source other than {Friday} since the end of your last turn, you may increase that damage by 1.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && GetDamageTypesSinceLastTurn().Count() > 0 && GetDamageTypesSinceLastTurn().Contains(dd.DamageType) && !dd.IsUnincreasable, IncreaseResponse, new TriggerType[1] { TriggerType.IncreaseDamage }, TriggerTiming.Before);
        }

        private IEnumerator IncreaseResponse(DealDamageAction dd)
        {
            if (!DecidedIncrease.HasValue || DecidedIncrease.Value != dd.InstanceIdentifier)
            {
                List<YesNoCardDecision> storedResults = new List<YesNoCardDecision>();
                IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.Custom, this.Card, dd, storedResults, null, GetCardSource());
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
                    DecidedIncrease = dd.InstanceIdentifier;
                }
            }
            if (DecidedIncrease.HasValue && DecidedIncrease.Value == dd.InstanceIdentifier)
            {
                Log.Debug("Increasing Friday's damage by 1");
                IEnumerator coroutine3 = base.GameController.IncreaseDamage(dd, 1, false, GetCardSource());
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
                DecidedIncrease = null;
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decision is YesNoCardDecision)
            {
                return new CustomDecisionText(
                $"Do you want to increase this damage by 1?",
                $"{decision.DecisionMaker.Name} is deciding whether to increase this damage by 1.",
                $"Vote for whether to increase this damage by 1.",
                $"whether to increase this damage by 1."
                );
            }
            else if (decision is SelectDamageTypeDecision)
            {
                string message = "";
                if (GetDamageTypesSinceLastTurn().Count() > 0)
                {
                    string types = GetDamageTypesSinceLastTurn().Select((DamageType t) => t.ToString()).ToCommaList();
                    message = " " + types + " can be increased.";
                }

                //Console.WriteLine($"Returning custom decision text for Arm Cannon: choose a damage type.{message}");

                return new CustomDecisionText(
                $"choose a damage type.{message}",
                $"{decision.DecisionMaker.Name} is choosing a damage type.{message}",
                $"Vote for a damage type.{message}",
                $"a damage type.{message}"
                );

                //return new CustomDecisionText(
                //$"choose a damage type. Extra text",
                //$"Friday is choosing a damage type.",
                //$"Vote for a damage type.",
                //$"a damage type."
                //);
            }
            return null;
            
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Select a damage type, then repeat the following text 3 times: {Friday} deals 1 target 1 damage of that type. Reduce damage dealt to that target by 1 this turn.
            int num1 = GetPowerNumeral(0, 3);
            int num2 = GetPowerNumeral(1, 1);
            int num3 = GetPowerNumeral(2, 1);
            int num4 = GetPowerNumeral(3, 1);

            List<SelectDamageTypeDecision> results = new List<SelectDamageTypeDecision>();
            IEnumerator coroutine = base.GameController.SelectDamageType(DecisionMaker, results, selectionType: SelectionType.Custom, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            DamageType? type = GetSelectedDamageType(results);

            if (type != null)
            {
                for (int i = 0; i < num1; i++)
                {
                    IEnumerator damage = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), (Card c) => num3, type.Value, () => num2, false, num2, addStatusEffect: (DealDamageAction dd) => ReduceDamageStatus(dd, num4), cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(damage);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(damage);
                    }
                }
            }
        }

        private IEnumerator ReduceDamageStatus(DealDamageAction dd, int reduceBy)
        {
            ReduceDamageStatusEffect effect = new ReduceDamageStatusEffect(reduceBy);
            effect.TargetCriteria.IsSpecificCard = dd.Target;
            effect.UntilThisTurnIsOver(base.Game);
            effect.UntilTargetLeavesPlay(dd.Target);
            IEnumerator coroutine = AddStatusEffect(effect);
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

