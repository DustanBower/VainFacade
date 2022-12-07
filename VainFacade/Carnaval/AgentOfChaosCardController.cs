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
    public class AgentOfChaosCardController : CardController
    {
        public AgentOfChaosCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "Play the top card of each other deck, in turn order, beginning with the environment deck."
            IEnumerable<Location> allDecks = FindLocationsWhere((Location l) => !l.OwnerTurnTaker.IsIncapacitatedOrOutOfGame && l.IsDeck && l.IsRealDeck && GameController.IsTurnTakerVisibleToCardSource(l.OwnerTurnTaker, GetCardSource()) && GameController.IsLocationVisibleToSource(l, GetCardSource()));
            TurnTakerController[] controllers = GameController.FindTurnTakerControllersWhere((TurnTakerController ttc) => true, cardSource: GetCardSource()).ToArray();
            int numControllers = controllers.Count();
            if (controllers.IndexOf(GameController.FindEnvironmentTurnTakerController()).HasValue)
            {
                int envIndex = controllers.IndexOf(GameController.FindEnvironmentTurnTakerController()).Value;
                for (int i = 0; i < numControllers; i++)
                {
                    int j = (i + envIndex) % numControllers;
                    TurnTakerController ttc = controllers.ElementAt(j);
                    if (ttc.IsIncapacitatedOrOutOfGame || !allDecks.Any((Location l) => l.OwnerTurnTaker == ttc.TurnTaker))
                    {
                        continue;
                    }
                    IEnumerable<Location> ttcDecks = allDecks.Where((Location l) => ttc.TurnTaker.Decks.Contains(l));
                    List<Location> selectedDecks = new List<Location>();
                    for (int k = 0; k < ttcDecks.Count(); k++)
                    {
                        List<SelectLocationDecision> choices = new List<SelectLocationDecision>();
                        IEnumerator selectCoroutine = base.GameController.SelectADeck(DecisionMaker, SelectionType.PlayTopCard, (Location l) => ttcDecks.Contains(l) && !selectedDecks.Contains(l), choices, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(selectCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(selectCoroutine);
                        }
                        Location selected = GetSelectedLocation(choices);
                        selectedDecks.Add(selected);
                        IEnumerator playCoroutine = base.GameController.PlayTopCard(DecisionMaker, ttc, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
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
        }
    }
}
