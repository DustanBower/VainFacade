using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class SphereCharacterCardController : HeroCharacterCardController
    {
        public SphereCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Emanation cards in play
            SpecialStringMaker.ShowListOfCardsInPlay(SphereUtilityCardController.isEmanation);
            // Show list of Emanation cards in trash
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Trash, SphereUtilityCardController.isEmanation);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int numTargets = GetPowerNumeral(0, 1);
            int meleeAmt = GetPowerNumeral(1, 1);
            int energyAmt = GetPowerNumeral(2, 1);
            // "Choose an Emanation in play and put a card with the same title into play from your trash or {Sphere} deals 1 target 1 melee damage and 1 energy damage."
            List<Function> options = new List<Function>();
            options.Add(new Function(base.HeroTurnTakerController, "Put an Emanation from your trash that matches one in play into play", SelectionType.PutIntoPlay, PlayMatchingEmanationFromTrash));
            options.Add(new Function(base.HeroTurnTakerController, "{Sphere} deals " + numTargets + " target " + meleeAmt + " melee damage and " + energyAmt + " energy damage", SelectionType.DealDamage, () => SphereDealsDamage(numTargets, meleeAmt, energyAmt)));
            SelectFunctionDecision select = new SelectFunctionDecision(GameController, base.HeroTurnTakerController, options, false, cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectAndPerformFunction(select);
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

        private IEnumerator PlayMatchingEmanationFromTrash()
        {
            // "Choose an Emanation in play and put a card with the same title into play from your trash"
            List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(base.HeroTurnTakerController, SelectionType.Custom, SphereUtilityCardController.isEmanationInPlay, storedResults, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            SelectCardDecision choice = storedResults.FirstOrDefault();
            if (choice != null && choice.SelectedCard != null)
            {
                Card chosen = choice.SelectedCard;
                IEnumerator playCoroutine = base.GameController.SelectAndPlayCard(base.HeroTurnTakerController, (Card c) => c.Location == base.TurnTaker.Trash && c.Title == chosen.Title, isPutIntoPlay: true, cardSource: GetCardSource(), noValidCardsMessage: "There are no cards with the same title as " + chosen.Title + " in " + base.TurnTaker.Name + "'s trash.");
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
            yield break;
        }

        private IEnumerator SphereDealsDamage(int targets, int melee, int energy)
        {
            // "{Sphere} deals 1 target 1 melee damage and 1 energy damage."
            List<DealDamageAction> hits = new List<DealDamageAction>();
            hits.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, melee, DamageType.Melee));
            hits.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, energy, DamageType.Energy));
            IEnumerator damageCoroutine = SelectTargetsAndDealMultipleInstancesOfDamage(hits, minNumberOfTargets: targets, maxNumberOfTargets: targets);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // ...
            yield break;
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Select an Emanation to find a copy in your trash", "selecting an Emanation to find a copy in their trash", "Vote for an Emanation for " + base.TurnTaker.Name + " to find a copy of in their trash", "Emanation to find a copy of in the trash");
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
            // "Select a target. Reduce damage dealt to that target by 1 until the start of your next turn."
            List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(base.HeroTurnTakerController, SelectionType.ReduceDamageTaken, new LinqCardCriteria((Card c) => c.IsTarget, "target", false, false, "target", "targets"), storedResults, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            SelectCardDecision choice = storedResults.FirstOrDefault();
            if (choice != null && choice.SelectedCard != null)
            {
                ReduceDamageStatusEffect forcefield = new ReduceDamageStatusEffect(1);
                forcefield.TargetCriteria.IsSpecificCard = choice.SelectedCard;
                forcefield.UntilStartOfNextTurn(base.TurnTaker);
                IEnumerator statusCoroutine = AddStatusEffect(forcefield);
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

        private IEnumerator UseIncapOption2()
        {
            // "Destroy an environment card."
            yield return base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), false, responsibleCard: base.Card, cardSource: GetCardSource());
        }

        private IEnumerator UseIncapOption3()
        {
            // "Destroy an Ongoing card."
            yield return base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsOngoing, "ongoing"), false, responsibleCard: base.Card, cardSource: GetCardSource());
        }
    }
}
