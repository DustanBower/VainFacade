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

        }

        //private DealDamageAction beingRedirected = null;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When another target would be dealt damage, you may destroy this card to redirect that damage to {CarnavalCharacter}. If you do, you may play 1 Trap and use 1 power."
            AddTrigger((DealDamageAction dda) => true, LogDealDamageActionResponse, TriggerType.Hidden, TriggerTiming.Before);
            AddTrigger((DealDamageAction dda) => dda.Target != base.CharacterCard && !dda.IsPretend && dda.Amount > 0, OptionalDestroyRedirectResponse, TriggerType.WouldBeDealtDamage, TriggerTiming.Before);
        }

        private IEnumerator LogDealDamageActionResponse(DealDamageAction dda)
        {
            Log.Debug("GotHimCardController.LogDealDamageActionResponse activated");
            Log.Debug("GotHimCardController.LogDealDamageActionResponse: dda: " + dda.ToString());
            Log.Debug("GotHimCardController.LogDealDamageActionResponse: dda.Target: " + dda.Target.Title);
            Log.Debug("GotHimCardController.LogDealDamageActionResponse: dda.Amount: " + dda.Amount.ToString());
            Log.Debug("GotHimCardController.LogDealDamageActionResponse: dda.Target != base.CharacterCard: " + (dda.Target != base.CharacterCard).ToString());
            Log.Debug("GotHimCardController.LogDealDamageActionResponse: !dda.IsPretend: " + (!dda.IsPretend).ToString());
            Log.Debug("GotHimCardController.LogDealDamageActionResponse: dda.Amount > 0: " + (dda.Amount > 0).ToString());
            yield break;
        }

        private IEnumerator OptionalDestroyRedirectResponse(DealDamageAction dda)
        {
            // "... you may destroy this card to redirect that damage to {CarnavalCharacter}. If you do, you may play 1 Trap and use 1 power."
            Log.Debug("GotHimCardController.OptionalDestroyRedirectResponse activated");
            //List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
            //Func<GameAction, IEnumerator> action = AddAfterDestroyedAction(TrapPowerResponse);
            IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, optional: true, responsibleCard: base.Card, postDestroyAction: () => TrapPowerResponse(dda), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
            //RemoveDestroyAction(BeforeOrAfter.After, action);
        }

        private IEnumerator TrapPowerResponse(DealDamageAction dda)
        {
            // "... to redirect that damage to {CarnavalCharacter}."
            IEnumerator redirectCoroutine = base.GameController.RedirectDamage(dda, base.CharacterCard, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(redirectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(redirectCoroutine);
            }
            // "If you do, you may play 1 Trap..."
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
