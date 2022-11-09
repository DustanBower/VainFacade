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
    public class NodeCharacterCardController : HeroCharacterCardController
    {
        public NodeCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            int numTargets = GetPowerNumeral(0, 1);
            int firstAmt = GetPowerNumeral(1, 0);
            int secondAmt = GetPowerNumeral(2, 0);
            int boostAmt = GetPowerNumeral(3, 1);
            // "Draw a card or {NodeCharacter} deals 1 target 0 psychic damage."
            List<DealDamageAction> results = new List<DealDamageAction>();
            IEnumerable<Function> options = new Function[2]{
                new Function(DecisionMaker, "Draw a card", SelectionType.DrawCard, () => DrawCard(base.TurnTaker.ToHero())),
                new Function(DecisionMaker, base.Card.Title + " deals " + numTargets.ToString() + " target " + firstAmt.ToString() + " psychic damage", SelectionType.DealDamage, () => base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.Card), firstAmt, DamageType.Psychic, numTargets, false, numTargets, storedResultsDamage: results, cardSource: GetCardSource()))
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
            foreach (DealDamageAction dda in results)
            {
                if (dda.DidDealDamage)
                {
                    // "If damage is dealt this way, {NodeCharacter} deals that target 0 psychic damage..."
                    IEnumerator secondCoroutine = DealDamage(base.Card, dda.Target, secondAmt, DamageType.Psychic, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(secondCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(secondCoroutine);
                    }
                    // "... and increases the next damage dealt to that target by 1."
                    IncreaseDamageStatusEffect debuff = new IncreaseDamageStatusEffect(boostAmt);
                    debuff.TargetCriteria.IsSpecificCard = dda.Target;
                    debuff.UntilTargetLeavesPlay(dda.Target);
                    debuff.NumberOfUses = 1;
                    IEnumerator statusCoroutine = base.GameController.AddStatusEffect(debuff, true, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(statusCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(statusCoroutine);
                    }
                }
            }
            yield break;
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator incapCoroutine;
            switch (index)
            {
                case 0:
                    incapCoroutine = UseIncapOption1();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 1:
                    incapCoroutine = UseIncapOption2();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 2:
                    incapCoroutine = UseIncapOption3();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
            }
            yield break;
        }

        private IEnumerator UseIncapOption1()
        {
            // "One player may discard a card to reduce the next damage dealt to a hero character card in their play area by the number of active heroes until the start of your next turn."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = base.GameController.SelectHeroToDiscardCard(DecisionMaker, storedResultsDiscard: discards, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(discards))
            {
                DiscardCardAction dca = discards.FirstOrDefault((DiscardCardAction d) => d.WasCardDiscarded);
                TurnTaker tt = dca.ResponsibleTurnTaker;
                if (tt.IsHero)
                {
                    ReduceDamageStatusEffect protection = new ReduceDamageStatusEffect(FindCardsWhere(new LinqCardCriteria((Card c) => c.IsHeroCharacterCard && !c.IsIncapacitatedOrOutOfGame, "active hero character"), visibleToCard: GetCardSource()).Count());
                    protection.TargetCriteria.IsHeroCharacterCard = true;
                    protection.TargetCriteria.IsAtLocation = tt.PlayArea;
                    protection.NumberOfUses = 1;
                    protection.UntilStartOfNextTurn(base.TurnTaker);
                    IEnumerator statusCoroutine = base.GameController.AddStatusEffect(protection, true, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(statusCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(statusCoroutine);
                    }
                }
            }
            yield break;
        }

        private IEnumerator UseIncapOption2()
        {
            // "Reveal the top card of a deck. Put it into play or discard it."
            List<SelectLocationDecision> choices = new List<SelectLocationDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.RevealTopCardOfDeck, (Location l) => true, choices, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            if (DidSelectDeck(choices))
            {
                Location deck = choices.First().SelectedLocation.Location;
                if (deck != null)
                {
                    IEnumerator revealCoroutine = RevealCard_PlayItOrDiscardIt(base.TurnTakerController, deck, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(revealCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(revealCoroutine);
                    }
                }
            }
            yield break;
        }

        private IEnumerator UseIncapOption3()
        {
            // "One player may discard up to 2 cards."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = base.GameController.SelectHeroToDiscardCards(DecisionMaker, 0, 2, storedResultsDiscard: discards, forDiscardEffect: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "For each card discarded in this way, one player may draw a card."
            for ( int i = 0; i < GetNumberOfCardsDiscarded(discards); i++)
            {
                IEnumerator drawCoroutine = base.GameController.SelectHeroToDrawCard(DecisionMaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawCoroutine);
                }
            }
            yield break;
        }
    }
}
