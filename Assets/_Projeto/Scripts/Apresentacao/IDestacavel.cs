namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>Objeto que acende quando o raio de um controle aponta para ele.</summary>
    public interface IDestacavel
    {
        /// <summary>Um raio começou (verdadeiro) ou deixou (falso) de apontar. Pode haver dois raios ao mesmo tempo.</summary>
        void Destacar(bool apontado);
    }
}
