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
    public class BastionPDVestCardController : CardController
    {
        public BastionPDVestCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether Burgess has been dealt damage this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageThisTurn, base.CharacterCard.Title + " has already been dealt damage this turn since " + base.Card.Title + " entered play.", base.CharacterCard.Title + " has not been dealt damage this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
            // If in play: show whether Burgess has been dealt projectile damage this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstProjectileThisTurn, base.CharacterCard.Title + " has already been dealt projectile damage this turn since " + base.Card.Title + " entered play.", base.CharacterCard.Title + " has not been dealt projectile damage this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected const string FirstDamageThisTurn = "FirstDamageThisTurn";
        protected const string FirstProjectileThisTurn = "FirstProjectileThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time {BurgessCharacter} is dealt damage each turn, he regains 1 HP."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(FirstDamageThisTurn) && dda.Target == base.CharacterCard && dda.DidDealDamage, FirstDamageResponse, TriggerType.GainHP, TriggerTiming.After);
            // "The first time {BurgessCharacter} is dealt projectile damage each turn, he regains 1 HP."
            AddTrigger((DealDamageAction dda) => HasBeenSetToTrueThisTurn(FirstDamageThisTurn) && !HasBeenSetToTrueThisTurn(FirstProjectileThisTurn) && dda.Target == base.CharacterCard && dda.DidDealDamage && dda.DamageType == DamageType.Projectile, FirstShotResponse, TriggerType.GainHP, TriggerTiming.After);
        }

        private IEnumerator FirstDamageResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageThisTurn);
            // "... he regains 1 HP."
            IEnumerator healCoroutine = base.GameController.GainHP(base.CharacterCard, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            // If the first damage WAS projectile damage...
            if (!HasBeenSetToTrueThisTurn(FirstProjectileThisTurn) && dda.DamageType == DamageType.Projectile)
            {
                IEnumerator shotResponse = FirstShotResponse(dda);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shotResponse);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shotResponse);
                }
            }
        }

        private IEnumerator FirstShotResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(FirstProjectileThisTurn);
            // "... he regains 1 HP."
            IEnumerator healCoroutine = base.GameController.GainHP(base.CharacterCard, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
        }


    }
}
