using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class ReapersRestTestCardController:CardController
	{
		public ReapersRestTestCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            //At the end of each player's turn, if that player did not play a card, did not use a power, or did not draw a card this turn, select a target. Increase the next damage dealt to that target by 1.
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsPlayer, IncreaseResponse,new TriggerType[] { TriggerType.IncreaseDamage, TriggerType.CreateStatusEffect });
        }

        private IEnumerator IncreaseResponse(PhaseChangeAction pca)
        {
            List<SelectCardDecision> results = new List<SelectCardDecision>();
            IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.IncreaseDamageTaken, new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText), results, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            Card card = GetSelectedCard(results);
            if (card != null)
            {
                IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(1);
                effect.NumberOfUses = 1;
                effect.TargetCriteria.IsSpecificCard = card;
                effect.UntilTargetLeavesPlay(card);
                coroutine = AddStatusEffect(effect);
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

