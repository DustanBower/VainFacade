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
    public class DeathsCallCardController : BaronessUtilityCardController
    {
        public DeathsCallCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show each hero's resonance
            ShowResonancePerHero();
        }

        public override IEnumerator Play()
        {
            //Log.Debug("DeathsCallCardController.Play: ");
            // "Each hero destroys one of their non-character cards for each point of resonance they have."
            List<DestroyCardAction> nonBloodDestroyed = new List<DestroyCardAction>();
            List<TurnTaker> heroes = base.GameController.FindTurnTakersWhere((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame).ToList();
            foreach(TurnTaker tt in heroes)
            {
                Log.Debug("DeathsCallCardController.Play: hero: " + tt.Name);
                Log.Debug("DeathsCallCardController.Play: resonance: " + Resonance(tt));
            }
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame && Resonance(tt) > 0 && base.GameController.FindCardsWhere((Card c) => c.Owner == tt && !c.IsCharacter && c.IsInPlay && (!c.IsFaceDownNonCharacter || !c.Location.IsPlayAreaOf(base.TurnTaker))).Count() > 0), SelectionType.DestroyCard, (TurnTaker tt) => DestroyAccordingToBlood(tt, nonBloodDestroyed), allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            // "For each card destroyed this way, destroy 1 Blood card owned by that hero."
            IEnumerator selectCoroutine2 = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame && Resonance(tt) > 0 && DestroyedCount(tt, nonBloodDestroyed) > 0), SelectionType.DestroyCard, (TurnTaker tt) => base.GameController.SelectAndDestroyCards(base.GameController.FindHeroTurnTakerController(tt.ToHero()), new LinqCardCriteria((Card c) => BloodCard().Criteria(c) && c.Owner == tt, "Blood", singular: "card belonging to " + tt.Name, plural: "cards belonging to " + tt.Name), DestroyedCount(tt, nonBloodDestroyed), requiredDecisions: DestroyedCount(tt, nonBloodDestroyed), allowAutoDecide: true, responsibleCard: base.Card, cardSource: GetCardSource()), allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine2);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine2);
            }
            // "Then {TheBaroness} deals each hero target that did not destroy a card in this way {H - 1} infernal damage."
            IEnumerator damageCoroutine = DealDamage(base.CharacterCard, (Card c) => c.IsHero && DestroyedCount(c.Owner, nonBloodDestroyed) <= 0, H - 1, DamageType.Infernal);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
        }

        private IEnumerator DestroyAccordingToBlood(TurnTaker tt, List<DestroyCardAction> results)
        {
            // "... destroys one of their non-character cards for each point of resonance they have."
            LinqCardCriteria theirNonBloodNonCharacter = new LinqCardCriteria((Card c) => c.Owner == tt && !c.IsCharacter && c.IsInPlay && (!c.IsFaceDownNonCharacter || !c.Location.IsPlayAreaOf(base.TurnTaker)));
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(base.GameController.FindHeroTurnTakerController(tt.ToHero()), theirNonBloodNonCharacter, null, dynamicNumberOfCards: () => Resonance(tt), storedResultsAction: results, allowAutoDecide: Resonance(tt) >= base.GameController.FindCardsWhere(theirNonBloodNonCharacter, visibleToCard: GetCardSource()).Count(), responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
        }

        private int DestroyedCount(TurnTaker tt, List<DestroyCardAction> stored)
        {
            return stored.Where((DestroyCardAction dca) => dca.WasCardDestroyed && dca.CardToDestroy != null && dca.CardToDestroy.Card.Owner == tt).Count();
        }
    }
}
