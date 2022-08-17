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
    public class S9PServicePistolCardController : BurgessUtilityCardController
    {
        public S9PServicePistolCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether Burgess or any Backups have been dealt damage this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageThisTurn, base.CharacterCard.Title + " or a Backup has already been dealt damage this turn since " + base.Card.Title + " entered play.", "Neither " + base.CharacterCard.Title + " nor any Backups have been dealt damage this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected readonly string FirstDamageThisTurn = "FirstDamageThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn {BurgessCharacter} or a Backup is dealt damage, {BurgessCharacter} may deal the source of that damage 1 psychic or 2 projectile damage."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(FirstDamageThisTurn) && dda.DidDealDamage && (dda.Target == base.CharacterCard || dda.Target.DoKeywordsContain(BackupKeyword)), PullTheGunResponse, TriggerType.DealDamage, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageThisTurn), TriggerType.Hidden);
        }

        private IEnumerator PullTheGunResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageThisTurn);
            // "... {BurgessCharacter} may deal the source of that damage 1 psychic or 2 projectile damage."
            if (dda.DamageSource.IsCard && dda.DamageSource.Card.IsTarget && dda.DamageSource.Card.IsInPlayAndHasGameText)
            {
                Card attacker = dda.DamageSource.Card;
                List<Function> options = new List<Function>();
                options.Add(new Function(base.HeroTurnTakerController, base.CharacterCard.Title + " deals " + attacker.Title + " 1 psychic damage", SelectionType.DealDamage, () => DealDamage(base.CharacterCard, attacker, 1, DamageType.Psychic, isCounterDamage: true, cardSource: GetCardSource())));
                options.Add(new Function(base.HeroTurnTakerController, base.CharacterCard.Title + " deals " + attacker.Title + " 2 projectile damage", SelectionType.DealDamage, () => DealDamage(base.CharacterCard, attacker, 2, DamageType.Projectile, isCounterDamage: true, cardSource: GetCardSource())));
                SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, options, true, associatedCards: attacker.ToEnumerable(), cardSource: GetCardSource());
                IEnumerator selectCoroutine = base.GameController.SelectAndPerformFunction(choice);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selectCoroutine);
                }
            }
            else
            {
                string message = base.TurnTaker.Name + " pulls his service weapon, but there's nothing for him to shoot...";
                if (dda.DamageSource.IsCard && dda.DamageSource.Card.IsTarget)
                {
                    message = base.TurnTaker.Name + " pulls his service weapon, but the attacker has already been taken out...";
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
    }
}
