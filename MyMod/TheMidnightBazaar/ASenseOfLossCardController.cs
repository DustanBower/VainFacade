using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacade.TheMidnightBazaar
{
    public class ASenseOfLossCardController : TheMidnightBazaarUtilityCardController
    {
        public ASenseOfLossCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If The Empty Well is in play, show the number of cards from each hero deck under it
            /*foreach (HeroTurnTaker hero in Game.HeroTurnTakers)
            {
                SpecialStringMaker.ShowNumberOfCardsAtLocation(FindCard(EmptyWellIdentifier).UnderLocation, new LinqCardCriteria((Card c) => c.Owner.ToHero() == hero, "from " + hero.Name + "'s deck", false, true), () => base.Card.IsInPlayAndHasGameText && FindCard(EmptyWellIdentifier).IsInPlayAndHasGameText).Condition = () => FindCard(EmptyWellIdentifier).IsInPlayAndHasGameText;
            }*/
            // Otherwise, remind the players that The Empty Well is not in play
            SpecialStringMaker.ShowSpecialString(() => FindCard(EmptyWellIdentifier).Title + " is not in play.").Condition = () => !IsEmptyWellInPlay();
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt by non-environment targets by 1."
            AddReduceDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsTarget && !dda.DamageSource.IsEnvironmentTarget, (DealDamageAction dda) => 1);
            // "At the start of the environment turn, deal each hero 1 psychic damage for each card from their deck under [i]The Empty Well.[/i]"
            AddStartOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment && IsEmptyWellInPlay(), PsychicDamageResponse, TriggerType.DealDamage);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, play the top card of the environment deck."
            IEnumerator playCoroutine = PlayTheTopCardOfTheEnvironmentDeckWithMessageResponse(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }

        private IEnumerator PsychicDamageResponse(PhaseChangeAction pca)
        {
            // "... deal each hero 1 psychic damage for each card from their deck under [i]The Empty Well.[/i]"
            bool dealingDamage = true;
            string failureMessage = "";
            if (!FindCard(EmptyWellIdentifier).IsInPlayAndHasGameText)
            {
                dealingDamage = false;
                failureMessage = FindCard(EmptyWellIdentifier).Title + " is not in play.";
            }
            else if (!FindCard(EmptyWellIdentifier).UnderLocation.Cards.Any((Card c) => c.IsHero))
            {
                dealingDamage = false;
                failureMessage = "There are no hero cards under " + FindCard(EmptyWellIdentifier).Title + ", so " + base.Card.Title + " does not deal damage.";
            }
            if (dealingDamage)
            {
                Dictionary<Card, int> cardAttacks = new Dictionary<Card, int>();
                foreach (Card character in FindCardsWhere((Card c) => c.IsHeroCharacterCard && c.IsTarget && c.IsInPlay))
                {
                    int count = FindCard(EmptyWellIdentifier).UnderLocation.Cards.Where((Card u) => u.Owner == character.Owner).Count();
                    if (count > 0)
                        cardAttacks[character] = count;
                }
                bool stillChoosing = true;
                while (cardAttacks.Count > 0 && base.Card.IsInPlayAndHasGameText && !base.GameController.IsGameOver)
                {
                    Card selectedTarget = null;
                    // Let the players choose a target, or choose one automatically if they've hit Choose For Me
                    if (stillChoosing)
                    {
                        Func<Card, int?> damageAmount = (Card target) => (target == null) ? null : new int?(1);
                        Func<Card, int?> numberOfTimes = (Card target) => (target == null) ? null : new int?(cardAttacks[target]);
                        List<SelectTargetDecision> storedResults = new List<SelectTargetDecision>();
                        IEnumerator selectCoroutine = base.GameController.SelectTargetAndStoreResults(DecisionMaker, cardAttacks.Keys, storedResults, allowAutoDecide: true, damageSource: base.Card, damageAmount: damageAmount, damageType: DamageType.Psychic, dynamicNumberOfTimes: numberOfTimes, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(selectCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(selectCoroutine);
                        }
                        SelectTargetDecision chosen = storedResults.FirstOrDefault((SelectTargetDecision d) => d.Completed);
                        if (chosen != null)
                        {
                            selectedTarget = chosen.SelectedCard;
                            stillChoosing = !chosen.AutoDecided;
                        }
                    }
                    else
                    {
                        selectedTarget = cardAttacks.Keys.FirstOrDefault();
                    }
                    // Hit that target the appropriate number of times
                    if (selectedTarget != null)
                    {
                        int attacks = cardAttacks[selectedTarget];
                        for (int i = 0; i < attacks; i++)
                        {
                            IEnumerator damageCoroutine = DealDamage(base.Card, selectedTarget, 1, DamageType.Psychic, cardSource: GetCardSource());
                            if (base.UseUnityCoroutines)
                            {
                                yield return base.GameController.StartCoroutine(damageCoroutine);
                            }
                            else
                            {
                                base.GameController.ExhaustCoroutine(damageCoroutine);
                            }
                        }
                        cardAttacks.Remove(selectedTarget);
                    }
                }
            }
            else
            {
                // Report why we're not dealing damage
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(failureMessage, Priority.High, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            yield break;
        }
    }
}
