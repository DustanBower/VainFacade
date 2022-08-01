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
    public class FangAndClawCardController : CardController
    {
        public FangAndClawCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show 2 hero targets with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP(ranking: 1, numberOfTargets: 2);
            // Show hero target with lowest HP
            SpecialStringMaker.ShowHeroTargetWithLowestHP(ranking: 1, numberOfTargets: 1);
        }

        public override IEnumerator Play()
        {
            // "{TheBaroness} deals the 2 hero targets with the highest HP {H} melee damage each..."
            IEnumerator clawsCoroutine = DealDamageToHighestHP(base.CharacterCard, 1, (Card c) => c.IsHero, (Card c) => H, DamageType.Melee, numberOfTargets: () => 2);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(clawsCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(clawsCoroutine);
            }
            // "... then deals the hero target with the lowest HP {H - 2} melee damage."
            IEnumerator fangscoroutine = DealDamageToLowestHP(base.CharacterCard, 1, (Card c) => c.IsHero, (Card c) => H - 2, DamageType.Melee);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(fangscoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(fangscoroutine);
            }
        }
    }
}
