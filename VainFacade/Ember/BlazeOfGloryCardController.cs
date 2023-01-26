using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Ember
{
    public class BlazeOfGloryCardController : CardController
    {
        public BlazeOfGloryCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "{EmberCharacter} deals each target 1 irreducible fire damage."
            IEnumerator fireCoroutine = DealDamage(base.CharacterCard, (Card c) => c.IsTarget, 1, DamageType.Fire, isIrreducible: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(fireCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(fireCoroutine);
            }
        }
    }
}
