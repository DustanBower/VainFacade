using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Blitz
{
    public class UnnecessaryRoughnessCardController : PlaybookCardController
    {
        public UnnecessaryRoughnessCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, destroy 1 environment card and {H - 2} hero Ongoing and/or Equipment cards. For each card destroyed this way, increase the next lightning damage dealt by {BlitzCharacter} by 1."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyCardsToChargeResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.CreateStatusEffect });
        }

        private IEnumerator DestroyCardsToChargeResponse(PhaseChangeAction pca)
        {
            // "... destroy 1 environment card..."
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            IEnumerator destroyEnvCoroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), false, results, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyEnvCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyEnvCoroutine);
            }
            // "... and {H - 2} hero Ongoing and/or Equipment cards."
            IEnumerator destroyHeroCoroutine = base.GameController.SelectAndDestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.IsHero && (IsOngoing(c) || IsEquipment(c)), "hero Ongoing or Equipment"), H - 2, requiredDecisions: H - 2, storedResultsAction: results, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyHeroCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyHeroCoroutine);
            }
            // "For each card destroyed this way, increase the next lightning damage dealt by {BlitzCharacter} by 1."
            IEnumerator increaseCoroutine = IncreaseNextLightningDamage(GetNumberOfCardsDestroyed(results), GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(increaseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(increaseCoroutine);
            }
        }
    }
}
