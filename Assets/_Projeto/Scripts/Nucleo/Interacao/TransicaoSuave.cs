namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>
    /// Fração (0 a 1) de uma transição com começo e fim suaves (smoothstep), por exemplo a caixa voltando à mesa.
    /// </summary>
    public sealed class TransicaoSuave
    {
        /// <summary>Duração da volta da caixa à mesa ao ser solta, em segundos (~0,4 s).</summary>
        public const double DuracaoDoRetornoDaCaixa = 0.4;

        private double _tempo = -1;

        public TransicaoSuave(double duracao)
        {
            if (!(duracao > 0)) throw new System.ArgumentOutOfRangeException(nameof(duracao), "A duração precisa ser maior que zero.");
            Duracao = duracao;
        }

        public double Duracao { get; }

        /// <summary>Se a transição está em curso.</summary>
        public bool Ativa => _tempo >= 0;

        /// <summary>Começa (ou recomeça) do zero.</summary>
        public void Iniciar() => _tempo = 0;

        /// <summary>Interrompe sem terminar (a caixa foi pega de novo no meio da volta).</summary>
        public void Parar() => _tempo = -1;

        /// <summary>Avança e devolve a fração suavizada; no fim devolve 1 e deixa de estar ativa.</summary>
        public double Avancar(double segundos)
        {
            if (!Ativa) return 1;
            _tempo += segundos < 0 ? 0 : segundos;
            if (_tempo >= Duracao)
            {
                _tempo = -1;
                return 1;
            }

            double t = _tempo / Duracao;
            return t * t * (3 - 2 * t);
        }
    }
}
