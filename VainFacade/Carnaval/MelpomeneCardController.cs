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
    public class MelpomeneCardController : MasqueCardController
    {
        public MelpomeneCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of One-Shot cards in Carnaval's trash
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Trash, new LinqCardCriteria((Card c) => c.IsOneShot, "One-Shot"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When one of your cards would enter play, {CarnavalCharacter} may deal 1 target 1 melee or 1 psychic damage."
            AddTrigger((CardEntersPlayAction cepa) => cepa.CardEnteringPlay.Owner == base.Card.Owner && cepa.CardEnteringPlay != base.Card, SurpriseResponse, TriggerType.DealDamage, TriggerTiming.Before);
            // "Increase damage dealt by {CarnavalCharacter} by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource.IsSameCard(base.CharacterCard), 1);
        }

        private IEnumerator SurpriseResponse(CardEntersPlayAction cepa)
        {
            // "... {CarnavalCharacter} may deal 1 target 1 melee or 1 psychic damage."
            List<SelectDamageTypeDecision> typeChoices = new List<SelectDamageTypeDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectDamageType(DecisionMaker, typeChoices, choices: new DamageType[] { DamageType.Melee, DamageType.Psychic }, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }

            if (GetSelectedDamageType(typeChoices).HasValue)
            {
                DamageType chosen = GetSelectedDamageType(typeChoices).Value;
                IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), 1, chosen, 1, false, 0, cardSource: GetCardSource());
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

        public override IEnumerator UsePower(int index = 0)
        {
            // "Play a One-Shot from your trash, ..."
            List<SelectCardDecision> choices = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.PlayCard, new LinqCardCriteria((Card c) => c.IsOneShot && c.IsAtLocationRecursive(base.TurnTaker.Trash), "One-Shot"), choices, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            Card chosen = GetSelectedCard(choices);
            if (chosen != null)
            {
                IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, chosen, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
                // "... then put it on the bottom of your deck."
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, chosen, base.TurnTaker.Deck, toBottom: true, showMessage: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
            }
        }
    }
}
