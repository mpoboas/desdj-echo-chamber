Vais converter o protótipo three.js do nosso jogo "Echo Chamber" para este projeto Unity, de ponta a ponta, até teres um jogo completo e jogável.

## Contexto

Este é o projeto Unity da cadeira DESDJ (ISEP). O jogo, o âmbito e as regras estão já documentados e o professor aprovou o conceito. Não reformules nada. A tua tarefa é implementar.

Começa por ler, por esta ordem, e só depois mexe em código:

1. `CLAUDE.md` (regras do projeto, pilares, convenções)
2. `docs/ScopeDocument.md`
3. `docs/PrototypeSpec.md` (regras e números exatos da simulação)
4. `docs/UnityPortPlan.md` (arquitetura alvo e marcos M0 a M7)
5. `docs/data/*.json` (posts, dias, vizinhos, locais, finais)
6. `prototipo/echo-chamber-3d.html` (o protótipo jogável; lê o código quando a spec não chegar. Abre-o no browser para ver o comportamento)

## Ferramentas

Este projeto tem as novas ferramentas de IA do Unity e o Unity MCP ligados. Começa por listar as ferramentas MCP que tens disponíveis e usa-as para tudo o que for possível: criar e editar cenas, GameObjects, componentes, materiais, prefabs, ler a consola, correr testes, entrar em Play Mode e tirar capturas da Game View. Se uma ferramenta falhar, tenta outra via (editar ficheiros diretamente, scripts de editor) antes de desistir da tarefa.

Verifica sempre a consola do Unity depois de alterar scripts. Compilação limpa e testes verdes antes de avançar de marco.

## O que fazer

Segue os marcos M0 a M7 de `docs/UnityPortPlan.md`, por ordem. Resumo:

- M0: preparar o projeto (estrutura de pastas, `.gitignore`, LFS, assembly definitions, pacotes).
- M1: toda a simulação em C# puro e testável, com os números do protótipo em ScriptableObjects, com testes de unidade e um simulador headless de 3 dias.
- M2: mundo (rua, praça, passadeiras, semáforos, NavMesh, vizinhos com IA de utilidade e reação visível). Importa os GLB de `assets/`.
- M3: sala de controlo com todos os objetos interativos e câmara com vistas.
- M4: um turno completo jogável (Decidir, Publicar, Simular, Jornal), monitor CRT, receita estimada. Este é o primeiro protótipo.
- M5: descoberta (anotar reações, caderno, quadro de cortiça, "bairro percebido", jornal sem causas).
- M6: jogo completo (3 dias, objetivos, 5 finais, relatório final com gráfico, tutorial de dicas, menu e reinício).
- M7: áudio, iluminação por hora do dia, balanceamento com o simulador headless, builds, pelo menos 10 minutos de jogo efetivo.

Faz commit no fim de cada marco (e mais vezes se fizer sentido), com mensagens curtas em inglês no imperativo. Trabalha num branch `feature/unity-port` e não mexas em `main`.

## Como decidir sem me perguntar

- Se a regra está em `docs/PrototypeSpec.md` ou no protótipo, reproduz.
- Se não está, escolhe a opção mais simples coerente com o scope document e os pilares, e regista em `docs/DECISIONS.md` (data, decisão, motivo).
- Se duas leituras do código divergirem, vale o código do protótipo.
- Cortar âmbito (menos polimento, menos conteúdo) é preferível a deixar um sistema por fazer. Os sistemas principais são: ciclo de turno, IA dos vizinhos, descoberta, finais.
- Quando algo visual ficar aquém do protótipo, regista em `docs/PROGRESS.md` como melhoria futura e segue em frente.

Mantém `docs/PROGRESS.md` sempre atualizado (feito, em curso, problemas conhecidos, próximo passo). Se a sessão for interrompida, é daí que se retoma.

## Critério de conclusão

O jogo arranca num menu, joga-se de ponta a ponta (3 dias, 9 turnos) sem erros na consola, os 5 finais são atingíveis (confirmado pelo simulador headless), a descoberta funciona e o "bairro percebido" aparece no fim, e há um build para Windows ou macOS que corre. Escreve em `docs/PROGRESS.md` um resumo final com o estado de cada marco e como correr tudo.

PS: Trabalha durante a noite inteira, sem parar e sem me pedir nenhum input. Não faças perguntas: decide, regista a decisão e continua. Se atingires o limite de uso, espera até ele fazer reset e retoma exatamente de onde ficaste (lê `docs/PROGRESS.md` e o `git log` para te orientares). Só paras quando o critério de conclusão estiver cumprido.
