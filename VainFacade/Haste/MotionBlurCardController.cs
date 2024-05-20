using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Haste
{
	public class MotionBlurCardController:HasteUtilityCardController
	{
		public MotionBlurCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowListOfCards(new LinqCardCriteria((Card c) => IsPropertyTrue(GeneratePerTargetKey(MotionBlurKey, c)), $"selected for {this.Card.Title} this turn", false, true));
		}

        private string MotionBlurKey = "MotionBlurKey";

        public override void AddTriggers()
        {
            //When you play a card, you may select a target that has not been selected this way this turn. Reduce the next damage dealt by that target by 1.
            AddTrigger<PlayCardAction>((PlayCardAction pca) => pca.ResponsibleTurnTaker == this.TurnTaker && !pca.IsPutIntoPlay, PlayResponse, TriggerType.CreateStatusEffect, TriggerTiming.After);
            AddAfterLeavesPlayAction(() => ResetFlagsAfterLeavesPlay(MotionBlurKey));
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Play a card. You may discard a card to play a card.
            IEnumerator coroutine = base.GameController.SelectAndPlayCardFromHand(DecisionMaker, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            List<DiscardCardAction> results = new List<DiscardCardAction>();
            coroutine = base.GameController.SelectAndDiscardCard(DecisionMaker, true, storedResults: results, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDiscardCards(results))
            {
                coroutine = base.GameController.SelectAndPlayCardFromHand(DecisionMaker, false, cardSource: GetCardSource());
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

        private IEnumerator PlayResponse(PlayCardAction pca)
        {
            List<SelectCardDecision> results = new List<SelectCardDecision>();
            IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.ReduceDamageDealt, new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && !IsPropertyTrue(GeneratePerTargetKey(MotionBlurKey,c))), results, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidSelectCard(results))
            {
                Card selected = GetSelectedCard(results);
                SetCardProperty(GeneratePerTargetKey(MotionBlurKey, selected), true);
                ReduceDamageStatusEffect effect = new ReduceDamageStatusEffect(1);
                effect.SourceCriteria.IsSpecificCard = selected;
                effect.NumberOfUses = 1;
                effect.UntilTargetLeavesPlay(selected);
                coroutine = AddStatusEffect(effect);
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

