using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class OverTheLineCardController : GrandfatherUtilityCardController
    {
        public OverTheLineCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Damage dealt by {Grandfather} is irreducible."
            AddMakeDamageIrreducibleTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card == base.CharacterCard);
            // "At the start of the villain turn, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, discard the top 5 cards of each hero deck..."
            IEnumerator massDiscardCoroutine = DiscardTopXCardsOfEachHeroDeckResponse(5, null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(massDiscardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(massDiscardCoroutine);
            }
            // "... then play the top card of the villain deck."
            IEnumerator playVillainCoroutine = base.GameController.PlayTopCard(DecisionMaker, base.TurnTakerController, responsibleTurnTaker: base.TurnTaker, showMessage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playVillainCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playVillainCoroutine);
            }
        }
    }
}
