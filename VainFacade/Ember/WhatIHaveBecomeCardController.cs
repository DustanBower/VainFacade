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
    public class WhatIHaveBecomeCardController : EmberUtilityCardController
    {
        public WhatIHaveBecomeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Blaze cards in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(BlazeCard);
        }

        public override IEnumerator Play()
        {
            // "{EmberCharacter} deals herself up to X psychic damage, where X = the number of Blaze cards in play."
            List<SelectNumberDecision> numberResults = new List<SelectNumberDecision>();
            IEnumerator numberCoroutine = base.GameController.SelectNumber(DecisionMaker, SelectionType.DealDamage, 0, NumBlazeCardsInPlay(), storedResults: numberResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(numberCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(numberCoroutine);
            }
            int amtToDeal = numberResults.First((SelectNumberDecision d) => d.Completed).SelectedNumber.Value;
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            IEnumerator damageCoroutine = DealDamage(base.CharacterCard, base.CharacterCard, amtToDeal, DamageType.Psychic, storedResults: damageResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "For each point of damage she takes this way, you may use a power on a Blaze card."
            int amtDealt = 0;
            foreach (DealDamageAction dda in damageResults)
            {
                if (dda.DidDealDamage && dda.Target == base.CharacterCard)
                {
                    amtDealt += dda.Amount;
                }
            }
            IEnumerator powerCoroutine = base.GameController.SelectAndUsePower(DecisionMaker, true, (Power p) => p.CardController.Card.DoKeywordsContain(BlazeKeyword), numberOfPowers: amtDealt, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(powerCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(powerCoroutine);
            }
        }
    }
}
