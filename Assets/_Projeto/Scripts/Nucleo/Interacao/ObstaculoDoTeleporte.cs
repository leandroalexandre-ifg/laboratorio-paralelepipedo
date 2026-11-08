namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>Volume onde não se pode chegar por teleporte: um retângulo no chão até uma altura.</summary>
    public readonly struct ObstaculoDoTeleporte
    {
        public ObstaculoDoTeleporte(AreaRetangular area, double baseY, double topoY, MotivoDoTeleporte motivo)
        {
            Area = area;
            BaseY = baseY;
            TopoY = topoY;
            Motivo = motivo;
        }

        public AreaRetangular Area { get; }
        public double BaseY { get; }
        public double TopoY { get; }
        public MotivoDoTeleporte Motivo { get; }

        /// <summary>Se o ponto está dentro do volume.</summary>
        public bool Contem(Vetor3 p) => p.Y >= BaseY && p.Y <= TopoY && Area.Contem(p.X, p.Z);
    }
}
