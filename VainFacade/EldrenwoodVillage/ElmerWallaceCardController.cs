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
    public class ElmerWallaceCardController : AfflictedCardController
    {
        public ElmerWallaceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, one player draws a card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker && CanActivateEffect(base.TurnTakerController, QuaintKey), (PhaseChangeAction pca) => GameController.SelectHeroToDrawCard(DecisionMaker, optionalDrawCard: false, cardSource: GetCardSource()), TriggerType.DrawCard);
        }

        public override IEnumerator SlainInHumanFormResponse()
        {
            // "... each hero target deals itself 2 psychic damage."
            yield return base.GameController.DealDamageToSelf(DecisionMaker, (Card c) => c.IsHero, 2, DamageType.Psychic, cardSource: GetCardSource());
        }
    }
}
