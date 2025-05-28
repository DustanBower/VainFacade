using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
	public class SecretSocietiesCardController:WebCardController
	{
		public SecretSocietiesCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
		}

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();

            if (!this.Card.IsFlipped)
            {
                //At the end of the environment turn, for every 3 HP this card possesses, increase the next damage dealt by The Baroness by 1.
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, EndOfTurnResponse, TriggerType.CreateStatusEffect, additionalCriteria: (PhaseChangeAction pca) => this.Card.HitPoints >= 3));
            }
            else
            {
                //At the end of the environment turn, each player may play a card and use a power. If a card enters play or a power is used this way, destroy this card.
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, EndOfTurnFlippedResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.DestroySelf }));
            }
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            //...for every 3 HP this card possesses, increase the next damage dealt by The Baroness by 1.
            int i = this.Card.HitPoints.Value / 3;
            IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(i);
            effect.NumberOfUses = 1;
            effect.SourceCriteria.IsSpecificCard = this.CharacterCard;
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

        private IEnumerator EndOfTurnFlippedResponse(PhaseChangeAction pca)
        {
            //...each player may play a card and use a power.
            List<PlayCardAction> playResults = new List<PlayCardAction>();
            List<UsePowerDecision> powerResults = new List<UsePowerDecision>();
            Func<TurnTaker, IEnumerator> action = (TurnTaker tt) => PlayAndPower(tt, playResults, powerResults);
            IEnumerator coroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame), SelectionType.Custom, action, requiredDecisions: 0, allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //If a card enters play or a power is used this way, destroy this card.
            if (DidPlayCards(playResults) || (powerResults.FirstOrDefault() != null && powerResults.FirstOrDefault().SelectedPower != null))
            {
                coroutine = DestroyThisCardResponse(null);
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

        private IEnumerator PlayAndPower(TurnTaker tt, List<PlayCardAction> playResults = null, List<UsePowerDecision> powerResults = null)
        {
            HeroTurnTakerController httc = FindHeroTurnTakerController(tt.ToHero());
            IEnumerator coroutine = SelectAndPlayCardFromHand(httc,storedResults: playResults);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.GameController.SelectAndUsePower(httc, storedResults: powerResults);
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
            string s = "play a card and use a power";

            return new CustomDecisionText(
            $"Select a player to {s}.",
            $"The players are selecting a player to {s}.",
            $"Vote for a player to {s}.",
            $"a player to {s}."
            );
        }
    }
}

