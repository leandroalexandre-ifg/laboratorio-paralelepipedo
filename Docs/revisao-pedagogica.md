# Laboratório do Paralelepípedo — revisão pedagógica

Gerado em 08/11/2026 a partir de `Assets/_Projeto/Dados/textos.json` e `Assets/_Projeto/Dados/desafios.json`, com `python3 -I Tools/gerar_revisao_pedagogica.py`.

**Como revisar:** marque ✓ na coluna *Ok?* ou escreva a sugestão na coluna *Sugestão*. Pode mudar qualquer texto, enunciado ou distrator; as respostas são calculadas pelo app a partir das dimensões da caixa.

Observações gerais:

- `{0}` e `{1}` são trocados por números na hora de mostrar (ex.: "Vértices: 3 de 8").
- Respostas não inteiras são aceitas com tolerância de ±0,01 (para √13 ≈ 3,6056, valem 3,60 e 3,61). Nas contagens (vértices, arestas, faces), só vale a resposta exata.
- Nos desafios de montar face, vale começar por qualquer vértice da face e seguir nos dois sentidos, voltando ou não ao vértice inicial (A → B → F → E, E → F → B → A…).
- Convenção dos vértices: ABCD no topo (A frente-esquerda, B fundo-esquerda, C fundo-direita, D frente-direita) e EFGH embaixo (E sob A, F sob B, G sob C, H sob D); a = EH, b = HG, c = AE.

## 1. Textos do app

| Chave | Texto | Ok? | Sugestão |
|---|---|---|---|
| `app.titulo` | Laboratório do Paralelepípedo | | |
| `app.creditos` | Desenvolvido por NumbERS · IFG – Câmpus Inhumas | | |
| `geral.acerto` | Muito bem! | | |
| `geral.erro` | Ainda não. Tente de novo! | | |
| `geral.proximo` | Próximo | | |
| `geral.tentarDeNovo` | Tentar de novo | | |
| `geral.recomecar` | Recomeçar | | |
| `controles.apontar` | Aponte e aperte o gatilho para escolher. | | |
| `controles.pegar` | Aponte para a caixa e segure o botão lateral para pegar. Solte para devolver. | | |
| `controles.teleporte` | Empurre o analógico para a frente, mire no chão e solte para ir até lá. | | |
| `controles.giro` | Empurre o analógico para o lado para girar. | | |
| `estacao.elementos.titulo` | 1. Elementos | | |
| `estacao.elementos.instrucao` | Toque os vértices, as arestas e as faces da caixa. | | |
| `estacao.elementos.vertices` | Vértices: {0} de {1} | | |
| `estacao.elementos.arestas` | Arestas: {0} de {1} | | |
| `estacao.elementos.faces` | Faces: {0} de {1} | | |
| `estacao.elementos.faceMontada` | Você montou a face {0}! | | |
| `estacao.elementos.naoEhFace` | Esses vértices não formam uma face. Comece de novo. | | |
| `estacao.diagonais.titulo` | 2. Diagonais | | |
| `estacao.diagonais.instrucao` | Trace a diagonal da base e veja o triângulo se levantar. | | |
| `estacao.diagonais.formulaDaBase` | d² = a² + b² | | |
| `estacao.diagonais.formulaDaDiagonal` | D² = d² + c² | | |
| `estacao.diagonais.triangulo` | O triângulo de lados d, c e D é retângulo. Use Pitágoras! | | |
| `estacao.areas.titulo` | 3. Áreas | | |
| `estacao.areas.instrucao` | Abra a caixa e compare as faces da mesma cor. | | |
| `estacao.areas.abrir` | Abrir a caixa | | |
| `estacao.areas.fechar` | Fechar a caixa | | |
| `estacao.areas.pares` | Faces da mesma cor são paralelas e têm a mesma área. | | |
| `estacao.areas.formulaDaBase` | A<sub>b</sub> = a · b | | |
| `estacao.areas.formulaLateral` | A<sub>L</sub> = 2(ac + bc) | | |
| `estacao.areas.formulaTotal` | A<sub>T</sub> = 2(ab + ac + bc) | | |
| `estacao.volume.titulo` | 4. Volume | | |
| `estacao.volume.instrucao` | Encha a caixa com cubinhos, camada por camada. | | |
| `estacao.volume.encher` | Encher a caixa | | |
| `estacao.volume.esvaziar` | Esvaziar | | |
| `estacao.volume.camadas` | Camadas: {0} | | |
| `estacao.volume.porCamada` | Cubinhos por camada: {0} | | |
| `estacao.volume.total` | Total: {0} cubinhos | | |
| `estacao.volume.formula` | V = A<sub>b</sub> · c = a · b · c | | |
| `estacao.volume.ressalva` | Atenção: as medidas desta caixa não são todas inteiras, e os cubinhos de aresta 1 não a preenchem exatamente. Eles são só um exemplo. A fórmula V = a · b · c vale para quaisquer medidas. | | |

## 2. Desafios

### 1. Elementos (BNCC EF06MA17)

| Id | Enunciado | Caixa (a × b × c) | Pergunta | Resposta esperada | Distratores e o erro que cada um imita | Ok? | Sugestão |
|---|---|---|---|---|---|---|---|
| `elem-01` | Monte a face ABFE: toque os vértices dela, um depois do outro. | 4 × 3 × 2 | montar uma face | face ABFE (qualquer vértice inicial, nos dois sentidos) | — | | |
| `elem-02` | Monte qualquer face da caixa: toque os 4 vértices dela, em ordem. | 4 × 3 × 2 | montar uma face | qualquer uma das 6 faces | — | | |
| `elem-03` | Quantas arestas tem a caixa? | 4 × 3 × 2 | número de arestas | 12 | **8**: contou os vértices em vez das arestas<br>**6**: contou as faces em vez das arestas<br>**9**: contou só as arestas visíveis (de um ponto de vista comum, 3 ficam escondidas) | | |
| `elem-04` | Quantas faces tem a caixa? | 4 × 3 × 2 | número de faces | 6 | **4**: contou só as faces laterais, sem as duas bases<br>**8**: contou os vértices em vez das faces<br>**12**: contou as arestas em vez das faces | | |

### 2. Diagonais (BNCC EF09MA14)

| Id | Enunciado | Caixa (a × b × c) | Pergunta | Resposta esperada | Distratores e o erro que cada um imita | Ok? | Sugestão |
|---|---|---|---|---|---|---|---|
| `diag-01` | Quanto mede a diagonal d da base? | 2 × 3 × 6 | diagonal da base d | √13 ≈ 3,61 | **5**: somou a + b em vez de usar Pitágoras (2 + 3)<br>**√5**: somou a + b dentro da raiz: √(2 + 3)<br>**13**: esqueceu a raiz: a² + b² = 13 | | |
| `diag-02` | Quanto mede a diagonal D da caixa? | 2 × 3 × 6 | diagonal do paralelepípedo D | 7 | **11**: somou as três dimensões (2 + 3 + 6)<br>**√13**: parou na diagonal da base d<br>**49**: esqueceu a raiz: a² + b² + c² = 49 | | |
| `diag-03` | Quanto mede a diagonal D desta caixa? | 3 × 4 × 12 | diagonal do paralelepípedo D | 13 | **19**: somou as três dimensões (3 + 4 + 12)<br>**5**: parou na diagonal da base d<br>**169**: esqueceu a raiz: a² + b² + c² = 169 | | |
| `diag-04` | Esta caixa é um cubo. Quanto mede a diagonal D? | 2 × 2 × 2 | diagonal do paralelepípedo D | 2√3 ≈ 3,46 | **2√2**: parou na diagonal da base (a diagonal de uma face do cubo)<br>**6**: somou as três arestas (2 + 2 + 2)<br>**√6**: somou as arestas dentro da raiz: √(2 + 2 + 2) | | |

### 3. Áreas (BNCC EF08MA19)

| Id | Enunciado | Caixa (a × b × c) | Pergunta | Resposta esperada | Distratores e o erro que cada um imita | Ok? | Sugestão |
|---|---|---|---|---|---|---|---|
| `area-01` | Qual é a área total da caixa? | 4 × 3 × 2 | área total A<sub>T</sub> | 52 | **24**: calculou o volume (4 · 3 · 2) em vez da área total<br>**26**: somou uma face de cada par sem dobrar: ab + ac + bc<br>**40**: esqueceu o par de faces b·c (esquerda e direita): 2(ab + ac) | | |
| `area-02` | Qual é a área lateral da caixa? | 4 × 3 × 2 | área lateral A<sub>L</sub> | 28 | **52**: calculou a área total, incluindo as duas bases<br>**14**: não dobrou: ac + bc<br>**24**: calculou o volume (4 · 3 · 2) | | |
| `area-03` | Qual é a área da base da caixa? | 4 × 3 × 2 | área da base A<sub>b</sub> | 12 | **7**: somou a + b em vez de multiplicar<br>**14**: calculou o perímetro da base: 2(a + b)<br>**24**: calculou o volume (4 · 3 · 2) | | |

### 4. Volume (BNCC EF09MA19)

| Id | Enunciado | Caixa (a × b × c) | Pergunta | Resposta esperada | Distratores e o erro que cada um imita | Ok? | Sugestão |
|---|---|---|---|---|---|---|---|
| `vol-01` | Quantos cubinhos enchem a caixa? Qual é o volume? | 4 × 3 × 3 | volume V | 36 | **10**: somou as dimensões (4 + 3 + 3)<br>**12**: contou só uma camada de cubinhos (a área da base)<br>**48**: contou uma camada a mais (4 camadas de 12) | | |
| `vol-02` | Os cubinhos não enchem esta caixa. Qual é o volume dela? | 2,5 × 1,2 × √2 | volume V | 3√2 ≈ 4,24 | **2**: contou só os cubinhos inteiros que cabem na caixa<br>**3**: calculou só a área da base (2,5 · 1,2)<br>**6**: trocou √2 por 2 (2,5 · 1,2 · 2) | | |
| `vol-03` | Esta caixa é um cubo. Qual é o volume dela? | 2 × 2 × 2 | volume V | 8 | **6**: somou as três arestas (2 + 2 + 2)<br>**4**: calculou a área de uma face (2 · 2)<br>**24**: calculou a área total (6 faces de área 4) | | |
