using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class TheyOweYouCardController:ProclamationCardController
	{
		public TheyOweYouCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHeroWithFewestCards(false).Condition = () => !this.Card.IsInPlayAndHasGameText;
		}

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            //Play this card next to the hero with the fewest cards in play.
            List<TurnTaker> results = new List<TurnTaker>();
            IEnumerator coroutine = FindHeroWithFewestCardsInPlay(results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (results.FirstOrDefault() != null)
            {
                TurnTaker hero = results.FirstOrDefault();
                List<Card> selected = new List<Card>();
                coroutine = FindCharacterCard(hero, SelectionType.MoveCardNextToCard, selected);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                if (selected.FirstOrDefault() != null)
                {
                    storedResults.Add(new MoveCardDestination(selected.FirstOrDefault().NextToLocation));
                }
            }
        }

        public override void AddTriggers()
        {
            //At the end of that hero’s turn, that hero regains 1 hp, then deals each other hero target 1 psychic damage.
            AddEndOfTurnTrigger((TurnTaker tt) => GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().IsHeroCharacterCard && tt == GetCardThisCardIsNextTo().Owner, EndOfTurnResponse, new TriggerType[2] { TriggerType.GainHP, TriggerType.DealDamage });
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            Card hero = GetCardThisCardIsNextTo();
            IEnumerator coroutine = base.GameController.GainHP(hero, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = DealDamage(hero, (Card c) => IsHeroTarget(c) && c != hero, 1, DamageType.Psychic);
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

