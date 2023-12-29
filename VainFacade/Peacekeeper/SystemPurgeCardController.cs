using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class SystemPurgeCardController:PeacekeeperCardUtilities
	{
		public SystemPurgeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //Draw a card.
            IEnumerator coroutine = DrawCard();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Destroy a serum or symptom.
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsSerum(c) || IsSymptom(c), "serum or symptom"), false, results, this.Card, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //If a card was destroyed this way, reveal the top 2 cards of your deck. Put one into your hand and one on the bottom of the deck.
            if (DidDestroyCard(results))
            {
                coroutine = RevealCardsFromDeckToMoveToOrderedDestinations(this.TurnTakerController, this.TurnTaker.Deck, new MoveCardDestination[] { new MoveCardDestination(this.HeroTurnTaker.Hand), new MoveCardDestination(this.TurnTaker.Deck, true) }, numberOfCardsToReveal: 2);
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
}

