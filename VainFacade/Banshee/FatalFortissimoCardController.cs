using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class FatalFortissimoCardController:CardController
	{
		public FatalFortissimoCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        public override IEnumerator Play()
        {
            //Select up to 3 targets. Increase the next damage dealt to those targets by 2.
            List<SelectCardsDecision> results = new List<SelectCardsDecision>();
            IEnumerator coroutine = base.GameController.SelectCardsAndDoAction(DecisionMaker, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsTarget), SelectionType.IncreaseDamageTaken, IncreaseResponse, 3, false, 0, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator IncreaseResponse(Card card)
        {
            IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(2);
            effect.NumberOfUses = 1;
            effect.TargetCriteria.IsSpecificCard = card;
            effect.UntilTargetLeavesPlay(card);
            IEnumerator increase = AddStatusEffect(effect);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(increase);
            }
            else
            {
                base.GameController.ExhaustCoroutine(increase);
            }
        }
    }
}

