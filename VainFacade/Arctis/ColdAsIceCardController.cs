using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class ColdAsIceCardController:ArctisCardUtilities
	{
		public ColdAsIceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstHPGain);
		}

        private string FirstHPGain = "FirstHPGain";

        public override void AddTriggers()
        {
            //When {Arctis} is dealt cold damage, he regains 1 hp.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DamageType == DamageType.Cold && dd.DidDealDamage, HealResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.DrawCard, TriggerType.PutIntoPlay }, TriggerTiming.After);
            ResetFlagAfterLeavesPlay(FirstHPGain);
        }

        public override IEnumerator Play()
        {
            //When this card enters play, {Arctis} deals himself 1 cold damage.
            IEnumerator coroutine = DealDamage(this.CharacterCard, this.CharacterCard, 1, DamageType.Cold, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator HealResponse(DealDamageAction dd)
        {
            List<GainHPAction> healResults = new List<GainHPAction>();
            IEnumerator coroutine = base.GameController.GainHP(this.CharacterCard, 1, storedResults: healResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (!IsPropertyTrue(FirstHPGain) && healResults.Any((GainHPAction hp) => hp.AmountActuallyGained > 0 && hp.HpGainer == this.CharacterCard))
            {
                //The first time he regains hp this way each turn, you may draw a card or put a card from your hand into play. If you do neither, {Arctis} regains 1 hp.
                SetCardProperty(FirstHPGain, true);
                IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(base.HeroTurnTakerController, "Draw a card", SelectionType.DrawCard, () => DrawCard(this.HeroTurnTaker, true), null, $"{this.TurnTaker.Name} has no cards in hand"),
                new Function(base.HeroTurnTakerController, "Put a card from your hand into play", SelectionType.PutIntoPlay, () => SelectAndPlayCardFromHand(DecisionMaker, isPutIntoPlay: true), this.HeroTurnTaker.Hand.Cards.Any())
                };

                SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, true, null, null, null, GetCardSource());
                List<SelectFunctionDecision> results = new List<SelectFunctionDecision>();
                IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction,results);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(choose);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(choose);
                }

                if (!DidSelectFunction(results, DecisionMaker))
                {
                    coroutine = base.GameController.GainHP(this.CharacterCard, 1, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }
            }
        }
    }
}

