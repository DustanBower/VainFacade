using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacade.TheMidnightBazaar
{
    public abstract class TheMidnightBazaarUtilityCardController : CardController
    {
        public TheMidnightBazaarUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

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
            return well.IsInPlayAndHasGameText;
        }

        public bool IsBlindedQueenInPlay()
        {
            Card queen = FindCard(BlindedQueenIdentifier);
            if (queen == null)
                return false;
            return queen.IsInPlayAndHasGameText;
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
            yield break;
        }

        public IEnumerator DropCardsFromHand(TurnTaker tt, int number, bool requireUnique, bool optional, List<bool> cardsMoved, CardSource cardSource)
        {
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
                int requiredCount = number;
                if (optional)
                    requiredCount = 0;
                if (number > 1 && requireUnique)
                {
                    // Have the player choose one card at a time
                    List<SelectCardDecision> choices = new List<SelectCardDecision>();
                    bool stillChoosing = true;
                    for (int i = 0; i < number && stillChoosing; i++)
                    {
                        LinqCardCriteria fullCriteria = new LinqCardCriteria((Card c) => handCriteria(c) && !choices.Where((SelectCardDecision scd) => scd.Choices.Any((Card p) => p.Title == c.Title)).Any());
                        IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(GameController.FindTurnTakerController(tt).ToHero(), SelectionType.MoveCardBelowCard, fullCriteria, choices, optional && choices.Count == 0, cardSource: cardSource);
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
                    // Have the player choose the cards, and move the cards they choose
                    IEnumerator selectCoroutine = GameController.SelectCardsAndDoAction(GameController.FindTurnTakerController(tt).ToHero(), new LinqCardCriteria(handCriteria), SelectionType.MoveCardBelowCard, (Card c) => GameController.MoveCard(TurnTakerController, c, FindCard(EmptyWellIdentifier).UnderLocation, playCardIfMovingToPlayArea: false, responsibleTurnTaker: tt, storedResults: moves, doesNotEnterPlay: true, cardSource: cardSource), numberOfCards: number, optional: optional, requiredDecisions: requiredCount, cardSource: cardSource);
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
            yield break;
        }
    }
}
