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

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When another target would be dealt damage, you may redirect that damage to {CarnavalCharacter}."
            AddRedirectDamageTrigger((DealDamageAction dda) => dda.Target != base.CharacterCard, () => base.CharacterCard, optional: true);
            // "After damage is redirected by this card, destroy this card."
            AddTrigger((DealDamageAction dda) => dda.DamageModifiers.Any((ModifyDealDamageAction mdda) => mdda is RedirectDamageAction && mdda.CardSource != null && mdda.CardSource.Card == base.Card), DestroyThisCardResponse, TriggerType.DestroySelf, TriggerTiming.After);
            // "After this card is destroyed, you may play 1 Trap and use 1 power."
            AddAfterDestroyedAction(TrapPowerResponse);
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
