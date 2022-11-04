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
    public class CollectiveUnconsciousCardController : ConnectionCardController
    {
        public CollectiveUnconsciousCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {NodeCharacter} or that hero uses a power, the other gains 1 HP."
            AddTrigger((UsePowerAction upa) => IsRelevantPower(upa), ExchangeHealResponse, TriggerType.GainHP, TriggerTiming.After);
            // "When {NodeCharacter} would deal that hero psychic damage, that hero recovers that much HP instead."
            AddPreventDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card == base.CharacterCard && dda.Target.IsHeroCharacterCard && dda.Target.Owner == base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker && dda.CanDealDamage && dda.DamageType == DamageType.Psychic, (DealDamageAction dda) => base.GameController.GainHP(dda.Target, dda.Amount, cardSource: GetCardSource()), new TriggerType[] { TriggerType.GainHP }, isPreventEffect: true);
        }

        private bool IsRelevantPower(UsePowerAction upa)
        {
            if (base.Card.Location.HighestRecursiveLocation.IsPlayArea && base.Card.Location.HighestRecursiveLocation.IsHero)
            {
                TurnTakerController areaOwner = FindTurnTakerController(base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker);
                if (upa.Power.TurnTakerController == areaOwner || upa.Power.TurnTakerController == base.TurnTakerController)
                {
                    return true;
                }
            }
            return false;
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> destination, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card in another hero's play area."
            // Adapted from SergeantSteelTeam.MissionObjectiveCardController
            List<SelectTurnTakerDecision> storedResults = new List<SelectTurnTakerDecision>();
            IEnumerator coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.MoveCardToPlayArea, storedResults, optional: false, allowAutoDecide: false, (TurnTaker tt) => tt.IsHero && tt != base.TurnTaker, null, null, checkExtraTurnTakersInstead: false, canBeCancelled: true, ignoreBattleZone: false, GetCardSource());
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

        private IEnumerator ExchangeHealResponse(UsePowerAction upa)
        {
            // "... the other gains 1 HP."
            Card user = upa.HeroCharacterCardUsingPower.Card;
            IEnumerator healCoroutine = base.GameController.GainHP(base.CharacterCard, 1, cardSource: GetCardSource());
            if (user == base.CharacterCard)
            {
                // Node used a power, so a hero in this play area heals
                healCoroutine = base.GameController.SelectAndGainHP(DecisionMaker, 1, additionalCriteria: (Card c) => c.IsHeroCharacterCard && c.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation, requiredDecisions: 1, cardSource: GetCardSource());
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
        }
    }
}
