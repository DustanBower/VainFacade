using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class MinionCardController : GrandfatherUtilityCardController
    {
        public MinionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, this card deals the hero target with the highest HP {H - 1} projectile damage."
            AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => c.IsHero && c.IsTarget, TargetType.HighestHP, H - 1, DamageType.Projectile);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, if there are no other non-Covert villain targets in play..."
            if (!FindCardsWhere(new LinqCardCriteria((Card c) => c != base.Card && c.IsInPlayAndHasGameText && c.IsTarget && c.IsVillainTarget && !c.DoKeywordsContain(CovertKeyword), "non-Covert villain", false, false, "target other than " + base.Card.Title, "targets other than " + base.Card.Title), visibleToCard: GetCardSource()).Any())
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.Card.Title + " has no other non-Covert villain targets in play to report to!", Priority.High, cardSource: GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                // "... play the top card of the environment deck..."
                IEnumerator playEnvCoroutine = base.GameController.PlayTopCard(DecisionMaker, FindEnvironment(), responsibleTurnTaker: base.TurnTaker, showMessage: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playEnvCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playEnvCoroutine);
                }
                // "... and the villain deck..."
                IEnumerator playVillainCoroutine = base.GameController.PlayTopCard(DecisionMaker, base.TurnTakerController, responsibleTurnTaker: base.TurnTaker, showMessage: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playVillainCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playVillainCoroutine);
                }
                // "... then destroy this card."
                IEnumerator destructCoroutine = DestroyThisCardResponse(null);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destructCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destructCoroutine);
                }
            }
            yield break;
        }
    }
}
