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
    public class CleverHackCardController : CardController
    {
        public CleverHackCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Equipment cards in play
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => IsEquipment(c), "Equipment"));
            // Show list of environment cards in play
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"));
            // Show list of Device cards in play
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsDevice, "device"));
        }

        public override IEnumerator Play()
        {
            // "Destroy 1 Equipment or environment card or {NodeCharacter} deals 1 Device 6 irreducible lightning damage."
            IEnumerable<Function> options = new Function[2]{
                new Function(DecisionMaker, "Destroy 1 Equipment or environment card", SelectionType.DestroyCard, () => base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsEquipment(c) || c.IsEnvironment, "Equipment or environment"), false, responsibleCard: base.Card, cardSource:GetCardSource())),
                new Function(DecisionMaker, base.CharacterCard.Title + " deals 1 Device 6 irreducible lightning damage", SelectionType.DealDamage, () => base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), 6, DamageType.Lightning, 1, false, 1, isIrreducible: true, additionalCriteria: (Card c) => c.IsDevice, cardSource: GetCardSource()))
            };
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, DecisionMaker, options, false, cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
        }
    }
}
