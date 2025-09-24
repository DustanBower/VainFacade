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
    public class StaneksGoonsCardController : ParadiseIsleUtilityCardController
    {
        public StaneksGoonsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show 2 non-conspiratr targets with lowest HP
            SpecialStringMaker.ShowLowestHP(numberOfTargets: () => 2, cardCriteria: isNonConspirator());
            //Show number of conspirators in play
            base.SpecialStringMaker.ShowNumberOfCardsInPlay(isConspirator());
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, this card deals the 2 targets with the lowest HP other than this card X projectile damage each, where X is the number of conspirators in play."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, TriggerType.DealDamage);
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            return DealDamageToLowestHP(this.Card, 1, (Card c) => !base.GameController.DoesCardContainKeyword(c,ConspiratorKeyword), (Card c) => FindCardsWhere((Card cc) => cc.IsInPlayAndHasGameText && base.GameController.DoesCardContainKeyword(cc, ConspiratorKeyword)).Count(), DamageType.Projectile, numberOfTargets: 2);
        }
    }
}
