using System;
using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Paralelepípedo reto-retângulo de dimensões a (comprimento), b (largura) e c (altura).
    /// Convenção dos vértices: ABCD no topo (A frente-esquerda, B fundo-esquerda, C fundo-direita,
    /// D frente-direita) e EFGH embaixo, com E sob A, F sob B, G sob C e H sob D; a = EH, b = HG, c = AE.
    /// </summary>
    public sealed class Paralelepipedo
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
