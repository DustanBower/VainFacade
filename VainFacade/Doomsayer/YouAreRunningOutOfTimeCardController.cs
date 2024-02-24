using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class YouAreRunningOutOfTimeCardController:ProclamationCardController
	{
		public YouAreRunningOutOfTimeCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstCardPlay);
		}

        private string FirstCardPlay = "FirstCardPlay";

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
            //The first time each turn that hero plays a card, put a random card from their hand into play.
            AddTrigger<PlayCardAction>((PlayCardAction pca) => !IsPropertyTrue(FirstCardPlay) && this.Card.Location.IsHeroPlayAreaRecursive && pca.ResponsibleTurnTaker == this.Card.Location.HighestRecursiveLocation.OwnerTurnTaker && !pca.IsPutIntoPlay, PlayRandom, TriggerType.PlayCard, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction g) => ResetFlagAfterLeavesPlay(FirstCardPlay), TriggerType.Hidden);
        }

        private IEnumerator PlayRandom(PlayCardAction pca)
        {
            SetCardProperty(FirstCardPlay, true);
            HeroTurnTaker hero = this.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.ToHero();
            IEnumerator coroutine;
            if (hero.HasCardsInHand)
            {
                Card random = hero.Hand.Cards.Take(1).FirstOrDefault();
                if (random != null)
                {
                    coroutine = base.GameController.PlayCard(this.TurnTakerController, random, true, cardSource: GetCardSource());
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
            else
            {
                coroutine = base.GameController.SendMessageAction($"{hero.Name} has no cards in hand, so {this.Card.Title} cannot play a random card", Priority.Low, GetCardSource());
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