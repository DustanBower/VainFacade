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

        private IEnumerator SetVillainIdentity()
        {
            List<List<string>> storedResults = new List<List<string>>();
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
            List<string> villainNemesisIdentifiers = villainStats.Copy();
            villainNemesisIdentifiers.RemoveAt(0);
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
            CardDefinition copyDef = new CardDefinition(jsonDef, base.TurnTaker.DeckDefinition, "VainFacadePlaytest.Grandfather");
            // Make the card, put it OffToTheSide
            Card newCopy = new Card(copyDef, base.TurnTaker, 1);
            base.TurnTaker.OffToTheSide.AddCard(newCopy);
            CardController copyController = CardControllerFactory.CreateInstance(newCopy, base.TurnTakerController, base.TurnTaker.Identifier);
            base.TurnTakerController.AddCardController(copyController);
            // Switch it with this one
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
            Dictionary<string, List<string>> titleStatsDict = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> playableBaseIdentifiers = LoadPlayableBaseVillainIdentifiers();
            foreach (string id in playableBaseIdentifiers.Keys)
            {
                Log.Debug("Villain character in the box: " + id);
                titleStatsDict.Add(id, playableBaseIdentifiers[id]);
            }
            Dictionary<string, List<string>> modIdentifiers = LoadAllModVillainIdentifiers();
            foreach (string id2 in modIdentifiers.Keys)
            {
                Log.Debug("Villain character in the box: " + id2);
                titleStatsDict.Add(id2, modIdentifiers[id2]);
            }
            string[] optionChoices = titleStatsDict.Keys.ToArray();
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
            if (DidSelectWord(optionsDecisionResult))
            {
                string selectedVillain = GetSelectedWord(optionsDecisionResult);
                List<string> selectedVillainStats = titleStatsDict[selectedVillain];
                storedResults.Add(selectedVillainStats);
            }
        }

        private Dictionary<string, List<string>> LoadPlayableBaseVillainIdentifiers()
        {
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
                    bool addedFront = false;
                    if (!characterDef.TargetKind.HasValue || characterDef.Keywords.Contains("villain") || characterDef.TargetKind.Value == DeckDefinition.DeckKind.Villain || characterDef.TargetKind.Value == DeckDefinition.DeckKind.VillainTeam)
                    {
                        // Villain on the front side? Add front side as an option
                        string charTitle = characterDef.PromoTitleOrTitle;
                        if (def2.PromoCardDefinitions.Contains(characterDef) || characterDef.FlippedNemesisIdentifiers.Count() > 0)
                        {
                            if (characterDef.PromoTitle == null)
                            {
                                charTitle += " (" + characterDef.Body + ")";
                            }
                        }
                        Log.Debug("Adding villain character option: " + charTitle + " (front)");
                        List<string> identifiers = new List<string>();
                        identifiers.Add(characterDef.PromoTitleOrTitle);
                        identifiers.AddRange(characterDef.NemesisIdentifiers);
                        titleNemesisPairs.Add(charTitle, identifiers);
                        addedFront = true;
                    }
                    if (!characterDef.FlippedTargetKind.HasValue || characterDef.FlippedKeywords.Contains("villain") || characterDef.FlippedTargetKind.Value == DeckDefinition.DeckKind.Villain || characterDef.FlippedTargetKind.Value == DeckDefinition.DeckKind.VillainTeam)
                    {
                        // Villain on the back side
                        bool isBackDistinct = false;
                        string flippedTitle = characterDef.PromoTitleOrTitle;
                        if (characterDef.FlippedTitle != null && characterDef.FlippedTitle != characterDef.Title)
                        {
                            isBackDistinct = true;
                            flippedTitle = characterDef.FlippedTitle;
                        }
                        if (characterDef.FlippedNemesisIdentifiers != null && characterDef.FlippedNemesisIdentifiers != characterDef.NemesisIdentifiers)
                        {
                            isBackDistinct = true;
                            if (def2.PromoCardDefinitions.Contains(characterDef))
                            {
                                if (characterDef.PromoTitle == null)
                                {
                                    flippedTitle = characterDef.PromoTitleOrTitle + " (" + characterDef.FlippedBody + ")";
                                }
                            }
                        }
                        if (characterDef.FlippedBody.Contains("Incapacitated") || !characterDef.FlippedShowHitPoints)
                        {
                            isBackDistinct = false;
                        }

                        if (!addedFront || isBackDistinct)
                        {
                            // Add back side as an option
                            Log.Debug("Adding villain character option: " + flippedTitle + " (back)");
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
                            titleNemesisPairs.Add(flippedTitle, identifiers);
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
                                    string charTitle = characterDef.PromoTitleOrTitle;
                                    if (def2.PromoCardDefinitions.Contains(characterDef) || characterDef.FlippedNemesisIdentifiers.Count() > 0)
                                    {
                                        if (characterDef.PromoTitle == null)
                                        {
                                            charTitle += " (" + characterDef.Body + ")";
                                        }
                                    }
                                    Log.Debug("Adding villain character option: " + charTitle + " (front)");
                                    List<string> identifiers = new List<string>();
                                    identifiers.Add(characterDef.PromoTitleOrTitle);
                                    identifiers.AddRange(characterDef.NemesisIdentifiers);
                                    titleNemesisPairs.Add(charTitle, identifiers);
                                    addedFront = true;
                                }
                                if (!characterDef.FlippedTargetKind.HasValue || characterDef.FlippedKeywords.Contains("villain") || characterDef.FlippedTargetKind.Value == DeckDefinition.DeckKind.Villain || characterDef.FlippedTargetKind.Value == DeckDefinition.DeckKind.VillainTeam)
                                {
                                    // Villain on the back side
                                    bool isBackDistinct = false;
                                    string flippedTitle = characterDef.PromoTitleOrTitle;
                                    if (characterDef.FlippedTitle != null && characterDef.FlippedTitle != characterDef.Title)
                                    {
                                        isBackDistinct = true;
                                        flippedTitle = characterDef.FlippedTitle;
                                    }
                                    if (characterDef.FlippedNemesisIdentifiers != null && characterDef.FlippedNemesisIdentifiers != characterDef.NemesisIdentifiers)
                                    {
                                        isBackDistinct = true;
                                        if (def2.PromoCardDefinitions.Contains(characterDef))
                                        {
                                            if (characterDef.PromoTitle == null)
                                            {
                                                flippedTitle = characterDef.PromoTitleOrTitle + " (" + characterDef.FlippedBody + ")";
                                            }
                                        }
                                    }
                                    if (characterDef.FlippedBody.Contains("Incapacitated") || !characterDef.FlippedShowHitPoints)
                                    {
                                        isBackDistinct = false;
                                    }

                                    if (!addedFront || isBackDistinct)
                                    {
                                        // Add back side as an option
                                        Log.Debug("Adding villain character option: " + flippedTitle + " (back)");
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
                                        titleNemesisPairs.Add(flippedTitle, identifiers);
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
