using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace EchoChamber.Editor
{
    /// <summary>Imports docs/data/*.json into the GameContent and TuningConfig ScriptableObjects.</summary>
    public static class ContentImporter
    {
        public const string DataDir = "docs/data";
        public const string ContentAssetPath = "Assets/_Project/Data/GameContent.asset";
        public const string TuningAssetPath = "Assets/_Project/Data/TuningConfig.asset";

        [MenuItem("Echo Chamber/Import Content From docs-data")]
        public static void ImportAll()
        {
            var content = BuildContent();
            var asset = AssetDatabase.LoadAssetAtPath<GameContent>(ContentAssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<GameContent>();
                AssetDatabase.CreateAsset(asset, ContentAssetPath);
            }
            asset.content = content;
            EditorUtility.SetDirty(asset);

            var tuning = AssetDatabase.LoadAssetAtPath<TuningConfig>(TuningAssetPath);
            if (tuning == null)
            {
                tuning = ScriptableObject.CreateInstance<TuningConfig>();
                AssetDatabase.CreateAsset(tuning, TuningAssetPath);
                EditorUtility.SetDirty(tuning);
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[EchoChamber] Imported {content.pool.Count} pool posts, {content.scripted.Count} scripted posts, " +
                      $"{content.days.Count} days, {content.neighbours.Count} neighbours, {content.places.Count} places, {content.endings.Count} endings.");
        }

        private static string ProjectRoot => Path.GetDirectoryName(Application.dataPath);

        private static JToken Read(string file)
        {
            return JToken.Parse(File.ReadAllText(Path.Combine(ProjectRoot, DataDir, file)));
        }

        public static ContentSet BuildContent()
        {
            var set = new ContentSet();

            JObject posts = (JObject)Read("posts.json");
            foreach (var p in posts["pool"]) set.pool.Add(ParsePost(p));
            foreach (var p in posts["scripted"]) set.scripted.Add(ParsePost(p));

            JObject days = (JObject)Read("days.json");
            foreach (var d in days["days"])
            {
                var day = new DayData
                {
                    name = (string)d["name"],
                    target = (int)d["target"],
                    boss = (string)d["boss"],
                    brief = (string)d["brief"]
                };
                foreach (var turn in d["turns"])
                {
                    var td = new TurnData();
                    foreach (var item in turn)
                    {
                        var e = new TurnEntry();
                        if (item.Type == JTokenType.Integer)
                        {
                            e.kind = TurnEntryKind.Random;
                            e.count = (int)item;
                        }
                        else if ((string)item == "+pub")
                        {
                            e.kind = TurnEntryKind.MerceariaAd;
                        }
                        else
                        {
                            e.kind = TurnEntryKind.Scripted;
                            e.postId = (string)item;
                        }
                        td.entries.Add(e);
                    }
                    day.turns.Add(td);
                }
                set.days.Add(day);
            }

            foreach (var n in (JArray)Read("neighbours.json"))
            {
                string label = (string)n["trait"];
                set.neighbours.Add(new NeighbourData
                {
                    id = (int)n["id"],
                    name = (string)n["name"],
                    traitLabel = label,
                    trait = ParseTrait(label),
                    color = (string)n["color"],
                    home = ParsePlace((string)n["home"])
                });
            }

            JObject places = (JObject)Read("places.json");
            foreach (var kv in (JObject)places["places"])
            {
                var v = kv.Value;
                var pd = new PlaceData
                {
                    id = ParsePlace(kv.Key),
                    shortName = (string)v["short"],
                    displayName = (string)v["name"],
                    rect = ToFloats(v["r"]),
                    door = ToFloats(v["door"]),
                    zone = v["zone"] != null ? ToFloats(v["zone"]) : new float[0],
                    indoor = (bool)v["indoor"],
                    open = v["open"] != null && (bool)v["open"],
                    height = (float)v["h"]
                };
                set.places.Add(pd);
            }

            foreach (var kv in (JObject)Read("endings.json"))
            {
                set.endings.Add(new EndingData
                {
                    id = ParseEnding(kv.Key),
                    headline = (string)kv.Value["h"],
                    text = (string)kv.Value["p"]
                });
            }
            return set;
        }

        private static float[] ToFloats(JToken t)
        {
            var list = new List<float>();
            foreach (var x in t) list.Add((float)x);
            return list.ToArray();
        }

        private static PostData ParsePost(JToken p)
        {
            var e = p["effects"];
            return new PostData
            {
                id = p["id"] != null && p["id"].Type != JTokenType.Null ? (string)p["id"] : null,
                category = ParseCategory((string)p["cat"]),
                text = (string)p["text"],
                author = p["author"] != null && p["author"].Type != JTokenType.Null ? (string)p["author"] : null,
                effects = new PostEffects
                {
                    medo = e["medo"] != null ? (float)e["medo"] : 0f,
                    emp = e["emp"] != null ? (float)e["emp"] : 0f,
                    desc = e["desc"] != null ? (float)e["desc"] : 0f,
                    cons = e["cons"] != null ? (float)e["cons"] : 0f,
                    apat = e["apat"] != null ? (float)e["apat"] : 0f
                },
                value = (float)p["value"],
                livraria = p["livraria"] != null ? (float)p["livraria"] : 0f,
                dilemma = p["dilemma"] != null && (bool)p["dilemma"]
            };
        }

        public static PostCategory ParseCategory(string s)
        {
            switch (s)
            {
                case "sens": return PostCategory.Sens;
                case "com": return PostCategory.Com;
                case "ent": return PostCategory.Ent;
                case "pub": return PostCategory.Pub;
                case "loc": return PostCategory.Loc;
            }
            throw new System.ArgumentException("Unknown category " + s);
        }

        public static Trait ParseTrait(string s)
        {
            switch (s)
            {
                case "ansiosa":
                case "ansioso": return Trait.Anxious;
                case "cético": return Trait.Sceptic;
                case "viciado":
                case "viciada": return Trait.Addict;
                case "solidária": return Trait.Solidary;
                case "consumista": return Trait.Consumerist;
                case "leitora": return Trait.Reader;
            }
            throw new System.ArgumentException("Unknown trait " + s);
        }

        public static PlaceId ParsePlace(string s)
        {
            switch (s)
            {
                case "cafe": return PlaceId.Cafe;
                case "mercearia": return PlaceId.Mercearia;
                case "casasN": return PlaceId.CasasN;
                case "junta": return PlaceId.Junta;
                case "livraria": return PlaceId.Livraria;
                case "casasS": return PlaceId.CasasS;
                case "praca": return PlaceId.Praca;
            }
            throw new System.ArgumentException("Unknown place " + s);
        }

        public static EndingId ParseEnding(string s)
        {
            switch (s)
            {
                case "sleep": return EndingId.Sleep;
                case "locked": return EndingId.Locked;
                case "alive": return EndingId.Alive;
                case "balance": return EndingId.Balance;
                case "gentry": return EndingId.Gentry;
            }
            throw new System.ArgumentException("Unknown ending " + s);
        }
    }
}
