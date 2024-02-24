using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class SilverTonguedDevilCardController:DoomsayerCardUtilities
	{
		public SilverTonguedDevilCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHeroCharacterCardWithHighestHP();
		}

        public override IEnumerator Play()
        {
            //When this card enters play, shuffle all one-shots and targets from the villain trash into the villain deck.
            IEnumerator coroutine = base.GameController.ShuffleCardsIntoLocation(DecisionMaker, FindCardsWhere((Card c) => c.Location == this.TurnTaker.Trash && (c.IsOneShot || c.IsTarget)), this.TurnTaker.Deck, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override void AddTriggers()
        {
            //When a villain card enters play, the hero with the highest hp may increase the next damage dealt to them by 3. Otherwise destroy a hero ongoing or equipment.
            //Then 1 player may discard their hand to destroy an ongoing card.
            AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cep) => IsVillain(cep.CardEnteringPlay) && cep.CardEnteringPlay != this.Card, EnterPlayResponse, new TriggerType[3] { TriggerType.CreateStatusEffect, TriggerType.DestroyCard, TriggerType.DiscardCard }, TriggerTiming.After);
        }

        private IEnumerator EnterPlayResponse(CardEntersPlayAction cep)
        {
            List<Card> highest = new List<Card>();

            IEnumerator coroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => IsHeroCharacterCard(c), highest, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (highest.FirstOrDefault() != null)
            {
                Card highestCard = highest.FirstOrDefault();
                HeroTurnTakerController httc = FindHeroTurnTakerController(highestCard.Owner.ToHero());


                List<YesNoCardDecision> yesno = new List<YesNoCardDecision>();
                coroutine = base.GameController.MakeYesNoCardDecision(httc, SelectionType.Custom, this.Card, null, yesno, new Card[] { highestCard }, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (DidPlayerAnswerYes(yesno))
                {
                    IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(3);
                    effect.TargetCriteria.IsSpecificCard = highestCard;
                    effect.NumberOfUses = 1;
                    effect.UntilTargetLeavesPlay(highestCard);
                    coroutine = AddStatusEffect(effect);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }
                else
                {
                    coroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsHero && (IsOngoing(c) || IsEquipment(c)), "hero ongoing or equipment"), false, responsibleCard: this.Card, cardSource: GetCardSource());
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
            SelectTurnTakerDecision decision = new SelectTurnTakerDecision(base.GameController, DecisionMaker, FindTurnTakersWhere((TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame && tt.ToHero().HasCardsInHand), SelectionType.DiscardHand, true, cardSource: GetCardSource());
            coroutine = base.GameController.SelectTurnTakerAndDoAction(decision, DiscardHandResponse);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator DiscardHandResponse(TurnTaker tt)
        {
            IEnumerator coroutine = base.GameController.DiscardHand(FindHeroTurnTakerController(tt.ToHero()), false, null, tt, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.GameController.SelectAndDestroyCard(FindHeroTurnTakerController(tt.ToHero()), new LinqCardCriteria((Card c) => IsOngoing(c), "ongoing"), false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            Card highestCard = decision.AssociatedCards.FirstOrDefault();
            return new CustomDecisionText(
                $"Increase the next damage dealt to {highestCard.Title} by 3?",
                $"The other heroes are deciding whether to increase the next damage dealt to {highestCard.Title} by 3.",
                $"Vote for whether to increase the next damage dealt to {highestCard.Title} by 3.",
                $"whether to increase the next damage dealt to {highestCard.Title} by 3."
            );
        }
    }
}

