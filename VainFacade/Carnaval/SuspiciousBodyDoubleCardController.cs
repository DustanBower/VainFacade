using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Carnaval
{
    public class SuspiciousBodyDoubleCardController : CardController
    {
        public SuspiciousBodyDoubleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If this card is in play, make sure the player can tell what play area it's in
            SpecialStringMaker.ShowSpecialString(() => "This card is in " + base.Card.Location.HighestRecursiveLocation.GetFriendlyName() + ".").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of that target's turn, the 3 targets in this play area with the highest HP each deal themselves 2 toxic damage. Then destroy this card."
            AddEndOfTurnTrigger((TurnTaker tt) => base.Card.UnderLocation.HasCards && tt == base.Card.UnderLocation.Cards.FirstOrDefault().Owner, GasResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroySelf });
        }

        private IEnumerator GasResponse(PhaseChangeAction pca)
        {
            // "... the 3 targets in this play area with the highest HP each deal themselves 2 toxic damage."
            List<Card> storedTargets = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetsWithHighestHitPoints(1, 3, (Card c) => c.IsInPlayAndHasGameText && c.IsTarget && c.IsAtLocationRecursive(base.Card.Location.HighestRecursiveLocation), storedTargets, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            IEnumerator damageCoroutine = base.GameController.DealDamageToSelf(DecisionMaker, (Card c) => storedTargets.Contains(c), 2, DamageType.Toxic, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "Then destroy this card."
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

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card in the play area of a target with 3 or fewer HP. Move that target under this card."
            IEnumerable<Card> options = FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsTarget && c.HitPoints.Value <= 3), visibleToCard: GetCardSource());
            if (options.Count() > 0)
            {
                List<SelectCardDecision> choices = new List<SelectCardDecision>();
                IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.MoveCardAboveCard, options, choices, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selectCoroutine);
                }
                Card selected = GetSelectedCard(choices);
                if (selected != null)
                {
                    Location dest = selected.Location;
                    IEnumerator hideCoroutine = base.GameController.MoveCard(base.TurnTakerController, selected, base.Card.UnderLocation, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(hideCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(hideCoroutine);
                    }
                    base.Card.PlayIndex = selected.PlayIndex;
                    selected.PlayIndex = null;
                    storedResults?.Add(new MoveCardDestination(dest));
                }
            }
            else
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction("There are no targets with 3 or fewer HP in play to move " + base.Card.Title + "above. Moving it to " + base.Card.Owner.Name + "'s trash.", Priority.High, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                storedResults?.Add(new MoveCardDestination(base.TurnTaker.Trash));
            }
        }
    }
}
