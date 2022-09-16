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
    public class DisasterScenarioCardController : GrandfatherUtilityCardController
    {
        public DisasterScenarioCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            // Show number of cards in environment deck
            SpecialStringMaker.ShowNumberOfCardsAtLocations(() => FindEnvironment().TurnTaker.Deck.ToEnumerable());
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "This card is indestructible while there are cards in the environment deck."
            if (card == base.Card)
            {
                TurnTakerController env = FindEnvironment();
                if (env.TurnTaker.Deck.HasCards)
                {
                    return true;
                }
            }
            return base.AskIfCardIsIndestructible(card);
        }

        private bool _reactingToEmptyDeck;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When the environment deck is empty, destroy this card."
            AddTrigger((GameAction ga) => !_reactingToEmptyDeck && FindEmptyEnvDeck() != null && !base.GameController.IsCardIndestructible(base.Card), DestructResponse, TriggerType.DestroySelf, TriggerTiming.After);
            // "Increase damage dealt by environment cards by 2."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource.IsEnvironmentCard, 2);
            // "At the end of the villain turn, discard the top card of the environment deck. If a target is discarded this way, play it. Otherwise, discard the top 5 cards of each hero deck."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardPlayOrDiscardResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PlayCard });
        }

        private Location FindEmptyEnvDeck()
        {
            return FindLocationsWhere((Location l) => l.IsEnvironment && l.IsDeck && l.Cards.Count() == 0).FirstOrDefault();
        }

        private IEnumerator DestructResponse(GameAction ga)
        {
            // "... destroy this card."
            _reactingToEmptyDeck = true;
            IEnumerator destructCoroutine = DestroyThisCardResponse(ga);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
            _reactingToEmptyDeck = false;
        }

        private IEnumerator DiscardPlayOrDiscardResponse(PhaseChangeAction pca)
        {
            // "... discard the top card of the environment deck."
            List<MoveCardAction> moves = new List<MoveCardAction>();
            IEnumerator discardEnvCoroutine = DiscardCardsFromTopOfDeck(FindEnvironment(), 1, storedResults: moves, showMessage: true, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardEnvCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardEnvCoroutine);
            }
            List<Card> discardedTargets = new List<Card>();
            if (GetNumberOfCardsMoved(moves) > 0)
            {
                List<Card> movedCards = new List<Card>();
                foreach (MoveCardAction mca in moves)
                {
                    if (mca.WasCardMoved)
                    {
                        movedCards.Add(mca.CardToMove);
                        if (mca.CardToMove.IsTarget)
                        {
                            discardedTargets.Add(mca.CardToMove);
                        }
                    }
                }
            }
            if (discardedTargets.Count > 0)
            {
                // "If a target is discarded this way, play it."
                foreach (Card target in discardedTargets)
                {
                    IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, target, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(playCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(playCoroutine);
                    }
                }
            }
            else
            {
                // "Otherwise, discard the top 5 cards of each hero deck."
                IEnumerator discardHeroCoroutine = DiscardTopXCardsOfEachHeroDeckResponse(5, null);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardHeroCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardHeroCoroutine);
                }
            }
        }
    }
}
