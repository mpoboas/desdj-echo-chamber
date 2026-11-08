# Echo Chamber: Game Design Document (rascunho v0.3)

Estado: proposta em iteração. [?] marca o que está por decidir. Substitui a v0.1. v0.3 (2026-11-08): LCD 16:9, cansaço dos vizinhos, vários posts no dock, vídeos 2D já feitos (ver `docs/DECISIONS.md`).

## 1. Pitch

Primeiro dia de trabalho numa empresa de tecnologia do Porto. Numa cave, escolhes o que cada pessoa de um bairro vê no telemóvel: memes, receitas, desporto, opiniões, polémicas. O chefe só quer uma coisa: que as pessoas passem mais tempo na app. Ninguém te diz o que isso faz às pessoas. Pela janela, de estores fechados, o bairro continua lá fora.

É o algoritmo do TikTok visto por quem o opera. O jogador nota aos poucos que o que prende as pessoas é o que as fecha, as irrita e as separa.

## 2. Pilares

1. **Descobres, não és informado.** Nenhum ecrã diz o que o jogo é. O jogador chega lá sozinho.
2. **Cada post vai para alguém.** Nunca é uma coisa abstrata. Tem destinatário, nome e cara.
3. **O que prende é o que fere.** O conteúdo que mais engagement dá é, tendencialmente, o que mais prejudica. Essa é a tensão do jogo.
4. **Uma coisa nova de cada vez.** Máximo de uma novidade por dia.
5. **O que vês na rua é a verdade.** Os números da empresa mentem por omissão. As pessoas, não.

## 3. Emoção alvo

Mal-estar que cresce devagar e se torna responsabilidade. Começa leve, quase simpático, acaba com peso. O jogador pode acabar satisfeito, culpado ou frustrado conforme o que escolheu depois de perceber.

## 4. Loop central (60 a 90 segundos por post)

1. Chega um post à secretária, com autor e texto.
2. Tens à frente os perfis dos vizinhos (nome, foto, interesses públicos).
3. **Arrastas o post para o feed de um vizinho**, ou para o lixo.
4. Resposta imediata (menos de 5 segundos): o post aparece no feed dessa pessoa na **Central de Feeds** e ela reage (pára, reage, ou passa). O seu **tempo na app** sobe ou não.
5. 4 a 6 posts por dia. No fim do dia, o chefe liga: bateste ou não a meta.

### A Central de Feeds (ecrã principal do jogo)

O CRT é substituído por um **LCD 16:9** (decidido a 2026-11-08, ver DECISIONS). É a **central de controlo dos feeds do bairro**: **um telemóvel em foco** (grande) e os outros vizinhos em cartões pequenos que só sinalizam eventos (3 no dia 1, até 5 no dia 5). O jogador escolhe quem ver e vê essa pessoa a fazer doomscroll em tempo real: vídeos curtos animados, likes, comentários.

- O feed de cada pessoa corre sozinho com **posts de fundo** gerados pelo sistema (desporto, memes, anúncios, notícias). É o "algoritmo por defeito".
- Os posts que o jogador dá a uma pessoa **entram nesse feed** passados poucos segundos, no meio do ruído, e a pessoa reage à frente dele.
- **A reação é o feedback:** se a pessoa pára a rolar (dwell), reage com um coração ou comenta, o post agradou. Se passa a correr, não. O tempo na app sobe em direto.
- O jogador pode **focar um ecrã** para o ver maior e ler os posts.
- Os interesses do perfil podem estar incompletos ou enganar: a verdade vê-se pelo que a pessoa realmente lê. Observar o doomscroll é uma forma de descobrir.
- **Cansaço (decidido a 2026-11-08):** quem só vê vídeos que não o interessam gasta energia e acaba por pousar o telemóvel, e deixa de contar tempo na app. Um post bem entregue acorda-o e prende-o mais uns vídeos. É por isso que o jogador tem de vigiar os cartões e servir quem está parado.
- **Posts pendentes:** podem estar até 4 na secretária ao mesmo tempo (chegam a horas fixas durante o dia). Um post entregue passa a seguir em menos de 2 s.
- Resolve três problemas de uma vez: feedback imediato, uma razão para o LCD existir e a ligação direta entre o que o jogador dá e o que a pessoa vê.

Risco: cinco feeds a correr ao mesmo tempo podem ser ilegíveis. Mitigação: o ecrã focado e a introdução gradual de vizinhos.

## 5. O sistema invisível (o que o jogador acaba por descobrir)

### Posts
Cada post tem:
- **Tema:** meme, receita/lifestyle, desporto, notícia local, opinião política, polémica/boato, publicidade.
- **Temperatura:** calma ou inflamada.
- **Pessoa mencionada** (opcional): um vizinho que o post cita ou ataca.

### Vizinhos
Cada um tem:
- **Interesses públicos** (visíveis no perfil): o que gosta de ver.
- **Traços escondidos:** agressividade, abertura a ideias diferentes, necessidade social.

### Engagement (o que a empresa vê)
- Conteúdo que combina com os interesses da pessoa rende tempo na app.
- **Conteúdo inflamado rende mais que o calmo, mesmo fora dos interesses.** Esta é a armadilha: a empresa premeia a polémica.
- Repetir sempre o mesmo tipo rende cada vez mais, mas as pessoas vão perdendo diversidade.

### Efeitos escondidos (nunca mostrados como números)
- **Bolha:** quanto mais uma pessoa só vê o que já pensa, menos fala com quem pensa diferente.
- **Agressividade:** conteúdo inflamado endurece-a e torna-a mais frontal.
- **Isolamento:** memes e conteúdo passivo tiram-na de casa menos vezes.
- **Contágio:** pessoas no mesmo sítio influenciam-se (já existe na simulação).

### O que o jogador vê (sinais legíveis, máx. 6)
Cada efeito tem um comportamento visível e inequívoco, sempre o mesmo:
- Fica em casa o dia todo (isolamento).
- Anda de cabeça baixa, a olhar para o telemóvel (apatia).
- Discute na praça (agressividade).
- Evita um vizinho específico (bolha).
- Visita outros e sorri (saudável).
- Vai ao café (social).

Esta lista é fixa e aprende-se. Se o jogador não conseguir descrever o que viu, falhámos.

## 6. Objetivo em três camadas

- **Imediato:** decidir o que dar a quem.
- **Do dia:** atingir a meta de tempo na app do Dr. Valente (uma barra simples, sem mais números).
- **Da história:** perceber o que o feed faz às pessoas e decidir se continuas a servir o chefe ou ao bairro.

**Porque é que "dar tudo o que prende" deixa de ser suficiente:** a partir do dia 4 a meta do chefe só se atinge continuando a inflamar. Mas isso desfaz as pessoas, e os finais refletem-no. O jogador que lê e investiga encontra caminhos alternativos (misturar conteúdo, quebrar uma bolha dando a uma pessoa um post de quem pensa diferente), com custo em engagement.

## 7. Falhar

Nunca há "game over". Falhar a meta dá um **aviso** do chefe. Acumular avisos tira ferramentas ou liberdade (o chefe passa a escolher alguns posts por ti). Três avisos até ao fim levam ao final "Despedido".

## 8. Personagens

- **Jogador:** sem rosto, sem nome.
- **Colega [?nome]:** guia do dia 1, simpática, humana. Ambígua: ajuda-te, mas também tem medo de perder o emprego. [?] decidir mais tarde para que lado pende.
- **Dr. Valente:** chefe, voz ao telefone. Fala só de números, nunca de pessoas. Simpático por fora, implacável por dentro. Não é caricatura soviética: é o gestor moderno de sorriso fácil.
- **Vizinhos em destaque (chegam por etapas):**

| Entra | Vizinho | Traço público | Papel narrativo [proposta] |
|---|---|---|---|
| Dia 1 | **Dona Rosa** | solidária, receitas, comunidade | Representa o lado humano do bairro. Cai quando a isolam. |
| Dia 1 | **Sr. Armando** | cético, política local, notícias | Facilmente radicalizável por conteúdo inflamado. |
| Dia 1 | **Tiago** | viciado em telemóvel, memes, desporto | Quem mais engagement dá e mais se perde. |
| Dia 2 | **Leonor** | leitora, livros, cultura | Frequenta a livraria. A bolha dela é a mais subtil. |
| Dia 4 | **Hugo** | consumista, publicidade | O elo entre bolhas: se se fecha, quebra o bairro. |

Os restantes vizinhos do bairro ficam como figurantes: dão vida à rua, mas o jogador nunca os decide.

## 9. Estrutura: 5 dias, ~12 a 15 minutos

Cada dia introduz uma única novidade.

| Dia | Vizinhos | Novidade | Momento narrativo | Estado do jogador |
|---|---|---|---|---|
| 1 | Rosa, Armando, Tiago | O telefone toca. A colega ensina "dar o que a pessoa gosta". CRT com tempo na app. **Estores fechados.** | Tudo parece simples e bom. | Inocente |
| 2 | + Leonor | Valente pede mais. Aparecem posts **inflamados** que rendem muito mais. | O jogador repara que a polémica "rende". | Satisfeito |
| 3 | os mesmos 4 | **Os estores abrem.** O jogador vê o bairro. Desbloqueia o caderno/cortiça. | Revelação: o que o jogador deu é o que as pessoas fizeram. | Revelação |
| 4 | + Hugo | Valente aperta. Pode **misturar** conteúdo entre pessoas (quebrar uma bolha). | Primeiro dilema real: engagement ou bairro. | Investigador |
| 5 | os 5 | Meta final impossível sem inflamar. | O jogador escolhe quem sacrifica. | Responsável |

## 10. Finais

Dependem do estado do bairro **e do que o jogador percebeu**.

- **Bairro dividido:** inflamou até ao fim, as pessoas já não se falam.
- **Bairro adormecido:** só memes e conteúdo passivo, ninguém sai de casa.
- **Cego:** cumpriu as metas sem nunca perceber o que fez.
- **Despedido:** quebrou as metas por proteger o bairro, perdeu o emprego, o bairro sobrevive.
- **Equilíbrio difícil:** percebeu o sistema e encontrou um meio-termo, a custo.

## 11. Ferramentas e como se ligam ao sistema

| Objeto | Função |
|---|---|
| Telefone | Chefe e colega. Substitui o ecrã de boas-vindas. |
| Secretária + posts | O post atual e os perfis dos vizinhos. |
| CRT / Central de Feeds | Parede de ecrãs com o feed de cada vizinho em direto. Ver secção 4. Sem mapa do bairro: a rua vê-se pela janela. |
| Janela | Começa fechada. É onde se vê o efeito. |
| Caderno e cortiça | Desbloqueia ao dia 3. Serve para anotar o que se aprende de cada pessoa (o que gosta, o que lhe faz mal, com quem fala). |
| Impressora, carimbos, alavanca, mesa de mistura | **Saem.** Não têm função neste modelo. |

## 12. Tom e arte

- **Tom:** humor seco, frases curtas. Tom de empresa simpática e vazia, nunca opressivo nem soviético.
- **Visual:** referência *Retro Story*. Sala quente e analógica (madeira, luz baixa) contra uma app limpa e fria no ecrã. Esse contraste conta a história.
- Art bible de uma página (paleta, tipografia, referências) fechada antes de qualquer UI.
- Cada ecrã: uma ideia principal e no máximo três elementos de texto.

## 13. O que a base técnica atual aproveita

- **Simulação:** estados, contágio, utilidade, rotinas e NavMesh ficam e passam a ser a razão do jogo funcionar.
- **Muda:** a entrega de posts passa a ser escolhida pelo jogador (hoje é automática). A cena precisa de 3 a 5 personagens legíveis, não 14 amontoados.
- **Refazer:** tutorial, UI toda, jornal, cortiça, cortes de câmara.
- **Escrever:** colega, Valente e os 5 vizinhos, cerca de 30 a 40 posts.

## 14. Riscos

- Escrita de posts e personagens é o trabalho mais pesado.
- Os 6 sinais visíveis têm de ser claros a 1 metro de câmara: precisa de animações ou ícones por pessoa.
- Equilibrar engagement contra dano para ninguém poder ganhar de olhos fechados.
- Playtest cedo, com alguém externo, no fim de cada semana.

## 15. Adiado (pinned, só depois de fechar o design e os posts)

- ~~Posts em vídeo~~ **Feito** (2026-11-08, a pedido do dono do projeto): 11 clips 2D animados em loop, com texto por cima. Ver `docs/DECISIONS.md`.
- **Art bible** de uma página (paleta, tipografia, referências *Retro Story*, UI da Central de Feeds).
- **Calibração dos dias** (metas, número de posts, duração). Só se faz com playtest real.

## 16. Em aberto

Decididas (2026-11-08, por delegação do dono do projeto):

- **Colega:** fica ambígua. Ajuda, mas tem medo de perder o emprego. O destino decide-se na escrita narrativa.
- **Misturar:** é o mesmo gesto de arrastar. Dar a uma pessoa um post de quem pensa diferente conta como misturar.
- **Limite de consciência:** o lixo existe e tem custo para o chefe (post descartado = menos tempo na app).
- **Mapa do bairro no CRT:** não há. O CRT é a Central de Feeds.

- **Nome da colega:** Sara.
- **Posts:** lista completa de 39 em `docs/GDD_Posts.md`, com papéis (base, armadilha, ponte, pista, ruído).

Por decidir (só com o jogo a correr):

1. Calibração dos dias e duração por dia.
2. Quantos posts de fundo por minuto em cada feed, para ser legível.
