namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Grandeza que um desafio pergunta. A lista é fechada; no JSON os nomes são
    /// diagonalDaBase, diagonal, areaDaBase, areaLateral, areaTotal, volume,
    /// numeroDeVertices, numeroDeArestas e numeroDeFaces.
    /// </summary>
    public enum Grandeza
    {
        DiagonalDaBase,
        Diagonal,
        AreaDaBase,
        AreaLateral,
        AreaTotal,
        Volume,

        /// <summary>Contagem (resposta inteira): 8 no paralelepípedo.</summary>
        NumeroDeVertices,

        /// <summary>Contagem (resposta inteira): 12 no paralelepípedo.</summary>
        NumeroDeArestas,

        /// <summary>Contagem (resposta inteira): 6 no paralelepípedo.</summary>
        NumeroDeFaces,
    }
}
