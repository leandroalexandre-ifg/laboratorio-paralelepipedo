using System;

namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>Ações dos controles que podem estar disponíveis no build.</summary>
    [Flags]
    public enum AcoesDoControle
    {
        Nenhuma = 0,
        /// <summary>Apontar com o raio e confirmar com o gatilho (só há o que escolher a partir das estações).</summary>
        Apontar = 1,
        /// <summary>Pegar e soltar a caixa com o botão lateral.</summary>
        Pegar = 2,
        Teleporte = 4,
        Giro = 8,
    }
}
