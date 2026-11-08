using System;
using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Desafio em que o aluno escolhe ou informa um valor (ex.: "Quanto mede a diagonal da base?").
    /// A resposta correta é sempre calculada a partir das dimensões e da grandeza: quem escreve o desafio
    /// nunca digita a resposta.
    /// </summary>
    public sealed class DesafioDeResposta : Desafio
    {
        /// <summary>Tolerância padrão para respostas reais: ±0,01 (aceita 3,60 e 3,61 para √13 ≈ 3,6056).</summary>
        public const double ToleranciaPadrao = 0.01;

        private readonly Medida[] _distratores;

        public override TipoDeDesafio Tipo => TipoDeDesafio.Resposta;
        public Grandeza Grandeza { get; }
        public double Tolerancia { get; }

        /// <summary>Opções erradas mostradas junto com a resposta correta.</summary>
        public IReadOnlyList<Medida> Distratores => _distratores;

        /// <summary>Resposta correta, calculada a partir da caixa.</summary>
        public double RespostaCorreta => Caixa.Medir(Grandeza);

        /// <summary>Tolerância padrão da grandeza: 0 para contagens (resposta inteira), ±0,01 para as demais.</summary>
        public static double ToleranciaPadraoPara(Grandeza grandeza) => grandeza.EhContagem() ? 0 : ToleranciaPadrao;

        /// <exception cref="DesafioInvalidoException">Se algum dado do desafio for inválido.</exception>
        public DesafioDeResposta(
            string id,
            Estacao estacao,
            string codigoBncc,
            string enunciado,
            Medida comprimento,
            Medida largura,
            Medida altura,
            Grandeza grandeza,
            IReadOnlyList<Medida> distratores = null,
            double? tolerancia = null)
            : base(id, estacao, codigoBncc, enunciado, comprimento, largura, altura)
        {
            if (!Enum.IsDefined(typeof(Grandeza), grandeza))
                throw new DesafioInvalidoException(id, $"grandeza desconhecida ({(int)grandeza}).");

            double tol = tolerancia ?? ToleranciaPadraoPara(grandeza);
            if (double.IsNaN(tol) || double.IsInfinity(tol) || tol < 0)
                throw new DesafioInvalidoException(id, $"a tolerância não pode ser negativa (veio {tol}).");
            if (tol == 0 && !grandeza.EhContagem())
                throw new DesafioInvalidoException(id,
                    "para diagonais, áreas e volume, a tolerância deve ser maior que zero.");

            Grandeza = grandeza;
            Tolerancia = tol;
            _distratores = ValidarDistratores(id, distratores, Caixa.Medir(grandeza), tol);
        }

        private static Medida[] ValidarDistratores(
            string id, IReadOnlyList<Medida> distratores, double resposta, double tolerancia)
        {
            if (distratores == null) return Array.Empty<Medida>();

            string tol = Medida.FormatarNumero(tolerancia);
            var validados = new Medida[distratores.Count];
            for (int i = 0; i < validados.Length; i++)
            {
                Medida distrator = distratores[i];
                int numero = i + 1;
                if (double.IsNaN(distrator.Valor) || double.IsInfinity(distrator.Valor))
                    throw new DesafioInvalidoException(id, $"o distrator {numero} não é um número válido.");
                if (AvaliadorDeResposta.Coincidem(distrator.Valor, resposta, tolerancia))
                    throw new DesafioInvalidoException(id,
                        $"o distrator {numero} ({distrator.Texto}) é igual à resposta correta " +
                        $"({Medida.FormatarNumero(resposta)}) dentro da tolerância de {tol}.");
                for (int j = 0; j < i; j++)
                    if (AvaliadorDeResposta.Coincidem(distrator.Valor, validados[j].Valor, tolerancia))
                        throw new DesafioInvalidoException(id,
                            $"os distratores {j + 1} ({validados[j].Texto}) e {numero} ({distrator.Texto}) " +
                            $"são iguais dentro da tolerância de {tol}.");
                validados[i] = distrator;
            }

            return validados;
        }
    }
}
