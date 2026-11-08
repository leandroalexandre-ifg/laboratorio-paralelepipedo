namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>Consultas sobre <see cref="Grandeza"/>.</summary>
    public static class Grandezas
    {
        /// <summary>Verdadeiro para as contagens de vértices, arestas e faces (respostas inteiras).</summary>
        public static bool EhContagem(this Grandeza grandeza) =>
            grandeza == Grandeza.NumeroDeVertices ||
            grandeza == Grandeza.NumeroDeArestas ||
            grandeza == Grandeza.NumeroDeFaces;
    }
}
