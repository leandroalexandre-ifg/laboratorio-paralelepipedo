using System;
using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Paralelepípedo reto-retângulo de dimensões a (comprimento), b (largura) e c (altura).
    /// Convenção dos vértices: ABCD no topo (A frente-esquerda, B fundo-esquerda, C fundo-direita,
    /// D frente-direita) e EFGH embaixo, com E sob A, F sob B, G sob C e H sob D; a = EH, b = HG, c = AE.
    /// </summary>
    public sealed class Paralelepipedo : IPrisma
    {
        /// <summary>Comprimento (a = EH).</summary>
        public double A { get; }

        /// <summary>Largura (b = HG).</summary>
        public double B { get; }

        /// <summary>Altura (c = AE).</summary>
        public double C { get; }

        /// <summary>Os 8 vértices, na ordem A, B, C, D, E, F, G, H.</summary>
        public IReadOnlyList<Vertice> Vertices { get; }

        /// <summary>As 12 arestas: AB, BC, CD, DA, EF, FG, GH, HE, AE, BF, CG, DH.</summary>
        public IReadOnlyList<Aresta> Arestas { get; }

        /// <summary>As 6 faces em ordem cíclica: ABCD, EFGH, ADHE, BCGF, ABFE, DCGH.</summary>
        public IReadOnlyList<Face> Faces { get; }

        /// <exception cref="ArgumentOutOfRangeException">Se alguma dimensão não for um número real positivo e finito.</exception>
        public Paralelepipedo(double a, double b, double c)
        {
            A = Validar(a, nameof(a));
            B = Validar(b, nameof(b));
            C = Validar(c, nameof(c));

            // x = comprimento (a), y = altura (c), z = largura (b, da frente para o fundo).
            var vA = new Vertice("A", new Vetor3(0, C, 0));
            var vB = new Vertice("B", new Vetor3(0, C, B));
            var vC = new Vertice("C", new Vetor3(A, C, B));
            var vD = new Vertice("D", new Vetor3(A, C, 0));
            var vE = new Vertice("E", new Vetor3(0, 0, 0));
            var vF = new Vertice("F", new Vetor3(0, 0, B));
            var vG = new Vertice("G", new Vetor3(A, 0, B));
            var vH = new Vertice("H", new Vetor3(A, 0, 0));
            Vertices = new[] { vA, vB, vC, vD, vE, vF, vG, vH };

            Arestas = new[]
            {
                new Aresta(vA, vB), new Aresta(vB, vC), new Aresta(vC, vD), new Aresta(vD, vA),
                new Aresta(vE, vF), new Aresta(vF, vG), new Aresta(vG, vH), new Aresta(vH, vE),
                new Aresta(vA, vE), new Aresta(vB, vF), new Aresta(vC, vG), new Aresta(vD, vH),
            };

            Faces = new[]
            {
                new Face(new[] { vA, vB, vC, vD }, A * B, ParDeFaces.Bases),
                new Face(new[] { vE, vF, vG, vH }, A * B, ParDeFaces.Bases),
                new Face(new[] { vA, vD, vH, vE }, A * C, ParDeFaces.FrenteEFundo),
                new Face(new[] { vB, vC, vG, vF }, A * C, ParDeFaces.FrenteEFundo),
                new Face(new[] { vA, vB, vF, vE }, B * C, ParDeFaces.EsquerdaEDireita),
                new Face(new[] { vD, vC, vG, vH }, B * C, ParDeFaces.EsquerdaEDireita),
            };
        }

        /// <summary>Diagonal da base: d = √(a² + b²) (ex.: EG).</summary>
        public double DiagonalDaBase => Math.Sqrt(A * A + B * B);

        /// <summary>
        /// Diagonal do paralelepípedo: D = √(d² + c²) = √(a² + b² + c²) (ex.: EC).
        /// O triângulo de catetos d e c e hipotenusa D é retângulo.
        /// </summary>
        public double Diagonal => Math.Sqrt(A * A + B * B + C * C);

        /// <summary>Altura do prisma: c.</summary>
        public double Altura => C;

        /// <summary>Área da base: A_b = a·b.</summary>
        public double AreaDaBase => A * B;

        /// <summary>Perímetro da base: 2(a + b).</summary>
        public double PerimetroDaBase => 2 * (A + B);

        /// <summary>Área lateral (frente, fundo, esquerda e direita): A_L = 2(ac + bc).</summary>
        public double AreaLateral => 2 * (A * C + B * C);

        /// <summary>Área total: A_T = 2(ab + ac + bc).</summary>
        public double AreaTotal => 2 * (A * B + A * C + B * C);

        /// <summary>Volume: V = A_b · c = a·b·c.</summary>
        public double Volume => AreaDaBase * C;

        /// <summary>Valor da grandeza pedida (diagonal, área, volume ou contagem de elementos).</summary>
        /// <exception cref="ArgumentOutOfRangeException">Se a grandeza não existir.</exception>
        public double Medir(Grandeza grandeza)
        {
            switch (grandeza)
            {
                case Grandeza.DiagonalDaBase: return DiagonalDaBase;
                case Grandeza.Diagonal: return Diagonal;
                case Grandeza.AreaDaBase: return AreaDaBase;
                case Grandeza.AreaLateral: return AreaLateral;
                case Grandeza.AreaTotal: return AreaTotal;
                case Grandeza.Volume: return Volume;
                case Grandeza.NumeroDeVertices: return Vertices.Count;
                case Grandeza.NumeroDeArestas: return Arestas.Count;
                case Grandeza.NumeroDeFaces: return Faces.Count;
                default:
                    throw new ArgumentOutOfRangeException(nameof(grandeza), grandeza, "Grandeza desconhecida.");
            }
        }

        /// <summary>Vértice pelo nome ("A" a "H"), ou null se não existir.</summary>
        public Vertice ObterVertice(string nome)
        {
            for (int i = 0; i < Vertices.Count; i++)
                if (string.Equals(Vertices[i].Nome, nome, StringComparison.Ordinal))
                    return Vertices[i];
            return null;
        }

        /// <summary>Face pelo nome na ordem cíclica da convenção (ex.: "ABFE"), ou null se não existir.</summary>
        public Face ObterFace(string nome)
        {
            for (int i = 0; i < Faces.Count; i++)
                if (string.Equals(Faces[i].Nome, nome, StringComparison.Ordinal))
                    return Faces[i];
            return null;
        }

        /// <summary>
        /// Cubo de aresta a: paralelepípedo com a = b = c. Todas as fórmulas do paralelepípedo valem direto.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se a aresta não for um número real positivo e finito.</exception>
        public static Paralelepipedo Cubo(double aresta)
        {
            if (double.IsNaN(aresta) || double.IsInfinity(aresta) || aresta <= 0)
                throw new ArgumentOutOfRangeException(nameof(aresta), aresta,
                    "A aresta do cubo deve ser um número real positivo e finito.");
            return new Paralelepipedo(aresta, aresta, aresta);
        }

        /// <summary>Verdadeiro se a = b = c (a menos de erro de arredondamento).</summary>
        public bool EhCubo => Numeros.QuaseIguais(A, B) && Numeros.QuaseIguais(B, C);

        private static double Validar(double valor, string nome)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor) || valor <= 0)
                throw new ArgumentOutOfRangeException(nome, valor,
                    $"A dimensão {nome} deve ser um número real positivo e finito.");
            return valor;
        }

        public override string ToString() => $"{A} × {B} × {C}";
    }
}
