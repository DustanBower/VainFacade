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

        private DealDamageAction beingRedirected = null;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When another target would be dealt damage, you may destroy this card to redirect that damage to {CarnavalCharacter}. If you do, you may play 1 Trap and use 1 power."
            AddTrigger((DealDamageAction dda) => dda.Target != base.CharacterCard && !dda.IsPretend && dda.Amount > 0 && dda.DamageSource.IsInPlayAndHasGameText, OptionalDestroyRedirectResponse, new TriggerType[] { TriggerType.DestroySelf, TriggerType.RedirectDamage }, TriggerTiming.Before);
        }

        private IEnumerator OptionalDestroyRedirectResponse(DealDamageAction dda)
        {
            // "... you may destroy this card to redirect that damage to {CarnavalCharacter}. If you do, you may play 1 Trap and use 1 power."
            //List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
            Func<GameAction, IEnumerator> action = AddAfterDestroyedAction(TrapPowerResponse);
            IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, optional: true, responsibleCard: base.Card, postDestroyAction: () => base.GameController.RedirectDamage(dda, base.CharacterCard, cardSource: GetCardSource()), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
            RemoveDestroyAction(BeforeOrAfter.After, action);
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
