namespace LaboratorioParalelepipedo.XR
{
    /// <summary>
    /// Vibrações leves do controle (mesmos valores do Missão Coração): uma ao acender um alvo e outra ao
    /// confirmar com o gatilho. Curtas: o Quest mantém a vibração ligada por até 2 s se ninguém a desligar.
    /// </summary>
    public static class Vibracao
    {
        /// <summary>O raio acendeu um alvo.</summary>
        public static readonly PulsoDeVibracao AoAcender = new PulsoDeVibracao(0.3f, 0.2f, 0.04f);

        /// <summary>O aluno confirmou com o gatilho.</summary>
        public static readonly PulsoDeVibracao AoConfirmar = new PulsoDeVibracao(0.6f, 0.5f, 0.08f);

        /// <summary>Duração máxima de um toque (um aviso, não um zumbido).</summary>
        public const float DuracaoMaxima = 0.1f;
    }
}
