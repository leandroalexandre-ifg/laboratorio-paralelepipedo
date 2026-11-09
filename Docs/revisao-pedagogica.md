# Laboratório do Paralelepípedo — revisão pedagógica

Gerado em 09/11/2026 a partir de `Assets/_Projeto/Dados/textos.json` e `Assets/_Projeto/Dados/desafios.json`, com `python3 -I Tools/gerar_revisao_pedagogica.py`.

**Como revisar:** marque ✓ na coluna *Ok?* ou escreva a sugestão na coluna *Sugestão*. Pode mudar qualquer texto, enunciado ou distrator; as respostas são calculadas pelo app a partir das dimensões da caixa.

Observações gerais:

- `{0}` e `{1}` são trocados por números na hora de mostrar (ex.: "Vértices: 3 de 8").
- Respostas não inteiras são aceitas com tolerância de ±0,01 (para √13 ≈ 3,6056, valem 3,60 e 3,61). Nas contagens (vértices, arestas, faces), só vale a resposta exata.
- Nos desafios de montar face, vale começar por qualquer vértice da face e seguir nos dois sentidos, voltando ou não ao vértice inicial (A → B → F → E, E → F → B → A…).
- Convenção dos vértices: ABCD no topo (A frente-esquerda, B fundo-esquerda, C fundo-direita, D frente-direita) e EFGH embaixo (E sob A, F sob B, G sob C, H sob D); a = EH, b = HG, c = AE.

## 1. Textos a revisar primeiro [REVISAR-PROFESSOR] (65)

Rascunhos escritos durante o desenvolvimento, ainda sem o aval do professor. O app já os mostra ao aluno; confira a clareza para o 9º ano, o vocabulário e a correção matemática (principalmente das dicas, que explicam o erro de cada resposta errada).

### Versão 0.5.0 · Estação 3 · Áreas (24)

| Chave | Texto | Observação | Ok? | Sugestão |
|---|---|---|---|---|
| `estacao.diagonais.proximaEstacao` **[REVISAR-PROFESSOR]** | Ir para a Estação 3 |  | | |
| `estacao.areas.titulo` **[REVISAR-PROFESSOR]** | Estação 3 · Áreas | mudou para "Estação 3 · Áreas" | | |
| `estacao.areas.instrucao` **[REVISAR-PROFESSOR]** | Aperte “Abrir a caixa” para ver as 6 faces abertas. | mudou | | |
| `estacao.areas.objetivo` **[REVISAR-PROFESSOR]** | Descubra a área da base, a área lateral e a área total da caixa. |  | | |
| `estacao.areas.paresInstrucao` **[REVISAR-PROFESSOR]** | Toque uma face. A face paralela a ela acende junto. |  | | |
| `estacao.areas.parEncontrado` **[REVISAR-PROFESSOR]** | {0} e {1} são paralelas e têm a mesma área: {2}. |  | | |
| `estacao.areas.paresContagem` **[REVISAR-PROFESSOR]** | Pares encontrados: {0} de {1} |  | | |
| `estacao.areas.pergunta` **[REVISAR-PROFESSOR]** | Pergunta {0} de {1} |  | | |
| `estacao.areas.ladosDaFace` **[REVISAR-PROFESSOR]** | {0} × {1} |  | | |
| `estacao.areas.areaDaFace` **[REVISAR-PROFESSOR]** | {0} · {1} = {2} |  | | |
| `estacao.areas.acerto` **[REVISAR-PROFESSOR]** | Isso! |  | | |
| `estacao.areas.contaDaBase` **[REVISAR-PROFESSOR]** | A<sub>b</sub> = a · b = {0} · {1} = {2} |  | | |
| `estacao.areas.contaLateral` **[REVISAR-PROFESSOR]** | A<sub>L</sub> = 2(ac + bc) = 2({0} + {1}) = {2} |  | | |
| `estacao.areas.contaTotal` **[REVISAR-PROFESSOR]** | A<sub>T</sub> = 2(ab + ac + bc) = 2({0} + {1} + {2}) = {3} |  | | |
| `estacao.areas.dica.volume` **[REVISAR-PROFESSOR]** | {0} é o volume (a · b · c), não uma área. |  | | |
| `estacao.areas.dica.somou` **[REVISAR-PROFESSOR]** | {0} é a + b. A área do retângulo multiplica: a · b. |  | | |
| `estacao.areas.dica.perimetro` **[REVISAR-PROFESSOR]** | {0} é o perímetro da base, a volta dela. A área é a · b. |  | | |
| `estacao.areas.dica.comAsBases` **[REVISAR-PROFESSOR]** | {0} inclui as duas bases. A área lateral é só a das 4 faces dos lados. |  | | |
| `estacao.areas.dica.semDobrar` **[REVISAR-PROFESSOR]** | {0} conta uma face de cada par. Cada par tem 2 faces iguais: falta dobrar. |  | | |
| `estacao.areas.dica.soLateral` **[REVISAR-PROFESSOR]** | {0} é só a área lateral. Faltaram as duas bases. |  | | |
| `estacao.areas.dica.faltouPar` **[REVISAR-PROFESSOR]** | {0} deixa um par de faces de fora. São 3 pares. |  | | |
| `estacao.areas.dica.geral` **[REVISAR-PROFESSOR]** | Ainda não. Use as áreas dos pares que você encontrou. |  | | |
| `estacao.areas.concluida` **[REVISAR-PROFESSOR]** | Estação concluída! |  | | |
| `estacao.areas.refazer` **[REVISAR-PROFESSOR]** | Refazer |  | | |

### Versão 0.4.0 · Estação 1 · Elementos (18)

| Chave | Texto | Observação | Ok? | Sugestão |
|---|---|---|---|---|
| `estacao.elementos.titulo` **[REVISAR-PROFESSOR]** | Estação 1 · Elementos | mudou para "Estação 1 · Elementos" | | |
| `estacao.elementos.objetivo` **[REVISAR-PROFESSOR]** | Conheça os elementos da caixa: vértices, arestas e faces. |  | | |
| `estacao.elementos.desafio` **[REVISAR-PROFESSOR]** | Desafio {0} de {1} |  | | |
| `estacao.elementos.tocados` **[REVISAR-PROFESSOR]** | Vértices: {0} |  | | |
| `estacao.elementos.naoEhFace` **[REVISAR-PROFESSOR]** | Esse vértice não continua a face. Tente outro. | mudou: só o toque errado é descartado | | |
| `estacao.elementos.foraDaFace` **[REVISAR-PROFESSOR]** | O vértice {0} não é da face {1}. Tente outro. |  | | |
| `estacao.elementos.ajuda` **[REVISAR-PROFESSOR]** | Os vértices que pulsam formam a face. |  | | |
| `estacao.elementos.acerto` **[REVISAR-PROFESSOR]** | Isso! São {0}. |  | | |
| `estacao.elementos.dica.vertices` **[REVISAR-PROFESSOR]** | {0} é o número de vértices, os cantos da caixa. |  | | |
| `estacao.elementos.dica.arestas` **[REVISAR-PROFESSOR]** | {0} é o número de arestas, os segmentos que ligam os vértices. |  | | |
| `estacao.elementos.dica.faces` **[REVISAR-PROFESSOR]** | {0} é o número de faces, as partes planas da caixa. |  | | |
| `estacao.elementos.dica.arestasEscondidas` **[REVISAR-PROFESSOR]** | {0} são só as arestas que dá para ver daqui. Pegue a caixa e gire: há arestas escondidas atrás. |  | | |
| `estacao.elementos.dica.semAsBases` **[REVISAR-PROFESSOR]** | {0} são só as faces dos lados. Faltaram a face de cima e a de baixo. |  | | |
| `estacao.elementos.dica.geral` **[REVISAR-PROFESSOR]** | Ainda não. Conte de novo, com calma. Pegue a caixa e gire para ver todos os lados. |  | | |
| `estacao.elementos.concluida` **[REVISAR-PROFESSOR]** | Estação concluída! |  | | |
| `estacao.elementos.resumo` **[REVISAR-PROFESSOR]** | A caixa tem {0} vértices, {1} arestas e {2} faces. |  | | |
| `estacao.elementos.proximaEstacao` **[REVISAR-PROFESSOR]** | Ir para a Estação 2 |  | | |
| `estacao.elementos.refazer` **[REVISAR-PROFESSOR]** | Refazer |  | | |

### Versão 0.3.0 · Estação 2 · Diagonais (23)

| Chave | Texto | Observação | Ok? | Sugestão |
|---|---|---|---|---|
| `abertura.comecar` **[REVISAR-PROFESSOR]** | Começar |  | | |
| `dica.escolher` **[REVISAR-PROFESSOR]** | Aperte o gatilho para escolher |  | | |
| `estacao.diagonais.titulo` **[REVISAR-PROFESSOR]** | Estação 2 · Diagonais |  | | |
| `estacao.diagonais.objetivo` **[REVISAR-PROFESSOR]** | Descubra quanto mede a diagonal D da caixa, de um canto ao canto oposto. |  | | |
| `estacao.diagonais.dimensoes` **[REVISAR-PROFESSOR]** | a = {0}    b = {1}    c = {2} |  | | |
| `estacao.diagonais.passo1` **[REVISAR-PROFESSOR]** | Aponte para o vértice E e aperte o gatilho. Depois, para o G. |  | | |
| `estacao.diagonais.passo2` **[REVISAR-PROFESSOR]** | Agora ligue E até C. |  | | |
| `estacao.diagonais.proximoVertice` **[REVISAR-PROFESSOR]** | Agora aponte para o vértice {0}. |  | | |
| `estacao.diagonais.verticeErrado` **[REVISAR-PROFESSOR]** | Esse é o vértice {0}. Procure o {1}. |  | | |
| `estacao.diagonais.contaDaBase` **[REVISAR-PROFESSOR]** | d² = {0} + {1} |  | | |
| `estacao.diagonais.resultadoDaBase` **[REVISAR-PROFESSOR]** | d = {0} |  | | |
| `estacao.diagonais.contaDaDiagonal` **[REVISAR-PROFESSOR]** | D² = {0} + {1} = {2} |  | | |
| `estacao.diagonais.incognita` **[REVISAR-PROFESSOR]** | D = ? |  | | |
| `estacao.diagonais.acerto` **[REVISAR-PROFESSOR]** | Isso! D = {0}. |  | | |
| `estacao.diagonais.dica.semRaiz` **[REVISAR-PROFESSOR]** | {0} é D². Falta tirar a raiz. |  | | |
| `estacao.diagonais.dica.somou` **[REVISAR-PROFESSOR]** | {0} é a + b + c. Some os quadrados, não as medidas. |  | | |
| `estacao.diagonais.dica.diagonalDaBase` **[REVISAR-PROFESSOR]** | {0} é a diagonal d da base. Falta usar a altura c. |  | | |
| `estacao.diagonais.dica.geral` **[REVISAR-PROFESSOR]** | Ainda não. Use D² = d² + c² e tire a raiz. |  | | |
| `estacao.diagonais.concluida` **[REVISAR-PROFESSOR]** | Estação concluída! |  | | |
| `estacao.diagonais.formulaGeral` **[REVISAR-PROFESSOR]** | D = √(a² + b² + c²) |  | | |
| `estacao.diagonais.proximoDesafio` **[REVISAR-PROFESSOR]** | Próximo desafio |  | | |
| `estacao.diagonais.refazer` **[REVISAR-PROFESSOR]** | Refazer |  | | |
| `estacao.diagonais.rotulo` **[REVISAR-PROFESSOR]** | {0} = {1} |  | | |

## 2. Demais textos (37)

Textos mais antigos ou de interface (títulos, botões, painel de diagnóstico). Vale uma conferência geral.

| Chave | Texto | Ok? | Sugestão |
|---|---|---|---|
| `app.titulo` | Laboratório do Paralelepípedo | | |
| `app.creditos` | Desenvolvido por NumbERS · IFG – Câmpus Inhumas | | |
| `app.versao` | Versão {0} | | |
| `diagnostico.titulo` | Diagnóstico dos controles (para o professor) | | |
| `geral.acerto` | Muito bem! | | |
| `geral.erro` | Ainda não. Tente de novo! | | |
| `geral.proximo` | Próximo | | |
| `geral.tentarDeNovo` | Tentar de novo | | |
| `geral.recomecar` | Recomeçar | | |
| `controles.titulo` | Como usar os controles | | |
| `controles.apontar` | <b>Gatilho</b> (dedo indicador): aponte e aperte para escolher. | | |
| `controles.pegar` | <b>Botão lateral</b> (dedo médio): aponte para a caixa e segure para pegar. Gire a mão para girar a caixa. Solte para devolver. | | |
| `controles.teleporte` | <b>Analógico para a frente</b>: mire no chão e solte para ir até lá. | | |
| `controles.giro` | <b>Analógico para o lado</b>: virar. | | |
| `dica.pegar` | Segure o botão lateral para pegar | | |
| `dica.teleporteValido` | Solte para ir até lá | | |
| `dica.teleporteInvalido` | Aqui não dá | | |
| `estacao.elementos.faceMontada` | Você montou a face {0}! | | |
| `estacao.diagonais.instrucao` | Trace a diagonal da base e veja o triângulo se levantar. | | |
| `estacao.diagonais.formulaDaBase` | d² = a² + b² | | |
| `estacao.diagonais.formulaDaDiagonal` | D² = d² + c² | | |
| `estacao.diagonais.triangulo` | O triângulo de lados d, c e D é retângulo. Use Pitágoras! | | |
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

## 3. Desafios

### 1. Elementos (BNCC EF06MA17)

| Id | Enunciado | Caixa (a × b × c) | Pergunta | Resposta esperada | Distratores e o erro que cada um imita | Ok? | Sugestão |
|---|---|---|---|---|---|---|---|
| `elem-01` | Monte a face ADHE, a da frente: toque os vértices dela, um depois do outro. | 4 × 3 × 2 | montar uma face | face ADHE (qualquer vértice inicial, nos dois sentidos) | — | | |
| `elem-02` | Monte a face ABFE: toque os vértices dela, um depois do outro. | 4 × 3 × 2 | montar uma face | face ABFE (qualquer vértice inicial, nos dois sentidos) | — | | |
| `elem-03` | Monte qualquer face da caixa: toque os 4 vértices dela, em ordem. | 4 × 3 × 2 | montar uma face | qualquer uma das 6 faces | — | | |
| `elem-04` | Quantas arestas tem a caixa? | 4 × 3 × 2 | número de arestas | 12 | **8**: contou os vértices em vez das arestas<br>**6**: contou as faces em vez das arestas<br>**9**: contou só as arestas visíveis (de um ponto de vista comum, 3 ficam escondidas) | | |
| `elem-05` | Quantas faces tem a caixa? | 4 × 3 × 2 | número de faces | 6 | **4**: contou só as faces laterais, sem as duas bases<br>**8**: contou os vértices em vez das faces<br>**12**: contou as arestas em vez das faces | | |

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
| `area-01` | Qual é a área da base da caixa? | 4 × 3 × 2 | área da base A<sub>b</sub> | 12 | **7**: somou a + b em vez de multiplicar<br>**14**: calculou o perímetro da base: 2(a + b)<br>**24**: calculou o volume (4 · 3 · 2) | | |
| `area-02` | Qual é a área lateral da caixa? | 4 × 3 × 2 | área lateral A<sub>L</sub> | 28 | **52**: calculou a área total, incluindo as duas bases<br>**14**: não dobrou: ac + bc<br>**24**: calculou o volume (4 · 3 · 2) | | |
| `area-03` | Qual é a área total da caixa? | 4 × 3 × 2 | área total A<sub>T</sub> | 52 | **24**: calculou o volume (4 · 3 · 2) em vez da área total<br>**26**: somou uma face de cada par sem dobrar: ab + ac + bc<br>**40**: esqueceu o par de faces b·c (esquerda e direita): 2(ab + ac) | | |

### 4. Volume (BNCC EF09MA19)

| Id | Enunciado | Caixa (a × b × c) | Pergunta | Resposta esperada | Distratores e o erro que cada um imita | Ok? | Sugestão |
|---|---|---|---|---|---|---|---|
| `vol-01` | Quantos cubinhos enchem a caixa? Qual é o volume? | 4 × 3 × 3 | volume V | 36 | **10**: somou as dimensões (4 + 3 + 3)<br>**12**: contou só uma camada de cubinhos (a área da base)<br>**48**: contou uma camada a mais (4 camadas de 12) | | |
| `vol-02` | Os cubinhos não enchem esta caixa. Qual é o volume dela? | 2,5 × 1,2 × √2 | volume V | 3√2 ≈ 4,24 | **2**: contou só os cubinhos inteiros que cabem na caixa<br>**3**: calculou só a área da base (2,5 · 1,2)<br>**6**: trocou √2 por 2 (2,5 · 1,2 · 2) | | |
| `vol-03` | Esta caixa é um cubo. Qual é o volume dela? | 2 × 2 × 2 | volume V | 8 | **6**: somou as três arestas (2 + 2 + 2)<br>**4**: calculou a área de uma face (2 · 2)<br>**24**: calculou a área total (6 faces de área 4) | | |
