using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class SinisterLaboratoryCardController : AreaCardController
    {
        public SinisterLaboratoryCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show current location of Test Subjects
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria(FindCard(TestSubjectsIdentifier)));
        }

        public static readonly string TestSubjectsIdentifier = "TestSubjects";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase toxic and psychic damage dealt by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageType == DamageType.Toxic || dda.DamageType == DamageType.Psychic, (DealDamageAction dda) => 1);
            // "At the end of the environment turn, reveal the top card of the environment deck. If that card is {TestSubjects}, put it into play, otherwise discard it and deal each hero target 1 toxic damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, PlayOrDiscardDamageResponse, TriggerType.RevealCard);
        }

        private IEnumerator PlayOrDiscardDamageResponse(PhaseChangeAction pca)
        {
            // "... reveal the top card of the environment deck. If that card is {TestSubjects}, put it into play, otherwise discard it..."
            List<Card> revealed = new List<Card>();
            IEnumerator checkCoroutine = RevealCards_PutSomeIntoPlay_DiscardRemaining(base.DecisionMaker, base.TurnTaker.Deck, 1, new LinqCardCriteria(FindCard(TestSubjectsIdentifier)), revealedCards: revealed);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(checkCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(checkCoroutine);
            }
            Card result = revealed.FirstOrDefault();
            if (result != null && result != FindCard(TestSubjectsIdentifier))
            {
                // "... and deal each hero target 1 toxic damage."
                IEnumerator damageCoroutine = DealDamage(base.Card, (Card c) => c.IsHero, 1, DamageType.Toxic);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
        }
    }
}
