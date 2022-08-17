using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Burgess
{
    public class WordOnTheStreetCardController : CardController
    {
        public WordOnTheStreetCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of each player's turn, you may discard 2 cards. If you do, damage dealt by that player's targets is irreducible until the end of the turn."
            AddStartOfTurnTrigger((TurnTaker tt) => tt.IsHero, DiscardToMakeIrreducibleResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.CreateStatusEffect });
        }

        private IEnumerator DiscardToMakeIrreducibleResponse(PhaseChangeAction pca)
        {
            if (!base.HeroTurnTaker.Hand.HasCards)
            {
                yield break;
            }
            // "... you may discard 2 cards."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = SelectAndDiscardCards(base.HeroTurnTakerController, 2, optional: true, storedResults: discards, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "If you do, damage dealt by that player's targets is irreducible until the end of the turn."
            if (DidDiscardCards(discards, 2))
            {
                MakeDamageIrreducibleStatusEffect buff = new MakeDamageIrreducibleStatusEffect();
                buff.SourceCriteria.IsTarget = true;
                buff.SourceCriteria.OwnedBy = pca.ToPhase.TurnTaker;
                buff.SourceCriteria.OutputString = pca.ToPhase.TurnTaker.Name + "'s targets";
                if (pca.ToPhase.TurnTaker.IsMultiCharacterTurnTaker && pca.ToPhase.TurnTaker.Name.EndsWith("s"))
                {
                    buff.SourceCriteria.OutputString = pca.ToPhase.TurnTaker.Name + "' targets";
                }
                buff.UntilThisTurnIsOver(Game);
                IEnumerator statusCoroutine = AddStatusEffect(buff);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
        }
    }
}
