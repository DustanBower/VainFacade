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
	public class InstantAnalysisCardController:HasteUtilityCardController
	{
		public InstantAnalysisCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
			base.SpecialStringMaker.ShowTokenPool(SpeedPool);
		}

        public override IEnumerator Play()
        {
            //When this card enters play, add 2 tokens to your speed pool.
            return AddSpeedTokens(2);
        }

        public override void AddTriggers()
        {
            //At the end of your turn, you may remove any number of tokens from your speed pool. X players draw a card, where X = the number of tokens removed this way.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.ModifyTokens, TriggerType.DrawCard });
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<int?> results = new List<int?>();
            IEnumerator coroutine = RemoveAnyNumberOfSpeedTokens(results);
            //IEnumerator coroutine = RemoveAnyNumberOfTokensFromTokenPool(SpeedPool, results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (results.FirstOrDefault().HasValue && results.FirstOrDefault().Value > 0)
            {
                int num = results.FirstOrDefault().Value;
                int numHeroes = FindTurnTakersWhere((TurnTaker tt) => tt.IsPlayer && base.GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource())).Count();
                coroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsPlayer), SelectionType.DrawCard, (TurnTaker tt) => DrawCard(tt.ToHero()), num, false, num, allowAutoDecide: num >= numHeroes, cardSource: GetCardSource());
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

