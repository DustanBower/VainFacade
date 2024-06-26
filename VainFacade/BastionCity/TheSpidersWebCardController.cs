using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.BastionCity
{
	public class TheSpidersWebCardController : BastionCityCardController
	{
		public TheSpidersWebCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowVillainTargetWithHighestHP();
			base.SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => IsMachination(c) && c != this.Card, "other machination"));
		}

		public override void AddTriggers()
		{
            //At the end of the environment turn, increase the next damage dealt by the villain target with the highest HP by 2.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, TriggerType.CreateStatusEffect);

            //At the start of the environment turn, destroy a machination other than this card. If you do not, destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, StartOfTurnResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.DestroySelf });
        }

		private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
		{
			List<Card> highest = new List<Card>();
			IEnumerator coroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => IsVillainTarget(c), highest, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

			Card card = highest.FirstOrDefault();
			if (card != null)
			{
				IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(2);
				effect.NumberOfUses = 1;
				effect.UntilTargetLeavesPlay(card);
				effect.SourceCriteria.IsSpecificCard = card;
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

		private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
		{
			List<DestroyCardAction> results = new List<DestroyCardAction>();
			IEnumerator coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsMachination(c) && c != this.Card, "machination"), false, results, this.Card, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (!DidDestroyCard(results))
            {
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
}

