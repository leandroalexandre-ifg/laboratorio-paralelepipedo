namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>Resultado de avaliar a resposta do aluno a um desafio.</summary>
    public readonly struct ResultadoDaAvaliacao
    {
        public bool Correta { get; }
        public double RespostaDada { get; }
        public double RespostaCorreta { get; }

        /// <summary>|resposta dada − resposta correta| (NaN se a resposta dada não for um número).</summary>
        public double Diferenca { get; }

        public ResultadoDaAvaliacao(bool correta, double respostaDada, double respostaCorreta, double diferenca)
        {
            Correta = correta;
            RespostaDada = respostaDada;
            RespostaCorreta = respostaCorreta;
            Diferenca = diferenca;
        }
    }
}
