using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Friday
{
	public class BuiltForWarCardController:CardController
	{
		public BuiltForWarCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
		}

        public Guid? DecidedReduce { get; set; }

        public int AmountToReduce;

        public override void AddTriggers()
        {
            //When {Friday} is dealt 5 or more damage from a single source, you may reduce that damage by up to 3.
            //Try changing to TriggerType.ModifyReduceDamage and changing the if to an else if in the response
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.Amount >= 5 && !dd.IsIrreducible, ReduceResponse, new TriggerType[1] {TriggerType.ModifyDamageAmount}, TriggerTiming.Before);
        }

        public override bool CanOrderAffectOutcome(GameAction action)
        {
            if (action is DealDamageAction)
            {
                return ((DealDamageAction)action).Target == this.CharacterCard;
            }
            return false;
        }

        private IEnumerator ReduceResponse(DealDamageAction dd)
        {
            if (!DecidedReduce.HasValue || DecidedReduce.Value != dd.InstanceIdentifier)
            {
                List<SelectNumberDecision> storedResults = new List<SelectNumberDecision>();
                //List<DealDamageAction> list = new List<DealDamageAction>();
                //list.Add(dd);
                IEnumerator coroutine = base.GameController.SelectNumber(DecisionMaker, SelectionType.Custom, 1, 3, optional: true, storedResults: storedResults, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                if (DidSelectNumber(storedResults))
                {
                    DecidedReduce = dd.InstanceIdentifier;
                    AmountToReduce = GetSelectedNumber(storedResults).Value;
                    IEnumerator coroutine3 = base.GameController.ReduceDamage(dd, GetSelectedNumber(storedResults).Value, null, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine3);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine3);
                    }
                }
            }
            else if (DecidedReduce.HasValue && DecidedReduce.Value == dd.InstanceIdentifier)
            {
                IEnumerator coroutine4 = base.GameController.ReduceDamage(dd, AmountToReduce, null, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine4);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine4);
                }
            }
            if (IsRealAction(dd))
            {
                DecidedReduce = null;
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decision is SelectNumberDecision)
            {
                return new CustomDecisionText(
                $"Reduce this damage by:",
                $"{decision.DecisionMaker.Name} is deciding how much to reduce the damage.",
                $"Vote for how much to reduce the damage.",
                $"how much to reduce the damage."
                );
            }
            return null;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Reveal cards from the top of your deck until 2 Mimicries are revealed. Put one into your hand or into play. Shuffle the remaining revealed cards into your deck. If a card entered play this way, destroy this card.
            int num = GetPowerNumeral(0, 2);
            List<MoveCardAction> results = new List<MoveCardAction>();
            IEnumerator coroutine = RevealCards_SelectSome_MoveThem_ReturnTheRestExModified(DecisionMaker, this.TurnTakerController, this.TurnTaker.Deck, (Card c) => base.GameController.DoesCardContainKeyword(c, "mimicry"), num, 1, true, true, true,false, "mimicry", true, results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidMoveCard(results) && results.FirstOrDefault().Destination.IsInPlay)
            {
                coroutine = base.GameController.DestroyCard(DecisionMaker, this.Card, cardSource: GetCardSource());
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

        //Modified version of the function so that I can get the results from the card play
        private IEnumerator RevealCards_SelectSome_MoveThem_ReturnTheRestExModified(HeroTurnTakerController hero, TurnTakerController revealingTurnTaker, Location locationToRevealFrom, Func<Card, bool> cardCriteria, int numberOfMatchesToReveal, int numberOfRevealedCardsToChoose, bool canPutInHand, bool canPlayCard, bool isPutIntoPlay, bool canDiscardCard, string cardCriteriaDescription = "cards", bool shuffleAfter = true, List<MoveCardAction> moveResults = null)
        {
            List<RevealCardsAction> revealedCards = new List<RevealCardsAction>();
            IEnumerator coroutine = GameController.RevealCards(revealingTurnTaker, locationToRevealFrom, cardCriteria, numberOfMatchesToReveal, revealedCards, RevealedCardDisplay.None, GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
            RevealCardsAction revealed = revealedCards.FirstOrDefault();
            if (revealed != null && revealed.FoundMatchingCards)
            {
                Card selectedCard = null;
                List<MoveCardDestination> destinations = new List<MoveCardDestination>();
                if (canPlayCard)
                {
                    Location location = locationToRevealFrom.OwnerTurnTaker.PlayArea;
                    if (locationToRevealFrom.IsSubDeck)
                    {
                        location = locationToRevealFrom.OwnerTurnTaker.FindSubPlayArea(locationToRevealFrom.Identifier);
                    }
                    destinations.Add(new MoveCardDestination(location));
                }
                if (canPutInHand)
                {
                    Location hand = hero.HeroTurnTaker.Hand;
                    if (locationToRevealFrom.OwnerTurnTaker.IsPlayer)
                    {
                        hand = locationToRevealFrom.OwnerTurnTaker.ToHero().Hand;
                    }
                    destinations.Add(new MoveCardDestination(hand));
                }
                if (canDiscardCard)
                {
                    Location location2 = locationToRevealFrom.OwnerTurnTaker.Trash;
                    if (locationToRevealFrom.IsSubDeck)
                    {
                        location2 = locationToRevealFrom.OwnerTurnTaker.FindSubTrash(locationToRevealFrom.Identifier);
                    }
                    destinations.Add(new MoveCardDestination(location2));
                }
                int num = revealed.MatchingCards.Count();
                if (num > 1 && numberOfMatchesToReveal > 1)
                {
                    if (num < numberOfRevealedCardsToChoose)
                    {
                        coroutine = GameController.SendMessageAction("Only found " + num + " " + cardCriteriaDescription + ".", Priority.High, GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(coroutine);
                        }
                    }
                    SelectionType selectionType = SelectionType.MoveCard;
                    if (canPlayCard && !canPutInHand && !canDiscardCard)
                    {
                        selectionType = SelectionType.PutIntoPlay;
                    }
                    else if (canPutInHand && !canPlayCard && !canDiscardCard)
                    {
                        selectionType = SelectionType.MoveCardToHand;
                    }
                    else if (canDiscardCard && !canPutInHand && !canPlayCard)
                    {
                        selectionType = SelectionType.DiscardCard;
                    }
                    GameController gameController = GameController;
                    HeroTurnTakerController hero2 = hero;
                    Func<Card, bool> criteria = (Card c) => revealed.MatchingCards.Contains(c);
                    SelectionType type = selectionType;
                    int? numberOfCards = numberOfRevealedCardsToChoose;
                    CardSource cardSource = GetCardSource();
                    SelectCardsDecision selectCardsDecision = new SelectCardsDecision(gameController, hero2, criteria, type, numberOfCards, isOptional: false, null, eliminateOptions: false, allowAutoDecide: false, allAtOnce: false, null, null, null, null, cardSource);
                    List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
                    coroutine = GameController.SelectCardsAndDoAction(selectCardsDecision, (SelectCardDecision d) => GameController.SelectLocationAndMoveCard(hero, d.SelectedCard, destinations, isPutIntoPlay, storedResults: moveResults), storedResults, null, GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }
                    SelectCardDecision selectCardDecision = storedResults.FirstOrDefault();
                    if (selectCardDecision != null && selectCardDecision.SelectedCard != null)
                    {
                        selectedCard = selectCardDecision.SelectedCard;
                        revealed.MatchingCards.Remove(selectedCard);
                    }
                    else
                    {
                        Log.Warning("[" + Card.Title + "]: A card was unable to be selected from a series of revealed cards: " + revealed.MatchingCards.ToCommaList() + ".");
                    }
                }
                else
                {
                    string arg = ((numberOfMatchesToReveal == 1) ? "first" : "only");
                    selectedCard = revealed.MatchingCards.FirstOrDefault();
                    string message = $"The {arg} matching card found in {locationToRevealFrom.GetFriendlyName()} was {selectedCard.Title}.";
                    coroutine = GameController.SendMessageAction(message, Priority.High, GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }
                    if (selectedCard != null)
                    {
                        GameController gameController2 = GameController;
                        HeroTurnTakerController hero3 = hero;
                        Card cardToMove = selectedCard;
                        List<MoveCardDestination> possibleDestinations = destinations;
                        bool isPutIntoPlay2 = isPutIntoPlay;
                        CardSource cardSource = GetCardSource();
                        coroutine = gameController2.SelectLocationAndMoveCard(hero3, cardToMove, possibleDestinations, isPutIntoPlay2, playIfMovingToPlayArea: true, null, null, moveResults, flipFaceDown: false, showOutput: false, null, isDiscardIfMovingToTrash: false, cardSource);
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(coroutine);
                        }
                        revealed.MatchingCards.Remove(selectedCard);
                    }
                }
                coroutine = GameController.MoveCards(TurnTakerController, revealed.RevealedCards.Where((Card c) => c != selectedCard), locationToRevealFrom, toBottom: false, isPutIntoPlay: false, playIfMovingToPlayArea: true, null, showIndividualMessages: false, isDiscard: false, null, GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }
                if (shuffleAfter)
                {
                    IEnumerator coroutine2 = ShuffleDeck(DecisionMaker, locationToRevealFrom);
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine2);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine2);
                    }
                }
            }
            else
            {
                coroutine = GameController.SendMessageAction("There were no " + cardCriteriaDescription + " in " + locationToRevealFrom.GetFriendlyName() + " to reveal.", Priority.High, GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }
            }
            List<Location> list = new List<Location>();
            list.Add(locationToRevealFrom.OwnerTurnTaker.Revealed);
            List<Card> cardsInList = revealedCards.SelectMany((RevealCardsAction rc) => rc.RevealedCards).ToList();
            coroutine = GameController.CleanupCardsAtLocations(revealingTurnTaker, list, locationToRevealFrom, toBottom: false, addInhibitorException: true, shuffleAfterwards: false, sendMessage: false, isDiscard: false, isReturnedToOriginalLocation: true, cardsInList, GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
        }
    }
}

