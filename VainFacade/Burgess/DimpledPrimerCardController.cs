using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Burgess
{
    public class DimpledPrimerCardController : CardController
    {
        public DimpledPrimerCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of cards in Burgess's hand
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.HeroTurnTaker.Hand);
        }

        public override IEnumerator Play()
        {
            // "Discard any number of cards."
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = SelectAndDiscardCards(base.HeroTurnTakerController, null, optional: false, requiredDecisions: 0, storedResults: results, allowAutoDecide: true, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "Increase the next damage dealt by Burgess by X plus 1, where X is the amount of cards discarded in this way."
            int x = 0;
            x += GetNumberOfCardsDiscarded(results);
            IncreaseDamageStatusEffect buff = new IncreaseDamageStatusEffect(x + 1);
            buff.SourceCriteria.IsSpecificCard = base.CharacterCard;
            buff.NumberOfUses = 1;
            buff.CardDestroyedExpiryCriteria.Card = base.CharacterCard;
            IEnumerator buffCoroutine = AddStatusEffect(buff);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(buffCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(buffCoroutine);
            }
        }
    }
}
