using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.EldrenwoodVillage
{
    public class JohnHadawayCardController : AfflictedCardController
    {
        public JohnHadawayCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, one hero target regains 1 HP."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker && CanActivateEffect(base.TurnTakerController, QuaintKey), (PhaseChangeAction pca) => base.GameController.SelectAndGainHP(DecisionMaker, 1, additionalCriteria: (Card c) => c.IsHero, requiredDecisions: 1, cardSource: GetCardSource()), TriggerType.GainHP);
        }

        public override IEnumerator SlainInHumanFormResponse()
        {
            // "... the players collectively discard {H} cards."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            while (GetNumberOfCardsDiscarded(discards) < H && base.GameController.FindTurnTakersWhere((TurnTaker tt) => base.GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource()) && tt.ToHero().HasCardsInHand).Count() > 0)
            {
                IEnumerator discardCoroutine = base.GameController.SelectHeroToDiscardCards(DecisionMaker, 0, null, storedResultsDiscard: discards, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
            }
        }
    }
}
