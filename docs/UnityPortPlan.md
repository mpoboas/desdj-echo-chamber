> **LEGADO (v1).** Este documento descreve o design v1, abandonado a 2026-11-08. O design atual esta em `docs/GDD.md` e `docs/GDD_Posts.md`. Manter apenas como referencia da simulacao (estados dos vizinhos, utilidade, contagio), que continua a ser usada.

# Plano de conversão para Unity

## Arquitetura alvo

```
Assets/_Project/
  Scripts/
    Simulation/        C# puro, sem UnityEngine onde possível
      NeighbourState.cs      estados (stress, pert, desc, apat, compra, livAff, phone) e trait
      UtilityBrain.cs        escolha de ação por utilidade (PrototypeSpec secção 3)
      ContagionSystem.cs     aproximação de estados entre vizinhos próximos
      PostDelivery.cs        entrega de um post (PrototypeSpec secção 4)
      Metrics.cs             coesão, tensão, receita, limiares
      CrisisRings.cs         anéis (mercearia, junta, livraria, praça) e eventos
      EndingResolver.cs      objetivos de dia e finais
      DiscoveryModel.cs      observações, traits descobertos, relações, "bairro percebido"
      GameSimulation.cs      orquestra tudo; avança com Tick(dt); sem cena
    Game/               ligação entre simulação e Unity
      GameManager.cs, TurnManager.cs   dias, turnos, fases (Decidir, Simular, Jornal, FimDeDia)
      NeighbourAgent.cs      MonoBehaviour + NavMeshAgent; lê da simulação, move, anima, mostra reação
      PlaceRegistry.cs       locais e pontos de interesse (portas, zonas) no mundo 3D
      TrafficSystem.cs       semáforos e passadeiras (ciclo de 16 s)
    Room/               interação da sala
      PrinterController.cs   impressão linha a linha (clipping) e talão a deslizar
      SlipView.cs            talão (Canvas em World Space ou textura)
      StampController.cs     carimbos A/O e cesto N
      MixerController.cs     5 faders arrastáveis, valor 0 a 100
      CRTMonitor.cs          RenderTexture de uma câmara ortográfica, mais overlay de dados
      CorkBoard.cs           quadro de cortiça (traits, relações, padrões)
      PhoneController.cs, RadioController.cs, LeverController.cs
      CameraRig.cs           vistas fixas com Cinemachine (home, crt, board, window, stand)
    UI/                 jornal, relatório final, HUD, diálogos, menu
  Data/                 ScriptableObjects e JSON importado
    PostDefinition, DayDefinition, NeighbourDefinition, PlaceDefinition, TuningConfig (todos os números)
```

`TuningConfig` guarda todas as constantes do protótipo (secção 3 a 6 do PrototypeSpec). Nada de números mágicos nas classes.

## Ordem de trabalho (marcos)

Cada marco só fecha quando compila sem erros, os testes passam e foi verificado em Play Mode.

**M0. Projeto.** Ver versão do Unity, pipeline e input em `ProjectSettings`. Criar a estrutura de pastas, `.gitignore`, `.gitattributes` com LFS, assembly definitions (`EchoChamber.Simulation`, `EchoChamber.Game`, `EchoChamber.Tests`). Instalar os pacotes necessários (Cinemachine, glTFast, Input System se aplicável, Test Framework).

**M1. Simulação sem cena.** Implementar `Simulation/` e importar `docs/data/*.json` para ScriptableObjects. Testes de unidade (EditMode): contágio converge, `deliver` altera estados como na spec, fórmulas de coesão e tensão, `EndingResolver` devolve cada um dos 5 finais para estados construídos de propósito. Um teste de simulação headless que corre 3 dias com decisões automáticas (tudo NORMAL, tudo AMPLIFICAR, tudo OCULTAR) e verifica que os extremos levam a finais diferentes.

**M2. Mundo.** Cena da rua: edifícios, praça, passadeiras, semáforos, NavMesh, locais com portas e zonas. Importar os GLB de `assets/`. Vizinhos com `NavMeshAgent`, animação simples (andar, parado, braços em tensão) e símbolo de reação por cima. Ligação à simulação: cada vizinho age conforme o `UtilityBrain`. Ciclo dia/noite.

**M3. Sala.** Sala de controlo com a mesa e os objetos. Câmara fixa com vistas e zoom por clique. Impressora a imprimir talões, suporte de leitura, carimbos, cesto, mesa de mistura, alavanca, telefone, rádio.

**M4. Ciclo completo de um turno (D2.2).** Decidir, publicar, simular 26 s, jornal. Monitor CRT com mapa dos vizinhos, anéis, coesão e tensão. Receita estimada no talão. Primeiro protótipo jogável de ponta a ponta.

**M5. Descoberta.** Clicar em vizinhos com reação ativa para anotar. Caderno e quadro de cortiça. "Bairro percebido". Jornal sem causas.

**M6. Jogo completo (D3.1).** Três dias, posts scripted, objetivos de dia, 5 finais, relatório final com gráfico de coesão e tensão. Tutorial de dicas no primeiro turno. Menu inicial e reinício.

**M7. Polimento.** Áudio (impressora, carimbo, alavanca, telefone, rádio, ambiente de rua que muda com o estado), iluminação por hora do dia, feedback visual, balanceamento com o simulador headless, build para Windows e macOS, pelo menos 10 minutos de jogo efetivo.

## Equivalências protótipo para Unity

| Protótipo (three.js) | Unity |
|---|---|
| Mundo 2D 1000x640 | Mundo 3D + `WorldMap` (coordenadas lógicas para posições) |
| Caminhos com `routeTo` e passadeiras | `NavMeshAgent` + waypoints nas passadeiras, semáforos como bloqueios |
| `chooseAction`, `deliver`, `computeMetrics` | `Simulation/*` com os mesmos números |
| Canvas 2D em texturas (talão, CRT, quadro, jornal) | Canvas World Space / RenderTexture |
| Clipping plane para a impressão | Shader com corte por altura, ou máscara |
| Sprite de reação sobre o vizinho | `Canvas` World Space ou `SpriteRenderer` billboard |
| Postura procedural por ossos | Animator simples ou rotação procedural dos ossos dos GLB |
| `#fast` (tempo x5) | `Time.timeScale` por tecla de debug |
| `__S()` | painel de debug in-game (F1) com estado da simulação |

## O que NÃO portar à letra

Os textos de ajuda que explicam a arquitetura (a caixa "Como isto passa para Unity" no ecrã final) não entram no jogo. A lógica de câmara e renderização do three.js (sombras, exposição, materiais) deve ser refeita com as ferramentas do Unity, procurando o mesmo aspeto (low-poly, cores quentes na sala, rua mais fria).
