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
	public class WebCardController: VillainCharacterCardController
    {
		public WebCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
        }

        //public override void AddSideTriggers()
        //{
        //    if (this.Card.IsFlipped)
        //    {
        //        //When this card is destroyed, remove it from the game.
        //        AddSideTrigger(AddWhenDestroyedTrigger((DestroyCardAction dc) => base.GameController.MoveCard(this.TurnTakerController, this.Card, this.TurnTaker.OutOfGame, cardSource: GetCardSource()), TriggerType.RemoveFromGame));
        //    }
        //}

        public override bool CanBeDestroyed
        {
            get
            {
                return false;
            }
        }

        public override IEnumerator DestroyAttempted(DestroyCardAction destroyCard)
        {
            //When this card would be destroyed, flip it instead
            IEnumerator coroutine;
            if (!base.Card.IsFlipped)
            {
                coroutine = base.GameController.RemoveTarget(this.Card, true, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                coroutine = base.GameController.FlipCard(this, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
            else
            {
                coroutine = base.GameController.MoveCard(this.TurnTakerController, this.Card, this.TurnTaker.OutOfGame, cardSource: GetCardSource());
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

