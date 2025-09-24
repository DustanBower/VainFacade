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
            SpecialStringMaker.ShowHighestHP(2, cardCriteria: new LinqCardCriteria((Card c) => !c.DoKeywordsContain(ConspiratorKeyword), "non-Conspirator", singular: "target", plural: "targets"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            //Reduce damage dealt to this card by 1.
            AddReduceDamageTrigger((Card c) => c == this.Card, 1);
            // "At the start of the environment turn, this card deals the non-Conspirator target with the second highest HP X projectile damage, where X = the current HP of this card."
            AddDealDamageAtStartOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => !c.DoKeywordsContain(ConspiratorKeyword), TargetType.HighestHP, 2, DamageType.Projectile, highestLowestRanking: 2, dynamicAmount: (Card c) => base.Card.HitPoints);
        }
    }
}
