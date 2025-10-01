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
    public class BombshellCardController : ParadiseIsleUtilityCardController
    {
        public BombshellCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // At the start of the environment turn, Bombshell deals each non-conspirator target 2 fire damage
            AddDealDamageAtStartOfTurnTrigger(this.TurnTaker, this.Card, (Card c) => !base.GameController.DoesCardContainKeyword(c,"conspirator"), TargetType.All, 2, DamageType.Fire);
        }
    }
}
