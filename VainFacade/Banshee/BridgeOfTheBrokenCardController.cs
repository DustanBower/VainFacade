using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class BridgeOfTheBrokenCardController:DirgeCardController
	{
		public BridgeOfTheBrokenCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageKey);
		}

        private string FirstDamageKey = "FirstDamageKey";

        public override void AddTriggers()
        {
            base.AddTriggers();

            //Damage dealt to that target is irreducible
            AddMakeDamageIrreducibleTrigger((DealDamageAction dd) => GetCardThisCardIsNextTo() != null && dd.Target == GetCardThisCardIsNextTo());

            //The first time each turn that target deals damage, Banshee may use a power.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(FirstDamageKey) && dd.DamageSource.IsTarget && GetCardThisCardIsNextTo() != null && dd.DamageSource.Card == GetCardThisCardIsNextTo() && dd.DidDealDamage, DamageResponse, TriggerType.UsePower, TriggerTiming.After);
            ResetFlagAfterLeavesPlay(FirstDamageKey);
        }

        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageKey);
            IEnumerator coroutine = base.GameController.SelectAndUsePower(DecisionMaker, cardSource: GetCardSource());
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

