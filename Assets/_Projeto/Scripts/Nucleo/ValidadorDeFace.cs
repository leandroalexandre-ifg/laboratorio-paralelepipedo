using System;
using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Diz se uma sequência de vértices tocados forma uma face do prisma.
    /// Vale qualquer vértice inicial, os dois sentidos e, opcionalmente, voltar ao vértice inicial
    /// (A → B → F → E e A → B → F → E → A identificam a face ABFE; E → F → B → A também).
    /// Funciona com qualquer <see cref="IPrisma"/>, não só com o paralelepípedo.
    /// </summary>
    public sealed class ValidadorDeFace
    {
        private readonly IPrisma _prisma;

        public ValidadorDeFace(IPrisma prisma)
        {
            _prisma = prisma ?? throw new ArgumentNullException(nameof(prisma));
        }

        /// <summary>
        /// Identifica a face formada pela sequência completa de nomes de vértices.
        /// </summary>
        /// <returns>Verdadeiro, com a face em <paramref name="face"/>, se a sequência forma uma face.</returns>
        public bool TentarIdentificar(IReadOnlyList<string> nomesTocados, out Face face)
        {
            if (nomesTocados == null) throw new ArgumentNullException(nameof(nomesTocados));

            foreach (var candidata in _prisma.Faces)
            {
                int n = candidata.Vertices.Count;
                bool completa = nomesTocados.Count == n ||
                                (nomesTocados.Count == n + 1 &&
                                 string.Equals(nomesTocados[0], nomesTocados[n], StringComparison.Ordinal));
                if (completa && SegueOContorno(candidata, nomesTocados))
                {
                    face = candidata;
                    return true;
                }
            }

            face = null;
            return false;
        }

        /// <summary>
        /// Verdadeiro se os vértices tocados até agora ainda podem completar alguma face
        /// (útil para avisar o aluno logo no toque errado). A sequência vazia sempre pode continuar.
        /// </summary>
        public bool PodeContinuar(IReadOnlyList<string> nomesTocados)
        {
            if (nomesTocados == null) throw new ArgumentNullException(nameof(nomesTocados));
            if (nomesTocados.Count == 0) return true;

            foreach (var candidata in _prisma.Faces)
                if (SegueOContorno(candidata, nomesTocados))
                    return true;
            return false;
        }

        /// <summary>
        /// Verdadeiro se a sequência percorre o contorno da face a partir de algum vértice, em algum sentido,
        /// sem passar de uma volta completa (no máximo n + 1 vértices, o último igual ao primeiro).
        /// </summary>
        private static bool SegueOContorno(Face face, IReadOnlyList<string> nomes)
        {
            int n = face.Vertices.Count;
            if (nomes.Count > n + 1) return false;

            for (int inicio = 0; inicio < n; inicio++)
            {
                if (Percorre(face, nomes, inicio, +1) || Percorre(face, nomes, inicio, -1))
                    return true;
            }

            return false;
        }

        private static bool Percorre(Face face, IReadOnlyList<string> nomes, int inicio, int sentido)
        {
            int n = face.Vertices.Count;
            for (int i = 0; i < nomes.Count; i++)
            {
                int indice = ((inicio + sentido * i) % n + n) % n;
                if (!string.Equals(face.Vertices[indice].Nome, nomes[i], StringComparison.Ordinal))
                    return false;
            }

            return true;
        }
    }
}
