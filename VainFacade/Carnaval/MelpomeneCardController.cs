﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Carnaval
{
    public class MelpomeneCardController : MasqueCardController
    {
        public MelpomeneCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of your turn, up to 2 targets regain 1 HP each."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => base.GameController.GainHP(DecisionMaker, (Card c) => c.IsTarget && c.IsInPlayAndHasGameText, 1, 2, requiredHpGainers: 0, cardSource: GetCardSource()), TriggerType.GainHP);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Up to 2 players may draw 1 card each."
            int numPlayers = GetPowerNumeral(0, 2);
            int numCards = GetPowerNumeral(1, 1);
            IEnumerator drawCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && tt.IsHero), SelectionType.DrawCard, (TurnTaker tt) => DrawCards(FindHeroTurnTakerController(tt.ToHero()), numCards), numPlayers, requiredDecisions: 0, numberOfCards: numCards, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
        }
    }
}