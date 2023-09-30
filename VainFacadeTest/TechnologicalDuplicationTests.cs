using NUnit.Framework;
using System;
using VainFacadePlaytest;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using System.Collections.Generic;
using VainFacadePlaytest.Friday;
using System.Runtime.Serialization;
using System.Security.Policy;
using Handelabra;

namespace VainFacadeTest
{
    [TestFixture()]
    public class TechnologicalDuplicationTests : BaseTest
    {
        //Heroes
        protected HeroTurnTakerController friday { get { return FindHero("Friday"); } }
        protected HeroTurnTakerController ember { get { return FindHero("Ember"); } }
        protected HeroTurnTakerController burgess { get { return FindHero("Burgess"); } }
        protected HeroTurnTakerController node { get { return FindHero("Node"); } }
        protected HeroTurnTakerController sphere { get { return FindHero("Sphere"); } }
        protected HeroTurnTakerController fury { get { return FindHero("TheFury"); } }
        protected HeroTurnTakerController carnaval { get { return FindHero("Carnaval"); } }

        //Villains
        protected TurnTakerController baroness { get { return FindVillain("TheBaroness"); } }
        protected TurnTakerController blitz { get { return FindVillain("Grandfather"); } }

        protected string techID = "TechnologicalDuplication";

        [Test()]
        public void TestTechnologicalDuplicationLightning()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Increase lightning damage dealt to {Friday} by 1.
            //Play this card next to an equipment card from another deck without a copy of this card beside it. This card gains the text of that card. Treat the name of the hero on that card as {Friday}, and “you” on that card as {Friday}'s player.
            //When that card leaves play, destroy this card.
            Card ring = PlayCard("TheLegacyRing");
            Card duplication = PlayCard(techID);
            QuickHPStorage(friday);
            DealDamage(legacy, friday, 1, DamageType.Lightning);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestTechnologicalDuplicationNoequipment()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Increase lightning damage dealt to {Friday} by 1.
            //Play this card next to an equipment card from another deck without a copy of this card beside it. This card gains the text of that card. Treat the name of the hero on that card as {Friday}, and “you” on that card as {Friday}'s player.
            //When that card leaves play, destroy this card.
            Card duplication = PlayCard(techID);
            AssertInTrash(duplication);
        }

        [Test()]
        public void TestTechnologicalDuplication()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Increase lightning damage dealt to {Friday} by 1.
            //Play this card next to an equipment card from another deck without a copy of this card beside it. This card gains the text of that card. Treat the name of the hero on that card as {Friday}, and “you” on that card as {Friday}'s player.
            //When that card leaves play, destroy this card.
            Card flak = PlayCard("FlakCannon");
            Card duplication = PlayCard(techID);
            AssertAtLocation(duplication, flak.NextToLocation);

            QuickHPStorage(akash);
            UsePower(friday, 1);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestTechnologicalDuplicationDestroy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Increase lightning damage dealt to {Friday} by 1.
            //Play this card next to an equipment card from another deck without a copy of this card beside it. This card gains the text of that card. Treat the name of the hero on that card as {Friday}, and “you” on that card as {Friday}'s player.
            //When that card leaves play, destroy this card.
            Card flak = PlayCard("FlakCannon");
            Card duplication = PlayCard(techID);
            AssertAtLocation(duplication, flak.NextToLocation);

            DestroyCard(flak);
            AssertInTrash(duplication);
        }

        [Test()]
        public void TestBattleZonesPower()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });

            StartGame();

            Card flak = PlayCard("FlakCannon");
            DecisionSelectCard = flak;
            Card duplication = PlayCard(techID);

            DecisionSelectTarget = progScion;
            QuickHPStorage(progScion);
            UsePower(friday, 1);
            QuickHPCheck(-3);

            SwitchBattleZone(friday);

            //Console.WriteLine($"Friday is in {friday.BattleZone.Identifier}");
            //Console.WriteLine($"Legacy is in {legacy.BattleZone.Identifier}");
            //Console.WriteLine($"Is charge visible to grim: {GameController.IsCardVisibleToCardSource(charge,FindCardController(grim).GetCardSource())}");

            DecisionSelectTarget = mindScion;
            QuickHPStorage(mindScion);

            try
            {
                UsePower(friday, 1);
            }
            catch
            {
                Console.WriteLine("Technological Duplication is no longer copying the power from Motivational Charge");
            }

            QuickHPCheck(0);
        }

        [Test()]
        public void TestPostDestroyAction()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "VoidGuardMainstay", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            Card belter = PlayCard("VoidBelter");
            Card duplication = PlayCard(techID);

            //Check that when Technological Duplication is destroyed, friday deals 2 and 2 damage to Akash'Bhuta
            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(akash);
            DestroyCard(duplication);
            QuickHPCheck(-4);
        }
    }
}
