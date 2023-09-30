using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace VainFacadePlaytest.Friday
{
	public class ProteanDoomCardController:CardController
	{
		public ProteanDoomCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.AddsPowers);
		}

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            IEnumerator coroutine = SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => c.IsRealCard && c.IsTarget && c.IsInPlayAndHasGameText, "target"), storedResults, isPutIntoPlay, decisionSources);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override IEnumerable<Card> FilterDecisionCardChoices(SelectCardDecision decision)
        {
            if (decision.SelectionType == SelectionType.MoveCardNextToCard && decision.Choices.Where((Card c) => !IsHeroTarget(c)).Count() > 0)
            {
                return decision.Choices.Where((Card c) => IsHeroTarget(c));
            }
            return null;
        }

        public override void AddTriggers()
        {
            //Play this card next to a target. Damage dealt to that target by {Friday} is irreducible and increased by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && IsThisCardNextToCard(dd.Target), 1);
            AddMakeDamageIrreducibleTrigger((DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard && IsThisCardNextToCard(dd.Target));

            AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(true, false);

            AddTrigger((UsePowerAction up) => up.Power != null && up.Power.IsContributionFromCardSource && up.Power.CopiedFromCardController == this,
                            ReplaceWithActualPower,
                            TriggerType.FirstTrigger,
                            TriggerTiming.Before);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Reveal the top 3 cards of your deck. Put one into your hand. Put the other revealed cards on the bottom of your deck in any order.
            int num = GetPowerNumeral(0, 3);
            IEnumerator coroutine = RevealThreeCardsFromTopOfDeck_DetermineTheirLocationModified(DecisionMaker, this.TurnTakerController, this.TurnTaker.Deck, this.HeroTurnTaker.Hand, this.TurnTaker.Deck, false, true, num, this.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator ReplaceWithActualPower(UsePowerAction up)
        {
            IEnumerator coroutine = CancelAction(up, false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = UsePowerOnOtherCard(this.Card);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }

        public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cardController)
        {
            Power[] powers = null;
            if (cardController.CardWithoutReplacements == CharacterCard)
            {
                if (!HasPowerBeenUsedThisTurn(new Power(DecisionMaker, this, CardWithoutReplacements.AllPowers.FirstOrDefault(), this.UsePower(), 0, null, GetCardSource())))
                {
                    return new Power[]
                    {
                        new Power(DecisionMaker, cardController, "Reveal the top 3 cards of your deck. Put one into your hand. Put the other revealed cards on the bottom of your deck in any order.", this.DoNothing(), 0, this, GetCardSource())
                    };
                }
            }
            return powers;
        }

        private bool HasPowerBeenUsedThisTurn(Power power)
        {
            List<UsePowerJournalEntry> source = Game.Journal.UsePowerEntriesThisTurn().ToList();
            Func<UsePowerJournalEntry, bool> predicate = delegate (UsePowerJournalEntry p)
            {
                bool flag = power.CardController.CardWithoutReplacements == p.CardWithPower;
                if (!flag && power.CardController.CardWithoutReplacements.SharedIdentifier != null && power.IsContributionFromCardSource)
                {
                    flag = power.CardController.CardWithoutReplacements.SharedIdentifier == p.CardWithPower.SharedIdentifier;
                }
                if (flag)
                {
                    flag &= p.NumberOfUses == 0;
                }
                if (flag)
                {
                    flag &= power.Index == p.PowerIndex;
                }
                if (flag)
                {
                    flag &= power.IsContributionFromCardSource == p.IsContributionFromCardSource;
                }
                if (flag)
                {
                    bool flag2 = power.TurnTakerController == null && p.PowerUser == null;
                    bool flag3 = false;
                    if (power.TurnTakerController != null && power.TurnTakerController.IsHero)
                    {
                        flag3 = power.TurnTakerController.ToHero().HeroTurnTaker == p.PowerUser;
                    }
                    flag = flag && (flag2 || flag3);
                }
                if (flag)
                {
                    if (!power.IsContributionFromCardSource)
                    {
                        if (flag && power.CardController.CardWithoutReplacements.PlayIndex.HasValue && p.CardWithPowerPlayIndex.HasValue)
                        {
                            flag &= power.CardController.CardWithoutReplacements.PlayIndex.Value == p.CardWithPowerPlayIndex.Value;
                        }
                    }
                    else
                    {
                        flag &= p.CardSource == power.CardSource.Card;
                        if (power.CardSource != null && power.CardSource.Card.PlayIndex.HasValue && p.CardSourcePlayIndex.HasValue)
                        {
                            flag &= power.CardSource.Card.PlayIndex.Value == p.CardSourcePlayIndex.Value;
                        }
                    }
                }
                return flag;
            };
            int num = source.Where(predicate).Count();
            if (num > 0)
            {
                if (!GameController.StatusEffectManager.AskIfPowerCanBeReused(power, num))
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        //This version modified to work if Applied Numerology modifies the number of cards to reveal
        private IEnumerator RevealThreeCardsFromTopOfDeck_DetermineTheirLocationModified(HeroTurnTakerController hero, TurnTakerController revealingTurnTaker, Location deck, Location firstCardDestination, Location secondAndThirdDestination, bool firstOnBottom, bool secondAndThirdToBottom, int numToReveal, TurnTaker responsibleTurnTaker = null, List<Card> storedResults = null)
        {
            List<Card> revealedCards = new List<Card>();
            IEnumerator coroutine = GameController.RevealCards(revealingTurnTaker, deck, numToReveal, revealedCards, fromBottom: false, RevealedCardDisplay.None, null, GetCardSource());
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
                if (actuallyRevealedCards.Count() < numToReveal)
                {
                    IEnumerator coroutine2 = GameController.SendMessageAction($"There are only {actuallyRevealedCards.Count()} cards to reveal.", Priority.High, GetCardSource());
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
                SelectionType selectionType = SelectionType.MoveCardOnDeck;
                if (firstCardDestination.IsTrash)
                {
                    selectionType = SelectionType.DiscardCard;
                    isDiscard = true;
                }
                else if (firstCardDestination.IsHand)
                {
                    selectionType = SelectionType.MoveCardToHand;
                }
                else if (firstOnBottom)
                {
                    selectionType = SelectionType.MoveCardOnBottomOfDeck;
                }
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
                    IEnumerator coroutine4 = gameController.MoveCard(turnTakerController, cardToMove, firstCardDestination, firstOnBottom, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, decisionSources, responsibleTurnTaker2, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard2, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine4);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine4);
                    }
                    actuallyRevealedCards.Remove(selectedCard);
                    if (actuallyRevealedCards.Count() > 1 && secondAndThirdDestination.IsDeck && numToReveal == 3)
                    {
                        List<SelectCardDecision> veryTopBottom = new List<SelectCardDecision>();
                        selectionType = SelectionType.MoveCardOnDeck;
                        if (secondAndThirdToBottom)
                        {
                            selectionType = SelectionType.MoveCardOnBottomOfDeck;
                        }
                        IEnumerator coroutine5 = GameController.SelectCardAndStoreResults(hero, selectionType, actuallyRevealedCards, veryTopBottom, optional: false, allowAutoDecide: false, null, null, null, null, null, maintainCardOrder: false, ignoreBattleZone: true, cardSource:GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(coroutine5);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(coroutine5);
                        }
                        GameController gameController2 = GameController;
                        TurnTakerController turnTakerController2 = TurnTakerController;
                        Card selectedCard3 = veryTopBottom.FirstOrDefault().SelectedCard;
                        decisionSources = veryTopBottom.CastEnumerable<SelectCardDecision, IDecision>();
                        responsibleTurnTaker2 = responsibleTurnTaker;
                        cardSource = GetCardSource();
                        IEnumerator moveCard2 = gameController2.MoveCard(turnTakerController2, selectedCard3, secondAndThirdDestination, secondAndThirdToBottom, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, decisionSources, responsibleTurnTaker2, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
                        actuallyRevealedCards.Remove(veryTopBottom.FirstOrDefault().SelectedCard);
                        GameController gameController3 = GameController;
                        TurnTakerController turnTakerController3 = TurnTakerController;
                        Card cardToMove2 = actuallyRevealedCards.FirstOrDefault();
                        decisionSources = veryTopBottom.CastEnumerable<SelectCardDecision, IDecision>();
                        responsibleTurnTaker2 = responsibleTurnTaker;
                        cardSource = GetCardSource();
                        IEnumerator coroutine6 = gameController3.MoveCard(turnTakerController3, cardToMove2, secondAndThirdDestination, secondAndThirdToBottom, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, decisionSources, responsibleTurnTaker2, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(coroutine6);
                            yield return GameController.StartCoroutine(moveCard2);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(coroutine6);
                            GameController.ExhaustCoroutine(moveCard2);
                        }
                    }
                    else if (actuallyRevealedCards.Count() > 1 && secondAndThirdDestination.IsDeck && numToReveal == 2)
                    {
                        coroutine3 = GameController.MoveCards(TurnTakerController, actuallyRevealedCards, secondAndThirdDestination, toBottom: secondAndThirdToBottom, isPutIntoPlay: false, playIfMovingToPlayArea: true, responsibleTurnTaker, showIndividualMessages: false, isDiscard: false, null, GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(coroutine3);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(coroutine3);
                        }
                    }
                    else if (actuallyRevealedCards.Count() > 1 && secondAndThirdDestination.IsDeck && numToReveal == 4)
                    {
                        List<SelectCardDecision> veryTopBottom = new List<SelectCardDecision>();
                        selectionType = SelectionType.MoveCardOnDeck;
                        if (secondAndThirdToBottom)
                        {
                            selectionType = SelectionType.MoveCardOnBottomOfDeck;
                        }
                        IEnumerator coroutine5 = GameController.SelectCardAndStoreResults(hero, selectionType, actuallyRevealedCards, veryTopBottom, optional: false, allowAutoDecide: false, null, null, null, null, null, maintainCardOrder: false, ignoreBattleZone: true, cardSource:GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(coroutine5);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(coroutine5);
                        }
                        GameController gameController2 = GameController;
                        TurnTakerController turnTakerController2 = TurnTakerController;
                        Card selectedCard3 = veryTopBottom.FirstOrDefault().SelectedCard;
                        decisionSources = veryTopBottom.CastEnumerable<SelectCardDecision, IDecision>();
                        responsibleTurnTaker2 = responsibleTurnTaker;
                        cardSource = GetCardSource();
                        IEnumerator moveCard2 = gameController2.MoveCard(turnTakerController2, selectedCard3, secondAndThirdDestination, secondAndThirdToBottom, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, decisionSources, responsibleTurnTaker2, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
                        actuallyRevealedCards.Remove(veryTopBottom.FirstOrDefault().SelectedCard);

                        List<SelectCardDecision> veryTopBottom2 = new List<SelectCardDecision>();
                        coroutine5 = GameController.SelectCardAndStoreResults(hero, selectionType, actuallyRevealedCards, veryTopBottom, optional: false, allowAutoDecide: false, null, null, null, null, null, maintainCardOrder: false, ignoreBattleZone: true);
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(coroutine5);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(coroutine5);
                        }
                        Card selectedCard4 = veryTopBottom2.FirstOrDefault().SelectedCard;
                        decisionSources = veryTopBottom.CastEnumerable<SelectCardDecision, IDecision>();
                        IEnumerator moveCard3 = gameController2.MoveCard(turnTakerController2, selectedCard4, secondAndThirdDestination, secondAndThirdToBottom, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, decisionSources, responsibleTurnTaker2, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
                        actuallyRevealedCards.Remove(veryTopBottom2.FirstOrDefault().SelectedCard);

                        GameController gameController3 = GameController;
                        TurnTakerController turnTakerController3 = TurnTakerController;
                        Card cardToMove2 = actuallyRevealedCards.FirstOrDefault();
                        decisionSources = veryTopBottom.CastEnumerable<SelectCardDecision, IDecision>();
                        responsibleTurnTaker2 = responsibleTurnTaker;
                        cardSource = GetCardSource();
                        IEnumerator coroutine6 = gameController3.MoveCard(turnTakerController3, cardToMove2, secondAndThirdDestination, secondAndThirdToBottom, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, decisionSources, responsibleTurnTaker2, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(coroutine6);
                            yield return GameController.StartCoroutine(moveCard3);
                            yield return GameController.StartCoroutine(moveCard2);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(coroutine6);
                            GameController.ExhaustCoroutine(moveCard3);
                            GameController.ExhaustCoroutine(moveCard2);
                        }
                    }
                    else if (revealedCards.Count() > 0 && !secondAndThirdDestination.IsDeck)
                    {
                        coroutine3 = GameController.MoveCards(TurnTakerController, actuallyRevealedCards, secondAndThirdDestination, toBottom: false, isPutIntoPlay: false, playIfMovingToPlayArea: true, responsibleTurnTaker, showIndividualMessages: false, isDiscard: false, null, GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(coroutine3);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(coroutine3);
                        }
                    }
                    else
                    {
                        GameController gameController4 = GameController;
                        TurnTakerController turnTakerController4 = TurnTakerController;
                        Card cardToMove3 = actuallyRevealedCards.FirstOrDefault();
                        decisionSources = selectCardDecision.CastEnumerable<SelectCardDecision, IDecision>();
                        responsibleTurnTaker2 = responsibleTurnTaker;
                        cardSource = GetCardSource();
                        IEnumerator coroutine7 = gameController4.MoveCard(turnTakerController4, cardToMove3, deck, secondAndThirdToBottom, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, decisionSources, responsibleTurnTaker2, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(coroutine7);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(coroutine7);
                        }
                    }
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
                    GameController gameController5 = GameController;
                    TurnTakerController turnTakerController5 = TurnTakerController;
                    Card cardToMove4 = selectedCard;
                    CardSource cardSource = GetCardSource();
                    coroutine8 = gameController5.MoveCard(turnTakerController5, cardToMove4, firstCardDestination, toBottom: false, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, null, null, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine8);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine8);
                    }
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

