using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Node
{
    public class TheIInTeamCardController : NodeUtilityCardController
    {
        public TheIInTeamCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Connected hero targets
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => IsConnected(c) && c.IsHero && c.IsTarget, "Connected hero targets", false, false, "target", "targets"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of your turn, {NodeCharacter} may deal herself 3 irreducible psychic damage. If she takes no damage this way, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction p) => DealDamageOrDestroyThisCardResponse(p, base.CharacterCard, base.CharacterCard, 3, DamageType.Psychic, true), new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroySelf });
            // "When a [i]Connected[/i] hero target would be dealt damage, you may redirect that damage to another [i]Connected[/i] hero target. Reduce damage redirected this way by 1."
            AddTrigger((DealDamageAction dda) => dda.CanDealDamage && dda.IsRedirectable && IsConnected(dda.Target) && dda.Target.IsHero, RedirectReduceResponse, new TriggerType[] { TriggerType.RedirectDamage, TriggerType.ReduceDamage }, TriggerTiming.Before);
        }

        private IEnumerator RedirectReduceResponse(DealDamageAction dda)
        {
            // "... you may redirect that damage to another [i]Connected[/i] hero target."
            List<SelectCardDecision> choices = new List<SelectCardDecision>();
            IEnumerator redirectCoroutine = base.GameController.SelectTargetAndRedirectDamage(DecisionMaker, (Card c) => IsConnected(c) && c.IsHero && c.IsTarget && c != dda.Target, dda, optional: true, storedResults: choices, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(redirectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(redirectCoroutine);
            }
            // "Reduce damage redirected this way by 1."
            if (DidSelectCard(choices))
            {
                IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, 1, null, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(reduceCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(reduceCoroutine);
                }
            }
        }
    }
}
