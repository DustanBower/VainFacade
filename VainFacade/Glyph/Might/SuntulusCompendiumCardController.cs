using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class SuntulusCompendiumCardController:GlyphCardUtilities
	{
		public SuntulusCompendiumCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator UsePower(int index = 0)
        {
            //Reveal cards from the top of your deck or trash until a ritual is revealed. Put it into your hand. Discard the other revealed cards.
            List<RevealCardsAction> results = new List<RevealCardsAction>();
            //IEnumerator coroutine = Reveal_Modified(DecisionMaker, this.TurnTakerController, this.TurnTaker.Deck, (Card c) => IsRitual(c), 1, 1, true, false, false, "ritual", this.TurnTaker, results);
            //if (UseUnityCoroutines)
            //{
            //    yield return base.GameController.StartCoroutine(coroutine);
            //}
            //else
            //{
            //    base.GameController.ExhaustCoroutine(coroutine);
            //}


            IEnumerator coroutine = base.GameController.RevealCards(this.TurnTakerController, this.TurnTaker.Deck, (Card c) => IsRitual(c), 1, results, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            RevealCardsAction revealed = results.FirstOrDefault();
            bool DidReveal = revealed.RevealedCards.Any();

            if (revealed != null && revealed.FoundMatchingCards)
            {
                Card selectedCard = revealed.MatchingCards.FirstOrDefault();
                coroutine = base.GameController.MoveCard(this.TurnTakerController, selectedCard, this.HeroTurnTaker.Hand, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }

                coroutine = GameController.MoveCards(TurnTakerController, revealed.RevealedCards.Where((Card c) => c != selectedCard), TurnTaker.Trash, toBottom: false, isPutIntoPlay: false, playIfMovingToPlayArea: true, this.TurnTaker, showIndividualMessages: false, isDiscard: true, null, GetCardSource());
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
                coroutine = GameController.SendMessageAction("There were no rituals in " + this.TurnTaker.Deck.GetFriendlyName() + " to reveal.", Priority.High, GetCardSource());
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
            list.Add(this.TurnTaker.Revealed);
            List<Card> cardsInList = results.SelectMany((RevealCardsAction rc) => rc.RevealedCards).ToList();
            coroutine = GameController.CleanupCardsAtLocations(this.TurnTakerController, list, this.TurnTaker.Trash, toBottom: false, addInhibitorException: true, shuffleAfterwards: false, sendMessage: false, isDiscard: true, isReturnedToOriginalLocation: false, cardsInList, GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            //If you revealed a card from the top of your deck this way, you may play a card.
            if (DidReveal)
            {
                coroutine = SelectAndPlayCardFromHand(DecisionMaker);
                if (UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
        }

        private IEnumerator Reveal_Modified(HeroTurnTakerController hero, TurnTakerController revealingTurnTaker, Location locationToRevealFrom, Func<Card, bool> cardCriteria, int numberOfMatchesToReveal, int numberOfRevealedCardsToChoose, bool canPutInHand, bool canPlayCard, bool isPutIntoPlay, string cardCriteriaDescription, TurnTaker responsibleTurnTaker = null, List<RevealCardsAction> results = null)
        {
            //Modified version of RevealCards_SelectSome_MoveThem_DiscardTheRest that returns results of reveal
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
            results = revealedCards;
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
                    SelectionType selectionType = SelectionType.MoveCard;
                    if (canPlayCard && !canPutInHand)
                    {
                        selectionType = SelectionType.PutIntoPlay;
                    }
                    else if (canPutInHand && !canPlayCard)
                    {
                        selectionType = SelectionType.MoveCardToHand;
                    }
                    GameController gameController = GameController;
                    HeroTurnTakerController hero2 = hero;
                    Func<Card, bool> criteria = (Card c) => revealed.MatchingCards.Contains(c);
                    SelectionType type = selectionType;
                    int? numberOfCards = numberOfRevealedCardsToChoose;
                    CardSource cardSource = GetCardSource();
                    SelectCardsDecision selectCardsDecision = new SelectCardsDecision(gameController, hero2, criteria, type, numberOfCards, isOptional: false, null, eliminateOptions: false, allowAutoDecide: false, allAtOnce: false, null, null, null, null, cardSource);
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
                    coroutine = GameController.SendMessageAction("The only matching card found in " + locationToRevealFrom.GetFriendlyName() + " was " + selectedCard.Title + ".", Priority.High, GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }
                    if (selectedCard != null)
                    {
                        GameController gameController2 = GameController;
                        HeroTurnTakerController heroTurnTakerController = HeroTurnTakerController;
                        Card cardToMove = selectedCard;
                        List<MoveCardDestination> possibleDestinations = destinations;
                        CardSource cardSource = GetCardSource();
                        coroutine = gameController2.SelectLocationAndMoveCard(heroTurnTakerController, cardToMove, possibleDestinations, isPutIntoPlay: true, playIfMovingToPlayArea: true, null, null, null, flipFaceDown: false, showOutput: false, null, isDiscardIfMovingToTrash: false, cardSource);
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
            coroutine = GameController.CleanupCardsAtLocations(revealingTurnTaker, list, locationToRevealFrom.OwnerTurnTaker.Trash, toBottom: false, addInhibitorException: true, shuffleAfterwards: false, sendMessage: false, isDiscard: true, isReturnedToOriginalLocation: false, cardsInList, GetCardSource());
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

