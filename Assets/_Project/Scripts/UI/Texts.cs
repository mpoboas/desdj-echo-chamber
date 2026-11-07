namespace EchoChamber
{
    /// <summary>Player-facing Portuguese strings (straight quotes, no em dashes).</summary>
    public static class Texts
    {
        public static string EventText(SimEventKind k)
        {
            switch (k)
            {
                case SimEventKind.MerceariaClosed: return "A Mercearia Central baixou o estore a meio do dia. O dono diz que já não se sente seguro.";
                case SimEventKind.Riot: return "Confusão à porta da Junta: o protesto acabou em gritos e empurrões.";
                case SimEventKind.LivrariaClosed: return "A Letra Viva fechou as portas de vez. Ninguém apareceu.";
                case SimEventKind.Festa: return "A praça encheu: alguém trouxe uma coluna e os vizinhos ficaram a conversar.";
                case SimEventKind.PoliceStarted: return "A PSP começou a patrulhar o bairro.";
                default: return "Os anunciantes estão a fugir! Com esta tensão pagam metade.";
            }
        }

        public static string RadioText(SimEventKind k)
        {
            return k == SimEventKind.PoliceStarted ? "A PSP reforçou a patrulha no bairro depois de várias queixas." : EventText(k);
        }

        /// <summary>Last line of the final report, by how much of the neighbourhood the player understood.</summary>
        public static string Epilogue(float understanding, TuningData t, bool balanceMissed)
        {
            string line = understanding >= t.epilogueHigh
                ? "Conhecias o bairro melhor do que ele se conhecia."
                : understanding >= t.epilogueMid
                    ? "Percebeste parte do bairro: o suficiente para o ler, não para o conhecer."
                    : "Nunca chegaste a saber quem eram. Para ti, o bairro foi sempre um número.";
            if (balanceMissed) line += " Tinhas o equilíbrio ao alcance, mas conheceste o bairro de menos para o conseguires segurar.";
            return line;
        }

        public const string NoInkLine = "A tinta do carimbo OCULTAR acabou neste turno. O que sobrar vai ter de passar.";

        /// <summary>What the boss adds to his morning call when he is imposing posts.</summary>
        public static string PressureLine(int level)
        {
            if (level <= 0) return null;
            return level == 1
                ? "E como ontem ficaste aquém, a empresa vai mandar um post por turno. Esses não se ocultam."
                : "Dois dias seguidos sem a meta. Agora a empresa manda dois posts por turno, e esses não se ocultam.";
        }

        public static string ModeLabel(DecisionMode m)
        {
            switch (m)
            {
                case DecisionMode.Amplify: return "Amplificado";
                case DecisionMode.Hide: return "Ocultado";
                case DecisionMode.Normal: return "Normal";
                default: return "Saiu sozinho";
            }
        }

        public static string TraitDescription(Trait t, string label)
        {
            switch (t)
            {
                case Trait.Anxious: return "o medo pega-lhe forte";
                case Trait.Sceptic: return "não acredita em alarmes";
                case Trait.Addict: return label.EndsWith("a") ? "entretenimento prende-a ao ecrã" : "entretenimento prende-o ao ecrã";
                case Trait.Solidary: return "reage muito a causas da comunidade";
                case Trait.Consumerist: return "anúncios funcionam com ele";
                default: return "tudo o que toca à livraria mexe com ela";
            }
        }

        public static string ActionLabel(ActionKind a)
        {
            switch (a)
            {
                case ActionKind.Casa: return "em casa";
                case ActionKind.Cafe: return "no café";
                case ActionKind.Mercearia: return "às compras";
                case ActionKind.Livraria: return "na livraria";
                case ActionKind.Junta: return "a protestar";
                case ActionKind.Praca: return "na praça";
                default: return "no telemóvel";
            }
        }

        public static readonly string[] TurnNames = { "Manhã", "Tarde", "Noite" };
    }
}
