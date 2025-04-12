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
    public class TheBlackDogCardController : TheMidnightBazaarUtilityCardController
    {
        public TheBlackDogCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHighestHP(2, () => 1, new LinqCardCriteria((Card c) => c != this.Card, $"target other than {this.Card.Title}"));
        }

        public override void AddTriggers()
        {
            //Reduce all HP recovery by 1
            AddTrigger<GainHPAction>((GainHPAction hp) => true, (GainHPAction hp) => base.GameController.ReduceHPGain(hp, 1, GetCardSource()), TriggerType.ReduceHPGain, TriggerTiming.Before);

            //At the end of the environment turn, this card deals the target other than itself with the second highest HP 2 irreducible infernal damage.
            //If 3 or more damage is dealt this way, destroy an ongoing or equipment card from that target's deck.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroyCard });
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            List<DealDamageAction> results = new List<DealDamageAction>();
            IEnumerator coroutine = DealDamageToHighestHP(this.Card, 2, (Card c) => c != this.Card, (Card c) => 2, DamageType.Infernal, true, storedResults: results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }



            if (DidDealDamage(results))
            {
                int amount = results.Where((DealDamageAction dd) => dd.DidDealDamage).Select((DealDamageAction dd) => dd.Amount).Sum();

                if (amount >= 3)
                {
                    Card damaged = results.FirstOrDefault().Target;
                    TurnTaker damagedOwner = damaged.Owner;
                    HeroTurnTakerController decisionMaker = null;
                    if (damagedOwner.IsPlayer)
                    {
                        //If a hero target was dealt damage, have that card's player decide what to destroy
                        decisionMaker = FindHeroTurnTakerController(damagedOwner.ToHero());
                    }

                    coroutine = base.GameController.SelectAndDestroyCard(decisionMaker, new LinqCardCriteria((Card c) => (IsOngoing(c) || IsEquipment(c)) && c.Owner == damagedOwner, "ongoing or equipment"), false, responsibleCard: this.Card, cardSource: GetCardSource());
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
    }
}
