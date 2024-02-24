using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class YouCantChangeCardController:ProclamationCardController
	{
		public YouCantChangeCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
		}

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            //Play this card in a hero play area.
            List<SelectTurnTakerDecision> results = new List<SelectTurnTakerDecision>();
            IEnumerator coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.MoveCardToPlayArea, results, additionalCriteria: (TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidSelectTurnTaker(results))
            {
                TurnTaker hero = GetSelectedTurnTaker(results);
                storedResults.Add(new MoveCardDestination(hero.PlayArea));
            }
        }

        public override void AddTriggers()
        {
            //When a power is used on a non-character card in that play area, destroy that card.
            AddTrigger<UsePowerAction>((UsePowerAction up) => up.Power.CardSource != null && !up.Power.CardSource.Card.IsCharacter && up.Power.CardSource.Card.Location.HighestRecursiveLocation == this.Card.Location.HighestRecursiveLocation, DestroyResponse, TriggerType.DestroyCard, TriggerTiming.After);
        }

        private IEnumerator DestroyResponse(UsePowerAction up)
        {
            Card powerCard = up.Power.CardSource.Card;
            IEnumerator coroutine = base.GameController.DestroyCard(DecisionMaker, powerCard, cardSource: GetCardSource());
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

