using System;
using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Face de um poliedro: vértices em ordem cíclica (ex.: A, B, F, E), área e par de faces a que pertence.
    /// </summary>
    public sealed class Face
    {
        private readonly Vertice[] _vertices;

        public string Nome { get; }
        public IReadOnlyList<Vertice> Vertices => _vertices;
        public double Area { get; }
        public ParDeFaces Par { get; }

        public Face(IReadOnlyList<Vertice> verticesEmOrdemCiclica, double area, ParDeFaces par)
        {
            if (verticesEmOrdemCiclica == null)
                throw new ArgumentNullException(nameof(verticesEmOrdemCiclica));
            if (verticesEmOrdemCiclica.Count < 3)
                throw new ArgumentException("Uma face tem pelo menos 3 vértices.", nameof(verticesEmOrdemCiclica));

            _vertices = new Vertice[verticesEmOrdemCiclica.Count];
            string nome = string.Empty;
            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i] = verticesEmOrdemCiclica[i] ??
                               throw new ArgumentException("Vértice nulo na face.", nameof(verticesEmOrdemCiclica));
                nome += _vertices[i].Nome;
            }

            Nome = nome;
            Area = area;
            Par = par;
        }

        /// <summary>Verdadeiro se o vértice pertence à face.</summary>
        public bool Contem(Vertice vertice) => Array.IndexOf(_vertices, vertice) >= 0;

        public override string ToString() => Nome;
    }
}
