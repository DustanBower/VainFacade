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
    public class OlPegSimmonsCardController : AfflictedCardController
    {
        public OlPegSimmonsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, one player draws 2 cards and discards 1 card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker && CanActivateEffect(base.TurnTakerController, QuaintKey), OnePlayerDraw2Discard1Response, new TriggerType[] { TriggerType.DrawCard, TriggerType.DiscardCard });
            // ...
        }

        private IEnumerator OnePlayerDraw2Discard1Response(PhaseChangeAction pca)
        {
            // "... one player draws 2 cards..."
            List<SelectTurnTakerDecision> choices = new List<SelectTurnTakerDecision>();
            IEnumerator drawCoroutine = base.GameController.SelectHeroToDrawCards(DecisionMaker, 2, optionalDrawCards: false, requiredDraws: 2, storedResults: choices, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "... and discards 1 card."
            TurnTaker selectedHero = (from c in choices where c.Completed select c.SelectedTurnTaker).FirstOrDefault();
            IEnumerator discardCoroutine = null;
            if (selectedHero != null)
            {
                discardCoroutine = base.GameController.SelectAndDiscardCard(base.GameController.FindHeroTurnTakerController(selectedHero.ToHero()), responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            }
            else
            {
                discardCoroutine = base.GameController.SelectHeroToDiscardCard(DecisionMaker, optionalDiscardCard: false, cardSource: GetCardSource());
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
        }

        public override IEnumerator SlainInHumanFormResponse()
        {
            // "...destroy a hero Ongoing card."
            yield return base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsHero && c.IsOngoing, "hero Ongoing"), false, responsibleCard: base.Card, cardSource: GetCardSource());
        }
    }
}
