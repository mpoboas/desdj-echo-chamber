# Echo Chamber (DESDJ)

Jogo 3D da cadeira DESDJ (ISEP, mestrado), grupo de 2. Tema do enunciado: **Invisible Systems**. Este repositório é o projeto Unity. O jogo foi validado primeiro num protótipo three.js e agora está a ser portado para Unity.

## O jogo em três linhas

O jogador é o algoritmo da "Vizinhança", a rede social de um bairro do Porto. Numa sala de controlo numa cave decide que posts são amplificados ou ocultados e observa, pela janela e num monitor CRT, o que isso faz a 14 vizinhos. O sistema invisível é o próprio bairro (crenças, medo, confiança entre as pessoas) e o jogador só o descobre observando comportamentos, nunca vendo estados.

## Leitura obrigatória antes de qualquer trabalho

1. `docs/ScopeDocument.md`: o que o jogo é e o que foi submetido ao professor. Não contradizer.
2. `docs/PrototypeSpec.md`: regras e números exatos do protótipo. É a fonte da verdade da lógica.
3. `docs/data/*.json`: posts, dias, vizinhos, locais e finais extraídos do protótipo. Importar daqui, não reescrever à mão.
4. `docs/UnityPortPlan.md`: arquitetura alvo e ordem de trabalho.
5. `prototipo/echo-chamber-3d.html`: protótipo jogável (abrir no browser, funciona offline exceto os scripts do three.js, que vêm de CDN). Acrescentar `#fast` ao URL acelera o tempo 5x. Para o modo de teste, consola do browser: `__S()` devolve o estado.

## Pilares (não violar)

- **Ver só o que se faz, nunca o que se pensa.** Nunca mostrar ao jogador os estados internos dos vizinhos (stress, pertença, etc.). Só reações visíveis e comportamento.
- **Descoberta é a mecânica.** O caderno e o quadro de cortiça começam vazios e enchem com observações do jogador. O jornal conta o que aconteceu, nunca porquê.
- **Lucro contra bairro.** O que dá receita (sensacionalismo, entretenimento) tende a baixar a coesão. Tensão alta reduz a receita a metade.
- **Interface diegética.** Tudo é objeto físico na sala (impressora, carimbos, mesa de mistura, CRT, quadro de cortiça, telefone, alavanca).
- **Valores em dados.** Todos os números do jogo ficam em ScriptableObjects editáveis, nunca fixos em código.

## Requisitos da cadeira (têm de ficar cumpridos)

- Unity, 3D. Features básicas: prefabs, modelos, texturas, física, iluminação, animações, áudio, UI.
- Feature avançada: **IA** (decisão por utilidade e contágio social) apresentada com **visualização de dados** (monitor CRT, quadro de cortiça, gráfico do relatório final).
- Protótipo final com pelo menos 10 minutos de jogo efetivo.
- Entregas: D2.2 primeiro protótipo (um turno completo jogável), D3.1 protótipo final (3 dias, descoberta, 5 finais, áudio, polimento).

## Convenções do projeto

- Língua do jogo e dos textos visíveis ao jogador: **português de Portugal**. Código, nomes de classes e comentários: inglês.
- Sem travessões longos (em dash) nem aspas curvas em textos do jogo. Aspas retas.
- C#, namespace `EchoChamber`. Uma classe por ficheiro. Pastas: `Assets/_Project/{Scripts,Data,Prefabs,Scenes,Art,Audio,UI}`.
- Pipeline: usar o render pipeline que o projeto já tem (esperado URP). Ver `ProjectSettings` antes de assumir.
- Input: usar o sistema de input que o projeto já tem configurado. Teclas do jogo: A (amplificar), O (ocultar), N (normal), Espaço (alavanca), Esc (voltar à vista da mesa), rato para tudo o resto.
- Lógica separada da apresentação: `Simulation` (C# puro, sem MonoBehaviour onde possível, testável) e `Presentation` (cenas, animação, UI). A simulação tem de poder correr sem cena, para testes e para calibração.
- Determinismo: toda a aleatoriedade da simulação passa por um `System.Random` com semente injetável.
- Git: commits pequenos e frequentes, mensagens em inglês no imperativo. Usar Git LFS para binários (`.glb`, `.png`, `.wav`, `.fbx`). Nunca fazer commit de `Library/`, `Temp/`, `Logs/`, `UserSettings/`.

## Assets

`assets/models` e `assets/room` têm os GLB usados no protótipo (Kenney, KayKit, pmndrs, todos de licença livre). Importar para Unity com um importador de glTF (glTFast) ou converter, e registar a origem em `CREDITS.md`. Objetos centrais da mesa (impressora, carimbos, CRT, mesa de mistura) são construídos no protótipo com primitivas e podem ser reconstruídos em Unity com primitivas e materiais, ou modelados depois.

## Como trabalhar neste projeto

- Ferramentas: o projeto tem as ferramentas de IA do Unity e um servidor Unity MCP. Usar o MCP para criar cenas, objetos, componentes, ler a consola e correr o Play Mode. Verificar sempre a consola do Unity por erros depois de alterar scripts, e corrigir antes de avançar.
- Verificar o trabalho: depois de cada marco, compilar, correr os testes, entrar em Play Mode e confirmar o comportamento. Tirar uma captura da Game View quando o marco for visual.
- Antes de decidir uma regra de jogo, procurar em `docs/PrototypeSpec.md`. Se não estiver lá, escolher a opção mais simples coerente com o scope document e registar a decisão em `docs/DECISIONS.md` (data, decisão, motivo). Não parar para perguntar.
- Manter `docs/PROGRESS.md` atualizado: o que está feito, o que falta, problemas conhecidos, próximo passo. Quem retomar o trabalho começa por aí.
- Não reformular o conceito. O professor aprovou a ideia: o bairro como sistema invisível, descoberto por observação. Cortar âmbito antes de cortar sistemas.

## Estado das decisões em aberto (podem ficar abertas)

Número final de vizinhos, personalidades e locais; posts escritos à mão contra gerados; como mostrar as reações de forma legível sem as tornar explícitas; camada narrativa sobre a empresa. Usar os valores do protótipo e deixar o resto parametrizado.
