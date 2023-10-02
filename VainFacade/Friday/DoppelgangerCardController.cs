using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Friday
{
	public class DoppelgangerCardController:CardController
	{
		public DoppelgangerCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //Select a deck.
            List<SelectLocationDecision> results = new List<SelectLocationDecision>();
            IEnumerator coroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.RevealCardsFromDeck, (Location L) => L.IsDeck && base.GameController.IsLocationVisibleToSource(L, GetCardSource()), results, false, null, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            Location deck = GetSelectedLocation(results);

            if (deck != null)
            {
                //You may destroy a target with 4 or fewer hp from that deck.
                coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.NativeDeck == deck && c.IsTarget && c.HitPoints.Value <= 4, $"targets with 4 or fewer HP from {deck.GetFriendlyName()}", false), true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                //Reveal the top 3 cards of that deck. You may play or discard one of the revealed cards. Replace the other cards in order.
                //List<Card> revealedCards = new List<Card>();
                //coroutine = base.GameController.RevealCards(this.TurnTakerController, deck, 3, revealedCards, cardSource: GetCardSource());
                //if (base.UseUnityCoroutines)
                //{
                //    yield return base.GameController.StartCoroutine(coroutine);
                //}
                //else
                //{
                //    base.GameController.ExhaustCoroutine(coroutine);
                //}
                //if (revealedCards.Count() > 0)
                //{
                //    Log.Debug("RevealedCards contains: " + revealedCards.Select((Card c) => c.Title).ToCommaList());
                //    SelectCardsDecision decision = new SelectCardsDecision(base.GameController, DecisionMaker, (Card c) => revealedCards.Contains(c),
                //        SelectionType.Custom, 1, false, 0, cardSource: GetCardSource());
                //    coroutine = base.GameController.SelectCardsAndDoAction(decision, PlayOrDiscard, cardSource: GetCardSource());
                //    if (base.UseUnityCoroutines)
                //    {
                //        yield return base.GameController.StartCoroutine(coroutine);
                //    }
                //    else
                //    {
                //        base.GameController.ExhaustCoroutine(coroutine);
                //    }

                //    coroutine = CleanupRevealedCards(deck.OwnerTurnTaker.Revealed, deck);
                //    if (base.UseUnityCoroutines)
                //    {
                //        yield return base.GameController.StartCoroutine(coroutine);
                //    }
                //    else
                //    {
                //        base.GameController.ExhaustCoroutine(coroutine);
                //    }
                //}

                coroutine = RevealThreeCardsFromTopOfDeck_DetermineTheirLocationModified(DecisionMaker, this.TurnTakerController, deck, deck, false, this.TurnTaker, null);
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

        private IEnumerator PlayOrDiscard(SelectCardDecision decision)
        {
            Card selected = decision.SelectedCard;

            CanPlayCardResult canPlayCard = base.GameController.CanPlayCard(FindCardController(decision.SelectedCard));
            //bool canPlay = base.GameController.CanPlayCards(this.TurnTakerController, GetCardSource());
            //string message = "";
            
            //if (!canPlay)
            //{
            //    message = $"{this.TurnTaker.Name} cannot play cards, so {selected.Title} is discarded.";
            //}
            //else if (canPlayCard != 0)
            //{
            //    message = $"{selected.Title} cannot be played, so it is discarded.";
            //    if (canPlayCard == CanPlayCardResult.CannotBecauseLimitedAndInPlay)
            //    {
            //        message = $"{this.Card.Title} cannot play {selected.Title} because it is limited and already in play.";
            //    }
            //}

            IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(base.HeroTurnTakerController, "Play", SelectionType.PlayCard, () => base.GameController.MoveCard(DecisionMaker, decision.SelectedCard, selected.Owner.PlayArea, responsibleTurnTaker: this.TurnTaker, playCardIfMovingToPlayArea: true, cardSource:GetCardSource()), canPlayCard == 0),
                new Function(base.HeroTurnTakerController, "Discard", SelectionType.DiscardCard, () => base.GameController.MoveCard(DecisionMaker, decision.SelectedCard, selected.NativeTrash, isDiscard: true, responsibleTurnTaker: this.TurnTaker), null)
                };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, true, null, null, new Card[] {selected } , GetCardSource());
            IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(choose);
            }
            else
            {
                base.GameController.ExhaustCoroutine(choose);
            }
        }

        private IEnumerator PlayOrDiscardCard(Card selected)
        {

            CanPlayCardResult canPlayCard = base.GameController.CanPlayCard(FindCardController(selected));
            //bool canPlay = base.GameController.CanPlayCards(this.TurnTakerController, GetCardSource());
            //string message = "";

            //if (!canPlay)
            //{
            //    message = $"{this.TurnTaker.Name} cannot play cards, so {selected.Title} is discarded.";
            //}
            //else if (canPlayCard != 0)
            //{
            //    message = $"{selected.Title} cannot be played, so it is discarded.";
            //    if (canPlayCard == CanPlayCardResult.CannotBecauseLimitedAndInPlay)
            //    {
            //        message = $"{this.Card.Title} cannot play {selected.Title} because it is limited and already in play.";
            //    }
            //}

            IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(base.HeroTurnTakerController, "Play", SelectionType.PlayCard, () => base.GameController.MoveCard(DecisionMaker, selected, selected.Owner.PlayArea, responsibleTurnTaker: this.TurnTaker, playCardIfMovingToPlayArea: true, cardSource:GetCardSource()), canPlayCard == 0),
                new Function(base.HeroTurnTakerController, "Discard", SelectionType.DiscardCard, () => base.GameController.MoveCard(DecisionMaker, selected, selected.NativeTrash, isDiscard: true, responsibleTurnTaker: this.TurnTaker), null)
                };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, true, null, null, new Card[] { selected }, GetCardSource());
            IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(choose);
            }
            else
            {
                base.GameController.ExhaustCoroutine(choose);
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            //Console.WriteLine("Returning custom selection text");
            return new CustomDecisionText(
            $"Select a card to play or discard.",
            $"{decision.DecisionMaker.Name} is selecting a card to play or discard.",
            $"Vote for a card to play or discard.",
            $"a card to play or discard."
            );
        }

        //Modified to play or discard the selected card, and move the other cards back to the deck
        //I don't know why, but this works while the code I wrote myself doesn't work on the Aeon Men deck.
        private IEnumerator RevealThreeCardsFromTopOfDeck_DetermineTheirLocationModified(HeroTurnTakerController hero, TurnTakerController revealingTurnTaker, Location deck, Location secondAndThirdDestination, bool secondAndThirdToBottom, TurnTaker responsibleTurnTaker = null, List<Card> storedResults = null)
        {
            List<Card> revealedCards = new List<Card>();
            IEnumerator coroutine = GameController.RevealCards(revealingTurnTaker, deck, 3, revealedCards, fromBottom: false, RevealedCardDisplay.None, null, GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
            List<Card> actuallyRevealedCards = revealedCards.Where((Card c) => c.Location.IsRevealed).ToList();
            List<Card> allRevealedCards = actuallyRevealedCards.ToList();
            storedResults?.AddRange(actuallyRevealedCards);
            if (actuallyRevealedCards.Count() > 1)
            {
                if (actuallyRevealedCards.Count() == 2)
                {
                    IEnumerator coroutine2 = GameController.SendMessageAction("There are only 2 cards to reveal.", Priority.High, GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine2);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine2);
                    }
                }
                bool isDiscard = false;
                List<SelectCardDecision> selectCardDecision = new List<SelectCardDecision>();



                //SelectionType selectionType = SelectionType.MoveCardOnDeck;
                //if (firstCardDestination.IsTrash)
                //{
                //    selectionType = SelectionType.DiscardCard;
                //    isDiscard = true;
                //}
                //else if (firstCardDestination.IsHand)
                //{
                //    selectionType = SelectionType.MoveCardToHand;
                //}
                //else if (firstOnBottom)
                //{
                //    selectionType = SelectionType.MoveCardOnBottomOfDeck;
                //}

                SelectionType selectionType = SelectionType.Custom;
                //Console.WriteLine("Performing Card Selection, selection type " + SelectionType.Custom.ToString());
                IEnumerator coroutine3 = GameController.SelectCardAndStoreResults(hero, selectionType, actuallyRevealedCards, selectCardDecision, optional: false, allowAutoDecide: false, null, null, null, null, null, maintainCardOrder: false, ignoreBattleZone: true, cardSource:GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine3);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine3);
                }
                Card selectedCard = GetSelectedCard(selectCardDecision);
                if (selectedCard != null)
                {
                    GameController gameController = GameController;
                    TurnTakerController turnTakerController = TurnTakerController;
                    Card cardToMove = selectedCard;
                    IEnumerable<IDecision> decisionSources = selectCardDecision.CastEnumerable<SelectCardDecision, IDecision>();
                    TurnTaker responsibleTurnTaker2 = responsibleTurnTaker;
                    bool isDiscard2 = isDiscard;
                    CardSource cardSource = GetCardSource();



                    //IEnumerator coroutine4 = gameController.MoveCard(turnTakerController, cardToMove, firstCardDestination, firstOnBottom, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, decisionSources, responsibleTurnTaker2, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard2, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
                    IEnumerator coroutine4 = PlayOrDiscardCard(selectedCard);
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine4);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine4);
                    }



                    actuallyRevealedCards.Remove(selectedCard);
                    //if (actuallyRevealedCards.Count() > 1 && secondAndThirdDestination.IsDeck)
                    //{
                    //    List<SelectCardDecision> veryTopBottom = new List<SelectCardDecision>();
                    //    selectionType = SelectionType.MoveCardOnDeck;
                    //    if (secondAndThirdToBottom)
                    //    {
                    //        selectionType = SelectionType.MoveCardOnBottomOfDeck;
                    //    }
                    //    IEnumerator coroutine5 = GameController.SelectCardAndStoreResults(hero, selectionType, actuallyRevealedCards, veryTopBottom, optional: false, allowAutoDecide: false, null, null, null, null, null, maintainCardOrder: false, ignoreBattleZone: true);
                    //    if (UseUnityCoroutines)
                    //    {
                    //        yield return GameController.StartCoroutine(coroutine5);
                    //    }
                    //    else
                    //    {
                    //        GameController.ExhaustCoroutine(coroutine5);
                    //    }
                    //    GameController gameController2 = GameController;
                    //    TurnTakerController turnTakerController2 = TurnTakerController;
                    //    Card selectedCard3 = veryTopBottom.FirstOrDefault().SelectedCard;
                    //    decisionSources = veryTopBottom.CastEnumerable<SelectCardDecision, IDecision>();
                    //    responsibleTurnTaker2 = responsibleTurnTaker;
                    //    cardSource = GetCardSource();
                    //    IEnumerator moveCard2 = gameController2.MoveCard(turnTakerController2, selectedCard3, secondAndThirdDestination, secondAndThirdToBottom, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, decisionSources, responsibleTurnTaker2, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
                    //    actuallyRevealedCards.Remove(veryTopBottom.FirstOrDefault().SelectedCard);
                    //    GameController gameController3 = GameController;
                    //    TurnTakerController turnTakerController3 = TurnTakerController;
                    //    Card cardToMove2 = actuallyRevealedCards.FirstOrDefault();
                    //    decisionSources = veryTopBottom.CastEnumerable<SelectCardDecision, IDecision>();
                    //    responsibleTurnTaker2 = responsibleTurnTaker;
                    //    cardSource = GetCardSource();
                    //    IEnumerator coroutine6 = gameController3.MoveCard(turnTakerController3, cardToMove2, secondAndThirdDestination, secondAndThirdToBottom, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, decisionSources, responsibleTurnTaker2, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
                    //    if (UseUnityCoroutines)
                    //    {
                    //        yield return GameController.StartCoroutine(coroutine6);
                    //        yield return GameController.StartCoroutine(moveCard2);
                    //    }
                    //    else
                    //    {
                    //        GameController.ExhaustCoroutine(coroutine6);
                    //        GameController.ExhaustCoroutine(moveCard2);
                    //    }
                    //}
                    //else if (revealedCards.Count() > 0 && !secondAndThirdDestination.IsDeck)
                    //{
                    //    coroutine3 = GameController.MoveCards(TurnTakerController, actuallyRevealedCards, secondAndThirdDestination, toBottom: false, isPutIntoPlay: false, playIfMovingToPlayArea: true, responsibleTurnTaker, showIndividualMessages: false, isDiscard: false, null, GetCardSource());
                    //    if (UseUnityCoroutines)
                    //    {
                    //        yield return GameController.StartCoroutine(coroutine3);
                    //    }
                    //    else
                    //    {
                    //        GameController.ExhaustCoroutine(coroutine3);
                    //    }
                    //}
                    //else
                    //{
                    //    GameController gameController4 = GameController;
                    //    TurnTakerController turnTakerController4 = TurnTakerController;
                    //    Card cardToMove3 = actuallyRevealedCards.FirstOrDefault();
                    //    decisionSources = selectCardDecision.CastEnumerable<SelectCardDecision, IDecision>();
                    //    responsibleTurnTaker2 = responsibleTurnTaker;
                    //    cardSource = GetCardSource();
                    //    IEnumerator coroutine7 = gameController4.MoveCard(turnTakerController4, cardToMove3, deck, secondAndThirdToBottom, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, decisionSources, responsibleTurnTaker2, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
                    //    if (UseUnityCoroutines)
                    //    {
                    //        yield return GameController.StartCoroutine(coroutine7);
                    //    }
                    //    else
                    //    {
                    //        GameController.ExhaustCoroutine(coroutine7);
                    //    }
                    //}
                }
            }
            else if (actuallyRevealedCards.Count() == 1)
            {
                Card selectedCard = actuallyRevealedCards.FirstOrDefault();
                IEnumerator coroutine8 = GameController.SendMessageAction(selectedCard.Title + " is the only card to reveal.", Priority.High, GetCardSource(), new Card[1] { selectedCard });
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine8);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine8);
                }



                //if (firstCardDestination.IsDeck && !deck.HasCards)
                //{
                //    GameController gameController5 = GameController;
                //    TurnTakerController turnTakerController5 = TurnTakerController;
                //    Card cardToMove4 = selectedCard;
                //    CardSource cardSource = GetCardSource();
                //    coroutine8 = gameController5.MoveCard(turnTakerController5, cardToMove4, firstCardDestination, toBottom: false, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, null, null, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
                //    if (UseUnityCoroutines)
                //    {
                //        yield return GameController.StartCoroutine(coroutine8);
                //    }
                //    else
                //    {
                //        GameController.ExhaustCoroutine(coroutine8);
                //    }
                //}
                //else
                //{
                //    List<MoveCardDestination> list = new List<MoveCardDestination>();
                //    if (firstCardDestination.IsTrash)
                //    {
                //        list.Add(new MoveCardDestination(FindTrashFromDeck(deck)));
                //    }
                //    else
                //    {
                //        list.Add(new MoveCardDestination(deck));
                //    }
                //    if (secondAndThirdToBottom)
                //    {
                //        list.Add(new MoveCardDestination(deck, toBottom: true));
                //    }
                //    GameController gameController6 = GameController;
                //    HeroTurnTakerController decisionMaker = DecisionMaker;
                //    Card cardToMove5 = selectedCard;
                //    CardSource cardSource = GetCardSource();
                //    coroutine8 = gameController6.SelectLocationAndMoveCard(decisionMaker, cardToMove5, list, isPutIntoPlay: false, playIfMovingToPlayArea: true, null, null, null, flipFaceDown: false, showOutput: false, null, isDiscardIfMovingToTrash: false, cardSource);
                //    if (UseUnityCoroutines)
                //    {
                //        yield return GameController.StartCoroutine(coroutine8);
                //    }
                //    else
                //    {
                //        GameController.ExhaustCoroutine(coroutine8);
                //    }
                //}


                coroutine8 = PlayOrDiscardCard(selectedCard);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine8);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine8);
                }
            }
            List<Location> list2 = new List<Location>();
            list2.Add(deck.OwnerTurnTaker.Revealed);
            IEnumerator coroutine9 = GameController.CleanupCardsAtLocations(revealingTurnTaker, list2, deck, toBottom: false, addInhibitorException: true, shuffleAfterwards: false, sendMessage: false, isDiscard: false, isReturnedToOriginalLocation: true, allRevealedCards, GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine9);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine9);
            }
        }
    }
}

