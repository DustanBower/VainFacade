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
    public class MockingVisageCardController : MasqueCardController
    {
        public MockingVisageCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: remind player to click on Carnaval's character card to use this card's power
            SpecialStringMaker.ShowSpecialString(() => "Click on " + base.TurnTaker.Name + "'s hero character card to use this power.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            base.AddAsPowerContributor();
            AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(false);
            // "Increase damage dealt to that target by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => IsThisCardNextToCard(dda.Target), 1);
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card next to a target."
            IEnumerator selectCoroutine = SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => c.IsTarget, "target"), storedResults, isPutIntoPlay, decisionSources);
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
            return "Each target in " + base.Card.Location.HighestRecursiveLocation.GetFriendlyName() + " deals itself 1 psychic damage.";
        }

        private IEnumerator UseGrantedPower()
        {
            int damageAmt = GetPowerNumeral(0, 1);
            // "Each target in this card's play area deals itself 1 psychic damage."
            IEnumerator psychicCoroutine = base.GameController.DealDamageToSelf(DecisionMaker, (Card c) => c.IsAtLocationRecursive(base.Card.Location.HighestRecursiveLocation), damageAmt, DamageType.Psychic, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(psychicCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(psychicCoroutine);
            }
        }
    }
}
