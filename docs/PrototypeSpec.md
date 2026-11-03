# Especificação do protótipo (fonte da verdade para a conversão)

Este documento descreve o que o protótipo three.js (`prototipo/echo-chamber-3d.html`) faz, com os números exatos. A conversão para Unity deve reproduzir estas regras. Quando este ficheiro e o código do protótipo discordarem, vale o código do protótipo, e este ficheiro deve ser corrigido.

Os valores são o ponto de partida e vão ser calibrados. Mantê-los todos em dados editáveis (ScriptableObjects), nunca espalhados pelo código.

Unidades de tempo: segundos de jogo. O protótipo usa um mundo 2D de 1000 x 640 para a lógica dos vizinhos. Em Unity usa-se um mundo 3D real, por isso as distâncias devem ser convertidas (ver "Conversão de espaço" no fim).

## 1. Estrutura do jogo

- 3 dias, 3 turnos por dia (Manhã, Tarde, Noite). Turno = Decidir, Publicar (alavanca), Simular, Jornal.
- Fase Decidir: a impressora imprime um talão de cada vez para o suporte de leitura. O jogador carimba AMPLIFICAR (tecla A), OCULTAR (O) ou envia para o cesto NORMAL (N). Pode mexer nos faders da mesa de mistura.
- Tempo de decisão: ilimitado no primeiro turno do primeiro dia. Nos outros, `10 + 12 × (número de talões)` segundos. O que não for decidido sai como "auto" (conta como NORMAL).
- Publicar: alavanca (Espaço). Os talões não decididos saem automaticamente.
- Fase Simular: 26 segundos. Os posts são entregues aos vizinhos de forma escalonada, os vizinhos reagem e agem. O jogador observa e clica nos vizinhos para anotar.
- Jornal no fim do turno. Mostra o que foi publicado, quantos vizinhos viram, receita, acontecimentos e variação de coesão e tensão. Não mostra as causas.
- Entre dias: os estados regressam parcialmente ao repouso (secção 3).

## 2. Posts

Categorias (cor): Sensacionalismo `sens` #FF4F8B, Comunidade `com` #4C7DFF, Entretenimento `ent` #F2B705, Publicidade `pub` #9AA0B8, Notícias locais `loc` #2FB37E.

Cada post tem: categoria, texto, autor, efeitos escondidos `t` (medo, emp, desc, cons, apat; pode ser negativo), valor `v` (peso na receita) e opcionalmente `liv` (efeito sobre a livraria).

A lista completa está em `data/posts.json`. Resumo do formato:

```json
{ "id": "liv2", "cat": "sens", "text": "...", "author": "Hugo",
  "effects": { "medo": 1, "desc": 4 }, "value": 1.8, "livraria": 0 }
```

Posts "pool" (aleatórios) têm autor só quando são de uma entidade (Junta, MegaShop, etc.); os restantes ficam sem autor e o jogo atribui um vizinho ao imprimir. Posts "scripted" (ids t1..t3, liv1..liv5, r1..r6) têm autor e entram em turnos específicos (ver `data/days.json`).

Seleção de um post aleatório (`pickPost`): escolhe uma categoria por sorteio ponderado e depois um post dessa categoria que não tenha saído nos últimos 9. Pesos:

```
sens = 0.20 + 0.50 × stressMédio + 0.35 × desconfiançaMédia
com  = 0.18 + 0.30 × pertençaMédia
ent  = 0.18 + 0.45 × apatiaMédia
pub  = 0.16
loc  = 0.12
```

O jogo reage ao bairro: bairro assustado produz mais sensacionalismo, bairro apático mais entretenimento.

## 3. Vizinhos (agentes)

14 vizinhos. Lista em `data/neighbours.json`: nome, personalidade, cor. Metade mora nas casas do norte (`casasN`) e metade no prédio do fundo (`casasS`), alternando por índice.

### Estados (0 a 1)

| Estado | Inicial (sorteio) | Repouso | Retorno | Notas |
|---|---|---|---|---|
| stress | 0.16 a 0.30 | 0.22 | 0.8%/s da distância | acima de 0.68 anda mais depressa e postura tensa; acima de 0.7 velocidade x1.4 |
| pert (pertença) | 0.42 a 0.62 | nenhum | nenhum | |
| desc (desconfiança) | 0.08 a 0.22 | 0.13 | 0.4%/s | |
| apat (apatia) | 0.12 a 0.28 | 0.20 | 0.4%/s | |
| compra | 0.20 a 0.60 | nenhum | sobe +0.8%/s | desce 0.45 numa ida às compras |
| livAff | 0.10 (leitor 0.50) | mínimo 0.05 (leitor 0.35) | desce 0.4%/s até ao mínimo | |
| phone | 0.40 a 0.75 (viciado 0.85 a 0.98) | fixo | | probabilidade base de ver posts |

Efeitos contínuos por local, por segundo, enquanto o vizinho está no local:
- Café: pert +1.2%, stress −0.7%.
- Praça: stress −1.2%, pert +0.5%.
- Em casa: stress −1%.
- Ao ecrã (ação "ecra"): apat +0.7%, pert −0.3%.
- Na Junta: desc +0.4%.
- Livraria (dia 2 em diante, durante a simulação): sobe o anel da livraria 0.09 e pert +0.04 por visita.
- Com polícia na rua e o vizinho fora de um edifício: stress +0.7%/s.

Fim do dia: stress, desc e apat aproximam-se do repouso (stress 30% da distância, desc 15%, apat 15%). A polícia sai. A mercearia reabre se o stress médio estiver abaixo de 0.45.

### Personalidades

`trait` altera a reação aos posts (ver secção 4) e a assinatura de reação usada no caderno:

| Personalidade | Efeito no post | Reação típica |
|---|---|---|
| ansiosa / ansioso | medo x1.5 | medo |
| cético | medo x0.55, desc x0.5 | indiferente |
| viciado / viciada | apat x1.4; propensão alta ao telemóvel | apatia |
| solidária | emp x1.4 | empatia |
| consumista | cons x1.6 | compra |
| leitora | emp x1.5 em posts da livraria; livAff inicial alto | empatia |

### Decisão (IA de utilidade)

Cada vizinho decide quando o tempo de permanência (`dwell`) acaba. Dwell: 7 a 12 s em casa, 5 a 9 s nos outros sítios, 5 a 8 s ao ecrã. Quando recebe um post tem o dwell reduzido a 0.4 a 1.4 s, para reagir depressa.

Utilidade de cada ação (`night` = 0.55 no turno da noite, senão 0):

```
casa      = 0.22 + 0.9×stress + night + (ansioso ? 0.3×stress : 0)
cafe      = 0.20 + 0.7×pert + 0.3×(1−stress) − 0.35×apat
mercearia = mercOpen ? 0.05 + 1.1×compra : −1
livraria  = livOpen  ? 0.02 + 0.35×pert + 0.95×livAff − 0.4×apat : −1
junta     = −0.45 + 1.35×desc + 0.35×stress − 0.3×apat
praca     = 0.12 + 0.45×(1−stress) + 0.25×pert − (polícia ? 0.15 : 0) − 0.6×night + (festa ativa ? 0.4 : 0)
ecra      = −0.12 + 1.15×apat + (viciado ? 0.15 : 0)
```

Depois: a ação atual leva −0.18 (evita ficar sempre na mesma), todas levam `+ random(0, 0.14)`, e ganha a maior.

### Contágio social

Dois vizinhos que estejam no mesmo local, a menos de 110 unidades (do mundo 2D), nenhum deles ao ecrã e fora de casa, aproximam `stress`, `desc` e `pert` a 3.5% da diferença por segundo. Regista-se o tempo que passaram juntos (`links`), que alimenta a descoberta de "andam juntos".

## 4. Entrega de um post

Quando um post é entregue (`deliver`), para cada vizinho:

```
mult = AMPLIFICAR 2.2 | OCULTAR 0.1 | NORMAL 1.0 | auto 1.0
w    = peso da categoria na mesa de mistura / 100        (0 a 1, por defeito 0.5)
pv   = clamp( 0.5 × (0.4 + 1.2×w) × mult × phone × (ao_ecra ? 1.5 : 1), 0, 0.97 )
```

O vizinho vê o post com probabilidade `pv`. Se vê, com `k = 0.07`:

```
medo = t.medo × (ansioso 1.5 | cético 0.55 | 1)
desc = t.desc × (cético 0.5 | 1)
emp  = t.emp  × (solidária 1.4 | 1)
cons = t.cons × (consumista 1.6 | 1)
apat = t.apat × (viciado 1.4 | 1)

stress  += (medo + 0.4×desc) × k
desc    += desc × k
pert    += (emp − 0.4×desc − 0.3×apat) × k
compra  += cons × k × 1.4
apat    += apat × k − 0.3 × emp × k
livAff  += post.livraria × 0.15
```

Todos limitados a [0,1]. Para a reação visível (símbolo sobre a cabeça, 9 s) escolhe-se o tipo com maior valor efetivo, mínimo 1.4, senão "indiferente":

```
medo   = medo + 0.4×desc      raiva = 1.1×desc      empatia = emp
compra = cons                 apatia = apat
```

`strong` é verdadeiro se o valor efetivo for 20% maior que o bruto (a personalidade amplificou).

Receita do post: `0` se OCULTAR, senão `viewers × value × 1.6` mais, para publicidade, `viewers × 2 × (0.5 + w)`. Metade se `adsNervous` (tensão > 0.70). Arredondada.

Receita estimada mostrada no talão antes de carimbar: igual, mas com `viewers` esperados = soma de `clamp(0.5×(0.4+1.2×w)×phone, 0, 0.97)` para todos.

## 5. Métricas e limiares

```
tensao  = clamp( 0.65×stressMédio + 0.35×descMédia + (motim ? 0.12 : 0) + (polícia ? 0.04 : 0) )
coesao  = clamp( 1.25 × pertMédia × (1 − 0.6×descMédia) − 0.2×apatMédia )
```

Valores iniciais: coesão 0.5, tensão 0.2.

- Tensão > 0.62: a polícia começa a patrulhar. Sai quando passa o limite do mapa e a tensão está < 0.5.
- Tensão > 0.70: `adsNervous`. Receitas a metade. O chefe avisa por telefone.

### Anéis de crise (enchem durante a simulação)

```
mercearia += (stressMédio − 0.42) × 0.11 × dt          a 1: mercearia fecha (evento)
junta     += (protestantes − 1.5) × 0.03 × dt          a 1: motim (8 s, stress +0.1 a todos)
livraria  −= 0.014 × dt   (dia 2 em diante, começa em 0.75 no dia 1 e 0.7 nos dias seguintes)   a 0: livraria fecha
praca     += (juntos_na_praça × 0.022 − 0.012) × dt    a 1: festa na praça (10 s, pert +0.08, stress −0.08)
```

`protestantes` = vizinhos na Junta. `juntos_na_praça` = vizinhos na praça com pert > 0.5.

## 6. Objetivos e finais

Objetivos de dia (no fecho): receita do dia >= meta (140, 220, 260), dia 2 também "livraria aberta", todos os dias "tensão < 0.70".

Final (avaliado no fim do dia 3, pela ordem):

1. apatia média > 0.55: Bairro adormecido
2. tensão > 0.65: Bairro trancado
3. coesão >= 0.55 e receita do dia 3 < 260: Bairro vivo, app em perda
4. coesão >= 0.50: Equilíbrio raro
5. nenhuma: Gentrificação silenciosa

Textos dos finais em `data/endings.json`.

## 7. Descoberta (mecânica central)

- O jogador nunca vê estados. Vê reações (símbolo sobre o vizinho) e comportamento.
- Clicar num vizinho com reação ativa regista uma observação no caderno (`notes`), se ainda não tiver sido anotada essa reação.
- A personalidade de um vizinho fica descoberta (`traitKnown`) quando as suas observações mostram o padrão da personalidade (ver `observe` no protótipo).
- Relações ("anda com X") ficam descobertas quando o tempo juntos passa de 5 s e estão a menos de 130 unidades. Máximo 8 contam para a percentagem.
- Quadro de cortiça: começa vazio e preenche-se com personalidades, relações e padrões por categoria (`catObs`, mostra o top 3 de reações para a categoria).
- "Bairro percebido" = (personalidades descobertas + min(8, relações)) / (14 + 8). Aparece no jornal e no relatório final. Não afeta o final.

## 8. Mundo

Locais (id, nome, função): `cafe` Café Aurora (encontro), `mercearia` Mercearia Central, `casasN` Casas da esquina, `junta` Junta de Freguesia (protesto), `livraria` Livraria Letra Viva, `casasS` Prédio do fundo, `praca` Praça (espaço aberto, fonte). Geometria completa em `data/places.json`.

Rua: passadeiras em x = 178 e x = 822 (mundo 2D), semáforos de carros e peões num ciclo de 16 s: carros verde 0 a 8, amarelo 8 a 10, vermelho de 10 a 16; peões "walk" 10.4 a 13.6, intermitente 13.6 a 15.2. Os vizinhos atravessam só nas passadeiras (`routeTo`).

Dia/noite: manhã clara, tarde quente, noite com candeeiros acesos (`TOD` no protótipo).

## 9. Sala de controlo (objetos interativos)

Impressora matricial (imprime linha a linha com clipping, o talão sai e desliza para o suporte), suporte de leitura, carimbos AMPLIFICAR e OCULTAR, cesto NORMAL, mesa de mistura com 5 faders (um por categoria, 0 a 100, por defeito 50), monitor CRT (mapa do bairro, pontos dos vizinhos, anéis, coesão e tensão), quadro de cortiça, telefone (o chefe), rádio do bairro, alavanca, relógio, janela para a rua. A câmara fica fixa e aproxima-se do objeto em foco (vistas: home, crt, board, window, stand).

## 10. Conversão de espaço

O protótipo tem um plano 2D de 1000 x 640 mapeado para a rua em 3D. Em Unity, definir um mapeamento único (`WorldMap`) entre as coordenadas lógicas dos locais e posições reais da cena, e dar aos vizinhos NavMeshAgent. Os raios (110 para contágio, 130 para relações) têm de ser convertidos para metros mantendo a proporção entre a largura da rua e o raio.
