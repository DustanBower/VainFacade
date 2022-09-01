using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.EldrenwoodVillage
{
    public class CallOfTheWildCardController : EldrenwoodUtilityCardController
    {
        public CallOfTheWildCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            // Show number of Werewolves in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => base.GameController.DoesCardContainKeyword(c, WerewolfKeyword), "Werewolf"));
            // If there are no Triggers in play, show list of Afflicted who will become Werewolves if this enters play?
            // ...
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            if (card.DoKeywordsContain(TriggerKeyword))
            {
                // "Triggers are indestructible while there is at least 1 Werewolf in play."
                return FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndNotUnderCard && base.GameController.DoesCardContainKeyword(c, WerewolfKeyword), "in play", false, true, "Werewolf card", "Werewolf cards"), visibleToCard: GetCardSource()).Count() > 0;
            }
            return base.AskIfCardIsIndestructible(card);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase all damage dealt by Werewolves by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && base.GameController.DoesCardContainKeyword(dda.DamageSource.Card, WerewolfKeyword), 1);
            // "At the start of the environment turn, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }
    }
}
