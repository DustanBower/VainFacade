using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VainFacadePlaytest.Peacekeeper;

namespace VainFacadePlaytest.Glyph
{
	public class EmbroideredCloakCardController:CardController
	{
		public EmbroideredCloakCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageKey);
		}

        private string FirstDamageKey = "FirstDamageKey";

        public override void AddTriggers()
        {
            //The first time each turn {Glyph} is dealt damage, she regains 1 HP.
            AddTrigger<DealDamageAction>((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DidDealDamage && !IsPropertyTrue(FirstDamageKey) && !HasDamageOccurredThisTurn(this.CharacterCard,null,dd), GainHPResponse, TriggerType.GainHP, TriggerTiming.After);

            //At the start of your turn, you may discard a card. If you do, {Glyph} is immune to damage, cards in {Glyph}'s play area can only be destroyed by {Glyph}, and cards from other decks cannot be played this turn.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, StartOfTurnResponse, new TriggerType[] { TriggerType.DiscardCard });

            //Handle "Cards in Glyph's play area can only be destroyed by Glyph"
            AddTrigger<DestroyCardAction>((DestroyCardAction dc) => dc.CardToDestroy.Card.Location.IsHeroPlayAreaRecursive && AffectedTurnTakers().Count() > 0 && AffectedTurnTakers().Contains(dc.CardToDestroy.Card.Location.OwnerTurnTaker) && ((dc.ResponsibleCard != null && dc.ResponsibleCard.Owner != dc.CardToDestroy.Card.Location.OwnerTurnTaker) || (dc.ResponsibleCard == null && dc.CardSource != null && dc.CardSource.Card.Owner != dc.CardToDestroy.Card.Location.OwnerTurnTaker) || (dc.ResponsibleCard == null && dc.CardSource == null)), (DestroyCardAction dc) => CancelAction(dc), TriggerType.CancelAction, TriggerTiming.Before, outOfPlayTrigger: true);

            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageKey), TriggerType.Hidden);
        }

        private List<TurnTaker> AffectedTurnTakers()
        {
            return base.GameController.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is EmbroideredCloakStatusEffect).Select((StatusEffectController sec) => ((EmbroideredCloakStatusEffect)sec.StatusEffect).Owner).Distinct().ToList();
        }

        private IEnumerator GainHPResponse(DealDamageAction dd)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageKey);
            IEnumerator coroutine = base.GameController.GainHP(this.CharacterCard, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
        {
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator coroutine = SelectAndDiscardCards(DecisionMaker, 1, false, 0, results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidDiscardCards(results))
            {
                //Glyph is immune to damage
                ImmuneToDamageStatusEffect immune = new ImmuneToDamageStatusEffect();
                immune.UntilThisTurnIsOver(base.Game);
                immune.UntilTargetLeavesPlay(this.CharacterCard);
                immune.TargetCriteria.IsSpecificCard = this.CharacterCard;

                coroutine = AddStatusEffect(immune);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                //Cards in Glyph's play area can only be destroyed by Glyph
                EmbroideredCloakStatusEffect nodestroy = new EmbroideredCloakStatusEffect(this.Card, this.TurnTaker);
                nodestroy.UntilThisTurnIsOver(base.Game);

                coroutine = AddStatusEffect(nodestroy);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                //Cards from other decks cannot be played this turn
                EmbroideredCloakCannotPlayCardsStatusEffect noplay = new EmbroideredCloakCannotPlayCardsStatusEffect(this.Card, this.TurnTaker);
                noplay.UntilThisTurnIsOver(base.Game);
                noplay.CardCriteria.IsOneOfTheseCards = FindCardsWhere((Card c) => c.Owner != this.TurnTaker).ToList();

                coroutine = AddStatusEffect(noplay);
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

