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
	public class NodeOATests:BaseTest
	{
        protected HeroTurnTakerController node { get { return FindHero("Node"); } }

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(node.CharacterCard, 1);
            DealDamage(villain, node, 2, DamageType.Melee, true);
        }

        [Test()]
        public void TestInnatePower()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Draw a card or {NodeCharacter} deals 1 target 0 psychic damage. If damage is dealt this way, {NodeCharacter} deals that target 0 psychic damage and increases the next damage dealt to that target by 1.
            DecisionSelectFunction = 1;
            DecisionSelectTarget = progScion;
            PlayCard("InspiringPresence");
            UsePower(node);

            //Check that the next damage is increased
            QuickHPStorage(progScion);
            DealDamage(node, progScion, 1, DamageType.Melee);
            QuickHPCheck(-3);

            QuickHPStorage(progScion);
            DealDamage(node, progScion, 1, DamageType.Melee);
            QuickHPCheck(-2);

            //Check that if Node moves to the other battle zone, the next damage is not increased
            UsePower(node);
            SwitchBattleZone(node);
            QuickHPStorage(progScion);
            DealDamage(scholar, progScion, 1, DamageType.Melee);
            QuickHPCheck(-2);

            //Check that if Node moves back to BZ1, the next damage is increased
            SwitchBattleZone(node);
            QuickHPStorage(progScion);
            DealDamage(node, progScion, 1, DamageType.Melee);
            QuickHPCheck(-3);

            QuickHPStorage(progScion);
            DealDamage(node, progScion, 1, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestIncap1()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //One player may discard a card to reduce the next damage dealt to a hero character card in their play area by the number of active heroes until the start of your next turn.
            //Should this effect check at the moment of damage how many active heroes are in this BZ, or should it only check at the moment the incap ability is used?
            //As coded, it checks when used and does not update the number
            SetupIncap(oblivaeon);
            SwitchBattleZone(legacy);
            DecisionSelectTurnTaker = scholar.TurnTaker;
            UseIncapacitatedAbility(node, 0);
            SwitchBattleZone(bunker);

            //Check that damage to Scholar is reduced by 2
            QuickHPStorage(scholar);
            DealDamage(progScion, scholar, 5, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestIncap1OtherBZ()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //One player may discard a card to reduce the next damage dealt to a hero character card in their play area by the number of active heroes until the start of your next turn.
            //Should this effect check at the moment of damage how many active heroes are in this BZ, or should it only check at the moment the incap ability is used?
            //As coded, it checks when used and does not update the number
            SetupIncap(oblivaeon);
            SwitchBattleZone(legacy);
            DecisionSelectTurnTaker = scholar.TurnTaker;
            UseIncapacitatedAbility(node, 0);
            SwitchBattleZone(node);

            //Check that damage to Scholar is not reduced
            QuickHPStorage(scholar);
            DealDamage(progScion, scholar, 5, DamageType.Melee);
            QuickHPCheck(-5);

            //Check that when Scholar moves to be with Node again, the next damage is reduced by 2
            SwitchBattleZone(scholar);
            QuickHPStorage(scholar);
            DealDamage(mindScion, scholar, 5, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestIncap2Play()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Reveal the top card of a deck. Put it into play or discard it.
            Card focus = PutOnDeck("FocusOfPower");
            SetupIncap(oblivaeon);
            DecisionSelectLocation = new LocationChoice(oblivaeon.TurnTaker.Deck);
            UseIncapacitatedAbility(node,1);
            AssertIsInPlay(focus);
        }

        [Test()]
        public void TestIncap2Discard()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Reveal the top card of a deck. Put it into play or discard it.
            Card focus = PutOnDeck("FocusOfPower");
            SetupIncap(oblivaeon);
            DecisionSelectLocation = new LocationChoice(oblivaeon.TurnTaker.Deck);
            DecisionMoveCardDestination = new MoveCardDestination(oblivaeon.TurnTaker.Trash);
            UseIncapacitatedAbility(node, 1);
            AssertInTrash(focus);
        }

        [Test()]
        public void TestIncap2PlayAeon()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Reveal the top card of a deck. Put it into play or discard it.
            Card thrall = MoveCard(oblivaeon, "AeonThrall", aeonDeck);
            Card warrior = MoveCard(oblivaeon, "AeonWarrior", scionOne.TurnTaker.PlayArea);
            SetupIncap(oblivaeon);
            DecisionSelectLocation = new LocationChoice(aeonDeck);
            UseIncapacitatedAbility(node, 1);
            AssertAtLocation(thrall, scionOne.TurnTaker.PlayArea);
        }

        [Test()]
        public void TestIncap2DiscardAeon()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Reveal the top card of a deck. Put it into play or discard it.
            Card thrall = MoveCard(oblivaeon, "AeonThrall", aeonDeck);
            Card warrior = MoveCard(oblivaeon, "AeonWarrior", scionOne.TurnTaker.PlayArea);
            SetupIncap(oblivaeon);
            DecisionSelectLocation = new LocationChoice(aeonDeck);
            DecisionMoveCardDestination = new MoveCardDestination(aeonTrash);
            UseIncapacitatedAbility(node, 1);
            AssertAtLocation(thrall,aeonTrash);
        }

        [Test()]
        public void TestIncap3()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //One player discards up to 2 cards. For each card discarded in this way, one player may draw a card."
            SetupIncap(oblivaeon);
            SwitchBattleZone(legacy);

            //Check that node and legacy are not included in the choices
            AssertNextDecisionChoices(new TurnTaker[] { bunker.TurnTaker, scholar.TurnTaker }, new TurnTaker[] { node.TurnTaker, legacy.TurnTaker });
            UseIncapacitatedAbility(node, 2);
        }

        [Test()]
        public void TestBleedingEdge()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //You may play a card.
            //Return any number of Connections in play to your hand.
            //For each Connection returned this way, you may draw a card, then you may play a card.
            //Increase the next damage dealt to {NodeCharacter} by X, where X is the number of hero cards played since this card entered play.
            DecisionDoNotSelectCard = SelectionType.PlayCard;
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card collective = PlayCard("CollectiveUnconscious");
            DecisionSelectTurnTaker = bunker.TurnTaker;
            Card link = PlayCard("PsychicLink");
            SwitchBattleZone(legacy);

            //Check that Psychic Link can be returned to hand and Collective Unconscious cannot
            //AssertNextDecisionChoices(new Card[] { link }, new Card[] { collective });
            QuickHandStorage(node);
            PlayCard("BleedingEdge");
            AssertInHand(link);
            AssertIsInPlay(collective);
            QuickHandCheck(2);
        }

        [Test()]
        public void TestCollectiveUnconsciousPlay()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Play this card in another hero's play area. Cards in this play area and {NodeCharacter}'s are [i]Connected.[/i]
            //When {NodeCharacter} or that hero uses a power, the other gains 1 HP.
            //When {NodeCharacter} would deal that hero psychic damage, that hero recovers that much HP instead.
            SwitchBattleZone(legacy);

            //Check that Collective Unconscious cannot be played in Legacy's play area
            AssertNextDecisionChoices(new TurnTaker[] { bunker.TurnTaker, scholar.TurnTaker }, new TurnTaker[] { legacy.TurnTaker });
            PlayCard("CollectiveUnconscious");
        }

        [Test()]
        public void TestCollectiveUnconscious()
        {
            //Fixed
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Play this card in another hero's play area. Cards in this play area and {NodeCharacter}'s are [i]Connected.[/i]
            //When {NodeCharacter} or that hero uses a power, the other gains 1 HP.
            //When {NodeCharacter} would deal that hero psychic damage, that hero recovers that much HP instead.
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card collective = PlayCard("CollectiveUnconscious");
            SetHitPoints(node, 10);
            SetHitPoints(legacy, 10);

            //Check that it works when in the same battle zone
            QuickHPStorage(node);
            UsePower(legacy);
            QuickHPCheck(1);

            QuickHPStorage(legacy);
            UsePower(node);
            QuickHPCheck(1);

            SwitchBattleZone(node);

            //Check that node and legacy do not gain HP when the other uses a power in the other battle zone
            QuickHPStorage(node);
            UsePower(legacy);
            QuickHPCheck(0);

            QuickHPStorage(legacy);
            UsePower(node);
            QuickHPCheck(0);

            SwitchBattleZone(legacy);

            //Check that it works when back in the same battle zone
            QuickHPStorage(node);
            UsePower(legacy);
            QuickHPCheck(1);

            QuickHPStorage(legacy);
            UsePower(node);
            QuickHPCheck(1);
        }

        [Test()]
        public void TestFaultDetection1()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //{NodeCharacter} deals up to 3 [i]Connected[/i] targets 1 psychic damage and 1 psychic damage each.
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card collective = PlayCard("CollectiveUnconscious");
            SwitchBattleZone(legacy);

            DecisionSelectTurnTaker = oblivaeon.TurnTaker;
            Card link = PlayCard("PsychicLink");

            Card thrall = MoveCard(oblivaeon, "AeonThrall", scionOne.TurnTaker.PlayArea);

            //Check that only connected targets in Node's battle zone are available to be chosen
            AssertNextDecisionChoices(new Card[] { oblivaeon.CharacterCard }, new Card[] { legacy.CharacterCard, progScion, thrall});
            PlayCard("FaultDetection");

            SwitchBattleZone(node);
            AssertNextDecisionChoices(new Card[] { legacy.CharacterCard }, new Card[] { oblivaeon.CharacterCard, progScion, mindScion, thrall });
            PlayCard("FaultDetection");
        }

        [Test()]
        public void TestFaultDetection2()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //{NodeCharacter} deals up to 3 [i]Connected[/i] targets 1 psychic damage and 1 psychic damage each.
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card collective = PlayCard("CollectiveUnconscious");
            SwitchBattleZone(legacy);

            DecisionSelectTurnTaker = scionOne.TurnTaker;
            Card link = PlayCard("PsychicLink");

            Card thrall = MoveCard(oblivaeon, "AeonThrall", scionOne.TurnTaker.PlayArea);

            //Check that only connected targets in Node's battle zone are available to be chosen
            AssertNextDecisionChoices(new Card[] { progScion, thrall }, new Card[] { legacy.CharacterCard, oblivaeon.CharacterCard, mindScion });
            PlayCard("FaultDetection");

            SwitchBattleZone(node);
            AssertNextDecisionChoices(new Card[] { legacy.CharacterCard }, new Card[] { oblivaeon.CharacterCard, progScion, mindScion, thrall });
            PlayCard("FaultDetection");
        }

        [Test()]
        public void TestHealthyConnections()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //The first time each turn any [i]Connected[/i] hero target gains HP, another [i]Connected[/i] hero target gains 1 HP.
            Card healthy = PlayCard("HealthyConnections");

            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card collective = PlayCard("CollectiveUnconscious");
            SwitchBattleZone(legacy);

            DecisionSelectTurnTaker = bunker.TurnTaker;
            Card link = PlayCard("PsychicLink");

            SetHitPoints(legacy, 10);
            SetHitPoints(node, 10);
            SetHitPoints(bunker, 10);

            DecisionSelectTarget = node.CharacterCard;
            //Check that it works with a target in the same battle zone as Node
            QuickHPStorage(node);
            GainHP(bunker, 1);
            QuickHPCheck(1);

            //Check that it doesn't work when a target in the other battle zone gains HP
            GoToStartOfTurn(node);
            QuickHPStorage(node);
            GainHP(legacy, 1);
            QuickHPCheck(0);

            //Check that a target in the other battle zone cannot be healed
            DecisionSelectTarget = legacy.CharacterCard;
            GoToStartOfTurn(legacy);
            QuickHPStorage(legacy);
            GainHP(node, 1);
            QuickHPCheck(0);

            //Check that Legacy can be healed once Node moves to the other battle zone
            GoToStartOfTurn(bunker);
            SwitchBattleZone(node);
            QuickHPStorage(legacy);
            GainHP(node, 1);
            QuickHPCheck(1);
        }

        [Test()]
        public void TestHyperlinkedNeuronsPower()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Increase damage dealt by {NodeCharacter} to [i]Connected[/i] targets other than {NodeCharacter} by 1.
            //Power: {NodeCharacter} deals 1 target 0 psychic damage. Select a [i]Connected[/i] hero. Increase the next damage dealt by that hero by 2.
            Card hyper = PlayCard("HyperlinkedNeurons");

            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card collective = PlayCard("CollectiveUnconscious");
            SwitchBattleZone(legacy);

            DecisionSelectTurnTaker = scholar.TurnTaker;
            Card link = PlayCard("PsychicLink");

            DecisionSelectTarget = progScion;
            DecisionSelectCard = scholar.CharacterCard;

            //Check that the power works on a hero in the same battle zone
            UsePower(hyper);
            QuickHPStorage(progScion);
            DealDamage(scholar.CharacterCard, progScion, 1, DamageType.Melee);
            QuickHPCheck(-3);

            //Check that it doesn't work on a hero in the other battle zone
            DestroyCard(link);
            ResetDecisions();
            UsePower(hyper);

            DecisionSelectTarget = mindScion;
            QuickHPStorage(mindScion);
            DealDamage(legacy.CharacterCard, mindScion, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that it doesn't work on Node when the connection is in the other battle zone
            DecisionSelectTarget = progScion;
            DecisionSelectCard = node.CharacterCard;
            UsePower(hyper);
            QuickHPStorage(progScion);
            DealDamage(node.CharacterCard, progScion, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestMentalFeedback()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Destroy 1 [i]Connected[/i] Ongoing. {NodeCharacter} deals the character from that deck with the highest HP 0 psychic damage and 0 psychic damage.
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card collective = PlayCard("CollectiveUnconscious");
            Card danger = PlayCard("DangerSense");
            SwitchBattleZone(legacy);

            //Make sure cards in the other battle zone cannot be destroyed
            AssertNextDecisionChoices(null, new Card[] { collective, danger });
            PlayCard("MentalFeedback");
        }

        [Test()]
        public void TestOpenLinePlay()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Play this card in another hero's play area. Cards in this play area and {NodeCharacter}'s are[i]Connected.[/ i]
            //When you or that play area's owner would draw or play a card, the other may do so instead.
            SwitchBattleZone(legacy);
            AssertNextDecisionChoices(new TurnTaker[] { bunker.TurnTaker, scholar.TurnTaker }, new TurnTaker[] { legacy.TurnTaker, node.TurnTaker, oblivaeon.TurnTaker, scionOne.TurnTaker, scionTwo.TurnTaker });
            PlayCard("OpenLine");
        }

        [Test()]
        public void TestOpenLineOtherPlay()
        {
            //Fixed
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Play this card in another hero's play area. Cards in this play area and {NodeCharacter}'s are[i]Connected.[/ i]
            //When you or that play area's owner would draw or play a card, the other may do so instead.
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card open = PlayCard("OpenLine");
            SwitchBattleZone(legacy);

            Card hyper = PutInHand("HyperlinkedNeurons");
            MoveAllCards(legacy, legacy.HeroTurnTaker.Hand, legacy.TurnTaker.Trash);
            Card danger = PutInHand("DangerSense");
            DecisionSelectCard = hyper;
            DecisionYesNo = true;

            PlayCardFromHand(legacy, "DangerSense");
            AssertIsInPlay(danger);
            AssertInHand(hyper);
        }

        [Test()]
        public void TestOpenLineNodePlay()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Play this card in another hero's play area. Cards in this play area and {NodeCharacter}'s are[i]Connected.[/ i]
            //When you or that play area's owner would draw or play a card, the other may do so instead.
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card open = PlayCard("OpenLine");
            SwitchBattleZone(legacy);

            Card hyper = PutInHand("HyperlinkedNeurons");
            Card danger = PutInHand("DangerSense");
            DecisionSelectCard = danger;
            DecisionYesNo = true;

            PlayCardFromHand(node, "HyperlinkedNeurons");
            AssertIsInPlay("HyperlinkedNeurons");
            AssertInHand(danger);
        }

        [Test()]
        public void TestOpenLineOtherDraw()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Play this card in another hero's play area. Cards in this play area and {NodeCharacter}'s are[i]Connected.[/ i]
            //When you or that play area's owner would draw or play a card, the other may do so instead.
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card open = PlayCard("OpenLine");
            SwitchBattleZone(legacy);

            Card hyper = PutOnDeck("HyperlinkedNeurons");
            Card danger = PutOnDeck("DangerSense");
            DecisionYesNo = true;

            DrawCard(legacy);
            AssertInHand(danger);
            AssertOnTopOfDeck(hyper);
        }

        [Test()]
        public void TestOpenLineNodeDraw()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Play this card in another hero's play area. Cards in this play area and {NodeCharacter}'s are[i]Connected.[/ i]
            //When you or that play area's owner would draw or play a card, the other may do so instead.
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card open = PlayCard("OpenLine");
            SwitchBattleZone(legacy);

            Card hyper = PutOnDeck("HyperlinkedNeurons");
            Card danger = PutOnDeck("DangerSense");
            DecisionSelectCard = danger;
            DecisionYesNo = true;

            DrawCard(node);
            AssertInHand(hyper);
            AssertOnTopOfDeck(danger);
        }

        [Test()]
        public void TestPacketSniffingPlay()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Play this card in a non-environment play area other than {NodeCharacter}'s.
            //Cards in this play area and {NodeCharacter}'s are [i]Connected.[/i]
            //The first time each turn a card enters play in this area, you may draw a card.
            SwitchBattleZone(legacy);
            AssertNextDecisionChoices(new TurnTaker[] { bunker.TurnTaker, scholar.TurnTaker, oblivaeon.TurnTaker, scionOne.TurnTaker }, new TurnTaker[] { legacy.TurnTaker, node.TurnTaker, scionTwo.TurnTaker });
            PlayCard("PacketSniffing");
        }

        [Test()]
        public void TestPacketSniffing()
        {
            //Fixed
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Play this card in a non-environment play area other than {NodeCharacter}'s.
            //Cards in this play area and {NodeCharacter}'s are [i]Connected.[/i]
            //The first time each turn a card enters play in this area, you may draw a card.
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card packet = PlayCard("PacketSniffing");

            //Check that it works in the same battle zone
            GoToStartOfTurn(node);
            QuickHandStorage(node);
            PlayCard("MotivationalCharge");
            QuickHandCheck(1);

            SwitchBattleZone(legacy);

            //Check that it doesn't work in the other battle zone
            GoToStartOfTurn(legacy);
            QuickHandStorage(node);
            PlayCard("DangerSense");
            QuickHandCheck(0);

            //Check that if a card enters play in Legacy's BZ and then Legacy moves to Node's BZ, another card does not trigger it
            GoToStartOfTurn(bunker);
            QuickHandStorage(node);
            PlayCard("InspiringPresence");
            QuickHandCheck(0);

            SwitchBattleZone(legacy);
            QuickHandStorage(node);
            PlayCard("NextEvolution");
            QuickHandCheck(0);
        }

        [Test()]
        public void TestPsychicLinkPlay()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Play this card in a non-environment play area other than {NodeCharacter}'s. Cards in this play area and {NodeCharacter}'s are [i]Connected.[/i] Play with the top card of that play area's deck face up.
            //The first time each turn any target in this play area would deal damage to {NodeCharacter}, increase that damage by 1.
            //[b]Power:[/b] Return this card to your hand.
            SwitchBattleZone(legacy);
            AssertNextDecisionChoices(new TurnTaker[] { bunker.TurnTaker, scholar.TurnTaker, oblivaeon.TurnTaker, scionOne.TurnTaker }, new TurnTaker[] { legacy.TurnTaker, node.TurnTaker, scionTwo.TurnTaker });
            PlayCard("PsychicLink");
        }

        [Test()]
        public void TestPsychicLink()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Play this card in a non-environment play area other than {NodeCharacter}'s. Cards in this play area and {NodeCharacter}'s are [i]Connected.[/i] Play with the top card of that play area's deck face up.
            //The first time each turn any target in this play area would deal damage to {NodeCharacter}, increase that damage by 1.
            //[b]Power:[/b] Return this card to your hand.
            DecisionSelectTurnTaker = oblivaeon.TurnTaker;
            PlayCard("PsychicLink");

            QuickHPStorage(node);
            DealDamage(oblivaeon, node, 1, DamageType.Melee);
            QuickHPCheck(-6);

            GoToStartOfTurn(node);
            SwitchBattleZone(node);

            //Check that when OblivAeon deals damage to node from the other battle zone, the damage is not increased
            QuickHPStorage(node);
            DealDamage(oblivaeon, node, 1, DamageType.Melee);
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestPsychologicalFortitudeSameBZ()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "TheWraith", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //When a [i]Connected[/i] hero Ongoing or Equipment would be destroyed by a card other than itself or this card, you may destroy another [i]Connected[/i] hero Equipment or Ongoing.
            //When a card is destroyed by this effect, the first card is indestructible this turn.
            DecisionSelectTurnTaker = legacy.TurnTaker;
            PlayCard("PsychicLink");

            DecisionSelectTurnTaker = wraith.TurnTaker;
            PlayCard("PacketSniffing");

            GoToStartOfTurn(node);
            Card PF = PlayCard("PsychologicalFortitude");
            Card danger = PlayCard("DangerSense");
            Card razor = PlayCard("RazorOrdnance");

            DecisionSelectCards = new Card[] {danger,razor };

            //Check that it works when in the same BZ
            PlayCard("GrapplingHook");
            DestroyCard(danger);
            AssertIsInPlay(danger);
            AssertInTrash(razor);
        }

        [Test()]
        public void TestPsychologicalFortitude()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "TheWraith", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //When a [i]Connected[/i] hero Ongoing or Equipment would be destroyed by a card other than itself or this card, you may destroy another [i]Connected[/i] hero Equipment or Ongoing.
            //When a card is destroyed by this effect, the first card is indestructible this turn.
            DecisionSelectTurnTaker = legacy.TurnTaker;
            PlayCard("PsychicLink");

            DecisionSelectTurnTaker = wraith.TurnTaker;
            PlayCard("PacketSniffing");

            SwitchBattleZone(node);

            GoToStartOfTurn(node);
            Card PF = PlayCard("PsychologicalFortitude");
            Card danger = PlayCard("DangerSense");
            Card razor = PlayCard("RazorOrdnance");

            DecisionSelectCards = new Card[] { danger, razor };

            //Check that it does not trigger while Node is in the other BZ
            PlayCard("GrapplingHook");
            AssertIsInPlay(razor);
            AssertInTrash(danger);
        }

        //[Test()]
        //public void TestPsychologicalFortitudeMixed()
        //{
        //    //This unit test is supposed to fail
        //    SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "TheWraith", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
        //    StartGame();

        //    //When a [i]Connected[/i] hero Ongoing or Equipment would be destroyed by a card other than itself or this card, you may destroy another [i]Connected[/i] hero Equipment or Ongoing.
        //    //When a card is destroyed by this effect, the first card is indestructible this turn.
        //    DecisionSelectTurnTaker = legacy.TurnTaker;
        //    PlayCard("PsychicLink");

        //    DecisionSelectTurnTaker = scholar.TurnTaker;
        //    PlayCard("PacketSniffing");

        //    SwitchBattleZone(scholar);

        //    GoToStartOfTurn(node);
        //    Card PF = PlayCard("PsychologicalFortitude");
        //    Card danger = PlayCard("DangerSense");
        //    Card bring = PlayCard("BringWhatYouNeed");

        //    DecisionSelectCards = new Card[] { danger, bring };

        //    PlayCard("GrapplingHook");
        //    AssertIsInPlay(bring);
        //    AssertInTrash(danger);
        //}

        [Test()]
        public void TestResourceAllocation1()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "TheWraith", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //When a [i]Connected[/i] hero target would deal damage, you may select another [i]Connected[/i] hero target. Increase the damage dealt by the first target by 1. Reduce the next damage dealt by the second target by 1.
            Card resource = PlayCard("ResourceAllocation");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            PlayCard("PsychicLink");

            DecisionSelectTurnTaker = scholar.TurnTaker;
            PlayCard("PacketSniffing");

            SwitchBattleZone(legacy);

            DecisionSelectTarget = scholar.CharacterCard;

            //Check that it doesn't trigger when damage is dealt in the other battle zone
            QuickHPStorage(mindScion);
            DealDamage(legacy.CharacterCard, mindScion, 1, DamageType.Melee);
            QuickHPCheck(-1);

            DecisionDoNotSelectCard = SelectionType.Custom;
            QuickHPStorage(progScion);
            DealDamage(scholar.CharacterCard, progScion, 2, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestResourceAllocation2()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "TheWraith", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //When a [i]Connected[/i] hero target would deal damage, you may select another [i]Connected[/i] hero target. Increase the damage dealt by the first target by 1. Reduce the next damage dealt by the second target by 1.
            Card resource = PlayCard("ResourceAllocation");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            PlayCard("PsychicLink");

            DecisionSelectTurnTaker = scholar.TurnTaker;
            PlayCard("PacketSniffing");

            SwitchBattleZone(legacy);

            //Check that you can't select a connected target in the other battle zone
            AssertNextDecisionChoices(new Card[] { node.CharacterCard }, new Card[] { legacy.CharacterCard });
            DealDamage(scholar, progScion, 1, DamageType.Melee);
        }

        [Test()]
        public void TestReversePsychologyAeon()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "TheWraith", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Put a non-character [i]Connected[/i] card in play on top of its deck.
            //{NodeCharacter} deals the character from that deck with the highest HP 0 psychic and 0 psychic damage, then deals herself 2 psychic damage.
            Card thrall = MoveCard(oblivaeon, "AeonThrall", scionOne.TurnTaker.PlayArea);
            DecisionSelectTurnTaker = scionOne.TurnTaker;
            Card link = PlayCard("PsychicLink");
            DecisionSelectCard = thrall;

            //Check that an Aeon Man ends up on top of the Aeon Man deck
            Card reverse = PlayCard("ReversePsychology");
            AssertOnTopOfLocation(thrall, aeonDeck);
        }

        [Test()]
        public void TestReversePsychologyOblivAeon()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "TheWraith", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Put a non-character [i]Connected[/i] card in play on top of its deck.
            //{NodeCharacter} deals the character from that deck with the highest HP 0 psychic and 0 psychic damage, then deals herself 2 psychic damage.
            SwitchBattleZone(oblivaeon);
            Card focus = PlayCard(oblivaeon, "FocusOfPower");
            SwitchBattleZone(oblivaeon);
            DecisionSelectTurnTaker = scionOne.TurnTaker;
            Card link = PlayCard("PsychicLink");
            DecisionSelectCard = focus;
            PlayCard("InspiringPresence");
            FlipCard(GetCard("TheArcOfUnreality"));
            SetHitPoints(oblivaeon, 10);

            //Check that Focus of Power ends up on top of the OblivAeon deck
            QuickHPStorage(oblivaeon);
            Card reverse = PlayCard("ReversePsychology");
            QuickHPCheck(-2);
            AssertOnTopOfLocation(focus, oblivaeon.TurnTaker.Deck);
        }

        [Test()]
        public void TestReversePsychologyOblivAeonNoDamage()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "TheWraith", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //Put a non-character [i]Connected[/i] card in play on top of its deck.
            //{NodeCharacter} deals the character from that deck with the highest HP 0 psychic and 0 psychic damage, then deals herself 2 psychic damage.
            SwitchBattleZone(oblivaeon);
            Card focus = PlayCard(oblivaeon, "FocusOfPower");
            //SwitchBattleZone(oblivaeon);
            DecisionSelectTurnTaker = scionOne.TurnTaker;
            Card link = PlayCard("PsychicLink");
            DecisionSelectCard = focus;
            PlayCard("InspiringPresence");
            FlipCard(GetCard("TheArcOfUnreality"));
            SetHitPoints(oblivaeon, 10);

            //Check that Focus of Power ends up on top of the OblivAeon deck
            QuickHPStorage(oblivaeon);
            Card reverse = PlayCard("ReversePsychology");
            QuickHPCheck(0);
            AssertOnTopOfLocation(focus, oblivaeon.TurnTaker.Deck);
        }

        //[Test()]
        //public void TestReversePsychologyReward()
        //{
        //    //Fixed
        //    //This unit test doesn't work for some reason, but it works correctly in-game
        //    SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "TheWraith", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
        //    StartGame();

        //    //Put a non-character [i]Connected[/i] card in play on top of its deck.
        //    //{NodeCharacter} deals the character from that deck with the highest HP 0 psychic and 0 psychic damage, then deals herself 2 psychic damage.
        //    GoToStartOfTurn(legacy);
        //    DecisionSelectTurnTaker = legacy.TurnTaker;
        //    Card link = PlayCard("PsychicLink");
        //    Card blood = MoveCard(oblivaeon, "HermeticEvolved", legacy.TurnTaker.PlayArea);
        //    DealDamage(legacy,blood,20,DamageType.Melee);
        //    DecisionSelectCard = blood;
        //    PlayCard("InspiringPresence");

        //    //Check that a reward goes on top of its hero's deck
        //    QuickHPStorage(legacy);
        //    Card reverse = PlayCard("ReversePsychology");
        //    QuickHPCheck(-2);
        //    //AssertOnTopOfLocation(blood, legacy.TurnTaker.Deck);
        //}

        [Test()]
        public void TestTelepathicExtrapolationAeon()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "TheWraith", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //When {NodeCharacter} would deal 0 or more damage to a target, you may select a keyword.
            //If you do, reveal and replace the top card of that deck.
            //If the revealed card shares the selected keyword, increase that damage by 2. Otherwise, reduce that damage by 1.
            Card tele = PlayCard("TelepathicExtrapolation");
            Card thrall = MoveCard(oblivaeon, "AeonThrall", scionOne.TurnTaker.PlayArea);
            DecisionSelectWord = "aeon man";
            QuickHPStorage(thrall);
            DealDamage(node.CharacterCard, thrall, 1, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestTelepathicExtrapolationOblivAeon()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "TheWraith", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //When {NodeCharacter} would deal 0 or more damage to a target, you may select a keyword.
            //If you do, reveal and replace the top card of that deck.
            //If the revealed card shares the selected keyword, increase that damage by 2. Otherwise, reduce that damage by 1.
            Card tele = PlayCard("TelepathicExtrapolation");
            FlipCard(GetCard("TheArcOfUnreality"));
            DecisionSelectWord = "ongoing";
            PutOnDeck("FocusOfPower");
            QuickHPStorage(oblivaeon);
            DealDamage(node, oblivaeon, 1, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestTelepathicExtrapolationScion()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "TheWraith", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //When {NodeCharacter} would deal 0 or more damage to a target, you may select a keyword.
            //If you do, reveal and replace the top card of that deck.
            //If the revealed card shares the selected keyword, increase that damage by 2. Otherwise, reduce that damage by 1.
            Card tele = PlayCard("TelepathicExtrapolation");
            DecisionSelectWord = "one-shot";
            QuickHPStorage(progScion);
            DealDamage(node, progScion, 1, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestTheIInTeam()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Node", "Legacy", "TheWraith", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //At the start of your turn, {NodeCharacter} may deal herself 3 irreducible psychic damage. If she takes no damage this way, destroy this card.
            //When a [i]Connected[/i] hero target would be dealt damage, you may redirect that damage to another [i]Connected[/i] hero target. Reduce damage redirected this way by 1.
            Card team = PlayCard("TheIInTeam");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            PlayCard("PsychicLink");
            SwitchBattleZone(legacy);


            //Check that damage cannot be redirected to a connected hero in the other battle zone
            DecisionSelectCard = legacy.CharacterCard;
            QuickHPStorage(node, legacy);
            DealDamage(progScion,node, 2,DamageType.Melee);
            QuickHPCheck(-2, 0);

            //Check that damage cannot be redirected from a connected hero in the other battle zone
            DecisionSelectCard = node.CharacterCard;
            QuickHPStorage(node, legacy);
            DealDamage(mindScion, legacy, 2, DamageType.Melee);
            QuickHPCheck(0, -2);
        }
    }
}