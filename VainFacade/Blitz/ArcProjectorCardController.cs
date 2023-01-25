using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Blitz
{
    public class ArcProjectorCardController : CardController
    {
        public ArcProjectorCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show list of hero targets with highest HP with length equal to number of cards under this card
            SpecialStringMaker.ShowHighestHP(1, () => base.Card.UnderLocation.NumberOfCards, new LinqCardCriteria((Card c) => c.IsHero && c.IsTarget, "hero", singular: "target", plural: "targets")).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {BlitzCharacter} is dealt lightning damage, put 1 card from the villain trash beneath this card for each point of damage dealt."
            AddTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && dda.DamageType == DamageType.Lightning && dda.DidDealDamage, (DealDamageAction dda) => base.GameController.MoveCards(base.TurnTakerController, base.TurnTaker.Trash.GetTopCards(dda.Amount), base.Card.UnderLocation, playIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource()), TriggerType.MoveCard, TriggerTiming.After);
            // "At the end of the villain turn, {BlitzCharacter} deals the X hero targets with the highest HP {H - 2} lightning damage, where X = the number of cards beneath this card. When a target is dealt damage this way, discard a card from beneath this card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DamageDiscardResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DiscardCard });
        }

        private IEnumerator DamageDiscardResponse(PhaseChangeAction pca)
        {
            // "... {BlitzCharacter} deals the X hero targets with the highest HP {H - 2} lightning damage, where X = the number of cards beneath this card."
            // "When a target is dealt damage this way, discard a card from beneath this card."
            ITrigger discardTrigger = AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card == base.CharacterCard && dda.CardSource.Card == base.Card && dda.DidDealDamage, (DealDamageAction dda) => base.GameController.DiscardTopCard(base.Card.UnderLocation, null, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource()), TriggerType.DiscardCard, TriggerTiming.After);
            IEnumerator damageCoroutine = DealDamageToHighestHP(base.CharacterCard, 1, (Card c) => c.IsHero && c.IsTarget, (Card c) => H - 2, DamageType.Lightning, numberOfTargets: () => base.Card.UnderLocation.NumberOfCards);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            RemoveTrigger(discardTrigger);
        }
    }
}
