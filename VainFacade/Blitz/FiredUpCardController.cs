using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Blitz
{
    public class FiredUpCardController : BlitzUtilityCardController
    {
        public FiredUpCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether Blitz has already been dealt damage by another target this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstNonSelfDamageThisTurn, base.CharacterCard.Title + " has already been dealt damage by another target this turn.", base.CharacterCard.Title + " has not been dealt damage by any other target this turn.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected readonly string FirstNonSelfDamageThisTurn = "FirstNonSelfDamageThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn {BlitzCharacter} is dealt damage by a target other than {BlitzCharacter}, he deals the source of that damage 3 lightning damage."
            AddTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && dda.DidDealDamage && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card != null && dda.DamageSource.Card != base.CharacterCard && !HasBeenSetToTrueThisTurn(FirstNonSelfDamageThisTurn), RetaliateResponse, TriggerType.DealDamage, TriggerTiming.After);
            // "At the start of the villain turn, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, reveal cards from the top of the villain deck until a Circuit is revealed. Put it into play. Discard the other revealed cards."
            IEnumerator revealCoroutine = RevealCards_PutSomeIntoPlay_DiscardRemaining(base.TurnTakerController, base.TurnTaker.Deck, null, IsCircuit, revealUntilNumberOfMatchingCards: 1);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
        }

        private IEnumerator RetaliateResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(FirstNonSelfDamageThisTurn);
            // "... he deals the source of that damage 3 lightning damage."
            if (dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card.IsTarget && dda.DamageSource.Card.IsInPlayAndHasGameText)
            {
                Card attacker = dda.DamageSource.Card;
                IEnumerator damageCoroutine = DealDamage(base.CharacterCard, attacker, 3, DamageType.Lightning, isCounterDamage: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
        }
    }
}
