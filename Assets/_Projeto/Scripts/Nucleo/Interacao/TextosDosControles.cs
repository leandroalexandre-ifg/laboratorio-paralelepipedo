using System.Collections.Generic;
using LaboratorioParalelepipedo.Nucleo.Dados;

namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>
    /// Chaves do painel "Controles": só as das ações que funcionam no build, sempre na mesma ordem
    /// (apontar, pegar, teleporte, giro). Os textos ficam no textos.json.
    /// </summary>
    public static class TextosDosControles
    {
        public static string[] Chaves(AcoesDoControle acoes)
        {
            var chaves = new List<string>(4);
            if ((acoes & AcoesDoControle.Apontar) != 0) chaves.Add(ChavesDeTexto.ControlesApontar);
            if ((acoes & AcoesDoControle.Pegar) != 0) chaves.Add(ChavesDeTexto.ControlesPegar);
            if ((acoes & AcoesDoControle.Teleporte) != 0) chaves.Add(ChavesDeTexto.ControlesTeleporte);
            if ((acoes & AcoesDoControle.Giro) != 0) chaves.Add(ChavesDeTexto.ControlesGiro);
            return chaves.ToArray();
        }
    }
}
