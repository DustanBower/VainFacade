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
    public class UndyingEssenceCardController : BaronessUtilityCardController
    {
        public UndyingEssenceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override bool CanOrderAffectOutcome(GameAction action)
        {
            if (action is DealDamageAction)
            {
                return (action as DealDamageAction).Target == base.CharacterCard;
            }
            return false;
        }

        private bool GainedHPThisTurn()
        {
            IEnumerable<GainHPJournalEntry> list = Journal.GainHPEntriesThisTurn().Where((GainHPJournalEntry e) => e.TargetCard == base.CharacterCard && e.Amount > 0);
            return list.Any();
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {TheBaroness} would be dealt 4 or more damage from a single source, reduce that damage by 2."
            AddReduceDamageTrigger((DealDamageAction dda) => dda.Amount >= 4, 2, null, (Card c) => c == base.CharacterCard);
            // "At the end of the villain turn, if {TheBaroness} gained HP this turn, destroy 1 Blood card. If a card is destroyed this way, {TheBaroness} regains 1 HP."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyHealResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.GainHP });
        }

        private IEnumerator DestroyHealResponse(PhaseChangeAction pca)
        {
            // "...  if {TheBaroness} gained HP this turn, ..."
            if (GainedHPThisTurn())
            {
                // "... destroy 1 Blood card."
                List<DestroyCardAction> results = new List<DestroyCardAction>();
                IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, BloodCard(), false, storedResultsAction: results, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
                // "If a card is destroyed this way, {TheBaroness} regains 1 HP."
                if (DidDestroyCards(results))
                {
                    IEnumerator healCoroutine = base.GameController.GainHP(base.CharacterCard, 1, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(healCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(healCoroutine);
                    }
                }
            }
        }
    }
}
