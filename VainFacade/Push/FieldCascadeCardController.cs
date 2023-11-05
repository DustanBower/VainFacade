using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class FieldCascadeCardController:PushCardControllerUtilities
	{
		public FieldCascadeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
            base.SpecialStringMaker.ShowSpecialString(() => DamagePrevented() + " damage has been prevented by " + this.TurnTaker.Name + " this turn");
		}

        public override void AddTriggers()
        {
            //When Push would deal damage to a target, prevent that damage and destroy this card.
            //AddTrigger<DealDamageAction>((DealDamageAction dd) => !this.Card.IsBeingDestroyed && dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && dd.Amount > 0, PreventResponse, new TriggerType[3] { TriggerType.WouldBeDealtDamage, TriggerType.CancelAction, TriggerType.DestroySelf }, TriggerTiming.Before);
            AddPreventDamageTrigger((DealDamageAction dd) => !this.Card.IsBeingDestroyed && dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard, DestroyThisCardResponse, new TriggerType[1] { TriggerType.DestroySelf }, true);
            //When this card is destroyed, for each target other than Push, discard 2 cards or Push deals that target X projectile damage,
            //where X = the damage Push has prevented this turn.
            AddWhenDestroyedTrigger(DestroyedResponse, new TriggerType[2] { TriggerType.DiscardCard, TriggerType.DealDamage });

            //At the start of your turn, destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }

        //private IEnumerator PreventResponse(DealDamageAction dd)
        //{
        //    IEnumerator coroutine = base.GameController.CancelAction(dd, isPreventEffect: true, cardSource:GetCardSource());
        //    if (base.UseUnityCoroutines)
        //    {
        //        yield return base.GameController.StartCoroutine(coroutine);
        //    }
        //    else
        //    {
        //        base.GameController.ExhaustCoroutine(coroutine);
        //    }

        //    if (!dd.IsPretend)
        //    {
        //        coroutine = DestroyThisCardResponse(null);
        //        if (base.UseUnityCoroutines)
        //        {
        //            yield return base.GameController.StartCoroutine(coroutine);
        //        }
        //        else
        //        {
        //            base.GameController.ExhaustCoroutine(coroutine);
        //        }
        //    }
        //}

        private IEnumerator DestroyedResponse(DestroyCardAction dc)
        {
            SelectCardsDecision decision = new SelectCardsDecision(base.GameController, DecisionMaker, (Card c) => c.IsInPlayAndHasGameText && c.IsTarget && c != this.CharacterCard, SelectionType.DealDamage, null, false, null, true, true, gameAction: new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, this.CharacterCard), null, DamagePrevented(), DamageType.Projectile),  cardSource: GetCardSource());
            IEnumerator coroutine = base.GameController.SelectCardsAndDoAction(decision, (SelectCardDecision scd) => DiscardOrDamage(scd.SelectedCard), cardSource:GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator DiscardOrDamage(Card card)
        {
            IEnumerable<Function> functionChoices = new Function[2]
                {
            new Function(base.HeroTurnTakerController, "Discard 2 cards", SelectionType.DiscardCard, () => SelectAndDiscardCards(DecisionMaker, 2, false, 2), this.HeroTurnTaker.Hand.Cards.Count() > 1),
            new Function(base.HeroTurnTakerController, "Deal " + card.Title + " " + DamagePrevented() + " projectile damage", SelectionType.DealDamage, () => DealDamage(this.CharacterCard, card, DamagePrevented(), DamageType.Projectile, cardSource:GetCardSource()), forcedActionMessage: this.TurnTaker.Name + " does not have enough cards to discard!")
                };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, false, new DealDamageAction(GetCardSource(), new DamageSource(base.GameController,this.CharacterCard), card, DamagePrevented(), DamageType.Projectile), null, new Card[] {card}, GetCardSource());
            IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(choose);
            }
            else
            {
                base.GameController.ExhaustCoroutine(choose);
            }
        }

        private int DamagePrevented()
        {
            return base.CharacterCardController.GetCardPropertyJournalEntryInteger("FieldCascadePreventionCount").GetValueOrDefault(0);
        }
    }
}

