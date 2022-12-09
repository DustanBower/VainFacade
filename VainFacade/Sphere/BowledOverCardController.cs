using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class BowledOverCardController : SphereUtilityCardController
    {
        public BowledOverCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Emanations in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(isEmanation);
        }

        public override IEnumerator Play()
        {
            // "{Sphere} deals 1 target X plus 2 energy damage, where X is the number of Emanations in play."
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), (Card c) => base.GameController.FindCardsWhere(isEmanationInPlay, visibleToCard: GetCardSource()).Count() + 2, DamageType.Energy, () => 1, false, 1, cardSource: GetCardSource());
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
