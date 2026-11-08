using System;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>Vértice de um poliedro, com nome (ex.: "A") e posição.</summary>
    public sealed class Vertice
    {
        public string Nome { get; }
        public Vetor3 Posicao { get; }

        public Vertice(string nome, Vetor3 posicao)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O vértice precisa de um nome.", nameof(nome));
            Nome = nome;
            Posicao = posicao;
        }

        public override string ToString() => Nome;
    }
}
