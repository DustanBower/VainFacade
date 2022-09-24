using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class GrandfatherUtilityCardController : CardController
    {
        public GrandfatherUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        protected const string CovertKeyword = "covert";
        protected const string DesignKeyword = "design";
        protected const string FulcrumKeyword = "fulcrum";

        protected IEnumerator RevealMoveDiscardDontShuffle(HeroTurnTakerController hero, TurnTakerController revealingTurnTaker, Location locationToRevealFrom, Func<Card, bool> cardCriteria, int numberOfMatchesToReveal, int numberOfRevealedCardsToChoose, bool canPutInHand, bool canPlayCard, bool isPutIntoPlay, string cardCriteriaDescription, TurnTaker responsibleTurnTaker = null)
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
                coroutine = GameController.SendMessageAction("There were no " + cardCriteriaDescription + " cards in " + locationToRevealFrom.GetFriendlyName() + " to reveal.", Priority.High, GetCardSource());
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
            coroutine = CleanupCardsAtLocations(list, locationToRevealFrom.OwnerTurnTaker.Trash, toBottom: false, addInhibitorException: true, shuffleAfterwards: false, sendMessage: false, isDiscard: true, isReturnedToOriginalLocation: false, cardsInList);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
        }

        protected IEnumerator DiscardTopXCardsOfEachHeroDeckResponse(int x, GameAction ga, bool showMessage = true)
        {
            if (x <= 0)
            {
                yield break;
            }
            if (showMessage)
            {
                string message = base.Card.Title + " discards the top ";
                if (x == 1)
                {
                    message += "card ";
                }
                else
                {
                    message += x.ToString() + " cards ";
                }
                message += "of each hero deck.";
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(message, Priority.Low, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.ToHero().IsIncapacitatedOrOutOfGame, "heroes with cards"), SelectionType.DiscardFromDeck, (TurnTaker tt) => DiscardCardsFromTopOfDeck(FindHeroTurnTakerController(tt.ToHero()), x, responsibleTurnTaker: base.TurnTaker), allowAutoDecide: true, numberOfCards: x, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }
    }
}
