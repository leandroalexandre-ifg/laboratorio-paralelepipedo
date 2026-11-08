using System;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Escala da caixa na cena, calculada para cada caixa (cada desafio): 1 unidade de medida vale
    /// <see cref="MetrosPorUnidadePadrao"/>, a não ser que a maior dimensão passe de
    /// <see cref="MaiorDimensaoMaximaEmMetros"/>; aí a escala diminui até a maior dimensão caber.
    /// O cubinho de aresta 1 da estação de volume usa a mesma escala.
    /// </summary>
    public static class EscalaDaCaixa
    {
        /// <summary>Maior dimensão que a caixa pode ocupar na mesa, em metros.</summary>
        public const double MaiorDimensaoMaximaEmMetros = 0.6;

        /// <summary>Escala das caixas pequenas: 1 unidade = 10 cm (a caixa 4 × 3 × 2 fica com 40 × 30 × 20 cm).</summary>
        public const double MetrosPorUnidadePadrao = 0.1;

        /// <summary>Metros na cena para cada unidade de medida do prisma.</summary>
        public static double MetrosPorUnidade(IPrisma prisma)
        {
            double maior = MaiorDimensao(prisma);
            return Math.Min(MetrosPorUnidadePadrao, MaiorDimensaoMaximaEmMetros / maior);
        }

        /// <summary>Escala da caixa do desafio.</summary>
        public static double MetrosPorUnidade(Desafio desafio) =>
            MetrosPorUnidade((desafio ?? throw new ArgumentNullException(nameof(desafio))).Caixa);

        /// <summary>Aresta do cubinho (1 unidade de medida) na cena, em metros.</summary>
        public static double ArestaDoCubinhoEmMetros(IPrisma prisma) => MetrosPorUnidade(prisma);

        /// <summary>Maior medida da caixa que envolve o prisma (comprimento, largura ou altura), em unidades.</summary>
        public static double MaiorDimensao(IPrisma prisma)
        {
            if (prisma == null) throw new ArgumentNullException(nameof(prisma));
            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;
            double minZ = double.MaxValue, maxZ = double.MinValue;
            foreach (var vertice in prisma.Vertices)
            {
                var p = vertice.Posicao;
                minX = Math.Min(minX, p.X);
                maxX = Math.Max(maxX, p.X);
                minY = Math.Min(minY, p.Y);
                maxY = Math.Max(maxY, p.Y);
                minZ = Math.Min(minZ, p.Z);
                maxZ = Math.Max(maxZ, p.Z);
            }

            return Math.Max(maxX - minX, Math.Max(maxY - minY, maxZ - minZ));
        }
    }
}
