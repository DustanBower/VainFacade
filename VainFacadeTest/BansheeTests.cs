using NUnit.Framework;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.UnitTest;
using System.Linq;
using VainFacadePlaytest.Banshee;
using Handelabra;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace VainFacadeTest
{
    [TestFixture()]
    public class BansheeTests : BaseTest
    {
        protected HeroTurnTakerController banshee { get { return FindHero("Banshee"); } }
        protected HeroTurnTakerController friday { get { return FindHero("Friday"); } }

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(banshee.CharacterCard, 1);
            DealDamage(villain, banshee, 2, DamageType.Melee, true);
        }

        [Test()]
        public void TestLoadBanshee()
        {
            SetupGameController("BaronBlade", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "TheScholar", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(banshee);
            Assert.IsInstanceOf(typeof(BansheeCharacterCardController), banshee.CharacterCardController);

            Assert.AreEqual(26, banshee.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestInnatePower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Select a non-hero target. Increase the next damage dealt to that target by 2.
            DecisionSelectTarget = akash.CharacterCard;
            Card rockslide = PutOnDeck("LivingRockslide");
            Card raptor = PlayCard("VelociraptorPack");
            AssertNextDecisionChoices(new Card[] { akash.CharacterCard, rockslide, raptor }, new Card[] { banshee.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, scholar.CharacterCard });
            UsePower(banshee);

            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-3);

            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestIncap1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Destroy an environment card.
            Card entomb = PlayCard("Entomb");
            Card raptor = PlayCard("VelociraptorPack");

            SetupIncap(akash);
            AssertNextDecisionChoices(new Card[] { raptor }, new Card[] { entomb, akash.CharacterCard, banshee.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, scholar.CharacterCard });
            UseIncapacitatedAbility(banshee, 0);
            AssertInTrash(raptor);
        }

        [Test()]
        public void TestIncap2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Destroy an ongoing card.
            Card entomb = PutOnDeck("Entomb");
            Card raptor = PlayCard("VelociraptorPack");

            SetupIncap(akash);
            AssertNextDecisionChoices(new Card[] { entomb }, new Card[] { raptor, akash.CharacterCard, banshee.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, scholar.CharacterCard });
            UseIncapacitatedAbility(banshee, 1);
            AssertInTrash(entomb);
        }

        [Test()]
        public void TestIncap3()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //Destroy a target with 3 or fewer HP.
            Card entomb = PlayCard("Entomb");
            Card raptor = PlayCard("VelociraptorPack");
            Card rockslide = PlayCard("LivingRockslide");
            SetHitPoints(rockslide, 3);

            SetupIncap(akash);
            AssertNextDecisionChoices(new Card[] { rockslide }, new Card[] { entomb, raptor, akash.CharacterCard, banshee.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, scholar.CharacterCard });
            UseIncapacitatedAbility(banshee, 2);
            AssertInTrash(rockslide);
        }

        [Test()]
        public void TestDirge()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When this card enters play, return all of your other dirges in play to your hand.
            //Play this card next to a target.
            //At the end of that target's turn, if this card did not enter play this turn, play up to 1 card and return this card to your hand.

            //Check that other dirges get returned to hand
            GoToPlayCardPhase(akash);
            DecisionSelectTarget = akash.CharacterCard;
            Card bridge = PlayCard("BridgeOfTheBroken");
            Card chorus = PlayCard("ChorusOfChaos");
            AssertInHand(bridge);
            AssertAtLocation(chorus, akash.CharacterCard.NextToLocation);

            //Check end of turn action when it did enter play this turn
            Card rhythm = PutInHand("RuinousRhythm");
            DecisionSelectCardToPlay = rhythm;
            GoToEndOfTurn(akash);
            AssertIsInPlay(chorus);
            AssertInHand(rhythm);

            //Check end of turn action when it did not enter play this turn
            GoToStartOfTurn(akash);
            GoToEndOfTurn(akash);
            AssertIsInPlay(rhythm);
            AssertInHand(chorus);
        }

        [Test()]
        public void TestBetterOffDeadBansheeIncap()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();
            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += MakeDecisions2;

            //When {Banshee} would be incapacitated, each player may first play a card.
            //Power: Draw a card. 1 incapacitated hero activates an ability. Then, if {Banshee} has 10 or fewer hp, you may play a card.
            Card better = PlayCard("BetterOffDead");
            MoveAllCards(banshee, banshee.HeroTurnTaker.Hand, banshee.TurnTaker.Trash);
            MoveAllCards(legacy, legacy.HeroTurnTaker.Hand, legacy.TurnTaker.Trash);
            MoveAllCards(bunker, bunker.HeroTurnTaker.Hand, bunker.TurnTaker.Trash);
            MoveAllCards(ra, ra.HeroTurnTaker.Hand, ra.TurnTaker.Trash);
            PutInHand("GraveRobber");
            Card flesh = PutInTrash("FleshOfTheSunGod");
            DecisionSelectCard = flesh;
            Card fortitude = PutInHand("Fortitude");
            Card ammo = PutInHand("AmmoDrop");
            Card blaze = PutInHand("BlazingTornado");
            
            SetupIncap(akash);
            AssertIsInPlay(new Card[] {flesh, fortitude, ammo, blaze });
        }

        [Test()]
        public void TestBetterOffDeadPower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //When {Banshee} would be incapacitated, each player may first play a card.
            //Power: Draw a card. 1 incapacitated hero activates an ability. Then, if {Banshee} has 10 or fewer hp, you may play a card.
            Card better = PlayCard("BetterOffDead");
            MoveCards(banshee, FindCardsWhere((Card c) => c.Identifier == "RuinousRhythm"),banshee.TurnTaker.Trash);
            Card rhythm = PutInHand("RuinousRhythm");
            DestroyCard(legacy);
            QuickHandStorage(banshee);
            UsePower(better);
            QuickHandCheck(1);
            AssertInHand(rhythm);

            //Legacy's incap let Banshee use her power, so check that she did
            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-3);

            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestBetterOffDeadPower2()
        {

            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //When {Banshee} would be incapacitated, each player may first play a card.
            //Power: Draw a card. 1 incapacitated hero activates an ability. Then, if {Banshee} has 10 or fewer hp, you may play a card.
            Card better = PlayCard("BetterOffDead");
            Card rhythm = PutInHand("RuinousRhythm");
            SetHitPoints(banshee, 10);
            DecisionSelectCardToPlay = rhythm;
            UsePower(better);
            AssertIsInPlay(rhythm);
        }

        [Test()]
        public void TestBetterOffDeadOblivAeon()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            //When {Banshee} would be incapacitated, each player may first play a card.
            //Power: Draw a card. 1 incapacitated hero activates an ability. Then, if {Banshee} has 10 or fewer hp, you may play a card.
            Card better = PlayCard("BetterOffDead");
            DestroyCard(legacy);
            DecisionYesNo = false;
            GoToAfterEndOfTurn(oblivaeon);
            GoToStartOfTurn(banshee);
            UsePower(better);
        }

        [Test()]
        public void TestBridgeOfTheBroken()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //When this card enters play, return all of your other dirges in play to your hand.
            //Play this card next to a target. Damage dealt to that target is irreducible. The first time each turn that target deals damage, {Banshee} may use a power.
            //At the end of that target's turn, if this card did not enter play this turn, play up to 1 card and return this card to your hand.
            Card bridge = PlayCard("BridgeOfTheBroken");
            Card carapace = PlayCard("MountainousCarapace");

            QuickHPStorage(akash);
            DealDamage(legacy, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);

            DealDamage(akash, legacy, 1,DamageType.Melee);

            QuickHPStorage(akash);
            DealDamage(legacy, akash, 1, DamageType.Melee);
            QuickHPCheck(-3);

            DealDamage(akash, legacy, 1, DamageType.Melee);

            QuickHPStorage(akash);
            DealDamage(legacy, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestCallOfTheGrave()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //Discard any number of cards.
            //{Banshee} deals herself 1 irreducible infernal damage for each card discarded this way.
            //{Banshee} deals a target X irreducible infernal damage, where X = 2 times the damage she took this way.
            MoveAllCards(banshee, banshee.HeroTurnTaker.Hand, banshee.TurnTaker.Trash);
            Card rhythm = PutInHand("RuinousRhythm");
            Card bridge = PutInHand("BridgeOfTheBroken");
            Card chorus = PutInHand("ChorusOfChaos");
            Card carapace = PlayCard("MountainousCarapace");

            QuickHPStorage(banshee, akash);
            Card call = PlayCard("CallOfTheGrave");
            AssertInTrash(new Card[] { rhythm, bridge, chorus });
            QuickHPCheck(-3, -6);
        }

        [Test()]
        public void TestCallOfTheGraveIncrease()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //Discard any number of cards.
            //{Banshee} deals herself 1 irreducible infernal damage for each card discarded this way.
            //{Banshee} deals a target X irreducible infernal damage, where X = 2 times the damage she took this way.
            MoveAllCards(banshee, banshee.HeroTurnTaker.Hand, banshee.TurnTaker.Trash);
            Card rhythm = PutInHand("RuinousRhythm");
            Card bridge = PutInHand("BridgeOfTheBroken");
            Card chorus = PutInHand("ChorusOfChaos");
            Card carapace = PlayCard("MountainousCarapace");

            UsePower(legacy);

            QuickHPStorage(banshee, akash);
            Card call = PlayCard("CallOfTheGrave");
            AssertInTrash(new Card[] { rhythm, bridge, chorus });
            QuickHPCheck(-6, -13);
        }

        [Test()]
        public void TestChorusOfChaos()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //When this card enters play, return all of your other dirges in play to your hand.
            //Play this card next to a target. The first time each turn a non-hero card deals damage to another target, that source deals that same damage to the target next to this card.
            //At the end of that target's turn, if this card did not enter play this turn, play up to 1 card and return this card to your hand.

            Card chorus = PlayCard("ChorusOfChaos");

            //Check that it works
            QuickHPStorage(akash);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that it only works the first time
            QuickHPStorage(akash);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(0);

            //Check that it works again next turn
            GoToStartOfTurn(banshee);
            QuickHPStorage(akash);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that it doesn't work when the target next to Chorus of Chaos is dealt damage
            GoToStartOfTurn(legacy);
            Card rockslide = PlayCard("LivingRockslide");

            QuickHPStorage(akash);
            DealDamage(rockslide, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that dealing that target damage did not use up the "once per turn" clause
            QuickHPStorage(akash);
            DealDamage(akash, legacy, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestDecayingTones()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //When a card is destroyed, you may draw a card.
            //At the start of your turn, {Banshee} regains 2 hp. Then destroy this card.
            Card tones = PlayCard("DecayingTones");
            Card fortitude = PlayCard("Fortitude");
            SetHitPoints(banshee, 10);
            Card rhythm = PutOnDeck("RuinousRhythm");

            QuickHandStorage(banshee);
            DestroyCard(fortitude);
            QuickHandCheck(1);
            AssertInHand(rhythm);

            QuickHPStorage(banshee);
            GoToStartOfTurn(banshee);
            QuickHPCheck(2);
            AssertInTrash(tones);
        }

        [Test()]
        public void TestDreadCacophony()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //1 target deals itself 1 irreducible infernal damage.
            //You may draw a card.
            //You may play a card.
            Card carapace = PlayCard("MountainousCarapace");
            Card rhythm = PutInHand("RuinousRhythm");
            Card chorus = PutOnDeck("ChorusOfChaos");

            DecisionSelectCardToPlay = rhythm;
            QuickHPStorage(akash);
            Card dread = PlayCard("DreadCacophony");
            QuickHPCheck(-1);
            AssertInHand(chorus);
            AssertIsInPlay(rhythm);
        }

        [Test()]
        public void TestDreamThiefDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //Discard the top 2 cards of your deck, the villain deck, and the environment deck. You may put a card discarded this way into play from the trash.
            //You may draw or play a card.
            Card carapace = PutOnDeck("MountainousCarapace");
            Card rockslide = PutOnDeck("LivingRockslide");
            Card raptor = PutOnDeck("VelociraptorPack");
            Card field = PutOnDeck("ObsidianField");
            Card reapers = PutOnDeck("ReapersRest");
            Card rhythm = PutOnDeck("RuinousRhythm");
            Card chorus = PutOnDeck("ChorusOfChaos");

            Card phalanges = PutInTrash("ArborealPhalanges");
            Card tension = PutInTrash("TensionAndResolution");
            Card river = PutInTrash("RiverOfLava");

            DecisionSelectCard = rhythm;
            DecisionSelectFunction = 1;

            AssertNextDecisionChoices(new Card[] { carapace, rockslide, raptor, field, rhythm, chorus }, new Card[] { phalanges, tension, river });
            Card thief = PlayCard("DreamThief");
            AssertInHand(reapers);
            AssertIsInPlay(rhythm);
            AssertInTrash(new Card[] { carapace, rockslide, raptor, field, chorus });
        }

        [Test()]
        public void TestDreamThiefPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //Discard the top 2 cards of your deck, the villain deck, and the environment deck. You may put a card discarded this way into play from the trash.
            //You may draw or play a card.
            Card carapace = PutOnDeck("MountainousCarapace");
            Card rockslide = PutOnDeck("LivingRockslide");
            Card raptor = PutOnDeck("VelociraptorPack");
            Card field = PutOnDeck("ObsidianField");
            Card rhythm = PutOnDeck("RuinousRhythm");
            Card chorus = PutOnDeck("ChorusOfChaos");
            Card morendo = PutInHand("MournfulMorendo");

            Card phalanges = PutInTrash("ArborealPhalanges");
            Card tension = PutInTrash("TensionAndResolution");
            Card river = PutInTrash("RiverOfLava");

            DecisionSelectCards = new Card[] { rhythm, morendo };
            DecisionSelectFunction = 0;

            AssertNextDecisionChoices(new Card[] { carapace, rockslide, raptor, field, rhythm, chorus }, new Card[] { phalanges, tension, river });
            Card thief = PlayCard("DreamThief");
            AssertIsInPlay(new Card[] { rhythm, morendo });
            AssertInTrash(new Card[] { carapace, rockslide, raptor, field, chorus });
        }

        [Test()]
        public void TestDreamThiefShuffle()
        {
            SetupGameController("GrandWarlordVoss", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //Discard the top 2 cards of your deck, the villain deck, and the environment deck. You may put a card discarded this way into play from the trash.
            //You may draw or play a card.

            //Check that if a deck is shuffled by the second discard, the first card is not included in the choices
            MoveAllCards(FindEnvironment(), FindEnvironment().TurnTaker.Deck, FindEnvironment().TurnTaker.Trash);
            Card toxicguy = PutOnDeck("GeneBoundBionaut");
            Card coldguy = PutOnDeck("GeneBoundFrosthound");
            Card raptor = PutOnDeck("VelociraptorPack");
            Card volcano = GetCard("VolcanicEruption");
            Card rhythm = PutOnDeck("RuinousRhythm");
            Card chorus = PutOnDeck("ChorusOfChaos");

            StackAfterShuffle(FindEnvironment().TurnTaker.Deck,new string[] { "VolcanicEruption" });

            AssertNextDecisionChoices(new Card[] { toxicguy, coldguy, volcano, rhythm, chorus }, new Card[] { raptor });
            PlayCard("DreamThief");
        }

        [Test()]
        public void TestFatalFortissimo()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //Select up to 3 targets. Increase the next damage dealt to those targets by 2.
            DecisionSelectCards = new Card[] { akash.CharacterCard, legacy.CharacterCard, bunker.CharacterCard };
            Card fatal = PlayCard("FatalFortissimo");

            QuickHPStorage(akash, banshee,legacy,bunker,ra);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            DealDamage(banshee, banshee, 1, DamageType.Melee);
            DealDamage(banshee, legacy, 1, DamageType.Melee);
            DealDamage(banshee, bunker, 1, DamageType.Melee);
            DealDamage(banshee, ra, 1, DamageType.Melee);
            QuickHPCheck(-3, -1, -3, -3, -1);

            QuickHPStorage(akash, banshee, legacy, bunker, ra);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            DealDamage(banshee, banshee, 1, DamageType.Melee);
            DealDamage(banshee, legacy, 1, DamageType.Melee);
            DealDamage(banshee, bunker, 1, DamageType.Melee);
            DealDamage(banshee, ra, 1, DamageType.Melee);
            QuickHPCheck(-1, -1, -1, -1, -1);
        }

        [Test()]
        public void TestGraveRobber()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //Put a card other than a one-shot from a trash into play.
            Card thokk = PutInTrash("Thokk");
            Card inspiring = PutInTrash("InspiringPresence");
            Card strike = PutInTrash("BackFistStrike");

            Card grave = PlayCard("GraveRobber");
            AssertIsInPlay(inspiring);
        }

        [Test()]
        public void TestMenacingMelody1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //Power: A target deals itself 0 irreducible infernal damage. Then, if that target has 3 or fewer hp, destroy it and {Banshee} regains X hp, where X = that target's hp before it was destroyed.
            //Power: Select a keyword. Increase the next damage dealt to each target with that keyword by 1.
            PutOnDeck("AlliesOfTheEarth");
            Card raptor = PlayCard("VelociraptorPack");
            Card melody = PlayCard("MenacingMelody");
            SetHitPoints(raptor, 3);
            SetHitPoints(banshee, 10);
            DecisionSelectTarget = raptor;

            QuickHPStorage(banshee);
            UsePower(melody, 0);
            QuickHPCheck(3);
        }

        [Test()]
        public void TestMenacingMelody2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //Power: A target deals itself 0 irreducible infernal damage. Then, if that target has 3 or fewer hp, destroy it and {Banshee} regains X hp, where X = that target's hp before it was destroyed.
            //Power: Select a keyword. Increase the next damage dealt to each target with that keyword by 1.
            Card melody = PlayCard("MenacingMelody");
            Card rockslide = PlayCard("LivingRockslide");
            Card arboreal = PlayCard("ArborealPhalanges");
            DecisionSelectWord = "primeval limb";
            UsePower(melody, 1);

            //Check that next damage to Living Rockslide is increased
            QuickHPStorage(rockslide);
            DealDamage(banshee,rockslide,1,DamageType.Melee);
            QuickHPCheck(-2);

            QuickHPStorage(rockslide);
            DealDamage(banshee, rockslide, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that next damage to Arboreal Phalanges is increased
            QuickHPStorage(arboreal);
            DealDamage(banshee, arboreal, 1, DamageType.Melee);
            QuickHPCheck(-2);

            QuickHPStorage(arboreal);
            DealDamage(banshee, arboreal, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that next damage to Ensnaring Brambles is not increased, since it was not in play when the power was used.
            Card brambles = PlayCard("EnsnaringBrambles");
            QuickHPStorage(brambles);
            DealDamage(banshee, brambles, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestMournfulMorendo1_Shuffle()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //Power: Select an environment card. You may shuffle it into its deck. If you do, destroy this card. Otherwise, destroy the selected card.
            //Power: Play a card.
            Card morendo = PlayCard("MournfulMorendo");
            Card field = PlayCard("ObsidianField");
            DecisionYesNo = true;
            UsePower(morendo, 0);
            AssertInDeck(field);
            AssertInTrash(morendo);
        }

        [Test()]
        public void TestMournfulMorendo1_NoShuffle()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //Power: Select an environment card. You may shuffle it into its deck. If you do, destroy this card. Otherwise, destroy the selected card.
            //Power: Play a card.
            Card morendo = PlayCard("MournfulMorendo");
            Card field = PlayCard("ObsidianField");
            DecisionYesNo = false;
            UsePower(morendo, 0);
            AssertInTrash(field);
            AssertIsInPlay(morendo);
        }

        [Test()]
        public void TestMournfulMorendo2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //Power: Select an environment card. You may shuffle it into its deck. If you do, destroy this card. Otherwise, destroy the selected card.
            //Power: Play a card.
            Card morendo = PlayCard("MournfulMorendo");
            Card rhythm = PutInHand("RuinousRhythm");
            DecisionSelectCardToPlay = rhythm;
            UsePower(morendo, 1);
            AssertIsInPlay(rhythm);
        }

        [Test()]
        public void TestReapersRestPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //When this card enters play, you may draw and play a card.
            //At the end of each player's turn, if that player did not play a card, did not use a power, or did not draw a card this turn, select a target. Increase the next damage dealt to that target by 1.
            Card rhythm = PutInHand("RuinousRhythm");
            Card chorus = PutOnDeck("ChorusOfChaos");
            DecisionSelectCardToPlay = rhythm;
            PlayCard("ReapersRest");
            AssertIsInPlay(rhythm);
            AssertInHand(chorus);
        }

        [Test()]
        public void TestReapersRestEndOfTurn()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Bunker", "Legacy", "Ra", "InsulaPrimalis");
            StartGame();

            //When this card enters play, you may draw and play a card.
            //At the end of each player's turn, if that player did not play a card, did not use a power, or did not draw a card this turn, select a target. Increase the next damage dealt to that target by 1.
            DecisionDoNotSelectCard = SelectionType.PlayCard;
            Card reaper = GoToPlayCardPhaseAndPlayCard(banshee, "ReapersRest");

            ResetDecisions();
            DecisionSelectCard = akash.CharacterCard;
            GoToEndOfTurn(banshee);

            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-2);

            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);

            GoToPlayCardPhaseAndPlayCard(bunker, "AmmoDrop");
            GoToUsePowerPhase(bunker);
            UsePower(bunker);
            GoToEndOfTurn(bunker);
            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestReapersRestEndOfTurn2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Bunker", "Legacy", "Ra", "InsulaPrimalis");
            StartGame();

            //When this card enters play, you may draw and play a card.
            //At the end of each player's turn, if that player did not play a card, did not use a power, or did not draw a card this turn, select a target. Increase the next damage dealt to that target by 1.
            DecisionDoNotSelectCard = SelectionType.PlayCard;
            Card reaper = GoToPlayCardPhaseAndPlayCard(banshee, "ReapersRest");
            Card reaper2 = GetCard("ReapersRest", 0, (Card c) => !c.IsInPlayAndHasGameText);
            PlayCard(reaper2);

            ResetDecisions();
            DecisionSelectCard = akash.CharacterCard;
            GoToEndOfTurn(banshee);

            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-3);

            GoToEndOfTurn(bunker);
            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-3);
        }

        //[Test()]
        //public void TestReapersRestEndOfTurn3()
        //{
        //    SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "VainFacadePlaytest.Friday", "Ra", "InsulaPrimalis");
        //    StartGame();
        //    this.GameController.OnMakeDecisions -= this.MakeDecisions;
        //    this.GameController.OnMakeDecisions += this.MakeDecisions2;

        //    //When this card enters play, you may draw and play a card.
        //    //At the end of each player's turn, if that player did not play a card, did not use a power, or did not draw a card this turn, select a target. Increase the next damage dealt to that target by 1.

        //    //If Grim Reflection copies the first Reaper's Rest that entered play, only the first Reaper's Rest and Grim Reflection will activate at the end of a turn.
        //    //If Grim Reflection copies only the second Reaper's Rest that entered play, all three will activate
        //    DecisionDoNotSelectCard = SelectionType.PlayCard;
        //    Card reaper = GoToPlayCardPhaseAndPlayCard(banshee, "ReapersRest");

        //    Card reaper2 = GetCard("ReapersRest", 0, (Card c) => !c.IsInPlayAndHasGameText);
        //    PlayCard(reaper2);

        //    Card grim = PlayCard("GrimReflection");
        //    MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash, false, 1);
        //    PutOnDeck("DamselInDistress");
        //    DecisionSelectFunction = 0;
        //    DecisionSelectCard = reaper2;
        //    UsePower(grim);

        //    //Card reaper2 = GetCard("ReapersRest", 0, (Card c) => !c.IsInPlayAndHasGameText);
        //    //PlayCard(reaper2);

        //    ResetDecisions();
        //    DecisionSelectCard = akash.CharacterCard;
        //    GoToEndOfTurn(banshee);

        //    QuickHPStorage(akash);
        //    DealDamage(banshee, akash, 1, DamageType.Melee);
        //    QuickHPCheck(-4);

        //    GoToEndOfTurn(legacy);
        //    QuickHPStorage(akash);
        //    DealDamage(banshee, akash, 1, DamageType.Melee);
        //    QuickHPCheck(-4);
        //}

        //[Test()]
        //public void TestReapersRestUhYeah()
        //{
        //    //This test passes with random seed 216012794, but fails with random seed 216012795
        //    SetupGameController(new string[]{ "AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Guise", "Ra", "InsulaPrimalis"}, randomSeed: 216012795);
        //    StartGame();

        //    Card reaper = GoToPlayCardPhaseAndPlayCard(banshee, "ReapersRestTest");

        //    Card reaper2 = GetCard("ReapersRestTest", 0, (Card c) => !c.IsInPlayAndHasGameText);
        //    PlayCard(reaper2);

        //    DestroyCard(reaper);
        //    PlayCard(reaper);

        //    Assert.AreEqual(2,FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.Title == "Reaper's Rest").Count());

        //    DecisionSelectTurnTaker = banshee.TurnTaker;
        //    PlayCard("UhYeahImThatGuy");

        //    ResetDecisions();
        //    DecisionSelectCard = akash.CharacterCard;
        //    GoToEndOfTurn(banshee);

        //    QuickHPStorage(akash);
        //    DealDamage(banshee, akash, 1, DamageType.Melee);
        //    QuickHPCheck(-4);
        //}

        //[Test()]
        //public void TestExploitUhYeah()
        //{
        //    SetupGameController("Omnitron", "Parse", "Legacy", "Guise", "Ra", "InsulaPrimalis");
        //    StartGame();

        //    DecisionDoNotSelectCard = SelectionType.PlayCard;
        //    Card exploit = GoToPlayCardPhaseAndPlayCard(parse, "ExploitVulnerability");

        //    Card exploit2 = GetCard("ExploitVulnerability", 0, (Card c) => !c.IsInPlayAndHasGameText);
        //    PlayCard(exploit2);

        //    DecisionSelectTurnTaker = parse.TurnTaker;
        //    PlayCard("UhYeahImThatGuy");

        //    DestroyCard(exploit);

        //    ResetDecisions();
        //    Card rockslide = PlayCard("EnragedTRex");

        //    QuickHPStorage(rockslide);
        //    DealDamage(parse, rockslide, 1, DamageType.Melee);
        //    QuickHPCheck(-3);
        //}

        [Test()]
        public void TestRuinousRhythm1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //Destroy a non-character, non-target card. If a non-ongoing is destroyed this way, destroy this card.
            //Draw a card.
            Card rhythm = PlayCard("RuinousRhythm");
            Card rockslide = PlayCard("LivingRockslide");
            Card entomb = PlayCard("Entomb");
            Card field = PlayCard("ObsidianField");

            AssertNextDecisionChoices(new Card[] { entomb, field, rhythm }, new Card[] { rockslide, akash.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, ra.CharacterCard, banshee.CharacterCard });
            DecisionSelectCard = entomb;
            UsePower(rhythm,0);
            AssertInTrash(entomb);
            AssertIsInPlay(rhythm);

            DecisionSelectCard = field;
            UsePower(rhythm, 0);
            AssertInTrash(new Card[] { field, rhythm });
        }

        [Test()]
        public void TestTensionAndResolution()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //The first time each non-hero turn you play a card with a name not in play, you may draw a card and use a power.
            Card tension = PlayCard("TensionAndResolution");

            //Check that it works on the environment turn
            GoToStartOfTurn(FindEnvironment());
            Card chorus = PutOnDeck("ChorusOfChaos");
            Card rhythm = PlayCard("RuinousRhythm");
            AssertInHand(chorus);
            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-3);
            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that playing another card does not trigger it
            Card chorus2 = PutOnDeck("ChorusOfChaos");
            Card morendo = PlayCard("MournfulMorendo");
            AssertOnTopOfDeck(chorus2);
            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that playing a card with a name in play does not trigger it.
            GoToStartOfTurn(akash);
            Card chorus3 = PutOnDeck("ChorusOfChaos");
            Card morendo2 = GetCard("MournfulMorendo",0,(Card c) => !c.Location.IsInPlay);
            PlayCard(morendo2);
            AssertOnTopOfDeck(chorus3);
            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestTensionAndResolutionHeroTurn()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //The first time each non-hero turn you play a card with a name not in play, you may draw a card and use a power.
            Card tension = PlayCard("TensionAndResolution");

            //Check that it does not trigger on hero turns
            GoToStartOfTurn(banshee);
            Card chorus = PutOnDeck("ChorusOfChaos");
            Card rhythm = PlayCard("RuinousRhythm");
            AssertOnTopOfDeck(chorus);
            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestTensionAndResolutionPutIntoPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //The first time each non-hero turn you play a card with a name not in play, you may draw a card and use a power.
            Card tension = PlayCard("TensionAndResolution");

            //Check that it does not trigger when you put a card into play
            GoToStartOfTurn(banshee);
            Card chorus = PutOnDeck("ChorusOfChaos");
            Card rhythm = PutIntoPlay("RuinousRhythm");
            AssertOnTopOfDeck(chorus);
            QuickHPStorage(akash);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestVerseForTheVanquishedDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //When this card enters play, return all of your other dirges in play to your hand.
            //Play this card next to a target. The first time each turn that target is dealt damage, a player may discard a card to draw or play a card.
            //At the end of that target's turn, if this card did not enter play this turn, play up to 1 card and return this card to your hand.
            Card verse = PlayCard("VerseForTheVanquished");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card fortitude = PutInHand("Fortitude");
            Card thokk = PutOnDeck("Thokk");
            DecisionSelectCard = fortitude;
            DecisionSelectFunction = 0;
            DealDamage(banshee, akash, 1, DamageType.Melee);
            AssertInTrash(fortitude);
            AssertInHand(thokk);

            //Check that it doesn't trigger a second time
            Card danger = PutInHand("DangerSense");
            Card takedown = PutOnDeck("TakeDown");
            DecisionSelectCard = danger;
            DealDamage(banshee, akash, 1, DamageType.Melee);
            AssertInHand(danger);
            AssertOnTopOfDeck(takedown);

            GoToStartOfTurn(legacy);
            DealDamage(banshee, akash, 1, DamageType.Melee);
            AssertInTrash(danger);
            AssertInHand(takedown);
        }

        [Test()]
        public void TestVerseForTheVanquishedPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //When this card enters play, return all of your other dirges in play to your hand.
            //Play this card next to a target. The first time each turn that target is dealt damage, a player may discard a card to draw or play a card.
            //At the end of that target's turn, if this card did not enter play this turn, play up to 1 card and return this card to your hand.
            Card verse = PlayCard("VerseForTheVanquished");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionSelectFunction = 1;
            Card fortitude = PutInHand("Fortitude");
            Card danger = PutInHand("DangerSense");
            DecisionDiscardCard = fortitude;
            DecisionSelectCardToPlay = danger;
            DealDamage(banshee, akash, 1, DamageType.Melee);
            AssertInTrash(fortitude);
            AssertIsInPlay(danger);
        }

        [Test()]
        public void TestVoiceForTheVoiceless()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //Search your deck for a dirge and put it into your hand. Shuffle your deck.
            //You may draw a card.
            //You may play a card.
            Card chorus = PutInDeck("ChorusOfChaos");
            Card rhythm = PutInHand("RuinousRhythm");

            QuickHandStorage(banshee);
            QuickShuffleStorage(banshee);
            DecisionSelectCard = chorus;
            DecisionSelectCardToPlay = rhythm;
            PlayCard("VoiceForTheVoiceless");
            QuickHandCheck(1);
            QuickShuffleCheck(1);
            AssertInHand(chorus);
            AssertIsInPlay(rhythm);
        }

        [Test()]
        public void TestWretchedRefrain()
        {
            SetupGameController("GrandWarlordVoss", "VainFacadePlaytest.Banshee", "Legacy", "Bunker", "Ra", "InsulaPrimalis");
            StartGame();

            //When this card enters play, return all of your other Dirges in play to your hand.
            //Play this card next to a target. When that target would be dealt damage, you may increase that damage by 1 and change the damage type to a type of your choice.
            //At the end of that target's turn, if this card did not enter play this turn, play up to 1 card and return this card to your hand.
            MoveAllCards(voss, voss.TurnTaker.PlayArea, voss.TurnTaker.Trash);
            Card toxicguy = PlayCard("GeneBoundBionaut");
            DecisionSelectCard = toxicguy;
            Card wretched = PlayCard("WretchedRefrain");
            DecisionYesNo = true;
            DecisionSelectDamageType = DamageType.Energy;

            QuickHPStorage(toxicguy);
            DealDamage(legacy, toxicguy, 1, DamageType.Toxic);
            QuickHPCheck(-2);
        }
    }
}