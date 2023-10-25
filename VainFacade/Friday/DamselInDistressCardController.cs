using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Friday
{
	public class DamselInDistressCardController:CardController
	{
		public DamselInDistressCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHeroTargetWithLowestHP();
            base.AddThisCardControllerToList(CardControllerListType.CanCauseDamageOutOfPlay);
		}

        public override void AddTriggers()
        {
            //When the hero target with the lowest HP would be dealt damage by a target other than {Friday}, redirect that damage to {Friday}.
            AddTrigger((DealDamageAction dd) => dd.Target != base.CharacterCard && dd.DamageSource.IsTarget && dd.DamageSource.Card != this.CharacterCard && CanCardBeConsideredLowestHitPoints(dd.Target, (Card c) => IsHeroTarget(c)), RedirectIfLowestResponse, new TriggerType[2]
            {
            TriggerType.WouldBeDealtDamage,
            TriggerType.RedirectDamage
            }, TriggerTiming.Before);
            base.AddTriggers();

            //When {Friday} is dealt damage by a target other than herself, you may destroy this card. If you do, {Friday} deals the source of that damage 4 melee damage.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.DamageSource.IsTarget && dd.DamageSource.Card != this.CharacterCard && dd.Target == this.CharacterCard && dd.DidDealDamage && !IsBeingDestroyed && dd.DidDealDamage, DestroyResponse, new TriggerType[2] { TriggerType.DestroySelf, TriggerType.DealDamage }, TriggerTiming.After);
        }

        private IEnumerator RedirectIfLowestResponse(DealDamageAction dd)
        {
            //Copied from Intervening Path Calculator
            List<bool> storedResults = new List<bool>();
            IEnumerator coroutine = DetermineIfGivenCardIsTargetWithLowestOrHighestHitPoints(dd.Target, highest: false, (Card c) => IsHeroTarget(c), dd, storedResults, askIfTied: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            if (storedResults.FirstOrDefault())
            {
                coroutine = base.GameController.RedirectDamage(dd, base.CharacterCard, isOptional: false, GetCardSource());
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

        private IEnumerator DestroyResponse(DealDamageAction dd)
        {
            IEnumerator coroutine = base.GameController.DestroyCard(DecisionMaker, this.Card, true, postDestroyAction: () => DestroyAction(dd), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator DestroyAction(DealDamageAction dd)
        {
            if (dd.DamageSource.IsTarget && dd.DamageSource.IsInPlayAndHasGameText)
            {
                Card target = dd.DamageSource.Card;
                //AddInhibitorException((GameAction ga) => true);
                IEnumerator coroutine = DealDamage(this.CharacterCard, target, 4, DamageType.Melee, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                //RemoveInhibitorException();
            }
        }
    }
}

