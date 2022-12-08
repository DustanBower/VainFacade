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
    public class CarnavalCharacterCardController : HeroCharacterCardController
    {
        public CarnavalCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Masque cards in Carnaval's deck
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Deck, MasqueCard);
        }

        protected static readonly string MasqueKeyword = "masque";
        protected static LinqCardCriteria MasqueCard = new LinqCardCriteria((Card c) => c.DoKeywordsContain(MasqueKeyword), "Masque");

        public override IEnumerator UsePower(int index = 0)
        {
            // "Reveal cards from the top of your deck until 2 Masques are revealed. Put 1 into your hand or into play. Shuffle the remaining cards back into your deck."
            int numMasques = GetPowerNumeral(0, 2);
            int numToMove = GetPowerNumeral(1, 1);
            IEnumerator revealMoveCoroutine = RevealCards_SelectSome_MoveThem_ReturnTheRest(DecisionMaker, base.TurnTakerController, base.TurnTaker.Deck, (Card c) => c.DoKeywordsContain(MasqueKeyword), numMasques, numToMove, true, true, true, "Masque");
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealMoveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealMoveCoroutine);
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
            yield break;
        }

        private IEnumerator UseIncapOption1()
        {
            // "Reveal the top card of a deck. Replace or discard it."
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
                    IEnumerator revealCoroutine = RevealCard_DiscardItOrPutItOnDeck(DecisionMaker, base.TurnTakerController, deck, false);
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

        private IEnumerator UseIncapOption2()
        {
            // "Destroy an environment card."
            return base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), false, responsibleCard: base.Card, cardSource: GetCardSource());
        }

        private IEnumerator UseIncapOption3()
        {
            // "Select a target."
            List<SelectCardDecision> choices = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.SelectTargetFriendly, new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText), choices, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            SelectCardDecision choice = choices.FirstOrDefault((SelectCardDecision d) => d.Completed);
            Card chosen = choice.SelectedCard;
            if (chosen != null)
            {
                // "The next time that target is dealt damage by a target, it deals the source of that damage 2 fire damage."
                OnDealDamageStatusEffect reaction = new OnDealDamageStatusEffect(CardWithoutReplacements, nameof(CounterDamageResponse), "The next time " + chosen.Title + " is dealt damage by a target, " + chosen.Title + " deals that target 2 fire damage.", new TriggerType[] { TriggerType.DealDamage }, chosen.Owner, base.Card);
                reaction.TargetCriteria.IsSpecificCard = chosen;
                reaction.SourceCriteria.IsTarget = true;
                reaction.DamageAmountCriteria.GreaterThan = 0;
                reaction.UntilTargetLeavesPlay(chosen);
                reaction.BeforeOrAfter = BeforeOrAfter.After;
                reaction.DoesDealDamage = true;
                reaction.NumberOfUses = 1;

                IEnumerator statusCoroutine = AddStatusEffect(reaction);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator CounterDamageResponse(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            // "... it deals the source of that damage 2 fire damage."
            Card reacting = dda.Target;
            if (reacting != null && dda.DamageSource.IsCard)
            {
                Card source = dda.DamageSource.Card;
                IEnumerator damageCoroutine = DealDamage(reacting, source, 2, DamageType.Fire, isCounterDamage: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            yield break;
        }
    }
}
