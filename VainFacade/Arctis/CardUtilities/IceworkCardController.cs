using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Arctis
{
	public class IceworkCardController:ArctisCardUtilities
	{
		public IceworkCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            //When this card is destroyed, draw 2 cards
            AddWhenDestroyedTrigger((DestroyCardAction dc) => DrawCards(this.HeroTurnTakerController, 2),TriggerType.DrawCard);
        }


        public override void AddStartOfGameTriggers()
        {
            //When this card is played, destroy it
            AddTrigger<PlayCardAction>((PlayCardAction pca) => pca.CardToPlay == this.Card && !pca.IsPutIntoPlay, (PlayCardAction pca) => DestroyThisCardResponse(pca), TriggerType.DestroySelf, TriggerTiming.After);
        }
    }
}