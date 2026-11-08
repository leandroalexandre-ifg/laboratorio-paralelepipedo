namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>
    /// Escurecimento curto do giro por passos: a tela escurece em <see cref="Duracao"/>/2, o giro acontece no
    /// escuro e a tela clareia na outra metade. Só no instante do giro; sem vinheta no teleporte nem parado.
    /// </summary>
    public sealed class EscurecimentoDoGiro
    {
        /// <summary>Duração total, em segundos (~100 ms).</summary>
        public const double Duracao = 0.1;

        private double _tempo = -1;
        private bool _girou;

        /// <summary>Se está escurecendo ou clareando agora.</summary>
        public bool Ativo => _tempo >= 0;

        /// <summary>Começa um escurecimento. Ignorado se já houver um em curso (o giro pedido seria repetido).</summary>
        public bool Iniciar()
        {
            if (Ativo) return false;
            _tempo = 0;
            _girou = false;
            return true;
        }

        /// <summary>
        /// Avança <paramref name="segundos"/> e devolve a opacidade (0 a 1). <paramref name="girarAgora"/> fica
        /// verdadeiro uma única vez, no quadro em que se chega ao meio (tela escura).
        /// </summary>
        public double Avancar(double segundos, out bool girarAgora)
        {
            girarAgora = false;
            if (!Ativo) return 0;

            _tempo += segundos < 0 ? 0 : segundos;
            double meio = Duracao / 2;
            if (!_girou && _tempo >= meio)
            {
                _girou = true;
                girarAgora = true;
            }

            if (_tempo >= Duracao)
            {
                _tempo = -1;
                return 0;
            }

            return _tempo <= meio ? _tempo / meio : (Duracao - _tempo) / meio;
        }
    }
}
