using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
    public class ArcaneVeinsCardController : BaronessUtilityCardController
    {
        public ArcaneVeinsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase infernal damage dealt by {TheBaroness} by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card == base.CharacterCard && dda.DamageType == DamageType.Infernal, 1);
            // "At the end of the villain turn, reveal {H - 2} cards from the top of the villain deck. Put any revealed Spells into play. Discard the other revealed cards."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => RevealCards_PutSomeIntoPlay_DiscardRemaining(base.TurnTakerController, base.TurnTaker.Deck, H - 2, new LinqCardCriteria((Card c) => c.DoKeywordsContain(SpellKeyword), "Spell")), new TriggerType[] { TriggerType.RevealCard, TriggerType.PutIntoPlay, TriggerType.DiscardCard });
        }
    }
}
