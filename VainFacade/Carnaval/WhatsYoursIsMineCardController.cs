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
    public class WhatsYoursIsMineCardController : CardController
    {
        public WhatsYoursIsMineCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If this card is in play, make sure the player can tell where it is...
            SpecialStringMaker.ShowSpecialString(() => "This card is in " + base.Card.Location.GetFriendlyName() + ".").Condition = () => base.Card.IsInPlayAndHasGameText;
            // ... and who it's going to hit when it goes off
            SpecialStringMaker.ShowListOfCardsAtLocationOfCard(base.Card, new LinqCardCriteria((Card c) => c.IsTarget, "target", false, false, "target", "targets")).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When another card enters play in this area, destroy this card."
            AddTrigger((CardEntersPlayAction cepa) => cepa.CardEnteringPlay.IsAtLocationRecursive(base.Card.Location.HighestRecursiveLocation) && cepa.CardEnteringPlay != base.Card, DestroyThisCardResponse, TriggerType.DestroySelf, TriggerTiming.After);
            // "When this card is destroyed, {CarnavalCharacter} deals each target in this play area 2 fire and 2 projectile damage."
            AddWhenDestroyedTrigger(ExplodeResponse, TriggerType.DealDamage);
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> destination, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card in another play area."
            // Adapted from SergeantSteelTeam.MissionObjectiveCardController
            List<SelectTurnTakerDecision> storedResults = new List<SelectTurnTakerDecision>();
            IEnumerator coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.MoveCardToPlayArea, storedResults, optional: false, allowAutoDecide: false, (TurnTaker tt) => tt != base.TurnTaker, null, null, checkExtraTurnTakersInstead: false, canBeCancelled: true, ignoreBattleZone: false, GetCardSource());
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

        private IEnumerator ExplodeResponse(DestroyCardAction dca)
        {
            // "... {CarnavalCharacter} deals each target in this play area 2 fire and 2 projectile damage."
            List<DealDamageAction> instances = new List<DealDamageAction>();
            DealDamageAction twoFire = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 2, DamageType.Fire);
            DealDamageAction twoProjectile = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 2, DamageType.Projectile);
            instances.Add(twoFire);
            instances.Add(twoProjectile);
            IEnumerator damageCoroutine = DealMultipleInstancesOfDamage(instances, (Card c) => c.IsTarget && c.IsAtLocationRecursive(base.Card.Location.HighestRecursiveLocation));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
        }
    }
}
