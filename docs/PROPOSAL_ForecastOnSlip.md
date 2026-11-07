# Proposta (por implementar): previsão no talão

Estado: **proposta do ponto 3 da revisão de design, parte funcional, adiada em 2026-11-06.** O utilizador escolheu implementar só o epílogo e a condição do "Equilíbrio raro" (feitos), e guardar esta previsão para depois, idealmente junto com os alfinetes (`docs/PROPOSAL_PinsOnCorkBoard.md`), que lhe dão utilidade direta. Implementar só depois de ok explícito.

## Problema
Com o "Bairro percebido" a condicionar um final, a descoberta passa a ser uma tarefa para desbloquear o final, mas continua sem ajudar a decidir melhor durante o jogo.

## Mecânica
O talão mostra, além do padrão da categoria ("observado: medo x3"), **quem vai sentir mais** o post: "pesa em: Marta, Rui".

- Usa só o que o jogador já conquistou: o padrão da categoria anotado (`DiscoveryModel.TopPattern`, precisa de 2 ou mais observações) e as personalidades já descobertas (`TraitKnown`).
- Regra: se a reação dominante da categoria é K e a assinatura da personalidade descoberta do vizinho é K (`SignatureOf`: ansioso/medo, solidária/empatia, consumista/compra, viciado/apatia), o vizinho aparece na lista. O cético descoberto aparece como "não liga" nos posts de medo.
- Vizinhos por descobrir e categorias com menos de 2 observações não aparecem. A previsão cresce com o caderno e não revela nada que o jogador não tenha ganho (não usa os efeitos escondidos do post).
- Máximo de 3 nomes.

## Mudanças necessárias
1. Simulação pura: `DiscoveryModel.Forecast(PostCategory, vizinhos)` com testes.
2. Talão (`TurnManager.PatternText` e `SlipView`): mostrar a linha "pesa em: ...".
3. Aviso de tutorial na primeira vez ("vai crescendo com o teu caderno").

## Critério de sucesso (balance report)
Uma política "informada" decide os carimbos usando a previsão (oculta o que pesa em vizinhos descobertos, amplifica o resto), com os traços conhecidos. Uma política "às cegas" usa a mesma regra sem informação. A informada deve melhorar o "Equilíbrio raro" em pelo menos 10 pontos ou baixar o "Locked" em pelo menos 10 pontos. O resto do balance report não pode mudar.

## Riscos
- O jogador pode nunca ver a previsão (precisa de 2 observações da categoria e de uma personalidade descoberta).
- Padrões baseados em poucas observações podem induzir em erro; é inferência, faz parte do jogo.
