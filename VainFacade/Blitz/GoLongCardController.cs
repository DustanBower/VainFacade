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
    public class GoLongCardController : CardController
    {
        public GoLongCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero target with lowest HP
            SpecialStringMaker.ShowHeroTargetWithLowestHP();
        }

        public override IEnumerator Play()
        {
            // "{BlitzCharacter} deals the hero target with the lowest HP {H - 1} lightning damage."
            IEnumerator damageCoroutine = DealDamageToLowestHP(base.CharacterCard, 1, (Card c) => IsHeroTarget(c), (Card c) => H - 1, DamageType.Lightning);
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
