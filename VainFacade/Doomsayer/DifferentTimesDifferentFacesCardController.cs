using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class DifferentTimesDifferentFacesCardController:DoomsayerCardUtilities
	{
		public DifferentTimesDifferentFacesCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowListOfCardsAtLocation(this.TurnTaker.Deck, ProclamationCriteria());
            base.SpecialStringMaker.ShowListOfCardsAtLocation(this.TurnTaker.Deck, RoleCriteria());
		}

        public override IEnumerator Play()
        {
            //Reveal cards from the top of the villain deck until a proclamation and a role are revealed. Put the first revealed card of each type into play. Shuffle the other cards into the villain deck.
            List<RevealCardsAction> results = new List<RevealCardsAction>();
            IEnumerator coroutine = base.GameController.RevealCards(this.TurnTakerController, this.TurnTaker.Deck, (Card c) => IsProclamation(c) || IsRole(c), 1, results, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            if (results.FirstOrDefault() != null && results.FirstOrDefault().NumberOfMatches > 0)
            {
                Card first = results.FirstOrDefault().MatchingCards.FirstOrDefault();
                List<Card> cardsToPlay = new List<Card>();
                cardsToPlay.Add(first);
                List<RevealCardsAction> results2 = new List<RevealCardsAction>();
                coroutine = base.GameController.RevealCards(this.TurnTakerController, this.TurnTaker.Deck, (Card c) => (IsProclamation(first) ? IsRole(c) : IsProclamation(c)), 1, results2, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }

                if (results2.FirstOrDefault() != null && results2.FirstOrDefault().NumberOfMatches > 0)
                {
                    Card second = results2.FirstOrDefault().MatchingCards.FirstOrDefault();
                    cardsToPlay.Add(second);
                }

                coroutine = base.GameController.PlayCards(DecisionMaker, (Card c) => cardsToPlay.Contains(c), false, true, allowAutoDecide: true, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }
            }

            coroutine = CleanupRevealedCards(this.TurnTaker.Revealed, this.TurnTaker.Deck, shuffleAfterwards: true);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            //Each player discards a random card from their hand.
            coroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsPlayer && tt.ToHero().HasCardsInHand), SelectionType.DiscardCard, DiscardRandom, allowAutoDecide: true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            //Play the top card of the villain deck.
            coroutine = PlayTheTopCardOfTheVillainDeckResponse(null);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator DiscardRandom(TurnTaker tt)
        {
            Card randomCard = tt.ToHero().Hand.Cards.Take(1).FirstOrDefault();
            if (randomCard != null)
            {
                IEnumerator coroutine = base.GameController.SendMessageAction($"{tt.Name} discards {randomCard.Title}", Priority.Medium, GetCardSource(), new Card[] { randomCard }, showCardSource: true);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }

                coroutine = base.GameController.DiscardCard(FindHeroTurnTakerController(tt.ToHero()), randomCard, null, tt, cardSource: GetCardSource());
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
    }
}

