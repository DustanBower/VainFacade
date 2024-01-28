using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class SigiledBladeCardController:GlyphCardUtilities
	{
		public SigiledBladeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //Increase melee damage dealt by Glyph by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsSameCard(this.CharacterCard) && dd.DamageType == DamageType.Melee, 1);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //{Glyph} deals 1 target 0 or 1 melee damage.
            int num1 = GetPowerNumeral(0, 1);
            int num2 = GetPowerNumeral(1, 0);
            int num3 = GetPowerNumeral(2, 1);

            List<SelectNumberDecision> numResults = new List<SelectNumberDecision>();
            IEnumerator coroutine = base.GameController.SelectNumber(DecisionMaker, SelectionType.DealDamage, num2, num3, storedResults: numResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidSelectNumber(numResults))
            {
                int damage = GetSelectedNumber(numResults).Value;
                List<DealDamageAction> results = new List<DealDamageAction>();
                coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), damage, DamageType.Melee, num1, false, num1, storedResultsDamage: results, cardSource:GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidDealDamage(results,this.CharacterCard))
                {
                    //If {Glyph} is dealt damage this way, you may put one of your cards into play face-down in any play area for each point of damage dealt this way,
                    int amount = results.Where((DealDamageAction dd) => dd.DidDealDamage).Select((DealDamageAction dd) => dd.Amount).Sum();

                    coroutine = base.GameController.SelectCardsAndDoAction(DecisionMaker, new LinqCardCriteria((Card c) => this.HeroTurnTaker.Hand.Cards.Contains(c)), SelectionType.Custom, PlayFaceDown, amount, false, 0, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    //then you may play a ritual.
                    coroutine = SelectAndPlayCardFromHand(DecisionMaker, cardCriteria: new LinqCardCriteria(IsRitual, "", false, false, "ritual", "rituals"));
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

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            string text = "a card to put into play face-down";
            CustomDecisionText result = new CustomDecisionText(
            $"Select {text}.",
            $"The other heroes are choosing {text}.",
            $"Vote for {text}.",
            $"{text}."
            );

            return result;
        }

        private IEnumerator PlayFaceDown(Card selected)
        {
            List<SelectTurnTakerDecision> locationResults = new List<SelectTurnTakerDecision>();
            IEnumerator coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.MoveCardToPlayArea, locationResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            Location loc = GetSelectedTurnTaker(locationResults).PlayArea;
            if (loc != null)
            {
                coroutine = base.GameController.MoveCard(DecisionMaker, selected, loc, isPutIntoPlay: true, playCardIfMovingToPlayArea: false, flipFaceDown: true, cardSource: GetCardSource());
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

