using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.EldrenwoodVillage
{
    public class SamRichardsCardController : AfflictedCardController
    {
        public SamRichardsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, one player discards or plays a card. If a card entered play this way, this card deals one hero 2 sonic damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker && CanActivateEffect(base.TurnTakerController, QuaintKey), SelectPlayerCheckResultsResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PlayCard, TriggerType.DealDamage });
        }

        private IEnumerator SelectPlayerCheckResultsResponse(PhaseChangeAction pca)
        {
            // "... one player discards or plays a card."
            List<PlayCardAction> plays = new List<PlayCardAction>();
            SelectTurnTakerDecision selection = new SelectTurnTakerDecision(base.GameController, DecisionMaker, GameController.FindTurnTakersWhere((TurnTaker tt) => tt.IsHero && tt.ToHero().HasCardsInHand && GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource())), SelectionType.Custom, cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakerAndDoAction(selection, (TurnTaker tt) => DiscardOrPlayResponse(tt, plays));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            // "If a card entered play this way, this card deals one hero 2 sonic damage."
            if (plays.Any((PlayCardAction playing) => playing.WasCardPlayed))
            {
                IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.Card), 2, DamageType.Sonic, 1, false, 1, additionalCriteria: (Card c) => c.IsHeroCharacterCard, cardSource: GetCardSource());
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

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Select a player to discard or play a card", "choosing a player to discard or play a card", "Vote for a player to discard or play a card", "player to discard or play a card");
        }

        private IEnumerator DiscardOrPlayResponse(TurnTaker tt, List<PlayCardAction> playResults)
        {
            // "... discards or plays a card."
            HeroTurnTakerController hero = base.GameController.FindHeroTurnTakerController(tt.ToHero());
            List<Function> options = new List<Function>();
            options.Add(new Function(hero, "Discard a card", SelectionType.DiscardCard, () => base.GameController.SelectAndDiscardCard(hero, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource()), onlyDisplayIfTrue: hero.HasCardsInHand, forcedActionMessage: hero.Name + " cannot play any cards, so they must discard a card."));
            options.Add(new Function(hero, "Play a card", SelectionType.PlayCard, () => base.GameController.SelectAndPlayCardFromHand(hero, false, playResults, cardSource: GetCardSource())));
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, hero, options, false, noSelectableFunctionMessage: hero.Name + " cannot discard or play any cards.", cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
        }

        public override IEnumerator SlainInHumanFormResponse()
        {
            // "... destroy one hero Equipment card."
            yield return base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsHero && IsEquipment(c), "hero Equipment"), false, responsibleCard: base.Card, cardSource: GetCardSource());
        }
    }
}
