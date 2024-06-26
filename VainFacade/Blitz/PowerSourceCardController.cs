﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Blitz
{
    public class PowerSourceCardController : CardController
    {
        public PowerSourceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, this card deals {BlitzCharacter} {H} lightning damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => base.GameController.DealDamage(DecisionMaker, base.Card, (Card c) => c == base.CharacterCard, H, DamageType.Lightning, cardSource: GetCardSource()), TriggerType.DealDamage);
        }
    }
}
