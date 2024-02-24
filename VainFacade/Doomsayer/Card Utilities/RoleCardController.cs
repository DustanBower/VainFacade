using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class RoleCardController:DoomsayerCardUtilities
	{
		public RoleCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            base.SpecialStringMaker.ShowTokenPool(pool());
        }

        //Doomsayer is indestructible
        public override bool AskIfCardIsIndestructible(Card card)
        {
            if (card == this.CharacterCard)
            {
                return true;
            }
            return base.AskIfCardIsIndestructible(card);
        }

        public override void AddTriggers()
        {
            //Doomsayer is immune to damage and can't deal damage
            AddImmuneToDamageTrigger((DealDamageAction dd) => dd.Target == this.CharacterCard);
            AddCannotDealDamageTrigger((Card c) => c == this.CharacterCard);

            //At the start of each player’s turn, that player may skip their play, power, or draw phase to add a token to this card.
            //Then, if there are more tokens on this card than active heroes, each hero deals itself 1 psychic damage, then remove this card from the game.
            AddStartOfTurnTrigger((TurnTaker tt) => tt.IsPlayer, StartOfTurnResponse, TriggerType.SkipPhase);

            //Reset token pool when this card is destroyed or moved out of play. Based on Magma's Rage
            AddWhenDestroyedTrigger((DestroyCardAction dc) => ResetTokenValue(), TriggerType.Hidden);
            AddTrigger<MoveCardAction>((MoveCardAction mc) => mc.Origin.IsInPlayAndNotUnderCard && !mc.Destination.IsInPlayAndNotUnderCard && mc.CardToMove == base.Card, (MoveCardAction mc) => ResetTokenValue(), TriggerType.ModifyTokens, TriggerTiming.After, ActionDescription.Unspecified, outOfPlayTrigger: true);
        }

        private TokenPool pool()
        {
            return this.Card.FindTokenPool("RolePool");
        }

        private IEnumerator ResetTokenValue()
        {
            pool().SetToInitialValue();
            yield return null;
        }

        private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
        {
            TurnTaker hero = pca.ToPhase.TurnTaker;
            if (!hero.IsIncapacitatedOrOutOfGame)
            {
                IEnumerable<Function> functionChoices = new Function[3]
                {
                new Function(base.HeroTurnTakerController, "Skip your play phase", SelectionType.SkipTurn, () => SkipPhaseResponse(hero,Phase.PlayCard)),
                new Function(base.HeroTurnTakerController, "Skip your power phase", SelectionType.SkipTurn, () => SkipPhaseResponse(hero,Phase.UsePower)),
                new Function(base.HeroTurnTakerController, "Skip your draw phase", SelectionType.SkipTurn, () => SkipPhaseResponse(hero,Phase.DrawCard))
                };

                SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, true, null, null, null, GetCardSource());
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
            

            if (pool().CurrentValue > FindTurnTakersWhere((TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame).Count())
            {
                IEnumerator coroutine = TokenResponse(null);
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

        private IEnumerator SkipPhaseResponse(TurnTaker tt, Phase phase)
        {
            PreventPhaseActionStatusEffect effect = new PreventPhaseActionStatusEffect();
            effect.ToTurnPhaseCriteria.Phase = phase;
            effect.ToTurnPhaseCriteria.TurnTaker = tt;
            effect.UntilThisTurnIsOver(base.Game);
            IEnumerator coroutine = AddStatusEffect(effect);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.GameController.AddTokensToPool(pool(), 1, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator TokenResponse(GameAction action)
        {
            IEnumerator coroutine = base.GameController.DealDamageToSelf(DecisionMaker, (Card c) => IsHeroCharacterCard(c), 1, DamageType.Psychic, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.GameController.MoveCard(this.TurnTakerController, this.Card, this.TurnTaker.OutOfGame, cardSource: GetCardSource());
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

