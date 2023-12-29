using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class GreenVeinsCardController:SymptomCardController
	{
		public GreenVeinsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(RedirectKey, $"Damage has been redirected from {this.CharacterCard.Title} this turn.", $"Damage has not been redirected from {this.CharacterCard.Title} this turn.");
            //base.SpecialStringMaker.ShowHasBeenUsedThisTurn(DamageKey,$"{this.CharacterCard.Title} has been dealt damage by a non-hero target this turn.",$"{this.CharacterCard.Title} has not been dealt damage by a non-hero target this turn.");
            base.SpecialStringMaker.ShowSpecialString(() => base.TurnTaker.Name + " does not have at least 2 cards in hand.").Condition = () => base.HeroTurnTaker.Hand.Cards.Count() < 2;
        }

        private string RedirectKey = "RedirectKey";
        //private string DamageKey = "DamageKey";

        public override void AddTriggers()
        {
            //The first time each turn damage is redirected from {Peacekeeper} to another target, draw a card.
            AddTrigger<RedirectDamageAction>((RedirectDamageAction rd) => !IsPropertyTrue(RedirectKey) && rd.OldTarget == this.CharacterCard && rd.NewTarget != this.CharacterCard, DrawResponse, TriggerType.DrawCard, TriggerTiming.After);

            //The first time each turn {Peacekeeper} would be dealt damage by a non-hero target, you may discard 2 cards to redirect that damage to another target.
            //AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(DamageKey) && dd.Target == this.CharacterCard && dd.DamageSource.IsTarget && !IsHeroTarget(dd.DamageSource.Card), SetTrue, TriggerType.Hidden, TriggerTiming.After);
            //AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(DamageKey) && dd.Target == this.CharacterCard && dd.DamageSource.IsTarget && !IsHeroTarget(dd.DamageSource.Card) && this.HeroTurnTaker.HasCardsInHand, RedirectResponse, new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.RedirectDamage }, TriggerTiming.Before);
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DamageSource.IsTarget && !IsHeroTarget(dd.DamageSource.Card) && this.HeroTurnTaker.HasCardsInHand, RedirectResponse, new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.RedirectDamage }, TriggerTiming.Before);

            //AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(DamageKey), TriggerType.Hidden);

            base.AddTriggers();
        }

        private IEnumerator DrawResponse(RedirectDamageAction rd)
        {
            SetCardProperty(RedirectKey, true);
            IEnumerator coroutine = DrawCard();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator RedirectResponse(DealDamageAction dd)
        {
            //Copied from Amulet of the Elder Gods
            //SetCardProperty(DamageKey,true);

            if (!base.HeroTurnTaker.Hand.HasCards)
            {
                yield break;
            }
            if (dd.IsRedirectable || base.HeroTurnTaker.NumberOfCardsInHand < 2)
            {
                List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
                IEnumerator coroutine = SelectAndDiscardCards(base.HeroTurnTakerController, 2, optional: true, null, storedResults, allowAutoDecide: false, dd);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                if (DidDiscardCards(storedResults, 2))
                {
                    coroutine = base.GameController.SelectTargetAndRedirectDamage(base.HeroTurnTakerController, null, dd, optional: false, null, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }
                yield break;
            }
            SelectionType selectionType = ((!dd.IsRedirectable) ? SelectionType.DiscardCardsNoRedirect : SelectionType.DiscardCardsToNoEffect);
            YesNoAmountDecision yesNo = new YesNoAmountDecision(base.GameController, DecisionMaker, selectionType, 2, upTo: false, requireUnanimous: false, null, null, GetCardSource());
            IEnumerator coroutine2 = base.GameController.MakeDecisionAction(yesNo);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine2);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine2);
            }
            if (DidPlayerAnswerYes(yesNo))
            {
                coroutine2 = SelectAndDiscardCards(base.HeroTurnTakerController, 2, optional: false, null, null, allowAutoDecide: false, null, null, null, selectionType);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine2);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine2);
                }
            }
        }

        //private IEnumerator SetTrue(DealDamageAction dd)
        //{
        //    SetCardProperty(DamageKey, true);
        //    yield return null;
        //}
    }
}

