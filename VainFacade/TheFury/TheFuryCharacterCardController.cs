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
    public class TheFuryCharacterCardController : HeroCharacterCardController
    {
        public TheFuryCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            int amtRegained = GetPowerNumeral(0, 2);
            int amtPsychic = GetPowerNumeral(1, 0);
            int numTargets = GetPowerNumeral(2, 1);
            int xBase = GetPowerNumeral(3, 1);
            // "{TheFuryCharacter} regains 2 HP..."
            IEnumerator healCoroutine = base.GameController.GainHP(base.Card, amtRegained, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            // "... and is indestructible this turn."
            MakeIndestructibleStatusEffect protection = new MakeIndestructibleStatusEffect();
            protection.CardsToMakeIndestructible.IsSpecificCard = base.Card;
            protection.UntilThisTurnIsOver(base.Game);
            IEnumerator protectCoroutine = base.GameController.AddStatusEffect(protection, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(protectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(protectCoroutine);
            }
            // "She deals herself 0 psychic damage..."
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            IEnumerator psychicCoroutine = DealDamage(base.Card, base.Card, amtPsychic, DamageType.Psychic, storedResults: damageResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(psychicCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(psychicCoroutine);
            }
            int amtTaken = 0;
            if (DidDealDamage(damageResults))
            {
                foreach(DealDamageAction dda in damageResults)
                {
                    if (DidDealDamage(dda.ToEnumerable().ToList(), toSpecificTarget: base.Card, fromDamageSource: base.Card))
                    {
                        amtTaken += dda.Amount;
                    }
                }
            }
            int x = xBase + amtTaken;
            if (x > 0)
            {
                // "... then deals 1 target X melee damage..."
                IEnumerator meleeCoroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.Card), x, DamageType.Melee, numTargets, false, numTargets, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(meleeCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(meleeCoroutine);
                }
                // "... and increases the next damage dealt to {TheFuryCharacter} by X, where X = 1 plus the damage she takes this way."
                IncreaseDamageStatusEffect debuff = new IncreaseDamageStatusEffect(x);
                debuff.TargetCriteria.IsSpecificCard = base.Card;
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
        }

        private IEnumerator UseIncapOption1()
        {
            // "1 hero may use a power."
            yield return base.GameController.SelectHeroToUsePower(DecisionMaker, cardSource: GetCardSource());
        }

        private IEnumerator UseIncapOption2()
        {
            List<SelectLocationDecision> deckResults = new List<SelectLocationDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.PlayTopCard, (Location d) => true, deckResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            if (DidSelectDeck(deckResults))
            {
                // "Put the top card of a deck into play."
                LocationChoice lc = deckResults.First().SelectedLocation;
                Location deck = lc.Location;
                IEnumerator putCoroutine = base.GameController.PlayTopCardOfLocation(base.TurnTakerController, deck, requiredNumberOfCards: 1, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(putCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(putCoroutine);
                }
                // "Cards from that deck cannot be played until the start of your next turn."
                CannotPlayCardsStatusEffect lockdown = new CannotPlayCardsStatusEffect();
                lockdown.CardCriteria.NativeDeck = deck;
                lockdown.UntilStartOfNextTurn(TurnTaker);
                IEnumerator lockCoroutine = base.GameController.AddStatusEffect(lockdown, true, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(lockCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(lockCoroutine);
                }
            }
        }

        private IEnumerator UseIncapOption3()
        {
            // "Select a target. Increase the next damage dealt to that target by 2."
            List<SelectCardDecision> choices = new List<SelectCardDecision>();
            IEnumerator chooseCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.SelectTargetNoDamage, new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText), choices, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (choices.Count > 0)
            {
                Card selectedCard = choices.First().SelectedCard;
                if (selectedCard != null)
                {
                    IncreaseDamageStatusEffect debuff = new IncreaseDamageStatusEffect(2);
                    debuff.TargetCriteria.IsSpecificCard = selectedCard;
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
        }
    }
}
