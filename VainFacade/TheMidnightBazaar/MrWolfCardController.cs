using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheMidnightBazaar
{
    public class MrWolfCardController : TheMidnightBazaarUtilityCardController
    {
        public MrWolfCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show whether The Blinded Queen is in play
            SpecialStringMaker.ShowIfSpecificCardIsInPlay(BlindedQueenIdentifier);
            // Show target with lowest HP other than this card
            SpecialStringMaker.ShowLowestHP(1, () => 1, cardCriteria: new LinqCardCriteria((Card c) => c != base.Card, "other than " + base.Card.Title, false, true, "target", "targets"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // This card cannot deal damage if The Blinded Queen is in play
            AddCannotDealDamageTrigger((Card c) => c == this.Card && IsBlindedQueenInPlay());
            //At the end of the environemnt turn, increase the next damage dealt by this card by 1...
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, IncreaseNextDamage, TriggerType.CreateStatusEffect);
            //...then this card deals the target with the lowest HP other than itself {H} melee damage."
            AddDealDamageAtEndOfTurnTrigger(TurnTaker, base.Card, (Card c) => c.IsTarget && c != base.Card, TargetType.LowestHP, H, DamageType.Melee);
        }

        private IEnumerator IncreaseNextDamage(PhaseChangeAction pca)
        {
            IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(1);
            effect.NumberOfUses = 1;
            effect.SourceCriteria.IsSpecificCard = this.Card;
            effect.TargetLeavesPlayExpiryCriteria.Card = this.Card;
            IEnumerator coroutine = AddStatusEffect(effect);
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
