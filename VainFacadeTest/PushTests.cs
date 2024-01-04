using NUnit.Framework;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.UnitTest;
using System.Linq;
using VainFacadePlaytest.Push;

namespace VainFacadeTest
{
    [TestFixture()]
    public class PushTests:BaseTest
	{
        protected HeroTurnTakerController push { get { return FindHero("Push"); } }

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(push.CharacterCard, 1);
            DealDamage(villain, push, 2, DamageType.Melee, true);
        }

        [Test()]
        public void TestLoadPush()
        {
            SetupGameController("BaronBlade", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(push);
            Assert.IsInstanceOf(typeof(PushCharacterCardController), push.CharacterCardController);

            Assert.AreEqual(26, push.CharacterCard.HitPoints);
        }

        //[Test()]
        //public void TestCollateralBonus()
        //{
        //    SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
        //    StartGame();
        //    Card obsidian = PlayCard("ObsidianField");
        //    Card bonus = PlayCard("CollateralBonus");
        //    Card analyzer = PutInHand("PhaseAnalyzer");
        //    DecisionSelectCards = new Card[] { obsidian, analyzer };
        //    QuickHPStorage(akash);
        //    DealDamage(push, akash, 1, DamageType.Melee);
        //    QuickHPCheck(-3);
        //}

        //[Test()]
        //public void TestCollateralBonus2()
        //{
        //    SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "TheBlock");
        //    StartGame();
        //    Card defense = PlayCard("DefensiveDisplacement");
        //    Card bonus = PlayCard("CollateralBonus");
        //    Card analyzer = PutInHand("PhaseAnalyzer");
        //    DecisionSelectCards = new Card[] { defense, analyzer };
        //    QuickHPStorage(akash);
        //    DealDamage(push, akash, 1, DamageType.Melee);
        //    QuickHPCheck(-3);
        //}

        [Test()]
        public void TestInnatePowerDiscard()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //You may discard a card or destroy an alteration.
            //Draw up to 2 cards.
            DecisionSelectFunction = 0;
            Card phase = PutInHand("PhaseAnalyzer");
            DecisionSelectCard = phase;
            QuickHandStorage(push);
            UsePower(push);
            QuickHandCheck(1);
            AssertInTrash(phase);
        }

        [Test()]
        public void TestInnatePowerDestroy()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //You may discard a card or destroy an alteration.
            //Draw up to 2 cards.
            DecisionSelectFunction = 1;
            Card bogged = PlayCard("BoggedDown");
            DecisionSelectCard = bogged;
            QuickHandStorage(push);
            UsePower(push);
            QuickHandCheck(2);
            AssertInTrash(bogged);
        }

        [Test()]
        public void TestIncap1Discard()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //1 player may discard a card. If a card is discarded this way, one of that player's heroes may deal 1 target 2 projectile damage.
            SetupIncap(akash);
            Card fortitude = PutInHand("Fortitude");
            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionDiscardCard = fortitude;
            DecisionSelectTarget = akash.CharacterCard;

            QuickHPStorage(akash);
            UseIncapacitatedAbility(push, 0);
            QuickHPCheck(-2);
            AssertInTrash(fortitude);
        }

        [Test()]
        public void TestIncap1NoDiscard()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //1 player may discard a card. If a card is discarded this way, one of that player's heroes may deal 1 target 2 projectile damage.
            SetupIncap(akash);

            Card fortitude = PutInHand("Fortitude");

            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionSelectTarget = akash.CharacterCard;
            DecisionDoNotSelectCard = SelectionType.DiscardCard;

            QuickHPStorage(akash);
            UseIncapacitatedAbility(push, 0);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestIncap2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //1 player may draw a card.
            SetupIncap(akash);
            DecisionSelectTurnTaker = legacy.TurnTaker;
            QuickHandStorage(legacy);
            UseIncapacitatedAbility(push, 1);
            QuickHandCheck(1);
        }

        [Test()]
        public void TestIncap3()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Destroy an environment card.
            SetupIncap(akash);
            Card obsidian = PlayCard("ObsidianField");
            DecisionSelectCard = obsidian;
            UseIncapacitatedAbility(push, 2);
            AssertInTrash(obsidian);
        }

        [Test()]
        public void TestBoggedDown()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //When {Push} deals a target damage, reduce the next damage dealt by that target by 2.
            Card bogged = PlayCard("BoggedDown");

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-3);

            DealDamage(push, akash, 1, DamageType.Melee);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-1);

            QuickHPStorage(legacy);
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-3);

            GoToStartOfTurn(push);
            AssertInTrash(bogged);
        }

        [Test()]
        public void TestCollateralBonus()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            //When Push deals damage, destroy an environment card. If a card is destroyed this way, discard a card and the environment deals the same target 2 irreducible projectile damage.
            Card bonus = PlayCard("CollateralBonus");
            Card raptor = PutOnDeck("VelociraptorPack");
            Card volcano = PlayCard("VolcanicEruption");
            Card carapace = PlayCard("MountainousCarapace");
            MoveAllCards(push, push.HeroTurnTaker.Hand, push.TurnTaker.Trash);
            Card phase = PutInHand("PhaseAnalyzer");
            QuickHPStorage(akash);
            DealDamage(push, akash, 2, DamageType.Melee);
            QuickHPCheck(-3);
            AssertInTrash(volcano);
            AssertInTrash(phase);

            PutInHand(phase);
            QuickHPStorage(akash);
            DealDamage(push, akash, 2, DamageType.Melee);
            QuickHPCheck(-1);
            AssertInHand(phase);
        }

        [Test()]
        public void TestFeedbackBounce()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //When {Push} deals damage, you may discard 2 cards. If 2 cards are discarded this way, destroy this card or put an ongoing from in play under this card. Otherwise put another hero's ongoing from in play under this card.
            //When this card is destroyed, {Push} deals X targets 4 projectile damage each, where X = the number of cards under this one.
            //At the start of your turn, destroy this card.
            Card feedback = PlayCard("FeedbackBounce");
            Card fortitude = PlayCard("Fortitude");
            Card rockslide = PlayCard("LivingRockslide");
            Card allies = PlayCard("AlliesOfTheEarth");
            Card arboreal = PlayCard("ArborealPhalanges");

            Card bonus = PutInHand("CollateralBonus");
            Card shockwave = PutInHand("Shockwave");
            Card bogged = PutInHand("BoggedDown");
            Card folding = PutInHand("FoldingForces");

            //Discard to put Allies of the Earth under Feedback Bounce
            DecisionSelectCards = new Card[]{bonus, shockwave, allies};
            DecisionYesNo = true;
            DecisionSelectFunction = 1;
            QuickHandStorage(push);
            DealDamage(push, akash, 1, DamageType.Melee);
            AssertAtLocation(allies, feedback.UnderLocation);
            AssertInTrash(bonus);
            AssertInTrash(shockwave);
            QuickHandCheck(-2);

            //Do not discard, and put Fortitude under Feedback Bounce
            DecisionSelectCard = fortitude;
            DecisionYesNo = false;
            QuickHandStorage(push);
            DealDamage(push, akash, 1, DamageType.Melee);
            AssertAtLocation(fortitude, feedback.UnderLocation);
            QuickHandCheck(0);

            //Discard to destroy Feedback Bounce
            ResetDecisions();
            //DecisionSelectCards = new Card[] { bogged, folding };
            QuickHandStorage(push);
            DecisionYesNo = true;
            DecisionSelectFunction = 0;
            DecisionSelectTargets = new Card[] { akash.CharacterCard, rockslide, arboreal };
            QuickHPStorage(akash.CharacterCard, rockslide, arboreal);
            DealDamage(push, legacy, 1, DamageType.Melee);
            QuickHandCheck(-2);
            //AssertInTrash(bogged);
            //AssertInTrash(folding);
            AssertInTrash(feedback);
            AssertInTrash(allies);
            AssertInTrash(fortitude);
            QuickHPCheck(-4, -4, 0);
        }

        [Test()]
        public void TestFeedbackBounceStartOfTurn()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //When {Push} deals damage, you may discard 2 cards. If 2 cards are discarded this way, destroy this card or put an ongoing from in play under this card. Otherwise put another hero's ongoing from in play under this card.
            //When this card is destroyed, {Push} deals X targets 4 projectile damage each, where X = the number of cards under this one.
            //At the start of your turn, destroy this card.

            Card feedback = PlayCard("FeedbackBounce");
            GoToStartOfTurn(push);
            AssertInTrash(feedback);
        }

        [Test()]
        public void TestFieldCascade()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //When {Push} would deal damage to a target, prevent that damage and destroy this card.
            //When this card is destroyed, for each target other than {Push}, discard 2 cards or {Push} deals that target X projectile damage, where X = the damage {Push} has prevented this turn.
            //At the start of your turn, destroy this card.
            Card field = PlayCard("ObsidianField");
            Card cascade = PlayCard("FieldCascade");
            DecisionSelectFunction = 1;
            QuickHPStorage(akash,legacy,bunker,scholar);
            DealDamage(push, akash, 1, DamageType.Melee);
            QuickHPCheck(-3,-3,-3,-3);
            AssertInTrash(cascade);
        }

        [Test()]
        public void TestFieldCascadeStartOfTurn()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //When {Push} would deal damage to a target, prevent that damage and destroy this card.
            //When this card is destroyed, for each target other than {Push}, discard 2 cards or {Push} deals that target X projectile damage, where X = the damage {Push} has prevented this turn.
            //At the start of your turn, destroy this card.
            Card cascade = PlayCard("FieldCascade");
            GoToStartOfTurn(push);
            AssertInTrash(cascade);
        }

        [Test()]
        public void TestFieldCascadeOtherPrevent()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //When {Push} would deal damage to a target, prevent that damage and destroy this card.
            //When this card is destroyed, for each target other than {Push}, discard 2 cards or {Push} deals that target X projectile damage, where X = the damage {Push} has prevented this turn.
            //At the start of your turn, destroy this card.
            MoveAllCards(push, push.HeroTurnTaker.Hand, push.TurnTaker.Deck);
            Card cascade = PlayCard("FieldCascade");
            DealDamage(push, akash, 2,DamageType.Melee);

            //Card cascade2 = GetCard("FieldCascade", 0, (Card c) => c.Location == push.TurnTaker.Deck);
            PlayCard("FieldCascade");

            QuickHPStorage(akash, push, legacy, bunker, scholar);
            DealDamage(push, akash, 3, DamageType.Melee);
            QuickHPCheck(-5, 0, -5, -5, -5);
        }

        [Test()]
        public void TestFoldingForces()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Increase projectile damage dealt by {Push} by 2.
            //At the start of your turn, destroy this card.
            Card folding = PlayCard("FoldingForces");

            QuickHPStorage(akash);
            DealDamage(push, akash, 1, DamageType.Projectile);
            QuickHPCheck(-3);

            QuickHPStorage(akash);
            DealDamage(push, akash, 1, DamageType.Melee);
            QuickHPCheck(-1);

            GoToStartOfTurn(push);
            AssertInTrash(folding);
        }

        [Test()]
        public void TestForcePulse()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Whenever an alteration card is in your hand, put it into play.
            //At the start of your turn, {Push} deals 1 target 5 projectile damage. Then destroy this card.
            MoveAllCards(push, push.HeroTurnTaker.Hand, push.TurnTaker.Trash);
            Card force = PlayCard("ForcePulse");
            DecisionSelectTarget = akash.CharacterCard;

            QuickHPStorage(akash);
            GoToStartOfTurn(push);
            QuickHPCheck(-5);
            AssertInTrash(force);
        }

        [Test()]
        public void TestAnchorPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            //Whenever an alteration card is in your hand, put it into play.
            //At the start of your turn, {Push} deals 1 target 5 projectile damage. Then destroy this card.
            MoveAllCards(push, push.HeroTurnTaker.Hand, push.TurnTaker.Trash);
            PlayCard("HostageSituation");

            //Alteration already in hand should be played
            Card bogged = PutInHand("BoggedDown");
            Card force = PlayCard("ForcePulse",0,true);
            AssertIsInPlay(bogged);

            //Alteration should be played when moved to hand
            Card folding = PutInHand("FoldingForces");
            AssertIsInPlay(folding);

            //Alteration should be played when drawn
            Card feedback = PutOnDeck("FeedbackBounce");
            DrawCard(push);
            AssertIsInPlay(feedback);
        }

        [Test()]
        public void TestHarmonicJunction()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //You may play an anchor. If you do, play all alterations in your hand.
            //Activate start of turn affects on all anchors, then destroy all alterations and anchors.
            MoveAllCards(push, push.HeroTurnTaker.Hand, push.TurnTaker.Trash);
            Card force = PutInHand("ForcePulse");
            Card bogged = PutInHand("BoggedDown");
            Card folding = PutInHand("FoldingForces");

            DecisionSelectTarget = akash.CharacterCard;
            QuickHPStorage(akash,legacy);
            Card harmonic = PlayCard("HarmonicJunction");

            //Check that the start of turn efefct for Force Pulse went off, and Bogged Down and Folding Forces both took effect
            DealDamage(akash, legacy, 3, DamageType.Melee);
            QuickHPCheck(-7, -1);
            AssertInTrash(new Card[] { force, bogged, folding, harmonic });
        }

        [Test()]
        public void TestKineticAbsorptionDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //When {Push} is dealt melee or projectile damage, draw a card.
            //Power: Reveal the top 3 cards of your deck. You may put 1 revealed ongoing into your hand or into play. Discard the remaining cards. If an ongoing entered play this way, destroy this card.
            Card kinetic = PlayCard("KineticAbsorption");

            QuickHandStorage(push);
            DealDamage(akash, push, 1, DamageType.Melee);
            QuickHandCheck(1);

            QuickHandStorage(push);
            DealDamage(akash, push, 1, DamageType.Projectile);
            QuickHandCheck(1);

            QuickHandStorage(push);
            DealDamage(akash, push, 1, DamageType.Energy);
            QuickHandCheck(0);
        }

        [Test()]
        public void TestKineticAbsorptionPowerHand()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //When {Push} is dealt melee or projectile damage, draw a card.
            //Power: Reveal the top 3 cards of your deck. You may put 1 revealed ongoing into your hand or into play. Discard the remaining cards. If an ongoing entered play this way, destroy this card.
            MoveAllCards(push, push.HeroTurnTaker.Hand, push.TurnTaker.Trash);
            Card kinetic = PlayCard("KineticAbsorption");

            Card folding = PutOnDeck("FoldingForces");
            Card harmonic = PutOnDeck("HarmonicJunction");
            Card phase = PutOnDeck("PhaseAnalyzer");

            UsePower(kinetic);
            AssertInHand(folding);
            AssertInTrash(harmonic);
            AssertInTrash(phase);
            AssertIsInPlay(kinetic);
        }

        [Test()]
        public void TestKineticAbsorptionPowerPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //When {Push} is dealt melee or projectile damage, draw a card.
            //Power: Reveal the top 3 cards of your deck. You may put 1 revealed ongoing into your hand or into play. Discard the remaining cards. If an ongoing entered play this way, destroy this card.
            MoveAllCards(push, push.HeroTurnTaker.Hand, push.TurnTaker.Trash);
            Card kinetic = PlayCard("KineticAbsorption");

            Card folding = PutOnDeck("FoldingForces");
            Card harmonic = PutOnDeck("HarmonicJunction");
            Card phase = PutOnDeck("PhaseAnalyzer");

            DecisionMoveCardDestination = new MoveCardDestination(push.TurnTaker.PlayArea);

            UsePower(kinetic);
            AssertIsInPlay(folding);
            AssertInTrash(harmonic);
            AssertInTrash(phase);
            AssertInTrash(kinetic);
        }

        [Test()]
        public void TestNewtonsCradle()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //The first time {Push} deals damage each turn, {Push} deals that same damage to 2 targets that have not been dealt damage this turn.
            //At the start of your turn, destroy this card.
            Card newton = PlayCard("NewtonsCradle");
            Card arboreal = PlayCard("ArborealPhalanges");
            Card rockslide = PlayCard("LivingRockslide");
            DealDamage(akash, legacy, 1, DamageType.Melee);

            AssertNextDecisionChoices(new Card[] { push.CharacterCard, bunker.CharacterCard, scholar.CharacterCard, rockslide, arboreal });
            DecisionSelectTargets = new Card[] { rockslide, arboreal };
            QuickHPStorage(rockslide, arboreal);
            DealDamage(push, akash, 2, DamageType.Melee);
            QuickHPCheck(-2, -2);

            DecisionSelectTargets = new Card[] { bunker.CharacterCard, scholar.CharacterCard };
            QuickHPStorage(bunker, scholar);
            DealDamage(push, akash, 2, DamageType.Melee);
            QuickHPCheck(0, 0);

            ResetDecisions();
            GoToStartOfTurn(push);
            AssertInTrash(newton);
        }

        [Test()]
        public void TestObjectsInMotion()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //When you discard a card from your hand, put it under this card.
            //At the end of your turn, discard all cards under this card. {Push} deals 1 target 2 projectile damage for each card discarded this way.
            Card objects = PlayCard("ObjectsInMotion");
            MoveAllCards(push, push.HeroTurnTaker.Hand, push.TurnTaker.Trash);
            Card folding = PutInHand("FoldingForces");
            Card bogged = PutInHand("BoggedDown");
            DiscardAllCards(push);
            AssertAtLocation(new Card[] { folding, bogged }, objects.UnderLocation);
            AssertNumberOfCardsAtLocation(objects.UnderLocation, 2);

            DecisionSelectTargets = new Card[] { akash.CharacterCard, legacy.CharacterCard };
            QuickHPStorage(akash, legacy);
            GoToEndOfTurn(push);
            QuickHPCheck(-2, -2);
            AssertInTrash(folding);
            AssertInTrash(bogged);
        }

        [Test()]
        public void TestPathOfDestruction()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Whenever an alteration card is in your hand, put it into play.
            //At the start of your turn, {Push} deals 3 targets 3 irreducible projectile damage each. Then you may destroy a hero ongoing or equipment belonging to another player. If no card is destroyed this way, {Push} deals each target other than {Push} 0 projectile damage. Then destroy this card.
            MoveAllCards(push, push.HeroTurnTaker.Hand, push.TurnTaker.Trash);
            Card path = PlayCard("PathOfDestruction");
            Card carapace = PlayCard("MountainousCarapace");
            Card fortitude = PlayCard("Fortitude");
            Card folding = PlayCard("FoldingForces");

            DecisionSelectTargets = new Card[] { akash.CharacterCard, carapace, legacy.CharacterCard };
            QuickHPStorage(akash.CharacterCard, carapace, push.CharacterCard,legacy.CharacterCard,bunker.CharacterCard,scholar.CharacterCard);
            GoToStartOfTurn(push);
            QuickHPCheck(-5, -5, 0, -5, 0, 0);
            AssertInTrash(path);
            AssertInTrash(fortitude);
        }

        [Test()]
        public void TestPathOfDestructionDamageAll()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Whenever an alteration card is in your hand, put it into play.
            //At the start of your turn, {Push} deals 3 targets 3 irreducible projectile damage each. Then you may destroy a hero ongoing or equipment belonging to another player. If no card is destroyed this way, {Push} deals each target other than {Push} 0 projectile damage. Then destroy this card.
            MoveAllCards(push, push.HeroTurnTaker.Hand, push.TurnTaker.Trash);
            Card path = PlayCard("PathOfDestruction");
            Card carapace = PlayCard("MountainousCarapace");
            Card fortitude = PlayCard("Fortitude");
            Card folding = PlayCard("FoldingForces");

            //DecisionSelectTargets = new Card[] { akash.CharacterCard, carapace, legacy.CharacterCard };
            DecisionDoNotSelectCard = SelectionType.DestroyCard; 
            QuickHPStorage(akash.CharacterCard, carapace, push.CharacterCard, legacy.CharacterCard, bunker.CharacterCard, scholar.CharacterCard);
            GoToStartOfTurn(push);
            QuickHPCheck(-6, -7, -5, -1, -2, -2);
            AssertInTrash(path);
            AssertIsInPlay(fortitude);
        }

        [Test()]
        public void TestPhaseAnalyzerDraw()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //The first time each turn that you draw a card, you may draw a second card or discard a card.
            Card phase = PlayCard("PhaseAnalyzer");
            DecisionSelectFunction = 0;
            QuickHandStorage(push);
            DrawCard(push);
            QuickHandCheck(2);

            QuickHandStorage(push);
            DrawCard(push);
            QuickHandCheck(1);
        }

        [Test()]
        public void TestPhaseAnalyzerDiscard()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //The first time each turn that you draw a card, you may draw a second card or discard a card.
            Card phase = PlayCard("PhaseAnalyzer");
            DecisionSelectFunction = 1;
            Card kinetic = PutInHand("KineticAbsorption");
            Card objects = PutInHand("ObjectsInMotion");
            DecisionDiscardCard = kinetic;
            QuickHandStorage(push);
            DrawCard(push);
            QuickHandCheck(0);
            AssertInTrash(kinetic);

            DecisionDiscardCard = objects;
            QuickHandStorage(push);
            DrawCard(push);
            QuickHandCheck(1);
            AssertInHand(objects);
        }

        [Test()]
        public void TestPivotPointHand()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Reveal cards from the top of your deck until an anchor is revealed. Put it into your hand or into play. Discard the other revealed cards.
            MoveAllCards(push, push.HeroTurnTaker.Hand, push.TurnTaker.Trash);
            Card shockwave = PutOnDeck("Shockwave");
            Card phase = PutOnDeck("PhaseAnalyzer");
            Card folding = PutOnDeck("FoldingForces");
            Card bogged = PutInHand("BoggedDown");
            Card feedback = PutInHand("FeedbackBounce");

            DecisionMoveCardDestination = new MoveCardDestination(push.HeroTurnTaker.Hand);

            PlayCard("PivotPoint");
            AssertInHand(shockwave);
            AssertInHand(bogged);
            AssertInHand(feedback);
            AssertInTrash(phase);
            AssertInTrash(folding);
        }

        [Test()]
        public void TestPivotPointPlay()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Reveal cards from the top of your deck until an anchor is revealed. Put it into your hand or into play. Discard the other revealed cards.
            MoveAllCards(push, push.HeroTurnTaker.Hand, push.TurnTaker.Trash);
            Card shockwave = PutOnDeck("Shockwave");
            Card phase = PutOnDeck("PhaseAnalyzer");
            Card folding = PutOnDeck("FoldingForces");
            Card bogged = PutInHand("BoggedDown");
            Card feedback = PutInHand("FeedbackBounce");

            DecisionMoveCardDestination = new MoveCardDestination(push.TurnTaker.PlayArea);

            PlayCard("PivotPoint");
            AssertIsInPlay(shockwave);
            AssertIsInPlay(bogged);
            AssertIsInPlay(feedback);
            AssertInTrash(phase);
            AssertInTrash(folding);
        }

        [Test()]
        public void TestReboundPulse()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //When {Push} deals melee or projectile damage, draw a card.
            //At the start of your turn, destroy this card.
            Card rebound = PlayCard("ReboundPulse");
            Card shockwave = PutOnDeck("Shockwave");
            Card path = PutOnDeck("PathOfDestruction");

            QuickHandStorage(push);
            DealDamage(push, akash, 1, DamageType.Melee);
            QuickHandCheck(1);
            AssertInHand(path);

            QuickHandStorage(push);
            DealDamage(push, akash, 1, DamageType.Projectile);
            QuickHandCheck(1);
            AssertInHand(shockwave);

            GoToStartOfTurn(push);
            AssertInTrash(rebound);
        }

        [Test()]
        public void TestReboundPulseNoDamage()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //When {Push} deals melee or projectile damage, draw a card.
            //At the start of your turn, destroy this card.
            Card rebound = PlayCard("ReboundPulse");
            Card path = PutOnDeck("PathOfDestruction");
            Card carapace = PlayCard("MountainousCarapace");

            QuickHandStorage(push);
            DealDamage(push, akash, 1, DamageType.Melee);
            QuickHandCheck(0);
            AssertOnTopOfDeck(path);
        }

        [Test()]
        public void TestShockwave()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Whenever an alteration card is in your hand, put it into play.
            //At the start of your turn, {Push} deals each non-hero target 2 projectile damage. Then destroy this card.
            MoveAllCards(push, push.HeroTurnTaker.Hand, push.TurnTaker.Trash);
            Card shockwave = PlayCard("Shockwave");
            Card arboreal = PutOnDeck("ArborealPhalanges");
            Card raptor = PlayCard("VelociraptorPack");
            QuickHPStorage(akash.CharacterCard, arboreal, raptor);
            GoToStartOfTurn(push);
            QuickHPCheck(-2, -2, -2);
            AssertInTrash(shockwave);
        }

        [Test()]
        public void TestSympatheticEchoChangeType()
        {
            SetupGameController(new string[] { "GloomWeaver", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis" },advanced: true);
            StartGame();
            //Once per turn, when damage would be dealt to a target, you may discard a card to change the damage type to projectile or melee.
            //The first time each turn melee, projectile, or sonic damage is dealt to {Push} by a target, {Push} may deal the source of that damage 3 projectile damage.
            Card echo = PlayCard("SympatheticEcho");
            Card feedback = PutInHand("FeedbackBounce");
            Card folding = PutInHand("FoldingForces");
            Card phase = PutInHand("PhaseAnalyzer");

            //Check that if no card is discarded, damage type is not changed and you can discard for the next damage
            DecisionSelectCard = feedback;
            DecisionYesNo = false;
            QuickHPStorage(gloom);
            DealDamage(legacy, gloom, 1, DamageType.Energy);
            QuickHPCheck(-1);

            //Check that damage type changes to melee
            ResetDecisions();
            DecisionYesNo = true;
            DecisionSelectCard = feedback;
            DecisionSelectDamageType = DamageType.Melee;
            QuickHPStorage(gloom);
            DealDamage(legacy, gloom, 1, DamageType.Energy);
            QuickHPCheck(0);
            AssertInTrash(feedback);

            //Check that it does not trigger again after being used 
            ResetDecisions();
            DecisionYesNo = true;
            DecisionSelectCard = folding;
            DecisionSelectDamageType = DamageType.Melee;
            QuickHPStorage(gloom);
            DealDamage(legacy, gloom, 1, DamageType.Energy);
            QuickHPCheck(-1);
            AssertInHand(folding);

            //Check that it can trigger again next turn
            ResetDecisions();
            GoToStartOfTurn(push);
            DecisionYesNo = true;
            DecisionSelectCard = phase;
            DecisionSelectDamageType = DamageType.Melee;
            QuickHPStorage(gloom);
            DealDamage(legacy, gloom, 1, DamageType.Energy);
            QuickHPCheck(0);
            AssertInTrash(phase);
        }

        [Test()]
        public void TestSympatheticEchoCounter()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Once per turn, when damage would be dealt to a target, you may discard a card to change the damage type to projectile or melee.
            //The first time each turn melee, projectile, or sonic damage is dealt to {Push} by a target, {Push} may deal the source of that damage 3 projectile damage.
            Card echo = PlayCard("SympatheticEcho");
            DecisionDoNotSelectCard = SelectionType.Custom;

            //Check that it works with Melee
            DecisionYesNo = true;
            QuickHPStorage(akash);
            DealDamage(akash, push, 1, DamageType.Melee);
            QuickHPCheck(-3);

            //Check that it doesn't react the second time
            QuickHPStorage(akash);
            DealDamage(akash, push, 1, DamageType.Melee);
            QuickHPCheck(0);

            //Check that it can react again next turn, and can react to Projectile
            GoToStartOfTurn(push);
            QuickHPStorage(akash);
            DealDamage(akash, push, 1, DamageType.Projectile);
            QuickHPCheck(-3);

            //Check that if you decide not to deal damage, it does not trigger again
            GoToStartOfTurn(legacy);
            DecisionYesNo = false;
            QuickHPStorage(akash);
            DealDamage(akash, push, 1, DamageType.Projectile);
            QuickHPCheck(0);

            DecisionYesNo = true;
            QuickHPStorage(akash);
            DealDamage(akash, push, 1, DamageType.Projectile);
            QuickHPCheck(0);

            //Check that it works with Sonic
            GoToStartOfTurn(bunker);
            QuickHPStorage(akash);
            DealDamage(akash, push, 1, DamageType.Sonic);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestVectorMath()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Draw up to 2 cards.
            //Discard any number of cards.
            //You may play a card.
            Card phase = PutInHand("PhaseAnalyzer");
            Card feedback = PutInHand("FeedbackBounce");
            Card bogged = PutOnDeck("BoggedDown");
            Card shockwave = PutOnDeck("Shockwave");
            DecisionSelectCardToPlay = phase;
            DecisionDoNotSelectCard = SelectionType.DiscardCard;

            PlayCard("VectorMath");
            AssertIsInPlay(phase);
            //AssertInTrash(feedback);
            AssertInHand(bogged);
            AssertInHand(shockwave);
        }

        [Test()]
        public void TestVecotrMathDiscard()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Draw up to 2 cards.
            //Discard any number of cards.
            //You may play a card.
            MoveAllCards(push, push.HeroTurnTaker.Hand, push.TurnTaker.Trash);
            Card bogged = PutOnDeck("BoggedDown");
            Card folding = PutOnDeck("FoldingForces");
            PlayCard("VectorMath");
            AssertInTrash(bogged);
            AssertInTrash(folding);
        }

        [Test()]
        public void TestYeahNo1()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Destroy a target with 4 or fewer HP.
            //You may shuffle a non-character target destroyed this way into its deck. If you do, increase the next damage dealt to {Push} by X, where X is the destroyed target's current HP before it was destroyed.
            Card rockslide = PlayCard("LivingRockslide");
            SetHitPoints(rockslide, 4);
            DecisionYesNo = false;

            PlayCard("YeahNo");
            AssertInTrash(rockslide);
            QuickHPStorage(push);
            DealDamage(akash, push, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestYeahNo2()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //Destroy a target with 4 or fewer HP.
            //You may shuffle a non-character target destroyed this way into its deck. If you do, increase the next damage dealt to {Push} by X, where X is the destroyed target's current HP before it was destroyed.
            Card rockslide = PlayCard("LivingRockslide");
            SetHitPoints(rockslide, 4);
            DecisionYesNo = true;

            PlayCard("YeahNo");
            AssertInDeck(rockslide);
            QuickHPStorage(push);
            DealDamage(akash, push, 1, DamageType.Melee);
            QuickHPCheck(-5);
            QuickHPStorage(push);
            DealDamage(akash, push, 1, DamageType.Melee);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestTwistImmune()
        {
            SetupGameController("AkashBhuta", "TheVisionary", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();

            PlayCard("HeroicInterception");
            DecisionSelectCard = akash.CharacterCard;
            PlayCard("TwistTheEther");
            DealDamage(akash, bunker, 1, DamageType.Melee);
        }

        [Test()]
        public void TestObjective()
        {
            SetupGameController(new string[] { "OblivAeon", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "Megalopolis", "InsulaPrimalis", "TombOfAnubis", "RuinsOfAtlantis", "NexusOfTheVoid", "ChampionStudios" }, shieldIdentifier: "TheArcOfUnreality", scionIdentifiers: new string[] { "DarkMindCharacter" });
            StartGame();

            Card omni = GetCard("TheDigitalAge");
            GoToBeforeStartOfTurn(push);
            MoveCard(push,omni,push.TurnTaker.PlayArea);
            GoToEndOfTurn(push);
        }

        [Test()]
        public void TestNewtonsCradleStatus()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //The first time {Push} deals damage each turn, {Push} deals that same damage to 2 targets that have not been dealt damage this turn.
            //At the start of your turn, destroy this card.
            Card newton = PlayCard("NewtonsCradle");
            Card arboreal = PlayCard("ArborealPhalanges");
            Card rockslide = PlayCard("LivingRockslide");
            Card bogged = PlayCard("BoggedDown");

            DecisionSelectTargets = new Card[] { rockslide, arboreal };
            QuickHPStorage(rockslide, arboreal);
            DealDamage(push, akash, 2, DamageType.Melee);
            QuickHPCheck(-2, -2);

            QuickHPStorage(legacy);
            DealDamage(rockslide, legacy, 5, DamageType.Melee);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestNewtonsCradleIrreducible()
        {
            SetupGameController("AkashBhuta", "VainFacadePlaytest.Push", "Legacy", "Bunker", "TheScholar", "InsulaPrimalis");
            StartGame();
            //The first time {Push} deals damage each turn, {Push} deals that same damage to 2 targets that have not been dealt damage this turn.
            //At the start of your turn, destroy this card.
            Card newton = PlayCard("NewtonsCradle");
            Card arboreal = PlayCard("ArborealPhalanges");
            Card rockslide = PlayCard("LivingRockslide");
            Card carapace = PlayCard("MountainousCarapace");

            DecisionSelectTargets = new Card[] { akash.CharacterCard, arboreal };
            QuickHPStorage(akash.CharacterCard, arboreal);
            DealDamage(push.CharacterCard, rockslide, 2, DamageType.Melee, isIrreducible: true);
            QuickHPCheck(-2, -2);
        }
    }
}

