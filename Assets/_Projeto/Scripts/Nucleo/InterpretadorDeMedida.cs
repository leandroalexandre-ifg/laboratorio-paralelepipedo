using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Lê medidas escritas por professores nos JSON:
    /// inteiros ("2"), decimais com vírgula ou ponto ("2,5" e "2.5"), frações ("5/2"),
    /// raízes ("√2", "√(13)") e múltiplos de raiz ("3√2", "2,5√2"). Espaços são ignorados.
    /// Medidas não podem ser negativas.
    /// </summary>
    public static class InterpretadorDeMedida
    {
        private const string Numero = @"[0-9]+(?:[.,][0-9]+)?";

        private static readonly Regex SoNumero = new Regex($"^({Numero})$");
        private static readonly Regex Fracao = new Regex(@"^([0-9]+)/([0-9]+)$");
        private static readonly Regex Raiz = new Regex($@"^({Numero})?√(?:\(({Numero})\)|({Numero}))$");
        private static readonly Regex Espacos = new Regex(@"\s+");

        /// <summary>Exemplos mostrados nas mensagens de erro.</summary>
        public const string Exemplos = "2, 2,5, 2.5, 5/2, √2 ou 3√2";

        /// <summary>Lê a medida ou lança <see cref="FormatException"/> com mensagem clara.</summary>
        public static Medida Interpretar(string texto)
        {
            if (TentarInterpretar(texto, out var medida, out var erro)) return medida;
            throw new FormatException(erro);
        }

        /// <returns>Verdadeiro se o texto é uma medida válida; senão, <paramref name="erro"/> explica o problema.</returns>
        public static bool TentarInterpretar(string texto, out Medida medida, out string erro)
        {
            medida = default;
            if (string.IsNullOrWhiteSpace(texto))
            {
                erro = "a medida está vazia.";
                return false;
            }

            string limpo = Espacos.Replace(texto, string.Empty);
            if (limpo.StartsWith("-", StringComparison.Ordinal) || limpo.StartsWith("−", StringComparison.Ordinal))
            {
                erro = $"\"{texto.Trim()}\": medidas não podem ser negativas.";
                return false;
            }

            Match m;
            if ((m = SoNumero.Match(limpo)).Success)
            {
                double valor = LerNumero(m.Groups[1].Value);
                return Concluir(texto, valor, Medida.FormatarNumero(valor), out medida, out erro);
            }

            if ((m = Fracao.Match(limpo)).Success)
            {
                double numerador = LerNumero(m.Groups[1].Value);
                double denominador = LerNumero(m.Groups[2].Value);
                if (denominador == 0)
                {
                    erro = $"\"{texto.Trim()}\": o denominador da fração não pode ser zero.";
                    return false;
                }

                string rotulo = Medida.FormatarNumero(numerador) + "/" + Medida.FormatarNumero(denominador);
                return Concluir(texto, numerador / denominador, rotulo, out medida, out erro);
            }

            if ((m = Raiz.Match(limpo)).Success)
            {
                bool temCoeficiente = m.Groups[1].Success;
                double coeficiente = temCoeficiente ? LerNumero(m.Groups[1].Value) : 1;
                double radicando = LerNumero(m.Groups[2].Success ? m.Groups[2].Value : m.Groups[3].Value);
                string rotulo = (temCoeficiente ? Medida.FormatarNumero(coeficiente) : string.Empty) +
                                "√" + Medida.FormatarNumero(radicando);
                return Concluir(texto, coeficiente * Math.Sqrt(radicando), rotulo, out medida, out erro);
            }

            erro = $"\"{texto.Trim()}\" não é uma medida válida. Exemplos aceitos: {Exemplos}.";
            return false;
        }

        private static bool Concluir(string texto, double valor, string rotulo, out Medida medida, out string erro)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor))
            {
                medida = default;
                erro = $"\"{texto.Trim()}\" é grande demais.";
                return false;
            }

            medida = new Medida(valor, rotulo);
            erro = null;
            return true;
        }

        private static double LerNumero(string digitos)
        {
            try
            {
                return double.Parse(digitos.Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture);
            }
            catch (OverflowException)
            {
                return double.PositiveInfinity;
            }
        }
    }
}
