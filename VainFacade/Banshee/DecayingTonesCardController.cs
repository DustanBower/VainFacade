using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class DecayingTonesCardController:CardController
	{
		public DecayingTonesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //When a card is destroyed, you may draw a card.
            AddTrigger<DestroyCardAction>((DestroyCardAction dc) => dc.WasCardDestroyed, (DestroyCardAction dc) => DrawCard(this.HeroTurnTaker, true), TriggerType.DrawCard, TriggerTiming.After);

            //At the start of your turn, {Banshee} regains 2 hp. Then destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, StartOfTurnResponse, new TriggerType[2] { TriggerType.GainHP, TriggerType.DestroySelf });
        }

        private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
        {
            IEnumerator coroutine = base.GameController.GainHP(this.CharacterCard, 2, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

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

