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
	public class PowerPlayCardController:BastionCityCardController
	{
		public PowerPlayCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNumberOfCardsAtLocation(this.TurnTaker.Deck, MachinationCriteria());
		}

        public override void AddTriggers()
        {
            //At the start of the environment turn, reveal the top {H} cards of the environment deck. Put any revealed machinations into play. Discard the other revealed cards. If a card enters play this way, destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, StartOfTurnResponse, new TriggerType[] {TriggerType.RevealCard, TriggerType.PutIntoPlay, TriggerType.DiscardCard, TriggerType.DestroySelf });
        }

        private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
        {
            List<Card> played = new List<Card>();
            IEnumerator coroutine = RevealCards_PutSomeIntoPlay_DiscardRemaining(this.TurnTakerController, this.TurnTaker.Deck, base.H, MachinationCriteria(), playedCards: played);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (played.Any())
            {
                coroutine = DestroyThisCardResponse(null);
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

