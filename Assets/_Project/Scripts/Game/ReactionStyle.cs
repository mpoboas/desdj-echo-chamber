using UnityEngine;

namespace EchoChamber
{
    /// <summary>Player-facing look and wording of reactions and categories (Portuguese).</summary>
    public static class ReactionStyle
    {
        public static Color Color(ReactionKind k)
        {
            switch (k)
            {
                case ReactionKind.Medo: return Hex("#FF4F8B");
                case ReactionKind.Raiva: return Hex("#E0473E");
                case ReactionKind.Empatia: return Hex("#4C7DFF");
                case ReactionKind.Compra: return Hex("#D9A441");
                case ReactionKind.Apatia: return Hex("#8C93AB");
                case ReactionKind.Silenciado: return Hex("#7A5CC8");
                default: return Hex("#BDB6A5");
            }
        }

        public static string Glyph(ReactionKind k)
        {
            switch (k)
            {
                case ReactionKind.Medo: return "!";
                case ReactionKind.Raiva: return "#";
                case ReactionKind.Empatia: return "+";
                case ReactionKind.Compra: return "$";
                case ReactionKind.Apatia: return "z";
                case ReactionKind.Silenciado: return "x";
                default: return "..";
            }
        }

        public static string Label(ReactionKind k)
        {
            switch (k)
            {
                case ReactionKind.Medo: return "assustou-se";
                case ReactionKind.Raiva: return "revoltou-se";
                case ReactionKind.Empatia: return "comoveu-se";
                case ReactionKind.Compra: return "quis ir às compras";
                case ReactionKind.Apatia: return "ficou agarrado ao ecrã";
                case ReactionKind.Silenciado: return "ficou calado e zangado";
                default: return "não ligou";
            }
        }

        public static string Short(ReactionKind k)
        {
            switch (k)
            {
                case ReactionKind.Medo: return "medo";
                case ReactionKind.Raiva: return "revolta";
                case ReactionKind.Empatia: return "empatia";
                case ReactionKind.Compra: return "compras";
                case ReactionKind.Apatia: return "scroll";
                case ReactionKind.Silenciado: return "silenciado";
                default: return "ignoram";
            }
        }

        public static Color CategoryColor(PostCategory c)
        {
            switch (c)
            {
                case PostCategory.Sens: return Hex("#FF4F8B");
                case PostCategory.Com: return Hex("#4C7DFF");
                case PostCategory.Ent: return Hex("#F2B705");
                case PostCategory.Pub: return Hex("#9AA0B8");
                default: return Hex("#2FB37E");
            }
        }

        public static string CategoryGlyph(PostCategory c)
        {
            switch (c)
            {
                case PostCategory.Sens: return "!?";
                case PostCategory.Com: return "+";
                case PostCategory.Ent: return ":)";
                case PostCategory.Pub: return "$";
                default: return "i";
            }
        }

        public static string CategoryLabel(PostCategory c)
        {
            switch (c)
            {
                case PostCategory.Sens: return "Sensacionalismo";
                case PostCategory.Com: return "Comunidade";
                case PostCategory.Ent: return "Entretenimento";
                case PostCategory.Pub: return "Publicidade";
                default: return "Notícias locais";
            }
        }

        public static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var c) ? c : UnityEngine.Color.white;
        }
    }
}
