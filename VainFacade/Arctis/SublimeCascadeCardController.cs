using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class SublimeCascadeCardController:ArctisCardUtilities
	{
		public SublimeCascadeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstColdDamage);
		}

        private string FirstColdDamage = "FirstColdDamage";

        public override void AddTriggers()
        {
            //Once per turn, when {Arctis} is dealt cold damage, you may put a card from your hand into play for each point of damage {Arctis} is dealt this way.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DamageType == DamageType.Cold && !IsPropertyTrue(FirstColdDamage) && dd.DidDealDamage, DamageResponse, TriggerType.PutIntoPlay, TriggerTiming.After);
            ResetFlagAfterLeavesPlay(FirstColdDamage);
        }

        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            int amount = dd.Amount;
            List<PlayCardAction> results = new List<PlayCardAction>();
            IEnumerator coroutine = SelectAndPlayCardsFromHandModified(DecisionMaker, amount, false, 0, isPutIntoPlay: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        //Modified to set the flag after playing the first card
        private IEnumerator SelectAndPlayCardsFromHandModified(HeroTurnTakerController hero, int numberOfCards, bool optional, int? requiredCards = null, LinqCardCriteria cardCriteria = null, bool isPutIntoPlay = false, List<PlayCardAction> storedResults = null, CardSource cardSource = null, Func<int> dynamicNumberOfCards = null, Func<bool> cancelDecisionsIfTrue = null)
        {
            Func<Card, bool> func = (Card card) => card.Location == hero.HeroTurnTaker.Hand;
            string criteriaDescription = null;
            if (cardCriteria != null)
            {
                func = (Card card) => card.Location == hero.HeroTurnTaker.Hand && cardCriteria.Criteria(card);
                criteriaDescription = cardCriteria.GetDescription();
            }
            string tryToPlayCardMessage = base.GameController.GetTryToPlayCardMessage(hero, isPutIntoPlay, func, criteriaDescription);
            if (tryToPlayCardMessage == null)
            {
                SelectionType type = (isPutIntoPlay ? SelectionType.PutIntoPlay : SelectionType.PlayCard);
                SelectCardsDecision selectCardsDecision = new SelectCardsDecision(this.GameController, hero, func, type, numberOfCards, optional, requiredCards, eliminateOptions: false, allowAutoDecide: false, allAtOnce: false, dynamicNumberOfCards, null, null, null, cardSource);
                return base.GameController.SelectCardsAndDoAction(selectCardsDecision, delegate (SelectCardDecision d)
                {
                    GameController gameController = this.GameController;
                    HeroTurnTakerController turnTakerController = hero;
                    Card selectedCard = d.SelectedCard;
                    bool isPutIntoPlay2 = isPutIntoPlay;
                    List<PlayCardAction> storedResults2 = storedResults;
                    CardSource cardSource2 = cardSource;
                    SetCardProperty(FirstColdDamage, true);
                    return gameController.PlayCard(turnTakerController, selectedCard, isPutIntoPlay2, null, optional: false, null, null, evenIfAlreadyInPlay: false, null, storedResults2, null, associateCardSource: false, fromBottom: false, canBeCancelled: true, cardSource2);
                }, null, cancelDecisionsIfTrue);
            }
            return base.GameController.SendMessageAction(tryToPlayCardMessage, Priority.High, cardSource);
        }

        //This uses a different decision structure, where you are asked yes/no if you want to put up to X cards in play
        //private IEnumerator DamageResponse(DealDamageAction dd)
        //{
        //    int amount = dd.Amount;
        //    List<YesNoCardDecision> results = new List<YesNoCardDecision>();
        //    IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.Custom, this.Card, dd, results, cardSource: GetCardSource());
        //    if (base.UseUnityCoroutines)
        //    {
        //        yield return base.GameController.StartCoroutine(coroutine);
        //    }
        //    else
        //    {
        //        base.GameController.ExhaustCoroutine(coroutine);
        //    }

        //    if (DidPlayerAnswerYes(results))
        //    {
        //        SetCardPropertyToTrueIfRealAction(FirstColdDamage);
        //        coroutine = base.GameController.SelectAndPlayCardsFromHand(DecisionMaker, amount, false, 0, isPutIntoPlay: true, cardSource: GetCardSource());
        //        if (base.UseUnityCoroutines)
        //        {
        //            yield return base.GameController.StartCoroutine(coroutine);
        //        }
        //        else
        //        {
        //            base.GameController.ExhaustCoroutine(coroutine);
        //        }
        //    }

        //}

        //public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        //{
        //    if (decision is YesNoCardDecision && ((YesNoCardDecision)decision).GameAction != null && ((YesNoCardDecision)decision).GameAction is DealDamageAction)
        //    {
        //        int amount = ((DealDamageAction)((YesNoCardDecision)decision).GameAction).Amount;
        //        string message = $"put {(amount == 1 ? "a card" : $"up to {amount} cards")} into play";
        //        return new CustomDecisionText(
        //        $"Do you want to {message}?",
        //        $"{decision.DecisionMaker.Name} is deciding whether to {message}.",
        //        $"Vote for whether to {message}.",
        //        $"whether to {message}."
        //        );
        //    }
        //    return null;
        //}
    }
}

