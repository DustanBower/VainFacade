using NUnit.Framework;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.UnitTest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadeTest
{
    [TestFixture()]
    public class BatNextToCardTests:BaseTest
	{
        protected TurnTakerController baroness { get { return FindVillain("TheBaroness"); } }

        [Test()]
        public void BatTest()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness", "SkyScraper", "ChronoRanger", "Bunker", "Legacy", "InsulaPrimalis");
            StartGame();

            Card cloud = PlayCard("CloudOfBats");
            Card fortitude = PutOnDeck("Fortitude");
            DealDamage(baroness, legacy, 1, DamageType.Melee);

            Card bat = FindCardsWhere((Card c) => c.Location.IsNextToCard && c.Location.OwnerCard == fortitude).FirstOrDefault();
            Assert.NotNull(bat);

            DecisionSelectCard = bat;
            Card link1 = PlayCard("CompulsionCanister");
            Card link2 = PlayCard("ReboundingDebilitator");
            Card bounty = PlayCard("ByAnyMeans");
            ResetDecisions();
            DealDamage(legacy, bat, 5, DamageType.Melee);
            AssertAtLocation(bat, baroness.TurnTaker.OffToTheSide);
            AssertAtLocation(fortitude, legacy.TurnTaker.Trash);
            AssertAtLocation(new Card[] { link1, link2 }, baroness.TurnTaker.PlayArea);
            AssertAtLocation(bounty, chrono.TurnTaker.Trash);
        }

        [Test()]
        public void BatSelfDamageTest()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness", "SkyScraper", "ChronoRanger", "Bunker", "Legacy", "InsulaPrimalis");
            StartGame();

            Card cloud = PlayCard("CloudOfBats");
            Card fortitude = PutOnDeck("Fortitude");
            DealDamage(baroness, legacy, 1, DamageType.Melee);

            Card bat = FindCardsWhere((Card c) => c.Location.IsNextToCard && c.Location.OwnerCard == fortitude).FirstOrDefault();
            Assert.NotNull(bat);

            DecisionSelectCard = bat;
            Card link1 = PlayCard("CompulsionCanister");
            DealDamage(bat, bat, 5, DamageType.Melee);
            AssertAtLocation(bat, baroness.TurnTaker.OffToTheSide);
            AssertAtLocation(fortitude, legacy.TurnTaker.Trash);
            AssertAtLocation(link1, baroness.TurnTaker.PlayArea);
        }

        [Test()]
        public void BatSelfDamageTestNoNextToCard()
        {
            SetupGameController("VainFacadePlaytest.TheBaroness", "SkyScraper", "ChronoRanger", "Bunker", "Legacy", "InsulaPrimalis");
            StartGame();

            Card cloud = PlayCard("CloudOfBats");
            Card fortitude = PutOnDeck("Fortitude");
            DealDamage(baroness, legacy, 1, DamageType.Melee);

            Card bat = FindCardsWhere((Card c) => c.Location.IsNextToCard && c.Location.OwnerCard == fortitude).FirstOrDefault();
            Assert.NotNull(bat);

            DecisionSelectCard = bat;
            DestroyCard(bat, bat);
            //DealDamage(bat, bat, 5, DamageType.Melee);
            AssertAtLocation(bat, baroness.TurnTaker.OffToTheSide);
            AssertAtLocation(fortitude, legacy.TurnTaker.Trash);

            //Check that Bat cannot destroy cards while out of play anymore
            PlayCard(fortitude);
            DestroyCard(fortitude, bat);
            AssertIsInPlay(fortitude);
        }
    }
}