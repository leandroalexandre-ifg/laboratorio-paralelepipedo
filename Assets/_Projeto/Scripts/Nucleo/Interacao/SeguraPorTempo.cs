namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>
    /// Dispara uma vez quando algo fica apertado por <see cref="Segundos"/> seguidos e só volta a disparar depois de
    /// soltar. Serve para o atalho do professor (os dois analógicos afundados por 3 s abrem o diagnóstico), difícil de
    /// acionar sem querer. Sem alocação.
    /// </summary>
    public sealed class SeguraPorTempo
    {
        private double _apertado;
        private bool _disparou;

        public double Segundos { get; }

        public SeguraPorTempo(double segundos)
        {
            Segundos = segundos;
        }

        /// <summary>Um quadro: se está apertado e quanto tempo passou. Verdadeiro só no quadro em que completa o tempo.</summary>
        public bool Atualizar(bool apertado, double segundos)
        {
            if (!apertado)
            {
                _apertado = 0;
                _disparou = false;
                return false;
            }

            if (_disparou) return false;
            _apertado += segundos < 0 ? 0 : segundos;
            if (_apertado < Segundos) return false;
            _disparou = true;
            return true;
        }
    }
}
