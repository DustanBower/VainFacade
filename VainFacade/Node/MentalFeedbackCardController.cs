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
    public class MentalFeedbackCardController : NodeUtilityCardController
    {
        public MentalFeedbackCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show Connected Ongoing cards in play
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsOngoing && IsConnected(c), "Connected Ongoing"));
        }

        public override IEnumerator Play()
        {
            // "Destroy 1 [i]Connected[/i] Ongoing."
            List<DestroyCardAction> chosen = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsOngoing && IsConnected(c), "Connected Ongoing"), false, chosen, base.Card, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "{NodeCharacter} deals the character from that deck with the highest HP 0 psychic damage and 0 psychic damage."
            Card chosenOngoing = chosen.Where((DestroyCardAction dca) => dca.CardToDestroy != null).FirstOrDefault().CardToDestroy.Card;
            List<DealDamageAction> instances = new List<DealDamageAction>();
            DealDamageAction zeroPsychic = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 0, DamageType.Psychic);
            instances.Add(zeroPsychic);
            instances.Add(zeroPsychic);
            IEnumerator damageCoroutine = DealMultipleInstancesOfDamageToHighestLowestHP(instances, (Card c) => c.ParentDeck == chosenOngoing.ParentDeck, HighestLowestHP.HighestHP);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
        }
    }
}
