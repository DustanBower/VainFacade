using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class SphereOfNalatukhaCardController:CardController
	{
		public SphereOfNalatukhaCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageKey);
		}

        private string FirstDamageKey = "FirstDamageKey";

        public override void AddTriggers()
        {
            //The first time each turn {Glyph} is dealt damage, you may draw or play a card.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(FirstDamageKey) && !HasDamageOccurredThisTurn(this.CharacterCard,(Card c) => true, dd) && dd.Target == this.CharacterCard && dd.DidDealDamage, DamageResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.PlayCard }, TriggerTiming.After);
            //AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(FirstDamageKey) && dd.Target == this.CharacterCard && dd.DidDealDamage, DamageResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.PlayCard }, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction action) => ResetFlagAfterLeavesPlay(FirstDamageKey), TriggerType.Hidden);
        }

        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageKey);
            IEnumerator coroutine = DrawACardOrPlayACard(DecisionMaker, true);
            if (UseUnityCoroutines)
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

