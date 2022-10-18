using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheFury
{
    public class ViciousOnslaughtCardController : CardController
    {
        public ViciousOnslaughtCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override bool DoNotMoveOneShotToTrash
        {
            get
            {
                if (base.Card.IsInHand)
                {
                    return true;
                }
                return false;
            }
        }

        public override IEnumerator Play()
        {
            // "{TheFuryCharacter} deals 1 target 3 irreducible melee damage, 3 targets 2 irreducible melee damage each, or 7 targets 1 irreducible melee damage each."
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            IEnumerable<Function> options = new Function[3]
            {
                new Function(DecisionMaker, base.CharacterCard.Title + " deals 1 target 3 irreducible melee damage", SelectionType.DealDamage, () => base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), 3, DamageType.Melee, 1, false, 1, isIrreducible: true, storedResultsDamage: damageResults, cardSource: GetCardSource())),
                new Function(DecisionMaker, base.CharacterCard.Title + " deals 3 targets 2 irreducible melee damage each", SelectionType.DealDamage, () => base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), 2, DamageType.Melee, 3, false, 3, isIrreducible: true, storedResultsDamage: damageResults, cardSource: GetCardSource())),
                new Function(DecisionMaker, base.CharacterCard.Title + " deals 7 targets 1 irreducible melee damage each", SelectionType.DealDamage, () => base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), 1, DamageType.Melee, 7, false, 7, isIrreducible: true, storedResultsDamage: damageResults, cardSource: GetCardSource()))
            };
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, DecisionMaker, options, false, noSelectableFunctionMessage: base.CharacterCard.Title + " cannot currently deal damage to any targets.", cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            // "If a hero target is dealt damage this way, you may play a card, use a power, or discard a card."
            if (damageResults.Where((DealDamageAction dda) => dda.DidDealDamage && dda.Target.IsHero).Any())
            {
                List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
                options = new Function[3]
                {
                    new Function(DecisionMaker, "Play a card", SelectionType.PlayCard, () => base.GameController.SelectAndPlayCardFromHand(DecisionMaker, true, cardSource: GetCardSource()), onlyDisplayIfTrue: base.TurnTaker.ToHero().HasCardsInHand),
                    new Function(DecisionMaker, "Use a power", SelectionType.UsePower, () => base.GameController.SelectAndUsePower(DecisionMaker, optional: true, cardSource: GetCardSource()), base.GameController.CanUsePowers(DecisionMaker, GetCardSource())),
                    new Function(DecisionMaker, "Discard a card", SelectionType.DiscardCard, () => SelectAndDiscardCards(DecisionMaker, 1, requiredDecisions: 1, storedResults: discardResults, responsibleTurnTaker: base.TurnTaker), onlyDisplayIfTrue: base.TurnTaker.ToHero().HasCardsInHand)
                };
                choice = new SelectFunctionDecision(base.GameController, DecisionMaker, options, false, noSelectableFunctionMessage: base.CharacterCard.Title + " cannot currently deal damage to any targets.", cardSource: GetCardSource());
                chooseCoroutine = base.GameController.SelectAndPerformFunction(choice);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(chooseCoroutine);
                }
                // "If you discard a card, return this card to your hand."
                if (DidDiscardCards(discardResults))
                {
                    IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.ToHero().Hand, responsibleTurnTaker: base.TurnTaker, showMessage: true, cardSource: GetCardSource());
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
}
