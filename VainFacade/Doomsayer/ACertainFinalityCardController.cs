using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class ACertainFinalityCardController:DoomsayerCardUtilities
	{
		public ACertainFinalityCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            //base.GameController.AddCardControllerToList(CardControllerListType.CancelsPreventing,this);
		}

        //public override bool AskIfCardMayPreventAction<T>(TurnTakerController ttc, CardController preventer)
        //{
        //    if (typeof(T) == typeof(DealDamageAction) && !IsVillain(preventer.Card))
        //    {
        //        return false;
        //    }
        //    return base.AskIfCardMayPreventAction<T>(ttc, preventer);
        //}

        public override IEnumerator Play()
        {
            //When this card enters play, discard cards from the top of the villain deck until a target or proclamation is discarded. Put it into play.
            List<MoveCardAction> results = new List<MoveCardAction>();
            IEnumerator coroutine = RevealAndDiscard(results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            while (!results.Any((MoveCardAction mc) => mc.CardToMove.IsTarget || IsProclamation(mc.CardToMove)) && DidMoveCard(results))
            {
                results = new List<MoveCardAction>();
                IEnumerator discard = RevealAndDiscard(results);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discard);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discard);
                }
            }

            if (results.Any((MoveCardAction mc) => mc.CardToMove.IsTarget || IsProclamation(mc.CardToMove)))
            {
                Card card = results.Where((MoveCardAction mc) => mc.CardToMove.IsTarget || IsProclamation(mc.CardToMove)).FirstOrDefault().CardToMove;
                coroutine = base.GameController.PlayCard(this.TurnTakerController, card, true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
            else
            {
                coroutine = base.GameController.SendMessageAction("There were no targets or proclamations in the villain deck.", Priority.Medium, GetCardSource());
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

        private IEnumerator RevealAndDiscard(List<MoveCardAction> storedResults = null)
        {
            List<Card> results = new List<Card>();
            IEnumerator coroutine = base.GameController.RevealCards(this.TurnTakerController, this.TurnTaker.Deck, 1, results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (results.FirstOrDefault() != null && results.FirstOrDefault().Location.IsRevealed)
            {
                Card revealed = results.FirstOrDefault();
                coroutine = base.GameController.MoveCard(this.TurnTakerController, revealed, FindCardController(revealed).GetTrashDestination().Location, storedResults: storedResults, responsibleTurnTaker: this.TurnTaker, isDiscard: true, cardSource: GetCardSource());
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

        public override void AddTriggers()
        {
            //When damage would be dealt to a hero target, that damage cannot be reduced, redirected, or prevented by non-villain cards.
            AddTrigger<ReduceDamageAction>((ReduceDamageAction rd) => IsHeroTarget(rd.DealDamageAction.Target) && rd.CardSource != null && !IsVillain(rd.CardSource.Card), base.CancelResponse, TriggerType.CancelAction, TriggerTiming.Before, priority: TriggerPriority.High);
            AddTrigger<RedirectDamageAction>((RedirectDamageAction rd) => IsHeroTarget(rd.OldTarget) && rd.CardSource != null && !IsVillain(rd.CardSource.Card), (RedirectDamageAction rd) => base.GameController.CancelAction(rd,cardSource:GetCardSource()), TriggerType.CancelAction, TriggerTiming.Before, priority: TriggerPriority.High);
            AddTrigger<CancelAction>((CancelAction ca) => ca.IsPreventEffect && ca.ActionToCancel is DealDamageAction && IsHeroTarget(((DealDamageAction)ca.ActionToCancel).Target) && ca.CardSource != null && !IsVillain(ca.CardSource.Card), base.CancelResponse, TriggerType.CancelAction, TriggerTiming.Before, priority: TriggerPriority.High);
        }

        //public override bool AskIfCardMayPreventAction<T>(TurnTakerController ttc, CardController preventer)
        //{
        //    if (typeof(T) == typeof(DealDamageAction) && IsHero(ttc.TurnTaker) && !IsVillain(preventer.Card))
        //    {
        //        return false;
        //    }
        //    return base.AskIfCardMayPreventAction<T>(ttc, preventer);
        //}
    }
}

