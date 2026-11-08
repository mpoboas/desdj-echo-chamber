using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace EchoChamber.Editor
{
    /// <summary>Imports docs/data/feed_posts.json and feed_profiles.json (v2) into the FeedContent asset.</summary>
    public static class FeedContentImporter
    {
        public const string AssetPath = "Assets/_Project/Data/FeedContent.asset";

        [MenuItem("Echo Chamber/Import Feed Content (v2)")]
        public static void ImportAll()
        {
            var content = Build();
            var asset = AssetDatabase.LoadAssetAtPath<FeedContentAsset>(AssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<FeedContentAsset>();
                AssetDatabase.CreateAsset(asset, AssetPath);
            }
            asset.content = content;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Debug.Log($"[EchoChamber] Imported {content.posts.Count} feed posts and {content.profiles.Count} profiles.");
        }

        private static JToken Read(string file)
        {
            string root = Path.GetDirectoryName(Application.dataPath);
            return JToken.Parse(File.ReadAllText(Path.Combine(root, ContentImporter.DataDir, file)));
        }

        public static FeedContent Build()
        {
            var set = new FeedContent();
            foreach (var p in Read("feed_posts.json")["posts"])
            {
                var post = new FeedPost
                {
                    id = (int)p["id"],
                    author = (string)p["author"],
                    text = (string)p["text"],
                    theme = ParseTheme((string)p["theme"]),
                    inflamed = (bool)p["inflamed"],
                    role = ParseRole((string)p["role"]),
                    minDay = (int)p["minDay"],
                    overlay = (string)p["overlay"],
                    clip = (string)p["clip"],
                    filler = (bool)p["filler"]
                };
                foreach (var m in p["matches"]) post.matches.Add((string)m);
                foreach (var m in p["mentions"]) post.mentions.Add((string)m);
                set.posts.Add(post);
            }
            foreach (var n in Read("feed_profiles.json")["profiles"])
            {
                var prof = new FeedProfile
                {
                    id = (int)n["id"],
                    key = (string)n["key"],
                    bio = (string)n["bio"],
                    name = (string)n["name"],
                    color = (string)n["color"],
                    home = ContentImporter.ParsePlace((string)n["home"]),
                    startDay = (int)n["startDay"],
                    aggression = (float)n["aggression"],
                    openness = (float)n["openness"],
                    social = (float)n["social"]
                };
                foreach (var i in n["interests"]) prof.interests.Add(ParseTheme((string)i));
                set.profiles.Add(prof);
            }
            foreach (var c in Read("feed_comments.json")["pools"])
            {
                var pool = new FeedCommentPool { key = (string)c["key"] };
                foreach (var l in c["lines"]) pool.lines.Add((string)l);
                set.commentPools.Add(pool);
            }
            var dj = Read("feed_dialogue.json");
            foreach (var d in dj["days"])
            {
                var day = new FeedDialogueDay { day = (int)d["day"], title = (string)d["title"], subtitle = (string)d["subtitle"] };
                foreach (var l in d["pre"]) day.pre.Add(Line(l));
                foreach (var l in d["call"]) day.call.Add(Line(l));
                set.dialogue.days.Add(day);
            }
            foreach (var l in dj["met"]) set.dialogue.met.Add(Line(l));
            foreach (var l in dj["missed"]) set.dialogue.missed.Add(Line(l));
            foreach (var l in dj["fired"]) set.dialogue.fired.Add(Line(l));
            return set;
        }

        private static FeedLine Line(JToken l)
        {
            return new FeedLine { speaker = (string)l["speaker"], text = (string)l["text"] };
        }

        private static FeedTheme ParseTheme(string s)
        {
            switch (s)
            {
                case "meme": return FeedTheme.Meme;
                case "receita": return FeedTheme.Receita;
                case "desporto": return FeedTheme.Desporto;
                case "noticia": return FeedTheme.Noticia;
                case "politica": return FeedTheme.Politica;
                case "polemica": return FeedTheme.Polemica;
                case "anuncio": return FeedTheme.Anuncio;
                case "comunidade": return FeedTheme.Comunidade;
                case "livros": return FeedTheme.Livros;
                case "boato": return FeedTheme.Boato;
                case "opiniao": return FeedTheme.Opiniao;
                case "pessoal": return FeedTheme.Pessoal;
                default: throw new ArgumentException("Unknown theme: " + s);
            }
        }

        private static FeedRole ParseRole(string s)
        {
            switch (s)
            {
                case "base": return FeedRole.Base;
                case "armadilha": return FeedRole.Armadilha;
                case "ponte": return FeedRole.Ponte;
                case "pista": return FeedRole.Pista;
                case "ruido": return FeedRole.Ruido;
                default: throw new ArgumentException("Unknown role: " + s);
            }
        }
    }
}
