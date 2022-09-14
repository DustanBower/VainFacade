using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Burgess
{
    public class UniformCardController : BurgessUtilityCardController
    {
        public UniformCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether Burgess or a Backup has been dealt damage this turn since this entered play
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageThisTurn, base.CharacterCard.Title + " or a Backup has already been dealt damage this turn since " + base.Card.Title + " entered play.", "Neither " + base.CharacterCard.Title + " nor any Backups have been dealt damage this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected readonly string FirstDamageThisTurn = "FirstDamageThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn {BurgessCharacter} or a Backup is dealt damage, this card may deal the source of that damage 1 projectile damage."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(FirstDamageThisTurn) && dda.DidDealDamage && (dda.Target == base.CharacterCard || dda.Target.DoKeywordsContain(BackupKeyword)), ShootBackResponse, TriggerType.DealDamage, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageThisTurn), TriggerType.Hidden);
        }

        private IEnumerator ShootBackResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageThisTurn);
            // "... this card may deal the source of that damage 1 projectile damage."
            if (dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card.IsTarget && dda.DamageSource.Card.IsInPlayAndHasGameText)
            {
                Card attacker = dda.DamageSource.Card;
                IEnumerator damageCoroutine = DealDamage(base.Card, attacker, 1, DamageType.Projectile, optional: true, isCounterDamage: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            else
            {
                string message = "The " + base.Card.Title + " looks around for the source of damage, but there's nothing for them to shoot...";
                if (dda.DamageSource.IsCard && dda.DamageSource.Card.IsTarget)
                {
                    message = "The " + base.Card.Title + " looks around for the source of damage, but they've already been taken out...";
                }
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, you may draw a card."
            IEnumerator drawCoroutine = DrawCard(base.HeroTurnTaker, optional: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            yield break;
        }
    }
}
