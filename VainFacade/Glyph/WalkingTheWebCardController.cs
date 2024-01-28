using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class WalkingTheWebCardController:GlyphCardUtilities
	{
		public WalkingTheWebCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //Destroy any number of your face-down insight cards. 
            List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
            List<Location> locationResults = new List<Location>();
            IEnumerator coroutine = SelectAndDestroyCardAndReturnLocation(DecisionMaker, new LinqCardCriteria(IsFaceDownInsight, "face-down insight"), true, destroyResults, locationResults, CustomDecisionMode.Insight, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            while (locationResults.FirstOrDefault() != null)
            {
                if (DidDestroyCard(destroyResults) && locationResults.FirstOrDefault() != null && locationResults.FirstOrDefault().IsPlayArea)
                {
                    //For each card destroyed this way, reveal the top 2 cards of a deck in that card's play area and put 1 card on the top and 1 on the bottom of that deck.
                    Location loc = locationResults.FirstOrDefault();
                    coroutine = RevealResponse(loc);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }

                destroyResults = new List<DestroyCardAction>();
                locationResults = new List<Location>();
                coroutine = SelectAndDestroyCardAndReturnLocation(DecisionMaker, new LinqCardCriteria(IsFaceDownInsight, "face-down insight"), true, destroyResults, locationResults, CustomDecisionMode.Insight, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }

            //You may destroy 1 of your face - down might cards to play or discard top card of a deck in that card's play area.
            List<DestroyCardAction> destroyMight = new List<DestroyCardAction>();
            List<Location> locationMight = new List<Location>();
            coroutine = SelectAndDestroyCardAndReturnLocation(DecisionMaker, new LinqCardCriteria(IsFaceDownMight, "face-down might"), true, destroyMight, locationMight, CustomDecisionMode.Might, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDestroyCard(destroyMight) && locationMight.FirstOrDefault() != null && locationMight.FirstOrDefault().IsPlayArea)
            {
                Location loc = locationMight.FirstOrDefault();
                List<SelectLocationDecision> deckResults = new List<SelectLocationDecision>();
                decisionMode = CustomDecisionMode.PlayOrDiscardTopCard;
                coroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.Custom, (Location L) => L.OwnerTurnTaker == loc.OwnerTurnTaker, deckResults, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                if (DidSelectDeck(deckResults))
                {
                    Location deck = deckResults.First().SelectedLocation.Location;
                    IEnumerable<Function> functionChoices = new Function[2]
                    {
                    new Function(base.HeroTurnTakerController, $"Play the top card of {deck.GetFriendlyName()}", SelectionType.PlayTopCard,() => base.GameController.PlayTopCardOfLocation(this.TurnTakerController, deck, cardSource:GetCardSource())),
                    new Function(base.HeroTurnTakerController, $"Discard the top card of {deck.GetFriendlyName()}", SelectionType.DiscardCard,() => base.GameController.DiscardTopCard(deck,null,responsibleTurnTaker: this.TurnTaker, cardSource:GetCardSource()))
                    };

                    SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, false, null, null, null, GetCardSource());
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
            }
        }

        private IEnumerator RevealResponse(Location loc)
        {
            //Based on Dark Visionary
            List<SelectLocationDecision> deckResults = new List<SelectLocationDecision>();
            IEnumerator coroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.RevealCardsFromDeck, (Location L) => L.OwnerTurnTaker == loc.OwnerTurnTaker, deckResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidSelectDeck(deckResults))
            {
                Location deck = deckResults.First().SelectedLocation.Location;
                List<Card> revealed = new List<Card>();
                coroutine = RevealCardsFromTopOfDeck_PutOnTopAndOnBottom(DecisionMaker, DecisionMaker, deck, storedResults: revealed);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                List<Location> list = new List<Location>();
                list.Add(deck.OwnerTurnTaker.Revealed);
                coroutine = base.GameController.CleanupCardsAtLocations(this.TurnTakerController, list, deck, cardsInList: revealed, cardSource: GetCardSource());
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

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decisionMode == CustomDecisionMode.PlayOrDiscardTopCard)
            {
                string text = $"a deck to play or discard the top card.";
                CustomDecisionText result = new CustomDecisionText(
                $"Select {text}",
                $"The other heroes are choosing {text}",
                $"Vote for {text}",
                $"{text}"
                );
                return result;
            }
            return base.GetCustomDecisionText(decision);
        }
    }
}

