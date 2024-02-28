using Handelabra.Sentinels.Engine;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class DictatingDestinyCardController:DoomsayerCardUtilities
	{
		public DictatingDestinyCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //When this card enters play, play the top card of each hero deck in turn order. The first {H - 2} times a non-one-shot enters play this way, play the top card of the villain deck.
            //Based on PlayTopCardOfEachDeckInTurnOrder()
            CardSource cardSource = GetCardSource();
            IEnumerable<Location> locations = GameController.GetVisibleLocations((TurnTakerController ttc) => ttc.IsHero, (Location L) => L.IsHero, isDeck: true, isTrash: false, cardSource);
            TurnTakerController[] tts = GameController.FindTurnTakerControllersWhere((TurnTakerController ttc) => ttc.IsHero, ignoreBattleZone: true, cardSource).ToArray();

            int n = 0;
            for (int i = 0; i < tts.Count(); i++)
            {
                if (!Card.IsInPlayAndHasGameText)
                {
                    break;
                }
                TurnTakerController who = tts.ElementAt(i);
                if (who.IsIncapacitatedOrOutOfGame || !locations.Any((Location l) => l.OwnerTurnTaker == who.TurnTaker))
                {
                    continue;
                }
                IEnumerable<Location> decks = locations.Where((Location l) => who.TurnTaker.Decks.Contains(l));
                List<Location> selectedDecks = new List<Location>();
                for (int j = 0; j < decks.Count(); j++)
                {
                    List<SelectLocationDecision> storedLocation = new List<SelectLocationDecision>();
                    IEnumerator coroutine = GameController.SelectADeck(DecisionMaker, SelectionType.PlayTopCard, (Location l) => decks.Contains(l) && !selectedDecks.Contains(l), storedLocation, optional: false, null, GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }
                    Location selectedLocation = GetSelectedLocation(storedLocation);
                    selectedDecks.Add(selectedLocation);
                    List<Card> results = new List<Card>();
                    coroutine = GameController.PlayTopCard(DecisionMaker, who, optional: false, 1, upTo: false, results, null, null, false, this.TurnTaker, playBottomInstead: false, false, showMessage: false, null, selectedLocation, GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }

                    if (results.FirstOrDefault() != null && !results.FirstOrDefault().IsOneShot && n < base.H-2)
                    {
                        n++;
                        coroutine = PlayTheTopCardOfTheVillainDeckWithMessageResponse(null);
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
        }

        public override void AddTriggers()
        {
            //When a villain target would be dealt damage, redirect that damage to the non-villain target with the lowest hp.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => IsVillainTarget(dd.Target), RedirectResponse, TriggerType.RedirectDamage, TriggerTiming.Before);

            //At the end of the villain turn, 1 player may discard 2 cards to destroy this card.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[2] { TriggerType.DiscardCard, TriggerType.DestroySelf });
        }

        private IEnumerator RedirectResponse(DealDamageAction dd)
        {
            List<Card> lowest = new List<Card>();
            IEnumerator coroutine = base.GameController.FindTargetWithLowestHitPoints(1, (Card c) => !IsVillainTarget(c), lowest, null, new DealDamageAction[] { dd }, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            if (lowest.FirstOrDefault() != null)
            {
                Card lowestCard = lowest.FirstOrDefault();
                coroutine = base.GameController.RedirectDamage(dd, lowestCard, cardSource: GetCardSource());
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

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator coroutine = base.GameController.SelectHeroToDiscardCards(DecisionMaker, 2, 2, true, storedResultsDiscard: results, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDiscardCards(results, 2))
            {
                coroutine = DestroyThisCardResponse(null);
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
}

