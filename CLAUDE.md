# Echo Chamber (DESDJ)

Jogo 3D da cadeira DESDJ (ISEP, mestrado), grupo de 2. Tema do enunciado: **Invisible Systems**. Este repositório é o projeto Unity. O jogo foi validado primeiro num protótipo three.js e agora está a ser portado para Unity.

## O jogo em três linhas (design v2, desde 2026-11-08)

O jogador começa o primeiro dia de trabalho numa empresa de tecnologia do Porto: numa cave, escolhe o que cada vizinho de um bairro vê no telemóvel, numa Central de Feeds (parede de ecrãs com o doomscroll de cada pessoa). O chefe só quer tempo na app. O sistema invisível é o algoritmo estilo TikTok: o que prende as pessoas (conteúdo inflamado, o que já pensam) é o que as fecha, irrita e separa, e o jogador só o descobre ao abrir os estores e ver o que o seu feed fez ao bairro.

O design v1 (impressora, carimbos amplificar/ocultar, alavanca, 14 vizinhos, 3 dias x 3 turnos) foi abandonado após playtest. O código v1 ainda existe e é removido por fases (ver `PLAN.md`).

## Leitura obrigatória antes de qualquer trabalho

1. `docs/GDD.md`: **fonte da verdade do design v2** (pilares, loop, sistema invisível, 5 dias, finais). Não contradizer; se for preciso mudar, registar em `docs/DECISIONS.md`.
2. `docs/GDD_Posts.md`: os 39 posts com tema, temperatura, papel e vizinhos a que combinam.
3. `PLAN.md`: fases e checkboxes. Ler no início e marcar passos feitos.
4. `docs/PROGRESS.md` e `docs/DECISIONS.md`: estado e decisões (as entradas de 2026-11-08 explicam o pivot).
5. Referência da simulação, ainda válida: `docs/PrototypeSpec.md` (estados dos vizinhos, utilidade, contágio) e `docs/data/*.json`. As regras de **jogo** deste documento (talões, carimbos, categorias, tensão e coesão como metas) são legado v1.
6. `docs/ScopeDocument.md` (D1, entregue ao professor) descreve o design v1 e não foi alterado. As entregas seguintes (D2.1, D3.1) usam o GDD v2. O tema Invisible Systems e a feature de IA mantêm-se.

## Pilares (não violar)

- **Descobres, não és informado.** Nenhum ecrã diz o que o jogo é nem explica o sistema. O mundo mostra, o jogador conclui. Nunca mostrar estados internos dos vizinhos (bolha, agressividade, etc.) como números: só comportamento visível.
- **Cada post vai para alguém.** Nunca é abstrato: tem destinatário, nome e cara.
- **O que prende é o que fere.** O conteúdo que mais engagement dá tende a ser o que mais prejudica. Essa é a tensão central.
- **Uma coisa nova de cada vez.** Máximo de uma novidade por dia de jogo. Ecrãs com uma ideia principal e no máximo três elementos de texto.
- **O que se vê na rua é a verdade.** A empresa só mostra o tempo na app.
- **Interface diegética.** Telefone, Central de Feeds, secretária, janela, caderno e cortiça.
- **Valores em dados.** Todos os números do jogo ficam em ScriptableObjects editáveis, nunca fixos em código.

## Requisitos da cadeira (têm de ficar cumpridos)

- Unity, 3D. Features básicas: prefabs, modelos, texturas, física, iluminação, animações, áudio, UI.
- Feature avançada: **IA** (decisão por utilidade e contágio social) apresentada com **visualização de dados** (Central de Feeds, quadro de cortiça, relatório final).
- Protótipo final com pelo menos 10 minutos de jogo efetivo (o v2 tem 5 dias, cerca de 12 a 15 minutos).
- Entregas: D2.2 primeiro protótipo jogável, D3.1 protótipo final (ver fases em `PLAN.md`).

## Convenções do projeto

- Língua do jogo e dos textos visíveis ao jogador: **português de Portugal**. Código, nomes de classes e comentários: inglês.
- Sem travessões longos (em dash) nem aspas curvas em textos do jogo. Aspas retas.
- C#, namespace `EchoChamber`. Uma classe por ficheiro. Pastas: `Assets/_Project/{Scripts,Data,Prefabs,Scenes,Art,Audio,UI}`.
- Pipeline: usar o render pipeline que o projeto já tem (esperado URP). Ver `ProjectSettings` antes de assumir.
- Input: usar o sistema de input que o projeto já tem configurado. Jogo v2: rato para tudo (arrastar o post para o feed de um vizinho ou para o lixo, focar um ecrã), Esc volta à vista da mesa. As teclas A/O/N e Espaço são do v1 e saem.
- Lógica separada da apresentação: `Simulation` (C# puro, sem MonoBehaviour onde possível, testável) e `Presentation` (cenas, animação, UI). A simulação tem de poder correr sem cena, para testes e para calibração.
- Determinismo: toda a aleatoriedade da simulação passa por um `System.Random` com semente injetável.
- Git: commits pequenos e frequentes, mensagens em inglês no imperativo. Usar Git LFS para binários (`.glb`, `.png`, `.wav`, `.fbx`). Nunca fazer commit de `Library/`, `Temp/`, `Logs/`, `UserSettings/`.

## Assets

`assets/models` e `assets/room` têm os GLB usados no protótipo (Kenney, KayKit, pmndrs, todos de licença livre). Importar para Unity com um importador de glTF (glTFast) ou converter, e registar a origem em `CREDITS.md`. Objetos centrais da mesa (impressora, carimbos, CRT, mesa de mistura) são construídos no protótipo com primitivas e podem ser reconstruídos em Unity com primitivas e materiais, ou modelados depois.

## Como trabalhar neste projeto

- Ferramentas: o projeto tem as ferramentas de IA do Unity e um servidor Unity MCP. Usar o MCP para criar cenas, objetos, componentes, ler a consola e correr o Play Mode. Verificar sempre a consola do Unity por erros depois de alterar scripts, e corrigir antes de avançar.
- Verificar o trabalho: depois de cada marco, compilar, correr os testes, entrar em Play Mode e confirmar o comportamento. Tirar uma captura da Game View quando o marco for visual.
- Antes de decidir uma regra de jogo, procurar em `docs/GDD.md`. Se não estiver lá, escolher a opção mais simples coerente com os pilares e registar a decisão em `docs/DECISIONS.md` (data, decisão, motivo). Para regras de simulação (estados, utilidade, contágio) ver também `docs/PrototypeSpec.md`.
- Manter `docs/PROGRESS.md` e `PLAN.md` atualizados: o que está feito, o que falta, problemas conhecidos, próximo passo. Quem retomar o trabalho começa por aí.
- Não avançar de fase sem a anterior jogável e testada com alguém. Cortar âmbito antes de cortar sistemas.

## Estado das decisões em aberto

Ver a secção "Em aberto" de `docs/GDD.md`: calibração dos dias e posts de fundo por minuto (só com o jogo a correr). Adiados: posts em vídeo, art bible.
