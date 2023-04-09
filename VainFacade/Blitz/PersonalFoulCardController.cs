using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Blitz
{
    public class PersonalFoulCardController : CardController
    {
        public PersonalFoulCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show the hero target with the highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
        }

        public override IEnumerator Play()
        {
            // "{BlitzCharacter} deals the hero target with the highest HP {H + 1} lightning damage."
            IEnumerator damageCoroutine = DealDamageToHighestHP(base.CharacterCard, 1, (Card c) => IsHeroTarget(c), (Card c) => H + 1, DamageType.Lightning);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "Destroy {H - 2} hero Ongoing and/or Equipment cards."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => IsHero(c) && (IsOngoing(c) || IsEquipment(c)), "hero Ongoing or Equipment"), H - 2, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
        }
    }
}
