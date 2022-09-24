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
    public class CountingDownCardController : GrandfatherUtilityCardController
    {
        public CountingDownCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of non-Covert non-hero targets in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsTarget && !c.IsHero && !c.DoKeywordsContain(CovertKeyword), "non-Covert non-hero", false, false, "target", "targets"));
            // Show non-hero target with second highest HP
            SpecialStringMaker.ShowNonHeroTargetWithHighestHP(ranking: 2);
            // Show 2 hero targets with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP(ranking: 1, numberOfTargets: 2);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, if there are fewer than 2 non-Covert non-hero targets in play, play the top card of the environment deck."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker && FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsTarget && !c.IsHero && !c.DoKeywordsContain(CovertKeyword), "non-Covert non-hero", false, false, "target", "targets"), visibleToCard: GetCardSource()).Count() < 2, (PhaseChangeAction pca) => base.GameController.PlayTopCard(DecisionMaker, FindEnvironment(), responsibleTurnTaker: base.TurnTaker, showMessage: true, cardSource: GetCardSource()), TriggerType.PlayCard);
            // "At the end of the villain turn, the non-hero target with the second highest HP deals the 2 hero targets with the highest HP {H} melee damage each."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, SecondHighestDealsDamageResponse, TriggerType.DealDamage);
        }

        private IEnumerator SecondHighestDealsDamageResponse(PhaseChangeAction pca)
        {
            // "... the non-hero target with the second highest HP deals the 2 hero targets with the highest HP {H} melee damage each."
            List<Card> results = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(2, (Card c) => !c.IsHero, results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            Card attacker = results.FirstOrDefault();
            if (attacker != null)
            {
                IEnumerator damageCoroutine = DealDamageToHighestHP(attacker, 1, (Card c) => c.IsHero, (Card c) => H, DamageType.Melee, numberOfTargets: () => 2);
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
