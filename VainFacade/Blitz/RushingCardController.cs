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
    public class RushingCardController : PlaybookCardController
    {
        public RushingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether Blitz has already dealt lightning damage to a hero target this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstHeroZappedThisTurn, base.CharacterCard.Title + " has already dealt lightning damage to a hero target this turn since " + base.Card.Title + " entered play.", base.CharacterCard.Title + " has not dealt lightning damage to a hero target this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected readonly string FirstHeroZappedThisTurn = "FirstHeroZappedThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn {BlitzCharacter} deals a hero target lightning damage, {H - 2} players discard a card, then play the top card of the villain deck."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card == base.CharacterCard && dda.Target.IsHero && dda.DamageType == DamageType.Lightning && dda.DidDealDamage && !HasBeenSetToTrueThisTurn(FirstHeroZappedThisTurn), DiscardPlayResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PlayCard }, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstHeroZappedThisTurn), TriggerType.Hidden);
        }

        private IEnumerator DiscardPlayResponse(DealDamageAction dda)
        {
            // "... {H - 2} players discard a card, ..."
            IEnumerator discardCoroutine = base.GameController.EachPlayerDiscardsCards(1, 1, requiredNumberOfHeroes: H - 2, showCounter: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "... then play the top card of the villain deck."
            IEnumerator playCoroutine = PlayTheTopCardOfTheVillainDeckResponse(dda);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }
    }
}
