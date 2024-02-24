using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class AcolytesOfTheBlackThornCardController:SideCharacterCardController
	{
		public AcolytesOfTheBlackThornCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageKey,$"{this.Card.Title} has already been dealt damage this turn",$"{this.Card.Title} has not been dealt damage this turn");
            base.SpecialStringMaker.ShowSpecialString(BuildSpecialString);
            base.SpecialStringMaker.ShowHeroTargetWithHighestHP(2);
            base.SpecialStringMaker.ShowHeroTargetWithLowestHP(1, base.H - 1);
		}

        private string BuildSpecialString()
        {
            if (!this.TurnTaker.Trash.HasCards)
            {
                return $"{this.TurnTaker.Trash.GetFriendlyName()} has no cards";
            }
            else
            {
                return $"The top card of {this.TurnTaker.Trash.GetFriendlyName()} is {(IsOngoing(this.TurnTaker.Trash.TopCard) ? "" : "not")} an ongoing";
            }
        }

        private string FirstDamageKey = "FirstDamageKey";

        public override void AddTriggers()
        {
            //The first time each turn this card is dealt damage, reveal and replace the top card of the villain deck, then 1 player may play a card.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(FirstDamageKey) && dd.DidDealDamage && dd.Target == this.Card, DamageResponse, new TriggerType[2] { TriggerType.RevealCard, TriggerType.PlayCard }, TriggerTiming.After);

            //At the end of the villain turn, if there is an ongoing on top of the villain trash, this card deals the hero target with the second highest HP H infernal damage,
            //otherwise this card deals the H-1 hero targets with the lowest HP 1 melee and 1 infernal damage each.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, EndOfTurnResponse, TriggerType.DealDamage);
        }

        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageKey);
            IEnumerator coroutine = base.GameController.RevealAndReplaceCards(this.TurnTakerController, this.TurnTaker.Deck, 1, null, revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = SelectHeroToPlayCard(DecisionMaker,true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            IEnumerator coroutine;
            if (IsOngoing(this.TurnTaker.Trash.TopCard))
            {
                coroutine = DealDamageToHighestHP(this.Card, 2, (Card c) => IsHeroTarget(c), (Card c) => base.H, DamageType.Infernal);
            }
            else
            {
                //based on Tryragon Rex
                List<DealDamageAction> list = new List<DealDamageAction>();
                list.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController,this.Card), null, 1, DamageType.Melee));
                list.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, this.Card), null, 1, DamageType.Infernal));
                coroutine = DealMultipleInstancesOfDamageToHighestLowestHP(list, (Card c) => IsHeroTarget(c), HighestLowestHP.LowestHP, 1, base.H - 1);
            }
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

