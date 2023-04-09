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
    public class ConvincingDoubleCardController : CardController
    {
        public ConvincingDoubleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If this card is in play, make sure the player can tell where it is...
            SpecialStringMaker.ShowSpecialString(() => "This card is in " + base.Card.Location.GetFriendlyName() + ".").Condition = () => base.Card.IsInPlayAndHasGameText;
            // ... and who it's protecting.
            SpecialStringMaker.ShowListOfCardsAtLocationOfCard(base.Card, new LinqCardCriteria((Card c) => IsHeroCharacterCard(c), "", false, false, "hero", "heroes")).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> destination, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card in a hero play area."
            // Adapted from SergeantSteelTeam.MissionObjectiveCardController
            List<SelectTurnTakerDecision> storedResults = new List<SelectTurnTakerDecision>();
            IEnumerator coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.MoveCardToPlayArea, storedResults, optional: false, allowAutoDecide: false, (TurnTaker tt) => IsHero(tt), null, null, checkExtraTurnTakersInstead: false, canBeCancelled: true, ignoreBattleZone: false, GetCardSource());
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

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a hero in this area would be dealt damage, redirect that damage to this card."
            AddRedirectDamageTrigger((DealDamageAction dda) => IsHeroCharacterCard(dda.Target) && dda.Target.IsAtLocationRecursive(base.Card.Location.HighestRecursiveLocation), () => base.Card);
        }
    }
}
