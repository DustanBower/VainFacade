using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class ChorusOfChaosCardController:DirgeCardController
	{
		public ChorusOfChaosCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
			base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageKey);
		}

		private string FirstDamageKey = "FirstDamageKey";

		public override void AddTriggers()
		{
			base.AddTriggers();

			//The first time each turn any non-hero card deals damage to another target, that source deals that same damage to the target next to this card.
			AddTrigger<DealDamageAction>((DealDamageAction dd) => GetCardThisCardIsNextTo() != null && !IsPropertyTrue(FirstDamageKey) && dd.DamageSource.IsCard && !IsHero(dd.DamageSource.Card) && dd.Target != GetCardThisCardIsNextTo() && dd.DidDealDamage, DamageResponse, TriggerType.DealDamage, TriggerTiming.After);
			ResetFlagAfterLeavesPlay(FirstDamageKey);
		}

		private IEnumerator DamageResponse(DealDamageAction dd)
		{
			SetCardPropertyToTrueIfRealAction(FirstDamageKey);
			Card source = dd.DamageSource.Card;
			int amount = dd.Amount;
			bool irr = dd.IsIrreducible;
			DamageType type = dd.DamageType;
			IEnumerator coroutine = DealDamage(source, GetCardThisCardIsNextTo(), amount, type, irr, false, cardSource: GetCardSource());
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

