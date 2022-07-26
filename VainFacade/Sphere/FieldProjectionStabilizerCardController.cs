using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class FieldProjectionStabilizerCardController : SphereUtilityCardController
    {
        public FieldProjectionStabilizerCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When you play an Emanation, you may draw or play a card."
            AddTrigger((PlayCardAction pca) => pca.WasCardPlayed && pca.ResponsibleTurnTaker == base.TurnTaker && pca.CardToPlay.DoKeywordsContain(emanationKeyword) && !pca.IsPutIntoPlay, DrawOrPlayResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.PlayCard }, TriggerTiming.After);
        }

        private IEnumerator DrawOrPlayResponse(GameAction ga)
        {
            if (CanActivateEffect(base.HeroTurnTakerController, HeartKey))
            {
                // Alien Heart is in play, so if Sphere is being prevented from doing one of these actions, we can give him different options
                List<Card> hearts = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.Identifier == heartIdentifier && c.Owner == base.Card.Owner && c.IsInPlayAndHasGameText)).ToList();
                Card alienHeart = hearts.FirstOrDefault();
                Function drawCard = new Function(base.HeroTurnTakerController, "Draw a card", SelectionType.DrawCard, () => DrawCard(base.HeroTurnTaker, true), onlyDisplayIfTrue: CanDrawCards(base.HeroTurnTakerController));
                Function playCard = new Function(base.HeroTurnTakerController, "Play a card", SelectionType.PlayCard, () => SelectAndPlayCardFromHand(base.HeroTurnTakerController), onlyDisplayIfTrue: CanPlayCardsFromHand(base.HeroTurnTakerController));
                Function playCardFail = new Function(base.HeroTurnTakerController, "Fail to play a card", SelectionType.UsePower, () => (base.GameController.FindCardController(alienHeart) as AlienHeartCardController).PreventedPlayResponse(null), onlyDisplayIfTrue: base.GameController.CanPerformAction<UsePowerAction>(base.TurnTakerController, GetCardSource()) && !CanPlayCardsFromHand(base.HeroTurnTakerController));
                List<Function> options = new List<Function>();
                options.Add(drawCard);
                options.Add(playCard);
                options.Add(playCardFail);
                List<Card> associated = new List<Card>();
                if (!CanPlayCardsFromHand(base.HeroTurnTakerController))
                {
                    associated.Add(base.TurnTaker.GetCardByIdentifier(heartIdentifier));
                }
                SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, options, true, noSelectableFunctionMessage: base.TurnTaker.Name + " cannot currently play cards, draw cards, or even use powers.", associatedCards: associated, cardSource: GetCardSource());
                IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(chooseCoroutine);
                }
            }
            else
            {
                IEnumerator drawPlayCoroutine = DrawACardOrPlayACard(base.HeroTurnTakerController, true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawPlayCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawPlayCoroutine);
                }
            }
            yield break;
        }
    }
}
