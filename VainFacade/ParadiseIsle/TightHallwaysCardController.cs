using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Net.WebRequestMethods;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class TightHallwaysCardController : AreaCardController
    {
        public TightHallwaysCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase projectile damage dealt by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageType == DamageType.Projectile, 1);
            // "Reduce non-projectile damage dealt by 1."
            AddReduceDamageTrigger((DealDamageAction dda) => dda.DamageType != DamageType.Projectile, (DealDamageAction dda) => 1);
            
            // "At the end of the environment turn, each player discards a card and draws a card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitatedOrOutOfGame, "hero"), SelectionType.DiscardAndDrawCard, DiscardAndDrawResponse, optional: false, allowAutoDecide: true, cardSource: GetCardSource()), new TriggerType[] { TriggerType.DiscardCard, TriggerType.DrawCard });
        }

        private IEnumerator DiscardAndDrawResponse(TurnTaker tt)
        {
            if (tt.IsHero)
            {
                // "... discards a card..."
                IEnumerator discardCoroutine = SelectAndDiscardCards(base.GameController.FindHeroTurnTakerController(tt.ToHero()), 1, requiredDecisions: 1);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
                // "... and draws a card."
                IEnumerator drawCoroutine = DrawCard(tt.ToHero());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawCoroutine);
                }
            }
        }
    }
}
