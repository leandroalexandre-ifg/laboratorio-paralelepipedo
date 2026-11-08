using System;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Avalia a resposta do aluno, com a tolerância do desafio para respostas reais
    /// (ex.: com ±0,01, aceita 3,61 e 3,60 para √13 ≈ 3,6056 e recusa 3,62).
    /// Serve tanto para um número digitado quanto para o valor da opção escolhida.
    /// </summary>
    public static class AvaliadorDeResposta
    {
        public static ResultadoDaAvaliacao Avaliar(Desafio desafio, double resposta)
        {
            if (desafio == null) throw new ArgumentNullException(nameof(desafio));

            double correta = desafio.RespostaCorreta;
            if (double.IsNaN(resposta) || double.IsInfinity(resposta))
                return new ResultadoDaAvaliacao(false, resposta, correta, double.NaN);

            return new ResultadoDaAvaliacao(
                Coincidem(resposta, correta, desafio.Tolerancia), resposta, correta, Math.Abs(resposta - correta));
        }

        /// <summary>
        /// Verdadeiro se |x − y| ≤ tolerância, sem recusar por erro de arredondamento na borda
        /// (7,01 conta como 7 com tolerância 0,01).
        /// </summary>
        internal static bool Coincidem(double x, double y, double tolerancia)
        {
            double folga = Numeros.Epsilon * Math.Max(1.0, Math.Max(Math.Abs(x), Math.Abs(y)));
            return Math.Abs(x - y) <= tolerancia + folga;
        }
    }
}
