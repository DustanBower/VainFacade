using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace VainFacadePlaytest.Doomsayer
{
	public class CountlessWordsCardController:VillainCharacterCardController
	{
		public CountlessWordsCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.AddAsPowerContributor();
            base.AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            base.SpecialStringMaker.ShowIfElseSpecialString(() => base.GameController.IsCardIndestructible(this.Card), () => $"{this.Card.Title} is indestructible.", () => $"{this.Card.Title} is not indestructible.").Condition = () => this.Card.IsInPlayAndHasGameText && base.GameController.IsCardIndestructible(this.Card);
        }

        public override void AddSideTriggers()
        {
            if (!this.Card.IsFlipped)
            {
                //When there are 4 cards under this card, flip this card.
                AddSideTrigger(AddTrigger<MoveCardAction>((MoveCardAction mc) => mc.Destination == this.Card.UnderLocation && this.Card.UnderLocation.Cards.Count() == 4, (MoveCardAction mc) => base.GameController.FlipCard(this, cardSource:GetCardSource()), TriggerType.FlipCard, TriggerTiming.After));

                //When there are no cards under this card, villain cards are indestructible and {Doomsayer} is immune to damage.
                AddSideTrigger(AddImmuneToDamageTrigger((DealDamageAction dd) => this.Card.UnderLocation.Cards.Count() == 0 && dd.Target == this.CharacterCard));
            }
            else
            {
                //Reduce damage dealt to hero targets by 1.
                AddSideTrigger(AddReduceDamageTrigger((Card c) => IsHeroTarget(c), 1));
            }
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            if (!this.Card.IsFlipped)
            {
                if (this.Card.UnderLocation.Cards.Count() == 0)
                {
                    //When there are no cards under this card, villain cards are indestructible
                    return IsVillain(card);
                }
                else if (this.Card.UnderLocation.Cards.Count() <= 2)
                {
                    //When there are 2 or fewer cards under this card, {Doomsayer} and proclamations are indestructible.
                    return IsProclamation(card) || card == this.CharacterCard;
                }
            }
            return base.AskIfCardIsIndestructible(card);
        }

        public bool IsProclamation(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "proclamation");
        }

        public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cardController)
        {
            //Each hero gains the following power:
            //Power: Destroy an ongoing or increase the next damage dealt to {Doomsayer} by 3.
            if (this.Card.IsFlipped && cardController.HeroTurnTakerController != null && cardController.Card.IsHeroCharacterCard && cardController.Card.Owner.IsPlayer && !cardController.Card.Owner.IsIncapacitatedOrOutOfGame && !cardController.Card.IsFlipped)
            {
                Power power = new Power(cardController.HeroTurnTakerController, cardController, "Destroy an ongoing or increase the next damage dealt to {Doomsayer} by 3.",() => PowerResponse(cardController), 0, null, GetCardSource());
                return new Power[1] { power };
            }
            return base.AskIfContributesPowersToCardController(cardController);
        }

        private IEnumerator PowerResponse(CardController cardController)
        {
            IEnumerable<Function> functionChoices = new Function[2]
                {
            new Function(base.HeroTurnTakerController, "Destroy an ongoing", SelectionType.DestroyCard, () => base.GameController.SelectAndDestroyCard(cardController.HeroTurnTakerController, new LinqCardCriteria((Card c) => IsOngoing(c), "ongoing"),false, cardSource:GetCardSource())),
            new Function(base.HeroTurnTakerController, "Increase the next damage dealt to {Doomsayer} by 3", SelectionType.IncreaseDamage, IncreaseResponse)
                };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, false, null, null, null, GetCardSource());
            IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(choose);
            }
            else
            {
                base.GameController.ExhaustCoroutine(choose);
            }
        }

        private IEnumerator IncreaseResponse()
        {
            IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(3);
            effect.TargetCriteria.IsSpecificCard = this.CharacterCard;
            effect.NumberOfUses = 1;
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

