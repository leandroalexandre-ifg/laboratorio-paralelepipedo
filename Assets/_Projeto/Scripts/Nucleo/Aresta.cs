using System;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>Aresta entre dois vértices; o nome junta os dois nomes (ex.: "AE").</summary>
    public sealed class Aresta
    {
        public Vertice Inicio { get; }
        public Vertice Fim { get; }
        public string Nome { get; }
        public double Comprimento { get; }

        public Aresta(Vertice inicio, Vertice fim)
        {
            Inicio = inicio ?? throw new ArgumentNullException(nameof(inicio));
            Fim = fim ?? throw new ArgumentNullException(nameof(fim));
            if (ReferenceEquals(inicio, fim))
                throw new ArgumentException("Uma aresta liga dois vértices diferentes.", nameof(fim));
            Nome = inicio.Nome + fim.Nome;
            Comprimento = Vetor3.Distancia(inicio.Posicao, fim.Posicao);
        }

        /// <summary>Verdadeiro se a aresta liga os dois vértices, em qualquer ordem.</summary>
        public bool Liga(Vertice p, Vertice q) =>
            (ReferenceEquals(Inicio, p) && ReferenceEquals(Fim, q)) ||
            (ReferenceEquals(Inicio, q) && ReferenceEquals(Fim, p));

        public override string ToString() => Nome;
    }
}
