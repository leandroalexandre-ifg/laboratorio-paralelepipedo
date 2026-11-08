#!/usr/bin/env python3
"""Confere se as fontes do app têm todos os caracteres que o aluno pode ver.

Junta os caracteres de textos.json e desafios.json (textos, enunciados, dimensões e distratores) com os
símbolos que o código de exibição usa, e procura cada um nas fontes, na ordem dada (a primeira é a principal;
as outras são os fallbacks do TextMeshPro, na mesma ordem).

Uso (precisa do fonttools: pip install fonttools):
    python3 Tools/verificar_glifos.py Assets/_Projeto/Fontes/Nunito-SemiBold.ttf [fallback.ttf ...]

Sai com código 1 se algum caractere não estiver em nenhuma fonte.
"""
import json
import re
import sys
import unicodedata
from pathlib import Path

from fontTools.ttLib import TTFont

RAIZ = Path(__file__).resolve().parent.parent
DADOS = RAIZ / "Assets/_Projeto/Dados"

# Símbolos que o código de exibição monta além dos JSON (dimensões "2 × 3 × 6", "√13 ≈ 3,61", rótulos A–H…).
SIMBOLOS_DO_CODIGO = "0123456789ABCDEFGH×≈√²³·,.+−-=()/: "

# Tags de rich text do TextMeshPro não são desenhadas.
TAG = re.compile(r"</?(sub|sup|b|i|u|color|size)[^>]*>")


def caracteres_dos_dados() -> set:
    textos = json.loads((DADOS / "textos.json").read_text(encoding="utf-8"))["textos"]
    desafios = json.loads((DADOS / "desafios.json").read_text(encoding="utf-8"))["desafios"]
    partes = [t["texto"] for t in textos]
    for d in desafios:
        partes.append(d["enunciado"])
        partes.extend(d["dimensoes"].values())
        partes.extend(d.get("distratores", []))
        partes.append(d.get("face", ""))
    texto = TAG.sub("", "".join(partes)).replace("{0}", "").replace("{1}", "")
    return set(texto) | set(SIMBOLOS_DO_CODIGO)


def main(caminhos: list) -> int:
    if not caminhos:
        print(__doc__)
        return 2
    fontes = [(Path(c).name, TTFont(c).getBestCmap()) for c in caminhos]
    faltando = []
    por_fonte = {nome: [] for nome, _ in fontes}
    for ch in sorted(caracteres_dos_dados()):
        if ch in "\n\r\t":
            continue
        dona = next((nome for nome, cmap in fontes if ord(ch) in cmap), None)
        if dona is None:
            faltando.append(ch)
        elif dona != fontes[0][0]:
            por_fonte[dona].append(ch)

    total = len(caracteres_dos_dados())
    print(f"{total} caracteres conferidos.")
    for nome, chars in por_fonte.items():
        if chars:
            lista = ", ".join(f"{c} (U+{ord(c):04X} {unicodedata.name(c, '?')})" for c in chars)
            print(f"Vêm do fallback {nome}: {lista}")
    if faltando:
        lista = ", ".join(f"{c!r} (U+{ord(c):04X} {unicodedata.name(c, '?')})" for c in faltando)
        print(f"FALTAM em todas as fontes: {lista}")
        return 1
    print("Todos os caracteres têm glifo.")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
