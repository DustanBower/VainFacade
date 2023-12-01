using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class TensionAndResolutionCardController:CardController
	{
		public TensionAndResolutionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstPlayKey);
		}

        private string FirstPlayKey = "FirstPlayKey";

        private int? PlayActionIdentifier;

        public override void AddTriggers()
        {
            //The first time each non-hero turn you play a card with a name not in play, you may draw a card and use a power.
            AddTrigger<PlayCardAction>((PlayCardAction pc) => !pc.IsPutIntoPlay && !PlayActionIdentifier.HasValue && pc.ResponsibleTurnTaker == this.TurnTaker && !base.Game.ActiveTurnTaker.IsHero && !IsPropertyTrue(FirstPlayKey) && !FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.Title == pc.CardToPlay.Title).Any(), LogPlayResponse, TriggerType.Hidden, TriggerTiming.Before);
            AddTrigger<PlayCardAction>((PlayCardAction pc) => PlayActionIdentifier.HasValue && PlayActionIdentifier == pc.ActionIdentifier, DrawAndPowerResponse, new TriggerType[2] { TriggerType.DrawCard, TriggerType.UsePower }, TriggerTiming.After);
            ResetFlagAfterLeavesPlay(FirstPlayKey);
        }

        private IEnumerator LogPlayResponse(PlayCardAction pc)
        {
            PlayActionIdentifier = pc.ActionIdentifier;
            SetCardProperty(FirstPlayKey, true);
            yield return null;
        }

        private IEnumerator DrawAndPowerResponse(PlayCardAction pc)
        {
            PlayActionIdentifier = null;
            IEnumerator coroutine = DrawCard(this.HeroTurnTaker, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.GameController.SelectAndUsePower(DecisionMaker, cardSource: GetCardSource());
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

