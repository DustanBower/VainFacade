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
    public class TwistedPerceptionsCardController : CardController
    {
        public TwistedPerceptionsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of times this card has redirected damage
            SpecialStringMaker.ShowIfElseSpecialString(() => RedirectCount() > 0, () => base.Card.Title + " has redirected damage " + RedirectCount().ToString() + " times since entering play.", () => base.Card.Title + " has not redirected damage since entering play.");
        }

        public override bool AllowFastCoroutinesDuringPretend
        {
            get 
            {
                return IsHighestHitPointsUnique((Card c) => c.IsHero);
            } 
        }

        private int RedirectCount()
        {
            if (base.Card.IsInPlayAndHasGameText)
            {
                PlayCardJournalEntry entered = base.GameController.Game.Journal.QueryJournalEntries((PlayCardJournalEntry pcje) => pcje.CardPlayed == base.Card).LastOrDefault();
                if (entered != null && base.GameController.Game.Journal.GetEntryIndex(entered).HasValue)
                {
                    int enteredIndex = base.GameController.Game.Journal.GetEntryIndex(entered).Value;
                    IEnumerable<RedirectDamageJournalEntry> redirects = base.GameController.Game.Journal.QueryJournalEntries((RedirectDamageJournalEntry e) => e.CardSource == base.Card && base.GameController.Game.Journal.GetEntryIndex(e).HasValueGreaterThan(enteredIndex));
                    return redirects.Count();
                }
            }
            return 0;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a hero target would deal damage, increase that damage by 3 and redirect it to the hero target with the highest HP."
            // "Reduce damage dealt this way by 1 for each time damage has been redirected this way."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card.IsHero && dda.DamageSource.Card.IsTarget, 3);
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card.IsHero && dda.DamageSource.Card.IsTarget, RedirectReduceResponse, TriggerType.RedirectDamage, TriggerTiming.Before);
            // "At the start of the villain turn, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => base.GameController.DestroyCard(DecisionMaker, base.Card, responsibleCard: base.Card, cardSource: GetCardSource()), TriggerType.DestroySelf);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, play the top card of each hero deck."
            IEnumerator playCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame), SelectionType.PlayTopCard, (TurnTaker tt) => base.GameController.PlayTopCard(DecisionMaker, FindTurnTakerController(tt), responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource()), allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }

        private IEnumerator RedirectReduceResponse(DealDamageAction dda)
        {
            // "... redirect it to the hero target with the highest HP."
            List<Card> results = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => c.IsHero, results, dda, evenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            Card target = results.FirstOrDefault();
            if (target != null)
            {
                IEnumerator redirectCoroutine = base.GameController.RedirectDamage(dda, target, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(redirectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(redirectCoroutine);
                }
            }
            // "Reduce damage dealt this way by 1 for each time damage has been redirected this way."
            IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, (DealDamageAction a) => RedirectCount(), null, cardSource: GetCardSource());
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
