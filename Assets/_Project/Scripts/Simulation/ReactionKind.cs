namespace EchoChamber
{
    /// <summary>Visible reaction shown over a neighbour after seeing a post. Order = tie-break order.</summary>
    public enum ReactionKind
    {
        Medo,
        Raiva,
        Empatia,
        Compra,
        Apatia,
        Indiferente,
        /// <summary>Shown by an author whose post the player hid. Caused by the player, not by personality.</summary>
        Silenciado
    }
}
