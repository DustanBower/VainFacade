using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class VillainShowdownCopyCardController : CardController
    {
        public VillainShowdownCopyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            SpecialStringMaker.ShowHeroTargetWithHighestHP(1);
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "This card is indestructible while it has 1 or more HP."
            if (card == base.Card)
            {
                return base.Card.HitPoints >= 1;
            }
            return base.AskIfCardIsIndestructible(card);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, this card deals the hero target with the highest HP {H + 1} melee damage."
            AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => c.IsHero, TargetType.HighestHP, H + 1, DamageType.Melee);
            // After this card leaves play, swap it with the original Villain Showdown
            AddAfterLeavesPlayAction(() => base.GameController.SwitchCards(base.Card, FindCard("VillainShowdown"), cardSource: GetCardSource()));
        }
    }
}
