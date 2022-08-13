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
    public class WitnessStatementsCardController : CardController
    {
        public WitnessStatementsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether attached target has dealt damage this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageFromTargetThisTurn, GetCardThisCardIsNextTo().Title + " has already dealt damage this turn since " + base.Card.Title + " entered play.", GetCardThisCardIsNextTo().Title + " has not dealt damage this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText && base.Card.Location.IsNextToCard;
        }

        protected readonly string FirstDamageFromTargetThisTurn = "FirstDamageFromTargetThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn that target deals damage, 1 hero may use a power."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(FirstDamageFromTargetThisTurn) && dda.DidDealDamage && dda.DamageSource.IsCard && dda.DamageSource.IsSameCard(GetCardThisCardIsNextTo()), GrantPowerResponse, TriggerType.UsePower, TriggerTiming.After);
            // "When that target leaves play, destroy this card."
            AddIfTheTargetThatThisCardIsNextToLeavesPlayDestroyThisCardTrigger();
        }

        private IEnumerator GrantPowerResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageFromTargetThisTurn);
            // "... 1 hero may use a power."
            IEnumerator powerCoroutine = base.GameController.SelectHeroToUsePower(base.HeroTurnTakerController, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(powerCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(powerCoroutine);
            }
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card next to a non-hero target."
            IEnumerator selectCoroutine = SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => c.IsTarget && !c.IsHero, "non-hero targets", useCardsSuffix: false), storedResults, isPutIntoPlay, decisionSources);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }
    }
}
