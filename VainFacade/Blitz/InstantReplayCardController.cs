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
    public class InstantReplayCardController : PlaybookCardController
    {
        public InstantReplayCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether Blitz has already dealt damage to another target this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageToNonSelfThisTurn, base.CharacterCard.Title + " has already dealt damage to another target this turn since " + base.Card.Title + " entered play.", base.CharacterCard.Title + " has not dealt damage to another target this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected readonly string FirstDamageToNonSelfThisTurn = "FirstDamageToNonSelfThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn {BlitzCharacter} deals a target other than himself damage, {BlitzCharacter} deals that target 2 melee and 2 lightning damage."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card == base.CharacterCard && dda.Target != base.CharacterCard && dda.DidDealDamage && !HasBeenSetToTrueThisTurn(FirstDamageToNonSelfThisTurn), ExtraDamageResponse, TriggerType.DealDamage, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageToNonSelfThisTurn), TriggerType.Hidden);
        }

        private IEnumerator ExtraDamageResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageToNonSelfThisTurn);
            // "... {BlitzCharacter} deals that target 2 melee and 2 lightning damage."
            List<DealDamageAction> instances = new List<DealDamageAction>();
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 2, DamageType.Melee));
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 2, DamageType.Lightning));
            IEnumerator damageCoroutine = DealMultipleInstancesOfDamage(instances, (Card c) => c == dda.Target);
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
