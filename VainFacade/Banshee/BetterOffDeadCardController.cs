using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Banshee
{
	public class BetterOffDeadCardController:CardController
	{
		public BetterOffDeadCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
		}

        private bool ReactingToIncap;

        public override void AddTriggers()
        {
            //When {Banshee} would be incapacitated, each player may first play a card.
            AddTrigger<DestroyCardAction>((DestroyCardAction dc) => dc.CardToDestroy.Card == this.CharacterCard, IncapResponse, TriggerType.PlayCard, TriggerTiming.Before);
        }

        private IEnumerator IncapResponse(DestroyCardAction dc)
        {
            IEnumerator coroutine;
            if (!ReactingToIncap)
            {
                ReactingToIncap = true;
                coroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsPlayer && CanPlayCards(FindTurnTakerController(tt)) && tt.ToHero().HasCardsInHand), SelectionType.PlayCard, (TurnTaker tt) => SelectAndPlayCardFromHand(FindHeroTurnTakerController(tt.ToHero())), requiredDecisions: 0, allowAutoDecide: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                ReactingToIncap = false;
            }
            else
            {
                coroutine = base.GameController.CancelAction(dc, false, cardSource: GetCardSource());
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

        public override IEnumerator UsePower(int index = 0)
        {
            //Draw a card.
            IEnumerator coroutine = DrawCard();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //1 incapacitated hero activates an ability.
            SelectTurnTakersDecision decision = new SelectTurnTakersDecision(base.GameController, DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsPlayer && (tt.IsIncapacitated || tt.PlayArea.Cards.Any((Card c) => c.IsHeroCharacterCard && c.IsIncapacitated)),"incapacitated heroes"), SelectionType.UseIncapacitatedAbility, 1, false, 1, cardSource:GetCardSource());
            coroutine = base.GameController.SelectTurnTakersAndDoAction(decision, (TurnTaker hero) => base.GameController.SelectIncapacitatedHeroAndUseAbility(FindHeroTurnTakerController(hero.ToHero())));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Then, if {Banshee} has 10 or fewer hp, you may play a card.
            if (this.CharacterCard.HitPoints <= 10)
            {
                coroutine = SelectAndPlayCardFromHand(DecisionMaker);
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

