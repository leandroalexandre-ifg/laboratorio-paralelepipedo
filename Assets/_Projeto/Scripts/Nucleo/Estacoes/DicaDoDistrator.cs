using LaboratorioParalelepipedo.Nucleo.Dados;

namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>
    /// Dica para uma resposta errada na pergunta da diagonal D, conforme o erro que ela imita: D² (esqueceu a raiz),
    /// a + b + c (somou as medidas em vez dos quadrados) ou d (parou na diagonal da base). Fora disso, uma dica geral.
    /// O professor não precisa escrever dicas no desafios.json: a escolha é calculada.
    /// </summary>
    public static class DicaDoDistrator
    {
        public static string Chave(Paralelepipedo caixa, double valor, double tolerancia)
        {
            double quadradoDaDiagonal = caixa.A * caixa.A + caixa.B * caixa.B + caixa.C * caixa.C;
            if (Perto(valor, quadradoDaDiagonal, tolerancia)) return ChavesDeTexto.DiagonaisDicaSemRaiz;
            if (Perto(valor, caixa.A + caixa.B + caixa.C, tolerancia)) return ChavesDeTexto.DiagonaisDicaSomou;
            if (Perto(valor, caixa.DiagonalDaBase, tolerancia)) return ChavesDeTexto.DiagonaisDicaDiagonalDaBase;
            return ChavesDeTexto.DiagonaisDicaGeral;
        }

        private static bool Perto(double x, double y, double tolerancia) =>
            AvaliadorDeResposta.Coincidem(x, y, tolerancia > 0 ? tolerancia : DesafioDeResposta.ToleranciaPadrao);
    }
}
