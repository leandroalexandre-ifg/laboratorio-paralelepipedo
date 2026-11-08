using System;

namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>
    /// Onde o aluno pode chegar por teleporte: no chão da sala, a pelo menos
    /// <see cref="DistanciaMinimaDasParedes"/> das paredes, fora da mesa e fora da caixa. Também traça o arco
    /// (parábola) que sai do controle e acha onde ele toca o chão ou bate num obstáculo. Em metros, y para cima,
    /// chão em y = 0. Sem alocação: o arco é escrito num buffer do chamador.
    /// </summary>
    public sealed class RegrasDoTeleporte
    {
        /// <summary>Distância mínima do destino até qualquer parede (decisão do projeto: 0,3 m).</summary>
        public const double DistanciaMinimaDasParedes = 0.3;

        /// <summary>Velocidade de saída do arco, em m/s (alcance de uns 4 m com o controle a 45°).</summary>
        public const double VelocidadeDoArco = 6;

        /// <summary>Gravidade do arco, em m/s².</summary>
        public const double Gravidade = 9.81;

        /// <summary>Intervalo de tempo entre dois pontos do arco, em segundos.</summary>
        public const double PassoDoArco = 0.025;

        private readonly AreaRetangular _sala;
        private readonly ObstaculoDoTeleporte[] _obstaculos;

        /// <param name="salaPorDentro">Retângulo do chão até a face interna das paredes.</param>
        /// <param name="quantidadeDeObstaculos">Vagas para obstáculos (mesa, caixa…), definidos com <see cref="DefinirObstaculo"/>.</param>
        public RegrasDoTeleporte(AreaRetangular salaPorDentro, int quantidadeDeObstaculos)
        {
            if (salaPorDentro.MeiaLargura <= DistanciaMinimaDasParedes ||
                salaPorDentro.MeiaProfundidade <= DistanciaMinimaDasParedes)
                throw new ArgumentException("A sala é pequena demais para a distância mínima das paredes.", nameof(salaPorDentro));
            _sala = salaPorDentro;
            _obstaculos = new ObstaculoDoTeleporte[quantidadeDeObstaculos];
        }

        /// <summary>Quantidade de vagas de obstáculo.</summary>
        public int QuantidadeDeObstaculos => _obstaculos.Length;

        /// <summary>Define (ou atualiza, por exemplo a caixa na mão do aluno) o obstáculo da vaga dada.</summary>
        public void DefinirObstaculo(int vaga, ObstaculoDoTeleporte obstaculo) => _obstaculos[vaga] = obstaculo;

        /// <summary>Se o destino (x, z) no chão vale; se não, por quê.</summary>
        public MotivoDoTeleporte Avaliar(double x, double z)
        {
            if (Math.Abs(x - _sala.CentroX) > _sala.MeiaLargura - DistanciaMinimaDasParedes ||
                Math.Abs(z - _sala.CentroZ) > _sala.MeiaProfundidade - DistanciaMinimaDasParedes)
                return MotivoDoTeleporte.PertoDaParede;

            foreach (var obstaculo in _obstaculos)
                if (obstaculo.Motivo != MotivoDoTeleporte.Nenhum && obstaculo.Area.Contem(x, z))
                    return obstaculo.Motivo;

            return MotivoDoTeleporte.Nenhum;
        }

        /// <summary>
        /// Traça o arco a partir do controle e escreve os pontos em <paramref name="pontos"/> (o primeiro é a
        /// origem; o último, o pouso). O arco para no chão, num obstáculo ou numa parede, ou quando o buffer acaba.
        /// </summary>
        public ResultadoDoTeleporte TracarArco(Vetor3 origem, Vetor3 direcao, Vetor3[] pontos)
        {
            if (pontos == null || pontos.Length < 2) throw new ArgumentException("O buffer precisa de 2 pontos ou mais.", nameof(pontos));
            double comprimento = direcao.Comprimento;
            if (comprimento < 1e-9) direcao = new Vetor3(0, 0, 1);
            else direcao = direcao * (1 / comprimento);

            Vetor3 velocidade = direcao * VelocidadeDoArco;
            pontos[0] = origem;
            Vetor3 anterior = origem;
            for (int i = 1; i < pontos.Length; i++)
            {
                double t = i * PassoDoArco;
                var atual = new Vetor3(origem.X + velocidade.X * t, origem.Y + velocidade.Y * t - Gravidade * t * t / 2,
                    origem.Z + velocidade.Z * t);

                if (atual.Y <= 0)
                {
                    // Corta o segmento no chão.
                    double fracao = anterior.Y / (anterior.Y - atual.Y);
                    var pouso = new Vetor3(anterior.X + (atual.X - anterior.X) * fracao, 0,
                        anterior.Z + (atual.Z - anterior.Z) * fracao);
                    pontos[i] = pouso;
                    return new ResultadoDoTeleporte(pouso, Avaliar(pouso.X, pouso.Z), i + 1);
                }

                pontos[i] = atual;
                foreach (var obstaculo in _obstaculos)
                    if (obstaculo.Motivo != MotivoDoTeleporte.Nenhum && obstaculo.Contem(atual))
                        return new ResultadoDoTeleporte(atual, obstaculo.Motivo, i + 1);

                if (!_sala.Contem(atual.X, atual.Z))
                    return new ResultadoDoTeleporte(atual, MotivoDoTeleporte.PertoDaParede, i + 1);

                anterior = atual;
            }

            return new ResultadoDoTeleporte(pontos[pontos.Length - 1], MotivoDoTeleporte.SemPouso, pontos.Length);
        }
    }
}
