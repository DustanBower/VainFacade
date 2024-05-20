using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class FutileEffortsCardController:PatternCardController
	{
		public FutileEffortsCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowTokenPool(pool());
		}

        private TokenPool pool()
        {
            return this.Card.FindTokenPool("FutileEffortsPool");
        }

        public override void AddTriggers()
        {
            //Reduce damage dealt by hero targets by 1.
            AddReduceDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsHero,(DealDamageAction dd) => 1);

            //At the end of the villain turn, each player may discard any number of cards. Add a token to this card for each card discarded this way. Then, if there are {H * 3} or more tokens on this card, put this card under {CountlessWords}.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[3] { TriggerType.DiscardCard, TriggerType.AddTokensToPool, TriggerType.MoveCard });

            //Reset token pool when this card is destroyed or moved out of play. Based on Magma's Rage
            AddWhenDestroyedTrigger((DestroyCardAction dc) => ResetTokenValue(), TriggerType.Hidden);
            AddTrigger<MoveCardAction>((MoveCardAction mc) => mc.Origin.IsInPlayAndNotUnderCard && !mc.Destination.IsInPlayAndNotUnderCard && mc.CardToMove == base.Card, (MoveCardAction mc) => ResetTokenValue(), TriggerType.ModifyTokens, TriggerTiming.After, ActionDescription.Unspecified, outOfPlayTrigger: true);
        }

        private IEnumerator ResetTokenValue()
        {
            pool().SetToInitialValue();
            yield return null;
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            IEnumerator coroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsPlayer && tt.ToHero().HasCardsInHand), SelectionType.DiscardCard, DiscardAnyNumber, requiredDecisions: 0, allowAutoDecide: true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            if (pool().CurrentValue >= base.H * 3)
            {
                coroutine = base.GameController.MoveCard(this.TurnTakerController, this.Card, countlessWords.UnderLocation, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }
            }
        }

        private IEnumerator DiscardAnyNumber(TurnTaker tt)
        {
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator coroutine = base.GameController.SelectAndDiscardCard(FindHeroTurnTakerController(tt.ToHero()), true, storedResults: results, responsibleTurnTaker: tt, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            bool loop = DidDiscardCards(results);

            while (loop)
            {
                coroutine = base.GameController.AddTokensToPool(pool(), 1, GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }

                List<DiscardCardAction> results2 = new List<DiscardCardAction>();
                coroutine = base.GameController.SelectAndDiscardCard(FindHeroTurnTakerController(tt.ToHero()), true, storedResults: results2, responsibleTurnTaker: tt, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }

                loop = DidDiscardCards(results2);
            }
        }
    }
}

