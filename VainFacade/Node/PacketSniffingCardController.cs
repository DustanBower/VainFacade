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
    public class PacketSniffingCardController : ConnectionCardController
    {
        public PacketSniffingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether a card has been played in this play area this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(PlayedThisTurn, "A card has already entered play in " + base.Card.Location.GetFriendlyName() + " this turn since " + base.Card.Title + " entered play.", "No cards have entered play in " + base.Card.Location.GetFriendlyName() + " this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected readonly string PlayedThisTurn = "FirstCardPlayedThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn a card enters play in this area, you may draw a card."
            AddTrigger((CardEntersPlayAction cepa) => !HasBeenSetToTrueThisTurn(PlayedThisTurn) && cepa.CardEnteringPlay.IsInLocation(base.Card.Location.HighestRecursiveLocation), DrawResponse, TriggerType.DrawCard, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(PlayedThisTurn), TriggerType.Hidden);
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

        private IEnumerator DrawResponse(CardEntersPlayAction cepa)
        {
            SetCardPropertyToTrueIfRealAction(PlayedThisTurn);
            // "... you may draw a card."
            IEnumerator drawCoroutine = DrawCard(base.HeroTurnTaker, optional: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
        }
    }
}
