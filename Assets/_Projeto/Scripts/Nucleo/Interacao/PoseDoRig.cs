namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>Posição do rig do óculos no mundo e o giro dele em torno do eixo vertical, em graus.</summary>
    public readonly struct PoseDoRig
    {
        public Vetor3 Posicao { get; }

        /// <summary>Giro em torno do eixo vertical, em graus, em [0, 360).</summary>
        public double Giro { get; }

        public PoseDoRig(Vetor3 posicao, double giro)
        {
            Posicao = posicao;
            Giro = giro;
        }
    }
}
