#!/usr/bin/env python3
"""Confere se as fontes do app têm todos os caracteres do atlas.

Os caracteres do atlas seguem a mesma regra do CaracteresDoAtlas (núcleo, C#), que o Unity usa no menu
"Laboratório › Regenerar atlas da fonte":
  - todos os valores de texto de textos.json e desafios.json (textos, enunciados, medidas, distratores…);
  - mais Assets/_Projeto/Fontes/caracteres-extras.txt (ASCII, letras acentuadas e símbolos matemáticos,
    para que um texto novo do professor quase sempre caiba no atlas sem regenerar).

Cada caractere é procurado nas fontes na ordem dada (a primeira é a principal; as outras seriam fallbacks).

Uso (precisa do fonttools: pip install fonttools):
    python3 Tools/verificar_glifos.py Assets/_Projeto/Fontes/Nunito-SemiBold.ttf [fallback.ttf ...]
    python3 Tools/verificar_glifos.py --lista      # só mostra os caracteres do atlas

Sai com código 1 se algum caractere não estiver em nenhuma fonte.
"""
import json
import sys
import unicodedata
from pathlib import Path

RAIZ = Path(__file__).resolve().parent.parent
DADOS = RAIZ / "Assets/_Projeto/Dados"
EXTRAS = RAIZ / "Assets/_Projeto/Fontes/caracteres-extras.txt"


def _textos(no):
    """Todos os valores de texto (e números escritos sem aspas) de um JSON, em profundidade."""
    if isinstance(no, dict):
        for valor in no.values():
            yield from _textos(valor)
    elif isinstance(no, list):
        for item in no:
            yield from _textos(item)
    elif isinstance(no, str):
        yield no
    elif isinstance(no, (int, float)) and not isinstance(no, bool):
        yield str(no)


def caracteres_do_atlas() -> str:
    conjunto = set(EXTRAS.read_text(encoding="utf-8"))
    for arquivo in ("textos.json", "desafios.json"):
        for texto in _textos(json.loads((DADOS / arquivo).read_text(encoding="utf-8-sig"))):
            conjunto.update(texto)
    return "".join(sorted(c for c in conjunto if not unicodedata.category(c).startswith("C")))


def main(argumentos: list) -> int:
    if argumentos == ["--lista"]:
        print(caracteres_do_atlas())
        return 0
    if not argumentos:
        print(__doc__)
        return 2

    from fontTools.ttLib import TTFont

    fontes = [(Path(c).name, TTFont(c).getBestCmap()) for c in argumentos]
    caracteres = caracteres_do_atlas()
    faltando = []
    por_fonte = {nome: [] for nome, _ in fontes}
    for ch in caracteres:
        dona = next((nome for nome, cmap in fontes if ord(ch) in cmap), None)
        if dona is None:
            faltando.append(ch)
        elif dona != fontes[0][0]:
            por_fonte[dona].append(ch)

    print(f"{len(caracteres)} caracteres conferidos.")
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
