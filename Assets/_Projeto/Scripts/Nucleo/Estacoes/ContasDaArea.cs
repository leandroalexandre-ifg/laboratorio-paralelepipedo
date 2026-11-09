namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>Os números das contas de área que o painel mostra (ex.: A<sub>L</sub> = 2(8 + 6) = 28), já formatados.</summary>
    public static class ContasDaArea
    {
        public static string A(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.A);
        public static string B(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.B);
        public static string C(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.C);
        public static string AB(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.A * caixa.B);
        public static string AC(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.A * caixa.C);
        public static string BC(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.B * caixa.C);
        public static string AreaDaBase(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.AreaDaBase);
        public static string AreaLateral(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.AreaLateral);
        public static string AreaTotal(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.AreaTotal);

        /// <summary>As duas medidas de uma face, a maior primeiro (ex.: 4 e 3 na base de uma caixa 4 × 3 × 2).</summary>
        public static (string maior, string menor) Lados(Face face)
        {
            Vetor3 p0 = face.Vertices[0].Posicao, p1 = face.Vertices[1].Posicao, p3 = face.Vertices[3].Posicao;
            double um = Vetor3.Distancia(p0, p1), outro = Vetor3.Distancia(p0, p3);
            return (FormatoDeConta.Numero(System.Math.Max(um, outro)), FormatoDeConta.Numero(System.Math.Min(um, outro)));
        }

        /// <summary>Área de uma face, formatada.</summary>
        public static string Area(Face face) => FormatoDeConta.Numero(face.Area);
    }
}
