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
    public class ArrowOfTimeCardController : CardController
    {
        public ArrowOfTimeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            // Show number of cards in each hero deck
            SpecialStringMaker.ShowNumberOfCardsAtLocations(() => from tt in FindTurnTakersWhere((TurnTaker tt) => tt.IsHero && !tt.ToHero().IsIncapacitatedOrOutOfGame) select tt.Deck, new LinqCardCriteria());
        }

        protected const string TokenPoolIdentifier = "ArrowOfTimePool";

        private TokenPool TimelineTokenPool()
        {
            return base.Card.FindTokenPool(TokenPoolIdentifier);
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "This card is indestructible."
            if (card == base.Card)
            {
                return true;
            }
            return base.AskIfCardIsIndestructible(card);
        }

        private bool _reactingToEmptyDeck;
        protected const string FirstDiscard = "FirstDiscardFromHeroDeckThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn each villain card discards a card from a hero's deck, that hero may destroy 1 of their Ongoing or Equipment cards. If no card is destroyed this way, add a token to this card."
            AddTrigger((DiscardCardAction dca) => dca.WasCardDiscarded && dca.CardSource != null && dca.CardSource.Card != null && dca.CardSource.Card.IsVillain && !IsPropertyTrue(GeneratePerTargetKey(FirstDiscard, dca.CardSource.Card)) && dca.Origin.IsDeck && dca.Origin.IsHero, DestroyOrAddTokenResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.AddTokensToPool }, TriggerTiming.After);
            ResetFlagsAfterLeavesPlay(FirstDiscard);
            // "When a hero deck becomes empty, add a token to this card and put the bottom 5 cards of that deck's trash on top of the deck."
            AddTrigger((GameAction ga) => !_reactingToEmptyDeck && FindEmptyHeroDeck() != null, AddTokenResetTrashResponse, new TriggerType[] { TriggerType.AddTokensToPool, TriggerType.MoveCard }, TriggerTiming.After);
        }

        private Location FindEmptyHeroDeck()
        {
            return FindLocationsWhere((Location l) => l.IsHero && l.OwnerTurnTaker.CharacterCards.Any((Card c) => !c.IsFlipped && c.IsTarget) && l.IsDeck && l.Cards.Count() == 0).FirstOrDefault();
        }

        private IEnumerator DestroyOrAddTokenResponse(DiscardCardAction dca)
        {
            SetCardPropertyToTrueIfRealAction(GeneratePerTargetKey(FirstDiscard, dca.CardSource.Card));
            // "... that hero may destroy 1 of their Ongoing or Equipment cards."
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(FindHeroTurnTakerController(dca.CardToDiscard.Owner.ToHero()), new LinqCardCriteria((Card c) => c.Owner == dca.CardToDiscard.Owner && (c.IsOngoing || IsEquipment(c)), "belonging to " + dca.CardToDiscard.Owner.Name, false, true, "Ongoing or Equipment card", "Ongoing or Equipment cards"), true, results, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "If no card is destroyed this way, add a token to this card."
            if (!DidDestroyCard(results))
            {
                IEnumerator tokenCoroutine = base.GameController.AddTokensToPool(TimelineTokenPool(), 1, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(tokenCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(tokenCoroutine);
                }
            }
        }

        private IEnumerator AddTokenResetTrashResponse(GameAction ga)
        {
            _reactingToEmptyDeck = true;
            while (FindEmptyHeroDeck() != null)
            {
                Location deckToRefill = FindEmptyHeroDeck();
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(deckToRefill.GetFriendlyName() + " is empty! " + base.CharacterCard.Title + " solidifies his grip on the timeline!", Priority.High, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                // "... add a token to this card..."
                IEnumerator tokenCoroutine = base.GameController.AddTokensToPool(TimelineTokenPool(), 1, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(tokenCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(tokenCoroutine);
                }
                // "... and put the bottom 5 cards of that deck's trash on top of the deck."
                Location trash = FindTrashFromDeck(deckToRefill);
                if (trash.NumberOfCards >= 5)
                {
                    // Take the bottom 5 cards in trash and put them on top of deckToRefill, in order
                    IEnumerator moveCoroutine = base.GameController.BulkMoveCards(base.TurnTakerController, trash.GetBottomCards(5).Reverse(), deckToRefill, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(moveCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(moveCoroutine);
                    }
                }
                else if (trash.NumberOfCards >= 1)
                {
                    // Inform players that fewer cards than usual are being moved
                    string message = "There ";
                    if (trash.NumberOfCards > 1)
                    {
                        message += "are only " + trash.NumberOfCards.ToString() + " cards";
                    }
                    else
                    {
                        message += "is only 1 card";
                    }
                    message += " in " + trash.GetFriendlyName() + " to move.";
                    IEnumerator reportCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(reportCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(reportCoroutine);
                    }
                    // Take all the cards in trash and put them on top of deckToRefill, in order
                    IEnumerator moveCoroutine = base.GameController.BulkMoveCards(base.TurnTakerController, trash.Cards.Reverse(), deckToRefill, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(moveCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(moveCoroutine);
                    }
                }
                else
                {
                    // Inform players that no cards are being moved
                    IEnumerator reportCoroutine = base.GameController.SendMessageAction("There are no cards in " + trash.GetFriendlyName() + " to move.", Priority.High, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(reportCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(reportCoroutine);
                    }
                }
            }
            _reactingToEmptyDeck = false;
        }
    }
}
