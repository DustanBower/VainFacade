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
    public class UpdatedProfileCardController : CardController
    {
        public UpdatedProfileCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        private ITrigger _reduceTrigger;

        public override bool AllowFastCoroutinesDuringPretend => false;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When that target deals damage to a target other than the hero target with the highest HP, you may discard 2 cards. If you do, redirect that damage to the hero target with the highest HP. Reduce damage redirected this way by 1."
            AddTrigger((DealDamageAction dda) => dda.DamageSource.IsCard && dda.DamageSource.Card == GetCardThisCardIsNextTo() && (!CanCardBeConsideredHighestHitPoints(dda.Target, (Card c) => c.IsHero && c.IsTarget) || !IsHighestHitPointsUnique((Card c) => c.IsHero)), DiscardToRedirectReduceResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.WouldBeDealtDamage, TriggerType.RedirectDamage, TriggerType.ReduceDamage }, TriggerTiming.Before);
            // "When that target leaves play, destroy this card."
            AddIfTheTargetThatThisCardIsNextToLeavesPlayDestroyThisCardTrigger();
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card next to a non-hero target."
            IEnumerator selectCoroutine = SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => c.IsTarget && !c.IsHero, "non-hero", singular: "target", plural: "targets"), storedResults, isPutIntoPlay, decisionSources);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }

        private IEnumerator DiscardToRedirectReduceResponse(DealDamageAction dda)
        {
            List<bool> isHighest = new List<bool>();
            IEnumerator checkCoroutine = DetermineIfGivenCardIsTargetWithLowestOrHighestHitPoints(dda.Target, true, (Card c) => c.IsHero && base.GameController.IsCardVisibleToCardSource(c, GetCardSource()), dda, isHighest);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(checkCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(checkCoroutine);
            }
            if (!isHighest.First())
            {
                // "... you may discard 2 cards."
                List<DiscardCardAction> discards = new List<DiscardCardAction>();
                IEnumerator discardCoroutine = SelectAndDiscardCards(base.HeroTurnTakerController, 2, optional: true, storedResults: discards, gameAction: dda, responsibleTurnTaker: base.TurnTaker);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
                // "If you do, redirect that damage to the hero target with the highest HP."
                if (DidDiscardCards(discards, 2))
                {
                    List<Card> chosenHighest = new List<Card>();
                    IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => c.IsHero && c != dda.Target && base.GameController.IsCardVisibleToCardSource(c, GetCardSource()), chosenHighest, dda, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(findCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(findCoroutine);
                    }
                    if (chosenHighest.Count() > 0)
                    {
                        Card newTarget = chosenHighest.FirstOrDefault();
                        IEnumerator redirectCoroutine = base.GameController.RedirectDamage(dda, newTarget, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(redirectCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(redirectCoroutine);
                        }
                        // "Reduce damage redirected this way by 1."
                        IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, 1, _reduceTrigger, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(reduceCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(reduceCoroutine);
                        }
                    }
                }
            }
            else
            {
                yield break;
            }
        }
    }
}
