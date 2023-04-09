using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class X51QuantumFieldStabilizerCardController : SphereUtilityCardController
    {
        public X51QuantumFieldStabilizerCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Emanations in Sphere's deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, isEmanation);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Reveal cards from the top of your deck until an Emanation is revealed. Put it into your hand or into play. Discard the other revealed cards."
            yield return RevealMoveDiscardDontShuffle(base.HeroTurnTakerController, base.TurnTakerController, base.TurnTaker.Deck, (Card c) => c.DoKeywordsContain(emanationKeyword), 1, 1, true, true, true, emanationKeyword, base.TurnTaker);
        }

        private IEnumerator RevealMoveDiscardDontShuffle(HeroTurnTakerController hero, TurnTakerController revealingTurnTaker, Location locationToRevealFrom, Func<Card, bool> cardCriteria, int numberOfMatchesToReveal, int numberOfRevealedCardsToChoose, bool canPutInHand, bool canPlayCard, bool isPutIntoPlay, string cardCriteriaDescription, TurnTaker responsibleTurnTaker = null)
        {
			List<RevealCardsAction> revealedCards = new List<RevealCardsAction>();
			IEnumerator coroutine = GameController.RevealCards(revealingTurnTaker, locationToRevealFrom, cardCriteria, numberOfMatchesToReveal, revealedCards, RevealedCardDisplay.None, GetCardSource());
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(coroutine);
			}
			else
			{
				GameController.ExhaustCoroutine(coroutine);
			}
			RevealCardsAction revealed = revealedCards.FirstOrDefault();
			if (revealed != null && revealed.FoundMatchingCards)
			{
				Card selectedCard = null;
				List<MoveCardDestination> destinations = new List<MoveCardDestination>();
				if (canPlayCard)
				{
					destinations.Add(new MoveCardDestination(TurnTaker.PlayArea));
				}
				if (canPutInHand)
				{
					destinations.Add(new MoveCardDestination(HeroTurnTaker.Hand));
				}
				int num = revealed.MatchingCards.Count();
				if (num > 1)
				{
					if (num < numberOfRevealedCardsToChoose)
					{
						coroutine = GameController.SendMessageAction("Only found " + num + " " + cardCriteriaDescription + ".", Priority.High, GetCardSource());
						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(coroutine);
						}
						else
						{
							GameController.ExhaustCoroutine(coroutine);
						}
					}
					SelectionType type = SelectionType.MoveCard;
					if (canPlayCard && !canPutInHand)
					{
						type = SelectionType.PutIntoPlay;
					}
					else if (canPutInHand && !canPlayCard)
					{
						type = SelectionType.MoveCardToHand;
					}
					SelectCardsDecision selectCardsDecision = new SelectCardsDecision(GameController, hero, (Card c) => revealed.MatchingCards.Contains(c), type, numberOfRevealedCardsToChoose, isOptional: false, null, eliminateOptions: false, allowAutoDecide: false, allAtOnce: false, null, null, null, null, GetCardSource());
					List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
					coroutine = GameController.SelectCardsAndDoAction(selectCardsDecision, (SelectCardDecision d) => GameController.SelectLocationAndMoveCard(hero, d.SelectedCard, destinations, isPutIntoPlay), storedResults, null, GetCardSource());
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(coroutine);
					}
					else
					{
						GameController.ExhaustCoroutine(coroutine);
					}
					SelectCardDecision selectCardDecision = storedResults.FirstOrDefault();
					if (selectCardDecision != null && selectCardDecision.SelectedCard != null)
					{
						selectedCard = selectCardDecision.SelectedCard;
						revealed.MatchingCards.Remove(selectedCard);
					}
					else
					{
						Log.Warning("[" + Card.Title + "]: A card was unable to be selected from a series of revealed cards: " + revealed.MatchingCards.ToCommaList() + ".");
					}
				}
				else
				{
					selectedCard = revealed.MatchingCards.FirstOrDefault();
					if (numberOfMatchesToReveal > 1)
					{
						coroutine = GameController.SendMessageAction("The only matching card found in " + locationToRevealFrom.GetFriendlyName() + " was " + selectedCard.Title + ".", Priority.High, GetCardSource());
						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(coroutine);
						}
						else
						{
							GameController.ExhaustCoroutine(coroutine);
						}
					}
					if (selectedCard != null)
					{
						coroutine = GameController.SelectLocationAndMoveCard(HeroTurnTakerController, selectedCard, destinations, isPutIntoPlay: true, playIfMovingToPlayArea: true, null, null, null, flipFaceDown: false, showOutput: false, null, isDiscardIfMovingToTrash: false, GetCardSource());
						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(coroutine);
						}
						else
						{
							GameController.ExhaustCoroutine(coroutine);
						}
						revealed.MatchingCards.Remove(selectedCard);
					}
				}
				coroutine = GameController.MoveCards(TurnTakerController, revealed.RevealedCards.Where((Card c) => c != selectedCard), TurnTaker.Trash, toBottom: false, isPutIntoPlay: false, playIfMovingToPlayArea: true, responsibleTurnTaker, showIndividualMessages: false, isDiscard: true, null, GetCardSource());
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(coroutine);
				}
				else
				{
					GameController.ExhaustCoroutine(coroutine);
				}
			}
			else
			{
				coroutine = GameController.SendMessageAction("There were no " + cardCriteriaDescription + " in " + locationToRevealFrom.GetFriendlyName() + " to reveal.", Priority.High, GetCardSource());
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(coroutine);
				}
				else
				{
					GameController.ExhaustCoroutine(coroutine);
				}
			}
			List<Location> list = new List<Location>();
			list.Add(locationToRevealFrom.OwnerTurnTaker.Revealed);
			List<Card> cardsInList = revealedCards.SelectMany((RevealCardsAction rc) => rc.RevealedCards).ToList();
			coroutine = base.GameController.CleanupCardsAtLocations(base.TurnTakerController, list, locationToRevealFrom.OwnerTurnTaker.Trash, toBottom: false, addInhibitorException: true, shuffleAfterwards: false, sendMessage: false, isDiscard: true, isReturnedToOriginalLocation: false, cardsInList);
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
