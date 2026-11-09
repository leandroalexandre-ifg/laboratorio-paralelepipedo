using System;
using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Planificação em cruz do paralelepípedo, aberta aos poucos (fração 0 = caixa fechada, 1 = aberta e plana em y = 0):
    /// a base EFGH fica parada; as 4 faces laterais giram 90° para fora em torno das arestas da base; a face de cima
    /// ABCD fica presa à de trás (BCGF) pela aresta BC e gira mais 90° em relação a ela. Coordenadas do núcleo
    /// (x = a, y = c, z = b). Aberta, ocupa x de −c a a + c e z de −c a 2b + c.
    /// </summary>
    public sealed class Planificacao
    {
        private readonly double _a, _b, _c;
        private readonly Dictionary<Face, LadoDaFace> _lados = new Dictionary<Face, LadoDaFace>(6);

        public Planificacao(Paralelepipedo caixa)
        {
            if (caixa == null) throw new ArgumentNullException(nameof(caixa));
            _a = caixa.A;
            _b = caixa.B;
            _c = caixa.C;
            foreach (Face face in caixa.Faces) _lados[face] = Calcular(face);   // uma vez: sem alocar a cada quadro
        }

        /// <summary>Largura da planificação aberta (x): a + 2c.</summary>
        public double LarguraAberta => _a + 2 * _c;

        /// <summary>Profundidade da planificação aberta (z): 2b + 2c.</summary>
        public double ProfundidadeAberta => 2 * _b + 2 * _c;

        /// <summary>Centro do retângulo que contém a planificação na fração dada (na fechada, o centro da base).</summary>
        public Vetor3 Centro(double fracao)
        {
            double t = Limitar(fracao);
            return new Vetor3(_a / 2, 0, _b / 2 + t * _b / 2);
        }

        /// <summary>Posição do <paramref name="ponto"/> da <paramref name="face"/> com a caixa aberta na fração dada.</summary>
        public Vetor3 Posicao(Face face, Vetor3 ponto, double fracao)
        {
            if (face == null) throw new ArgumentNullException(nameof(face));
            double angulo = Limitar(fracao) * Math.PI / 2;
            double cos = Math.Cos(angulo), sin = Math.Sin(angulo);
            double x = ponto.X, y = ponto.Y, z = ponto.Z;

            switch (Lado(face))
            {
                case LadoDaFace.Base:
                    return ponto;
                case LadoDaFace.Frente:   // gira em torno de EH (z = 0) para −z
                    return new Vetor3(x, y * cos, -y * sin);
                case LadoDaFace.Fundo:    // gira em torno de FG (z = b) para +z
                    return new Vetor3(x, y * cos, _b + y * sin);
                case LadoDaFace.Esquerda: // gira em torno de EF (x = 0) para −x
                    return new Vetor3(-y * sin, y * cos, z);
                case LadoDaFace.Direita:  // gira em torno de HG (x = a) para +x
                    return new Vetor3(_a + y * sin, y * cos, z);
                default:
                {
                    // Topo: preso à aresta BC da face de trás, que já girou; gira mais o mesmo ângulo em relação a ela.
                    double distanciaDeBC = _b - z;
                    double duplo = 2 * angulo;
                    return new Vetor3(x, _c * cos + distanciaDeBC * Math.Sin(duplo),
                        _b + _c * sin - distanciaDeBC * Math.Cos(duplo));
                }
            }
        }

        /// <summary>Os vértices da face, na ordem dela, com a caixa aberta na fração dada (sem alocar).</summary>
        public void PosicoesDaFace(Face face, double fracao, Vetor3[] destino)
        {
            if (face == null) throw new ArgumentNullException(nameof(face));
            if (destino == null || destino.Length < face.Vertices.Count)
                throw new ArgumentException("O destino precisa de uma posição por vértice da face.", nameof(destino));
            for (int i = 0; i < face.Vertices.Count; i++)
                destino[i] = Posicao(face, face.Vertices[i].Posicao, fracao);
        }

        private enum LadoDaFace { Base, Topo, Frente, Fundo, Esquerda, Direita }

        private LadoDaFace Lado(Face face) => _lados.TryGetValue(face, out LadoDaFace lado) ? lado : Calcular(face);

        // Pela posição dos vértices: a face inteira em y = 0 é a base, em y = c o topo, e assim por diante.
        private LadoDaFace Calcular(Face face)
        {
            if (Todos(face, p => Perto(p.Y, 0))) return LadoDaFace.Base;
            if (Todos(face, p => Perto(p.Y, _c))) return LadoDaFace.Topo;
            if (Todos(face, p => Perto(p.Z, 0))) return LadoDaFace.Frente;
            if (Todos(face, p => Perto(p.Z, _b))) return LadoDaFace.Fundo;
            if (Todos(face, p => Perto(p.X, 0))) return LadoDaFace.Esquerda;
            if (Todos(face, p => Perto(p.X, _a))) return LadoDaFace.Direita;
            throw new ArgumentException($"A face {face.Nome} não é desta caixa.", nameof(face));
        }

        private static bool Todos(Face face, Func<Vetor3, bool> condicao)
        {
            for (int i = 0; i < face.Vertices.Count; i++)
                if (!condicao(face.Vertices[i].Posicao)) return false;
            return true;
        }

        private static bool Perto(double x, double y) => Math.Abs(x - y) < 1e-9;

        private static double Limitar(double fracao) => fracao < 0 ? 0 : fracao > 1 ? 1 : fracao;
    }
}
