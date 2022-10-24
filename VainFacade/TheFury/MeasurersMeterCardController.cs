using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheFury
{
    public class MeasurersMeterCardController : TheFuryUtilityCardController
    {
        public MeasurersMeterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Select up to 3 targets."
            int numTargets = GetPowerNumeral(0, 3);
            int amtModified = GetPowerNumeral(1, 1);
            List<SelectCardsDecision> decisions = new List<SelectCardsDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardsAndStoreResults(DecisionMaker, SelectionType.SelectTargetNoDamage, (Card c) => c.IsInPlay && c.IsTarget, numTargets, decisions, false, requiredDecisions: 0, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            if (decisions.Count() > 0)
            {
                // "Increase or reduce the next damage dealt to each selected target by 1."
                IEnumerable<Card> targets = from d in decisions.FirstOrDefault().SelectCardDecisions where d.SelectedCard != null select d.SelectedCard;
                for (int i = 0; i < targets.Count(); i++)
                {
                    Card currentTarget = targets.ElementAt(i);
                    IEnumerable<Function> functionChoices = new Function[2]
                    {
                        new Function(DecisionMaker, "Increase the next damage dealt to " + currentTarget.Title + " by " + amtModified.ToString(), SelectionType.IncreaseDamage, () => IncreaseNextDamageTo(currentTarget, amtModified, GetCardSource())),
                        new Function(DecisionMaker, "Reduce the next damage dealt to " + currentTarget.Title + " by " + amtModified.ToString(), SelectionType.ReduceNextDamageTaken, () => ReduceNextDamageTo(currentTarget, amtModified, GetCardSource()))
                    };
                    SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, DecisionMaker, functionChoices, false, associatedCards: currentTarget.ToEnumerable(), cardSource: GetCardSource());
                    IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(chooseCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(chooseCoroutine);
                    }
                }
            }
        }
    }
}
