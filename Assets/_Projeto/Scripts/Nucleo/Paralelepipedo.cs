using System;

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

        /// <exception cref="ArgumentOutOfRangeException">Se alguma dimensão não for um número real positivo e finito.</exception>
        public Paralelepipedo(double a, double b, double c)
        {
            A = Validar(a, nameof(a));
            B = Validar(b, nameof(b));
            C = Validar(c, nameof(c));
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
