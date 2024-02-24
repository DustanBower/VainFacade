using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class LookingForALoopholeCardController:PatternCardController
	{
		public LookingForALoopholeCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowTokenPool(pool());
        }

        private TokenPool pool()
        {
            return this.Card.FindTokenPool("LookingForALoopholePool");
        }

        public override void AddTriggers()
        {
            //At the end of each hero turn, that hero’s player discards 1 card or destroys one of their ongoing or equipment cards, then may play a card.
            //Then they may discard a card to put a token on this card. Then, if there are {H} or more tokens on this card, put this card under {CountlessWords}.
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame, EndOfTurnResponse, new TriggerType[5] { TriggerType.DiscardCard, TriggerType.DestroyCard, TriggerType.PlayCard , TriggerType.AddTokensToPool, TriggerType.MoveCard});

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
            HeroTurnTakerController httc = FindHeroTurnTakerController(pca.ToPhase.TurnTaker.ToHero());
            IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(httc, "Discard a card", SelectionType.DiscardCard, () => base.GameController.SelectAndDiscardCard(httc, responsibleTurnTaker: httc.TurnTaker,cardSource:GetCardSource()), httc.HeroTurnTaker.HasCardsInHand, $"{httc.Name} has no ongoing or equipment cards in play"),
                new Function(httc, "Destroy one of your ongoing or equipment cards", SelectionType.DestroyCard, () => base.GameController.SelectAndDestroyCard(httc, new LinqCardCriteria((Card c) => c.Owner == httc.TurnTaker && (IsOngoing(c) || IsEquipment(c)),"ongoing or equipment"),false, cardSource:GetCardSource()), FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.Owner == httc.TurnTaker && (IsOngoing(c) || IsEquipment(c))).Count() > 0,$"{httc.Name} has no cards in hand")
                };
            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, false, null, $"{httc.Name} has no cards in hand and no ongoing or equipment cards in play", null, GetCardSource());
            IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(choose);
            }
            else
            {
                base.GameController.ExhaustCoroutine(choose);
            }

            IEnumerator coroutine = base.GameController.SelectAndPlayCardFromHand(httc, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            List<DiscardCardAction> results = new List<DiscardCardAction>();
            coroutine = base.GameController.SelectAndDiscardCard(httc, true, storedResults: results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDiscardCards(results))
            {
                coroutine = base.GameController.AddTokensToPool(pool(), 1, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }

            if (pool().CurrentValue >= base.H)
            {
                coroutine = base.GameController.MoveCard(this.TurnTakerController, this.Card, countlessWords.UnderLocation, cardSource: GetCardSource());
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
}

