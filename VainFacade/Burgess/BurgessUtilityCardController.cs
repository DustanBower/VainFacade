using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Burgess
{
    public class BurgessUtilityCardController : CardController
    {
        public BurgessUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        protected static readonly string BackupKeyword = "backup";
        protected static readonly string ClueKeyword = "clue";

        protected static LinqCardCriteria BackupCard = new LinqCardCriteria((Card c) => c.DoKeywordsContain(BackupKeyword), "Backup");
        protected static LinqCardCriteria ClueCard = new LinqCardCriteria((Card c) => c.DoKeywordsContain(ClueKeyword), "Clue");
    }
}
