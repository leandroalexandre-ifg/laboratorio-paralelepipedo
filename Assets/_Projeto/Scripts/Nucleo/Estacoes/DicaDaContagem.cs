using LaboratorioParalelepipedo.Nucleo.Dados;

namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>
    /// Dica para uma resposta errada numa pergunta de contagem, conforme o erro que ela imita: contou outro elemento
    /// (o número de vértices, de arestas ou de faces), contou só as arestas que se veem de um lado (3 ficam escondidas)
    /// ou só as faces dos lados (sem as 2 bases). Fora disso, uma dica geral. A escolha é calculada a partir do prisma:
    /// o professor não escreve dicas no desafios.json.
    /// </summary>
    public static class DicaDaContagem
    {
        /// <summary>Arestas escondidas de um ponto de vista comum (a caixa vista de um canto mostra 9 das 12).</summary>
        public const int ArestasEscondidas = 3;

        /// <summary>Faces que não são laterais (as 2 bases).</summary>
        public const int Bases = 2;

        public static string Chave(IPrisma prisma, Grandeza grandeza, double valor)
        {
            int vertices = prisma.Vertices.Count, arestas = prisma.Arestas.Count, faces = prisma.Faces.Count;
            if (grandeza == Grandeza.NumeroDeArestas && Igual(valor, arestas - ArestasEscondidas))
                return ChavesDeTexto.ElementosDicaArestasEscondidas;
            if (grandeza == Grandeza.NumeroDeFaces && Igual(valor, faces - Bases))
                return ChavesDeTexto.ElementosDicaSemAsBases;
            if (grandeza != Grandeza.NumeroDeVertices && Igual(valor, vertices)) return ChavesDeTexto.ElementosDicaVertices;
            if (grandeza != Grandeza.NumeroDeArestas && Igual(valor, arestas)) return ChavesDeTexto.ElementosDicaArestas;
            if (grandeza != Grandeza.NumeroDeFaces && Igual(valor, faces)) return ChavesDeTexto.ElementosDicaFaces;
            return ChavesDeTexto.ElementosDicaGeral;
        }

        private static bool Igual(double valor, int contagem) => System.Math.Abs(valor - contagem) < 1e-9;
    }
}
