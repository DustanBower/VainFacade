using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class NothingToSeeHereCardController:PatternCardController
	{
		public NothingToSeeHereCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowTokenPool(pool());
		}

        private TokenPool pool()
        {
            return this.Card.FindTokenPool("NothingToSeeHerePool");
        }

        private string FirstDamageKey = "FirstDamageKey";

        public override void AddTriggers()
        {
            //The first time each turn each hero is dealt damage by any non-hero card other than {Doomsayer}, put a token on this card.
            //Then if there are {H} or more tokens on this card, put this card under {CountlessWords}.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => !IsPropertyTrue(GeneratePerTargetKey(FirstDamageKey, dd.Target)) && IsHeroCharacterCard(dd.Target) && dd.DamageSource.IsCard && !IsHero(dd.DamageSource.Card) && dd.DamageSource.Card != this.CharacterCard && dd.DidDealDamage, DamageResponse, new TriggerType[2] { TriggerType.AddTokensToPool, TriggerType.MoveCard }, TriggerTiming.After);

            //At the end of the villain turn, play the top card of the villain deck.
            AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, (PhaseChangeAction pca) => PlayTheTopCardOfTheVillainDeckResponse(pca), TriggerType.PlayCard);

            //Reset token pool when this card is destroyed or moved out of play. Based on Magma's Rage
            AddWhenDestroyedTrigger((DestroyCardAction dc) => ResetTokenValue(), TriggerType.Hidden);
            AddTrigger<MoveCardAction>((MoveCardAction mc) => mc.Origin.IsInPlayAndNotUnderCard && !mc.Destination.IsInPlayAndNotUnderCard && mc.CardToMove == base.Card, (MoveCardAction mc) => ResetTokenValue(), TriggerType.ModifyTokens, TriggerTiming.After, ActionDescription.Unspecified, outOfPlayTrigger: true);
        }

        private IEnumerator ResetTokenValue()
        {
            pool().SetToInitialValue();
            yield return null;
        }

        private IEnumerator DamageResponse(DealDamageAction dd)
        {
            SetCardPropertyToTrueIfRealAction(GeneratePerTargetKey(FirstDamageKey, dd.Target));
            IEnumerator coroutine = base.GameController.AddTokensToPool(pool(), 1, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (pool().CurrentValue >= base.H)
            {
                coroutine = base.GameController.MoveCard(this.TurnTakerController, this.Card, countlessWords.UnderLocation, cardSource: GetCardSource());
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

