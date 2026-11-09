# CLAUDE.md — Laboratório do Paralelepípedo (Meta Quest 2)

Aplicação educacional em realidade virtual para o **9º ano do Ensino Fundamental** sobre o **paralelepípedo
reto-retângulo**: elementos, diagonais (teorema de Pitágoras), áreas e volume. Proposta pelo núcleo de pesquisa
**NumbERS** (IFG – Câmpus Inhumas), em particular pelo professor Kariton Lula.
Créditos no app: "Desenvolvido por NumbERS · IFG – Câmpus Inhumas".

- **BNCC:** EF09MA14 (problemas com o teorema de Pitágoras) e EF09MA19 (volume de prismas com expressões de cálculo);
  retomadas: EF06MA17 (estação Elementos) e EF08MA19 (estação Áreas)
- **Dispositivo-alvo:** Meta Quest 2
- **Idioma:** todo texto visível ao aluno, mensagens de commit e documentação em **português do Brasil**.
  Identificadores de código em português, seguindo o domínio (`Paralelepipedo`, `AreaTotal`, `ValidadorDeFace`).
- **Crescimento previsto:** depois do paralelepípedo vêm o **cubo** (paralelepípedo com a = b = c, todas as fórmulas
  valem direto) e prismas de outras bases (`IPrisma`). A arquitetura deve aceitar isso sem reescrita.

---

## Conteúdo matemático (fonte da verdade)

- Dimensões: a (comprimento), b (largura), c (altura), números reais > 0 e finitos.
- Vértices: ABCD no topo (A frente-esquerda, B fundo-esquerda, C fundo-direita, D frente-direita) e EFGH embaixo,
  com E sob A, F sob B, G sob C e H sob D. a = EH, b = HG, c = AE.
- Coordenadas do núcleo: x = comprimento (a), y = altura (c), z = largura (b, da frente para o fundo).
  E = (0,0,0), H = (a,0,0), G = (a,0,b), F = (0,0,b); A, D, C, B são os mesmos pontos com y = c.
- 12 arestas: AB, BC, CD, DA, EF, FG, GH, HE, AE, BF, CG, DH.
- 6 faces em ordem cíclica: ABCD e EFGH (bases, a·b), ADHE e BCGF (frente e fundo, a·c), ABFE e DCGH
  (esquerda e direita, b·c). Faces paralelas são congruentes.
- Diagonal da base: d = √(a² + b²). Diagonal do paralelepípedo: D = √(d² + c²) = √(a² + b² + c²).
- Área da base: A_b = a·b. Área lateral: A_L = 2(ac + bc). Área total: A_T = 2(ab + ac + bc).
- Volume: V = A_b · c = a·b·c.
- **Ressalva pedagógica obrigatória:** a contagem com cubinhos de aresta 1 é só um exemplo visual para dimensões
  naturais. A fórmula vale para quaisquer medidas reais (racionais ou irracionais), em que os cubinhos não preenchem a
  caixa exatamente. O app deve dizer isso ao aluno quando a caixa tiver dimensões não inteiras.

Não inventar conteúdo. Texto pedagógico ausente vira placeholder `[REVISAR-PROFESSOR]`, e você deve me avisar.

## Stack

- Unity **6000.6.7f1**, **URP** 17.6.0, perfil de build **Meta Quest** (Android, IL2CPP, ARM64)
- **OpenXR** + **só `com.meta.xr.sdk.core` 207** do Meta XR SDK. **Nada de Interaction SDK** (traz a
  `libISDKEngineTelemetry.so`), Voice, Platform, MR Utility Kit, Avatars ou `com.meta.xr.sdk.all`.
- Raio, pegar e soltar, teleporte e giro por passos são scripts próprios em C#, no padrão do `~/appMissaoCoracao`.
- TextMeshPro (no `com.unity.ugui` 2.6.0), fonte Nunito.
- Unity Test Framework 1.8.0 (EditMode).
- Git + **Git LFS** para binários (png, fbx, ttf, áudio…).

Projeto de referência com o mesmo ambiente: `~/appMissaoCoracao`. Pode ler para reaproveitar padrões; **nunca
alterar nada nele**.

Não adicionar pacotes do Package Manager ou da Asset Store sem perguntar. Não atualizar o Meta XR SDK sem perguntar.

## Estrutura

```
Assets/_Projeto/
  Scripts/
    Nucleo/        # C# puro, sem UnityEngine: Paralelepipedo, IPrisma, ValidadorDeFace, ContagemDeCubinhos,
                   # Desafio (DesafioDeResposta, DesafioDeMontarFace), AvaliadorDeResposta, InterpretadorDeMedida
      Dados/       # DTOs dos JSON, LeitorDeTextos, LeitorDeDesafios, CatalogoDeTextos, ChavesDeTexto
    Apresentacao/  # MonoBehaviours finos que leem o núcleo e atualizam a cena; CarregadorDeDados (JsonUtility)
    XR/            # raio, gatilho, grip, teleporte, giro por passos
    Editor/        # configuração do projeto, atlas da fonte, gerador da cena e prévia (menu "Laboratório")
      Release/     # build assinado, lista de verificação do APK e envio ao ALPHA (menu "Laboratório › Release")
  Dados/           # JSON (TextAsset) de textos e desafios
  Fontes/          # Nunito SemiBold (.ttf via LFS, licença OFL) e a fonte TextMeshPro dinâmica "Nunito SDF"
  Cenas/  Prefabs/  Materiais/
  Testes/EditMode/
```

Assemblies: `LaboratorioParalelepipedo.Nucleo` (`noEngineReferences: true`), `LaboratorioParalelepipedo.Apresentacao`
(→ Núcleo), `LaboratorioParalelepipedo.XR` (→ Núcleo, Apresentação), `LaboratorioParalelepipedo.Testes.EditMode`
(→ Núcleo e Apresentação, só Editor), `LaboratorioParalelepipedo.Editor` (→ Núcleo, Apresentação, TextMeshPro,
Meta XR Core e OpenXR, só Editor). Namespaces `LaboratorioParalelepipedo.<Área>`.

Ficam fora de `Assets/_Projeto/` só os arquivos que o Unity e os pacotes exigem nos lugares padrão:
`Assets/TextMesh Pro/` (recursos essenciais), `Assets/XR/` (OpenXR), `Assets/Settings/Build Profiles/Meta Quest.asset`, `Assets/Plugins/Android/AndroidManifest.xml` (remove permissões de rede e
telemetria), `Assets/StreamingAssets/` (criada pelo Meta SDK ao dar Play)
(perfil de build; com ele ativo, as Player Settings do Android ficam no perfil), `Assets/Oculus/` e `Assets/Resources/`
(Meta SDK).

Não criar arquivos fora de `Assets/_Projeto/` sem pedir. Não editar `Library/`, `Temp/`, `Logs/` nem pacotes de
terceiros.

## Arquitetura

- **Lógica no núcleo, cena fina.** Toda regra (geometria, validação de faces, contagem de cubinhos, avaliação de
  respostas, validação dos desafios) fica no Núcleo, coberta por testes EditMode sem cena nem XR.
- `IPrisma` descreve vértices, arestas, faces, altura, área da base, perímetro da base, área lateral, área total e
  volume. Hoje só o `Paralelepipedo` a implementa; o cubo é `Paralelepipedo.Cubo(a)`.
- **Desafios:** a resposta correta é sempre **calculada** a partir das dimensões e da grandeza. O professor nunca
  digita a resposta.
- **Decisões pedagógicas registradas:**
  - Montar face: vale qualquer vértice inicial, os dois sentidos e, opcionalmente, voltar ao vértice inicial
    (A → B → F → E → A).
  - Tolerância padrão das respostas reais: **±0,01** (aceita 3,60 e 3,61 para √13), ajustável por desafio no JSON.
  - Cubinhos: limite técnico de **1000** por caixa; desafios da estação de volume recusam dimensões **acima de 10**.
  - Interação (`Nucleo/Interacao`, Fase 5): **experiência estacionária** (decisão de 08/11/2026). Raio nas duas
    mãos; gatilho confirma; botão lateral segurado pega a caixa, que gira com a mão e, ao ser solta, volta à mesa em
    **0,4 s** na orientação original. Pegar a caixa nunca move o aluno nem a mesa. Vibração leve ao acender um alvo
    e ao confirmar. **Sem teleporte nem giro por passos na cena:** o analógico não faz nada. O código deles
    (`ComandoDoAnalogico`, `RegrasDoTeleporte`, `MovimentoDoRig`, `LocomocaoDoAluno`) continua no projeto, testado,
    fora da cena. Só o rig se move (na recentralização), nunca a câmera. O painel "Como usar os controles" fica à
    à esquerda da mesa, virado para o aluno, uma linha por botão, e mostra só as ações que existem na cena
    (gatilho: vértices e botões; botão lateral: caixa). Dica curta acima do controle com o raio num alvo.
  - Diagnóstico para o professor num painel próprio, à direita, **escondido**: abre e fecha com os **dois
    analógicos afundados por 3 s** (`AtalhoDoDiagnostico`). Mostra versão, eventos da recentralização, controles
    e erros. A versão aparece sempre também nos créditos.
  - Estação 1 · Elementos (`Nucleo/Estacoes/EstacaoDosElementos`, 0.4.0): "Começar" no painel do título leva a
    ela; o fechamento ("Ir para a Estação 2") leva à Estação 2, no mesmo lugar do painel. Usa os desafios de
    montar face e de contagem da estação de elementos, na ordem do JSON, todos com a caixa 4 × 3 × 2. Montar face:
    vale qualquer vértice inicial e os dois sentidos, a face fica pronta no 4º vértice; vértice que não continua a
    face pisca laranja com uma dica e **só esse toque é descartado** (os certos ficam). Nada pulsa no começo; depois
    de **2 erros seguidos**, pulsam os 4 vértices da face pedida (ou de uma face que continua o que já foi tocado,
    quando vale qualquer face), sem indicar a ordem. A face pronta fica com a cor do par e o nome no meio. Contagens:
    opções embaralhadas, cada distrator com a dica do erro que imita (`DicaDaContagem`: outro elemento, só as 9
    arestas visíveis, só as 4 faces laterais). Na caixa, só as letras dos vértices (sem medidas). "Refazer" refaz a
    estação inteira. Sem exploração livre de vértices, arestas e faces.
  - Estação 2 · Diagonais (`Nucleo/Estacoes/EstacaoDasDiagonais`, 0.3.0): chega-se a ela pelo fechamento da
    Estação 1. Usa só os desafios de diagonais cuja grandeza é a diagonal D, na ordem do JSON, e volta ao
    primeiro depois do último. Ligar E–G e depois E–C, nos dois sentidos; vértice errado pisca laranja com uma
    dica, sem penalidade. Pergunta com as opções embaralhadas; cada distrator tem uma dica própria (D², a + b + c,
    diagonal da base ou geral). O vértice pedido pulsa. Painel da estação à direita da caixa, a 1,25 m e virado
    para o aluno: instrução em destaque no alto, medidas, contas de d e D em duas colunas grandes; o fundo cresce
    conforme o conteúdo. No acerto, só o botão certo fica verde. Os rótulos da caixa usam o shader Overlay do
    TextMeshPro (aparecem através das faces; a tela escura continua por cima).
  - **[REVISAR-PROFESSOR] textos novos da 0.4.0** (rascunhos meus, em `Docs/revisao-pedagogica.md`):
    `estacao.elementos.titulo` (mudou para "Estação 1 · Elementos"), `.objetivo`, `.desafio`, `.tocados`,
    `.naoEhFace` (mudou: "Esse vértice não continua a face. Tente outro."), `.foraDaFace`, `.ajuda`, `.acerto`,
    `.dica.vertices`, `.dica.arestas`, `.dica.faces`, `.dica.arestasEscondidas`, `.dica.semAsBases`, `.dica.geral`,
    `.concluida`, `.resumo`, `.proximaEstacao`, `.refazer`.
  - **Pendências futuras:** (1) desafio de faces paralelas congruentes na Estação 1 ("Qual face é paralela a
    ABFE?"); (2) botão "Escolher estação" no título quando houver 3 ou mais estações.
  - Caixa girada **20°** na mesa (`GeradorDaCena.GiroDaCaixaNaMesa`), para os 8 vértices ficarem separados da
    posição inicial em todas as caixas da estação (com 30°, A cobriria C na 2×3×6). Título acima dos rótulos B e C
    (olhos de 1,3 m a 1,7 m); painel de controles inteiro na vista inicial (bordas a menos de 45°).
  - **[REVISAR-PROFESSOR] textos novos da 0.3.0** (rascunhos meus, em `Docs/revisao-pedagogica.md`):
    `abertura.comecar`, `dica.escolher`, `estacao.diagonais.titulo`, `.objetivo`, `.dimensoes`, `.passo1`,
    `.passo2`, `.proximoVertice`, `.verticeErrado`, `.contaDaBase`, `.resultadoDaBase`, `.contaDaDiagonal`,
    `.incognita`, `.acerto`, `.dica.semRaiz`, `.dica.somou`, `.dica.diagonalDaBase`, `.dica.geral`, `.concluida`,
    `.formulaGeral`, `.proximoDesafio`, `.refazer`, `.rotulo`.
  - Recentralização (`SequenciaDeRecentralizacao`, `RecentralizacaoDoAluno`): o aluno é posto na origem, olhando para
    a mesa (+z), ao abrir o app, ao recentralizar com o botão Meta, quando a origem do rastreamento muda e ao voltar
    do menu ou da pausa. Move o rig com a tela escura (0,15 s para escurecer e clarear), depois de a cabeça ficar
    parada por 0,5 s (no máximo 3 s no escuro). Nenhum movimento da cabeça dispara a recentralização sozinho.
  - Escala da caixa (`EscalaDaCaixa`): 1 unidade = 10 cm, reduzida por desafio até a maior dimensão caber em
    **0,6 m**; o cubinho de aresta 1 usa a mesma escala.

### Formato dos JSON (`Assets/_Projeto/Dados/`)

Lidos com `JsonUtility` por `CarregadorDeDados` (Apresentação) e conferidos por `LeitorDeTextos` e
`LeitorDeDesafios` (Núcleo), que reúnem **todos** os erros do arquivo numa `DadosInvalidosException`.

- `textos.json`: `{ "versao": 1, "textos": [ { "chave": "...", "texto": "..." } ] }`. As chaves ficam em
  `ChavesDeTexto`; os testes exigem todas e recusam chaves a mais. `{0}`, `{1}` recebem valores ao mostrar;
  fórmulas usam rich text do TextMeshPro (`A<sub>b</sub>`).
- `desafios.json`: `{ "versao": 1, "desafios": [ ... ] }`. Campos de todo desafio: `id` (único), `tipo`,
  `estacao` (`elementos`, `diagonais`, `areas`, `volume`), `bncc`, `enunciado`, `dimensoes` (`a`, `b`, `c`).
  - `tipo` `"resposta"` (padrão quando ausente): `grandeza`, `distratores` e `tolerancia` (opcional).
    `grandeza` é uma lista fechada: `diagonalDaBase`, `diagonal`, `areaDaBase`, `areaLateral`, `areaTotal`,
    `volume`, `numeroDeVertices`, `numeroDeArestas`, `numeroDeFaces`. Contagens têm resposta inteira e tolerância
    padrão 0; as demais, ±0,01 (e a tolerância precisa ser maior que zero).
  - `tipo` `"montarFace"`: `face` com uma face em qualquer ordem cíclica válida (`"ABFE"`, `"FEAB"`) ou
    `"qualquer"`; avaliado pelo `ValidadorDeFace`. Sem `grandeza`, `distratores` nem `tolerancia`.
  - Erro claro para tipo, estação ou grandeza desconhecidos, face inexistente, campo que não pertence ao tipo,
    id ausente ou repetido, distrator inválido, igual à resposta (dentro da tolerância) ou repetido.
  - Medidas, distratores e tolerância são **texto**, lidos pelo `InterpretadorDeMedida`: inteiros, decimais com
    vírgula ou ponto (`"2,5"`, `"2.5"`), frações (`"5/2"`), raízes (`"√2"`, `"√(13)"`) e `"k√n"` (`"3√2"`).
    Escreva-os **entre aspas**: um número sem aspas também funciona, mas o JsonUtility o arredonda a 6 casas.
  - Os nomes das listas fechadas não diferenciam maiúsculas de minúsculas. Campos com erro de digitação
    (ex.: `"grandesa"`) são ignorados pelo JsonUtility; o erro aparece como campo faltando.
  - BNCC: Elementos `EF06MA17` (retomada), Diagonais `EF09MA14`, Áreas `EF08MA19` (retomada), Volume `EF09MA19`.

## Regras inegociáveis

1. **100% offline:** sem rede, telemetria, analytics, anúncios ou login. Engine Diagnostics desligado
   (`m_EngineDiagnosticsEnabled: 0`) e sem o módulo `unityanalytics`.
2. **Sem dados pessoais:** nada de nome, placar persistente ou identificadores. O progresso existe só na sessão.
3. **Conforto em VR:** deslocamento só por teleporte e giro por passos. **Nunca mover a câmera por script.** Painéis
   presos ao mundo, nunca à cabeça.
4. **72 fps estáveis no Quest 2:** iluminação baked, no máximo uma luz em tempo real, materiais compartilhados, GPU
   instancing nos cubinhos, limite de cubinhos por caixa e nada de pós-processamento pesado (bloom, SSAO, motion blur).
   Nada de alocação por frame em `Update` (LINQ, `new`, concatenação de strings, `GetComponent` em loop).
5. **Textos em português e fora do código:** tudo que o aluno lê vem de JSON (`TextAsset`). Desafios também em JSON,
   para professores criarem novos sem programar.
6. **Testes EditMode** para toda a lógica, sem dependência de cena ou XR.
7. **Commits pequenos**, um por passo lógico, em português no imperativo (ex.: "Adiciona cálculo da diagonal do
   paralelepípedo"). `git push` ao fim de cada fase.

## Visual

Paleta: parede `#DCE6EC`, piso `#B8C7D1`, mesa `#5E6E7C` / `#43515E`, arestas `#134B6E`, faces translúcidas `#9FD3F2`
(~28% de opacidade), destaque azul `#1E78C8`, destaque laranja `#C2410C`, pares de faces (bases `#DCD3F0`/`#3B2A73`,
a·c `#CFE6F5`/`#134B6E`, b·c `#F8DCC0`/`#7A3A0B`), cubinhos `#E9B949`/`#A9771F`, botão principal `#D9661F`, teleporte
`#17807D`, texto `#1B2A3A`. Fonte arredondada (Nunito via TextMeshPro), textos curtos e grandes.

## Convenções de código

- `PascalCase` para tipos e membros públicos, `_camelCase` para campos privados, `[SerializeField] private` em vez de
  campos públicos nos MonoBehaviours.
- Um tipo público por arquivo, nome do arquivo igual ao do tipo.
- Comentários XML (`///`) nos tipos públicos, em português.

## Arquivos do Unity — cuidados

- Sempre commitar o `.meta` junto com o asset. Nunca apagar ou regenerar `.meta` de assets existentes.
- Não editar à mão `.unity`, `.prefab` e `.asset` grandes; cenas e prefabs vêm de scripts de editor.
- Não alterar `ProjectSettings/` sem explicar o motivo antes.
- O projeto está vinculado ao Unity Cloud (`cloudProjectId`); se `UnityConnectSettings.asset` aparecer alterado no
  `git status` sem motivo, reverta.

## Comandos

```bash
UNITY=/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity

# Testes EditMode
"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode \
  -testResults Logs/editmode-results.xml -logFile Logs/editmode.log

# Gerar a cena de novo (depois de mudar o GeradorDaCena) e renderizar a prévia em Logs/previa-*.png
"$UNITY" -batchmode -nographics -projectPath . -executeMethod LaboratorioParalelepipedo.Editor.GeradorDaCena.Gerar \
  -quit -logFile Logs/cena.log
"$UNITY" -batchmode -projectPath . -executeMethod LaboratorioParalelepipedo.Editor.PreviaDaCena.Renderizar \
  -quit -logFile Logs/previa.log

# APK de release assinado e conferido (Builds/LaboratorioParalelepipedo-<versionCode>.apk); envio em Docs/DEPLOY.md
"$UNITY" -batchmode -nographics -projectPath . -buildTarget Android \
  -executeMethod LaboratorioParalelepipedo.Editor.Release.ScriptDeBuild.GerarRelease -quit -logFile Logs/release.log
```

Configuração do zero (já feita; os passos estão no comentário de `ConfiguracaoDoProjeto`): recursos do TextMeshPro,
perfil Meta Quest, Player Settings e OpenXR, fonte Nunito SDF. Glifos das fontes: `Tools/verificar_glifos.py`.
A Nunito SDF tem atlas **estático** (fonte padrão do TextMeshPro) com os caracteres dos JSON mais
`Fontes/caracteres-extras.txt`. Se um texto novo usar caractere fora do atlas, o teste EditMode avisa: rode
**Laboratório › Regenerar atlas da fonte** (ou `-executeMethod LaboratorioParalelepipedo.Editor.AtlasDaFonte.Regenerar`)
e commite o `Nunito SDF.asset`.

Modo Editor (sem óculos, só `#if UNITY_EDITOR`, objeto `EditorOnly`): olhos a 1,4 m, botão direito do mouse olha em
volta, clique esquerdo age como o gatilho do raio.

O Unity não pode estar aberto neste projeto durante comandos em `-batchmode`. Se falhar por isso, avise em vez de
contornar.

## Deploy

Somente pelo canal **ALPHA** do Meta Horizon Developer Dashboard, via `ovr-platform-util` (o USB do Quest de testes
não transmite dados). App ID: **1398124376723979**. Passo a passo, segredos e lista de verificação do APK em `Docs/DEPLOY.md`.
Package name: **`br.edu.ifg.laboratorioparalelepipedo`** (mesmo padrão do Missão Coração, `br.edu.ifg.missaocoracao`),
fixado em `ProjectSettings.asset`. O perfil de build Meta Quest, quando for criado, deve repetir o mesmo valor.
**Nunca mudar:** depois do primeiro envio ao ALPHA, outro package name vira outro app na loja.
App Secret, senhas e tokens vêm de **variáveis de ambiente** e **nunca** entram no repositório, em log ou em mensagem.

## Fluxo de trabalho com o Claude Code

1. Antes de algo não trivial, apresentar um plano curto e esperar o ok.
2. Primeiro lógica pura + testes EditMode, depois componentes de cena.
3. Ao fim de cada fase: rodar os testes EditMode, mostrar o resultado, listar os commits e fazer o push.
4. Você não vê o óculos nem o Editor: ao terminar uma funcionalidade, liste o que verificar no Quest 2.
5. Decisões de versão de pacote, package name ou comportamento pedagógico: perguntar, não supor.

Fases: 0) base; 1) núcleo + testes; 2) JSON de textos e desafios, com carregador e testes; 3) cena base;
4) estações 1 a 4; 5) interações XR e teleporte; 6) build e upload para o ALPHA.
