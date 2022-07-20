using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class AlterationWardCardController : EmanationCardController
    {
        public AlterationWardCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show whether Sphere has been dealt damage by a non-hero target this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(FirstDamageThisTurn), () => base.CharacterCard.Title + " has already been dealt damage by a non-hero target this turn since " + base.Card.Title + " entered play.", () => base.CharacterCard.Title + " has not been dealt damage by a non-hero target this turn since " + base.Card.Title + " entered play.");
        }

        protected const string FirstDamageThisTurn = "FirstDamageThisTurn";

        public override bool CanOrderAffectOutcome(GameAction action)
        {
            if (action is DealDamageAction)
            {
                return (action as DealDamageAction).Target == base.CharacterCard;
            }
            return false;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {Sphere} would be dealt exactly 1 damage, prevent it."
            AddPreventDamageTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && dda.Amount == 1, isPreventEffect: true);
            // "The first time each turn {Sphere} is dealt damage by a non-hero target, draw a card."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(FirstDamageThisTurn) && dda.Target == base.CharacterCard && dda.DamageSource != null && dda.DamageSource.IsTarget && !dda.DamageSource.IsHero, DrawResponse, TriggerType.DrawCard, TriggerTiming.After);
            AddAfterLeavesPlayAction(() => ResetFlagAfterLeavesPlay(FirstDamageThisTurn));
        }

        private IEnumerator DrawResponse(DealDamageAction dda)
        {
            if (!HasBeenSetToTrueThisTurn(FirstDamageThisTurn))
            {
                SetCardPropertyToTrueIfRealAction(FirstDamageThisTurn);
                // "... draw a card."
                IEnumerator drawCoroutine = base.GameController.DrawCard(base.HeroTurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawCoroutine);
                }
            }
            yield break;
        }
    }
}
