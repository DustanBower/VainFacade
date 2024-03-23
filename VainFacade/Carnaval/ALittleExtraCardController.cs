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
    public class ALittleExtraCardController : CardController
    {
        public ALittleExtraCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: remind player to click on Carnaval's character card to use this card's power
            SpecialStringMaker.ShowSpecialString(() => "Click on " + base.TurnTaker.Name + "'s hero character card to use this power.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            base.AddAsPowerContributor();
            AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(true);
            // "When that card is destroyed, you may destroy up to 2 non-character, non-target cards or {CarnavalCharacter} may deal each non-hero target 1 fire damage and 1 projectile damage."
            AddTrigger((DestroyCardAction dca) => IsThisCardNextToCard(dca.CardToDestroy.Card), ExplodeResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroyCard }, TriggerTiming.After);
        }

        private IEnumerator ExplodeResponse(DestroyCardAction dca)
        {
            // "... you may destroy up to 2 non-character, non-target cards or {CarnavalCharacter} may deal each non-hero target 1 fire damage and 1 projectile damage."
            List<DealDamageAction> instances = new List<DealDamageAction>();
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 1, DamageType.Fire));
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 1, DamageType.Projectile));
            List<Function> options = new List<Function>();
            options.Add(new Function(DecisionMaker, "Destroy up to 2 non-character non-target cards", SelectionType.DestroyCard, () => base.GameController.SelectAndDestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => !c.IsCharacter && !c.IsTarget, "non-character non-target"), 2, requiredDecisions: 0, responsibleCard: base.Card, cardSource: GetCardSource())));
            options.Add(new Function(DecisionMaker, base.CharacterCard.Title + " deals each non-hero target 1 fire damage and 1 projectile damage", SelectionType.DealDamage, () => DealMultipleInstancesOfDamage(instances, (Card c) => c.IsTarget && !IsHeroTarget(c))));
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, DecisionMaker, options, true, cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card next to a non-character, non-target card."
            IEnumerator selectCoroutine = SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => !c.IsCharacter && !c.IsTarget && !c.IsOneShot && c.IsInPlay, "non-character non-target"), storedResults, isPutIntoPlay, decisionSources);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }

        public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cardController)
        {
            // For ease of use, this card's power is accessed by clicking Carnaval's character card, not this card
            if (base.TurnTakerController.CharacterCardControllers.Any((CharacterCardController cc) => cc == cardController))
            {
                return new Power[] { new Power(base.HeroTurnTakerController, cardController, PowerDescription(), UseGrantedPower(), 0, null, GetCardSource()) };
            }
            else
            {
                return null;
            }
        }

        private string PowerDescription()
        {
            return "Destroy a hero Ongoing or Equipment card.";
        }

        private IEnumerator UseGrantedPower()
        {
            // "Destroy a hero Ongoing or Equipment card."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsHero(c) && (IsOngoing(c) || IsEquipment(c)), "hero Ongoing or Equipment"), false, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
        }
    }
}
