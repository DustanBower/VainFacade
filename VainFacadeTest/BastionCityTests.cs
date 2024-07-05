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
    public class BastionCityTests : BaseTest
    {
        protected TurnTakerController bastion { get { return FindEnvironment(); } }

        [Test()]
        public void TestAForkInTheRoadDiscardTarget()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, destroy a machination and this card.
            //When this card is destroyed, discard the top card of the environment deck. If that card is not a target, play the top card of the villain deck. Otherwise a player may play a card.
            Card fork = PlayCard("AForkInTheRoad");
            Card business = PlayCard("BusinessAsUsual");
            Card mathis = PutOnDeck("BrotherMathis");
            Card fortitude = PutInHand("Fortitude");
            Card gauntlet = PutOnDeck("GauntletOfPerdition");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionSelectCardToPlay = fortitude;

            GoToEndOfTurn(bastion);
            AssertInTrash(new Card[] { fork, business, mathis });
            AssertIsInPlay(fortitude);
            AssertOnTopOfDeck(gauntlet);
        }

        [Test()]
        public void TestAForkInTheRoadDiscardNonTarget()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, destroy a machination and this card.
            //When this card is destroyed, discard the top card of the environment deck. If that card is not a target, play the top card of the villain deck. Otherwise a player may play a card.
            Card fork = PlayCard("AForkInTheRoad");
            Card business = PlayCard("BusinessAsUsual");
            Card clean = PutOnDeck("CleanStreetsDirtyHands");
            Card fortitude = PutInHand("Fortitude");
            Card gauntlet = PutOnDeck("GauntletOfPerdition");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionSelectCardToPlay = fortitude;

            GoToEndOfTurn(bastion);
            AssertInTrash(new Card[] { fork, business, clean });
            AssertIsInPlay(gauntlet);
            AssertInHand(fortitude);
        }

        [Test()]
        public void TestAForkInTheRoadDiscardNonTargetOA()
        {
            SetupGameController(new string[] { "OblivAeon", "Bunker", "Legacy", "Haka", "VainFacadePlaytest.BastionCity", "MobileDefensePlatform", "InsulaPrimalis", "RuinsOfAtlantis", "Magmaria" }, shieldIdentifier: "ThePrimaryObjective");
            StartGame();
            //At the end of the environment turn, destroy a machination and this card.
            //When this card is destroyed, discard the top card of the environment deck. If that card is not a target, play the top card of the villain deck. Otherwise a player may play a card.
            GoToPlayCardPhase(FindEnvironment(bzOne));
            Card fork = PlayCard("AForkInTheRoad");
            Card business = PlayCard("BusinessAsUsual");
            Card clean = PutOnDeck("CleanStreetsDirtyHands");
            Card thrall = MoveCard(oblivaeon,"AeonThrall",aeonDeck);
            Card vassal = PlayCard("AeonVassal");

            AssertNextDecisionChoices(new LocationChoice[] { new LocationChoice(oblivaeon.TurnTaker.Deck), new LocationChoice(scionDeck), new LocationChoice(aeonDeck) }, new LocationChoice[] { new LocationChoice(missionDeck) });
            DecisionSelectLocation = new LocationChoice(aeonDeck);

            GoToEndOfTurn(FindEnvironment(bzOne));
            AssertIsInPlay(thrall);
        }

        [Test()]
        public void TestAForkInTheRoadDiscardNonTargetNoScionOA()
        {
            SetupGameController(new string[] { "OblivAeon", "Bunker", "Legacy", "Haka", "VainFacadePlaytest.BastionCity", "MobileDefensePlatform", "InsulaPrimalis", "RuinsOfAtlantis", "Magmaria" }, shieldIdentifier: "PrimaryObjective");
            StartGame();
            //At the end of the environment turn, destroy a machination and this card.
            //When this card is destroyed, discard the top card of the environment deck. If that card is not a target, play the top card of the villain deck. Otherwise a player may play a card.
            GoToPlayCardPhase(FindEnvironment(bzOne));
            Card fork = PlayCard("AForkInTheRoad");
            Card business = PlayCard("BusinessAsUsual");
            Card clean = PutOnDeck("CleanStreetsDirtyHands");
            Card thrall = MoveCard(oblivaeon, "AeonThrall", aeonDeck);
            Card vassal = PlayCard("AeonVassal");

            AssertNextDecisionChoices(new LocationChoice[] { new LocationChoice(oblivaeon.TurnTaker.Deck), new LocationChoice(aeonDeck) }, new LocationChoice[] { new LocationChoice(scionDeck), new LocationChoice(missionDeck) });
            DecisionSelectLocation = new LocationChoice(aeonDeck);

            GoToEndOfTurn(FindEnvironment(bzOne));
            AssertIsInPlay(thrall);
        }

        [Test()]
        public void TestAForkInTheRoadOtherDestroy()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, destroy a machination and this card.
            //When this card is destroyed, discard the top card of the environment deck. If that card is not a target, play the top card of the villain deck. Otherwise a player may play a card.
            Card fork = PlayCard("AForkInTheRoad");
            Card mathis = PutOnDeck("BrotherMathis");
            Card fortitude = PutInHand("Fortitude");
            Card gauntlet = PutOnDeck("GauntletOfPerdition");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionSelectCardToPlay = fortitude;

            DestroyCard(fork);
            AssertInTrash(new Card[] { fork, mathis });
            AssertIsInPlay(fortitude);
            AssertOnTopOfDeck(gauntlet);
        }

        [Test()]
        public void TestBrotherMathisNonEnvironment()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, this card deals the target with the lowest HP {H - 2} infernal damage.
            //If an environment card is dealt damage this way, this card deals each non-environment target 2 infernal damage.
            GoToPlayCardPhase(bastion);
            Card mathis = PlayCard("BrotherMathis");
            Card gauntlet = PlayCard("GauntletOfPerdition");
            QuickHPStorage(gauntlet, apostate.CharacterCard, ra.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, tachyon.CharacterCard, mathis);
            GoToEndOfTurn(bastion);
            QuickHPCheck(-2, 0, 0, 0, 0, 0, 0);
        }

        [Test()]
        public void TestBrotherMathisEnvironment()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, this card deals the target with the lowest HP {H - 2} infernal damage.
            //If an environment card is dealt damage this way, this card deals each non-environment target 2 infernal damage.
            GoToPlayCardPhase(bastion);
            Card mathis = PlayCard("BrotherMathis");
            QuickHPStorage(apostate.CharacterCard, ra.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, tachyon.CharacterCard, mathis);
            GoToEndOfTurn(bastion);
            QuickHPCheck(-2, -2, -2, -2, -2, -2);
        }

        [Test()]
        public void TestBusinessAsUsualDiscard()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, 1 player may discard a card. If they do, select a target. Reduce the next damage dealt to that target by 2. If they do not, deal the non-environment target with the lowest hp 2 melee damage.
            GoToPlayCardPhase(bastion);
            Card business = PlayCard("BusinessAsUsual");
            Card fortitude = PutInHand("Fortitude");
            Card gauntlet = PlayCard("GauntletOfPerdition");

            DecisionDiscardCard = fortitude;
            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionSelectCard = legacy.CharacterCard;

            //Check that if a player discards a card, Gauntlet is not dealt damage
            QuickHPStorage(gauntlet);
            GoToEndOfTurn(bastion);
            AssertInTrash(fortitude);
            QuickHPCheck(0);

            //Check that the next damage to Legacy is reduced by 2
            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 3, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 3, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestBusinessAsUsualNoDiscard()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, 1 player may discard a card. If they do, select a target. Reduce the next damage dealt to that target by 2. If they do not, deal the non-environment target with the lowest hp 2 melee damage.
            GoToPlayCardPhase(bastion);
            Card business = PlayCard("BusinessAsUsual");
            Card fortitude = PutInHand("Fortitude");
            Card gauntlet = PlayCard("GauntletOfPerdition");

            DecisionDiscardCard = fortitude;
            DecisionDoNotSelectTurnTaker = true;
            DecisionSelectCard = legacy.CharacterCard;

            //Check that if a player does not discard a card, Gauntlet is dealt 2 damage
            QuickHPStorage(gauntlet);
            GoToEndOfTurn(bastion);
            AssertInHand(fortitude);
            QuickHPCheck(-2);

            //Check that the next damage to Legacy is not reduced by 2
            QuickHPStorage(legacy);
            DealDamage(ra, legacy, 3, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestBystanderDamage()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //When this card is dealt damage, one player discards a card.
            //When this card reduced to 0 or fewer hp, each hero target deals itself 1 irreducible psychic damage. Then shuffle this card into the environment deck.
            Card bystander = PlayCard("Bystander");

            QuickHandStorage(ra, legacy, bunker, tachyon);
            DecisionSelectTurnTaker = legacy.TurnTaker;
            DealDamage(ra, bystander, 1, DamageType.Melee);
            QuickHandCheck(0, -1, 0, 0);
        }

        [Test()]
        public void TestBystanderDestroy()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //When this card is dealt damage, one player discards a card.
            //When this card reduced to 0 or fewer hp, each hero target deals itself 1 irreducible psychic damage. Then shuffle this card into the environment deck.
            Card bystander = PlayCard("Bystander");

            QuickHPStorage(apostate, ra, legacy, bunker, tachyon);
            QuickShuffleStorage(bastion);
            QuickHandStorage(ra, legacy, bunker, tachyon);

            DealDamage(ra, bystander, 5, DamageType.Fire);

            QuickHPCheck(0, -1, -1, -1, -1);
            QuickShuffleCheck(1);
            AssertAtLocation(bystander, bastion.TurnTaker.Deck);
            QuickHandCheckZero();

            PlayCard(bystander);
            QuickHPStorage(apostate, ra, legacy, bunker, tachyon);
            QuickShuffleStorage(bastion);
            Card blinding = PutInTrash("BlindingSpeed");
            QuickHandStorage(ra, legacy, bunker, tachyon);

            DecisionSelectCard = bystander;
            PlayCard(blinding);

            QuickHPCheckZero();
            QuickShuffleCheck(0);
            AssertAtLocation(bystander, bastion.TurnTaker.Trash);
            QuickHandCheckZero();
        }

        [Test()]
        public void TestBystanderDestroyOA()
        {
            SetupGameController(new string[] { "OblivAeon", "Bunker", "Legacy", "Tachyon", "VainFacadePlaytest.BastionCity", "MobileDefensePlatform", "InsulaPrimalis", "RuinsOfAtlantis", "Magmaria" }, shieldIdentifier: "TheArcOfUnreality");
            StartGame();
            //When this card is dealt damage, one player discards a card.
            //When this card reduced to 0 or fewer hp, each hero target deals itself 1 irreducible psychic damage. Then shuffle this card into the environment deck.
            Card bystander = PlayCard("Bystander");
            Card shield = GetCard("TheArcOfUnreality");
            FlipCard(shield);

            QuickShuffleStorage(bastion);
            QuickHPStorage(bunker, legacy, tachyon);
            DecisionSelectTurnTaker = bunker.TurnTaker;
            QuickHandStorage(bunker);
            
            //Check that if the destruction is prevented, Bystander does not go off
            DealDamage(bunker, bystander, 5, DamageType.Fire);
            QuickShuffleCheck(0);
            QuickHPCheck(0, 0, 0);
            QuickHandCheck(-3);
            AssertIsInPlay(bystander);

            //Check that if it is destroyed when at 0 or less HP, it does go off, even if it wasn't destroyed by damage
            DecisionDoNotSelectTurnTaker = true;
            QuickShuffleStorage(bastion);
            QuickHPStorage(bunker, legacy, tachyon);
            QuickHandStorage(bunker);
            PlayCard("BlindingSpeed");
            QuickShuffleCheck(1);
            QuickHPCheck(-1, -1, -1);
            QuickHandCheck(0);
            AssertAtLocation(bystander,bastion.TurnTaker.Deck);
        }

        [Test()]
        public void TestBystanderJackHandle()
        {
            SetupGameController("Apostate", "MrFixer", "Haka", "Legacy", "VainFacadePlaytest.BastionCity");
            StartGame();

            Card bystander = PlayCard("Bystander");
            Card jack = PlayCard("JackHandle");

            QuickHPStorage(apostate, fixer, haka, legacy);
            QuickHandStorage(fixer, haka, legacy);
            DealDamage(legacy, bystander, 5, DamageType.Melee);
            QuickHPCheck(-1, 0, -1, -1);
            QuickHandCheckZero();
        }

        [Test()]
        public void TestCleanStreetsDirtyHands()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, discard the top {H - 1} cards of the environment deck. When a non-machination card is discarded this way with a keyword matching a card in play, put it into play.
            GoToPlayCardPhase(bastion);
            Card clean = PlayCard("CleanStreetsDirtyHands");
            Card tithe = PlayCard("DarkTithe");
            Card business = PutOnDeck("BusinessAsUsual");
            Card goodman = PutOnDeck("MrGoodman");
            Card grim = PutOnDeck("GrimSebastian");
            DecisionDoNotSelectTurnTaker = true;

            //Business as Usual and Grim Sebastian should be in the trash, and Mr Goodman should be in play.
            GoToEndOfTurn(bastion);
            AssertInTrash(business, grim);
            AssertIsInPlay(goodman);
        }

        [Test()]
        public void TestCleanStreetsDirtyHandsOneCard()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, discard the top {H - 1} cards of the environment deck. When a non-machination card is discarded this way with a keyword matching a card in play, put it into play.
            GoToPlayCardPhase(bastion);
            PlayCard("CleanStreetsDirtyHands");
            MoveAllCards(bastion, bastion.TurnTaker.Deck, bastion.TurnTaker.OutOfGame);
            Card grim = MoveCard(bastion,"GrimSebastian",bastion.TurnTaker.Deck);
            QuickShuffleStorage(bastion.TurnTaker.Deck);
            GoToEndOfTurn(bastion);
            QuickShuffleCheck(2);
            AssertInTrash(grim);
        }

        [Test()]
        public void TestCleanStreetsDirtyHandsEmptyDeck()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, discard the top {H - 1} cards of the environment deck. When a non-machination card is discarded this way with a keyword matching a card in play, put it into play.
            GoToPlayCardPhase(bastion);
            PlayCard("CleanStreetsDirtyHands");
            MoveAllCards(bastion, bastion.TurnTaker.Deck, bastion.TurnTaker.OutOfGame);
            QuickShuffleStorage(bastion.TurnTaker.Deck);
            GoToEndOfTurn(bastion);
            QuickShuffleCheck(3);
        }

        [Test()]
        public void TestDarkTitheStartOfTurn()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the start of the environment turn, discard the top {H - 2} cards of the environment deck. If {MrGoodman} is discarded this way, put him into play.
            Card clean = PutOnDeck("CleanStreetsDirtyHands");
            Card goodman = PutOnDeck("MrGoodman");
            Card business = PutOnDeck("BusinessAsUsual");

            Card tithe = PlayCard("DarkTithe");

            GoToStartOfTurn(bastion);
            AssertOnTopOfDeck(clean);
            AssertInTrash(business);
            AssertIsInPlay(goodman);
        }

        [Test()]
        public void TestDarkTitheEndOfTurn()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, a player may discard a card. If a card is discarded this way, then 1 player may play a card.
            GoToPlayCardPhase(bastion);
            Card tithe = PlayCard("DarkTithe");

            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card fortitude = PutInHand("Fortitude");
            Card danger = PutInHand("DangerSense");
            DecisionSelectCardToPlay = fortitude;
            DecisionDiscardCard = danger;

            AssertDecisionIsOptional(SelectionType.DiscardCard);
            GoToEndOfTurn(bastion);
            AssertInTrash(danger);
            AssertIsInPlay(fortitude);
        }

        [Test()]
        public void TestGrimSebastianStartOfTurn()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the start of the environment turn, discard a card from under this card. This card deals the non-keeper card with the highest HP from that card's deck {H - 2} melee damage.
            GoToEndOfTurn(tachyon);
            Card grim = PlayCard("GrimSebastian");
            Card gauntlet = PlayCard("GauntletOfPerdition");
            Card apocalypse = MoveCard(bastion, "Apocalypse", grim.UnderLocation);

            QuickHPStorage(apostate.CharacterCard, gauntlet);
            GoToStartOfTurn(bastion);
            QuickHPCheck(-2, 0);
            AssertInTrash(apocalypse);
        }

        [Test()]
        public void TestGrimSebastianEndOfTurn()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, reveal and discard the top card of each deck. Put the revealed card with the most keywords under this card.
            GoToPlayCardPhase(bastion);
            Card grim = PlayCard("GrimSebastian");
            Card fortitude = PutOnDeck("Fortitude");
            Card staff = PutOnDeck("TheStaffOfRa");
            Card ammo = PutOnDeck("AmmoDrop");
            Card blinding = PutOnDeck("BlindingSpeed");
            Card apocalypse = PutOnDeck("Apocalypse");
            Card tithe = PutOnDeck("DarkTithe");

            GoToEndOfTurn(bastion);
            AssertInTrash(new Card[] { fortitude, ammo, blinding, apocalypse });
            AssertAtLocation(staff, grim.UnderLocation);
        }

        [Test()]
        public void TestGrimSebastianEndOfTurnSameNumberOfKeywords()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, reveal and discard the top card of each deck. Put the revealed card with the most keywords under this card.
            GoToPlayCardPhase(bastion);
            Card grim = PlayCard("GrimSebastian");
            Card fortitude = PutOnDeck("Fortitude");
            Card blazing = PutOnDeck("BlazingTornado");
            Card ammo = PutOnDeck("AmmoDrop");
            Card blinding = PutOnDeck("BlindingSpeed");
            Card apocalypse = PutOnDeck("Apocalypse");
            Card tithe = PutOnDeck("DarkTithe");

            AssertNextDecisionChoices(new Card[] { fortitude, blazing, ammo, blinding, tithe }, new Card[] { apocalypse });
            DecisionSelectCard = fortitude;
            GoToEndOfTurn(bastion);
            AssertInTrash(new Card[] { blazing, ammo, blinding, apocalypse });
            AssertAtLocation(fortitude, grim.UnderLocation);
        }

        [Test()]
        public void TestGrimSebastianEndOfTurnOA()
        {
            SetupGameController(new string[] { "OblivAeon","Ra", "Bunker", "Legacy", "Tachyon", "VainFacadePlaytest.BastionCity", "MobileDefensePlatform", "InsulaPrimalis", "RuinsOfAtlantis", "Magmaria" }, shieldIdentifier: "ThePrimaryObjective");
            StartGame();
            //At the end of the environment turn, reveal and discard the top card of each deck. Put the revealed card with the most keywords under this card.
            GoToPlayCardPhase(bastion);
            Card grim = PlayCard("GrimSebastian");
            Card fortitude = PutOnDeck("Fortitude");
            Card staff = PutOnDeck("TheStaffOfRa");
            Card ammo = PutOnDeck("AmmoDrop");
            Card blinding = PutOnDeck("BlindingSpeed");
            Card focus = PutOnDeck("FocusOfPower");
            Card thrall = MoveCard(oblivaeon, "AeonThrall", aeonDeck);
            Card vassal = PlayCard("AeonVassal");
            Card scatter = MoveCard(oblivaeon, "ScatterSlaughter", scionDeck);
            Card tithe = PutOnDeck("DarkTithe");

            IEnumerable<Location> list = new Location[] { oblivaeon.TurnTaker.Deck, scionDeck, aeonDeck };
            AssertNextDecisionChoices(list.Select((Location L) => new LocationChoice(L)),new LocationChoice[] { new LocationChoice(missionDeck) });
            GoToEndOfTurn(bastion);
        }

        [Test()]
        public void TestGrimSebastianEndOfTurnOANoScion()
        {
            SetupGameController(new string[] { "OblivAeon", "Ra", "Bunker", "Legacy", "Tachyon", "VainFacadePlaytest.BastionCity", "MobileDefensePlatform", "InsulaPrimalis", "RuinsOfAtlantis", "Magmaria" }, shieldIdentifier: "PrimaryObjective");
            StartGame();
            //At the end of the environment turn, reveal and discard the top card of each deck. Put the revealed card with the most keywords under this card.
            GoToPlayCardPhase(bastion);
            Card grim = PlayCard("GrimSebastian");
            Card fortitude = PutOnDeck("Fortitude");
            Card staff = PutOnDeck("TheStaffOfRa");
            Card ammo = PutOnDeck("AmmoDrop");
            Card blinding = PutOnDeck("BlindingSpeed");
            Card focus = PutOnDeck("FocusOfPower");
            Card thrall = MoveCard(oblivaeon, "AeonThrall", aeonDeck);
            Card vassal = PlayCard("AeonVassal");
            Card scatter = MoveCard(oblivaeon, "ScatterSlaughter", scionDeck);
            Card tithe = PutOnDeck("DarkTithe");

            IEnumerable<Location> list = new Location[] { oblivaeon.TurnTaker.Deck, aeonDeck };
            AssertNextDecisionChoices(list.Select((Location L) => new LocationChoice(L)), new LocationChoice[] {new LocationChoice(scionDeck), new LocationChoice(missionDeck) });
            GoToEndOfTurn(bastion);
        }

        [Test()]
        public void TestLegitimateBusinessmen()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, destroy a hero ongoing or equipment card.
            //Then this card deals the non-coalition target with the lowest hp 2 melee damage.
            GoToPlayCardPhase(bastion);
            Card legit = PlayCard("LegitimateBusinessmen");
            Card fortitude = PlayCard("Fortitude");
            Card flak = PlayCard("FlakCannon");
            Card gauntlet = PlayCard("GauntletOfPerdition");
            Card apocalypse = PlayCard("Apocalypse");

            AssertNextDecisionChoices(new Card[] { fortitude, flak }, new Card[] { apocalypse, gauntlet });
            DecisionSelectCard = fortitude;
            QuickHPStorage(gauntlet, ra.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, tachyon.CharacterCard, legit);
            GoToEndOfTurn(bastion);
            AssertInTrash(fortitude);
            AssertIsInPlay(new Card[] { flak, apocalypse });
            QuickHPCheck(-2, 0, 0, 0, 0, 0);
        }

        [Test()]
        public void TestMrGoodman()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the end of the environment turn, select a deck with no cards under this card. Put the top card of that deck under this card, then put the top card of that deck into play.
            //Then, this card deals each target from a deck with a card under this one 2 psychic damage each.
            GoToPlayCardPhase(bastion);
            Card goodman = PlayCard("MrGoodman");
            Card fortitude = MoveCard(legacy, "Fortitude", goodman.UnderLocation);
            Card flak = PutOnDeck("FlakCannon");
            Card ammo = PutOnDeck("AmmoDrop");

            QuickHPStorage(ra.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, tachyon.CharacterCard, apostate.CharacterCard, goodman);
            List<Location> locations = new Location[] { ra.TurnTaker.Deck, bunker.TurnTaker.Deck, tachyon.TurnTaker.Deck, apostate.TurnTaker.Deck }.ToList();
            AssertNextDecisionChoices(locations.Select((Location L) => new LocationChoice(L)), new LocationChoice[] { new LocationChoice(legacy.TurnTaker.Deck) });
            DecisionSelectLocation = new LocationChoice(bunker.TurnTaker.Deck);

            GoToEndOfTurn(bastion);
            AssertAtLocation(ammo, goodman.UnderLocation);
            AssertIsInPlay(flak);
            QuickHPCheck(0, -2, -2, 0, 0, 0);
        }

        [Test()]
        public void TestMrGoodmanOA()
        {
            SetupGameController(new string[] { "OblivAeon", "Ra", "Bunker", "Legacy", "Tachyon", "VainFacadePlaytest.BastionCity", "MobileDefensePlatform", "InsulaPrimalis", "RuinsOfAtlantis", "Magmaria" }, shieldIdentifier: "ThePrimaryObjective");
            StartGame();
            //At the end of the environment turn, select a deck with no cards under this card. Put the top card of that deck under this card, then put the top card of that deck into play.
            //Then, this card deals each target from a deck with a card under this one 2 psychic damage each.
            GoToPlayCardPhase(FindEnvironment(bzOne));
            Card goodman = PlayCard("MrGoodman");
            Card locus = MoveCard(oblivaeon, "AeonLocus", aeonDeck);
            Card thrall = MoveCard(oblivaeon, "AeonThrall", aeonDeck);
            Card vassal = PlayCard("AeonVassal");

            IEnumerable<Location> list = new Location[] { oblivaeon.TurnTaker.Deck, scionDeck, aeonDeck };
            AssertNextDecisionChoices(list.Select((Location L) => new LocationChoice(L)), new LocationChoice[] {new LocationChoice(missionDeck) });
            DecisionSelectLocation = new LocationChoice(aeonDeck);
            GoToEndOfTurn(FindEnvironment(bzOne));
            AssertAtLocation(thrall, goodman.UnderLocation);
            AssertIsInPlay(locus);
        }

        [Test()]
        public void TestMrGoodmanOANoScion()
        {
            SetupGameController(new string[] { "OblivAeon", "Ra", "Bunker", "Legacy", "Tachyon", "VainFacadePlaytest.BastionCity", "MobileDefensePlatform", "InsulaPrimalis", "RuinsOfAtlantis", "Magmaria" }, shieldIdentifier: "PrimaryObjective");
            StartGame();
            //At the end of the environment turn, select a deck with no cards under this card. Put the top card of that deck under this card, then put the top card of that deck into play.
            //Then, this card deals each target from a deck with a card under this one 2 psychic damage each.
            GoToPlayCardPhase(FindEnvironment(bzOne));
            Card goodman = PlayCard("MrGoodman");
            Card locus = MoveCard(oblivaeon, "AeonLocus", aeonDeck);
            Card thrall = MoveCard(oblivaeon, "AeonThrall", aeonDeck);
            Card vassal = PlayCard("AeonVassal");

            IEnumerable<Location> list = new Location[] { oblivaeon.TurnTaker.Deck, aeonDeck };
            AssertNextDecisionChoices(list.Select((Location L) => new LocationChoice(L)),new LocationChoice[] { new LocationChoice(scionDeck), new LocationChoice(missionDeck) });
            DecisionSelectLocation = new LocationChoice(aeonDeck);
            GoToEndOfTurn(FindEnvironment(bzOne));
            AssertAtLocation(thrall, goodman.UnderLocation);
            AssertIsInPlay(locus);
        }

        [Test()]
        public void TestPlayingBothSidesPlay()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();

            //When this card enters play, put the top card of each deck under it.
            //At the end of the environment turn, a player may discard 2 cards. If they do not, put the top card of the villain deck under this card.
            //At the start of the environment turn, put a random card from under this card into play. If a one-shot or environment card is played this way, destroy this card.
            Card fortitude = PutOnDeck("Fortitude");
            Card ammo = PutOnDeck("AmmoDrop");
            Card blast = PutOnDeck("FireBlast");
            Card blinding = PutOnDeck("BlindingSpeed");
            Card apocalypse = PutOnDeck("BlindingSpeed");
            Card grim = PutOnDeck("GrimSebastian");

            Card sides = PlayCard("PlayingBothSides");
            AssertAtLocation(new Card[] { fortitude, ammo, blast, blinding, apocalypse, grim }, sides.UnderLocation);
        }

        [Test()]
        public void TestPlayingBothSidesEndDiscard()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();

            //When this card enters play, put the top card of each deck under it.
            //At the end of the environment turn, a player may discard 2 cards. If they do not, put the top card of the villain deck under this card.
            //At the start of the environment turn, put a random card from under this card into play. If a one-shot or environment card is played this way, destroy this card.
            Card fortitude = PutOnDeck("Fortitude");
            Card ammo = PutOnDeck("AmmoDrop");
            Card blast = PutOnDeck("FireBlast");
            Card blinding = PutOnDeck("BlindingSpeed");
            Card apocalypse = PutOnDeck("BlindingSpeed");
            Card grim = PutOnDeck("GrimSebastian");

            GoToPlayCardPhase(bastion);
            Card sides = PlayCard("PlayingBothSides");
            AssertAtLocation(new Card[] { fortitude, ammo, blast, blinding, apocalypse, grim }, sides.UnderLocation);

            DecisionSelectTurnTaker = bunker.TurnTaker;
            Card imp = PutOnDeck("ImpPilferer");
            QuickHandStorage(bunker);

            GoToEndOfTurn(bastion);
            QuickHandCheck(-2);
            AssertOnTopOfDeck(imp);
        }

        [Test()]
        public void TestPlayingBothSidesEndNoDiscard()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();

            //When this card enters play, put the top card of each deck under it.
            //At the end of the environment turn, a player may discard 2 cards. If they do not, put the top card of the villain deck under this card.
            //At the start of the environment turn, put a random card from under this card into play. If a one-shot or environment card is played this way, destroy this card.
            Card fortitude = PutOnDeck("Fortitude");
            Card ammo = PutOnDeck("AmmoDrop");
            Card blast = PutOnDeck("FireBlast");
            Card blinding = PutOnDeck("BlindingSpeed");
            Card apocalypse = PutOnDeck("BlindingSpeed");
            Card grim = PutOnDeck("GrimSebastian");

            GoToPlayCardPhase(bastion);
            Card sides = PlayCard("PlayingBothSides");
            AssertAtLocation(new Card[] { fortitude, ammo, blast, blinding, apocalypse, grim }, sides.UnderLocation);

            DecisionDoNotSelectTurnTaker = true;
            Card imp = PutOnDeck("ImpPilferer");
            QuickHandStorage(bunker);

            GoToEndOfTurn(bastion);
            QuickHandCheck(0);
            AssertAtLocation(imp, sides.UnderLocation);
        }

        [Test()]
        public void TestPlayingBothSidesStartNonOneShot()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();

            //When this card enters play, put the top card of each deck under it.
            //At the end of the environment turn, a player may discard 2 cards. If they do not, put the top card of the villain deck under this card.
            //At the start of the environment turn, put a random card from under this card into play. If a one-shot or environment card is played this way, destroy this card.
            Card fortitude = PutOnDeck("Fortitude");
            Card ammo = PutOnDeck("AmmoDrop");
            Card blazing = PutOnDeck("BlazingTornado");
            Card goggles = PutOnDeck("HUDGoggles");
            Card imp = PutOnDeck("ImpPilferer");
            Card grim = PutOnDeck("GrimSebastian");

            Card sides = PlayCard("PlayingBothSides");
            AssertAtLocation(new Card[] { fortitude, ammo, blazing, goggles, imp, grim }, sides.UnderLocation);
            MoveCard(bastion, grim, bastion.TurnTaker.Trash);

            GoToStartOfTurn(bastion);
            AssertNumberOfCardsAtLocation(sides.UnderLocation, 4);
            AssertIsInPlay(sides);
        }

        [Test()]
        public void TestPlayingBothSidesStartOneShot()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();

            //When this card enters play, put the top card of each deck under it.
            //At the end of the environment turn, a player may discard 2 cards. If they do not, put the top card of the villain deck under this card.
            //At the start of the environment turn, put a random card from under this card into play. If a one-shot or environment card is played this way, destroy this card.
            Card thokk = PutOnDeck("Thokk");
            Card ext = PutOnDeck("ExternalCombustion");
            Card blast = PutOnDeck("FireBlast");
            Card sucker = PutOnDeck("SuckerPunch");
            Card fallen = PutOnDeck("FallenAngel");
            Card grim = PutOnDeck("GrimSebastian");

            Card sides = PlayCard("PlayingBothSides");
            AssertAtLocation(new Card[] { thokk, ext, blast, sucker, fallen, grim }, sides.UnderLocation);
            MoveCard(bastion, grim, bastion.TurnTaker.Trash);

            GoToStartOfTurn(bastion);
            AssertAtLocation(sides, bastion.TurnTaker.Trash);
        }

        [Test()]
        public void TestPlayingBothSidesStartEnviro()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();

            //When this card enters play, put the top card of each deck under it.
            //At the end of the environment turn, a player may discard 2 cards. If they do not, put the top card of the villain deck under this card.
            //At the start of the environment turn, put a random card from under this card into play. If a one-shot or environment card is played this way, destroy this card.
            Card thokk = PutOnDeck("Thokk");
            Card ext = PutOnDeck("ExternalCombustion");
            Card blast = PutOnDeck("FireBlast");
            Card sucker = PutOnDeck("SuckerPunch");
            Card fallen = PutOnDeck("FallenAngel");
            Card grim = PutOnDeck("GrimSebastian");

            Card sides = PlayCard("PlayingBothSides");
            MoveCards(bastion, new Card[] {thokk, ext, blast, sucker, fallen},(Card c) => FindCardController(c).GetTrashDestination().Location);

            GoToStartOfTurn(bastion);
            AssertInTrash(sides);
            AssertIsInPlay(grim);
        }

        [Test()]
        public void TestPowerPlayNoMachinations()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the start of the environment turn, reveal the top {H} cards of the environment deck. Put any revealed machinations into play. Discard the other revealed cards. If a card enters play this way, destroy this card.
            Card fork = PutOnDeck("AForkInTheRoad");
            Card mathis = PutOnDeck("BrotherMathis");
            Card bystander = PutOnDeck("Bystander");
            Card clean = PutOnDeck("CleanStreetsDirtyHands");
            Card grim = PutOnDeck("GrimSebastian");

            Card powerplay = PlayCard("PowerPlay");

            GoToStartOfTurn(bastion);
            AssertInTrash(new Card[] { mathis, bystander, clean, grim });
            AssertOnTopOfDeck(fork);
        }

        [Test()]
        public void TestPowerPlay()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the start of the environment turn, reveal the top {H} cards of the environment deck. Put any revealed machinations into play. Discard the other revealed cards. If a card enters play this way, destroy this card.
            Card fork = PutOnDeck("AForkInTheRoad");
            Card mathis = PutOnDeck("BrotherMathis");
            Card rites = PutOnDeck("RitesOfTheBlackThorn");
            Card clean = PutOnDeck("CleanStreetsDirtyHands");
            Card business = PutOnDeck("BusinessAsUsual");

            Card powerplay = PlayCard("PowerPlay");

            GoToStartOfTurn(bastion);
            AssertInTrash(new Card[] { mathis, clean });
            AssertIsInPlay(new Card[] { rites, business });
            AssertOnTopOfDeck(fork);
        }

        [Test()]
        public void TestRitesOfTheBlackThornIncreaseAndIrreducible()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //Infernal damage is increased by 1 and irreducible.
            //The first time each turn any target is destroyed by infernal damage, play the top card of the environment deck.
            Card rites = PlayCard("RitesOfTheBlackThorn");
            QuickHPStorage(legacy);
            Card fortitude = PlayCard("Fortitude");
            DealDamage(apostate, legacy, 2, DamageType.Infernal);
            QuickHPCheck(-3);

            QuickHPStorage(legacy);
            DealDamage(apostate, legacy, 2, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestRitesOfTheBlackThornDestroy()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //Infernal damage is increased by 1 and irreducible.
            //The first time each turn any target is destroyed by infernal damage, play the top card of the environment deck.
            Card rites = PlayCard("RitesOfTheBlackThorn");
            Card grim = PutOnDeck("GrimSebastian");
            Card imp = PlayCard("ImpPilferer");

            DealDamage(legacy.CharacterCard, imp, 5, DamageType.Infernal);
            AssertIsInPlay(grim);
        }

        [Test()]
        public void TestRitesOfTheBlackThornDestroyNonInfernal()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //Infernal damage is increased by 1 and irreducible.
            //The first time each turn any target is destroyed by infernal damage, play the top card of the environment deck.
            Card rites = PlayCard("RitesOfTheBlackThorn");
            Card grim = PutOnDeck("GrimSebastian");
            Card imp = PlayCard("ImpPilferer");

            DealDamage(legacy.CharacterCard, imp, 5, DamageType.Melee);
            AssertOnTopOfDeck(grim);
        }

        [Test()]
        public void TestRitesOfTheBlackThornDestroyNonDamage()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //Infernal damage is increased by 1 and irreducible.
            //The first time each turn any target is destroyed by infernal damage, play the top card of the environment deck.
            Card rites = PlayCard("RitesOfTheBlackThorn");
            Card grim = PutOnDeck("GrimSebastian");
            Card imp = PlayCard("ImpPilferer");
            SetHitPoints(imp, 2);

            PlayCard("SuckerPunch");
            AssertOnTopOfDeck(grim);
        }

        [Test()]
        public void TestTheSpidersWebStartDestroyMachination()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the start of the environment turn, destroy a machination other than this card. If you do not, destroy this card.
            //At the end of the environment turn, increase the next damage dealt by the villain target with the highest HP by 2.
            Card spider = PlayCard("TheSpidersWeb");
            Card rites = PlayCard("RitesOfTheBlackThorn");

            GoToEndOfTurn(tachyon);
            AssertNextDecisionChoices(new Card[] { rites }, new Card[] { spider });
            GoToStartOfTurn(bastion);
            AssertInTrash(rites);
            AssertIsInPlay(spider);
        }

        [Test()]
        public void TestTheSpidersWebStartNoMachination()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the start of the environment turn, destroy a machination other than this card. If you do not, destroy this card.
            //At the end of the environment turn, increase the next damage dealt by the villain target with the highest HP by 2.
            Card spider = PlayCard("TheSpidersWeb");

            GoToEndOfTurn(tachyon);
            GoToStartOfTurn(bastion);
            AssertInTrash(spider);
        }

        [Test()]
        public void TestTheSpidersWebEnd()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //At the start of the environment turn, destroy a machination other than this card. If you do not, destroy this card.
            //At the end of the environment turn, increase the next damage dealt by the villain target with the highest HP by 2.
            GoToPlayCardPhase(bastion);
            Card spider = PlayCard("TheSpidersWeb");

            SetHitPoints(apostate, 1);
            Card sword = GetCard("Condemnation");
            GoToEndOfTurn(bastion);

            QuickHPStorage(legacy);
            DealDamage(apostate, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(legacy);
            DealDamage(sword, legacy.CharacterCard, 1, DamageType.Melee);
            QuickHPCheck(-3);

            QuickHPStorage(legacy);
            DealDamage(sword, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestTwoSidesToShadowsReduce()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //Reduce damage dealt to keepers and civilians by 1.
            //At the end of the environment turn, destroy an ongoing card. Then play the top card of that card's deck.
            Card shadows = PlayCard("TwoSidesToShadows");
            Card grim = PlayCard("GrimSebastian");
            Card bystander = PlayCard("Bystander");
            Card goodman = PlayCard("MrGoodman");

            QuickHPStorage(grim, bystander, goodman, legacy.CharacterCard, apostate.CharacterCard);
            DealDamage(apostate, grim, 2, DamageType.Melee);
            DealDamage(apostate, bystander, 2, DamageType.Melee);
            DealDamage(apostate, goodman, 2, DamageType.Melee);
            DealDamage(apostate, legacy, 2, DamageType.Melee);
            DealDamage(apostate, apostate, 2, DamageType.Melee);


            QuickHPCheck(-1, -1,-2, -2, -2);
        }

        [Test()]
        public void TestTwoSidesToShadowsEnd()
        {
            SetupGameController("Apostate", "Ra", "Legacy", "Bunker", "Tachyon", "VainFacadePlaytest.BastionCity");
            StartGame();
            //Reduce damage dealt to keepers and civilians by 1.
            //At the end of the environment turn, destroy an ongoing card. Then play the top card of that card's deck.
            GoToPlayCardPhase(bastion);
            Card shadows = PlayCard("TwoSidesToShadows");

            Card fortitude = PlayCard("Fortitude");
            Card flak = PlayCard("FlakCannon");
            Card apocalypse = PlayCard("Apocalypse");
            Card imp = PutOnDeck("ImpPilferer");
            DecisionSelectCard = apocalypse;

            AssertNextDecisionChoices(new Card[] { fortitude, apocalypse }, new Card[] { flak });
            GoToEndOfTurn(bastion);
            AssertInTrash(apocalypse);
            AssertIsInPlay(new Card[] { imp, fortitude, flak });
        }
    }
}