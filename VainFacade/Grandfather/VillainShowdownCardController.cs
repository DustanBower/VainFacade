using Boomlagoon.JSON;
using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class VillainShowdownCardController : CardController
    {
        public VillainShowdownCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowHeroTargetWithHighestHP(1);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, select a villain from the box. Treat this card as having the same name and nemesis."
            yield return SetVillainIdentity();
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Select a villain", "choosing a villain", "Vote for a villain", "a villain for Villain Showdown to copy");
        }

        private IEnumerator SetVillainIdentity()
        {
            Log.Debug("VillainShowdownCardController.SetVillainIdentity: starting");
            List<List<string>> storedResults = new List<List<string>>();
            Log.Debug("VillainShowdownCardController.SetVillainIdentity: calling SelectVillain");
            IEnumerator selectCoroutine = SelectVillain(storedResults);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            if (storedResults.Count() == 0)
            {
                yield break;
            }
            List<string> villainStats = storedResults.First();
            string villainTitle = villainStats.First();
            Log.Debug("VillainShowdownCardController.SetVillainIdentity: villainTitle: " + villainTitle);
            List<string> villainNemesisIdentifiers = new List<string>();
            foreach (string item in villainStats)
            {
                villainNemesisIdentifiers.Add(item);
            }
            villainNemesisIdentifiers.RemoveAt(0);
            foreach(string nemesis in villainNemesisIdentifiers)
            {
                Log.Debug("VillainShowdownCardController.SetVillainIdentity: nemesis identifier: " + nemesis);
            }
            // Now we have the stats we need, just need to splice them into a JSON object to use for a CardDefinition...
            JSONObject jsonDef = new JSONObject();
            jsonDef.Add("identifier", new JSONValue("VillainShowdownCopy"));
            jsonDef.Add("count", new JSONValue(1));
            jsonDef.Add("title", new JSONValue(villainTitle));
            JSONArray keywords = new JSONArray();
            keywords.Add(new JSONValue("fulcrum"));
            jsonDef.Add("keywords", keywords);
            jsonDef.Add("hitpoints", new JSONValue(40));
            JSONArray nemeses = new JSONArray();
            foreach(string identifier in villainNemesisIdentifiers)
            {
                nemeses.Add(new JSONValue(identifier));
            }
            jsonDef.Add("nemesisIdentifiers", nemeses);
            JSONArray body = new JSONArray();
            body.Add(new JSONValue("This card is indestructible while it has 1 or more HP."));
            body.Add(new JSONValue("At the end of the villain turn, this card deals the hero target with the highest HP {H + 1} melee damage."));
            jsonDef.Add("body", body);
            JSONArray icons = new JSONArray();
            icons.Add(new JSONValue("Indestructible"));
            icons.Add(new JSONValue("EndOfTurnAction"));
            icons.Add(new JSONValue("DealDamageMelee"));
            jsonDef.Add("icons", icons);
            JSONObject quote = new JSONObject();
            quote.Add("identifier", new JSONValue("Scavenger"));
            quote.Add("text", new JSONValue("Dracimp infiltrator: Reaper series{BR}prototype combat mimic. Only unit that{BR}survived. Dangerously unstable, Gramps.{BR}Power up at your own risk."));
            JSONArray quotes = new JSONArray();
            quotes.Add(quote);
            jsonDef.Add("flavorQuotes", quotes);
            jsonDef.Add("flavorReference", new JSONValue("Scavenger, Sphere: Grandfather Paradox #2"));
            // Make the CardDefinition
            Log.Debug("VillainShowdownCardController.SetVillainIdentity: creating CardDefinition copyDef");
            CardDefinition copyDef = new CardDefinition(jsonDef, base.TurnTaker.DeckDefinition, "VainFacadePlaytest.Grandfather");
            // Make the card, put it OffToTheSide
            Log.Debug("VillainShowdownCardController.SetVillainIdentity: creating Card newCopy");
            Card newCopy = new Card(copyDef, base.TurnTaker, 1);
            base.TurnTaker.OffToTheSide.AddCard(newCopy);
            CardController copyController = CardControllerFactory.CreateInstance(newCopy, base.TurnTakerController, base.TurnTaker.Identifier);
            base.TurnTakerController.AddCardController(copyController);
            // Switch it with this one
            Log.Debug("VillainShowdownCardController.SetVillainIdentity: calling GameController.SwitchCards");
            IEnumerator switchCoroutine = base.GameController.SwitchCards(base.Card, newCopy, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(switchCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(switchCoroutine);
            }

        }

        private IEnumerator SelectVillain(List<List<string>> storedResults)
        {
            Log.Debug("VillainShowdownCardController.SelectVillain: starting");
            Dictionary<string, List<string>> titleStatsDict = new Dictionary<string, List<string>>();
            Log.Debug("VillainShowdownCardController.SelectVillain: calling LoadPlayableBaseVillainIdentifiers");
            Dictionary<string, List<string>> playableBaseIdentifiers = LoadPlayableBaseVillainIdentifiers();
            foreach (string id in playableBaseIdentifiers.Keys)
            {
                Log.Debug("Villain character in the box: " + id);
                titleStatsDict.Add(id, playableBaseIdentifiers[id]);
            }
            Log.Debug("VillainShowdownCardController.SelectVillain: calling LoadAllModVillainIdentifiers");
            Dictionary<string, List<string>> modIdentifiers = LoadAllModVillainIdentifiers();
            foreach (string id2 in modIdentifiers.Keys)
            {
                Log.Debug("Villain character in the box: " + id2);
                titleStatsDict.Add(id2, modIdentifiers[id2]);
            }
            string[] optionChoices = titleStatsDict.Keys.ToArray();
            Log.Debug("VillainShowdownCardController.SelectVillain: calling GameController.SelectWord");
            List<SelectWordDecision> optionsDecisionResult = new List<SelectWordDecision>();
            IEnumerator coroutine = base.GameController.SelectWord(DecisionMaker, optionChoices, SelectionType.Custom, optionsDecisionResult, optional: false, null, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            Log.Debug("VillainShowdownCardController.SelectVillain: DidSelectWord returned " + DidSelectWord(optionsDecisionResult).ToString());
            if (DidSelectWord(optionsDecisionResult))
            {
                string selectedVillain = GetSelectedWord(optionsDecisionResult);
                Log.Debug("VillainShowdownCardController.SelectVillain: selectedVillain: " + selectedVillain);
                List<string> selectedVillainStats = titleStatsDict[selectedVillain];
                storedResults.Add(selectedVillainStats);
            }
        }

        private Dictionary<string, List<string>> LoadPlayableBaseVillainIdentifiers()
        {
            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: starting");
            Dictionary<string, List<string>> titleNemesisPairs = new Dictionary<string, List<string>>();
            Assembly assembly = (from a in AppDomain.CurrentDomain.GetAssemblies()
                                 where a.GetName().Name == "SentinelsEngine"
                                 select a).FirstOrDefault();
            if (assembly == null)
            {
                return titleNemesisPairs;
            }
            IEnumerable<string> source = (from kvp in base.GameController.GetHeroCardsInBox((string s) => true, (string s) => true)
                                          select kvp.Key into id
                                          where !id.Contains('.')
                                          select DeckDefinitionCache.GetDeckDefinition(id) into dd
                                          where dd.ExpansionIdentifier != null
                                          select dd.ExpansionIdentifier).Distinct();
            string[] manifestResourceNames = assembly.GetManifestResourceNames();
            foreach (string text in manifestResourceNames)
            {
                Stream manifestResourceStream = assembly.GetManifestResourceStream(text);
                if (manifestResourceStream == null || manifestResourceStream.Length == 0)
                {
                    continue;
                }
                JSONObject jSONObject;
                using (StreamReader streamReader = new StreamReader(manifestResourceStream))
                {
                    string text2 = streamReader.ReadToEnd();
                    if (string.IsNullOrEmpty(text2))
                    {
                        continue;
                    }
                    jSONObject = JSONObject.Parse(text2);
                    goto IL_01cc;
                }
            IL_01cc:
                if (jSONObject == null)
                {
                    continue;
                }
                string @string = jSONObject.GetString("kind");
                if (@string != "Villain")
                {
                    continue;
                }
                if (jSONObject.ContainsKey("expansionIdentifier"))
                {
                    string string2 = jSONObject.GetString("expansionIdentifier");
                    if (!source.Contains(string2))
                    {
                        continue;
                    }
                }
                string text3 = text.Replace("DeckList.json", string.Empty);
                text3 = text3.Replace("Handelabra.Sentinels.Engine.DeckLists.", string.Empty);
                DeckDefinition def2 = DeckDefinitionCache.GetDeckDefinition(text3);
                List<CardDefinition> characterCardDefinitions = def2.GetAllCardDefinitions().Where((CardDefinition cd) => cd.IsCharacter).ToList();
                foreach (CardDefinition characterDef in characterCardDefinitions)
                {
                    Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: checking stats for " + characterDef.PromoIdentifierOrIdentifier);
                    if (characterDef.PromoIdentifierOrIdentifier.Contains("OblivAeon"))
                    {
                        Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: found OblivAeon CharacterCardDefinition: " + characterDef.PromoIdentifierOrIdentifier);
                        if (characterDef.PromoIdentifierOrIdentifier.Contains("FrontPage"))
                        {
                            string frontKey = characterDef.PromoTitleOrTitle;
                            if (def2.PromoCardDefinitions.Contains(characterDef) || characterDef.FlippedNemesisIdentifiers.Count() > 0)
                            {
                                if (characterDef.PromoTitle == null)
                                {
                                    if (characterDef.Body.Any())
                                    {
                                        frontKey += " (" + characterDef.Body.First() + ")";
                                    }
                                    else
                                    {
                                        frontKey += " (promo, front)";
                                    }
                                }
                            }
                            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: Adding villain character option: " + frontKey + " (front)");
                            List<string> identifiers = new List<string>();
                            identifiers.Add(characterDef.PromoTitleOrTitle);
                            identifiers.AddRange(characterDef.NemesisIdentifiers);
                            titleNemesisPairs.Add(frontKey, identifiers);
                        }
                        else
                        {
                            Log.Debug("VillainShowdownCardController.LoadPlayableVillainIdentifiers: not adding any more OblivAeon CharacterCardDefinitions");
                        }
                    }
                    else
                    {
                        bool addedFront = false;
                        Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + " stats: TargetKind.HasValue: " + characterDef.TargetKind.HasValue.ToString());
                        Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + " stats: keywords contain Villain: " + characterDef.Keywords.Contains("villain").ToString());
                        if (characterDef.TargetKind.HasValue)
                        {
                            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + " stats: TargetKind.Value == Villain: " + (characterDef.TargetKind.Value == DeckDefinition.DeckKind.Villain).ToString());
                            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + " stats: TargetKind.Value == VillainTeam: " + (characterDef.TargetKind.Value == DeckDefinition.DeckKind.VillainTeam).ToString());
                        }
                        if (!characterDef.TargetKind.HasValue || characterDef.Keywords.Contains("villain") || characterDef.TargetKind.Value == DeckDefinition.DeckKind.Villain || characterDef.TargetKind.Value == DeckDefinition.DeckKind.VillainTeam)
                        {
                            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: conclusion: " + characterDef.PromoIdentifierOrIdentifier + " is villain on front side");
                            // Villain on the front side? Add front side as an option
                            string frontKey = characterDef.PromoTitleOrTitle;
                            if (def2.PromoCardDefinitions.Contains(characterDef) || characterDef.FlippedNemesisIdentifiers.Count() > 0)
                            {
                                if (characterDef.PromoTitle == null)
                                {
                                    if (characterDef.Body.Any())
                                    {
                                        frontKey += " (" + characterDef.Body.First() + ")";
                                    }
                                    else
                                    {
                                        frontKey += " (promo, front)";
                                    }
                                }
                            }
                            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: Adding villain character option: " + frontKey + " (front)");
                            List<string> identifiers = new List<string>();
                            identifiers.Add(characterDef.PromoTitleOrTitle);
                            identifiers.AddRange(characterDef.NemesisIdentifiers);
                            titleNemesisPairs.Add(frontKey, identifiers);
                            addedFront = true;
                        }
                        Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + " stats: FlippedTargetKind.HasValue: " + characterDef.FlippedTargetKind.HasValue.ToString());
                        Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + " stats: FlippedKeywords contain Villain: " + characterDef.FlippedKeywords.Contains("villain").ToString());
                        if (characterDef.FlippedTargetKind.HasValue)
                        {
                            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + " stats: FlippedTargetKind.Value == Villain: " + (characterDef.FlippedTargetKind.Value == DeckDefinition.DeckKind.Villain).ToString());
                            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + " stats: FlippedTargetKind.Value == VillainTeam: " + (characterDef.FlippedTargetKind.Value == DeckDefinition.DeckKind.VillainTeam).ToString());
                        }
                        if (!characterDef.FlippedTargetKind.HasValue || characterDef.FlippedKeywords.Contains("villain") || characterDef.FlippedTargetKind.Value == DeckDefinition.DeckKind.Villain || characterDef.FlippedTargetKind.Value == DeckDefinition.DeckKind.VillainTeam)
                        {
                            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: conclusion: " + characterDef.PromoIdentifierOrIdentifier + " is villain on back side");
                            // Villain on the back side
                            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: checking if back side has distinct stats");
                            bool isBackDistinct = false;
                            string backKey = characterDef.PromoTitleOrTitle;
                            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + " stats: Title: \"" + characterDef.Title + "\", FlippedTitle: \"" + characterDef.FlippedTitle + "\"");
                            if (characterDef.FlippedTitle != null && characterDef.FlippedTitle != "" && characterDef.FlippedTitle != characterDef.Title)
                            {
                                Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: conclusion: " + characterDef.PromoIdentifierOrIdentifier + " back side is distinct villain");
                                isBackDistinct = true;
                                backKey = characterDef.FlippedTitle;
                            }
                            foreach (string nemesis in characterDef.NemesisIdentifiers)
                            {
                                Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + " stats: nemesis: " + nemesis);
                            }
                            foreach (string nemesis in characterDef.FlippedNemesisIdentifiers)
                            {
                                Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + " stats: flipped nemesis: " + nemesis);
                            }
                            if (characterDef.FlippedNemesisIdentifiers != null && characterDef.FlippedNemesisIdentifiers.Count() > 0 && characterDef.FlippedNemesisIdentifiers != characterDef.NemesisIdentifiers)
                            {
                                Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: conclusion: " + characterDef.PromoIdentifierOrIdentifier + " back side is distinct villain");
                                isBackDistinct = true;
                                if (characterDef.FlippedBody.Any())
                                {
                                    backKey = characterDef.PromoTitleOrTitle + " (" + characterDef.FlippedBody.First() + ")";
                                }
                                else
                                {
                                    backKey = characterDef.PromoTitleOrTitle + " (back)";
                                }
                            }
                            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + " stats: FlippedBody contains Incapacitated: " + characterDef.FlippedBody.Contains("Incapacitated").ToString());
                            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + " stats: FlippedShowHitPoints: " + characterDef.FlippedShowHitPoints.ToString());
                            if (characterDef.FlippedBody.Contains("Incapacitated") || !characterDef.FlippedShowHitPoints)
                            {
                                Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: conclusion: " + characterDef.PromoIdentifierOrIdentifier + " back side is **NOT** distinct villain");
                                isBackDistinct = false;
                            }


                            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + ": addedFront: " + addedFront.ToString());
                            Log.Debug("VillainShowdownCardController.LoadPlayableBaseVillainIdentifiers: " + characterDef.PromoIdentifierOrIdentifier + ": isBackDistinct: " + isBackDistinct.ToString());
                            if (!addedFront || isBackDistinct)
                            {
                                // Add back side as an option
                                Log.Debug("Adding villain character option: " + backKey + " (back)");
                                List<string> identifiers = new List<string>();
                                identifiers.Add(text3);
                                if (characterDef.FlippedNemesisIdentifiers.Count() > 0)
                                {
                                    identifiers.AddRange(characterDef.FlippedNemesisIdentifiers);
                                }
                                else
                                {
                                    identifiers.AddRange(characterDef.NemesisIdentifiers);
                                }
                                titleNemesisPairs.Add(backKey, identifiers);
                            }
                        }
                    }
                }
            }
            return titleNemesisPairs;
        }

        private Dictionary<string, List<string>> LoadAllModVillainIdentifiers()
        {
            Dictionary<string, List<string>> titleNemesisPairs = new Dictionary<string, List<string>>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Assembly assemblyForAssemblyName = ModHelper.GetAssemblyForAssemblyName(assembly.GetName());
                if (assemblyForAssemblyName == null)
                {
                    continue;
                }
                string[] manifestResourceNames = assemblyForAssemblyName.GetManifestResourceNames();
                foreach (string text in manifestResourceNames)
                {
                    Stream manifestResourceStream = assemblyForAssemblyName.GetManifestResourceStream(text);
                    if (manifestResourceStream == null || manifestResourceStream.Length == 0)
                    {
                        continue;
                    }
                    JSONObject jSONObject;
                    using (StreamReader streamReader = new StreamReader(manifestResourceStream))
                    {
                        string text2 = streamReader.ReadToEnd();
                        if (string.IsNullOrEmpty(text2))
                        {
                            continue;
                        }
                        jSONObject = JSONObject.Parse(text2);
                        goto IL_00bd;
                    }
                IL_00bd:
                    if (jSONObject == null)
                    {
                        continue;
                    }
                    string @string = jSONObject.GetString("kind");
                    if (!(@string != "Villain"))
                    {
                        string text3 = text.Replace("DeckList.json", string.Empty);
                        text3 = text3.Replace("DeckLists.", string.Empty);
                        if (!(text3 == base.TurnTaker.QualifiedIdentifier))
                        {
                            DeckDefinition def2 = DeckDefinitionCache.GetDeckDefinition(text3);
                            List<CardDefinition> characterCardDefinitions = def2.GetAllCardDefinitions().Where((CardDefinition cd) => cd.IsCharacter).ToList();
                            foreach (CardDefinition characterDef in characterCardDefinitions)
                            {
                                bool addedFront = false;
                                if (!characterDef.TargetKind.HasValue || characterDef.Keywords.Contains("villain") || characterDef.TargetKind.Value == DeckDefinition.DeckKind.Villain || characterDef.TargetKind.Value == DeckDefinition.DeckKind.VillainTeam)
                                {
                                    // Villain on the front side? Add front side as an option
                                    string frontKey = characterDef.PromoTitleOrTitle;
                                    if (def2.PromoCardDefinitions.Contains(characterDef) || characterDef.FlippedNemesisIdentifiers.Count() > 0)
                                    {
                                        if (characterDef.PromoTitle == null)
                                        {
                                            if (characterDef.Body.Any())
                                            {
                                                frontKey += " (" + characterDef.Body.First() + ")";
                                            }
                                            else
                                            {
                                                frontKey += " (promo, front)";
                                            }
                                        }
                                    }
                                    Log.Debug("Adding villain character option: " + frontKey + " (front)");
                                    List<string> identifiers = new List<string>();
                                    identifiers.Add(characterDef.PromoTitleOrTitle);
                                    identifiers.AddRange(characterDef.NemesisIdentifiers);
                                    if (!titleNemesisPairs.Keys.Contains(frontKey))
                                    {
                                        titleNemesisPairs.Add(frontKey, identifiers);
                                        addedFront = true;
                                    }
                                    else
                                    {
                                        Log.Debug("Could not add villain character option: " + frontKey + " (duplicate key)");
                                    }
                                }
                                if (!characterDef.FlippedTargetKind.HasValue || characterDef.FlippedKeywords.Contains("villain") || characterDef.FlippedTargetKind.Value == DeckDefinition.DeckKind.Villain || characterDef.FlippedTargetKind.Value == DeckDefinition.DeckKind.VillainTeam)
                                {
                                    // Villain on the back side
                                    bool isBackDistinct = false;
                                    string backKey = characterDef.PromoTitleOrTitle;
                                    if (characterDef.FlippedTitle != null && characterDef.FlippedTitle != "" && characterDef.FlippedTitle != characterDef.Title)
                                    {
                                        isBackDistinct = true;
                                        backKey = characterDef.FlippedTitle;
                                    }
                                    if (characterDef.FlippedNemesisIdentifiers != null && characterDef.FlippedNemesisIdentifiers != characterDef.NemesisIdentifiers)
                                    {
                                        isBackDistinct = true;
                                        if (characterDef.FlippedBody.Any())
                                        {
                                            backKey = characterDef.PromoTitleOrTitle + " (" + characterDef.FlippedBody.First() + ")";
                                        }
                                        else
                                        {
                                            backKey = characterDef.PromoTitleOrTitle + " (back)";
                                        }
                                    }
                                    if (characterDef.FlippedBody.Contains("Incapacitated") || !characterDef.FlippedShowHitPoints)
                                    {
                                        isBackDistinct = false;
                                    }

                                    if (!addedFront || isBackDistinct)
                                    {
                                        // Add back side as an option
                                        Log.Debug("Adding villain character option: " + backKey + " (back)");
                                        List<string> identifiers = new List<string>();
                                        identifiers.Add(text3);
                                        if (characterDef.FlippedNemesisIdentifiers.Count() > 0)
                                        {
                                            identifiers.AddRange(characterDef.FlippedNemesisIdentifiers);
                                        }
                                        else
                                        {
                                            identifiers.AddRange(characterDef.NemesisIdentifiers);
                                        }
                                        if (!titleNemesisPairs.Keys.Contains(backKey))
                                        {
                                            titleNemesisPairs.Add(backKey, identifiers);
                                        }
                                        else
                                        {
                                            Log.Debug("Could not add villain character option: " + backKey + " (duplicate key)");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return titleNemesisPairs;
        }
    }
}
