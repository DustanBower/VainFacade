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
    public class ReversePsychologyCardController : NodeUtilityCardController
    {
        public ReversePsychologyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "Put a non-character [i]Connected[/i] card in play on top of its deck."
            List<SelectCardDecision> choices = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.MoveCardOnDeck, new LinqCardCriteria((Card c) => !c.IsCharacter && IsConnected(c), "non-character Connected"), choices, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            if (DidSelectCard(choices) && GetSelectedCard(choices) != null)
            {
                Card moving = GetSelectedCard(choices);
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, moving, GetNativeDeck(moving), showMessage: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
                // "{NodeCharacter} deals the character from that deck with the highest HP 0 psychic and 0 psychic damage, ..."
                List<DealDamageAction> instances = new List<DealDamageAction>();
                DealDamageAction zeroPsychic = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 0, DamageType.Psychic);
                instances.Add(zeroPsychic);
                instances.Add(zeroPsychic);
                IEnumerator zeroCoroutine = DealMultipleInstancesOfDamageToHighestLowestHP(instances, (Card c) => c.IsCharacter && GetNativeDeck(c) == GetNativeDeck(moving), HighestLowestHP.HighestHP);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(zeroCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(zeroCoroutine);
                }
                // "... then deals herself 2 psychic damage."
                IEnumerator selfCoroutine = base.GameController.DealDamage(DecisionMaker, base.CharacterCard, (Card c) => c == base.CharacterCard, 2, DamageType.Psychic, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selfCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selfCoroutine);
                }
            }
            yield break;
        }
    }
}
