namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>Retângulo no chão (plano x–z, em metros), alinhado aos eixos.</summary>
    public readonly struct AreaRetangular
    {
        public AreaRetangular(double centroX, double centroZ, double largura, double profundidade)
        {
            CentroX = centroX;
            CentroZ = centroZ;
            MeiaLargura = largura / 2;
            MeiaProfundidade = profundidade / 2;
        }

        public double CentroX { get; }
        public double CentroZ { get; }
        public double MeiaLargura { get; }
        public double MeiaProfundidade { get; }

        /// <summary>Se o ponto (x, z) está dentro do retângulo (a borda conta como dentro).</summary>
        public bool Contem(double x, double z) =>
            System.Math.Abs(x - CentroX) <= MeiaLargura && System.Math.Abs(z - CentroZ) <= MeiaProfundidade;
    }
}
