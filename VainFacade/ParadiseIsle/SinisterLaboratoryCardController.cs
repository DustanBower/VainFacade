using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class SinisterLaboratoryCardController : AreaCardController
    {
        public SinisterLaboratoryCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }


        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase toxic and psychic damage dealt by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageType == DamageType.Toxic || dda.DamageType == DamageType.Psychic, (DealDamageAction dda) => 1);
            // "At the end of the environment turn, reveal the top card of the environment deck. If that card is {TestSubjects}, put it into play, otherwise discard it and deal each hero target 1 toxic damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, PlayTheTopCardOfTheEnvironmentDeckWithMessageResponse, TriggerType.PlayCard);
        }
    }
}
