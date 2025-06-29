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
    public class TheWhisperScribeCardController : TheMidnightBazaarUtilityCardController
    {
        public TheWhisperScribeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, a player may put 2 different cards from their hand under [i]The Empty Well[/i]. If 2 cards are moved this way, search a deck for a card and put it into play, then shuffle that deck and remove this card from the game."
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment && IsEmptyWellInPlay(), SelectPlayerResponse, new TriggerType[] { TriggerType.MoveCard, TriggerType.PutIntoPlay, TriggerType.RemoveFromGame });
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment && !IsEmptyWellInPlay(), EmptyWellNotInPlayResponse, TriggerType.ShowMessage);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, search the environment deck and trash for [i]The Empty Well[/i] and put it into play. Shuffle the environment deck."
            IEnumerator fetchCoroutine = FetchWellResponse();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(fetchCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(fetchCoroutine);
            }
        }

        private IEnumerator SelectPlayerResponse(PhaseChangeAction pca)
        {
            // "... a player may put 2 different cards from their hand under [i]The Empty Well[/i]. If 2 cards are moved this way, search a deck for a card and put it into play, then shuffle that deck. If a card enters play this way, remove this card from the game."
            List<bool> cardsMoved = new List<bool>();
            currentMode = CustomMode.PlayerToDropUniqueCards;
            SelectTurnTakerDecision selection = new SelectTurnTakerDecision(base.GameController, DecisionMaker, GameController.FindTurnTakersWhere((TurnTaker tt) => IsHero(tt) && GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource())), SelectionType.Custom, isOptional: true, cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakerAndDoAction(selection, (TurnTaker tt) => SearchRemoveResponse(tt));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }

        private IEnumerator SearchRemoveResponse(TurnTaker tt)
        {
            // "... a player may put 2 different cards from their hand under [i]The Empty Well[/i]."
            List<bool> cardsMoved = new List<bool>();
            IEnumerator moveCoroutine = DropCardsFromHand(tt, 2, true, true, cardsMoved, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            int cardsDropped = 0;
            foreach (bool b in cardsMoved)
            {
                if (b)
                    cardsDropped++;
            }
            // "If 2 cards are moved this way, search a deck for a card and put it into play, then shuffle that deck."
            if (cardsDropped == 2)
            {
                // Select a deck
                List<SelectLocationDecision> deckChoice = new List<SelectLocationDecision>();
                IEnumerator chooseDeckCoroutine = base.GameController.SelectADeck(base.HeroTurnTakerController, SelectionType.SearchDeck, (Location l) => l.HasCards, storedResults: deckChoice, optional: false, noValidLocationsMessage: "There are no cards in decks to search.", cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(chooseDeckCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(chooseDeckCoroutine);
                }
                Location deck = deckChoice.First().SelectedLocation.Location;
                // Search that deck for a card, put it into play, and shuffle
                List<PlayCardAction> played = new List<PlayCardAction>();
                IEnumerator playCoroutine = base.GameController.SelectAndPlayCard(base.GameController.FindTurnTakerController(tt).ToHero(), (Card c) => c.Location == deck, isPutIntoPlay: true, cardSource: GetCardSource(), noValidCardsMessage: "There are no cards in " + deck.Name + ".", storedResults: played);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
                IEnumerator shuffleCoroutine = ShuffleDeck(base.GameController.FindTurnTakerController(tt).ToHero(), deck);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shuffleCoroutine);
                }
                // "If a card enters play this way, remove this card from the game."
                PlayCardAction act = played.FirstOrDefault();
                if (act.WasCardPlayed)
                {
                    IEnumerator removeCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.OutOfGame, showMessage: true, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, actionSource: act, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(removeCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(removeCoroutine);
                    }
                }
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            string s = $"put 2 different cards from their hand under The Empty Well";

            return new CustomDecisionText(
            $"Select a player to {s}.",
            $"The players are selecting a player to {s}.",
            $"Vote for a player to {s}.",
            $"a player to {s}."
            );
        }
    }
}
