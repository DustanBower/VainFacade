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
            AddTrigger((PlayCardAction pca) => pca.WasCardPlayed && pca.ResponsibleTurnTaker == base.TurnTaker && pca.CardToPlay.DoKeywordsContain(emanationKeyword) && !pca.IsPutIntoPlay, (PlayCardAction pca) => DrawACardOrPlayACard(base.HeroTurnTakerController, true), new TriggerType[] { TriggerType.DrawCard, TriggerType.PlayCard }, TriggerTiming.After);
        }
    }
}
