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
    public class NoMrRookeIExpectYouToDieCardController : ParadiseIsleUtilityCardController
    {
        public NoMrRookeIExpectYouToDieCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero target with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // At the start of the environment turn, this card deals the hero target with the highest HP {H + 2} irreducible energy damage.
            AddDealDamageAtStartOfTurnTrigger(this.TurnTaker, this.Card, (Card c) => IsHeroTarget(c), TargetType.HighestHP, H + 2, DamageType.Energy, true);
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }
    }
}
