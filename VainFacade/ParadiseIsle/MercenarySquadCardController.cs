using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class MercenarySquadCardController : ParadiseIsleUtilityCardController
    {
        public MercenarySquadCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show the non-Conspirator target with the second highest HP
            SpecialStringMaker.ShowHighestHP(2, cardCriteria: new LinqCardCriteria((Card c) => !c.DoKeywordsContain(ConspiratorKeyword), "non-Conspirator"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When this card would be dealt damage, discard a card beneath it. If you do, prevent that damage."
            AddTrigger((DealDamageAction dda) => dda.Target == base.Card && dda.Amount > 0 && base.Card.UnderLocation.Cards.Count() > 0, (DealDamageAction dda) => MoveUnderCardToTrashToPreventDamage(base.Card, dda), new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.CancelAction, TriggerType.MoveCard }, TriggerTiming.Before);
            // "At the start of the environment turn, this card deals the non-Conspirator target with the second highest HP X projectile damage, where X = 2 plus the number of cards beneath this one."
            AddDealDamageAtStartOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => !c.DoKeywordsContain(ConspiratorKeyword), TargetType.HighestHP, 2, DamageType.Projectile, highestLowestRanking: 2, dynamicAmount: (Card c) => 2 + base.Card.UnderLocation.Cards.Count());
            // When this leaves play, move cards under it to its trash
            AddBeforeLeavesPlayActions(MoveCardsUnderThisCardToTrash);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, place the top {H} cards of the environment deck beneath it."
            IEnumerator moveCoroutine = base.GameController.MoveCards(base.TurnTakerController, base.TurnTaker.Deck.GetTopCards(H), base.Card.UnderLocation, playIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
        }

        private IEnumerator MoveCardsUnderThisCardToTrash(GameAction ga)
        {
            IEnumerator coroutine = base.GameController.MoveCards(base.TurnTakerController, base.Card.UnderLocation.Cards, base.TurnTaker.Trash, toBottom: false, isPutIntoPlay: false, playIfMovingToPlayArea: true, null, showIndividualMessages: false, isDiscard: false, null, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }
    }
}
