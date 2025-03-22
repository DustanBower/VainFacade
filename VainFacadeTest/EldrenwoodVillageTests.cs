using NUnit.Framework;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.UnitTest;
using System.Linq;
using VainFacadePlaytest.Doomsayer;
using Handelabra;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace VainFacadeTest
{
    [TestFixture()]
    public class EldrenwoodVillageTests : BaseTest
    {
        protected TurnTakerController eldren { get { return FindEnvironment(); } }

        [Test()]
        public void TestACallOfTheWildEnviroWW()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.EldrenwoodVillage");
            StartGame();
            // "Triggers are indestructible while there is at least 1 environment werewolf in play."
            Card elmer = PutOnDeck("ElmerWallace");
            Card trigger = PlayCard("FullMoon");
            Card call = PlayCard("CallOfTheWild");
            MoveCards(eldren, trigger.UnderLocation.Cards, (Card c) => c.NativeTrash);
            DestroyCard(trigger);
            AssertIsInPlay(trigger);

            DestroyCard(elmer);
            DestroyCard(trigger);
            AssertInTrash(trigger);
        }
    }
}