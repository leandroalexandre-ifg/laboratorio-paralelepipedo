using System;

namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>
    /// Decide quando a pose da cabeça está pronta para pôr o aluno de frente para a mesa. No Quest, a pose demora
    /// alguns quadros para chegar e o espaço de rastreamento ainda pode girar logo depois do início. Por isso a cabeça
    /// precisa ficar abaixo de <see cref="GiroMaximo"/> °/s e <see cref="DeslocamentoMaximo"/> m/s por
    /// <see cref="SegundosParada"/> s seguidos. Usa o tempo entre os quadros: não depende do fps. Sem alocação.
    /// </summary>
    public sealed class EsperaPelaPoseEstavel
    {
        public const double SegundosParada = 0.5;

        /// <summary>Giro máximo da cabeça ainda considerado parado, em graus por segundo.</summary>
        public const double GiroMaximo = 216;

        /// <summary>Deslocamento máximo da cabeça ainda considerado parado, em metros por segundo.</summary>
        public const double DeslocamentoMaximo = 2.16;

        private bool _temAnterior;
        private Vetor3 _posicaoAnterior;
        private double _direcaoAnterior;
        private double _parada = -1;

        /// <summary>Se está esperando (entre <see cref="Comecar"/> e a pose ficar estável).</summary>
        public bool Esperando { get; private set; }

        /// <summary>Começa a esperar, do zero.</summary>
        public void Comecar()
        {
            Esperando = true;
            _temAnterior = false;
            _parada = -1;
        }

        /// <summary>
        /// Um quadro: se a pose está pronta (cabeça rastreada), onde está a cabeça e para onde olha (graus), e quanto
        /// tempo passou. Devolve verdadeiro uma vez, quando a pose fica estável (e para de esperar).
        /// </summary>
        public bool Avancar(bool posePronta, Vetor3 cabeca, double direcaoDoOlhar, double segundos)
        {
            if (!Esperando) return false;
            if (!posePronta)
            {
                _temAnterior = false;
                _parada = -1;
                return false;
            }

            double passo = segundos < 0 ? 0 : segundos;
            bool mexeu = _temAnterior &&
                         (Math.Abs(PosicaoInicialDoAluno.DiferencaDeAngulo(_direcaoAnterior, direcaoDoOlhar)) > GiroMaximo * passo ||
                          Vetor3.Distancia(_posicaoAnterior, cabeca) > DeslocamentoMaximo * passo);
            bool primeiro = !_temAnterior;
            _temAnterior = true;
            _posicaoAnterior = cabeca;
            _direcaoAnterior = direcaoDoOlhar;

            if (primeiro || mexeu)
            {
                _parada = 0;
                return false;
            }

            _parada += passo;
            if (_parada < SegundosParada) return false;
            Esperando = false;
            return true;
        }
    }
}
