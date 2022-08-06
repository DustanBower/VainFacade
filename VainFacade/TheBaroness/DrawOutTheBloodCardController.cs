using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
    public class DrawOutTheBloodCardController : BaronessUtilityCardController
    {
        public DrawOutTheBloodCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "{TheBaroness} deals each non-villain target 1 infernal damage."
            IEnumerator damageCoroutine = DealDamage(base.CharacterCard, (Card c) => c.IsTarget && !c.IsVillain, 1, DamageType.Infernal);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "Put the top card of each hero deck face-down in the villain play area."
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame && tt.Deck.HasCards, "heroes with cards in their decks"), SelectionType.MoveCardFaceDownToVillainPlayArea, (TurnTaker tt) => MoveFaceDownToVillainPlayArea(tt.Deck.TopCard), allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            yield break;
        }
    }
}
