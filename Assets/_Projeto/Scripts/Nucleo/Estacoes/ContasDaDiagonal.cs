namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>
    /// Valores das contas da estação de diagonais, já como texto, para os modelos do textos.json:
    /// d² = a² + b² → d² = 4 + 9 → d = √13 e D² = d² + c² → D² = 13 + 36 = 49 → D = 7.
    /// </summary>
    public static class ContasDaDiagonal
    {
        public static string A(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.A);
        public static string B(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.B);
        public static string C(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.C);

        public static string QuadradoDeA(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.A * caixa.A);
        public static string QuadradoDeB(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.B * caixa.B);
        public static string QuadradoDeC(Paralelepipedo caixa) => FormatoDeConta.Numero(caixa.C * caixa.C);

        /// <summary>d² = a² + b².</summary>
        public static string QuadradoDaDiagonalDaBase(Paralelepipedo caixa) =>
            FormatoDeConta.Numero(caixa.A * caixa.A + caixa.B * caixa.B);

        /// <summary>D² = a² + b² + c².</summary>
        public static string QuadradoDaDiagonal(Paralelepipedo caixa) =>
            FormatoDeConta.Numero(caixa.A * caixa.A + caixa.B * caixa.B + caixa.C * caixa.C);

        /// <summary>d, como raiz simplificada (√13) ou inteiro (5).</summary>
        public static string DiagonalDaBase(Paralelepipedo caixa) =>
            FormatoDeConta.Raiz(caixa.A * caixa.A + caixa.B * caixa.B);

        /// <summary>D, como raiz simplificada (2√3) ou inteiro (7).</summary>
        public static string Diagonal(Paralelepipedo caixa) =>
            FormatoDeConta.Raiz(caixa.A * caixa.A + caixa.B * caixa.B + caixa.C * caixa.C);
    }
}
