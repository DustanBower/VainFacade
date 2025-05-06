using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
	public class PoliticsAsUsualCardController:WebCardController
	{
		public PoliticsAsUsualCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
		}

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();

            if (!this.Card.IsFlipped)
            {
                //At the end of the environment turn, for every 3 HP this card possesses, play the top card of the environment deck.
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, EndOfTurnResponse, TriggerType.PlayCard, additionalCriteria: (PhaseChangeAction pca) => this.Card.HitPoints >= 3));
            }
            else
            {
                //At the end of the environment turn, each player may discard a card to draw 2 cards. If a card is discarded this way, destroy this card.
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, EndOfTurnFlippedResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.DestroySelf }));
            }
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            //...for every 3 HP this card possesses, play the top card of the environment deck.
            int i = 3;
            while (this.Card.HitPoints.HasValue && i <= this.Card.HitPoints)
            {
                IEnumerator coroutine = PlayTheTopCardOfTheEnvironmentDeckResponse(null);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                i += 3;
            }
        }

        private IEnumerator EndOfTurnFlippedResponse(PhaseChangeAction pca)
        {
            //...each player may discard a card to draw 2 cards.
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            Func<TurnTaker, IEnumerator> action = (TurnTaker tt) => DiscardToDraw2(tt, results);
            IEnumerator coroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame), SelectionType.DiscardCard, action, requiredDecisions: 0, allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //If a card is discarded this way, destroy this card.
            if (DidDiscardCards(results))
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

        private IEnumerator DiscardToDraw2(TurnTaker tt, List<DiscardCardAction> storedResults = null)
        {
            int num = storedResults.Count();
            HeroTurnTakerController httc = FindHeroTurnTakerController(tt.ToHero());
            IEnumerator coroutine = SelectAndDiscardCards(httc, 1, false, requiredDecisions: 0, selectionType: SelectionType.DiscardCard, storedResults: storedResults, responsibleTurnTaker: tt);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDiscardCards(storedResults) && storedResults.Count() > num)
            {
                coroutine = DrawCards(httc, 2);
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

