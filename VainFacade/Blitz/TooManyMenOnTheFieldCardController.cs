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
    public class TooManyMenOnTheFieldCardController : CardController
    {
        public TooManyMenOnTheFieldCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero target with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
            // Show number of hero targets in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsHero && c.IsTarget, "hero", singular: "target", plural: "targets"));
        }

        public override IEnumerator Play()
        {
            // "{BlitzCharacter} deals the hero target with the highest HP X melee damage, where X = the number of hero targets in play."
            IEnumerator damageCoroutine = DealDamageToHighestHP(base.CharacterCard, 1, (Card c) => c.IsHero && c.IsTarget, (Card c) => base.GameController.FindCardsWhere((Card c2) => c2.IsHero && c2.IsTarget, visibleToCard: GetCardSource()).Count(), DamageType.Melee);
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
