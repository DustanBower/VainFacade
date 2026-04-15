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
            base.SpecialStringMaker.ShowListOfCardsAtLocation(this.TurnTaker.Deck, new LinqCardCriteria((Card c) => IsRitual(c), "ritual", false));
            base.SpecialStringMaker.ShowListOfCardsAtLocation(this.TurnTaker.Trash, new LinqCardCriteria((Card c) => IsRitual(c), "ritual", false));
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Reveal cards from the top of your deck or trash until a ritual is revealed. Put it into your hand. Discard the other revealed cards.
            List<RevealCardsAction> results = new List<RevealCardsAction>();
            IEnumerable<LocationChoice> locations = new LocationChoice[] {new LocationChoice(base.TurnTaker.Deck), new LocationChoice(base.TurnTaker.Trash) };
            List<SelectLocationDecision> locationResults = new List<SelectLocationDecision>();
            IEnumerator choice = base.GameController.SelectLocation(DecisionMaker, locations, SelectionType.RevealCardsFromDeck, locationResults, false, GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(choice);
            }
            else
            {
                base.GameController.ExhaustCoroutine(choice);
            }

            if (DidSelectLocation(locationResults))
            {
                Location revealFrom = GetSelectedLocation(locationResults);


                IEnumerator coroutine = base.GameController.RevealCards(this.TurnTakerController, revealFrom, (Card c) => IsRitual(c), 1, results, RevealedCardDisplay.None, GetCardSource());
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
                    coroutine = base.GameController.MoveCard(this.TurnTakerController, selectedCard, this.HeroTurnTaker.Hand, showMessage: false, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }

                    string text = $"{this.Card.Title} moves {selectedCard.Title} into {this.HeroTurnTaker.Hand.GetFriendlyName()}.";
                    coroutine = base.GameController.SendMessageAction(text, Priority.Low, GetCardSource(), new Card[] { selectedCard });
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
                    coroutine = GameController.SendMessageAction("There were no rituals in " + revealFrom.GetFriendlyName() + " to reveal.", Priority.High, GetCardSource());
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
                if (DidReveal && revealFrom.IsDeck && revealFrom.OwnerTurnTaker == this.TurnTaker)
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
        }
    }
}

