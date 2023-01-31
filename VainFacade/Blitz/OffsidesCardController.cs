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
    public class OffsidesCardController : CardController
    {
        public OffsidesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of non-character non-villain cards in the villain play area
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.PlayArea, new LinqCardCriteria((Card c) => !c.IsCharacter && !c.IsVillain, "non-character non-villain"));
        }

        public override IEnumerator Play()
        {
            // "Destroy all non-character non-villain cards in the villain play area."
            List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = base.GameController.DestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => !c.IsCharacter && !c.IsVillain && c.Location.IsPlayAreaOf(base.TurnTaker), "non-character non-villain", singular: "card in the villain play area", plural: "cards in the villain play area"), storedResults: destroyResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            int x = 1 + GetNumberOfCardsDestroyed(destroyResults);
            // "{BlitzCharacter} deals each hero target X plus 1 lightning damage, where X = the number of cards destroyed in this way."
            IEnumerator damageCoroutine = DealDamage(base.CharacterCard, (Card c) => c.IsHero && c.IsTarget, x, DamageType.Lightning);
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
