using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.BastionCity
{
	public class MrGoodmanCardController:BastionCityCardController
	{
		public MrGoodmanCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowSpecialString(BuildDeckSpecialString).Condition = () => DecksWithCardsUnder().Count() > 0;
		}

        private string BuildDeckSpecialString()
        {
            if (DecksWithCardsUnder().Count() > 0)
            {
                string prefix = $"Decks with cards under {this.Card.Title}: ";
                string list = DecksWithCardsUnder().Select((Location L) => L.GetFriendlyName()).ToCommaList();
                return prefix + list;
            }
            return $"There are no cards under {this.Card.Title}.";
        }

        public override void AddTriggers()
        {
            //At the end of the environment turn, select a deck with no cards under this card. Put the top card of that deck under this card, then put the top card of that deck into play.
            //Then, this card deals each target from a deck with a card under this one 2 psychic damage each.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.MoveCard, TriggerType.DealDamage });
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<SelectLocationDecision> results = new List<SelectLocationDecision>();
            IEnumerator coroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.MoveCardToUnderCard, (Location L) => base.GameController.IsLocationVisibleToSource(L, GetCardSource()) && !DecksWithCardsUnder().Contains(L), results, false, $"All decks have a card under {this.Card.Title}", GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidSelectLocation(results))
            {
                Location deck = GetSelectedLocation(results);
                if (deck.HasCards)
                {
                    coroutine = base.GameController.MoveCard(this.TurnTakerController, deck.TopCard, this.Card.UnderLocation, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    coroutine = base.GameController.PlayTopCardOfLocation(this.TurnTakerController, deck, isPutIntoPlay: true, cardSource: GetCardSource());
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

            coroutine = DealDamage(this.Card, (Card c) => DecksWithCardsUnder().Contains(GetNativeDeck(c)), 2, DamageType.Psychic);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerable<Location> DecksWithCardsUnder()
        {
            return this.Card.UnderLocation.Cards.Select((Card c) => GetNativeDeck(c)).Distinct();
        }
    }
}

