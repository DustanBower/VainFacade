using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Peacekeeper
{
	public class YTechCustomSerum4319BCardController:SerumCardController
	{
		public YTechCustomSerum4319BCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override void AddTriggers()
        {
            //{Peacekeeper} is immune to toxic damage.
            AddImmuneToDamageTrigger((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DamageType == DamageType.Toxic);

            //At the end of your turn, return a symptom in play to your hand.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, (PhaseChangeAction pca) => base.GameController.SelectAndMoveCard(DecisionMaker, (Card c) => IsSymptom(c) && c.IsInPlayAndHasGameText, this.HeroTurnTaker.Hand, cardSource: GetCardSource()), TriggerType.MoveCard);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Destroy this card.
            IEnumerator coroutine = DestroyThisCardResponse(null);
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

