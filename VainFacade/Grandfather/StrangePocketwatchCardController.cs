using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class StrangePocketwatchCardController : CardController
    {
        public StrangePocketwatchCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play with game text: show whether a card has entered play from the environment deck this turn yet
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstEnvPlay, "A card has already entered play from the environment deck this turn since " + base.Card.Title + " entered play.", "No cards have entered play from the environment deck this turn since " + base.Card.Title + " entered play.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected const string FirstEnvPlay = "FirstEnvironmentPlayThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time each turn a card enters play from the top of the environment deck, play the top card of the villain deck."
            AddTrigger((CardEntersPlayAction cepa) => !HasBeenSetToTrueThisTurn(FirstEnvPlay) && EnteredPlayFromEnvironmentDeck(cepa), PlayVillainCardOnceResponse, TriggerType.PlayCard, TriggerTiming.After);
            ResetFlagAfterLeavesPlay(FirstEnvPlay);
        }

        protected bool EnteredPlayFromEnvironmentDeck(CardEntersPlayAction cepa)
        {
            MoveCardJournalEntry playMove = (from mc in base.Journal.MoveCardEntriesThisTurn()
                                          where mc.Card == cepa.CardEnteringPlay && mc.ToLocation.IsPlayArea
                                          select mc).LastOrDefault();
            bool result = false;
            if (playMove != null && playMove.FromLocation.IsEnvironment)
            {
                if (playMove.FromLocation.IsDeck)
                {
                    result = true;
                }
                else if (playMove.FromLocation.IsRevealed)
                {
                    MoveCardJournalEntry previousMove = (from mc in base.Journal.MoveCardEntriesThisTurn()
                                                                 where mc.Card == cepa.CardEnteringPlay && mc.ToLocation.IsRevealed
                                                                 select mc).LastOrDefault();
                    if (previousMove != null && previousMove.FromLocation.IsDeck)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        private IEnumerator PlayVillainCardOnceResponse(CardEntersPlayAction cepa)
        {
            SetCardPropertyToTrueIfRealAction(FirstEnvPlay);
            IEnumerator playCoroutine = PlayTheTopCardOfTheVillainDeckResponse(cepa);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }
    }
}
