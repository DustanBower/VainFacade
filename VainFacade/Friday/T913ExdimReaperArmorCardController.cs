using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Friday
{
	public class T913ExdimReaperArmorCardController:CardController
	{
		public T913ExdimReaperArmorCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstTimeDamageKey);
		}

        private string FirstTimeDamageKey = "FirstTimeDamageKey";

        public override void AddTriggers()
        {
            //Reduce damage dealt to {Friday} by 1.
            AddReduceDamageTrigger((Card c) => c == this.CharacterCard, 1);

            //Once per turn when {Friday} deals damage, she may deal 1 damage of the same type to the same target.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.DidDealDamage && dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && !IsPropertyTrue(FirstTimeDamageKey), DamageResponse, TriggerType.DealDamage, TriggerTiming.After);

            //At the end of your turn, you may discard a card. If you do not, {Friday} deals each other target other than Friday 1 psychic damage each.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[2] { TriggerType.DiscardCard, TriggerType.DealDamage });
        }

        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            if (dd.Target.IsInPlayAndHasGameText && dd.Target.IsTarget)
            {
                List<YesNoCardDecision> storedResults = new List<YesNoCardDecision>();
                IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.DealDamage, this.Card, new DealDamageAction(base.GameController, new DamageSource(base.GameController, this.CharacterCard), dd.Target, 1, dd.DamageType), storedResults, null, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidPlayerAnswerYes(storedResults))
                {
                    SetCardPropertyToTrueIfRealAction(FirstTimeDamageKey);
                    coroutine = DealDamage(this.CharacterCard, dd.Target, 1, dd.DamageType, false, false, cardSource: GetCardSource());
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

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator coroutine = base.GameController.SelectAndDiscardCard(DecisionMaker, true, null, results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (!DidDiscardCards(results))
            {
                coroutine = DealDamage(this.CharacterCard, (Card c) => c != this.CharacterCard, 1, DamageType.Psychic);
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

