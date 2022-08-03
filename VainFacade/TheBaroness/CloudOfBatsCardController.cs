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
    public class CloudOfBatsCardController : BaronessUtilityCardController
    {
        public CloudOfBatsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show if Winged Terror is in play
            SpecialStringMaker.ShowIfSpecificCardIsInPlay(TerrorIdentifier);
        }

        private LinqCardCriteria VillainCardNamedBat()
        {
            return new LinqCardCriteria((Card c) => c.Owner == base.TurnTaker && c.Identifier == BatIdentifier, "Bat");
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a Bat is dealt damage, reduce damage dealt to Bats by 1 this turn."
            AddTrigger((DealDamageAction dda) => dda.Target.DoKeywordsContain(BatKeyword) && dda.DidDealDamage, BatsScatterResponse, TriggerType.CreateStatusEffect, TriggerTiming.After);
            // "Blood cards have a maximum HP of 4 and are villain Bats."
            // When a card becomes Blood, assign it a Bat placeholder card from OffToTheSide
            AddTrigger((MoveCardAction mca) => mca.CardToMove.IsFaceDownNonCharacter && mca.CardToMove.IsHero && mca.WasCardMoved && mca.Destination.IsPlayAreaOf(base.TurnTaker) && !mca.Origin.IsPlayAreaOf(base.TurnTaker), (MoveCardAction mca) => MakeBat(mca.CardToMove), TriggerType.Hidden, TriggerTiming.After);
            // When this card leaves play, move all Bat placeholder cards back to OffToTheSide
            AddAfterLeavesPlayAction(() => base.GameController.MoveCards(DecisionMaker, VillainCardNamedBat(), (Card c) => base.TurnTaker.OffToTheSide, cardSource: GetCardSource()));
            // "At the start of the villain turn, each Bat deals each hero target 1 projectile damage. If less than 4 damage is dealt this way, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, BatsSwarmResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroyCard });
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, destroy Winged Terror."
            Card terror = base.TurnTaker.FindCard(TerrorIdentifier);
            if (terror.IsInPlayAndHasGameText)
            {
                IEnumerator destroyCoroutine = base.GameController.DestroyCard(DecisionMaker, terror, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            // "Blood cards have a maximum HP of 4 and are villain Bats."
            // Assign a Bat to each Blood card in play
            IEnumerable<Card> bloodCards = base.GameController.FindCardsWhere(BloodCard(), visibleToCard: GetCardSource());
            foreach (Card c in bloodCards)
            {
                IEnumerator makeCoroutine = MakeBat(c);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(makeCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(makeCoroutine);
                }
            }
        }

        private IEnumerator MakeBat(Card blood)
        {
            // Get a Bat from OffToTheSide and assign it to blood
            Card newBat = TurnTakerControllerWithoutReplacements.TurnTaker.GetAllCards().Where((Card c) => c.Location.IsOffToTheSide).FirstOrDefault();
            if (newBat != null && blood.Location.IsPlayAreaOf(base.TurnTaker) && blood.IsFaceDownNonCharacter)
            {
                IEnumerator moveBatCoroutine = base.GameController.MoveCard(base.TurnTakerController, newBat, blood.NextToLocation, playCardIfMovingToPlayArea: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveBatCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveBatCoroutine);
                }
            }
        }

        private IEnumerator BatsScatterResponse(DealDamageAction dda)
        {
            // "... reduce damage dealt to Bats by 1 this turn."
            ReduceDamageStatusEffect shield = new ReduceDamageStatusEffect(1);
            List<string> keywords = new List<string>();
            keywords.Add(BatIdentifier);
            shield.TargetCriteria.HasAnyOfTheseKeywords = keywords;
            shield.UntilThisTurnIsOver(base.GameController.Game);
            IEnumerator protectCoroutine = AddStatusEffect(shield);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(protectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(protectCoroutine);
            }
        }

        private IEnumerator BatsSwarmResponse(PhaseChangeAction pca)
        {
            // "...each Bat deals each hero target 1 projectile damage."
            List<DealDamageAction> results = new List<DealDamageAction>();
            IEnumerator damageCoroutine = MultipleDamageSourcesDealDamage(new LinqCardCriteria((Card c) => c.DoKeywordsContain(BatKeyword), "Bat"), TargetType.All, null, new LinqCardCriteria((Card c) => c.IsHero && c.IsTarget), 1, DamageType.Projectile, damageResults: results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "If less than 4 damage is dealt this way, destroy this card."
            int totalDamage = 0;
            foreach (DealDamageAction dda in results)
            {
                if (dda.DidDealDamage)
                {
                    totalDamage += dda.Amount;
                }
            }
            if (totalDamage < 4)
            {
                IEnumerator destroyCoroutine = DestroyThisCardResponse(null);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
        }
    }
}
