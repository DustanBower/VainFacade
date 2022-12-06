using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Carnaval
{
    public class MasqueCardController : CarnavalUtilityCardController
    {
        public MasqueCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "When this card entes play, destroy all other Masques."
            IEnumerator destroyCoroutine = base.GameController.DestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.DoKeywordsContain(MasqueKeyword) && c != base.Card), cardSource: GetCardSource());
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
