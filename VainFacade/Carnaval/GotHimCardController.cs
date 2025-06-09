using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Carnaval
{
    public class GotHimCardController : CarnavalUtilityCardController
    {
        public GotHimCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
        }

        private string BasicMode = "GotHimBasicMode";
        private string ModeSelected = "GotHimModeSelected";

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
            // "When another target would be dealt damage, you may redirect that damage to {CarnavalCharacter}."
            AddRedirectDamageTrigger(GotHimCriteria, () => base.CharacterCard, optional: true);
            // "After damage is redirected by this card, destroy this card."
            AddTrigger((DealDamageAction dda) => dda.DamageModifiers.Any((ModifyDealDamageAction mdda) => mdda is RedirectDamageAction && mdda.CardSource != null && mdda.CardSource.Card == base.Card), DestroyThisCardResponse, TriggerType.DestroySelf, TriggerTiming.After);
            // "After this card is destroyed, you may play 1 Trap and use 1 power."
            AddAfterDestroyedAction(TrapPowerResponse);

            //Ask what mode to use at the start of Carnaval's turn
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker && !IsPropertyTrue(ModeSelected), (PhaseChangeAction pca) => SetModeResponse(), TriggerType.Hidden);
            AddAfterLeavesPlayAction(() => ResetFlagAfterLeavesPlay(BasicMode));
            AddAfterLeavesPlayAction(() => ResetFlagAfterLeavesPlay(ModeSelected));
        }

        //Based on Diving Save
        private IEnumerator SetModeResponse()
        {
            SelectFunctionDecision selectControlMode = new SelectFunctionDecision(functionChoices: new List<Function>
            {
                new Function(base.HeroTurnTakerController, $"Basic Control: Only allow {this.Card.Title} to redirect damage aimed at hero targets", SelectionType.None, () => SetFlags(flag: true)),
                new Function(base.HeroTurnTakerController, $"Full Control: Allow {this.Card.Title} to redirect any damage", SelectionType.None, () => SetFlags(flag: false))
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

        private bool GotHimCriteria(DealDamageAction dda)
        {
            return dda.Target != base.CharacterCard && (IsHeroTarget(dda.Target) || !IsPropertyCurrentlyTrue(BasicMode));
        }

        private IEnumerator TrapPowerResponse(GameAction ga)
        {
            // "... you may play 1 Trap..."
            IEnumerator trapCoroutine = SelectAndPlayCardFromHand(DecisionMaker, cardCriteria: TrapCard);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(trapCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(trapCoroutine);
            }
            // "... and use 1 power."
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
}
