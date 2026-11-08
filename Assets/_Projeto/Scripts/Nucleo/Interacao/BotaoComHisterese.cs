namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>
    /// Gatilho ou botão lateral lido pelo valor (0 a 1): aperta acima de <see cref="LimiarDeAperto"/> e só solta
    /// abaixo de <see cref="LimiarDeSoltura"/>, para um dedo indeciso não apertar e soltar várias vezes.
    /// Lido pelo estado, e não pelo "GetDown" do SDK, que pode perder ou repetir o aperto conforme a ordem dos scripts.
    /// </summary>
    public sealed class BotaoComHisterese
    {
        public const double LimiarDeAperto = 0.55;
        public const double LimiarDeSoltura = 0.35;

        /// <summary>Se o botão está apertado agora.</summary>
        public bool Apertado { get; private set; }

        /// <summary>Avança um quadro com o valor do botão.</summary>
        public TransicaoDoBotao Atualizar(double valor)
        {
            if (double.IsNaN(valor)) valor = 0;
            if (!Apertado && valor >= LimiarDeAperto)
            {
                Apertado = true;
                return TransicaoDoBotao.Apertou;
            }

            if (Apertado && valor <= LimiarDeSoltura)
            {
                Apertado = false;
                return TransicaoDoBotao.Soltou;
            }

            return TransicaoDoBotao.Nenhuma;
        }

        /// <summary>Volta a solto sem avisar (controle desconectado).</summary>
        public void Reiniciar() => Apertado = false;

        /// <summary>Atualiza com um botão digital (apertado ou não).</summary>
        public TransicaoDoBotao Atualizar(bool apertado) => Atualizar(apertado ? 1.0 : 0.0);
    }
}
