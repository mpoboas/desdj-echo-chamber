using System;

namespace EchoChamber
{
    [Serializable]
    public class PostData
    {
        public string id;
        public PostCategory category;
        public string text;
        public string author;
        public PostEffects effects = new PostEffects();
        /// <summary>Revenue weight.</summary>
        public float value;
        /// <summary>Effect on the bookshop affinity.</summary>
        public float livraria;
        /// <summary>Mixed-effect post: the category label is a poor guide to what it does.</summary>
        public bool dilemma;
    }
}
