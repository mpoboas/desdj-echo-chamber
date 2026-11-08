# Plano: redesign v2 (Central de Feeds)

Fonte de design: `docs/GDD.md` (v0.2) e `docs/GDD_Posts.md`. Antes de qualquer trabalho, ler ambos.
Regra: um sistema novo por dia de jogo; nada de código sem passar pela fase anterior jogável.

## Fase 0: Design (feito)

- [x] Diagnóstico do protótipo v1 (jogo ilegível, sem objetivo, "aprovar tudo" ganha)
- [x] GDD v0.2 com mecânica "dar um post a um vizinho" e Central de Feeds
- [x] 39 posts de exemplo com papéis (base, armadilha, ponte, pista, ruído)
- [x] Documentação atualizada (CLAUDE.md, DECISIONS.md, PROGRESS.md, banners nos docs legados)

## Referência visual (HTML)

`docs/prototypes/echo_chamber.html` e `central_feeds.html` são só referência visual da Central de Feeds (layout, cenas 2D, ritmo). Não são um passo do plano: o jogo faz-se em Unity, pela Fase 1.

## Fase 1: Núcleo mínimo (só dia 1, 3 vizinhos)

Objetivo: provar que o loop "dar post a alguém, ver a reação" é legível e agrada.

- [x] Modelo de dados novo: `FeedPost`, `FeedProfile`, `FeedContent` (Simulation/)
- [x] Dados: `docs/data/feed_posts.json` (39) e `feed_profiles.json` (5); menu `Import Feed Content (v2)`
- [x] `FeedSystem.Give/Discard` (destinatário escolhido pelo jogador)
- [x] Engagement por pessoa (interesse, inflamado, repetição) em `FeedSystem`
- [x] Efeitos escondidos + `Signal()` (6 sinais visíveis); falta ligar ao UtilityBrain/NPCs
- [x] Central de Feeds em UI Toolkit (`CentralFeedsView`, `FeedPhoneView`, `TileView`, `CentralFeedsView`, `UI/CentralFeeds.uss`), no LCD 16:9 da sala (cena `Game.unity`, menu `Build Game Scene (v2)`). Vídeos 2D em `Art/Videos/feed_*.mp4` (11 clips) com `FeedClipCatalog`. Cena `Feed.unity` e uGUI antiga apagadas
- [x] Reação visível: anel nos cartões, coração, comentários, "pousou o telemóvel" (simulação `FeedViewer`)
- [x] Arrastar post para vizinho ou lixo e clicar nos cartões: o rato real é apanhado em `FeedGameController.PollMouse` (painéis UI Toolkit em RenderTexture não recebem eventos do EventSystem). Verificado com estado de rato injetado (focar Armando, arrastar para o Tiago)
- [x] Testes: `FeedSystemTests` (13) e `FeedDayTests` (7) a passar (98 no total). `FeedDay`/`FeedCareer`: fila de posts, meta, avisos, despedido. Testes v1 ficam até o código v1 sair
- [x] Dia 1 jogável de ponta a ponta no `Game.unity`: cartão do dia, Sara, telefone a tocar (clicar no telefone ou em "Atender"), Valente, dia a correr, chamada de fim de dia, resumo e avisos, e passagem ao dia 2 (verificado com rato injetado; diálogos em `docs/data/feed_dialogue.json`)
- [x] Código v1 de apresentação removido (impressora, carimbos, alavanca, mesa de mistura, tubo, jornal, HUD e fluxo de turnos, CRT, builders e cena Main). Ficam a rua, os vizinhos, a simulação v1 e a cortiça para a Fase 2
- [ ] Playtest com alguém externo (3 a 5 minutos de dia 1)

## Fase 2: Os 5 dias

- [x] Telefone: colega Sara (tutorial) e Dr. Valente (meta e pressão): feito na Fase 1 (dia 1, e falas dos dias 2 a 5 em `docs/data/feed_dialogue.json`)
- [~] Estores da janela e revelação ao dia 3: estores fechados feitos; falta abrir ao dia 3 e mostrar a rua
- [x] Vizinhos por etapas (Rosa, Armando, Tiago, depois Leonor, depois Hugo): feito na Fase 1
- [ ] Os 6 sinais visíveis na rua (animações ou ícones)
- [ ] Caderno e cortiça (só a partir do dia 3)
- [ ] "Misturar" e posts-ponte (dia 4)
- [~] Avisos do chefe e finais (dividido, adormecido, cego, despedido, equilíbrio difícil): avisos e despedido feitos; faltam os outros finais
- [ ] Playtest dos 5 dias

## Fase 3: Polimento

- [ ] Art bible de uma página (paleta, tipografia, referências Retro Story)
- [ ] UI da Central de Feeds e da secretária
- [x] Posts em vídeo (clips curtos em loop): feito na Fase 1, falta só regravar a mais resolução se for preciso
- [ ] Áudio
- [ ] Calibração dos dias (metas, duração, nº de posts de fundo)
- [ ] Build e documentação final

## Notas

- O código v1 (impressora, carimbos, alavanca, mesa de mistura, jornal) é removido por fases, não de uma vez. O que não for usado na fase corrente pode ficar até a fase 2.
- A simulação (utilidade, contágio, NavMesh) mantém-se. É a feature de IA da cadeira.
- ScopeDocument (D1) já foi entregue e descreve o design v1. A entrega D2.1/D3.1 deve usar o GDD v2.
