using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Node
{
    public class HyperlinkedNeuronsCardController : NodeUtilityCardController
    {
        public HyperlinkedNeuronsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show Connected targets other than Node in play
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsTarget && IsConnected(c) && c != base.CharacterCard, "other than Node", false, true, "Connected target", "Connected targets"), () => base.Card.IsInPlayAndHasGameText);
            // Show Connected heroes
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => IsHeroCharacterCard(c) && c.IsTarget && IsConnected(c), "active Connected hero character"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase damage dealt by {NodeCharacter} to [i]Connected[/i] targets other than {NodeCharacter} by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card == base.CharacterCard && IsConnected(dda.Target) && dda.Target != base.CharacterCard, (DealDamageAction dda) => 1);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int numTargets = GetPowerNumeral(0, 1);
            int psychicAmt = GetPowerNumeral(1, 0);
            int increaseAmt = GetPowerNumeral(2, 2);
            // "{NodeCharacter} deals 1 target 0 psychic damage."
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), psychicAmt, DamageType.Psychic, numTargets, false, numTargets, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "Select a [i]Connected[/i] hero. Increase the next damage dealt by that hero by 2."
            //if (FindCardsWhere(new LinqCardCriteria((Card c) => c.IsHeroCharacterCard && c.IsTarget && IsConnected(c), "Connected hero character"), visibleToCard: GetCardSource()).Any())
            IEnumerator increaseCoroutine = base.GameController.SelectTargetAndIncreaseNextDamage(DecisionMaker, increaseAmt, additionalCriteria: new LinqCardCriteria((Card c) => IsHeroCharacterCard(c) && c.IsTarget && IsConnected(c), "active Connected hero character"), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(increaseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(increaseCoroutine);
            }
        }
    }
}
