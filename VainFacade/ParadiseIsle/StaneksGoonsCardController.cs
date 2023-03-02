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
            // Show whether there is a Conspirator in play with higher HP than this card
            SpecialStringMaker.ShowIfElseSpecialString(HasTougherBackup, () => "There is a Conspirator in play with more HP than " + base.Card.Title + ".", () => "There are no Conspirators in play with more HP than " + base.Card.Title + ".");
            // Show 2 non-Conspirator targets with lowest HP
            SpecialStringMaker.ShowLowestHP(numberOfTargets: () => 2, cardCriteria: new LinqCardCriteria((Card c) => !c.DoKeywordsContain(ConspiratorKeyword), "non-Conspirator"));
        }

        private bool HasTougherBackup()
        {
            return FindCardsWhere((Card c) => c.DoKeywordsContain(ConspiratorKeyword) && c.IsTarget && c.HitPoints.HasValue && base.Card.HitPoints.HasValue && c.HitPoints.Value > base.Card.HitPoints.Value).Any();
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase damage dealt by this card by 1 if there is a Conspirator in play with higher HP than this card."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource.IsSameCard(base.Card) && HasTougherBackup(), 1);
            // "At the end of the environment turn, this card deals the 2 non-Conspirator targets with the lowest HP 2 projectile damage each."
            AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => !c.DoKeywordsContain(ConspiratorKeyword), TargetType.LowestHP, 2, DamageType.Projectile, numberOfTargets: 2);
        }
    }
}
