using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class DreamThiefCardController:CardController
	{
		public DreamThiefCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //Discard the top 2 cards of your deck, the villain deck, and the environment deck. 
            List<MoveCardAction> results = new List<MoveCardAction>();
            IEnumerator coroutine = base.GameController.DiscardTopCards(DecisionMaker, this.TurnTaker.Deck, 2, results, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            List<SelectLocationDecision> locationResults = new List<SelectLocationDecision>();
            coroutine = FindVillainDeck(DecisionMaker, SelectionType.DiscardFromDeck, locationResults, (Location L) => true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            Location deck = GetSelectedLocation(locationResults);

            if (deck != null)
            {
                coroutine = base.GameController.DiscardTopCards(DecisionMaker, deck, 2, results, responsibleTurnTaker: this.TurnTaker, cardSource:GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }

            coroutine = base.GameController.DiscardTopCards(DecisionMaker, FindEnvironment().TurnTaker.Deck, 2, results, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //You may put a card discarded this way into play from the trash.
            List<SelectCardDecision> cardResults = new List<SelectCardDecision>();
            Console.WriteLine($"Cards discarded for Dream Thief: {results.Select((MoveCardAction mc) => mc.CardToMove.Title).ToCommaList()}");
            Console.WriteLine($"Locations: {results.Select((MoveCardAction mc) => mc.CardToMove.Location.GetFriendlyName()).ToCommaList()}");
            Console.WriteLine($"IsTrash: {results.Select((MoveCardAction mc) => mc.CardToMove.Location.IsTrash).ToCommaList()}");
            Console.WriteLine($"IsSubTrash: {results.Select((MoveCardAction mc) => mc.CardToMove.Location.IsSubTrash).ToCommaList()}");
            coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.PutIntoPlay, results.Select((MoveCardAction mc) => mc.CardToMove).Where((Card c) => c.Location.IsTrash), cardResults,true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            Card card = GetSelectedCard(cardResults);
            if (card != null)
            {
                coroutine = base.GameController.PlayCard(DecisionMaker, card, true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }

            //You may draw or play a card.
            coroutine = DrawACardOrPlayACard(DecisionMaker, true);
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

