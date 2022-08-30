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
    public class EldrenwoodTargetCardController : EldrenwoodUtilityCardController
    {
        public EldrenwoodTargetCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "If this card is reduced to 0 or fewer HP, ..."
            AddBeforeDestroyAction(CheckHPResponse);
        }

        public IEnumerator CheckHPResponse(GameAction ga)
        {
            if (base.Card.HitPoints.Value <= 0)
            {
                IEnumerator responseCoroutine = ReducedToZeroResponse();
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(responseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(responseCoroutine);
                }
            }
        }

        public virtual IEnumerator ReducedToZeroResponse()
        {
            yield return null;
        }
    }
}
