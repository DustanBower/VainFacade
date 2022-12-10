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
    public class PortableSnareCardController : CardController
    {
        public PortableSnareCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(false);
            // "If this card is not next to a target, the first time a villain target would deal damage, move this card next to that target."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card.IsVillainTarget && !IsThisNextToATarget(), (DealDamageAction dda) => base.GameController.MoveCard(base.TurnTakerController, base.Card, dda.DamageSource.Card.NextToLocation, playCardIfMovingToPlayArea: false, showMessage: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource()), new TriggerType[] {TriggerType.MoveCard, TriggerType.WouldBeDealtDamage}, TriggerTiming.Before);
            // "Reduce damage dealt by the target next to this card by 3."
            AddReduceDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && IsThisCardNextToCard(dda.DamageSource.Card), (DealDamageAction dda) => 3);
            // "At the start of that target's turn, if this card is next to a target, {CarnavalCharacter} deals that target 2 melee damage and destroys this card."
            AddStartOfTurnTrigger((TurnTaker tt) => IsThisNextToATarget() && GetCardThisCardIsNextTo().Owner == tt, CrunchResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroySelf });
        }

        private bool IsThisNextToATarget()
        {
            return GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().IsTarget;
        }

        private IEnumerator CrunchResponse(PhaseChangeAction pca)
        {
            // "... {CarnavalCharacter} deals that target 2 melee damage..."
            IEnumerator meleeCoroutine = DealDamage(base.CharacterCard, GetCardThisCardIsNextTo(), 2, DamageType.Melee, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(meleeCoroutine);
            }
            // "... and destroys this card."
            IEnumerator destructCoroutine = DestroyThisCardResponse(pca);
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
