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
    internal class GrandfatherCharacterCardController : VillainCharacterCardController
    {
        public GrandfatherCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            // Both sides: Show number of villain Designs in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsVillain && c.DoKeywordsContain(DesignKeyword), "villain Design"));
            // Both sides: Show number of tokens on Arrow of Time (if in play)
            SpecialStringMaker.ShowTokenPool(ArrowOfTimeIdentifier, ArrowOfTimePoolIdentifier).Condition = () => FindCard(ArrowOfTimeIdentifier).IsInPlayAndHasGameText;
            // Both sides: Show message if Arrow of Time is not in play
            SpecialStringMaker.ShowSpecialString(() => "Arrow of Time is not in play.", () => true).Condition = () => !FindCard(ArrowOfTimeIdentifier).IsInPlayAndHasGameText;
            // Front side: Show number of environment cards in trashes
            SpecialStringMaker.ShowTotalNumberOfCardsAtLocations(() => FindLocationsWhere((Location l) => l.IsTrash), "There {0} in trashes.", new LinqCardCriteria((Card c) => c.IsEnvironment, "environment", useCardsSuffix: false), () => true).Condition = () => !base.Card.IsFlipped;
        }

        protected const string ArrowOfTimeIdentifier = "ArrowOfTime";
        protected const string ArrowOfTimePoolIdentifier = "ArrowOfTimePool";
        protected const string DesignKeyword = "design";

        public override bool AskIfCardIsIndestructible(Card card)
        {
            if (!base.Card.IsFlipped)
            {
                // Front side:
                // "When there is only 1 villain Design in play, it is indestructible."
                if (card.IsVillain && card.DoKeywordsContain(DesignKeyword))
                {
                    if (FindCardsWhere(new LinqCardCriteria((Card c) => c.IsVillain && c.DoKeywordsContain(DesignKeyword) && c.IsInPlayAndHasGameText), visibleToCard: GetCardSource()).Count() == 1)
                    {
                        return true;
                    }
                }
            }
            else
            {
                // Back side:
                // "When there is only 1 villain Design in play, it is indestructible."
                if (card.IsVillain && card.DoKeywordsContain(DesignKeyword))
                {
                    if (FindCardsWhere(new LinqCardCriteria((Card c) => c.IsVillain && c.DoKeywordsContain(DesignKeyword) && c.IsInPlayAndHasGameText), visibleToCard: GetCardSource()).Count() == 1)
                    {
                        return true;
                    }
                }
            }
            return base.AskIfCardIsIndestructible(card);
        }

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();
            if (!base.Card.IsFlipped)
            {
                // Front side:
                // "{Grandfather} is immune to damage."
                AddSideTrigger(AddImmuneToDamageTrigger((DealDamageAction dda) => dda.Target == base.Card));
                // "When there are 24 or more tokens on [i]Arrow of Time[/i], {Grandfather} controls the timeline. [b]The heroes lose.[/b]"
                AddSideTrigger(AddTrigger((ModifyTokensAction mta) => ArrowOfTimeTokens() >= 24, TimelineGameOverResponse, TriggerType.GameOver, TriggerTiming.After));
                // "At the end of each turn, if there are {H + 1} or more environment cards in the trash, each player may discard any number of cards. If {H} or more cards are discarded this way, flip {Grandfather}'s character cards."
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => FindCardsWhere(new LinqCardCriteria((Card c) => c.IsEnvironment && c.Location.IsTrash), visibleToCard: GetCardSource()).Count() >= H + 1, DiscardCardsToFlipResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.FlipCard }));

                if (base.IsGameAdvanced)
                {
                    // Front side, Advanced:
                    // "At the end of the villain turn, discard the top 2 cards of each hero deck."
                    AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, Discard2TopCardsResponse, TriggerType.DiscardCard));
                }
            }
            else
            {
                // Back side:
                // "When there are 24 or more tokens on [i]Arrow of Time[/i], {Grandfather} controls the timeline. [b]The heroes lose.[/b]"
                AddSideTrigger(AddTrigger((ModifyTokensAction mta) => ArrowOfTimeTokens() >= 24, TimelineGameOverResponse, TriggerType.GameOver, TriggerTiming.After));
                // "At the start of each player's turn, that player may discard any number of cards. For each card discarded this way, increase damage dealt to {Grandfather} by 1 this turn."
                AddSideTrigger(AddStartOfTurnTrigger((TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitatedOrOutOfGame, DiscardCardsToIncreaseDamageResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.CreateStatusEffect }));
                // "At the end of the villain turn, {Grandfather} deals each hero 3 energy damage."
                AddSideTrigger(AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => IsHeroCharacterCard(c), TargetType.All, 3, DamageType.Energy));

                if (base.IsGameAdvanced)
                {
                    // Back side, Advanced:
                    // "Increase damage dealt to hero targets by 1."
                    AddSideTrigger(AddIncreaseDamageTrigger((DealDamageAction dda) => IsHeroTarget(dda.Target), (DealDamageAction dda) => 1));
                }
            }
            AddDefeatedIfDestroyedTriggers();
            AddDefeatedIfMovedOutOfGameTriggers();
        }

        private int ArrowOfTimeTokens()
        {
            Card timeline = FindCard(ArrowOfTimeIdentifier);
            if (timeline.IsInPlayAndHasGameText)
            {
                TokenPool arrowPool = timeline.FindTokenPool(ArrowOfTimePoolIdentifier);
                if (arrowPool != null)
                {
                    return arrowPool.CurrentValue;
                }
            }
            return 0;
        }

        private IEnumerator TimelineGameOverResponse(GameAction ga)
        {
            // "... {Grandfather} controls the timeline."
            IEnumerator messageCoroutine = base.GameController.SendMessageAction("There are " + ArrowOfTimeTokens().ToString() + " tokens on Arrow of Time. " + base.Card.Title + " has taken control of the timeline.", Priority.Critical, GetCardSource(), FindCard(ArrowOfTimeIdentifier).ToEnumerable());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            // "[b]The heroes lose.[/b]"
            IEnumerator endCoroutine = base.GameController.GameOver(EndingResult.AlternateDefeat, base.Card.Title + " has taken control of the timeline.", cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(endCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(endCoroutine);
            }
        }

        private IEnumerator DiscardCardsToFlipResponse(PhaseChangeAction pca)
        {
            // "... each player may discard any number of cards."
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = base.GameController.EachPlayerDiscardsCards(0, null, results, showCounter: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "If {H} or more cards are discarded this way, flip {Grandfather}'s character cards."
            if (GetNumberOfCardsDiscarded(results) >= H)
            {
                IEnumerator flipCoroutine = base.GameController.FlipCard(this, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(flipCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(flipCoroutine);
                }
            }
        }

        private IEnumerator Discard2TopCardsResponse(PhaseChangeAction pca)
        {
            // "... discard the top 2 cards of each hero deck."
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.Card.Title + " discards the top 2 cards of each hero deck.", Priority.Low, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => IsHero(tt) && !tt.ToHero().IsIncapacitatedOrOutOfGame, "heroes with cards"), SelectionType.DiscardFromDeck, (TurnTaker tt) => DiscardCardsFromTopOfDeck(FindHeroTurnTakerController(tt.ToHero()), 2, responsibleTurnTaker: base.TurnTaker), allowAutoDecide: true, numberOfCards: 2, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }

        private IEnumerator DiscardCardsToIncreaseDamageResponse(PhaseChangeAction pca)
        {
            // "... that player may discard any number of cards."
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = SelectAndDiscardCards(FindHeroTurnTakerController(pca.ToPhase.TurnTaker.ToHero()), null, requiredDecisions: 0, storedResults: results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "For each card discarded this way, increase damage dealt to {Grandfather} by 1 this turn."
            int discarded = GetNumberOfCardsDiscarded(results);
            if (discarded > 0)
            {
                IncreaseDamageStatusEffect buff = new IncreaseDamageStatusEffect(discarded);
                buff.TargetCriteria.IsSpecificCard = base.Card;
                buff.UntilCardLeavesPlay(base.Card);
                buff.UntilThisTurnIsOver(base.Game);
                IEnumerator statusCoroutine = AddStatusEffect(buff);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
            else
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction("No cards were discarded, so damage dealt to " + base.Card.Title + " will not be increased.", Priority.Medium, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
        }
    }
}
