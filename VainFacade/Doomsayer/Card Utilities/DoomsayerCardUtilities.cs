using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class DoomsayerCardUtilities:CardController
	{
		public DoomsayerCardUtilities(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowIfElseSpecialString(() => base.GameController.IsCardIndestructible(this.Card), () => $"{this.Card.Title} is indestructible.", () => $"{this.Card.Title} is not indestructible.").Condition = () => this.Card.IsInPlayAndHasGameText && base.GameController.IsCardIndestructible(this.Card);
        }

        public bool IsProclamation(Card c)
		{
			return base.GameController.DoesCardContainKeyword(c, "proclamation");
		}

		public LinqCardCriteria ProclamationCriteria()
		{
            return new LinqCardCriteria((Card c) => IsProclamation(c), "proclamation", false, false, "proclamation", "proclamations");
        }

        public bool IsRole(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "role");
        }

        public LinqCardCriteria RoleCriteria()
        {
            return new LinqCardCriteria((Card c) => IsRole(c), "role", false, false, "role", "roles");
        }

        public bool IsPattern(Card c)
        {
            return base.GameController.DoesCardContainKeyword(c, "pattern");
        }

        public LinqCardCriteria PatternCriteria()
        {
            return new LinqCardCriteria((Card c) => IsPattern(c), "pattern", false, false, "pattern", "patterns");
        }
    }
}

