using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.EldrenwoodVillage
{
    public class JohnHadawayCardController : AfflictedCardController
    {
        public JohnHadawayCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, one hero target regains 1 HP."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker && CanActivateEffect(base.TurnTakerController, QuaintKey), (PhaseChangeAction pca) => base.GameController.SelectAndGainHP(DecisionMaker, 1, additionalCriteria: (Card c) => c.IsHero, requiredDecisions: 1, cardSource: GetCardSource()), TriggerType.GainHP);
        }

        public override IEnumerator SlainInHumanFormResponse()
        {
            // "... the players collectively discard {H} cards."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            /*Log.Debug("JohnHadawayCardController.SlainInHumanFormResponse: cards discarded so far: " + GetNumberOfCardsDiscarded(discards).ToString());
            List<TurnTaker> canDiscard = base.GameController.FindTurnTakersWhere((TurnTaker tt) => base.GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource()) && tt.IsHero && tt.ToHero().HasCardsInHand).ToList();
            foreach(TurnTaker discarder in canDiscard)
            {
                Log.Debug("JohnHadawayCardController.SlainInHumanFormResponse: " + discarder.Name + " can discard cards");
            }*/
            while (GetNumberOfCardsDiscarded(discards) < H && base.GameController.FindTurnTakersWhere((TurnTaker tt) => base.GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource()) && tt.IsHero && tt.ToHero().HasCardsInHand).Count() > 0)
            {
                IEnumerator discardCoroutine = SelectHeroToDiscardCards(DecisionMaker, 0, H - GetNumberOfCardsDiscarded(discards), storedResultsDiscard: discards, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
            }
        }

        // Copied from GameController with one adjustment: removed numberOfCards argument from call to SelectHeroTurnTaker
        // This way it stops letting heroes discard after a total of H cards have been discarded, but displays "select a hero to discard a card", not "select a hero to discard [#] cards"
        public IEnumerator SelectHeroToDiscardCards(HeroTurnTakerController hero, int minNumberOfCards, int? maxNumberOfCards, bool optionalSelectHero = false, bool optionalDiscardCard = false, bool allowAutoDecide = false, LinqTurnTakerCriteria additionalHeroCriteria = null, Func<Card, bool> additionalCardCriteria = null, List<SelectTurnTakerDecision> storedResultsTurnTaker = null, List<DiscardCardAction> storedResultsDiscard = null, IEnumerable<Card> associatedCards = null, bool forDiscardEffect = false, CardSource cardSource = null)
        {
            if (storedResultsTurnTaker == null)
            {
                storedResultsTurnTaker = new List<SelectTurnTakerDecision>();
            }
            if (additionalHeroCriteria == null)
            {
                string text = "heroes who can discard";
                if (cardSource != null && cardSource.Card != null)
                {
                    text = text + " for " + cardSource.Card.Title;
                }
                additionalHeroCriteria = new LinqTurnTakerCriteria((TurnTaker httc) => true, text);
            }
            LinqTurnTakerCriteria heroCriteria = new LinqTurnTakerCriteria((TurnTaker httc) => httc.IsHero && !httc.ToHero().IsIncapacitatedOrOutOfGame && !httc.ToHero().Hand.IsEmpty && additionalHeroCriteria.Criteria(httc), additionalHeroCriteria.Description);
            int value = (maxNumberOfCards.HasValue ? maxNumberOfCards.Value : minNumberOfCards);
            Func<String> counter = () => "Cards discarded so far: " + (from en in Game.Journal.DiscardCardEntriesThisTurn()
                                                                       where en.Card.Owner.IsHero && en.CardSource == cardSource.Card && en.CardSourcePlayIndex == cardSource.Card.PlayIndex
                                                                       select en).Count();
            IEnumerator coroutine = SelectHeroTurnTaker(hero, SelectionType.DiscardCard, optionalSelectHero, allowAutoDecide, storedResultsTurnTaker, heroCriteria, null, allowIncapacitatedHeroes: false, null, null, counter, true, associatedCards, cardSource);
            if (UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            TurnTaker turnTaker = (from d in storedResultsTurnTaker
                                   where d.Completed
                                   select d.SelectedTurnTaker).FirstOrDefault();
            if (turnTaker != null && turnTaker.IsHero)
            {
                LinqCardCriteria cardCriteria = null;
                if (additionalCardCriteria != null)
                {
                    cardCriteria = new LinqCardCriteria(additionalCardCriteria);
                }
                SelectionType selectionType = SelectionType.DiscardCard;
                if (forDiscardEffect && turnTaker.ToHero().NumberOfCardsInHand < minNumberOfCards)
                {
                    selectionType = SelectionType.DiscardCardsToNoEffect;
                }
                IEnumerator coroutine2 = base.GameController.SelectAndDiscardCards(FindHeroTurnTakerController(turnTaker.ToHero()), maxNumberOfCards, optionalDiscardCard, minNumberOfCards, storedResultsDiscard, allowAutoDecide, null, null, null, cardCriteria, selectionType, null, cardSource);
                if (UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine2);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine2);
                }
            }
        }

        // Copied from GameController with one adjustment: new argument extraInfo to be used in constructing the SelectTurnTakerDecision
        // This allows me to show a count of how many cards have already been discarded, like Handelabra does for cards like The Challenge of Fire
        public IEnumerator SelectHeroTurnTaker(HeroTurnTakerController hero, SelectionType selectionType, bool optional, bool allowAutoDecide, List<SelectTurnTakerDecision> storedResults, LinqTurnTakerCriteria heroCriteria = null, int? numberOfCards = null, bool allowIncapacitatedHeroes = false, GameAction gameAction = null, IEnumerable<DealDamageAction> dealDamageInfo = null, Func<String> extraInfo = null, bool canBeCancelled = true, IEnumerable<Card> associatedCards = null, CardSource cardSource = null)
        {
            IEnumerable<TurnTaker> enumerable = Game.TurnTakers.Where((TurnTaker t) => t.IsHero);
            IEnumerable<TurnTaker> enumerable2 = enumerable;
            if (!allowIncapacitatedHeroes)
            {
                enumerable2 = enumerable2.Where((TurnTaker t) => !t.IsIncapacitatedOrOutOfGame);
            }
            if (heroCriteria != null)
            {
                enumerable2 = enumerable2.Where(heroCriteria.Criteria);
            }
            SelectTurnTakerDecision selectTurnTakerDecision = new SelectTurnTakerDecision(base.GameController, hero, enumerable2, selectionType, optional, allowAutoDecide, numberOfCards, gameAction, dealDamageInfo, extraInfo, cardSource, associatedCards);
            storedResults?.Add(selectTurnTakerDecision);
            List<TurnTaker> source = selectTurnTakerDecision.Choices.ToList();
            if (source.Count() == 0)
            {
                if (heroCriteria != null && heroCriteria.Description != null)
                {
                    string message = "There are no " + heroCriteria.Description + ".";
                    IEnumerator coroutine = base.GameController.SendMessageAction(message, Priority.Medium, cardSource, null, showCardSource: true);
                    if (UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }
                else if (cardSource.CardController.Card.IsHeroCharacterCard && !enumerable.Any((TurnTaker tt) => tt != hero.HeroTurnTaker && base.GameController.IsTurnTakerVisibleToCardSource(tt, cardSource)))
                {
                    IEnumerator coroutine2 = base.GameController.SendMessageAction(cardSource.CardController.Card.Title + " cannot affect other heroes.", Priority.Medium, cardSource);
                    if (UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine2);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine2);
                    }
                }
            }
            else if (source.Count() == 1 && !optional)
            {
                selectTurnTakerDecision.SelectedTurnTaker = source.First();
            }
            else
            {
                IEnumerator coroutine3 = base.GameController.MakeDecisionAction(selectTurnTakerDecision, canBeCancelled);
                if (UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine3);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine3);
                }
            }
        }
    }
}
