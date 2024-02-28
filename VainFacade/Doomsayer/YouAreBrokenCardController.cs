using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class YouAreBrokenCardController:ProclamationCardController
	{
		public YouAreBrokenCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHeroWithFewestCards(true).Condition = () => !this.Card.IsInPlayAndHasGameText;
		}

        private string HasShownHealBlockMessage = "HasShownHealBlockMessage";

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            //Play this card next to the hero whose player has the fewest cards in their hand.
            List<TurnTaker> results = new List<TurnTaker>();
            IEnumerator coroutine = base.GameController.DetermineTurnTakersWithMostOrFewest(false, 1, 1, (TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame, (TurnTaker tt) => tt.ToHero().NumberOfCardsInHand, SelectionType.Custom, results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (results.FirstOrDefault() != null)
            {
                TurnTaker hero = results.FirstOrDefault();
                List<Card> selected = new List<Card>();
                coroutine = FindCharacterCard(hero, SelectionType.MoveCardNextToCard, selected);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                if (selected.FirstOrDefault() != null)
                {
                    storedResults.Add(new MoveCardDestination(selected.FirstOrDefault().NextToLocation));
                }
            }
        }

        public override void AddTriggers()
        {
            //Targets in this play area cannot regain hp or have their hp increased.
            AddTrigger<GainHPAction>((GainHPAction hp) => hp.HpGainer.Location.HighestRecursiveLocation == this.Card.Location.HighestRecursiveLocation, HealResponse,new TriggerType[2] { TriggerType.WouldGainHP, TriggerType.CancelAction }, TriggerTiming.Before);
            AddTrigger<SetHPAction>((SetHPAction hp) => hp.HpGainer.Location.HighestRecursiveLocation == this.Card.Location.HighestRecursiveLocation && hp.Amount > hp.HpGainer.HitPoints, HealResponse, new TriggerType[2] { TriggerType.WouldGainHP, TriggerType.CancelAction }, TriggerTiming.Before);

            base.AddTriggers();
        }

        private IEnumerator HealResponse(GameAction g)
        {
            IEnumerator coroutine = CancelAction(g, showOutput: false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            if (g.CardSource != null && !HasBeenSetToTrueThisTurn(HasShownHealBlockMessage))
            {
                SetCardPropertyToTrueIfRealAction(HasShownHealBlockMessage);
                coroutine = base.GameController.SendMessageAction($"{base.Card.Title} prevents targets in {this.Card.Location.HighestRecursiveLocation.GetFriendlyName()} from gaining HP.", Priority.Medium, GetCardSource(), null, showCardSource: true);
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
            return new CustomDecisionText(
                $"Choose the hero with the fewest cards in hand.",
                $"The other heroes are choosing the hero with the fewest cards in hand.",
                $"Vote for the hero with the fewest cards in hand.",
                $"the hero with the fewest cards in hand."
            );
        }
    }
}

