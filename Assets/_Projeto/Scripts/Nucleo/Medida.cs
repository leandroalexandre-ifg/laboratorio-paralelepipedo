using System.Globalization;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Medida com o valor numérico e o texto para mostrar ao aluno (ex.: valor 1,41421… e texto "√2").
    /// O texto vem em forma canônica, com vírgula decimal: "2.5" vira "2,5" e "√(13)" vira "√13".
    /// </summary>
    public readonly struct Medida
    {
        public double Valor { get; }
        public string Texto { get; }

        public Medida(double valor, string texto)
        {
            Valor = valor;
            Texto = string.IsNullOrEmpty(texto) ? FormatarNumero(valor) : texto;
        }

        /// <summary>Medida a partir de um número; o texto é o número com vírgula decimal.</summary>
        public static implicit operator Medida(double valor) => new Medida(valor, null);

        /// <summary>Número com vírgula decimal e sem zeros à direita (2,5; 3; 0,05).</summary>
        public static string FormatarNumero(double valor) =>
            valor.ToString("0.##########", CultureInfo.InvariantCulture).Replace('.', ',');

        public override string ToString() => Texto;
    }
}
