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
    public class OldForestRdCardController : EldrenwoodUtilityCardController
    {
        public OldForestRdCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of target cards in the environment deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.IsTarget, "target"));
            // Show number of Trigger cards in the environment deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.DoKeywordsContain(TriggerKeyword), "Trigger"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the environment turn, reveal the top {H} cards of the environment deck. Put any revealed targets and Triggers into play. Discard the other revealed cards. Then destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, RevealPlayDiscardDestructResponse, new TriggerType[] { TriggerType.RevealCard, TriggerType.PutIntoPlay, TriggerType.DiscardCard, TriggerType.DestroySelf });
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, each target regains 1 HP."
            IEnumerator healCoroutine = base.GameController.GainHP(DecisionMaker, (Card c) => c.IsTarget, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
        }

        private IEnumerator RevealPlayDiscardDestructResponse(PhaseChangeAction pca)
        {
            // "... reveal the top {H} cards of the environment deck. Put any revealed targets and Triggers into play. Discard the other revealed cards."
            IEnumerator checkCoroutine = RevealCards_PutSomeIntoPlay_DiscardRemaining(base.TurnTakerController, base.TurnTaker.Deck, H, new LinqCardCriteria((Card c) => c.IsTarget || c.DoKeywordsContain(TriggerKeyword), "target or Trigger"));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(checkCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(checkCoroutine);
            }
            // "Then destroy this card."
            IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
        }
    }
}
