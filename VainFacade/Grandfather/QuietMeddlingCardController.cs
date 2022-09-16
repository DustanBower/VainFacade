using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class QuietMeddlingCardController : CardController
    {
        public QuietMeddlingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {Grandfather} would deal a hero target energy damage, that target's player may discard up to X cards from the top of their deck where X = the amount of damage. Reduce that damage by 1 for each card discarded this way."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card == base.CharacterCard && dda.DamageType == DamageType.Energy && dda.Target.IsHero && dda.Target.Owner.IsHero, DiscardCardsReduceDamageResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.ReduceDamage, TriggerType.WouldBeDealtDamage }, TriggerTiming.Before, orderMatters: true);
            // "At the end of the villain turn, {Grandfather} deals each hero 2 energy damage."
            AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.CharacterCard, (Card c) => c.IsHeroCharacterCard, TargetType.All, 2, DamageType.Energy);
        }

        private IEnumerator DiscardCardsReduceDamageResponse(DealDamageAction dda)
        {
            // "... that target's player may discard up to X cards from the top of their deck where X = the amount of damage."
            List<MoveCardAction> discards = new List<MoveCardAction>();
            List<SelectNumberDecision> numberDecisions = new List<SelectNumberDecision>();
            IEnumerator decideCoroutine = base.GameController.SelectNumber(FindHeroTurnTakerController(dda.Target.Owner.ToHero()), SelectionType.DiscardFromDeck, 0, dda.Amount, storedResults: numberDecisions, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(decideCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(decideCoroutine);
            }
            SelectNumberDecision choice = numberDecisions.FirstOrDefault();
            if (choice != null && choice.SelectedNumber.HasValue)
            {
                int toDiscard = choice.SelectedNumber.Value;
                IEnumerator discardCoroutine = DiscardCardsFromTopOfDeck(FindTurnTakerController(dda.Target.Owner), toDiscard, storedResults: discards, showMessage: true, responsibleTurnTaker: dda.Target.Owner);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
            }
            // "Reduce that damage by 1 for each card discarded this way."
            int moved = GetNumberOfCardsMoved(discards);
            IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, moved, null, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(reduceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(reduceCoroutine);
            }
        }
    }
}
