using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheFury
{
    public class BrokenBondsCardController : CardController
    {
        public BrokenBondsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase damage dealt to and by {TheFuryCharacter} by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard, 1);
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card == base.CharacterCard, 1);
            // "When this card is destroyed, you may play a card or use a power."
            AddWhenDestroyedTrigger(PlayOrPowerResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.UsePower });
            // "At the start of your turn, you may destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => base.GameController.DestroyCard(DecisionMaker, base.Card, optional: true, responsibleCard: base.Card, cardSource: GetCardSource()), TriggerType.DestroySelf);
        }

        private IEnumerator PlayOrPowerResponse(DestroyCardAction dca)
        {
            // "... you may play a card or use a power."
            List<Function> options = new List<Function>();
            options.Add(new Function(DecisionMaker, "Play a card", SelectionType.PlayCard, () => SelectAndPlayCardFromHand(DecisionMaker, optional: true), CanPlayCardsFromHand(DecisionMaker), base.TurnTaker.Name + " cannot use any powers, so they must play a card."));
            options.Add(new Function(DecisionMaker, "Use a power", SelectionType.UsePower, () => base.GameController.SelectAndUsePower(DecisionMaker, optional: true, cardSource: GetCardSource()), base.GameController.CanUsePowers(DecisionMaker, GetCardSource()), base.TurnTaker.Name + " cannot play any cards, so they must use a power."));
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, DecisionMaker, options, true, noSelectableFunctionMessage: base.TurnTaker.Name + " cannot currently play cards or use powers.", cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice, associatedCards: base.Card.ToEnumerable());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
        }
    }
}
