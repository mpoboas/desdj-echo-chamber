# Proposta (por implementar): alfinetes no quadro de cortiça

Estado: **aprovada em princípio pelo utilizador em 2026-11-06, adiada para uma próxima sessão.** As três decisões abaixo ainda não foram confirmadas (as recomendações são o ponto de partida). Implementar só depois do ok explícito. Ponto 2 da revisão de design.

## Problema
O jogador descobre quem é ansioso, quem é cético e quem anda com quem, mas não tem nenhuma forma de usar isso. As únicas alavancas são o carimbo por post e os faders por categoria, que afetam o bairro todo por igual. O caderno e o quadro de cortiça são decoração.

## Mecânica
Alfinetes espetados nos cartões dos vizinhos no quadro de cortiça:

- **Resguardo (azul):** o vizinho deixa de ver posts (probabilidade de ver = 0). Protege quem é frágil.
- **Destaque (vermelho):** o vizinho vê mais posts (probabilidade x1,8, mesmo teto de 97% que já existe). Dirige conteúdo a quem o aproveita (publicidade a um consumista rende mais).
- Número limitado de alfinetes ao mesmo tempo.
- Clicar num cartão com o quadro em foco alterna: sem alfinete, resguardo, destaque, sem alfinete.

Porque funciona: o conhecimento passa a servir (proteger o ansioso do sensacionalismo, expor o consumista a anúncios); aprofunda o lucro contra bairro (destaque rende, resguardo custa receita); as relações contam (proteger o centro de um grupo trava o contágio, como os fios vermelhos ensinam); continua diegético e não mostra estados internos.

## Mudanças necessárias
- Simulação: modo de feed por vizinho (normal, resguardado, destaque) a entrar no cálculo de `PostDelivery.ViewProbability`. A estimativa de receita do talão (`EstimateRevenue`) tem de contar com os alfinetes.
- Quadro de cortiça (`CorkBoardView`): hoje é um desenho sem cliques (canvas em world space). Cada cartão precisa de um collider com `RoomClickable` (nova `ClickAction`) e de um alfinete desenhado.
- Política "informada" no simulador (`PolicyLibrary`) que usa alfinetes, para medir o efeito.
- Tutorial/dica a explicar os alfinetes.

## Decisões em aberto (recomendação primeiro)
1. Tipos: resguardo e destaque (recomendado) ou só resguardo.
2. Quantos ao mesmo tempo: 3 (recomendado), 2 ou 4.
3. Duração: ficam até serem tirados (recomendado) ou limpam-se a cada turno.

## Critério de sucesso (balance report)
- `all-amplify` com resguardo dos 3 vizinhos mais stressados deve acabar em "Locked" em menos de 60% das partidas (hoje 79% sem alfinetes), perdendo menos de 15% de receita.
- Salvaguarda: alfinetes ao acaso (sem conhecimento) não devem ser melhores do que usar bem a informação.

## Riscos
- UI: o quadro nunca foi interativo, é a parte mais trabalhosa; validar em Play Mode.
- Fricção se os alfinetes se repuserem a cada turno (9 turnos).
- Se forem fortes demais, `all-amplify` deixa de falhar e o jogo perde tensão; ajustável em dados (`TuningData`).
