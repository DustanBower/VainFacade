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
    }
}
