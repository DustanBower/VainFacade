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
    public class PsychologicalFortitudeCardController : NodeUtilityCardController
    {
        public PsychologicalFortitudeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Connected hero Ongoing and/or Equipment cards
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => IsConnected(c) && c.IsHero && (IsOngoing(c) || IsEquipment(c)), "Connected hero Ongoing or Equipment"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a [i]Connected[/i] hero Ongoing or Equipment would be destroyed by a card other than itself or this card, you may destroy another [i]Connected[/i] hero Equipment or Ongoing."
            AddTrigger((DestroyCardAction dca) => dca.CardToDestroy != null && dca.CardToDestroy.Card != null && IsConnected(dca.CardToDestroy.Card) && dca.CardToDestroy.Card.IsHero && (IsOngoing(dca.CardToDestroy.Card) || IsEquipment(dca.CardToDestroy.Card)) && dca.ResponsibleCard != dca.CardToDestroy.Card && dca.ResponsibleCard != base.Card, DestroyToProtectResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.CreateStatusEffect}, TriggerTiming.Before);
        }

        private IEnumerator DestroyToProtectResponse(DestroyCardAction dca)
        {
            // "... you may destroy another [i]Connected[/i] hero Equipment or Ongoing."
            List<DestroyCardAction> destruction = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c != dca.CardToDestroy.Card && IsConnected(c) && c.IsHero && (IsOngoing(c) || IsEquipment(c)), "other Connected hero Ongoing or Equipment"), true, destruction, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            if (DidDestroyCard(destruction))
            {
                // "When a card is destroyed by this effect, the first card is indestructible this turn."
                Card firstCard = dca.CardToDestroy.Card;
                MakeIndestructibleStatusEffect protection = new MakeIndestructibleStatusEffect();
                protection.CardsToMakeIndestructible.IsSpecificCard = firstCard;
                protection.UntilThisTurnIsOver(base.Game);
                IEnumerator protectCoroutine = base.GameController.AddStatusEffect(protection, true, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(protectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(protectCoroutine);
                }
            }
        }
    }
}
