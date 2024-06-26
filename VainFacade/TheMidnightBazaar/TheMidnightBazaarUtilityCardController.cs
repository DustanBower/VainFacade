﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheMidnightBazaar
{
    public abstract class TheMidnightBazaarUtilityCardController : CardController
    {
        public TheMidnightBazaarUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        protected enum CustomMode
        {
            PlayerToDropCard,
            PlayerToDropCards,
            PlayerToDropUniqueCards,
            CardToDrop,
            VillainDeckToPutBottomCardIntoPlay
        }

        protected CustomMode currentMode;

        public static readonly string EmptyWellIdentifier = "TheEmptyWell";
        public static readonly string BlindedQueenIdentifier = "TheBlindedQueen";
        public static readonly string ThreenKeyword = "threen";
        public static readonly string UnbindingKeyword = "unbinding";

        public static bool IsThreen(Card c)
        {
            return c.DoKeywordsContain(ThreenKeyword);
        }

        public static bool IsUnbinding(Card c)
        {
            return c.DoKeywordsContain(UnbindingKeyword);
        }

        public static readonly LinqCardCriteria ThreenCards = new LinqCardCriteria((Card c) => IsThreen(c), "Threen");
        public static readonly LinqCardCriteria ThreenInPlay = new LinqCardCriteria((Card c) => IsThreen(c) && c.IsInPlayAndHasGameText, "Threen in play", false, false, "Threen in play", "Threen in play");
        public static readonly LinqCardCriteria UnbindingCards = new LinqCardCriteria((Card c) => IsUnbinding(c), "Unbinding");

        public bool IsEmptyWellInPlay()
        {
            Card well = FindCard(EmptyWellIdentifier);
            if (well == null)
                return false;
            return well.IsInPlayAndHasGameText && base.GameController.IsCardVisibleToCardSource(well, GetCardSource());
        }

        public bool IsBlindedQueenInPlay()
        {
            Card queen = FindCard(BlindedQueenIdentifier);
            if (queen == null)
                return false;
            return queen.IsInPlayAndHasGameText && base.GameController.IsCardVisibleToCardSource(queen, GetCardSource());
        }

        public IEnumerator FetchWellResponse()
        {
            // "search the environment deck and trash for [i]The Empty Well[/i] and put it into play. Shuffle the environment deck."
            Card well = FindCard(EmptyWellIdentifier);
            if (well.IsInDeck || well.IsInTrash)
            {
                IEnumerator playCoroutine = base.GameController.PlayCard(TurnTakerController, well, isPutIntoPlay: true, responsibleTurnTaker: TurnTaker, associateCardSource: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
                IEnumerator shuffleCoroutine = ShuffleDeck(DecisionMaker, TurnTaker.Deck);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shuffleCoroutine);
                }
            }
        }

        public IEnumerator EmptyWellNotInPlayResponse(GameAction ga)
        {
            IEnumerator announceCoroutine = base.GameController.SendMessageAction("[i]The Empty Well[/i] is not in play.", Priority.High, GetCardSource(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(announceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(announceCoroutine);
            }
        }

        public IEnumerator DropCardsFromHand(TurnTaker tt, int number, bool requireUnique, bool optional, List<bool> cardsMoved, CardSource cardSource)
        {
            //Log.Debug("TheMidnightBazaarUtilityCardController.DropCardsFromHand: number = " + number.ToString());
            //Log.Debug("TheMidnightBazaarUtilityCardController.DropCardsFromHand: requireUnique = " + requireUnique.ToString());
            //Log.Debug("TheMidnightBazaarUtilityCardController.DropCardsFromHand: optional = " + optional.ToString());
            // Have [the player for TurnTaker tt] choose and move [number] cards from hand under The Empty Well
            // If [requireUnique], the cards must have different titles
            if (FindCard(EmptyWellIdentifier).IsInPlayAndHasGameText && base.GameController.IsCardVisibleToCardSource(FindCard(EmptyWellIdentifier), cardSource))
            {
                Func<Card, bool> handCriteria = (Card c) => c != null && c.IsInHand;
                if (tt != null)
                {
                    handCriteria = (Card c) => c != null && c.Location == tt.ToHero().Hand;
                }
                if (cardsMoved == null)
                {
                    cardsMoved = new List<bool>(number);
                }
                List<MoveCardAction> moves = new List<MoveCardAction>();
                currentMode = CustomMode.CardToDrop;
                int requiredCount = number;
                if (optional)
                    requiredCount = 0;
                if (number > 1 && requireUnique)
                {
                    //Log.Debug("TheMidnightBazaarUtilityCardController.DropCardsFromHand: choosing multiple cards, one at a time");
                    // Have the player choose one card at a time
                    List<SelectCardDecision> choices = new List<SelectCardDecision>();
                    bool stillChoosing = true;
                    LinqCardCriteria fullCriteria = new LinqCardCriteria((Card c) => handCriteria(c));
                    for (int i = 0; i < number && stillChoosing; i++)
                    {
                        // If any cards have already been chosen, then cards with the same name can't be chosen this time
                        if (choices.Any((SelectCardDecision scd) => scd != null && scd.SelectedCard != null))
                        {
                            List<Card> eliminated = new List<Card>();
                            foreach (SelectCardDecision choice in choices.Where((SelectCardDecision scd) => scd != null && scd.SelectedCard != null))
                            {
                                eliminated.Add(choice.SelectedCard);
                            }
                            fullCriteria = new LinqCardCriteria((Card c) => handCriteria(c) && !eliminated.Where((Card el) => el.Title == c.Title && el.Owner == c.Owner).Any());
                        }
                        //Log.Debug("TheMidnightBazaarUtilityCardController.DropCardsFromHand: calling SelectCardAndStoreResults");
                        currentMode = CustomMode.CardToDrop;
                        IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(GameController.FindTurnTakerController(tt).ToHero(), SelectionType.Custom, fullCriteria, choices, optional && choices.Count == 0, cardSource: cardSource);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(selectCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(selectCoroutine);
                        }
                        SelectCardDecision lastChosen = choices.LastOrDefault();
                        if (lastChosen == null || lastChosen.SelectedCard == null)
                            stillChoosing = false;
                    }
                    // Move the chosen cards
                    List<Card> chosen = new List<Card>();
                    foreach (SelectCardDecision choice in choices)
                    {
                        if (choice != null && choice.SelectedCard != null)
                            chosen.Add(choice.SelectedCard);
                    }
                    //Log.Debug("TheMidnightBazaarUtilityCardController.DropCardsFromHand: calling MoveCards");
                    IEnumerator moveCoroutine = base.GameController.MoveCards(base.GameController.FindTurnTakerController(tt), chosen, FindCard(EmptyWellIdentifier).UnderLocation, playIfMovingToPlayArea: false, responsibleTurnTaker: tt, storedResultsAction: moves, cardSource: cardSource);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(moveCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(moveCoroutine);
                    }
                    // Note whether each move succeeded
                    foreach (MoveCardAction move in moves)
                    {
                        cardsMoved.Add(move.WasCardMoved);
                    }
                }
                else
                {
                    //Log.Debug("TheMidnightBazaarUtilityCardController.DropCardsFromHand: selecting all cards to move");
                    // Have the player choose the cards, and move the cards they choose
                    currentMode = CustomMode.CardToDrop;
                    //Log.Debug("TheMidnightBazaarUtilityCardController.DropCardsFromHand: calling SelectCardsAndDoAction");
                    IEnumerator selectCoroutine = GameController.SelectCardsAndDoAction(GameController.FindTurnTakerController(tt).ToHero(), new LinqCardCriteria(handCriteria), SelectionType.Custom, (Card c) => GameController.MoveCard(TurnTakerController, c, FindCard(EmptyWellIdentifier).UnderLocation, playCardIfMovingToPlayArea: false, responsibleTurnTaker: tt, storedResults: moves, doesNotEnterPlay: true, cardSource: cardSource), numberOfCards: number, optional: false, requiredDecisions: requiredCount, cardSource: cardSource);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(selectCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(selectCoroutine);
                    }
                    // Note whether each move succeeded
                    foreach (MoveCardAction move in moves)
                    {
                        cardsMoved.Add(move.WasCardMoved);
                    }
                }
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            string emptyWellString = "[i]The Empty Well[/i]";
            if (currentMode is CustomMode.CardToDrop)
            {
                return new CustomDecisionText("Select a card to move under " + emptyWellString, "choosing a card to move under " + emptyWellString, "Vote for a card to move under " + emptyWellString, "card to move under " + emptyWellString);
            }
            else if (currentMode is CustomMode.PlayerToDropCard)
            {
                return new CustomDecisionText("Select a player to move a card from their hand under " + emptyWellString, "choosing a player to move a card from their hand under " + emptyWellString, "Vote for a player to move a card from their hand under " + emptyWellString, "player to move a card from their hand under " + emptyWellString);
            }
            else if (currentMode is CustomMode.PlayerToDropCards)
            {
                return new CustomDecisionText("Select a player to move cards under " + emptyWellString, "choosing a player to move cards under " + emptyWellString, "Vote for a player to move cards under " + emptyWellString, "player to move cards under " + emptyWellString);
            }
            else if (currentMode is CustomMode.PlayerToDropUniqueCards)
            {
                return new CustomDecisionText("Select a player to move 2 different cards from their hand under " + emptyWellString, "choosing a player to move cards from their hand under " + emptyWellString, "Vote for a player to move 2 different cards from their hand under " + emptyWellString, "player to move 2 different cards from their hand under " + emptyWellString);
            }
            else if (currentMode is CustomMode.VillainDeckToPutBottomCardIntoPlay)
            {
                return new CustomDecisionText("Select a deck to put its bottom card into play", "choosing a deck to put its bottom card into play", "Vote for a deck to put its bottom card into play", "deck to put the bottom card of into play");
            }
            return base.GetCustomDecisionText(decision);
        }
    }
}
