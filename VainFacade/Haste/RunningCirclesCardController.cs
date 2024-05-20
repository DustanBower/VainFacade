using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Haste
{
	public class RunningCirclesCardController:HasteUtilityCardController
	{
		public RunningCirclesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //When this card enters play, add 3 tokens to your speed pool.
            return AddSpeedTokens(3);
        }

        public override void AddTriggers()
        {
            //When you discard a card, add a token to your speed pool.
            AddTrigger<DiscardCardAction>((DiscardCardAction dc) => dc.ResponsibleTurnTaker == this.TurnTaker, (DiscardCardAction dc) => AddSpeedTokens(1), TriggerType.AddTokensToPool, TriggerTiming.After);

            //At the end of your turn, you may discard a card and draw a card.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DrawCard });
        }

        //Copied from Erratic Form
        private IEnumerator EndOfTurnResponse(PhaseChangeAction p)
        {
            List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
            IEnumerator coroutine = base.GameController.SelectAndDiscardCard(DecisionMaker, optional: true, null, storedResults, SelectionType.DiscardCard, null, null, ignoreBattleZone: false, null, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            if (DidDiscardCards(storedResults))
            {
                coroutine = DrawCard(base.HeroTurnTaker, optional: true);
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

