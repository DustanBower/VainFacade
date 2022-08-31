using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.EldrenwoodVillage
{
    public class AfflictedCardController : EldrenwoodTargetCardController
    {
        public AfflictedCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If Howls in the Distance is face up, remind the player that text on this card doesn't matter
            SpecialStringMaker.ShowSpecialString(() => "Howls in the Distance: Afflicted targets have no game text.").Condition = () => CanActivateEffect(base.TurnTakerController, HowlsKey);
        }

        public override IEnumerator ReducedToZeroResponse()
        {
            if (CanActivateEffect(base.TurnTakerController, QuaintKey))
            {
                IEnumerator respondCoroutine = SlainInHumanFormResponse();
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(respondCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(respondCoroutine);
                }
            }
            else
            {
                // Howls in the Distance: Afflicted targets are Werewolves and have no game text
                yield break;
            }
        }

        public virtual IEnumerator SlainInHumanFormResponse()
        {
            yield return null;
        }
    }
}
