namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Par de faces paralelas (e congruentes) do paralelepípedo; usado para colorir a planificação.
    /// </summary>
    public enum ParDeFaces
    {
        /// <summary>ABCD e EFGH, área a·b.</summary>
        Bases,

        /// <summary>ADHE e BCGF, área a·c.</summary>
        FrenteEFundo,

        /// <summary>ABFE e DCGH, área b·c.</summary>
        EsquerdaEDireita,
    }
}
