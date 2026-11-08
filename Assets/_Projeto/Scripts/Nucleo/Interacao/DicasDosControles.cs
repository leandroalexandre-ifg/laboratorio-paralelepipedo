using LaboratorioParalelepipedo.Nucleo.Dados;

namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>
    /// Dica curta perto da mão, conforme o que ela está fazendo. Mirando o teleporte: "Solte para ir até lá" ou
    /// "Aqui não dá". Com o raio na caixa (sem segurá-la): "Segure o botão lateral para pegar". Fora disso, nenhuma
    /// (<c>null</c>). O teleporte vem antes porque o raio some enquanto a mão mira. Textos no textos.json.
    /// </summary>
    public static class DicasDosControles
    {
        public static string Chave(bool mirandoTeleporte, bool teleporteValido, bool apontandoParaPegar, bool segurando)
        {
            if (mirandoTeleporte)
                return teleporteValido ? ChavesDeTexto.DicaTeleporteValido : ChavesDeTexto.DicaTeleporteInvalido;
            if (apontandoParaPegar && !segurando) return ChavesDeTexto.DicaPegar;
            return null;
        }
    }
}
