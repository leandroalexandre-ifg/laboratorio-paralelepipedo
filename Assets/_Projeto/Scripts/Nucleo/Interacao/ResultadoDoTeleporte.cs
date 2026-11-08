namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>Resultado do arco do teleporte: onde ele termina, se o destino vale e quantos pontos desenhar.</summary>
    public readonly struct ResultadoDoTeleporte
    {
        public ResultadoDoTeleporte(Vetor3 pouso, MotivoDoTeleporte motivo, int quantidadeDePontos)
        {
            Pouso = pouso;
            Motivo = motivo;
            QuantidadeDePontos = quantidadeDePontos;
        }

        /// <summary>Onde o arco termina (no chão, ou onde bateu).</summary>
        public Vetor3 Pouso { get; }

        public MotivoDoTeleporte Motivo { get; }

        /// <summary>Pontos do arco preenchidos no buffer, do controle até o pouso.</summary>
        public int QuantidadeDePontos { get; }

        /// <summary>Se dá para teleportar até <see cref="Pouso"/>.</summary>
        public bool Valido => Motivo == MotivoDoTeleporte.Nenhum;
    }
}
