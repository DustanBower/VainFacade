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

namespace VainFacadeTest
{
    [TestFixture()]
    public class FridayTests : BaseTest
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

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(friday.CharacterCard, 1);
            DealDamage(villain, friday, 2, DamageType.Melee, true);
        }

        [Test()]
        public void TestLoadFriday()
        {
            SetupGameController("BaronBlade", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(friday);
            Assert.IsInstanceOf(typeof(FridayCharacterCardController), friday.CharacterCardController);

            Assert.AreEqual(32, friday.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestFridayInnatePower1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            ;
            //{Friday} deals 1 target 2 melee or projectile damage or 3 damage of a type that has been dealt since the end of your last turn.
            QuickHPStorage(akash);
            UsePower(friday);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestFridayInnatePower2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //{Friday} deals 1 target 2 melee or projectile damage or 3 damage of a type that has been dealt since the end of your last turn.
            GoToStartOfTurn(bunker);
            DealDamage(friday, akash, 1, DamageType.Lightning);
            DecisionSelectFunction = 1;
            QuickHPStorage(akash);
            UsePower(friday);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestFridayIncap1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            SetupIncap(akash);

            //Once before the start of your next turn, when a target deals damage, it may deal 1 damage of the same type to the same target.
            UseIncapacitatedAbility(friday, 0);
            DecisionYesNo = true;
            QuickHPStorage(akash);
            DealDamage(legacy, akash, 1, DamageType.Melee);
            QuickHPCheck(-2);

            QuickHPStorage(akash);
            DealDamage(legacy, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestFridayIncap2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            SetupIncap(akash);

            //One player may put the top card of their trash into play if that card is an ongoing or one-shot. If they do, discard the top card of that deck.
            Card fortitude = PutInTrash("Fortitude");
            Card inspiring = PutOnDeck("InspiringPresence");
            UseIncapacitatedAbility(friday, 1);
            AssertIsInPlay(fortitude);
            AssertInTrash(inspiring);
        }

        [Test()]
        public void TestFridayIncap3()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            SetupIncap(akash);

            //One hero may use a power.
            DecisionSelectTurnTaker = bunker.TurnTaker;
            QuickHandStorage(bunker);
            UseIncapacitatedAbility(friday, 2);
            QuickHandCheck(1);
        }

        [Test()]
        public void TestAdaptiveEndoskeletonLightning()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Increase lightning damage dealt to {Friday} by 1.
            //When {Friday} would be dealt damage by a source other than {Friday}, you may attune this card to that damage's type.
            //Each time {Friday} is dealt damage of a type this card is attuned to, reduce damage of that type dealt to {Friday} by 1 until this card is attuned to another type or this card leaves play.
            Card endo = PlayCard("AdaptiveEndoskeleton");
            QuickHPStorage(friday);
            DealDamage(legacy, friday, 1, DamageType.Lightning);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestAdaptiveEndoskeletonAttune()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Increase lightning damage dealt to {Friday} by 1.
            //When {Friday} would be dealt damage by a source other than {Friday}, you may attune this card to that damage's type.
            //Each time {Friday} is dealt damage of a type this card is attuned to, reduce damage of that type dealt to {Friday} by 1 until this card is attuned to another type or this card leaves play.
            Card endo = PlayCard("AdaptiveEndoskeleton");
            DecisionYesNo = true;

            //First damage should not be reduced
            //Attune to melee
            QuickHPStorage(friday);
            DealDamage(akash, friday, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Second damage should be reduced by 1
            QuickHPStorage(friday);
            DealDamage(akash, friday, 2, DamageType.Melee);
            QuickHPCheck(-1);

            //Third damage should be reduced by 2
            //Reduced to 0, so does not add another -1
            QuickHPStorage(friday);
            DealDamage(akash, friday, 2, DamageType.Melee);
            QuickHPCheck(0);

            //Fourth Damage should be reduced by 2
            QuickHPStorage(friday);
            DealDamage(akash, friday, 3, DamageType.Melee);
            QuickHPCheck(-1);

            //Attune to energy
            QuickHPStorage(friday);
            DealDamage(akash, friday, 1, DamageType.Energy);
            QuickHPCheck(-1);

            //Next energy damage is reduced by 1
            QuickHPStorage(friday);
            DealDamage(akash, friday, 1, DamageType.Energy);
            QuickHPCheck(0);

            //Melee damage is now not reduced
            DecisionYesNo = false;
            QuickHPStorage(friday);
            DealDamage(akash, friday, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Attunement should go away when this card is destroyed
            DestroyCard(endo);
            QuickHPStorage(friday);
            DealDamage(akash, friday, 1, DamageType.Energy);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestAnythingYouCanDoLightning()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Increase lightning damage dealt to {Friday} by 1.
            //Play this card next to a character other than {Friday}. The first time each turn that character regains HP, {Friday} regains 1 hp.
            //The second time each turn...{BR}} a card is drawn from that character's deck, you may draw a card.{BR}} a card from that character's deck enters play, you may play a card.
            Card anything = PlayCard("AnythingYouCanDo");
            QuickHPStorage(friday);
            DealDamage(legacy, friday, 1, DamageType.Lightning);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestAnythingYouCanDoHeal()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Increase lightning damage dealt to {Friday} by 1.
            //Play this card next to a character other than {Friday}. The first time each turn that character regains HP, {Friday} regains 1 hp.
            //The second time each turn...{BR}} a card is drawn from that character's deck, you may draw a card.{BR}} a card from that character's deck enters play, you may play a card.
            DecisionSelectCard = legacy.CharacterCard;
            Card anything = PlayCard("AnythingYouCanDo");
            SetHitPoints(legacy, 10);
            SetHitPoints(friday, 10);

            QuickHPStorage(friday);
            GainHP(legacy, 1);
            QuickHPCheck(1);

            QuickHPStorage(friday);
            GainHP(legacy, 1);
            QuickHPCheck(0);

            GoToStartOfTurn(legacy);
            QuickHPStorage(friday);
            GainHP(legacy, 1);
            QuickHPCheck(1);
        }

        [Test()]
        public void TestAnythingYouCanDoDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Increase lightning damage dealt to {Friday} by 1.
            //Play this card next to a character other than {Friday}. The first time each turn that character regains HP, {Friday} regains 1 hp.
            //The second time each turn...{BR}} a card is drawn from that character's deck, you may draw a card.{BR}} a card from that character's deck enters play, you may play a card.
            DecisionSelectCard = legacy.CharacterCard;
            Card anything = PlayCard("AnythingYouCanDo");

            Card grim = PutOnDeck("GrimReflection");
            Card better = PutOnDeck("ICanDoBetter");
            Card damsel = PutOnDeck("DamselInDistress");
            Card war = PutOnDeck("BuiltForWar");

            QuickHandStorage(friday);
            DrawCard(legacy);
            QuickHandCheck(0);

            QuickHandStorage(friday);
            DrawCard(legacy);
            QuickHandCheck(1);

            QuickHandStorage(friday);
            DrawCard(legacy);
            QuickHandCheck(0);

            GoToStartOfTurn(legacy);
            QuickHandStorage(friday);
            DrawCard(legacy);
            QuickHandCheck(0);

            QuickHandStorage(friday);
            DrawCard(legacy);
            QuickHandCheck(1);

            QuickHandStorage(friday);
            DrawCard(legacy);
            QuickHandCheck(0);
        }

        [Test()]
        public void TestAnythingYouCanDoPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Increase lightning damage dealt to {Friday} by 1.
            //Play this card next to a character other than {Friday}. The first time each turn that character regains HP, {Friday} regains 1 hp.
            //The second time each turn...{BR}} a card is drawn from that character's deck, you may draw a card.{BR}} a card from that character's deck enters play, you may play a card.
            DecisionSelectCard = legacy.CharacterCard;
            Card anything = PlayCard("AnythingYouCanDo");

            Card war = PutInHand("BuiltForWar");
            Card grim = PutInHand("GrimReflection");
            Card better = PutInHand("ICanDoBetter");

            DecisionSelectCardToPlay = war;
            PlayCard("Fortitude");
            AssertInHand(war);

            PlayCard("InspiringPresence");
            AssertIsInPlay(war);

            DecisionSelectCardToPlay = grim;
            PlayCard("NextEvolution");
            AssertInHand(grim);

            GoToStartOfTurn(legacy);
            PlayCard("SurgeOfStrength");
            AssertInHand(grim);

            PlayCard("DangerSense");
            AssertIsInPlay(grim);

            DecisionSelectCardToPlay = better;
            PlayCard("LeadFromTheFront");
            AssertInHand(better);
        }

        [Test()]
        public void TestBuiltForWarReduce()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When {Friday} is dealt 5 or more damage from a single source, you may reduce that damage by up to 3.
            //Power: Reveal cards from the top of your deck until 2 Mimicries are revealed. Put one into your hand or into play. Shuffle the remaining revealed cards into your deck. If a card entered play this way, destroy this card.
            Card war = PlayCard("BuiltForWar");

            DecisionSelectNumber = 3;
            QuickHPStorage(friday);
            DealDamage(akash, friday, 6, DamageType.Melee);
            QuickHPCheck(-3);

            DecisionSelectNumber = 2;
            QuickHPStorage(friday);
            DealDamage(akash, friday, 6, DamageType.Melee);
            QuickHPCheck(-4);

            DecisionSelectNumber = 1;
            QuickHPStorage(friday);
            DealDamage(akash, friday, 6, DamageType.Melee);
            QuickHPCheck(-5);

            DecisionSelectNumber = 3;
            QuickHPStorage(friday);
            DealDamage(akash, friday, 4, DamageType.Melee);
            QuickHPCheck(-4);
        }

        [Test()]
        public void TestBuiltForWarReduce2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "Tempest", "Megalopolis");
            StartGame();

            //When {Friday} is dealt 5 or more damage from a single source, you may reduce that damage by up to 3.
            //Power: Reveal cards from the top of your deck until 2 Mimicries are revealed. Put one into your hand or into play. Shuffle the remaining revealed cards into your deck. If a card entered play this way, destroy this card.
            Card war = PlayCard("BuiltForWar");
            //Card shield = PlayCard("ShieldingWinds");
            Card endo = PlayCard("AdaptiveEndoskeleton");
            DecisionSelectCard = endo;
            DecisionYesNo = true;

            DealDamage(akash, friday, 1, DamageType.Melee);

            ResetDecisions();
            DecisionSelectCard = endo;
            DecisionYesNo = false;

            DecisionSelectNumber = 3;
            QuickHPStorage(friday);
            DealDamage(akash, friday, 10, DamageType.Energy);
            QuickHPCheck(-7);
        }

        [Test()]
        public void TestBuiltForWarPower1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When {Friday} is dealt 5 or more damage from a single source, you may reduce that damage by up to 3.
            //Power: Reveal cards from the top of your deck until 2 Mimicries are revealed. Put one into your hand or into play. Shuffle the remaining revealed cards into your deck. If a card entered play this way, destroy this card.
            Card war = PlayCard("BuiltForWar");
            DecisionMoveCardDestination = new MoveCardDestination(friday.HeroTurnTaker.Hand);
            Card grim = PutOnDeck("GrimReflection");
            Card anything = PutOnDeck("AnythingYouCanDo");
            Card damsel = PutOnDeck("DamselInDistress");
            DecisionSelectCard = grim;
            QuickShuffleStorage(friday);

            UsePower(war);
            AssertIsInPlay(war);
            AssertInHand(grim);
            AssertInDeck(anything);
            AssertInDeck(damsel);
            QuickShuffleCheck(1);
        }

        [Test()]
        public void TestBuiltForWarPower2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When {Friday} is dealt 5 or more damage from a single source, you may reduce that damage by up to 3.
            //Power: Reveal cards from the top of your deck until 2 Mimicries are revealed. Put one into your hand or into play. Shuffle the remaining revealed cards into your deck. If a card entered play this way, destroy this card.
            Card war = PlayCard("BuiltForWar");
            DecisionMoveCardDestination = new MoveCardDestination(friday.TurnTaker.PlayArea);
            Card grim = PutOnDeck("GrimReflection");
            Card anything = PutOnDeck("AnythingYouCanDo");
            Card damsel = PutOnDeck("DamselInDistress");
            DecisionSelectCard = grim;
            QuickShuffleStorage(friday);

            UsePower(war);
            AssertInTrash(war);
            AssertIsInPlay(grim);
            AssertInDeck(anything);
            AssertInDeck(damsel);
            QuickShuffleCheck(1);
        }

        [Test()]
        public void TestDamselInDistress()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When the hero target with the lowest HP would be dealt damage, redirect that damage to {Friday}.
            //When {Friday} is dealt damage by a target, you may destroy this card. If you do, {Friday} deals the source of that damage 4 melee damage.
            Card damsel = PlayCard("DamselInDistress");
            DecisionYesNo = true;
            QuickHPStorage(friday, bunker, akash);

            DealDamage(akash, bunker, 1, DamageType.Melee);
            QuickHPCheck(-1, 0, -4);
            AssertInTrash(damsel);
        }

        [Test()]
        public void TestDamselInDistressNoDestroy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When the hero target with the lowest HP would be dealt damage, redirect that damage to {Friday}.
            //When {Friday} is dealt damage by a target, you may destroy this card. If you do, {Friday} deals the source of that damage 4 melee damage.
            Card damsel = PlayCard("DamselInDistress");
            DecisionYesNo = false;
            QuickHPStorage(friday, bunker, akash);

            DealDamage(akash, bunker, 1, DamageType.Melee);
            QuickHPCheck(-1, 0, 0);
            AssertIsInPlay(damsel);
        }

        [Test()]
        public void TestDamselInDistressNonTargetSource1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When the hero target with the lowest HP would be dealt damage, redirect that damage to {Friday}.
            //When {Friday} is dealt damage by a target, you may destroy this card. If you do, {Friday} deals the source of that damage 4 melee damage.
            Card damsel = PlayCard("DamselInDistress");
            Card allies = PlayCard("AlliesOfTheEarth");
            DecisionYesNo = true;
            QuickHPStorage(friday, bunker, akash);

            DealDamage(allies, bunker.CharacterCard, 1, DamageType.Melee);
            QuickHPCheck(0, -1, 0);
            AssertIsInPlay(damsel);
        }

        [Test()]
        public void TestDamselInDistressNonTargetSource2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When the hero target with the lowest HP would be dealt damage, redirect that damage to {Friday}.
            //When {Friday} is dealt damage by a target, you may destroy this card. If you do, {Friday} deals the source of that damage 4 melee damage.
            Card damsel = PlayCard("DamselInDistress");
            Card allies = PlayCard("AlliesOfTheEarth");
            DecisionYesNo = true;
            QuickHPStorage(friday, bunker, akash);

            DealDamage(allies, friday.CharacterCard, 1, DamageType.Melee);
            QuickHPCheck(-1, 0, 0);
            AssertIsInPlay(damsel);
        }

        [Test()]
        public void TestDamselInDistressFridaySource1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When the hero target with the lowest HP would be dealt damage, redirect that damage to {Friday}.
            //When {Friday} is dealt damage by a target, you may destroy this card. If you do, {Friday} deals the source of that damage 4 melee damage.
            Card damsel = PlayCard("DamselInDistress");
            DecisionYesNo = true;
            QuickHPStorage(friday, bunker);

            DealDamage(friday, bunker, 1, DamageType.Melee);
            QuickHPCheck(0, -1);
            AssertIsInPlay(damsel);
        }

        [Test()]
        public void TestDamselInDistressFridaySource2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When the hero target with the lowest HP would be dealt damage, redirect that damage to {Friday}.
            //When {Friday} is dealt damage by a target, you may destroy this card. If you do, {Friday} deals the source of that damage 4 melee damage.
            Card damsel = PlayCard("DamselInDistress");
            DecisionYesNo = true;
            QuickHPStorage(friday);

            DealDamage(friday, friday, 1, DamageType.Melee);
            QuickHPCheck(-1);
            AssertIsInPlay(damsel);
        }

        [Test()]
        public void TestDoppelgangerDiscard()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Select a deck.
            //You may destroy a target with 4 or fewer hp from that deck.
            //Reveal the top 3 cards of that deck. You may play or discard one of the revealed cards. Replace the other cards in order.
            DecisionSelectLocation = new LocationChoice( akash.TurnTaker.Deck);
            Card rockslide = PlayCard("LivingRockslide");
            SetHitPoints(rockslide, 4);

            Card entomb = PutOnDeck("Entomb");
            Card arboreal = PutOnDeck("ArborealPhalanges");
            Card allies = PutOnDeck("AlliesOfTheEarth");

            DecisionDestroyCard = rockslide;
            DecisionSelectCard = allies;
            DecisionSelectFunction = 1;

            PlayCard("Doppelganger");
            AssertInTrash(allies);
            AssertInTrash(rockslide);
            AssertOnTopOfDeck(arboreal);
            AssertOnTopOfDeck(entomb, 1);
        }

        [Test()]
        public void TestDoppelgangerPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Select a deck.
            //You may destroy a target with 4 or fewer hp from that deck.
            //Reveal the top 3 cards of that deck. You may play or discard one of the revealed cards. Replace the other cards in order.
            DecisionSelectLocation = new LocationChoice(akash.TurnTaker.Deck);
            Card rockslide = PlayCard("LivingRockslide");
            SetHitPoints(rockslide, 4);

            Card entomb = PutOnDeck("Entomb");
            Card arboreal = PutOnDeck("ArborealPhalanges");
            Card allies = PutOnDeck("AlliesOfTheEarth");

            DecisionDestroyCard = rockslide;
            DecisionSelectCard = allies;
            DecisionSelectFunction = 0;

            PlayCard("Doppelganger");
            AssertIsInPlay(allies);
            AssertInTrash(rockslide);
            AssertOnTopOfDeck(arboreal);
            AssertOnTopOfDeck(entomb, 1);
        }

        [Test()]
        public void TestDoppelgangerSkip()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Select a deck.
            //You may destroy a target with 4 or fewer hp from that deck.
            //Reveal the top 3 cards of that deck. You may play or discard one of the revealed cards. Replace the other cards in order.
            DecisionSelectLocation = new LocationChoice(akash.TurnTaker.Deck);
            Card rockslide = PlayCard("LivingRockslide");
            SetHitPoints(rockslide, 4);

            Card entomb = PutOnDeck("Entomb");
            Card arboreal = PutOnDeck("ArborealPhalanges");
            Card allies = PutOnDeck("AlliesOfTheEarth");

            DecisionDestroyCard = rockslide;
            DecisionDoNotSelectFunction = true;

            PlayCard("Doppelganger");
            AssertInTrash(rockslide);
            AssertOnTopOfDeck(allies);
            AssertOnTopOfDeck(arboreal, 1);
            AssertOnTopOfDeck(entomb, 2);
        }

        [Test()]
        public void TestDoppelgangerOneCardDiscard()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Select a deck.
            //You may destroy a target with 4 or fewer hp from that deck.
            //Reveal the top 3 cards of that deck. You may play or discard one of the revealed cards. Replace the other cards in order.
            DecisionSelectLocation = new LocationChoice(akash.TurnTaker.Deck);
            Card rockslide = PlayCard("LivingRockslide");
            SetHitPoints(rockslide, 4);

            MoveAllCards(akash, akash.TurnTaker.Deck, akash.TurnTaker.Trash);

            Card allies = PutOnDeck("AlliesOfTheEarth");

            DecisionDestroyCard = rockslide;
            DecisionSelectFunction = 1;
            
            PlayCard("Doppelganger");
            AssertInTrash(rockslide);
            AssertInTrash(allies);
        }

        [Test()]
        public void TestDoppelgangerOneCardPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Select a deck.
            //You may destroy a target with 4 or fewer hp from that deck.
            //Reveal the top 3 cards of that deck. You may play or discard one of the revealed cards. Replace the other cards in order.
            DecisionSelectLocation = new LocationChoice(akash.TurnTaker.Deck);
            Card rockslide = PlayCard("LivingRockslide");
            SetHitPoints(rockslide, 4);

            MoveAllCards(akash, akash.TurnTaker.Deck, akash.TurnTaker.Trash);

            Card allies = PutOnDeck("AlliesOfTheEarth");

            DecisionDestroyCard = rockslide;
            DecisionSelectFunction = 0;

            PlayCard("Doppelganger");
            AssertInTrash(rockslide);
            AssertIsInPlay(allies);
        }

        [Test()]
        public void TestDoppelgangerOneCardSkip()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Select a deck.
            //You may destroy a target with 4 or fewer hp from that deck.
            //Reveal the top 3 cards of that deck. You may play or discard one of the revealed cards. Replace the other cards in order.
            DecisionSelectLocation = new LocationChoice(akash.TurnTaker.Deck);
            Card rockslide = PlayCard("LivingRockslide");
            SetHitPoints(rockslide, 4);

            MoveAllCards(akash, akash.TurnTaker.Deck, akash.TurnTaker.Trash);

            Card allies = PutOnDeck("AlliesOfTheEarth");

            DecisionDestroyCard = rockslide;
            DecisionDoNotSelectFunction = true;

            PlayCard("Doppelganger");
            AssertInTrash(rockslide);
            AssertOnTopOfDeck(allies);
        }

        [Test()]
        public void TestDoppelgangerAeon()
        {
            SetupGameController("OblivAeon", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios");
            StartGame();

            //Select a deck.
            //You may destroy a target with 4 or fewer hp from that deck.
            //Reveal the top 3 cards of that deck. You may play or discard one of the revealed cards. Replace the other cards in order.
            DecisionSelectLocation = new LocationChoice(aeonDeck);
            Card thrall = PlayCard("AeonThrall");
            //Card thrall = PlayCard(oblivaeon, GetCard("AeonThrall"), overridePlayLocation: scionTwo.TurnTaker.PlayArea);
            DecisionDestroyCard = thrall;
            Card warrior = MoveCard(oblivaeon, "AeonWarrior", aeonDeck);
            Card locus = MoveCard(oblivaeon, "AeonLocus", aeonDeck);
            Card vassal = MoveCard(oblivaeon, "AeonVassal", aeonDeck);

            DecisionSelectCard = locus;
            DecisionSelectFunction = 1;

            PlayCard("Doppelganger");
            AssertAtLocation(thrall,aeonTrash);
            AssertAtLocation(locus,aeonTrash);
            AssertOnTopOfLocation(vassal,aeonDeck);
            AssertOnTopOfLocation(warrior, aeonDeck, 1);
        }

        [Test()]
        public void TestDoppelgangerOblivaeon()
        {
            SetupGameController("OblivAeon", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios");
            StartGame();

            //Select a deck.
            //You may destroy a target with 4 or fewer hp from that deck.
            //Reveal the top 3 cards of that deck. You may play or discard one of the revealed cards. Replace the other cards in order.
            DecisionSelectLocation = new LocationChoice(oblivaeon.TurnTaker.Deck);
            Card thrall = PlayCard("AeonThrall");
            DecisionDestroyCard = thrall;
            Card focus = PutOnDeck("FocusOfPower");
            Card doom = PutOnDeck("ImpendingDoom");
            Card global = PutOnDeck("GlobalDevastation");

            DecisionSelectCard = focus;
            DecisionSelectFunction = 1;

            PlayCard("Doppelganger");
            AssertIsInPlay(thrall);
            AssertInTrash(focus);
            AssertOnTopOfDeck(global);
            AssertOnTopOfDeck(doom, 1);
        }

        [Test()]
        public void TestDoppelgangerScion()
        {
            SetupGameController("OblivAeon", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios");
            StartGame();

            //Select a deck.
            //You may destroy a target with 4 or fewer hp from that deck.
            //Reveal the top 3 cards of that deck. You may play or discard one of the revealed cards. Replace the other cards in order.
            DecisionSelectLocation = new LocationChoice(scionDeck);
            Card thrall = PlayCard("AeonThrall");
            DecisionDestroyCard = thrall;
            Card aeon = MoveCard(oblivaeon,"AeonAssault",scionDeck);
            Card reality = MoveCard(oblivaeon, "RealityAltered", scionDeck);
            Card certain = MoveCard(oblivaeon, "CertainFinality", scionDeck);

            DecisionSelectCard = reality;
            DecisionSelectFunction = 1;

            PlayCard("Doppelganger");
            AssertIsInPlay(thrall);
            AssertAtLocation(reality, scionTrash);
            AssertOnTopOfLocation(certain, scionDeck);
            AssertOnTopOfLocation(aeon, scionDeck, 1);
        }

        [Test()]
        public void TestDracimpR9A3ArmCannon()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When {Friday} would deal a type of damage that has been dealt by a source other than {Friday} since the end of your last turn, you may increase that damage by 1.
            //Power: Select a damage type, then repeat the following text 3 times: {Friday} deals 1 target 1 damage of that type. Reduce damage dealt to that target by 1 this turn.
            Card cannon = PlayCard("DracimpR9A3ArmCannon");
            GoToStartOfTurn(legacy);

            DecisionYesNo = true;

            //Check that melee damage is not increased
            QuickHPStorage(akash);
            DealDamage(friday, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that Friday dealing melee damage did not make it increased
            QuickHPStorage(akash);
            DealDamage(friday, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that Legacy dealing melee damage does make it increased
            DealDamage(legacy, akash, 1, DamageType.Melee);
            QuickHPStorage(akash);
            DealDamage(friday, akash, 1, DamageType.Melee);
            QuickHPCheck(-2);

            //Check that energy damage is still not increased
            QuickHPStorage(akash);
            DealDamage(friday, akash, 1, DamageType.Energy);
            QuickHPCheck(-1);

            //Check that it resets after Friday's next end of turn
            GoToStartOfTurn(legacy);
            QuickHPStorage(akash);
            DealDamage(friday, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestDracimpR9A3ArmCannonPower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When {Friday} would deal a type of damage that has been dealt by a source other than {Friday} since the end of your last turn, you may increase that damage by 1.
            //Power: Select a damage type, then repeat the following text 3 times: {Friday} deals 1 target 1 damage of that type. Reduce damage dealt to that target by 1 this turn.
            Card cannon = PlayCard("DracimpR9A3ArmCannon");
            Card rockslide = PlayCard("LivingRockslide");
            Card arboreal = PlayCard("ArborealPhalanges");
            DecisionSelectTargets = new Card[] { akash.CharacterCard, rockslide, arboreal };

            //Check that damage was dealt correctly
            QuickHPStorage(akash.CharacterCard, rockslide, arboreal);
            UsePower(cannon);
            QuickHPCheck(-1, -1, - 1);

            //Check that damage is reduced
            QuickHPStorage(akash.CharacterCard, rockslide, arboreal);
            DealDamage(legacy, akash, 2, DamageType.Melee);
            DealDamage(legacy, rockslide, 2, DamageType.Melee);
            DealDamage(legacy, arboreal, 2, DamageType.Melee);
            QuickHPCheck(-1, -1, -1);
        }

        [Test()]
        public void TestEnergyReplicationLightning()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Increase Lightning Damage dealt to {Friday} by 1.
            //Once per round, when another target deals damage, {Friday} may deal 1 target X damage of the same type, where X = the amount of damage dealt or 3, whichever is lower.
            Card energy = PlayCard("EnergyReplication");
            QuickHPStorage(friday);
            DealDamage(legacy, friday, 1, DamageType.Lightning);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestEnergyReplication()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Increase Lightning Damage dealt to {Friday} by 1.
            //Once per round, when another target deals damage, {Friday} may deal 1 target X damage of the same type, where X = the amount of damage dealt or 3, whichever is lower.
            Card energy = PlayCard("EnergyReplication");

            DecisionYesNo = true;
            DecisionSelectTarget = akash.CharacterCard;

            //Check that Friday dealing damage does not trigger it
            QuickHPStorage(akash);
            DealDamage(friday, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that Legacy dealing damage does trigger it
            QuickHPStorage(akash);
            DealDamage(legacy, akash, 1, DamageType.Melee);
            QuickHPCheck(-2);

            //Check that it does not trigger later in the same round
            GoToStartOfTurn(legacy);
            QuickHPStorage(akash);
            DealDamage(legacy, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that it resets on the next round, and deals 3 damage
            GoToStartOfTurn(akash);
            QuickHPStorage(akash);
            DealDamage(legacy, akash, 10, DamageType.Melee);
            QuickHPCheck(-13);
        }

        [Test()]
        public void TestEnergyReplicationIncap()
        {
            SetupGameController("AkashBhuta", "Ra", "Legacy", "Bunker", "VainFacadePlaytest.Friday", "InsulaPrimalis");
            StartGame();
            //When another target deals damage, Friday may deal 1 target X damage of the same type
            //Check that it works if the damage is a hero incapping themself
            Card energy = PlayCard("EnergyReplication");
            SetHitPoints(ra, 1);
            DecisionYesNo = true;
            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(akash);
            DealDamage(ra, ra, 2, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestICanDoBetter()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When a target deals {Friday} damage, {Friday} may deal the source of that damage X plus 2 damage of the same type, where X = the amount of damage dealt to {Friday}. If {Friday} deals damage this way, destroy this card.
            //Power: Play a mimicry or draw 2 cards.
            Card better = PlayCard("ICanDoBetter");
            DecisionYesNo = true;
            QuickHPStorage(akash);
            DealDamage(akash, friday, 1, DamageType.Melee);
            QuickHPCheck(-3);
            AssertInTrash(better);
        }

        [Test()]
        public void TestICanDoBetterFridayDamage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When a target deals {Friday} damage, {Friday} may deal the source of that damage X plus 2 damage of the same type, where X = the amount of damage dealt to {Friday}. If {Friday} deals damage this way, destroy this card.
            //Power: Play a mimicry or draw 2 cards.
            Card better = PlayCard("ICanDoBetter");
            DecisionYesNo = true;
            QuickHPStorage(friday);
            DealDamage(friday, friday, 1, DamageType.Melee);
            QuickHPCheck(-1);
            AssertIsInPlay(better);
        }

        [Test()]
        public void TestICanDoBetterSkip()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When a target deals {Friday} damage, {Friday} may deal the source of that damage X plus 2 damage of the same type, where X = the amount of damage dealt to {Friday}. If {Friday} deals damage this way, destroy this card.
            //Power: Play a mimicry or draw 2 cards.
            Card better = PlayCard("ICanDoBetter");
            DecisionYesNo = false;
            QuickHPStorage(akash);
            DealDamage(akash, friday, 1, DamageType.Melee);
            QuickHPCheck(0);
            AssertIsInPlay(better);
        }

        [Test()]
        public void TestICanDoBetterPower1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When a target deals {Friday} damage, {Friday} may deal the source of that damage X plus 2 damage of the same type, where X = the amount of damage dealt to {Friday}. If {Friday} deals damage this way, destroy this card.
            //Power: Play a mimicry or draw 2 cards.
            Card better = PlayCard("ICanDoBetter");
            DecisionSelectFunction = 0;
            Card grim = PutInHand("GrimReflection");
            DecisionSelectCardToPlay = grim;
            UsePower(better);
            AssertIsInPlay(grim);
            AssertIsInPlay(better);
        }

        [Test()]
        public void TestICanDoBetterPower2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When a target deals {Friday} damage, {Friday} may deal the source of that damage X plus 2 damage of the same type, where X = the amount of damage dealt to {Friday}. If {Friday} deals damage this way, destroy this card.
            //Power: Play a mimicry or draw 2 cards.
            Card better = PlayCard("ICanDoBetter");
            DecisionSelectFunction = 1;
            QuickHandStorage(friday);
            PutOnDeck("GrimReflection");
            PutOnDeck("DamselInDistress");
            UsePower(better);
            QuickHandCheck(2);
        }

        [Test()]
        public void TestProteanDoomPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Play this card next to a target. Damage dealt to that target by {Friday} is irreducible and increased by 1.
            //Power: Reveal the top 3 cards of your deck. Put one into your hand. Put the other revealed cards on the bottom of your deck in any order.
            DecisionSelectCard = akash.CharacterCard;
            Card protean = PlayCard("ProteanDoom");
            AssertAtLocation(protean, akash.CharacterCard.NextToLocation);
        }

        [Test()]
        public void TestProteanDoom()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Play this card next to a target. Damage dealt to that target by {Friday} is irreducible and increased by 1.
            //Power: Reveal the top 3 cards of your deck. Put one into your hand. Put the other revealed cards on the bottom of your deck in any order.
            DecisionSelectCard = akash.CharacterCard;
            Card protean = PlayCard("ProteanDoom");
            PlayCard("MountainousCarapace");

            //Check that damage from Friday is irreducible and increased
            QuickHPStorage(akash);
            DealDamage(friday, akash, 2, DamageType.Melee);
            QuickHPCheck(-3);

            //Check that damage from Legacy is not
            QuickHPStorage(akash);
            DealDamage(legacy, akash, 2, DamageType.Melee);
            QuickHPCheck(-1);
        }

        protected Card SelectFirstCard;
        protected Card SelectSecondCard;
        protected Card SelectThirdCard;
        protected int DecisionIndex = 1;
        protected int NumberOfCardsToSelect;

        protected IEnumerator NewDecisions(IDecision decision)
        {
            if (decision is SelectCardDecision && ((SelectCardDecision)decision).CardSource.Card.Identifier == "ProteanDoom")
            {
                if (DecisionIndex == 1)
                {
                    decision.SelectedCard = SelectFirstCard;
                    DecisionIndex = 2;
                }
                else if (DecisionIndex == 2)
                {
                    if (NumberOfCardsToSelect == 1)
                    {
                        ((SelectCardDecision)decision).FinishedSelecting = true;
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
            else
            {
                RunCoroutine(base.MakeDecisions(decision));
            }

            yield return null;
        }

        [Test()]
        public void TestProteanDoomPower1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Play this card next to a target. Damage dealt to that target by {Friday} is irreducible and increased by 1.
            //Power: Reveal the top 3 cards of your deck. Put one into your hand. Put the other revealed cards on the bottom of your deck in any order.
            DecisionSelectCard = akash.CharacterCard;
            Card protean = PlayCard("ProteanDoom");

            this.GameController.OnMakeDecisions -= MakeDecisions;
            this.GameController.OnMakeDecisions += NewDecisions;

            Card grim = PutOnDeck("GrimReflection");
            Card damsel = PutOnDeck("DamselInDistress");
            Card energy = PutOnDeck("EnergyReplication");

            SelectFirstCard = damsel;
            SelectSecondCard = grim;
            SelectThirdCard = energy;

            UsePower(protean);
            AssertInHand(damsel);
            AssertOnBottomOfDeck(grim);
            AssertOnBottomOfDeck(energy, 1);
        }

        [Test()]
        public void TestProteanDoomPower2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Play this card next to a target. Damage dealt to that target by {Friday} is irreducible and increased by 1.
            //Power: Reveal the top 3 cards of your deck. Put one into your hand. Put the other revealed cards on the bottom of your deck in any order.
            DecisionSelectCard = akash.CharacterCard;
            Card protean = PlayCard("ProteanDoom");

            //this.GameController.OnMakeDecisions -= MakeDecisions;
            //this.GameController.OnMakeDecisions += NewDecisions;

            Card grim = PutOnDeck("GrimReflection");
            Card damsel = PutOnDeck("DamselInDistress");
            Card energy = PutOnDeck("EnergyReplication");

            //SelectFirstCard = damsel;
            //SelectSecondCard = energy;
            //SelectThirdCard = grim;

            ResetDecisions();

            UsePower(protean);
            AssertInHand(energy);
            AssertOnBottomOfDeck(damsel);
            AssertOnBottomOfDeck(grim, 1);
        }

        [Test()]
        public void TestProteanDoomPowerOneCard()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Play this card next to a target. Damage dealt to that target by {Friday} is irreducible and increased by 1.
            //Power: Reveal the top 3 cards of your deck. Put one into your hand. Put the other revealed cards on the bottom of your deck in any order.
            DecisionSelectCard = akash.CharacterCard;
            Card protean = PlayCard("ProteanDoom");
            DecisionSelectCard = null;

            MoveAllCards(friday, friday.TurnTaker.Deck, friday.TurnTaker.Trash);
            MoveAllCards(friday, friday.HeroTurnTaker.Hand, friday.TurnTaker.Trash);
            Card grim = PutOnDeck("GrimReflection");
            UsePower("ProteanDoom");
            AssertInHand(grim);
        }

        [Test()]
        public void TestPrototypeCombatMimic()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Once per round, at the end of another hero's turn, {Friday} may deal that hero 2 lightning damage.
            //When a hero other than {Friday} is dealt damage this way, {Friday} may use a power in that hero's play area, replacing the name of that hero on that card with {Friday} and with “You” on that card referring to {Friday}'s player.
            Card proto = PlayCard("PrototypeCombatMimic");
            DecisionYesNo = true;

            //Use Legacy's power
            QuickHPStorage(legacy);
            GoToEndOfTurn(legacy);
            QuickHPCheck(-2);

            //Check that Galvanize is in effect
            QuickHPStorage(akash);
            DealDamage(friday, akash, 1, DamageType.Melee);
            QuickHPCheck(-2);

            //Check that it does not activate on Bunker's turn
            QuickHPStorage(bunker);
            GoToEndOfTurn(bunker);
            QuickHPCheck(0);

            //Check that it activates on Bunker's turn next round
            DecisionYesNo = false;
            GoToStartOfTurn(bunker);
            QuickHPStorage(bunker);
            DecisionYesNo = true;
            PutOnDeck("ProteanDoom");
            QuickHandStorage(friday);
            GoToEndOfTurn(bunker);
            QuickHPCheck(-2);
            QuickHandCheck(1);
        }

        [Test()]
        public void TestPrototypeCombatMimicLink()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Bunker", "Legacy", "SkyScraper", "Megalopolis");
            StartGame();

            //Once per round, at the end of another hero's turn, {Friday} may deal that hero 2 lightning damage.
            //When a hero other than {Friday} is dealt damage this way, {Friday} may use a power in that hero's play area, replacing the name of that hero on that card with {Friday} and with “You” on that card referring to {Friday}'s player.
            Card proto = PlayCard("PrototypeCombatMimic");
            DecisionSelectCard = bunker.CharacterCard;
            Card link = PlayCard("MicroAssembler");
            ResetDecisions();

            //Check whether Prototype Combat Mimic lets Friday use the power granted from Micro Assembler
            DecisionYesNo = true;
            DecisionPowerIndex = 1;
            GoToEndOfTurn(bunker);
            AssertNumberOfCardsInPlay(friday, 3);
        }

        [Test()]
        public void TestRuthlessMachine()
        {
            SetupGameController("Kismet", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When a card from another deck would cause {Friday} to deal herself damage, you may discard a card from your hand to prevent that damage.
            //Power: {Friday} deals 1 Target 2 melee or 2 projectile damage. Draw a card.
            DecisionSelectTurnTaker = friday.TurnTaker;
            Card ruthless = PlayCard("RuthlessMachine");
            PlayCard("DangerSense");
            PlayCard("UpgradeMode");
            PlayCard("BringWhatYouNeed");
            Card leftFeet = PlayCard("TwoLeftFeet");
            Card grim = PutInHand("GrimReflection");
            Card vessel = PutInHand("VesselOfDestruction");
            DecisionSelectCard = grim;

            DecisionYesNo = true;
            QuickHandStorage(friday);
            QuickHPStorage(friday);
            PlayCardFromHand(friday, "VesselOfDestruction");
            QuickHandCheck(-2);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestRuthlessMachineSelf()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When a card from another deck would cause {Friday} to deal herself damage, you may discard a card from your hand to prevent that damage.
            //Power: {Friday} deals 1 Target 2 melee or 2 projectile damage. Draw a card.
            Card ruthless = PlayCard("RuthlessMachine");
            DecisionYesNo = true;
            PutOnDeck("GrimReflection");
            PutOnDeck("TechnologicalDuplication");

            //Check that Ruthless Machine does not prevent damage from Friday's cards
            QuickHPStorage(friday);
            PlayCard("UnstableCircuitry");
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestRuthlessMachinePower()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When a card from another deck would cause {Friday} to deal herself damage, you may discard a card from your hand to prevent that damage.
            //Power: {Friday} deals 1 Target 2 melee or 2 projectile damage. Draw a card.
            Card ruthless = PlayCard("RuthlessMachine");
            PutOnDeck("GrimReflection");
            QuickHPStorage(akash);
            QuickHandStorage(friday);
            UsePower(ruthless);
            QuickHPCheck(-2);
            QuickHandCheck(1);
        }

        [Test()]
        public void TestT913ExdimReaperArmorReduction()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Reduce damage dealt to {Friday} by 1.
            //Once per turn when {Friday} deals damage, she may deal 1 damage of the same type to the same target.
            //At the end of your turn, you may discard a card. If you do not, {Friday} deals each other target 1 psychic damage.
            Card armor = PlayCard("T913ExdimReaperArmor");
            QuickHPStorage(friday);
            DealDamage(akash, friday, 2, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestT913ExdimReaperArmorDamage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Reduce damage dealt to {Friday} by 1.
            //Once per turn when {Friday} deals damage, she may deal 1 damage of the same type to the same target.
            //At the end of your turn, you may discard a card. If you do not, {Friday} deals each other target 1 psychic damage.
            Card armor = PlayCard("T913ExdimReaperArmor");
            DecisionYesNo = true;
            GoToStartOfTurn(friday);

            //Check that it does not activate for Legacy
            QuickHPStorage(akash);
            DealDamage(legacy, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);

            //Check that it activates
            QuickHPStorage(akash);
            DealDamage(friday, akash, 1, DamageType.Melee);
            QuickHPCheck(-2);

            //Check that it does not activate a second time
            QuickHPStorage(akash);
            DealDamage(friday, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);

            DecisionYesNo = false;
            GoToStartOfTurn(legacy);
            DecisionYesNo = true;

            //Check that it activates again on the next turn
            QuickHPStorage(akash);
            DealDamage(friday, akash, 1, DamageType.Melee);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestT913ExdimReaperArmorDiscard()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Reduce damage dealt to {Friday} by 1.
            //Once per turn when {Friday} deals damage, she may deal 1 damage of the same type to the same target.
            //At the end of your turn, you may discard a card. If you do not, {Friday} deals each other target 1 psychic damage.
            Card armor = PlayCard("T913ExdimReaperArmor");
            Card grim = PutInHand("GrimReflection");
            DecisionSelectCard = grim;
            QuickHPStorage(legacy, bunker, scholar, akash);
            GoToEndOfTurn(friday);
            AssertInTrash(grim);
            QuickHPCheck(0, 0, 0, 0);
        }

        [Test()]
        public void TestT913ExdimReaperArmorNoDiscard()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //Reduce damage dealt to {Friday} by 1.
            //Once per turn when {Friday} deals damage, she may deal 1 damage of the same type to the same target.
            //At the end of your turn, you may discard a card. If you do not, {Friday} deals each other target 1 psychic damage.
            Card armor = PlayCard("T913ExdimReaperArmor");
            DecisionDoNotSelectCard = SelectionType.DiscardCard;
            QuickHPStorage(legacy, bunker, scholar, akash);
            GoToEndOfTurn(friday);
            QuickHPCheck(-1, -1, -1, -1);
        }

        [Test()]
        public void TestUnstableCircuitryNoDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //When this card enters your hand, put it into play.
            //{Friday} deals herself 1 irreducible lightning damage.
            //You may draw 2 cards. If you do, shuffle this card into your deck.

            DecisionYesNo = false;
            PlayCard("T913ExdimReaperArmor");
            QuickHPStorage(friday);
            QuickHandStorage(friday);
            QuickShuffleStorage(friday);
            Card unstable = PutInHand("UnstableCircuitry");
            QuickHPCheck(-1);
            AssertInTrash(unstable);
            QuickHandCheck(0);
            QuickShuffleCheck(0);
        }

        [Test()]
        public void TestUnstableCircuitryDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When this card enters your hand, put it into play.
            //{Friday} deals herself 1 irreducible lightning damage.
            //You may draw 2 cards. If you do, shuffle this card into your deck.

            DecisionYesNo = true;
            PlayCard("T913ExdimReaperArmor");
            QuickHPStorage(friday);
            QuickHandStorage(friday);
            QuickShuffleStorage(friday);

            //Put all Unstable Circuitry cards in the trash so they don't get drawn and mess up the hand check
            PutInTrash(FindCardsWhere((Card c) => c.Identifier == "UnstableCircuitry"));

            Card unstable = PutInHand("UnstableCircuitry");
            QuickHPCheck(-1);
            AssertInDeck(unstable);
            QuickHandCheck(2);
            QuickShuffleCheck(1);
        }

        [Test()]
        public void TestVesselOfDestructionDestroyDamage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When this card is destroyed, {Friday} deals 1 target 1 irreducible fire damage or destroys 1 environment or ongoing card.
            //Power: Select a hero. Destroy any number of their ongoing or equipment cards. For each card destroyed this way, {Friday} deals 1 target 1 irreducible fire damage or destroys an ongoing or environment card. Destroy this card.
            Card vessel = PlayCard("VesselOfDestruction");

            //Test damage when Vessel of Destruction is destroyed
            PlayCard("MountainousCarapace");
            DecisionSelectTarget = akash.CharacterCard;
            DecisionSelectFunction = 0;
            QuickHPStorage(akash);
            DestroyCard(vessel);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestVesselOfDestructionDestroyOngoing()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When this card is destroyed, {Friday} deals 1 target 1 irreducible fire damage or destroys 1 environment or ongoing card.
            //Power: Select a hero. Destroy any number of their ongoing or equipment cards. For each card destroyed this way, {Friday} deals 1 target 1 irreducible fire damage or destroys an ongoing or environment card. Destroy this card.
            Card vessel = PlayCard("VesselOfDestruction");

            //Test ongoing destruction when Vessel of Destruction is destroyed
            Card allies = PlayCard("AlliesOfTheEarth");
            DecisionSelectCard = allies;
            DecisionSelectFunction = 1;
            DestroyCard(vessel);
            AssertInTrash(allies);
        }

        [Test()]
        public void TestVesselOfDestructionDestroyEnvironment()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When this card is destroyed, {Friday} deals 1 target 1 irreducible fire damage or destroys 1 environment or ongoing card.
            //Power: Select a hero. Destroy any number of their ongoing or equipment cards. For each card destroyed this way, {Friday} deals 1 target 1 irreducible fire damage or destroys an ongoing or environment card. Destroy this card.
            Card vessel = PlayCard("VesselOfDestruction");

            //Test environment destruction when Vessel of Destruction is destroyed
            Card obsidian = PlayCard("ObsidianField");
            DecisionSelectCard = obsidian;
            DecisionSelectFunction = 1;
            DestroyCard(vessel);
            AssertInTrash(obsidian);
        }

        [Test()]
        public void TestVesselOfDestructionPowerDamage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            //When this card is destroyed, {Friday} deals 1 target 1 irreducible fire damage or destroys 1 environment or ongoing card.
            //Power: Select a hero. Destroy any number of their ongoing or equipment cards. For each card destroyed this way, {Friday} deals 1 target 1 irreducible fire damage or destroys an ongoing or environment card. Destroy this card.
            Card vessel = PlayCard("VesselOfDestruction");

            //Test Vessel of Destruction power
            Card fortitude = PlayCard("Fortitude");
            Card ring = PlayCard("TheLegacyRing");
            Card flak = PlayCard("FlakCannon");

            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionSelectFunction = 0;
            DecisionSelectTarget = akash.CharacterCard;
            PlayCard("MountainousCarapace");

            QuickHPStorage(akash);
            UsePower(vessel);
            QuickHPCheck(-3);
            AssertInTrash(fortitude);
            AssertInTrash(ring);
            AssertIsInPlay(flak);
        }

        //[Test()]
        //public void TestMercenarySquad()
        //{
        //    SetupGameController("AkashBhuta", "VainFacadePlaytest.Friday", "Legacy", "Bunker", "TheScholar", "VainFacadePlaytest.ParadiseIsle");
        //    StartGame();

        //    //Test how Mercenary Squad interacts with Arm Cannon
        //    Card mercenary = PlayCard("MercenarySquad");
        //    Card cannon = PlayCard("DracimpR9A3ArmCannon");
        //    DecisionSelectTarget = mercenary;

        //    GoToEndOfTurn(legacy);
        //    DealDamage(legacy, akash, 1, DamageType.Melee);
        //    DecisionSelectDamageType = DamageType.Melee;
        //    UsePower(cannon);
        //}

        //[Test()]
        //public void TestUnstableCircuitryOblivAeon()
        //{
        //    SetupGameController("OblivAeon", "Ra", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios");
        //    StartGame();

        //    //When this card enters your hand, put it into play.
        //    //{Friday} deals herself 1 irreducible lightning damage.
        //    //You may draw 2 cards. If you do, shuffle this card into your deck.
        //    DecisionSelectFromBoxIdentifiers = new string[] { "Friday" };
        //    DecisionSelectFromBoxTurnTakerIdentifier = "VainFacadePlaytest.Friday";
        //    DestroyCard(ra);
        //    GoToAfterEndOfTurn(oblivaeon);
        //    RunActiveTurnPhase();

        //    GoToDrawCardPhase(friday);

        //    //SaveAndLoad(GameController);

        //    Card unstable = PutOnDeck("UnstableCircuitry");
        //    QuickHPStorage(friday);
        //    DrawCard(friday);
        //    QuickHPCheck(-1);
        //    AssertInTrash(unstable);

        //}

        //[Test()]
        //public void TestMonsterOfIdOblivAeon()
        //{
        //    SetupGameController("OblivAeon", "Ra", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios");
        //    StartGame();

        //    DecisionSelectFromBoxIdentifiers = new string[] { "VoidGuardTheIdealist" };
        //    DecisionSelectFromBoxTurnTakerIdentifier = "VoidGuardTheIdealist";
        //    DestroyCard(ra);
        //    GoToAfterEndOfTurn(oblivaeon);
        //    RunActiveTurnPhase();

        //    GoToDrawCardPhase(voidIdealist);

        //    Card id = PutOnDeck("MonsterOfId");
        //    QuickHPStorage(voidIdealist);
        //    DrawCard(voidIdealist);
        //    AssertIsInPlay(id);
        //}

        //[Test()]
        //public void TestShinobiOblivAeon()
        //{
        //    SetupGameController("OblivAeon", "Ra", "Legacy", "SkyScraper", "TheScholar", "Megalopolis", "InsulaPrimalis", "TheTempleOfZhuLong", "RuinsOfAtlantis", "NexusOfTheVoid");
        //    StartGame();

        //    Card inevitable = GetCard("InevitableDestruction");
        //    MoveCards(oblivaeon, FindCardsWhere((Card c) => c.Identifier == "AeonThrall"), oblivaeon.TurnTaker.FindSubDeck("AeonMenDeck"));

        //    RemoveTokensFromPool(inevitable.FindTokenPool("CountdownTokenPool"),6); 
        //    Card shinobi = GetCard("ShinobiAssassin");
        //    AssertInDeck(FindEnvironment(bzOne),shinobi);

        //    SetHitPoints(new TurnTakerController[] {legacy, sky, scholar }, 20);
        //    SetHitPoints(ra, 10);
        //    PlayCard(shinobi);
        //    DestroyCard(shinobi);
        //    DestroyCards(FindCardsWhere((Card c) => c.IsAeonMan));

        //    QuickHPStorage(ra);
        //    DrawCard(ra);
        //    Console.WriteLine($"{shinobi.Title} is in {shinobi.Location.GetFriendlyName()}");
        //    AssertIsInPlay(shinobi);
        //    QuickHPCheck(-3);
        //}
    }
}