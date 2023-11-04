using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Push
{
	public class AnchorCardController:PushCardControllerUtilities
	{
		public AnchorCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowListOfCardsAtLocation(this.HeroTurnTaker.Hand, AlterationCriteria()).Condition = () => !this.Card.IsInPlayAndHasGameText;
		}

        private string playingAlterations = "PlayingAlterations";

        public override void AddTriggers()
        {
            //Whenever an alteration card is in your hand, put it into play.
            AddTrigger<GameAction>((GameAction ga) => !(ga is MakeDecisionAction) && !IsPropertyTrue(playingAlterations,(CardPropertiesJournalEntry e) => e.Key == playingAlterations) && this.HeroTurnTaker.Hand.Cards.Any((Card c) => IsAlteration(c)), PlayAlterationResponse, TriggerType.PutIntoPlay, TriggerTiming.After, requireActionSuccess: false);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(playingAlterations), TriggerType.Hidden);
        }

        private IEnumerator PlayAlterationResponse(GameAction a)
        {
            SetCardProperty(playingAlterations, true);
            Console.WriteLine("Is Property True: " + IsPropertyTrue(playingAlterations, (CardPropertiesJournalEntry e) => e.Key == playingAlterations));
            IEnumerator coroutine = SelectAndPlayCardsFromHand(DecisionMaker, 100, false, 100, AlterationCriteria(), true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            SetCardProperty(playingAlterations, false);
        }
    }
}

