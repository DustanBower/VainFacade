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
    public class TheCloisteredPrinceCardController : TheMidnightBazaarUtilityCardController
    {
        public TheCloisteredPrinceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card in a hero play area."
            List<SelectTurnTakerDecision> storedDecisions = new List<SelectTurnTakerDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectHeroTurnTaker(DecisionMaker, SelectionType.MoveCardToPlayArea, optional: false, allowAutoDecide: false, storedDecisions, null, null, allowIncapacitatedHeroes: false, null, null, canBeCancelled: true, null, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            SelectTurnTakerDecision selection = storedDecisions.FirstOrDefault();
            if (selection != null && selection.SelectedTurnTaker != null)
            {
                storedResults?.Add(new MoveCardDestination(selection.SelectedTurnTaker.PlayArea));
            }
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase damage dealt to and by targets in this play area by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.Target.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation, (DealDamageAction dda) => 1);
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card.IsTarget && dda.DamageSource.Card.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation, (DealDamageAction dda) => 1);
            // "At the end of that hero's turn, they may play a card or use a power. If they do, play the top card of the environment or villain deck."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker && !base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.ToHero().IsIncapacitatedOrOutOfGame, PlayOrPowerWithCostResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.UsePower });
        }

        private IEnumerator PlayOrPowerWithCostResponse(PhaseChangeAction pca)
        {
            // "... they may play a card or use a power."
            List<Function> options = new List<Function>();
            List<PlayCardAction> playedCards = new List<PlayCardAction>();
            List<UsePowerDecision> usedPowers = new List<UsePowerDecision>();
            options.Add(new Function(base.GameController.FindTurnTakerController(base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker).ToHero(), "Play a card", SelectionType.PlayCard, () => SelectAndPlayCardFromHand(base.GameController.FindTurnTakerController(base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker).ToHero(), storedResults: playedCards)));
            options.Add(new Function(base.GameController.FindTurnTakerController(base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker).ToHero(), "Use a power", SelectionType.UsePower, () => base.GameController.SelectAndUsePower(base.GameController.FindTurnTakerController(base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker).ToHero(), storedResults: usedPowers, cardSource: GetCardSource())));
            SelectFunctionDecision select = new SelectFunctionDecision(GameController, base.GameController.FindTurnTakerController(base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker).ToHero(), options, true, noSelectableFunctionMessage: base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.Name + " cannot play cards or use powers.", cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectAndPerformFunction(select);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            // "If they do, play the top card of the environment or villain deck."
            bool didSomething = false;
            if (playedCards.Any((PlayCardAction play) => play.WasCardPlayed))
                didSomething = true;
            UsePowerDecision used = usedPowers.FirstOrDefault();
            if (used != null && used.SelectedPower != null)
                didSomething = true;
            if (didSomething)
            {
                List<Function> consequences = new List<Function>();
                consequences.Add(new Function(DecisionMaker, "Play the top card of the environment deck", SelectionType.PlayTopCardOfEnvironmentDeck, () => PlayTheTopCardOfTheEnvironmentDeckWithMessageResponse(null), FindEnvironment().TurnTaker.Deck.HasCards || FindEnvironment().TurnTaker.Trash.HasCards));
                consequences.Add(new Function(DecisionMaker, "Play the top card of the villain deck", SelectionType.PlayTopCardOfVillainDeck, () => PlayTheTopCardOfTheVillainDeckWithMessageResponse(null), FindVillainTurnTakerControllers(true).Any((TurnTakerController ttc) => ttc.TurnTaker.Deck.HasCards || ttc.TurnTaker.Trash.HasCards)));
                SelectFunctionDecision poison = new SelectFunctionDecision(GameController, DecisionMaker, consequences, false, cardSource: GetCardSource());
                IEnumerator pickCoroutine = base.GameController.SelectAndPerformFunction(poison);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(pickCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(pickCoroutine);
                }
            }
            yield break;
        }
    }
}
