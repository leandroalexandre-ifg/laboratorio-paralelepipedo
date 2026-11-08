namespace LaboratorioParalelepipedo.XR
{
    /// <summary>Um toque de vibração do controle: frequência e força (0 a 1) e duração em segundos.</summary>
    public readonly struct PulsoDeVibracao
    {
        public PulsoDeVibracao(float frequencia, float forca, float duracao)
        {
            Frequencia = frequencia;
            Forca = forca;
            Duracao = duracao;
        }

        public float Frequencia { get; }
        public float Forca { get; }
        public float Duracao { get; }
    }
}
