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
    public class SweepRightCardController : PlaybookCardController
    {
        public SweepRightCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
            // Show hero target with second lowest HP
            SpecialStringMaker.ShowHeroTargetWithLowestHP(2);
            // If in play: show whether Blitz has been dealt damage this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageToBlitzThisTurn, base.CharacterCard.Title + " has already been dealt damage this turn since " + base.Card.Title + " entered play.", base.CharacterCard.Title + " has not been dealt damage this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
            // If in play: show whether Blitz has dealt damage to another target this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageByBlitzToOthersThisTurn, base.CharacterCard.Title + " has already dealt damage to another target this turn since " + base.Card.Title + " entered play.", base.CharacterCard.Title + " has not dealt damage to any other targets this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected readonly string FirstDamageToBlitzThisTurn = "FirstDamageToBlitzThisTurn";
        protected readonly string FirstDamageByBlitzToOthersThisTurn = "FirstDamageByBlitzToOthersThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time {BlitzCharacter} is dealt damage each turn, 1 player discards a card."
            AddTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && dda.DidDealDamage && !HasBeenSetToTrueThisTurn(FirstDamageToBlitzThisTurn), OnePlayerDiscardsResponse, TriggerType.DiscardCard, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageToBlitzThisTurn), TriggerType.Hidden);
            // "The first time {BlitzCharacter} would deal damage to a target other than {BlitzCharacter} each turn, redirect that damage to the hero target with the second lowest HP."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card == base.CharacterCard && dda.Target != base.CharacterCard && dda.Amount > 0 && !HasBeenSetToTrueThisTurn(FirstDamageByBlitzToOthersThisTurn), RedirectToSecondLowestResponse, TriggerType.RedirectDamage, TriggerTiming.Before);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageByBlitzToOthersThisTurn), TriggerType.Hidden);
        }

        private IEnumerator OnePlayerDiscardsResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageToBlitzThisTurn);
            // "... 1 player discards a card."
            IEnumerator discardCoroutine = base.GameController.SelectHeroToDiscardCard(DecisionMaker, optionalDiscardCard: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
        }

        private IEnumerator RedirectToSecondLowestResponse(DealDamageAction dda)
        {
            //Log.Debug("SweepRightCardController.RedirectToSecondLowestResponse: dda: " + dda.ToString());
            //Log.Debug("SweepRightCardController.RedirectToSecondLowestResponse: HasBeenSetToTrueThisTurn(FirstDamageByBlitzToOthersThisTurn): " + HasBeenSetToTrueThisTurn(FirstDamageByBlitzToOthersThisTurn).ToString());
            SetCardPropertyToTrueIfRealAction(FirstDamageByBlitzToOthersThisTurn);
            // "... redirect that damage to the hero target with the second lowest HP."
            if (dda.IsRedirectable)
            {
                List<Card> results = new List<Card>();
                IEnumerator findCoroutine = base.GameController.FindTargetWithLowestHitPoints(2, (Card c) => IsHeroTarget(c) && base.GameController.IsCardVisibleToCardSource(c, GetCardSource()), results, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(findCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(findCoroutine);
                }
                Card chosen = results.FirstOrDefault();
                if (chosen != null)
                {
                    IEnumerator redirectCoroutine = base.GameController.RedirectDamage(dda, chosen, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(redirectCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(redirectCoroutine);
                    }
                }
            }
        }
    }
}
