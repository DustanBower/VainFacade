using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheMidnightBazaar
{
    public class TheEmptyWellCardController : TheMidnightBazaarUtilityCardController
    {
        public TheEmptyWellCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            // Show list of cards under this card (if this is in play and has cards under it)
            SpecialStringMaker.ShowListOfCardsAtLocation(base.Card.UnderLocation, new LinqCardCriteria((Card c) => true, ""), () => base.Card.IsInPlayAndHasGameText && base.Card.UnderLocation.HasCards).Condition = () => base.Card.IsInPlayAndHasGameText && base.Card.UnderLocation.HasCards;
            // Show "There are no cards under The Empty Well." (if this is in play and has no cards under it)
            SpecialStringMaker.ShowSpecialString(() => "There are no cards under " + base.Card.Title + ".", () => base.Card.IsInPlayAndHasGameText && !base.Card.UnderLocation.HasCards).Condition = () => base.Card.IsInPlayAndHasGameText && !base.Card.UnderLocation.HasCards;
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "This card is indestructible."
            return card == base.Card;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            //When a card would enter play, if another copy of that card is under this card, put that card under this card instead.
            AddTrigger((CardEntersPlayAction c) => base.Card.UnderLocation.Cards.Any((Card u) => u.Title == c.CardEnteringPlay.Title && u.Owner == c.CardEnteringPlay.Owner) && base.GameController.IsCardVisibleToCardSource(c.CardEnteringPlay, GetCardSource()), PutInWellResponse, new TriggerType[] { TriggerType.CancelAction, TriggerType.MoveCard }, TriggerTiming.Before);

            //At the end of the environment turn, a player draws or discards 2 cards.
            //If a card is discarded this way, discard a card from under this card.
            //Otherwise a player puts a card from their hand under this card.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.DiscardCard, TriggerType.MoveCard });
        }

        private IEnumerator PutInWellResponse(CardEntersPlayAction cepa)
        {
            Card entering = cepa.CardEnteringPlay;
            List<Card> associated = new List<Card>(1);
            associated.Add(entering);
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(entering.Title + " matches a card under " + base.Card.Title + ", so it moves under " + base.Card.Title + "!", Priority.High, GetCardSource(), associatedCards: associated);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            IEnumerator cancelCoroutine = CancelAction(cepa);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cancelCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cancelCoroutine);
            }
            IEnumerator removeCoroutine = base.GameController.MoveCard(base.TurnTakerController, entering, this.Card.UnderLocation, responsibleTurnTaker: base.TurnTaker, actionSource: cepa, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(removeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(removeCoroutine);
            }
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            //At the end of the environment turn, a player draws or discards 2 cards.
            List<SelectTurnTakerDecision> TurnTakerResults = new List<SelectTurnTakerDecision>();
            decisionMode = CustomDecisionMode.DrawOrDiscard;
            IEnumerator coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.Custom, TurnTakerResults,additionalCriteria: (TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
            if (DidSelectTurnTaker(TurnTakerResults))
            {
                TurnTaker selected = GetSelectedTurnTaker(TurnTakerResults);
                HeroTurnTakerController selectedHTTC = FindHeroTurnTakerController(selected.ToHero());
                IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(base.HeroTurnTakerController, "Draw 2 cards", SelectionType.DrawCard, () => DrawCards(selectedHTTC, 2), CanDrawCards(selectedHTTC), $"{selected.Name} has no cards to discard, so they must draw 2 cards"),
                new Function(base.HeroTurnTakerController, "Discard 2 cards", SelectionType.DiscardCard, () => SelectAndDiscardCards(selectedHTTC, 2, storedResults: discardResults, responsibleTurnTaker: selected), selected.ToHero().HasCardsInHand, $"{selected.Name} cannot draw cards, so they must discard 2 cards")
                };

                SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, false, null, $"{selected.Name} cannot draw or discard cards", null, GetCardSource());
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

            if (DidDiscardCards(discardResults))
            {
                //If a card is discarded this way, discard a card from under this card.
                List<SelectCardDecision> cardResults = new List<SelectCardDecision>();
                coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.DiscardCard, FindCardsWhere((Card c) => this.Card.UnderLocation.Cards.Contains(c)), cardResults, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                
                if (DidSelectCard(cardResults))
                {
                    Card move = GetSelectedCard(cardResults);
                    coroutine = base.GameController.MoveCard(this.TurnTakerController, move, move.NativeTrash, isDiscard: true, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
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
            else
            {
                //Otherwise a player puts a card from their hand under this card.
                List<SelectTurnTakerDecision> TurnTakerResults2 = new List<SelectTurnTakerDecision>();
                decisionMode = CustomDecisionMode.PutCardFromHandUnderWell;
                coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.Custom, TurnTakerResults2, additionalCriteria: (TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame && tt.ToHero().HasCardsInHand, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidSelectTurnTaker(TurnTakerResults2))
                {
                    TurnTaker PutInWell = GetSelectedTurnTaker(TurnTakerResults2);
                    HeroTurnTakerController PutInWellhttc = FindHeroTurnTakerController(PutInWell.ToHero());
                    coroutine = base.GameController.SelectCardFromLocationAndMoveIt(PutInWellhttc, PutInWell.ToHero().Hand, new LinqCardCriteria((Card c) => true), new MoveCardDestination[] { new MoveCardDestination(this.Card.UnderLocation) }, cardSource: GetCardSource());
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
        }

        private enum CustomDecisionMode
        {
            DrawOrDiscard,
            PutCardFromHandUnderWell
        };

        private CustomDecisionMode decisionMode;

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            string s;
            if (decisionMode == CustomDecisionMode.DrawOrDiscard)
                s = "draw or discard 2 cards";
                
            else if (decisionMode == CustomDecisionMode.PutCardFromHandUnderWell)
            {
                s = $"put a card from their hand under {this.Card.Title}";
            }
            else
            {
                return base.GetCustomDecisionText(decision);
            }

            return new CustomDecisionText(
            $"Select a player to {s}.",
            $"The players are selecting a player to {s}.",
            $"Vote for a player to {s}.",
            $"a player to {s}."
            );
        }
    }
}
