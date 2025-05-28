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
	public class JustBusinessCardController:WebCardController
	{
		public JustBusinessCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
		}

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();

            if (!this.Card.IsFlipped)
            {
                //At the end of the environment turn, for every 3 HP this card possesses, a player discards a card.
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, EndOfTurnResponse, TriggerType.DiscardCard, additionalCriteria: (PhaseChangeAction pca) => this.Card.HitPoints >= 3));
            }
            else
            {
                //At the end of the environment turn, each player may play up to 2 cards. If a card entered play this way, destroy this card.
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, EndOfTurnFlippedResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.DestroySelf }));
            }
        }

        private int n;

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            //...for every 3 HP this card possesses, a player discards a card.
            int i = 3;
            n = 1;
            while (this.Card.HitPoints.HasValue && i <= this.Card.HitPoints)
            {
                IEnumerator coroutine = base.GameController.SelectHeroToDiscardCard(DecisionMaker, false, false, selectionType: SelectionType.Custom, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                i += 3;
                n += 1;
            }
        }

        private IEnumerator EndOfTurnFlippedResponse(PhaseChangeAction pca)
        {
            //...each player may play up to 2 cards.
            List<PlayCardAction> results = new List<PlayCardAction>();
            Func<TurnTaker, IEnumerator> action = (TurnTaker tt) => base.GameController.SelectAndPlayCardsFromHand(FindHeroTurnTakerController(tt.ToHero()), 2, false, 0, storedResults: results, cardSource: GetCardSource());
            IEnumerator coroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame), SelectionType.PlayCard, action, requiredDecisions: 0, allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //If a card entered play this way, destroy this card.
            if (DidPlayCards(results))
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

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            string s;
            if (n == 1)
                s = "discard a card";

            else if (n == 2)
            {
                s = $"discard a second card";
            }
            else if (n == 3)
            {
                s = $"discard a third card";
            }
            else if (n == 4)
            {
                s = $"discard a fourth card";
            }
            else if (n == 5)
            {
                s = $"discard a fifth card";
            }
            else
            {
                return base.GetCustomDecisionText(decision);
            }

            return new CustomDecisionText(
            $"Select a player to {s}.",
            $"The players are selecting a player to {s}.",
            $"Vote for a player to {s}.",
            $"a player to {s}."
            );
        }
    }
}

