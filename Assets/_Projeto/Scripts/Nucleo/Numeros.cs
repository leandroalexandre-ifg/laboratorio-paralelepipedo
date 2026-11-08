using System;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>Comparações com tolerância para erros de arredondamento de ponto flutuante.</summary>
    internal static class Numeros
    {
        /// <summary>Tolerância relativa para considerar dois números iguais (ex.: 0,1 · 30 e 3).</summary>
        public const double Epsilon = 1e-9;

        /// <summary>Verdadeiro se x e y são iguais a menos de um erro de arredondamento.</summary>
        public static bool QuaseIguais(double x, double y) =>
            Math.Abs(x - y) <= Epsilon * Math.Max(1.0, Math.Max(Math.Abs(x), Math.Abs(y)));

        /// <summary>Verdadeiro se x é um número natural positivo (1, 2, 3…), a menos de arredondamento.</summary>
        public static bool EhNaturalPositivo(double x)
        {
            double inteiro = Math.Round(x);
            return inteiro >= 1 && QuaseIguais(x, inteiro);
        }

        /// <summary>Parte inteira de x ≥ 0, tratando 2,9999999999 como 3.</summary>
        public static long ParteInteira(double x)
        {
            double inteiro = Math.Round(x);
            return QuaseIguais(x, inteiro) ? (long)inteiro : (long)Math.Floor(x);
        }
    }
}
