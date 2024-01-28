using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Glyph
{
	public class MaledictionOfInaalaCardController:GlyphCardUtilities
	{
		public MaledictionOfInaalaCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria(IsFaceDownMight, "face-down might"));
		}

        public override IEnumerator Play()
        {
            //Discard up to 3 cards.
            //You may destroy 1 of your face-down might cards.
            //If you do, you may discard any number of cards instead.
            List<DestroyCardAction> destroyMight = new List<DestroyCardAction>();
            IEnumerator coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria(IsFaceDownMight, "face-down might"), true, destroyMight,cardSource:GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            int? amount = 3;
            if (DidDestroyCard(destroyMight))
            {
                amount = null;
            }

            List<DiscardCardAction> results = new List<DiscardCardAction>();
            coroutine = base.GameController.SelectAndDiscardCards(DecisionMaker, amount, false, 0, results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Glyph deals 1 target X times 2 infernal damage where X = the number of cards discarded this way.
            int num = GetNumberOfCardsDiscarded(results) * 2;
            coroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, this.CharacterCard), num, DamageType.Infernal, 1, false, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }
    }
}

