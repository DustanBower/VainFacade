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
	public class GrimReflectionTests:BaseTest
	{
        //Heroes
        protected HeroTurnTakerController friday { get { return FindHero("Friday"); } }
        protected HeroTurnTakerController ember { get { return FindHero("Ember"); } }
        protected HeroTurnTakerController burgess { get { return FindHero("Burgess"); } }
        protected HeroTurnTakerController node { get { return FindHero("Node"); } }
        protected HeroTurnTakerController sphere { get { return FindHero("Sphere"); } }
        protected HeroTurnTakerController fury { get { return FindHero("TheFury"); } }
        protected HeroTurnTakerController carnaval { get { return FindHero("Carnaval"); } }
        protected HeroTurnTakerController peacekeeper { get { return FindHero("Peacekeeper"); } }
        protected HeroTurnTakerController banshee { get { return FindHero("Banshee"); } }
        protected HeroTurnTakerController arctis { get { return FindHero("Arctis"); } }
        protected HeroTurnTakerController push { get { return FindHero("Push"); } }

        //Villains
        protected TurnTakerController baroness { get { return FindVillain("TheBaroness"); } }
        protected TurnTakerController blitz { get { return FindVillain("Grandfather"); } }

        protected string grimID = "GrimReflection";

        //Decision stuff
        protected Card SelectFirstCard;
        protected Card SelectSecondCard;
        protected Card SelectThirdCard;
        protected int DecisionIndex = 1;
        protected int NumberOfCardsToSelect;

        protected Location SelectFirstLocation;
        protected Location SelectSecondLocation;
        protected Location SelectThirdLocation;

        protected IEnumerator NewDecisions(IDecision decision)
        {
            if (decision is SelectCardDecision && ((SelectCardDecision)decision).CardSource.Card.Identifier == "GrimReflection")
            {
                if (DecisionIndex == 1)
                {
                    Console.WriteLine("Selecting " + SelectFirstCard.Title);
                    decision.SelectedCard = SelectFirstCard;
                    Console.WriteLine("Selected " + SelectFirstCard.Title);
                    DecisionIndex = 2;
                }
                else if (DecisionIndex == 2)
                {
                    if (NumberOfCardsToSelect == 1)
                    {
                        Console.WriteLine("Stopping card selection");
                        ((SelectCardDecision)decision).FinishedSelecting = true;
                        Console.WriteLine("StoppedCardSelection");
                    }
                    else
                    {
                        decision.SelectedCard = SelectSecondCard;
                        DecisionIndex = 3;
                    }
                }
                else if (DecisionIndex == 3)
                {
                    if (NumberOfCardsToSelect == 2)
                    {
                        ((SelectCardDecision)decision).FinishedSelecting = true;
                    }
                    else
                    {
                        decision.SelectedCard = SelectThirdCard;
                    }
                }
            }
            else if (decision is SelectLocationDecision && ((SelectLocationDecision)decision).CardSource.Card.Identifier == "GrimReflection")
            {
                if (DecisionIndex == 1)
                {
                    ((SelectLocationDecision)decision).SelectedLocation = new LocationChoice(SelectFirstLocation);
                    DecisionIndex = 2;
                }
                else if (DecisionIndex == 2)
                {
                    ((SelectLocationDecision)decision).SelectedLocation = new LocationChoice(SelectSecondLocation);
                    DecisionIndex = 3;
                }
                else if (DecisionIndex == 3)
                {
                    ((SelectLocationDecision)decision).SelectedLocation = new LocationChoice(SelectThirdLocation);
                }
            }
            else
            {
                RunCoroutine(base.MakeDecisions(decision));
            }
            Console.WriteLine("Exiting NewDecisions");
            yield return null;
        }

        [Test()]
        public void GrimReflectionPowerDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            //this.GameController.OnMakeDecisions -= MakeDecisions;
            //this.GameController.OnMakeDecisions += NewDecisions;
            StartGame();
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash);
            Card allies = PlayCard("AlliesOfTheEarth");
            Card grim = PlayCard(grimID);
            Card damsel = PutInHand("DamselInDistress");
            Card protean = PutOnDeck("ProteanDoom");
            DecisionSelectFunction = 0;

            //NumberOfCardsToSelect = 1;
            //SelectFirstCard = PutInHand("ProteanDoom");
            //SelectFirstLocation = allies.UnderLocation;

            //Check that Grim Reflection's power lets Friday draw a card
            QuickHandStorage(friday);
            UsePower(grim);
            QuickHandCheck(0);
            AssertAtLocation(damsel, allies.UnderLocation);
            AssertInHand(protean);
        }

        [Test()]
        public void GrimReflectionPowerUsePower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += NewDecisions;

            Card allies = PlayCard("AlliesOfTheEarth");
            Card grim = PlayCard(grimID);
            DecisionSelectFunction = 1;

            NumberOfCardsToSelect = 1;
            SelectFirstCard = PutInHand("ProteanDoom");
            SelectFirstLocation = allies.UnderLocation;

            //Check that Grim Reflection's power lets Friday use a power
            QuickHandStorage(friday);
            QuickHPStorage(akash);
            UsePower(grim);
            QuickHandCheck(-1);
            QuickHPCheck(-2);
            AssertAtLocation(SelectFirstCard, allies.UnderLocation);
        }

        [Test()]
        public void TestInspiringPresenceTrigger()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            //this.GameController.OnMakeDecisions -= MakeDecisions;
            //this.GameController.OnMakeDecisions += NewDecisions;

            Card inspiring = PlayCard("InspiringPresence");
            Card grim = PlayCard(grimID);
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash);
            Card protean = PutInHand("ProteanDoom");
            DecisionSelectFunction = 0;

            UsePower(grim);

            DecisionSelectTarget = akash.CharacterCard;

            //Check that damage from any hero is increased by both Inspiring Presence and Grim Reflection
            QuickHPStorage(akash);
            DealDamage(friday, akash, 1, DamageType.Melee);
            QuickHPCheck(-3);

            QuickHPStorage(akash);
            DealDamage(bunker, akash, 1, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestInspiringPresencePlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            //this.GameController.OnMakeDecisions -= MakeDecisions;
            //this.GameController.OnMakeDecisions += NewDecisions;

            Card inspiring = PlayCard("InspiringPresence");
            Card grim = PlayCard(grimID);
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash);
            Card protean = PutInHand("ProteanDoom");
            DecisionSelectFunction = 0;

            UsePower(grim);

            DestroyCard(grim);
            SetHitPoints(friday, 10);
            SetHitPoints(legacy, 10);
            SetHitPoints(bunker, 10);
            SetHitPoints(scholar, 10);


            //Check that each hero regains 1 HP when Grim Reflection enters play
            QuickHPStorage(friday, legacy, bunker, scholar);
            PlayCard(grim);
            QuickHPCheck(1, 1, 1, 1);
        }

        [Test()]
        public void TestNextEvolutionPower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            //this.GameController.OnMakeDecisions -= MakeDecisions;
            //this.GameController.OnMakeDecisions += NewDecisions;
            StartGame();
            GoToEndOfTurn(friday);
            Card evolution = PlayCard("NextEvolution");
            Card grim = PlayCard(grimID);
            DecisionSelectFunction = 0;

            //NumberOfCardsToSelect = 1;
            //SelectFirstCard = PutInHand("ProteanDoom");
            //SelectFirstLocation = evolution.UnderLocation;

            UsePower(grim);

            Card vessel = PlayCard("VesselOfDestruction");
            DecisionDestroyCard = vessel;

            //Check that Next Evolution's power applies correctly to Friday
            DecisionSelectDamageType = DamageType.Fire;
            
            UsePower(grim, 1);
            QuickHPStorage(friday);
            DealDamage(akash, friday, 2, DamageType.Fire);
            QuickHPCheck(0);

            QuickHPStorage(friday);
            DealDamage(akash, friday, 2, DamageType.Melee);
            QuickHPCheck(-2);

            GoToEndOfTurn(legacy);
            QuickHPStorage(friday);
            DealDamage(akash, friday, 2, DamageType.Fire);
            QuickHPCheck(0);

            GoToEndOfTurn(friday);
            QuickHPStorage(friday);
            DealDamage(akash, friday, 2, DamageType.Fire);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestImpendingDoom()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "EmpyreonCharacter" });

            StartGame();
            MoveCards(oblivaeon, FindCardsWhere((Card c) => c.Identifier == "AeonThrall"), aeonDeck);
            GoToEndOfTurn(bunker);

            //Play this card in the other battle zone's villain play area
            //At the start of this villain turn, if OblivAeon is here, OblivAeon deals each non-scion target 9999 infernal damage.
            //Then, move the countdown token 1 space towards 0 and destroy this card.
            DecisionSelectFunction = 0;
            SwitchBattleZone(friday);
            SwitchBattleZone(legacy);
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash, leaveSomeCards: 1);
            PutInHand("DamselInDistress");
            PutOnDeck("BuiltForWar");
            Card doom = PlayCard("ImpendingDoom");

            Card grim = PlayCard("GrimReflection");
            //Console.WriteLine("Triggers from Grim Reflection: " + base.GameController.FindTriggersWhere((ITrigger t) => t.CardSource != null && t.CardSource.Card == grim).ToCommaList());
            //Console.WriteLine("Location ownerTurnTaker: " + doom.UnderLocation.OwnerTurnTaker.Name);
            //Console.WriteLine("IsLocationVisible: " + oblivaeon.IsLocationVisible(doom.UnderLocation, new CardSource(FindCardController(grim))));

            //Console.WriteLine("The hero is in " + friday.BattleZone.Identifier);
            //Console.WriteLine("Impending Doom is in " + doom.UnderLocation.BattleZone.Identifier);
            //Console.WriteLine("Impending Doom's OwnerTurnTaker is in " + doom.UnderLocation.OwnerTurnTaker.BattleZone.Identifier);
            UsePower(grim);

            Card damsel = PlayCard("DamselInDistress");
            Card heroic = PlayCard("HeroicInterception");

            //At this point, Impending Doom and Friday are in the second battle zone and Grim Reflection is copying Impending Doom
            //OblivAeon is in BZ1

            //Go to the start of the second scion turn
            QuickTokenPoolStorage(GetCard("InevitableDestruction").FindTokenPool(TokenPool.CountdownTokenPool));
            GoToStartOfTurn(scionTwo);
            QuickTokenPoolCheck(-1);
            AssertNumberOfCardsInPlay((Card c) => c.IsAeonMan, 0);
            AssertInTrash(grim);
        }

        [Test()]
        public void TestBattleZonesFridayMoves()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });

            StartGame();
            MoveCards(oblivaeon, FindCardsWhere((Card c) => c.Identifier == "AeonThrall"), aeonDeck);
            GoToEndOfTurn(bunker);
            DecisionSelectFunction = 0;
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash, leaveSomeCards: 1);
            PutInHand("DamselInDistress");
            PutOnDeck("BuiltForWar");
            Card presence = PlayCard("InspiringPresence");

            Card grim = PlayCard("GrimReflection");

            UsePower(grim);

            //Friday and Legacy both in BZ1
            QuickHPStorage(progScion);
            DealDamage(friday, progScion, 1, DamageType.Melee);
            QuickHPCheck(-3);

            //Friday in BZ2, Legacy in BZ1
            SwitchBattleZone(friday);
            QuickHPStorage(mindScion);
            DealDamage(friday, mindScion, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Friday back in BZ1, Legacy in BZ1
            SwitchBattleZone(friday);
            QuickHPStorage(progScion);
            DealDamage(friday, progScion, 1, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestBattleZonesLegacyMoves()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });

            StartGame();
            MoveCards(oblivaeon, FindCardsWhere((Card c) => c.Identifier == "AeonThrall"), aeonDeck);
            GoToEndOfTurn(bunker);
            DecisionSelectFunction = 0;
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash, leaveSomeCards: 1);
            PutInHand("DamselInDistress");
            PutOnDeck("BuiltForWar");
            Card presence = PlayCard("InspiringPresence");

            Card grim = PlayCard("GrimReflection");

            UsePower(grim);

            //Friday and Legacy both in BZ1
            QuickHPStorage(progScion);
            DealDamage(friday, progScion, 1, DamageType.Melee);
            QuickHPCheck(-3);

            //Friday in BZ1, Legacy in BZ2
            SwitchBattleZone(legacy);
            QuickHPStorage(progScion);
            DealDamage(friday, progScion, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Friday in BZ1, Legacy back in BZ1
            SwitchBattleZone(legacy);
            QuickHPStorage(progScion);
            DealDamage(friday, progScion, 1, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestBattleZonesPlayGrim()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });

            StartGame();
            MoveCards(oblivaeon, FindCardsWhere((Card c) => c.Identifier == "AeonThrall"), aeonDeck);
            GoToEndOfTurn(bunker);
            DecisionSelectFunction = 0;
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash, leaveSomeCards: 1);
            PutInHand("DamselInDistress");
            PutOnDeck("BuiltForWar");
            Card presence = PlayCard("InspiringPresence");

            Card grim = PlayCard("GrimReflection");

            UsePower(grim);

            DestroyCard(grim);
            SwitchBattleZone(friday);

            //When Grim Reflection enters play in BZ2, it should not run the Play method of Inspiring Presence or copy it
            SetHitPoints(friday, 10);
            QuickHPStorage(friday.CharacterCard, mindScion);
            PlayCard(grim);
            DealDamage(friday, mindScion, 1, DamageType.Melee);
            QuickHPCheck(0,-1);

            //When Friday moves back to BZ1, it should copy Inspiring Presence
            SwitchBattleZone(friday);
            QuickHPStorage(progScion);
            DealDamage(friday, progScion, 1, DamageType.Melee);
            QuickHPCheck(-3);

            //When played in BZ1, it should run the Play method of Inspiring Presence and copy it
            DestroyCard(grim);
            QuickHPStorage(friday.CharacterCard, progScion);
            PlayCard(grim);
            DealDamage(friday, progScion, 1, DamageType.Melee);
            QuickHPCheck(1, -3);
        }

        [Test()]
        public void TestGalvanized()
        {
            SetupGameController("IronLegacy", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            Card galvanized = PlayCard("Galvanized");
            StartGame();
            DestroyCards(FindCardsWhere((Card c) => c.IsVillain && c.IsOngoing));

            Card grim = PlayCard("GrimReflection");
            DecisionSelectFunction = 0;
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash);
            PutInHand("Doppelganger");
            PutOnDeck("BuiltForWar");
            UsePower(grim);

            //Check that Friday's damage is increased
            QuickHPStorage(iron);
            DealDamage(friday, iron, 1, DamageType.Melee);
            QuickHPCheck(-2);

            //Check that Grim Destruction is indestructible
            GoToStartOfTurn(friday);
            AssertIsInPlay(grim);

        }

        //[Test()]
        //public void TestBattleZonesUhYeah()
        //{
        //    SetupGameController(new string[] { "OblivAeon", "Guise", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
        //    StartGame();
        //    DecisionSelectTurnTaker = legacy.TurnTaker;
        //    Card UhYeah = PlayCard("UhYeahImThatGuy");
        //    PlayCard("MotivationalCharge");
        //    SwitchBattleZone(guise);
        //    SetHitPoints(guise, 10);
        //    QuickHPStorage(mindScion, guise.CharacterCard);

        //    //Legacy is in BZ1, Guise is in BZ2, but he can still use the power from Motivational Charge
        //    UsePower(UhYeah);
        //    QuickHPCheck(-2, 1);
        //}

        [Test()]
        public void TestBattleZonesPower()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });

            StartGame();
            
            DecisionSelectFunction = 0;
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash);
            PutInHand("DamselInDistress");
            PutOnDeck("BuiltForWar");
            Card charge = PlayCard("MotivationalCharge");

            Card grim = PlayCard("GrimReflection");

            UsePower(grim);
            DecisionSelectTarget = progScion;
            SetHitPoints(friday, 10);
            QuickHPStorage(progScion, friday.CharacterCard);
            UsePower(grim, 1);
            QuickHPCheck(-2, 1);

            SwitchBattleZone(friday);

            //Console.WriteLine($"Friday is in {friday.BattleZone.Identifier}");
            //Console.WriteLine($"Legacy is in {legacy.BattleZone.Identifier}");
            //Console.WriteLine($"Is charge visible to grim: {GameController.IsCardVisibleToCardSource(charge,FindCardController(grim).GetCardSource())}");

            DecisionSelectTarget = mindScion;
            QuickHPStorage(mindScion, friday.CharacterCard);

            try
            {
                UsePower(grim, 1);
            }
            catch
            {
                Console.WriteLine("Grim Reflection is no longer copying the power from Motivational Charge");
            }

            QuickHPCheck(0, 0);
        }

        [Test()]
        public void TestRemoveFromOngoings()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            Card inspiring = PlayCard("InspiringPresence");
            Card grim = PlayCard("GrimReflection");
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Deck);
            Card damsel = PutInHand("DamselInDistress");
            Card protean = PutOnDeck("ProteanDoom");
            DecisionSelectFunction = 0;

            UsePower(grim);

            QuickHPStorage(akash);
            DealDamage(friday,akash,1,DamageType.Melee);
            QuickHPCheck(-3);

            DestroyCard(damsel);

            QuickHPStorage(akash);
            DealDamage(friday, akash, 1, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestPostDestroyAction()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "VoidGuardMainstay", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            Card headlock = PlayCard("Headlock");
            Card grim = PlayCard("GrimReflection");
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Deck);
            Card damsel = PutInHand("DamselInDistress");
            Card protean = PutOnDeck("ProteanDoom");
            DecisionSelectFunction = 0;

            UsePower(grim);

            //Check that when Grim Reflection is destroyed, friday deals 3 damage to Akash'Bhuta
            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(akash);
            DestroyCard(grim);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestBufferOverflowPostDestroy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Parse", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            Card buffer = PlayCard("BufferOverflow");
            Card grim = PlayCard("GrimReflection");
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Deck);
            Card damsel = PutInHand("DamselInDistress");
            Card protean = PutOnDeck("ProteanDoom");
            DecisionSelectFunction = 0;

            UsePower(grim);

            Card war = PutOnDeck("BuiltForWar");
            Card allies = PutOnDeck("AlliesOfTheEarth");

            //When this card is destroyed, draw a card and play the top card of the villain deck
            QuickHandStorage(friday);
            DestroyCard(grim);

            AssertIsInPlay(allies);
            QuickHandCheck(1);
            AssertInHand(war);
        }

        [Test()]
        public void TestBulletHellPower1Grim()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "VainFacadePlaytest.Friday", "TheBlock");
            StartGame();

            //Power: Until the start of your next turn, the first time each turn any target deals damage, {Peacekeeper} may discard a card to deal 1 target 1 projectile damage.
            Card bullet = PlayCard("BulletHell");
            GoToUsePowerPhase(peacekeeper);
            UsePower(bullet);

            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;

            Card grim = PlayCard("GrimReflection");
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash, false, 1);
            DecisionSelectCard = bullet;
            PutOnDeck("ProteanDoom");
            DecisionSelectFunction = 0;
            UsePower(grim);
            UsePower(grim, 1);

            //Check that it works if another hero uses the power on Bullet Hell.
            DecisionSelectTarget = akash.CharacterCard;
            Card war = PutInHand("BuiltForWar");
            QuickHPStorage(akash);
            QuickHandStorage(peacekeeper, friday);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(-2);
            QuickHandCheck(-1, -1);
            AssertInTrash(war);
        }

        [Test()]
        public void TestCoverFirePowerGrim()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "VainFacadePlaytest.Friday", "TheBlock");
            StartGame();

            //Power: Until the start of your next turn, the first time each turn any hero target would be dealt damage by another target, reduce that damage by 1, then {Peacekeeper} deals the source of that damage 2 projectile damage.
            GoToUsePowerPhase(peacekeeper);
            Card cover = PlayCard("CoverFire");
            UsePower(cover);

            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;

            Card grim = PlayCard("GrimReflection");
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash, false, 1);
            DecisionSelectCard = cover;
            PutOnDeck("ProteanDoom");
            DecisionSelectFunction = 0;
            UsePower(grim);
            UsePower(grim, 1);

            //Check that it works if another hero uses the power on cover fire.
            QuickHPStorage(legacy, akash);
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-1,-4);
        }

        [Test()]
        public void TestToxicBloodGrim()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Peacekeeper", "Legacy", "Bunker", "VainFacadePlaytest.Friday", "TheBlock");
            StartGame();

            //Increase damage dealt by {Peacekeeper} to other targets by 1.
            //At the end of your turn, {Peacekeeper} deals himself 1 irreducible toxic damage.
            GoToPlayCardPhase(friday);
            Card toxic = PlayCard("ToxicBlood");

            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;

            Card grim = PlayCard("GrimReflection");
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash, false, 1);
            DecisionSelectCard = toxic;
            PutOnDeck("ProteanDoom");
            DecisionSelectFunction = 0;
            UsePower(grim);

            //Check that Friday gets the damage increase
            QuickHPStorage(akash);
            DealDamage(friday, akash, 1, DamageType.Melee);
            QuickHPCheck(-2);

            //Check that Friday copies the end of turn damage
            QuickHPStorage(friday);
            GoToEndOfTurn(friday);
            QuickHPCheck(-1);

            QuickHPStorage(peacekeeper);
            GoToEndOfTurn(peacekeeper);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestMalleableBattleForm()
        {
            SetupGameController("Argo", "Ra", "Legacy", "Bunker", "VainFacadePlaytest.Friday", "TheBlock");
            StartGame();

            //At the end of the villain turn, Argo deals each target 1 projectile damage.
            DestroyCards(FindCardsWhere((Card c) => c.IsImprint && c.IsInPlayAndHasGameText));
            Card malleable = PlayCard("MalleableBattleForm");
            TurnTakerController argo = FindVillain("Argo");

            base.GameController.OnMakeDecisions -= MakeDecisions;
            base.GameController.OnMakeDecisions += MakeDecisions2;

            Card grim = PlayCard("GrimReflection");
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash, false, 1);
            DecisionSelectCard = malleable;
            PutOnDeck("ProteanDoom");
            DecisionSelectFunction = 0;
            UsePower(grim);

            GoToEndOfTurn(argo);
        }
    }
}

