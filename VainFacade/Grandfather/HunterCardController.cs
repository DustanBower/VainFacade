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
    public class HunterCardController : CardController
    {
        public HunterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show the hero target with the lowest HP
            SpecialStringMaker.ShowHeroTargetWithLowestHP(ranking: 1);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, this card deals the hero target with the lowest HP {H - 2} melee damage."
            AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => c.IsHero && c.IsTarget, TargetType.LowestHP, H - 2, DamageType.Melee);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, increase damage dealt by this card by 2 this turn."
            IncreaseDamageStatusEffect buff = new IncreaseDamageStatusEffect(2);
            buff.SourceCriteria.IsSpecificCard = base.Card;
            buff.UntilThisTurnIsOver(base.Game);
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(buff, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
        }
    }
}
