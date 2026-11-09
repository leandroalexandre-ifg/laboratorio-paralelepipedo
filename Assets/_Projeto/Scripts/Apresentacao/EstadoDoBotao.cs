namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>Aparência de um botão de resposta.</summary>
    public enum EstadoDoBotao
    {
        Normal,
        /// <summary>Resposta certa: verde-água.</summary>
        Certo,
        /// <summary>Resposta errada já tentada: laranja (ainda pode ser apertado).</summary>
        Errado,
    }
}
