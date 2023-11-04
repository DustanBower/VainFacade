using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class CollateralBonusCardControllerOld:PushCardControllerUtilities
	{
		public CollateralBonusCardControllerOld(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
            base.SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"));
		}

        public Guid? PerformDestroyForDamage { get; set; }

        public Card CardToDiscard { get; set; }

        public Card CardToDestroy { get; set; }

        private Card Target;

        public override void AddTriggers()
        {
            //If an environment card is selected to be destroyed and that destruction is prevented, you still get the damage increase, for the same reason that Planquez Vous still prevents the damage if it fails to be destroyed.

            //When Push would deal damage, destroy an environment card. If a card is destroyed this way, discard a card and incrase that damage by 2.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard, DamageResponse, new TriggerType[3] {TriggerType.FirstTrigger, TriggerType.DestroyCard, TriggerType.DiscardCard}, TriggerTiming.Before);

            //At the start of your turn, destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }

        //The code for this is mostly copied from Small-Time Thief, with the discard part coming from The Shadow Cloak
        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            //if (!PerformDestroyForDamage.HasValue || PerformDestroyForDamage.Value != dd.InstanceIdentifier)
            if ((base.GameController.PretendMode || dd.Target != Target))
            {
                List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
                IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.DestroyCard, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsEnvironment && !base.GameController.IsCardIndestructible(c), "environment"), storedResults, false, gameAction: null, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                if (DidSelectCard(storedResults))
                {
                    PerformDestroyForDamage = dd.InstanceIdentifier;
                    CardToDestroy = GetSelectedCard(storedResults);

                    List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
                    coroutine = base.GameController.SelectAndDiscardCard(DecisionMaker, optional: false, null, discardResults, SelectionType.DiscardCard, null, null, ignoreBattleZone: false, null, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                    if (DidDiscardCards(discardResults))
                    {
                        if (discardResults.Any((DiscardCardAction dc) => dc.IsPretend))
                        {
                            CardToDiscard = discardResults.First().CardToDiscard;
                        }
                    }
                }
                Target = dd.Target;
            }
            if (CardToDestroy != null)
            {
                IEnumerator coroutine3 = base.GameController.IncreaseDamage(dd, 2, cardSource:GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine3);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine3);
                }
                if (IsRealAction(dd))
                {
                    GameController gameController = base.GameController;
                    HeroTurnTakerController decisionMaker = DecisionMaker;
                    Card card = CardToDestroy;
                    CardSource cardSource = GetCardSource();
                    coroutine3 = gameController.DestroyCard(decisionMaker, card, optional: false, null, null, null, null, null, null, null, null, cardSource);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine3);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine3);
                    }
                }
            }
            if (CardToDiscard != null)
            {
                IEnumerator coroutine2 = base.GameController.DiscardCard(DecisionMaker, CardToDiscard, null, base.TurnTaker, null, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine2);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine2);
                }
            }
            
            if (!base.GameController.PretendMode)
            {
                PerformDestroyForDamage = null;
                CardToDestroy = null;
                CardToDiscard = null;
            }
        }
    }
}

