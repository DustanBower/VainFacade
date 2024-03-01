using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class DarkestBeforeTheDawnCardController:DoomsayerCardUtilities
	{
		public DarkestBeforeTheDawnCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageKey);
		}

        private string FirstDamageKey = "FirstDamageKey";

        public override void AddTriggers()
        {
            //The first time each turn any hero target would be dealt damage, increase that damage by 3.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(FirstDamageKey) && IsHeroTarget(dd.Target) && !HasDamageOccurredThisTurn(null, null, dd, (Card c) => IsHeroTarget(c)), IncreaseResponse, TriggerType.IncreaseDamage, TriggerTiming.Before);

            //When {Doomsayer} regains 13 or more hp at once, destroy an ongoing.
            AddTrigger<GainHPAction>((GainHPAction hp) => hp.HpGainer == this.CharacterCard && hp.AmountActuallyGained >= 13, (GainHPAction hp) => base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsOngoing(c), "ongoing"), true, cardSource: GetCardSource()), TriggerType.DestroyCard, TriggerTiming.After);

            //At the end of each turn, if {Doomsayer} has -13 or fewer hp, he regains 13hp.
            AddEndOfTurnTrigger((TurnTaker tt) => true, (PhaseChangeAction pca) => base.GameController.GainHP(this.CharacterCard, 13, cardSource: GetCardSource()), TriggerType.GainHP, (PhaseChangeAction pca) => this.CharacterCard.HitPoints <= -13);
        }

        private IEnumerator IncreaseResponse(DealDamageAction dd)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageKey);
            IEnumerator coroutine = base.GameController.IncreaseDamage(dd, 3, false, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override IEnumerable<Card> FilterDecisionCardChoices(SelectCardDecision decision)
        {
            if (decision.SelectionType == SelectionType.DestroyCard && decision.Choices.Where((Card c) => !IsHero(c)).Count() > 0)
            {
                return decision.Choices.Where((Card c) => IsHero(c));
            }
            return null;
        }
    }
}

