using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.BastionCity
{
	public class CleanStreetsDirtyHandsCardController:BastionCityCardController
	{
		public CleanStreetsDirtyHandsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsTarget && DoesCardShareKeywordWithMachination(c), "targets sharing a keyword with a machination", false, false));
            base.SpecialStringMaker.ShowListOfCardsAtLocation(this.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.IsTarget, "target", false, false));
		}

        public override void AddTriggers()
        {
            //Increase damage dealt by targets sharing a keyword with a machination by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsTarget && DoesCardShareKeywordWithMachination(dd.DamageSource.Card), 1);

            //At the end of the environment turn, discard the top card of the environment deck. If a target is discarded this way, put it into play.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PutIntoPlay });
        }

        private bool DoesCardShareKeywordWithMachination(Card card)
        {
            List<string> keywords = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsMachination(c)).SelectMany((Card c) => base.GameController.GetAllKeywords(c)).Distinct().ToList();
            List<string> cardKeywords = base.GameController.GetAllKeywords(card).ToList();
            return cardKeywords.Any((string s) => keywords.Contains(s));
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<MoveCardAction> results = new List<MoveCardAction>();
            IEnumerator coroutine = DiscardCardsFromTopOfDeck(this.TurnTakerController, 1, false, results, responsibleTurnTaker: this.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidMoveCard(results))
            {
                Card card = results.FirstOrDefault().CardToMove;
                if (card.Location == this.TurnTaker.Trash && card.IsTarget)
                {
                    coroutine = base.GameController.SendMessageAction($"{this.Card.Title} puts {card.Title} into play", Priority.Low, GetCardSource(), new Card[] { card }, true);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    coroutine = base.GameController.PlayCard(this.TurnTakerController, card, true, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
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
}

