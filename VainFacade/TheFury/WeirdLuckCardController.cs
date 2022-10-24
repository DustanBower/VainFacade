using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheFury
{
    public class WeirdLuckCardController : TheFuryUtilityCardController
    {
        public WeirdLuckCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When an effect would increase the next damage dealt to {TheFuryCharacter}, select a target other than {TheFuryCharacter}. Increase the next damage dealt to that target by 1 or increase the next damage dealt by that target by 1."
            AddTrigger((AddStatusEffectAction sa) => sa.StatusEffect is IncreaseDamageStatusEffect && (sa.StatusEffect as IncreaseDamageStatusEffect).TargetCriteria.IsSpecificCard == base.CharacterCard && (sa.StatusEffect as IncreaseDamageStatusEffect).NumberOfUses.HasValue, SelectTargetIncreaseResponse, TriggerType.CreateStatusEffect, TriggerTiming.Before);
        }

        private IEnumerator SelectTargetIncreaseResponse(GameAction ga)
        {
            // "... select a target other than {TheFuryCharacter}."
            List<SelectCardDecision> decisions = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.SelectTargetNoDamage, new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && c != base.CharacterCard), decisions, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            if (DidSelectCard(decisions))
            {
                // "Increase the next damage dealt to that target by 1 or increase the next damage dealt by that target by 1."
                Card selected = GetSelectedCard(decisions);
                if (selected != null)
                {
                    IEnumerable<Function> options = new Function[2]
                    {
                        new Function(DecisionMaker, "Increase the next damage dealt to " + selected.Title + " by 1", SelectionType.IncreaseDamage, () => IncreaseNextDamageTo(selected, 1, GetCardSource())),
                        new Function(DecisionMaker, "Increase the next damage dealt by " + selected.Title + " by 1", SelectionType.IncreaseDamage, () => IncreaseNextDamageBy(selected, 1, GetCardSource()))
                    };
                    SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, DecisionMaker, options, false, associatedCards: selected.ToEnumerable(), cardSource: GetCardSource());
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
