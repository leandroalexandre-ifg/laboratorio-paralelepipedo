using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Prisma reto: duas bases congruentes e paralelas ligadas por faces laterais retangulares.
    /// Hoje só o <see cref="Paralelepipedo"/> (e o cubo, caso particular dele) implementa esta interface;
    /// prismas de outras bases entram depois sem mudar quem usa a interface.
    /// Relações que todo prisma reto cumpre:
    /// AreaLateral = PerimetroDaBase · Altura, AreaTotal = 2 · AreaDaBase + AreaLateral, Volume = AreaDaBase · Altura.
    /// </summary>
    public interface IPrisma
    {
        IReadOnlyList<Vertice> Vertices { get; }
        IReadOnlyList<Aresta> Arestas { get; }

        /// <summary>Faces com os vértices em ordem cíclica.</summary>
        IReadOnlyList<Face> Faces { get; }

        double Altura { get; }
        double AreaDaBase { get; }
        double PerimetroDaBase { get; }
        double AreaLateral { get; }
        double AreaTotal { get; }
        double Volume { get; }
    }
}
