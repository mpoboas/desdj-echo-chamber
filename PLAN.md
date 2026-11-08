# Plano: redesign v2 (Central de Feeds)

Fonte de design: `docs/GDD.md` (v0.2) e `docs/GDD_Posts.md`. Antes de qualquer trabalho, ler ambos.
Regra: um sistema novo por dia de jogo; nada de código sem passar pela fase anterior jogável.

## Fase 0: Design (feito)

- [x] Diagnóstico do protótipo v1 (jogo ilegível, sem objetivo, "aprovar tudo" ganha)
- [x] GDD v0.2 com mecânica "dar um post a um vizinho" e Central de Feeds
- [x] 39 posts de exemplo com papéis (base, armadilha, ponte, pista, ruído)
- [x] Documentação atualizada (CLAUDE.md, DECISIONS.md, PROGRESS.md, banners nos docs legados)

## Fase 1: Núcleo mínimo (só dia 1, 3 vizinhos)

Objetivo: provar que o loop "dar post a alguém, ver a reação" é legível e agrada.

- [ ] Modelo de dados novo: post com tema, temperatura, autor, pessoa mencionada, papel
- [ ] Importar `GDD_Posts.md` para dados (`docs/data/posts.json` novo)
- [ ] Simulação: o jogador escolhe o destinatário do post (hoje a entrega é automática)
- [ ] Simulação: engagement (tempo na app) por pessoa, com bónus de temperatura inflamada
- [ ] Simulação: efeitos escondidos (bolha, agressividade, isolamento) por exposição
- [ ] Central de Feeds: 3 ecrãs com doomscroll, posts de fundo, o nosso post entra no feed
- [ ] Reação visível no feed (pára, reage, passa)
- [ ] Arrastar post para o feed de um vizinho (e lixo)
- [ ] Testes de simulação atualizados e a passar
- [ ] Playtest com alguém externo (3 a 5 minutos de dia 1)

## Fase 2: Os 5 dias

- [ ] Telefone: colega Sara (tutorial) e Dr. Valente (meta e pressão)
- [ ] Estores da janela e revelação ao dia 3
- [ ] Vizinhos por etapas (Rosa, Armando, Tiago, depois Leonor, depois Hugo)
- [ ] Os 6 sinais visíveis na rua (animações ou ícones)
- [ ] Caderno e cortiça (só a partir do dia 3)
- [ ] "Misturar" e posts-ponte (dia 4)
- [ ] Avisos do chefe e finais (dividido, adormecido, cego, despedido, equilíbrio difícil)
- [ ] Playtest dos 5 dias

## Fase 3: Polimento

- [ ] Art bible de uma página (paleta, tipografia, referências Retro Story)
- [ ] UI da Central de Feeds e da secretária
- [ ] Posts em vídeo (clips curtos em loop)
- [ ] Áudio
- [ ] Calibração dos dias (metas, duração, nº de posts de fundo)
- [ ] Build e documentação final

## Notas

- O código v1 (impressora, carimbos, alavanca, mesa de mistura, jornal) é removido por fases, não de uma vez. O que não for usado na fase corrente pode ficar até a fase 2.
- A simulação (utilidade, contágio, NavMesh) mantém-se. É a feature de IA da cadeira.
- ScopeDocument (D1) já foi entregue e descreve o design v1. A entrega D2.1/D3.1 deve usar o GDD v2.
