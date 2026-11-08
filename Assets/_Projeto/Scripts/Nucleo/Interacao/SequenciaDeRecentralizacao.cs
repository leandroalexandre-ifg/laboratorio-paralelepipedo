namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>
    /// Cada recentralização: escurece em <see cref="DuracaoDoEscurecimento"/> s, espera a pose da cabeça ficar estável
    /// (<see cref="EsperaPelaPoseEstavel"/>), repõe o aluno no escuro e clareia. Nada se move à vista. O escuro dura no
    /// máximo <see cref="EscuroMaximo"/> s para o aluno não ficar preso numa tela preta: se a pose não estabilizou,
    /// repõe com a atual; se não há pose, clareia sem mover e refaz tudo quando a pose voltar. Sem alocação.
    /// </summary>
    public sealed class SequenciaDeRecentralizacao
    {
        public const double DuracaoDoEscurecimento = 0.15;
        public const double EscuroMaximo = 3;

        private readonly EsperaPelaPoseEstavel _espera = new EsperaPelaPoseEstavel();
        private double _tempoNaFase;
        private double _opacidadeNoInicio;
        private bool _esperandoPose;

        public FaseDaRecentralizacao Fase { get; private set; } = FaseDaRecentralizacao.Parada;

        /// <summary>Opacidade da tela escura: 0 (cena à vista) a 1 (tudo escuro).</summary>
        public double Opacidade { get; private set; }

        /// <summary>Se há uma recentralização em curso (incluindo escurecer e clarear).</summary>
        public bool Ativa => Fase != FaseDaRecentralizacao.Parada;

        /// <summary>Começa já no escuro: o início do app, antes de o aluno estar de frente para a mesa.</summary>
        public void ComecarNoEscuro()
        {
            _esperandoPose = false;
            Opacidade = 1;
            EntrarNoEscuro();
        }

        /// <summary>
        /// Pede uma recentralização. Já no escuro, recomeça a espera pela pose; clareando, volta a escurecer a partir
        /// da opacidade atual.
        /// </summary>
        public void Pedir()
        {
            _esperandoPose = false;
            if (Fase == FaseDaRecentralizacao.Escurecendo) return;
            if (Fase == FaseDaRecentralizacao.Escura)
            {
                EntrarNoEscuro();
                return;
            }
            Entrar(FaseDaRecentralizacao.Escurecendo);
        }

        /// <summary>
        /// Um quadro. Devolve verdadeiro quando é para repor o aluno agora (sempre com a tela toda escura).
        /// </summary>
        public bool Avancar(bool posePronta, Vetor3 cabeca, double direcaoDoOlhar, double segundos)
        {
            double passo = segundos < 0 ? 0 : segundos;
            _tempoNaFase += passo;

            switch (Fase)
            {
                case FaseDaRecentralizacao.Escurecendo:
                    Opacidade = Limitar(_opacidadeNoInicio + _tempoNaFase / DuracaoDoEscurecimento);
                    if (Opacidade >= 1) EntrarNoEscuro();
                    return false;

                case FaseDaRecentralizacao.Escura:
                    bool estavel = _espera.Avancar(posePronta, cabeca, direcaoDoOlhar, passo);
                    if (estavel)
                    {
                        Entrar(FaseDaRecentralizacao.Clareando);
                        return true;
                    }
                    if (_tempoNaFase >= EscuroMaximo)
                    {
                        _esperandoPose = !posePronta;
                        Entrar(FaseDaRecentralizacao.Clareando);
                        return posePronta;
                    }
                    return false;

                case FaseDaRecentralizacao.Clareando:
                    Opacidade = Limitar(1 - _tempoNaFase / DuracaoDoEscurecimento);
                    if (Opacidade <= 0) Entrar(FaseDaRecentralizacao.Parada);
                    return false;

                default:
                    if (_esperandoPose && posePronta)
                    {
                        _esperandoPose = false;
                        Entrar(FaseDaRecentralizacao.Escurecendo);
                    }
                    return false;
            }
        }

        private void EntrarNoEscuro()
        {
            _espera.Comecar();
            Entrar(FaseDaRecentralizacao.Escura);
        }

        private void Entrar(FaseDaRecentralizacao fase)
        {
            Fase = fase;
            _tempoNaFase = 0;
            _opacidadeNoInicio = Opacidade;
        }

        private static double Limitar(double valor) => valor < 0 ? 0 : valor > 1 ? 1 : valor;
    }
}
