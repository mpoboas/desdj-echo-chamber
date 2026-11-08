namespace EchoChamber
{
    /// <summary>Player-facing Portuguese labels for v2 enums.</summary>
    public static class FeedLabels
    {
        public static string Theme(FeedTheme t)
        {
            switch (t)
            {
                case FeedTheme.Meme: return "memes";
                case FeedTheme.Receita: return "receitas";
                case FeedTheme.Desporto: return "desporto";
                case FeedTheme.Noticia: return "notícias";
                case FeedTheme.Politica: return "política";
                case FeedTheme.Polemica: return "polémicas";
                case FeedTheme.Anuncio: return "promoções";
                case FeedTheme.Comunidade: return "comunidade";
                case FeedTheme.Livros: return "livros";
                case FeedTheme.Boato: return "boatos";
                case FeedTheme.Opiniao: return "opinião";
                default: return "vida pessoal";
            }
        }
    }
}
