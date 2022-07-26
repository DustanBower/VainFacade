using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class SphereUtilityCardController : CardController
    {
        public SphereUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public static readonly string emanationKeyword = "emanation";
        public static readonly string heartIdentifier = "AlienHeart";
        public static readonly string HeartKey = "AlienHeartEffectKey";

        public static LinqCardCriteria isEmanation = new LinqCardCriteria((Card c) => c.DoKeywordsContain(emanationKeyword), "Emanation");
        public static LinqCardCriteria isEmanationInPlay = new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain(emanationKeyword), "Emanation in play", false, false, "Emanation in play", "Emanations in play");

        protected IEnumerable<Power> UnusedPowersThisTurn()
        {
            List<Power> powers = new List<Power>();
            BattleZone battleZone = base.Card.BattleZone;
            List<CardController> ccWithPower = new List<CardController>();
            ccWithPower.AddRange(base.GameController.FindCardControllersWhere((Card c) => c.IsInPlayAndHasGameText && c.IsCharacter && FindCardController(c).DecisionMakerWithoutReplacements == base.HeroTurnTakerController, realCardsOnly: false, battleZone));
            ccWithPower.AddRange(base.GameController.FindCardControllersWhere((Card c) => c.IsInPlayAndHasGameText && !c.IsCharacter && FindCardController(c).DecisionMaker == base.HeroTurnTakerController, realCardsOnly: false, battleZone));
            foreach (CardController cc in ccWithPower)
            {
                foreach (Power p in base.GameController.GetAllPowersForCardController(cc, base.HeroTurnTakerController))
                {
                    if (!cc.IsBeingDestroyed && (!p.IsContributionFromCardSource || !p.CardSource.CardController.IsBeingDestroyed))
                    {
                        List<UsePowerJournalEntry> entries = Game.Journal.UsePowerEntriesThisTurn().ToList();
                        Func<UsePowerJournalEntry, bool> predicate = delegate (UsePowerJournalEntry entry)
                        {
                            bool flag = p.CardController.CardWithoutReplacements == entry.CardWithPower;
                            if (!flag && p.CardController.CardWithoutReplacements.SharedIdentifier != null && p.IsContributionFromCardSource)
                            {
                                flag = p.CardController.CardWithoutReplacements.SharedIdentifier == entry.CardWithPower.SharedIdentifier;
                            }
                            if (flag)
                            {
                                flag &= entry.NumberOfUses == 0;
                            }
                            if (flag)
                            {
                                flag &= p.Index == entry.PowerIndex;
                            }
                            if (flag)
                            {
                                flag &= p.IsContributionFromCardSource == entry.IsContributionFromCardSource;
                            }
                            if (flag)
                            {
                                bool flag2 = p.TurnTakerController == null && entry.PowerUser == null;
                                bool flag3 = false;
                                if (p.TurnTakerController != null && p.TurnTakerController.IsHero)
                                {
                                    flag3 = p.TurnTakerController.ToHero().HeroTurnTaker == entry.PowerUser;
                                }
                                flag = flag && (flag2 || flag3);
                            }
                            if (flag)
                            {
                                if (!p.IsContributionFromCardSource)
                                {
                                    if (flag && p.CardController.CardWithoutReplacements.PlayIndex.HasValue && entry.CardWithPowerPlayIndex.HasValue)
                                    {
                                        flag &= p.CardController.CardWithoutReplacements.PlayIndex.Value == entry.CardWithPowerPlayIndex.Value;
                                    }
                                }
                                else
                                {
                                    flag &= entry.CardSource == p.CardSource.Card;
                                    if (p.CardSource != null && p.CardSource.Card.PlayIndex.HasValue && entry.CardSourcePlayIndex.HasValue)
                                    {
                                        flag &= p.CardSource.Card.PlayIndex.Value == entry.CardSourcePlayIndex.Value;
                                    }
                                }
                            }
                            return flag;
                        };
                        int uses = entries.Where(predicate).Count();
                        if (uses <= 0)
                        {
                            powers.Add(p);
                        }
                    }
                }
            }
            return powers;
        }
    }
}
