using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra;

namespace VainFacadePlaytest.Push
{
	public class NewtonsCradleCardController:PushCardControllerUtilities
	{
		public NewtonsCradleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamage);
            base.SpecialStringMaker.ShowListOfCards(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsTarget && !HasBeenDealtDamageThisTurn(c), "not dealt damage this turn", false, true));
		}

        private string FirstDamage = "FirstDamage";

        public override void AddTriggers()
        {
            //The first time Push deals damage each turn, Push deals that same damage to 2 targets that have not been dealt damage this turn.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(FirstDamage) && dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && dd.DidDealDamage, DamageResponse, TriggerType.DealDamage, TriggerTiming.After);

            //At the start of your turn, destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }

        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            SetCardProperty(FirstDamage,true);
            int amount = dd.Amount;
            DamageType type = dd.DamageType;
            //IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), amount, type, 2, false, 2, addStatusEffect: (dd.StatusEffectResponses != null) ? dd.StatusEffectResponses.FirstOrDefault() : null, isIrreducible: dd.IsIrreducible, additionalCriteria: (Card c) => !HasBeenDealtDamageThisTurn(c), cardSource: GetCardSource());
            IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), amount, type, 2, false, 2, isIrreducible: dd.IsIrreducible, additionalCriteria: (Card c) => !HasBeenDealtDamageThisTurn(c), cardSource: GetCardSource());
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

