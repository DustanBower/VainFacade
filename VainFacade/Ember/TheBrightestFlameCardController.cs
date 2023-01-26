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
    public class TheBrightestFlameCardController : CardController
    {
        public TheBrightestFlameCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            IEnumerator loopCoroutine = BurnDiscardLoopResponse();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(loopCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(loopCoroutine);
            }
        }

        private IEnumerator BurnDiscardLoopResponse()
        {
            // "{EmberCharacter} deals 1 target 2 fire damage."
            IEnumerator burnOtherCoroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), 2, DamageType.Fire, 1, false, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(burnOtherCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(burnOtherCoroutine);
            }
            // "{EmberCharacter} may deal herself 2 irreducible fire damage."
            List<DealDamageAction> selfDamageResults = new List<DealDamageAction>();
            IEnumerator burnSelfCoroutine = DealDamage(base.CharacterCard, base.CharacterCard, 2, DamageType.Fire, isIrreducible: true, optional: true, storedResults: selfDamageResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(burnSelfCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(burnSelfCoroutine);
            }
            // "If she takes damage this way, you may discard a card to repeat the text of this card."
            if (DidDealDamage(selfDamageResults, base.CharacterCard))
            {
                List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
                IEnumerator discardCoroutine = SelectAndDiscardCards(DecisionMaker,1, optional: true, storedResults: discardResults, responsibleTurnTaker: base.TurnTaker);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
                if (DidDiscardCards(discardResults, 1))
                {
                    IEnumerator loopCoroutine = BurnDiscardLoopResponse();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(loopCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(loopCoroutine);
                    }
                }
            }
        }
    }
}
