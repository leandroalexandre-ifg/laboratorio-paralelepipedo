using System;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Base dos desafios: id, estação, código da BNCC, enunciado e a caixa em que o desafio acontece.
    /// As dimensões guardam o texto escrito pelo professor (ex.: "√2") para mostrar ao aluno.
    /// </summary>
    public abstract class Desafio
    {
        /// <summary>Maior dimensão aceita nos desafios da estação de volume.</summary>
        public const double DimensaoMaximaNaEstacaoDeVolume = 10;

        public string Id { get; }
        public Estacao Estacao { get; }
        public string CodigoBncc { get; }
        public string Enunciado { get; }

        /// <summary>Medida a (comprimento), com o texto para mostrar.</summary>
        public Medida Comprimento { get; }

        /// <summary>Medida b (largura), com o texto para mostrar.</summary>
        public Medida Largura { get; }

        /// <summary>Medida c (altura), com o texto para mostrar.</summary>
        public Medida Altura { get; }

        public Paralelepipedo Caixa { get; }

        public abstract TipoDeDesafio Tipo { get; }

        /// <exception cref="DesafioInvalidoException">Se algum dado do desafio for inválido.</exception>
        protected Desafio(
            string id,
            Estacao estacao,
            string codigoBncc,
            string enunciado,
            Medida comprimento,
            Medida largura,
            Medida altura)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new DesafioInvalidoException(id, "falta o id.");
            if (!Enum.IsDefined(typeof(Estacao), estacao))
                throw new DesafioInvalidoException(id, $"estação desconhecida ({(int)estacao}).");
            if (string.IsNullOrWhiteSpace(codigoBncc))
                throw new DesafioInvalidoException(id, "falta o código da BNCC.");
            if (string.IsNullOrWhiteSpace(enunciado))
                throw new DesafioInvalidoException(id, "falta o enunciado.");

            Paralelepipedo caixa;
            try
            {
                caixa = new Paralelepipedo(comprimento.Valor, largura.Valor, altura.Valor);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Medida errada = e.ParamName == "a" ? comprimento : e.ParamName == "b" ? largura : altura;
                throw new DesafioInvalidoException(id,
                    $"a dimensão {e.ParamName} deve ser um número positivo (veio {errada.Texto}).");
            }

            if (estacao == Estacao.Volume)
            {
                ConferirLimiteDeVolume(id, comprimento, "a");
                ConferirLimiteDeVolume(id, largura, "b");
                ConferirLimiteDeVolume(id, altura, "c");
            }

            Id = id;
            Estacao = estacao;
            CodigoBncc = codigoBncc;
            Enunciado = enunciado;
            Comprimento = comprimento;
            Largura = largura;
            Altura = altura;
            Caixa = caixa;
        }

        private static void ConferirLimiteDeVolume(string id, Medida medida, string nome)
        {
            if (medida.Valor > DimensaoMaximaNaEstacaoDeVolume &&
                !Numeros.QuaseIguais(medida.Valor, DimensaoMaximaNaEstacaoDeVolume))
                throw new DesafioInvalidoException(id,
                    $"na estação de volume, a dimensão {nome} deve ser no máximo " +
                    $"{DimensaoMaximaNaEstacaoDeVolume} (veio {medida.Texto}).");
        }
    }
}
