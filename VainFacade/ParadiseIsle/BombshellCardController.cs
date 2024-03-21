using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class BombshellCardController : ParadiseIsleUtilityCardController
    {
        public BombshellCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether this card is immune to damage
            SpecialStringMaker.ShowIfElseSpecialString(HasDealtDamageSinceEnteringPlay, () => base.Card.Title + " has dealt damage since she entered play, and is no longer immune to damage.", () => base.Card.Title + " hasn't dealt damage since she entered play, and is still immune to damage.", () => true).Condition = () => base.Card.IsInPlayAndHasGameText;
            // If in play: show whether a Conspirator has dealt damage this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstConspiratorDamage, "A Conspirator has already dealt damage this turn since " + base.Card.Title + " entered play.", "No Conspirators have dealt damage this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
            // If in play: show whether...
            // a) it's not the environment turn, so Bombshell won't deal damage to all non-Conspirators this turn regardless of what happens
            SpecialStringMaker.ShowSpecialString(() => base.Card.Title + " won't deal fire damage to all non-Conspirators this turn because it isn't the environment turn.", () => true).Condition = () => base.Card.IsInPlayAndHasGameText && !base.Game.ActiveTurnTaker.IsEnvironment;
            // b) it is the environment turn, but no targets have been dealt damage this turn since Bombshell entered play
            SpecialStringMaker.ShowSpecialString(() => "No targets have been dealt damage this environment turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText && base.Game.ActiveTurnTaker.IsEnvironment && TargetsDealtDamageThisEnvironmentTurn().Count() == 0;
            // c) it is the environment turn, and exactly 1 target has been dealt damage this turn since Bombshell entered play
            SpecialStringMaker.ShowSpecialString(() => "The only target to be dealt damage this environment turn since " + base.Card.Title + " entered play is " + TargetsDealtDamageThisEnvironmentTurn().FirstOrDefault().Title + ".").Condition = () => base.Card.IsInPlayAndHasGameText && base.Game.ActiveTurnTaker.IsEnvironment && TargetsDealtDamageThisEnvironmentTurn().Count() == 1;
            // d) it is the environment turn, and multiple targets have been dealt damage this turn since Bombshell entered play
            SpecialStringMaker.ShowSpecialString(() => "Multiple targets have been dealt damage this environment turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText && HasBeenSetToTrueThisTurn(FirstMultiTargetDamage);
        }

        public readonly string FirstConspiratorDamage = "FirstConspiratorDamageThisTurn";
        public readonly string FirstMultiTargetDamage = "FirstMultipleTargetDamageThisEnvironmentTurn";

        public bool HasDealtDamageSinceEnteringPlay()
        {
            PlayCardJournalEntry enteredPlay = base.GameController.Game.Journal.QueryJournalEntries((PlayCardJournalEntry e) => e.CardPlayed == base.Card).LastOrDefault();
            DealDamageJournalEntry dealtDamage = base.GameController.Game.Journal.MostRecentDealDamageEntry((DealDamageJournalEntry e) => e.SourceCard == base.Card && e.Amount > 0);
            if (enteredPlay != null)
            {
                int? entranceIndex = base.GameController.Game.Journal.GetEntryIndex(enteredPlay);
                int? damageIndex = base.GameController.Game.Journal.GetEntryIndex(dealtDamage);
                /*if (entranceIndex.HasValue)
                {
                    Log.Debug("BombshellCardController.HasDealtDamageSinceEnteringPlay: entranceIndex = " + entranceIndex.Value.ToString());
                }
                else
                {
                    Log.Debug("BombshellCardController.HasDealtDamageSinceEnteringPlay: entranceIndex.HasValue is false");
                }
                if (damageIndex.HasValue)
                {
                    Log.Debug("BombshellCardController.HasDealtDamageSinceEnteringPlay: damageIndex = " + damageIndex.Value.ToString());
                }
                else
                {
                    Log.Debug("BombshellCardController.HasDealtDamageSinceEnteringPlay: damageIndex.HasValue is false");
                }*/
                if (damageIndex.HasValue && entranceIndex.HasValue && damageIndex.Value > entranceIndex.Value)
                {
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<Card> TargetsDealtDamageThisEnvironmentTurn()
        {
            IEnumerable<Card> targetsHit = new List<Card>();
            if (base.Game.ActiveTurnTaker.IsEnvironment)
            {
                targetsHit = (from ddje in base.GameController.Game.Journal.DealDamageEntriesThisTurn() where ddje.Amount > 0 select ddje.TargetCard).Distinct();
            }
            return targetsHit;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "This card is immune to damage if it has not dealt damage since entering play."
            AddImmuneToDamageTrigger((DealDamageAction dda) => dda.Target == base.Card && !HasDealtDamageSinceEnteringPlay());
            // "The first time any target is dealt damage by any Conspirator each turn, {Bombshell} deals that target {H} projectile damage."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card.DoKeywordsContain(ConspiratorKeyword) && dda.DidDealDamage && !HasBeenSetToTrueThisTurn(FirstConspiratorDamage), SupportingFireResponse, TriggerType.DealDamage, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstConspiratorDamage), TriggerType.Hidden);
            // "The first time multiple targets are dealt damage during each environment turn, {Bombshell} deals each non-Conspirator target 2 fire damage."
            AddTrigger((DealDamageAction dda) => base.Game.ActiveTurnTaker.IsEnvironment && dda.DidDealDamage && TargetsDealtDamageThisEnvironmentTurn().Count() > 1 && !HasBeenSetToTrueThisTurn(FirstMultiTargetDamage), ExplosionResponse, TriggerType.DealDamage, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstMultiTargetDamage), TriggerType.Hidden);
        }

        private IEnumerator SupportingFireResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(FirstConspiratorDamage);
            // "... {Bombshell} deals that target {H} projectile damage."
            Card target = dda.Target;
            if (target.IsInPlayAndHasGameText && target.IsTarget)
            {
                IEnumerator damageCoroutine = DealDamage(base.Card, target, H, DamageType.Projectile, cardSource: GetCardSource());
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
                string message = base.Card.Title + " is ready to provide supporting fire, but the target has already been eliminated.";
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

        private IEnumerator ExplosionResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(FirstMultiTargetDamage);
            // "... {Bombshell} deals each non-Conspirator target 2 fire damage."
            IEnumerator damageCoroutine = base.GameController.DealDamage(DecisionMaker, base.Card, (Card c) => !c.DoKeywordsContain(ConspiratorKeyword), 2, DamageType.Fire, cardSource: GetCardSource());
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
