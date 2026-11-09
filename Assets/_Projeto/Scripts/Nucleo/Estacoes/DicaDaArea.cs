using LaboratorioParalelepipedo.Nucleo.Dados;

namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>
    /// Dica para uma resposta errada numa pergunta de área, conforme o erro que ela imita, calculada a partir das
    /// medidas (o professor não escreve dicas no desafios.json). Base: a + b, perímetro 2(a + b) ou volume. Lateral:
    /// a área total, ac + bc sem dobrar ou volume. Total: ab + ac + bc sem dobrar, só a lateral, um par esquecido ou
    /// volume. Fora disso, uma dica geral.
    /// </summary>
    public static class DicaDaArea
    {
        public static string Chave(Paralelepipedo caixa, Grandeza grandeza, double valor, double tolerancia)
        {
            double a = caixa.A, b = caixa.B, c = caixa.C;
            double ab = a * b, ac = a * c, bc = b * c;
            bool Perto(double x) =>
                AvaliadorDeResposta.Coincidem(valor, x, tolerancia > 0 ? tolerancia : DesafioDeResposta.ToleranciaPadrao);

            if (Perto(caixa.Volume)) return ChavesDeTexto.AreasDicaVolume;
            switch (grandeza)
            {
                case Grandeza.AreaDaBase:
                    if (Perto(a + b)) return ChavesDeTexto.AreasDicaSomou;
                    if (Perto(2 * (a + b))) return ChavesDeTexto.AreasDicaPerimetro;
                    break;
                case Grandeza.AreaLateral:
                    if (Perto(caixa.AreaTotal)) return ChavesDeTexto.AreasDicaComAsBases;
                    if (Perto(ac + bc)) return ChavesDeTexto.AreasDicaSemDobrar;
                    break;
                case Grandeza.AreaTotal:
                    if (Perto(ab + ac + bc)) return ChavesDeTexto.AreasDicaSemDobrar;
                    if (Perto(caixa.AreaLateral)) return ChavesDeTexto.AreasDicaSoLateral;
                    if (Perto(2 * (ab + ac)) || Perto(2 * (ab + bc))) return ChavesDeTexto.AreasDicaFaltouPar;
                    break;
            }
            return ChavesDeTexto.AreasDicaGeral;
        }
    }
}
