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
    public class AllottedTimeCardController : TheFuryUtilityCardController
    {
        public AllottedTimeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        private Card ProtectedCard { get; set; }
        private Guid ReactedDamage { get; set; }

        private string BasicMode = "AllottedTimeBasicMode";
        private string ModeSelected = "AllotedTimeModeSelected";

        public override IEnumerator Play()
        {
            if (!IsPropertyCurrentlyTrue(ModeSelected))
            {
                IEnumerator coroutine = SetModeResponse();
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

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a target would be dealt damage, you may make that target indestructible this turn."
            AddTrigger<DealDamageAction>(AllottedTimeCriteria, MayProtectResponse, TriggerType.CreateStatusEffect, TriggerTiming.Before);
            // "Resolve that damage normally, then that target regains X HP and destroys this card, where X = 1 plus the damage dealt this way."
            AddTrigger((DealDamageAction dda) => !dda.IsPretend && dda.InstanceIdentifier == ReactedDamage, HealDestructResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.DestroySelf }, TriggerTiming.After);

            //Ask what mode to use at the start of Fury's turn
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker && !IsPropertyTrue(ModeSelected), (PhaseChangeAction pca) => SetModeResponse(), TriggerType.Hidden);
            AddAfterLeavesPlayAction(() => ResetFlagAfterLeavesPlay(BasicMode));
            AddAfterLeavesPlayAction(() => ResetFlagAfterLeavesPlay(ModeSelected));
        }

        //Based on Diving Save
        private IEnumerator SetModeResponse()
        {
            SelectFunctionDecision selectControlMode = new SelectFunctionDecision(functionChoices: new List<Function>
            {
                new Function(base.HeroTurnTakerController, $"Basic Control: Only allow {this.Card.Title} to make hero targets indestructible", SelectionType.None, () => SetFlags(flag: true)),
                new Function(base.HeroTurnTakerController, $"Full Control: Allow {this.Card.Title} to make any target indestructible", SelectionType.None, () => SetFlags(flag: false))
            }, gameController: base.GameController, hero: base.HeroTurnTakerController, optional: false, gameAction: null, noSelectableFunctionMessage: null, associatedCards: null, cardSource: GetCardSource());
            IEnumerator performFunction = base.GameController.SelectAndPerformFunction(selectControlMode, null, new Card[1] { base.Card });
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(performFunction);
            }
            else
            {
                base.GameController.ExhaustCoroutine(performFunction);
            }
        }

        //Based on Diving Save
        private IEnumerator SetFlags(bool flag)
        {
            IEnumerable<Card> GotHim = base.TurnTaker.GetCardsByIdentifier(this.Card.Identifier);
            foreach (Card ds in GotHim)
            {
                base.GameController.FindCardController(ds).SetCardProperty(BasicMode, flag);
                base.GameController.FindCardController(ds).SetCardProperty(ModeSelected, value: true);
            }
            yield return null;
        }

        private bool AllottedTimeCriteria(DealDamageAction dda)
        {
            return !dda.IsPretend && dda.IsSuccessful && dda.CanDealDamage && dda.Amount > 0 && (IsHeroTarget(dda.Target) || !IsPropertyCurrentlyTrue(BasicMode));
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Do you want to make this target indestructible?", "deciding whether to make a target indestructible", "Vote for whether to make this target indestructible", "whether to make a target indestructible");
        }

        private IEnumerator MayProtectResponse(DealDamageAction dda)
        {
            // "... you may make that target indestructible this turn."
            YesNoCardDecision choice = new YesNoCardDecision(base.GameController, DecisionMaker, SelectionType.Custom, dda.Target, dda, cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.MakeDecisionAction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (DidPlayerAnswerYes(choice))
            {
                ReactedDamage = dda.InstanceIdentifier;
                ProtectedCard = dda.Target;

                IEnumerator protectCoroutine = MakeIndestructibleThisTurn(dda.Target, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(protectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(protectCoroutine);
                }
            }
        }

        private IEnumerator HealDestructResponse(DealDamageAction dda)
        {
            // "... then that target regains X HP and destroys this card, where X = 1 plus the damage dealt this way."
            if (dda.DidDealDamage)
            {
                int x = 1 + dda.Amount;
                IEnumerator healCoroutine = base.GameController.GainHP(ProtectedCard, x, cardSource: GetCardSource());
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
                string message = "No damage was dealt, so " + ProtectedCard.Title + " does not regain HP.";
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(message, Priority.High, GetCardSource(), ProtectedCard.ToEnumerable(), true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
        }
    }
}
