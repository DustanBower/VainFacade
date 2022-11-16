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
    public class PsychicLinkCardController : ConnectionCardController
    {
        public PsychicLinkCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // "Play with the top card of that play area's deck face up."
            SpecialStringMaker.ShowSpecialString(() => "The top card of " + base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.Deck.GetFriendlyName() + " is " + base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.Deck.TopCard.Title + ".", showInEffectsList: () => base.Card.IsInPlayAndHasGameText).Condition = () => base.Card.IsInPlayAndHasGameText;
            // If in play: show whether this card has increased damage this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(DamageThisTurn, base.Card.Title + " has already increased damage this turn.", base.Card.Title + " has not increased damage this turn.").Condition = () => base.Card.IsInPlayAndHasGameText;
            // If in play: remind player to click on Node's character card to use this card's power
            SpecialStringMaker.ShowSpecialString(() => "Click on " + base.TurnTaker.Name + "'s hero character card to use this power.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override bool IsValidPlayArea(TurnTaker tt)
        {
            // "Play this card in a non-environment play area other than {NodeCharacter}'s."
            return !tt.IsEnvironment && tt != base.TurnTaker;
        }

        protected readonly string DamageThisTurn = "FirstDamageToNodeFromThisPlayAreaThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            base.AddAsPowerContributor();
            // "The first time each turn a target in this play area would deal damage to {NodeCharacter}, increase that damage by 1."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(DamageThisTurn) && dda.Target == base.CharacterCard && dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card.IsTarget && dda.DamageSource.Card.Location.IsPlayAreaOf(base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker), IncreaseResponse, TriggerType.IncreaseDamage, TriggerTiming.Before);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(DamageThisTurn), TriggerType.Hidden);
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> destination, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card in a non-environment play area other than {NodeCharacter}'s."
            // Adapted from SergeantSteelTeam.MissionObjectiveCardController
            List<SelectTurnTakerDecision> storedResults = new List<SelectTurnTakerDecision>();
            IEnumerator coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.MoveCardToPlayArea, storedResults, optional: false, allowAutoDecide: false, (TurnTaker tt) => !tt.IsEnvironment && tt != base.TurnTaker, null, null, checkExtraTurnTakersInstead: false, canBeCancelled: true, ignoreBattleZone: false, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            SelectTurnTakerDecision selectTurnTakerDecision = storedResults.FirstOrDefault();
            if (selectTurnTakerDecision != null && selectTurnTakerDecision.SelectedTurnTaker != null && destination != null)
            {
                destination.Add(new MoveCardDestination(selectTurnTakerDecision.SelectedTurnTaker.PlayArea, toBottom: false, showMessage: true));
                yield break;
            }
            coroutine = base.GameController.SendMessageAction("No viable play locations. Putting this card in the trash", Priority.Low, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            destination.Add(new MoveCardDestination(base.TurnTaker.Trash, toBottom: false, showMessage: true));
        }

        public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cardController)
        {
            // For ease of use, this card's power is accessed by clicking Node's character card, not this card
            //Log.Debug("PsychicLinkCardController.AskIfContributesPowersToCardController activated for " + cardController.Card.Title);
            if (base.TurnTakerController.CharacterCardControllers.Any((CharacterCardController cc) => cc == cardController))
            {
                //Log.Debug("PsychicLinkCardController.AskIfContributesPowersToCardController: returning granted power");
                return new Power[] { new Power(base.HeroTurnTakerController, cardController, PowerDescription(), UseGrantedPower(), 0, null, GetCardSource()) };
            }
            else
            {
                //Log.Debug("PsychicLinkCardController.AskIfContributesPowersToCardController: returning none");
                return null;
            }
        }

        private string PowerDescription()
        {
            return "Return " + base.Card.Title + " from " + base.Card.Location.GetFriendlyName() + " to your hand.";
        }

        private IEnumerator IncreaseResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(DamageThisTurn, gameAction: dda);
            // "... increase that damage by 1."
            IEnumerator increaseCoroutine = base.GameController.IncreaseDamage(dda, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(increaseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(increaseCoroutine);
            }
        }

        public IEnumerator UseGrantedPower()
        {
            // "Return this card to your hand."
            IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.ToHero().Hand, showMessage: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
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
