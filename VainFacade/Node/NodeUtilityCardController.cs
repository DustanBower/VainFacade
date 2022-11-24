using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace VainFacadePlaytest.Node
{
    public class NodeUtilityCardController : CardController
    {
        public NodeUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public static readonly string ConnectionKeyword = "connection";
        public static readonly string CollectiveIdentifier = "CollectiveUnconscious";
        public static readonly string OpenIdentifier = "OpenLine";
        public static readonly string PacketIdentifier = "PacketSniffing";
        public static readonly string PsychicIdentifier = "PsychicLink";
        public static readonly string[] ConnectorIdentifiers = new string[] { CollectiveIdentifier, OpenIdentifier, PacketIdentifier, PsychicIdentifier };

        public static LinqCardCriteria isConnection = new LinqCardCriteria((Card c) => c.DoKeywordsContain(ConnectionKeyword), "Connection");
        public static LinqCardCriteria isConnectionInPlay = new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain(ConnectionKeyword), "Connection in play", false, false, "Connection in play", "Connections in play");

        public static LinqCardCriteria isConnector = new LinqCardCriteria((Card c) => c.Owner.Name == "Node" && ConnectorIdentifiers.Contains(c.Identifier));
        public static LinqCardCriteria isConnectorInPlay = new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.Owner.Name == "Node" && ConnectorIdentifiers.Contains(c.Identifier));

        public bool IsConnected(Card c)
        {
            // Collective Unconscious, Open Line, Packet Sniffing, and Psychic Link:
            // "Cards in this play area and {NodeCharacter}'s are [i]Connected.[/i]"
            if (c.IsInPlayAndHasGameText)
            {
                List<Card> results = FindCardsWhere(isConnectorInPlay).ToList();
                if (c.Location.HighestRecursiveLocation == base.TurnTaker.PlayArea)
                {
                    // Return true if Collective Unconscious, Open Line, Packet Sniffing, and/or Psychic Link is in play anywhere
                    return results.Any();
                }
                else
                {
                    // Return true if Collective Unconscious, Open Line, Packet Sniffing, and/or Psychic Link is in play *in the same play area as the specified card*
                    Location playArea = c.Location.HighestRecursiveLocation;
                    List<Card> playAreaResults = results.Where((Card d) => d.Location.HighestRecursiveLocation == playArea).ToList();
                    return playAreaResults.Any();
                }
            }
            return false;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            yield break;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            yield break;
        }
    }
}
