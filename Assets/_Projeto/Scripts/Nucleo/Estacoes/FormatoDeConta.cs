using System;

namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>
    /// Números e raízes como o aluno escreve: vírgula decimal, sem zeros à direita, e raízes simplificadas
    /// (√13, 2√3; √49 vira 7). Para quadrados não inteiros, a raiz fica indicada: √(6,25 + 4).
    /// </summary>
    public static class FormatoDeConta
    {
        private const long MaiorRadicandoSimplificado = 1_000_000_000;

        /// <summary>Número com vírgula decimal, arredondado a 4 casas para esconder erros de ponto flutuante.</summary>
        public static string Numero(double valor) => Medida.FormatarNumero(Math.Round(valor, 4));

        /// <summary>Raiz quadrada de <paramref name="quadrado"/> como texto: "7", "√13", "2√3" ou "√(6,25)".</summary>
        public static string Raiz(double quadrado)
        {
            if (quadrado < 0 || double.IsNaN(quadrado) || double.IsInfinity(quadrado))
                throw new ArgumentOutOfRangeException(nameof(quadrado), "A raiz precisa de um número real ≥ 0.");

            double arredondado = Math.Round(quadrado);
            if (!Numeros.QuaseIguais(quadrado, arredondado) || arredondado > MaiorRadicandoSimplificado)
                return "√(" + Numero(quadrado) + ")";

            long n = (long)arredondado;
            if (n == 0) return "0";
            long fora = 1, dentro = n;
            for (long k = 2; k * k <= dentro; k++)
            {
                while (dentro % (k * k) == 0)
                {
                    dentro /= k * k;
                    fora *= k;
                }
            }

            if (dentro == 1) return fora.ToString();
            return fora == 1 ? "√" + dentro : fora + "√" + dentro;
        }
    }
}
