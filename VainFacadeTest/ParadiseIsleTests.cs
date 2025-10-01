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
    public class ParadiseIsleTests : BaseTest
    {
        protected TurnTakerController paradise { get { return FindEnvironment(); } }

        [Test()]
        public void TestBombshell()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();
            //At the start of the environment turn, Bombshell deals each non-conspirator target 2 fire damage
            Card bombshell = PlayCard("Bombshell");
            PutOnDeck("AdaptivePlatingSubroutine");
            PutOnDeck("DisintegrationRay");

            QuickHPStorage(omnitron.CharacterCard, ra.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, tachyon.CharacterCard, bombshell);
            GoToStartOfTurn(paradise);
            QuickHPCheck(-2, -2, -2, -2, -2, 0);
        }

        [Test()]
        public void TestDrWendigo()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();
            //"When {DrWendigo} deals damage, add a token to this card.",
            //"At the end of the environment turn, {DrWendigo} deals the target other than himself with the second highest HP X melee damage, where X = 2 plus the number of tokens on this card."

            //Check that he no longer has damage reduction
            Card wendigo = PlayCard("DrWendigo");
            QuickHPStorage(wendigo);
            DealDamage(legacy, wendigo, 2, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestGiselaCaro()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();

            //Increase damage dealt to this card by 1 for each token on this card.
            //At the end of the environment turn, you may reveal the top card of a deck. Discard or replace it. Then add a token to this card.
            Card gisela = PlayCard("GiselaCaro");
            TokenPool pool = gisela.FindTokenPool("GiselaPool");
            QuickHPStorage(gisela);
            DealDamage(legacy, gisela, 2, DamageType.Cold);
            QuickHPCheck(-2);

            PutOnDeck("AdaptivePlatingSubroutine");
            PutOnDeck("DisintegrationRay");

            QuickTokenPoolStorage(pool);
            GoToEndOfTurn(paradise);
            QuickTokenPoolCheck(1);

            QuickHPStorage(gisela);
            DealDamage(legacy, gisela, 2, DamageType.Cold);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestMercenarySquadReduce()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();

            //Reduce damage dealt to this card by 1.
            //At the start of the environment turn, this card deals the non-Conspirator target with the second highest HP X projectile damage, where X = the current HP of this card.

            Card merc = PlayCard("MercenarySquad");
            QuickHPStorage(merc);
            DealDamage(legacy, merc, 2, DamageType.Cold);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestMercenarySquad8Damage()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();

            //Reduce damage dealt to this card by 1.
            //At the start of the environment turn, this card deals the non-Conspirator target with the second highest HP X projectile damage, where X = the current HP of this card.
            PutOnDeck("AdaptivePlatingSubroutine");
            PutOnDeck("DisintegrationRay");

            Card merc = PlayCard("MercenarySquad");

            QuickHPStorage(omnitron, ra, legacy, bunker, tachyon);
            GoToStartOfTurn(paradise);
            QuickHPCheck(0, 0, -8, 0, 0);
        }

        [Test()]
        public void TestMercenarySquadLessDamage()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();

            //Reduce damage dealt to this card by 1.
            //At the start of the environment turn, this card deals the non-Conspirator target with the second highest HP X projectile damage, where X = the current HP of this card.
            PutOnDeck("AdaptivePlatingSubroutine");
            PutOnDeck("DisintegrationRay");

            Card merc = PlayCard("MercenarySquad");
            SetHitPoints(merc, 3);

            QuickHPStorage(omnitron, ra, legacy, bunker, tachyon);
            GoToStartOfTurn(paradise);
            QuickHPCheck(0, 0, -3, 0, 0);
        }

        [Test()]
        public void TestNoMrRooke()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();

            //"At the start of the environment turn, this card deals the hero target with the highest HP {H + 2} irreducible energy damage.",
            //"Then destroy this card."

            PutOnDeck("AdaptivePlatingSubroutine");
            PutOnDeck("DisintegrationRay");

            PlayCard("Fortitude");
            Card NoMrRooke = PlayCard("NoMrRookeIExpectYouToDie");

            QuickHPStorage(omnitron, ra, legacy, bunker, tachyon);
            GoToStartOfTurn(paradise);
            QuickHPCheck(0, 0, -6, 0, 0);
            AssertInTrash(NoMrRooke);
        }

        [Test()]
        public void TestSecretLairIncrease()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();

            //Increase damage dealt to conspirators by 1.
            //At the start of the environment turn, each conspirator deals the non-conspirator target with the second highest HP 2 projectile damage each.
            Card lair = PlayCard("SecretLair");
            Card goons = PlayCard("StaneksGoons");

            QuickHPStorage(goons);
            DealDamage(legacy, goons, 1, DamageType.Cold);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestSecretLairStartOfTurn()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();

            //Increase damage dealt to conspirators by 1.
            //At the start of the environment turn, each conspirator deals the non-conspirator target with the second highest HP 2 projectile damage each.
            Card lair = PlayCard("SecretLair");
            Card goons = PlayCard("StaneksGoons");
            Card wendigo = PlayCard("DrWendigo");

            PutOnDeck("AdaptivePlatingSubroutine");
            PutOnDeck("DisintegrationRay");

            QuickHPStorage(omnitron, ra, legacy, bunker, tachyon);
            GoToStartOfTurn(paradise);
            QuickHPCheck(0,-2,-2,0,0);
        }

        [Test()]
        public void TestSinisterLaboratory()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();

            //At the end of the environment turn, play the top card of the environment deck.
            PutOnDeck("AdaptivePlatingSubroutine");
            PutOnDeck("DisintegrationRay");

            Card lab = PlayCard("SinisterLaboratory");
            Card test = PutOnDeck("TestSubjects");

            GoToEndOfTurn(paradise);
            AssertIsInPlay(test);
        }

        [Test()]
        public void TestStaneksGoons()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();

            //At the end of the environment turn, this card deals the 2 non-conspirator targets with the lowest HP X projectile damage each, where X is the number of conspirators in play.
            Card goons = PlayCard("StaneksGoons");
            PutOnDeck("AdaptivePlatingSubroutine");
            PutOnDeck("DisintegrationRay");
            Card bombshell = PlayCard("Bombshell");

            GoToStartOfTurn(paradise);
            QuickHPStorage(omnitron.CharacterCard, ra.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, tachyon.CharacterCard, goons, bombshell);
            GoToEndOfTurn(paradise);
            QuickHPCheck(0, 0, 0, -2, -2, 0, 0);
        }

        [Test()]
        public void TestTestSubjectsEndOfTurn()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();

            //At the end of the environment turn, {DrWendigo} regains 2 HP, then deals this card 2 toxic damage.
            PutOnDeck("AdaptivePlatingSubroutine");
            PutOnDeck("DisintegrationRay");
            Card test = PlayCard("TestSubjects");
            Card wendigo = PlayCard("DrWendigo");
            SetHitPoints(wendigo, 5);

            QuickHPStorage(test, wendigo);
            GoToEndOfTurn(paradise);
            QuickHPCheck(-2, 2);
        }

        [Test()]
        public void TestTestSubjectsEndOfTurnNoWendigo()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();

            //At the end of the environment turn, {DrWendigo} regains 2 HP, then deals this card 2 toxic damage.
            PutOnDeck("AdaptivePlatingSubroutine");
            PutOnDeck("DisintegrationRay");
            Card test = PlayCard("TestSubjects");

            QuickHPStorage(test);
            GoToEndOfTurn(paradise);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestTestSubjectsStartOfTurn()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();

            //At the start of the environment turn, 1 player may draw a card.
            PutOnDeck("AdaptivePlatingSubroutine");
            PutOnDeck("DisintegrationRay");
            Card test = PlayCard("TestSubjects");

            QuickHandStorage(legacy);
            DecisionSelectTurnTaker = legacy.TurnTaker;
            GoToStartOfTurn(paradise);
            QuickHandCheck(1);
        }

        [Test()]
        public void TestTightHallways()
        {
            SetupGameController("Omnitron", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.ParadiseIsle");
            StartGame();

            //Increase projectile damage dealt by 1.
            //Reduce non-projectile damage dealt by 1.
            Card hallways = PlayCard("TightHallways");

            QuickHPStorage(omnitron);
            DealDamage(legacy, omnitron, 2, DamageType.Projectile, true);
            QuickHPCheck(-3);

            QuickHPStorage(omnitron);
            DealDamage(legacy, omnitron, 2, DamageType.Toxic);
            QuickHPCheck(-1);
        }
    }
}