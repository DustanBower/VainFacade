using NUnit.Framework;
using System;
using VainFacadePlaytest;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using System.Collections.Generic;
using VainFacadePlaytest.Node;
using System.Runtime.Serialization;
using System.Security.Policy;
using NUnit.Framework.Interfaces;

namespace VainFacadeTest
{
    [TestFixture]
    public class NodeTests : BaseTest
    {
        protected HeroTurnTakerController node { get { return FindHero("Node"); } }

        [Test()]
        public void OpenLineTest3DrawLimit()
        {
            SetupGameController("Apostate", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card openline = PlayCard("OpenLine");
            DecisionYesNo = true;
            QuickHandStorage(legacy, node);
            DrawCard(node, 4);
            QuickHandCheck(3, 1);

            GoToStartOfTurn(legacy);
            QuickHandStorage(legacy, node);
            DrawCard(node, 4);
            QuickHandCheck(3, 1);
        }

        [Test()]
        public void OpenLineTest3PlayLimit()
        {
            SetupGameController("Apostate", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card openline = PlayCard("OpenLine");
            DecisionYesNo = true;
            Card fortitude = PutInHand("Fortitude");
            Card danger = PutInHand("DangerSense");
            Card evo = PutInHand("NextEvolution");
            Card thokk = PutInHand("Thokk");
            Card dial = PutInHand("DialingIn");

            DecisionSelectCard = fortitude;
            PlayCard(dial);
            AssertIsInPlay(fortitude);
            AssertInHand(dial);

            DecisionSelectCard = danger;
            PlayCard(dial);
            AssertIsInPlay(danger);
            AssertInHand(dial);

            DecisionSelectCard = evo;
            PlayCard(dial);
            AssertIsInPlay(evo);
            AssertInHand(dial);

            DecisionSelectCard = thokk;
            PlayCard(dial);
            AssertIsInPlay(dial);
            AssertInHand(thokk);
        }

        [Test()]
        public void OpenLineTestMixed()
        {
            SetupGameController("Apostate", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card openline = PlayCard("OpenLine");

            Card fortitude = PutInHand("Fortitude");
            Card danger = PutInHand("DangerSense");
            Card evo = PutInHand("NextEvolution");
            Card dial = PutInHand("DialingIn");

            DecisionYesNo = true;
            DecisionSelectCard = fortitude;
            PlayCard(dial);
            AssertIsInPlay(fortitude);
            AssertInHand(dial);

            DecisionYesNo = false;
            DecisionSelectCard = danger;
            PlayCard(dial);
            AssertIsInPlay(dial);
            AssertInHand(evo);

            DecisionYesNo = true;
            QuickHandStorage(legacy, node);
            DrawCard(node);
            QuickHandCheck(1, 0);

            DecisionYesNo = true;
            QuickHandStorage(legacy, node);
            DrawCard(node);
            QuickHandCheck(1, 0);

            DecisionYesNo = true;
            QuickHandStorage(legacy, node);
            DrawCard(node);
            QuickHandCheck(0, 1);
        }
    }
}

