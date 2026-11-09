#!/usr/bin/env python3
"""Gera Docs/revisao-pedagogica.md a partir de textos.json e desafios.json.

Uso (na raiz do repositório):
    python3 -I Tools/gerar_revisao_pedagogica.py

Cada distrator precisa de uma explicação em ERROS_DOS_DISTRATORES (o erro de raciocínio que ele imita).
O script falha se faltar alguma, para o documento nunca sair incompleto.

PENDENTES é a lista única dos textos [REVISAR-PROFESSOR]: rascunhos escritos sem o aval do professor, agrupados
pela versão em que entraram. Eles vão para o topo do documento; os outros textos ficam na seção "Demais textos".
Ao criar ou mudar um texto pedagógico, inclua a chave aqui; quando o professor aprovar, retire. O script falha se
uma chave pendente não existir no textos.json.
"""
import json
import math
import re
import sys
from datetime import date
from pathlib import Path

RAIZ = Path(__file__).resolve().parent.parent
DADOS = RAIZ / "Assets/_Projeto/Dados"
SAIDA = RAIZ / "Docs/revisao-pedagogica.md"

# Textos [REVISAR-PROFESSOR], por versão: (chave, observação para o revisor ou "").
PENDENTES = {
    "0.5.0 · Estação 3 · Áreas": [
        ("estacao.diagonais.proximaEstacao", ""),
        ("estacao.areas.titulo", "mudou para \"Estação 3 · Áreas\""),
        ("estacao.areas.instrucao", "mudou"),
        ("estacao.areas.objetivo", ""),
        ("estacao.areas.paresInstrucao", ""),
        ("estacao.areas.parEncontrado", ""),
        ("estacao.areas.paresContagem", ""),
        ("estacao.areas.pergunta", ""),
        ("estacao.areas.ladosDaFace", ""),
        ("estacao.areas.areaDaFace", ""),
        ("estacao.areas.acerto", ""),
        ("estacao.areas.contaDaBase", ""),
        ("estacao.areas.contaLateral", ""),
        ("estacao.areas.contaTotal", ""),
        ("estacao.areas.dica.volume", ""),
        ("estacao.areas.dica.somou", ""),
        ("estacao.areas.dica.perimetro", ""),
        ("estacao.areas.dica.comAsBases", ""),
        ("estacao.areas.dica.semDobrar", ""),
        ("estacao.areas.dica.soLateral", ""),
        ("estacao.areas.dica.faltouPar", ""),
        ("estacao.areas.dica.geral", ""),
        ("estacao.areas.concluida", ""),
        ("estacao.areas.refazer", ""),
    ],
    "0.4.0 · Estação 1 · Elementos": [
        ("estacao.elementos.titulo", "mudou para \"Estação 1 · Elementos\""),
        ("estacao.elementos.objetivo", ""),
        ("estacao.elementos.desafio", ""),
        ("estacao.elementos.tocados", ""),
        ("estacao.elementos.naoEhFace", "mudou: só o toque errado é descartado"),
        ("estacao.elementos.foraDaFace", ""),
        ("estacao.elementos.ajuda", ""),
        ("estacao.elementos.acerto", ""),
        ("estacao.elementos.dica.vertices", ""),
        ("estacao.elementos.dica.arestas", ""),
        ("estacao.elementos.dica.faces", ""),
        ("estacao.elementos.dica.arestasEscondidas", ""),
        ("estacao.elementos.dica.semAsBases", ""),
        ("estacao.elementos.dica.geral", ""),
        ("estacao.elementos.concluida", ""),
        ("estacao.elementos.resumo", ""),
        ("estacao.elementos.proximaEstacao", ""),
        ("estacao.elementos.refazer", ""),
    ],
    "0.3.0 · Estação 2 · Diagonais": [
        ("abertura.comecar", ""),
        ("dica.escolher", ""),
        ("estacao.diagonais.titulo", ""),
        ("estacao.diagonais.objetivo", ""),
        ("estacao.diagonais.dimensoes", ""),
        ("estacao.diagonais.passo1", ""),
        ("estacao.diagonais.passo2", ""),
        ("estacao.diagonais.proximoVertice", ""),
        ("estacao.diagonais.verticeErrado", ""),
        ("estacao.diagonais.contaDaBase", ""),
        ("estacao.diagonais.resultadoDaBase", ""),
        ("estacao.diagonais.contaDaDiagonal", ""),
        ("estacao.diagonais.incognita", ""),
        ("estacao.diagonais.acerto", ""),
        ("estacao.diagonais.dica.semRaiz", ""),
        ("estacao.diagonais.dica.somou", ""),
        ("estacao.diagonais.dica.diagonalDaBase", ""),
        ("estacao.diagonais.dica.geral", ""),
        ("estacao.diagonais.concluida", ""),
        ("estacao.diagonais.formulaGeral", ""),
        ("estacao.diagonais.proximoDesafio", ""),
        ("estacao.diagonais.refazer", ""),
        ("estacao.diagonais.rotulo", ""),
    ],
}

ESTACOES = [
    ("elementos", "1. Elementos"),
    ("diagonais", "2. Diagonais"),
    ("areas", "3. Áreas"),
    ("volume", "4. Volume"),
]

PERGUNTAS = {
    "diagonalDaBase": "diagonal da base d",
    "diagonal": "diagonal do paralelepípedo D",
    "areaDaBase": "área da base A<sub>b</sub>",
    "areaLateral": "área lateral A<sub>L</sub>",
    "areaTotal": "área total A<sub>T</sub>",
    "volume": "volume V",
    "numeroDeVertices": "número de vértices",
    "numeroDeArestas": "número de arestas",
    "numeroDeFaces": "número de faces",
}

# (id do desafio, distrator como está no JSON) -> erro que o distrator imita.
ERROS_DOS_DISTRATORES = {
    ("elem-04", "8"): "contou os vértices em vez das arestas",
    ("elem-04", "6"): "contou as faces em vez das arestas",
    ("elem-04", "9"): "contou só as arestas visíveis (de um ponto de vista comum, 3 ficam escondidas)",
    ("elem-05", "4"): "contou só as faces laterais, sem as duas bases",
    ("elem-05", "8"): "contou os vértices em vez das faces",
    ("elem-05", "12"): "contou as arestas em vez das faces",
    ("diag-01", "5"): "somou a + b em vez de usar Pitágoras (2 + 3)",
    ("diag-01", "√5"): "somou a + b dentro da raiz: √(2 + 3)",
    ("diag-01", "13"): "esqueceu a raiz: a² + b² = 13",
    ("diag-02", "11"): "somou as três dimensões (2 + 3 + 6)",
    ("diag-02", "√13"): "parou na diagonal da base d",
    ("diag-02", "49"): "esqueceu a raiz: a² + b² + c² = 49",
    ("diag-03", "19"): "somou as três dimensões (3 + 4 + 12)",
    ("diag-03", "5"): "parou na diagonal da base d",
    ("diag-03", "169"): "esqueceu a raiz: a² + b² + c² = 169",
    ("diag-04", "2√2"): "parou na diagonal da base (a diagonal de uma face do cubo)",
    ("diag-04", "6"): "somou as três arestas (2 + 2 + 2)",
    ("diag-04", "√6"): "somou as arestas dentro da raiz: √(2 + 2 + 2)",
    ("area-03", "24"): "calculou o volume (4 · 3 · 2) em vez da área total",
    ("area-03", "26"): "somou uma face de cada par sem dobrar: ab + ac + bc",
    ("area-03", "40"): "esqueceu o par de faces b·c (esquerda e direita): 2(ab + ac)",
    ("area-02", "52"): "calculou a área total, incluindo as duas bases",
    ("area-02", "14"): "não dobrou: ac + bc",
    ("area-02", "24"): "calculou o volume (4 · 3 · 2)",
    ("area-01", "7"): "somou a + b em vez de multiplicar",
    ("area-01", "14"): "calculou o perímetro da base: 2(a + b)",
    ("area-01", "24"): "calculou o volume (4 · 3 · 2)",
    ("vol-01", "10"): "somou as dimensões (4 + 3 + 3)",
    ("vol-01", "12"): "contou só uma camada de cubinhos (a área da base)",
    ("vol-01", "48"): "contou uma camada a mais (4 camadas de 12)",
    ("vol-02", "2"): "contou só os cubinhos inteiros que cabem na caixa",
    ("vol-02", "3"): "calculou só a área da base (2,5 · 1,2)",
    ("vol-02", "6"): "trocou √2 por 2 (2,5 · 1,2 · 2)",
    ("vol-03", "6"): "somou as três arestas (2 + 2 + 2)",
    ("vol-03", "4"): "calculou a área de uma face (2 · 2)",
    ("vol-03", "24"): "calculou a área total (6 faces de área 4)",
}

NUMERO = r"[0-9]+(?:[.,][0-9]+)?"


def valor(texto: str) -> float:
    """Mesmas regras do InterpretadorDeMedida (inteiro, decimal, fração, √n, k√n)."""
    t = re.sub(r"\s+", "", texto)
    if re.fullmatch(NUMERO, t):
        return float(t.replace(",", "."))
    m = re.fullmatch(r"([0-9]+)/([0-9]+)", t)
    if m:
        return int(m.group(1)) / int(m.group(2))
    m = re.fullmatch(rf"({NUMERO})?√\(?({NUMERO})\)?", t)
    if m:
        k = float(m.group(1).replace(",", ".")) if m.group(1) else 1.0
        return k * math.sqrt(float(m.group(2).replace(",", ".")))
    raise ValueError(f"medida que o script não entende: {texto!r}")


def numero(x: float) -> str:
    if abs(x - round(x)) < 1e-9:
        return str(int(round(x)))
    return f"{x:.2f}".replace(".", ",")


def forma_exata(x: float) -> str:
    """Escreve x como inteiro, k√n ou √n quando x² é inteiro; senão, decimal."""
    if abs(x - round(x)) < 1e-9:
        return str(int(round(x)))
    quadrado = x * x
    if abs(quadrado - round(quadrado)) < 1e-9:
        n = int(round(quadrado))
        k = 1
        for f in range(int(math.isqrt(n)), 1, -1):
            if n % (f * f) == 0:
                k = f
                break
        resto = n // (k * k)
        exata = f"√{resto}" if k == 1 else f"{k}√{resto}"
        return f"{exata} ≈ {numero(x)}"
    # 3√2 etc.: tenta coeficientes racionais simples sobre raízes pequenas.
    for r in (2, 3, 5, 6, 7):
        k = x / math.sqrt(r)
        if abs(k - round(k)) < 1e-9:
            return f"{int(round(k))}√{r} ≈ {numero(x)}"
    return numero(x)


def resposta(d: dict) -> float:
    a, b, c = (valor(d["dimensoes"][k]) for k in "abc")
    return {
        "diagonalDaBase": math.sqrt(a * a + b * b),
        "diagonal": math.sqrt(a * a + b * b + c * c),
        "areaDaBase": a * b,
        "areaLateral": 2 * (a * c + b * c),
        "areaTotal": 2 * (a * b + a * c + b * c),
        "volume": a * b * c,
        "numeroDeVertices": 8,
        "numeroDeArestas": 12,
        "numeroDeFaces": 6,
    }[d["grandeza"]]


def celula(texto: str) -> str:
    return texto.replace("|", "\\|").replace("\n", "<br>")


def main() -> int:
    textos = json.loads((DADOS / "textos.json").read_text(encoding="utf-8"))["textos"]
    desafios = json.loads((DADOS / "desafios.json").read_text(encoding="utf-8"))["desafios"]

    faltando = [
        (d["id"], t) for d in desafios for t in d.get("distratores", [])
        if (d["id"], t) not in ERROS_DOS_DISTRATORES
    ]
    if faltando:
        print("Faltam explicações para os distratores:", faltando, file=sys.stderr)
        return 1

    porChave = {t["chave"]: t["texto"] for t in textos}
    pendentes = [c for grupo in PENDENTES.values() for c, _ in grupo]
    inexistentes = [c for c in pendentes if c not in porChave]
    repetidas = sorted({c for c in pendentes if pendentes.count(c) > 1})
    if inexistentes or repetidas:
        print("PENDENTES com chave inexistente:", inexistentes, "ou repetida:", repetidas, file=sys.stderr)
        return 1

    l = []
    l.append("# Laboratório do Paralelepípedo — revisão pedagógica")
    l.append("")
    l.append(f"Gerado em {date.today().strftime('%d/%m/%Y')} a partir de `Assets/_Projeto/Dados/textos.json` e "
             "`Assets/_Projeto/Dados/desafios.json`, com `python3 -I Tools/gerar_revisao_pedagogica.py`.")
    l.append("")
    l.append("**Como revisar:** marque ✓ na coluna *Ok?* ou escreva a sugestão na coluna *Sugestão*. "
             "Pode mudar qualquer texto, enunciado ou distrator; as respostas são calculadas pelo app a partir "
             "das dimensões da caixa.")
    l.append("")
    l.append("Observações gerais:")
    l.append("")
    l.append("- `{0}` e `{1}` são trocados por números na hora de mostrar (ex.: \"Vértices: 3 de 8\").")
    l.append("- Respostas não inteiras são aceitas com tolerância de ±0,01 (para √13 ≈ 3,6056, valem 3,60 e 3,61). "
             "Nas contagens (vértices, arestas, faces), só vale a resposta exata.")
    l.append("- Nos desafios de montar face, vale começar por qualquer vértice da face e seguir nos dois sentidos, "
             "voltando ou não ao vértice inicial (A → B → F → E, E → F → B → A…).")
    l.append("- Convenção dos vértices: ABCD no topo (A frente-esquerda, B fundo-esquerda, C fundo-direita, "
             "D frente-direita) e EFGH embaixo (E sob A, F sob B, G sob C, H sob D); a = EH, b = HG, c = AE.")
    l.append("")

    l.append(f"## 1. Textos a revisar primeiro [REVISAR-PROFESSOR] ({len(pendentes)})")
    l.append("")
    l.append("Rascunhos escritos durante o desenvolvimento, ainda sem o aval do professor. O app já os mostra ao "
             "aluno; confira a clareza para o 9º ano, o vocabulário e a correção matemática (principalmente das "
             "dicas, que explicam o erro de cada resposta errada).")
    l.append("")
    for versao, grupo in PENDENTES.items():
        l.append(f"### Versão {versao} ({len(grupo)})")
        l.append("")
        l.append("| Chave | Texto | Observação | Ok? | Sugestão |")
        l.append("|---|---|---|---|---|")
        for chave, obs in grupo:
            l.append(f"| `{chave}` **[REVISAR-PROFESSOR]** | {celula(porChave[chave])} | {celula(obs)} | | |")
        l.append("")

    l.append(f"## 2. Demais textos ({len(textos) - len(pendentes)})")
    l.append("")
    l.append("Textos mais antigos ou de interface (títulos, botões, painel de diagnóstico). Vale uma conferência geral.")
    l.append("")
    l.append("| Chave | Texto | Ok? | Sugestão |")
    l.append("|---|---|---|---|")
    for t in textos:
        if t["chave"] not in pendentes:
            l.append(f"| `{t['chave']}` | {celula(t['texto'])} | | |")
    l.append("")

    l.append("## 3. Desafios")
    l.append("")
    for chave, titulo in ESTACOES:
        doEstacao = [d for d in desafios if d["estacao"] == chave]
        bnccs = sorted({d["bncc"] for d in doEstacao})
        l.append(f"### {titulo} (BNCC {', '.join(bnccs)})")
        l.append("")
        l.append("| Id | Enunciado | Caixa (a × b × c) | Pergunta | Resposta esperada | "
                 "Distratores e o erro que cada um imita | Ok? | Sugestão |")
        l.append("|---|---|---|---|---|---|---|---|")
        for d in doEstacao:
            caixa = " × ".join(d["dimensoes"][k] for k in "abc")
            if d.get("tipo") == "montarFace":
                pergunta = "montar uma face"
                face = d["face"]
                esperada = ("qualquer uma das 6 faces" if face.lower() == "qualquer"
                            else f"face {face} (qualquer vértice inicial, nos dois sentidos)")
                distratores = "—"
            else:
                pergunta = PERGUNTAS[d["grandeza"]]
                esperada = forma_exata(resposta(d))
                distratores = "<br>".join(
                    f"**{t}**: {ERROS_DOS_DISTRATORES[(d['id'], t)]}" for t in d.get("distratores", []))
            l.append(f"| `{d['id']}` | {celula(d['enunciado'])} | {caixa} | {pergunta} | {esperada} | "
                     f"{distratores} | | |")
        l.append("")

    SAIDA.parent.mkdir(parents=True, exist_ok=True)
    SAIDA.write_text("\n".join(l), encoding="utf-8")
    print(f"Gerado: {SAIDA.relative_to(RAIZ)} ({len(textos)} textos, {len(desafios)} desafios)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
