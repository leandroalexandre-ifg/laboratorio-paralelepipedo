using System;
using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Pergunta sobre uma caixa (ex.: "Quanto mede a diagonal da base da caixa 2 × 3 × 6?").
    /// A resposta correta é sempre calculada a partir das dimensões e da grandeza: quem escreve o desafio
    /// nunca digita a resposta.
    /// </summary>
    public sealed class Desafio
    {
        /// <summary>Tolerância padrão para respostas reais: ±0,01 (aceita 3,60 e 3,61 para √13 ≈ 3,6056).</summary>
        public const double ToleranciaPadrao = 0.01;

        /// <summary>Maior dimensão aceita nos desafios da estação de volume.</summary>
        public const double DimensaoMaximaNaEstacaoDeVolume = 10;

        private readonly double[] _distratores;

        public string Id { get; }
        public Estacao Estacao { get; }
        public string CodigoBncc { get; }
        public string Enunciado { get; }
        public Paralelepipedo Caixa { get; }
        public Grandeza Grandeza { get; }
        public double Tolerancia { get; }

        /// <summary>Opções erradas mostradas junto com a resposta correta.</summary>
        public IReadOnlyList<double> Distratores => _distratores;

        /// <summary>Resposta correta, calculada a partir da caixa.</summary>
        public double RespostaCorreta => Caixa.Medir(Grandeza);

        /// <exception cref="DesafioInvalidoException">Se algum dado do desafio for inválido.</exception>
        public Desafio(
            string id,
            Estacao estacao,
            string codigoBncc,
            string enunciado,
            Paralelepipedo caixa,
            Grandeza grandeza,
            IReadOnlyList<double> distratores = null,
            double? tolerancia = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new DesafioInvalidoException(id, "falta o id.");
            if (!Enum.IsDefined(typeof(Estacao), estacao))
                throw new DesafioInvalidoException(id, $"estação desconhecida ({(int)estacao}).");
            if (string.IsNullOrWhiteSpace(codigoBncc))
                throw new DesafioInvalidoException(id, "falta o código da BNCC.");
            if (string.IsNullOrWhiteSpace(enunciado))
                throw new DesafioInvalidoException(id, "falta o enunciado.");
            if (caixa == null)
                throw new DesafioInvalidoException(id, "faltam as dimensões da caixa.");
            if (!Enum.IsDefined(typeof(Grandeza), grandeza))
                throw new DesafioInvalidoException(id, $"grandeza desconhecida ({(int)grandeza}).");

            double tol = tolerancia ?? ToleranciaPadrao;
            if (double.IsNaN(tol) || double.IsInfinity(tol) || tol <= 0)
                throw new DesafioInvalidoException(id, $"a tolerância deve ser um número positivo (veio {tol}).");

            if (estacao == Estacao.Volume)
                ValidarDimensoesDaEstacaoDeVolume(id, caixa);

            Id = id;
            Estacao = estacao;
            CodigoBncc = codigoBncc;
            Enunciado = enunciado;
            Caixa = caixa;
            Grandeza = grandeza;
            Tolerancia = tol;
            _distratores = ValidarDistratores(id, distratores, caixa.Medir(grandeza), tol);
        }

        private static void ValidarDimensoesDaEstacaoDeVolume(string id, Paralelepipedo caixa)
        {
            void Conferir(double valor, string nome)
            {
                if (valor > DimensaoMaximaNaEstacaoDeVolume &&
                    !Numeros.QuaseIguais(valor, DimensaoMaximaNaEstacaoDeVolume))
                    throw new DesafioInvalidoException(id,
                        $"na estação de volume, a dimensão {nome} deve ser no máximo " +
                        $"{DimensaoMaximaNaEstacaoDeVolume} (veio {valor}).");
            }

            Conferir(caixa.A, "a");
            Conferir(caixa.B, "b");
            Conferir(caixa.C, "c");
        }

        private static double[] ValidarDistratores(
            string id, IReadOnlyList<double> distratores, double resposta, double tolerancia)
        {
            if (distratores == null) return Array.Empty<double>();

            var validados = new double[distratores.Count];
            for (int i = 0; i < validados.Length; i++)
            {
                double valor = distratores[i];
                int numero = i + 1;
                if (double.IsNaN(valor) || double.IsInfinity(valor))
                    throw new DesafioInvalidoException(id, $"o distrator {numero} não é um número válido.");
                if (AvaliadorDeResposta.Coincidem(valor, resposta, tolerancia))
                    throw new DesafioInvalidoException(id,
                        $"o distrator {numero} ({valor}) é igual à resposta correta ({resposta}) " +
                        $"dentro da tolerância de {tolerancia}.");
                for (int j = 0; j < i; j++)
                    if (AvaliadorDeResposta.Coincidem(valor, validados[j], tolerancia))
                        throw new DesafioInvalidoException(id,
                            $"os distratores {j + 1} e {numero} são iguais dentro da tolerância de {tolerancia}.");
                validados[i] = valor;
            }

            return validados;
        }
    }
}
