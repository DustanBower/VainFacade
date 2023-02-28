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
    public class DrWendigoCardController : CardController
    {
        public DrWendigoCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show target with the second highest HP other than this card
            SpecialStringMaker.ShowHighestHP(2, cardCriteria: new LinqCardCriteria((Card c) => c != base.Card, "other than " + base.Card.Title, false, true, "target", "targets"));
        }

        public static readonly string TokenPoolIdentifier = "DrWendigoPool";

        private TokenPool DrWendigoTokenPool()
        {
            return base.Card.FindTokenPool(TokenPoolIdentifier);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt to {DrWendigo} by 1."
            AddReduceDamageTrigger((Card c) => c == base.Card, 1);
            // "When {DrWendigo} deals damage, add a token to this card."
            AddTrigger((DealDamageAction dda) => dda.DidDealDamage && dda.DamageSource.IsSameCard(base.Card), (DealDamageAction dda) => base.GameController.AddTokensToPool(DrWendigoTokenPool(), 1, GetCardSource()), TriggerType.AddTokensToPool, TriggerTiming.After);
            // "At the end of the environment turn, {DrWendigo} deals the target other than himself with the second highest HP X melee damage, where X = 2 plus the number of tokens on this card."
            AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => c != base.Card, TargetType.HighestHP, 0, DamageType.Melee, highestLowestRanking: 2, dynamicAmount: (Card c) => 2 + DrWendigoTokenPool().CurrentValue);
            // When destroyed: reset number of tokens
            AddWhenDestroyedTrigger(ResetTokensResponse, TriggerType.Hidden);
        }

        private IEnumerator ResetTokensResponse(GameAction ga)
        {
            DrWendigoTokenPool().SetToInitialValue();
            yield return null;
        }
    }
}
