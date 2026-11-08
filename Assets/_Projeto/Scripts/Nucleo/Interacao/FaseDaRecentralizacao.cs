namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>Fase da recentralização: o aluno só é reposto com a tela escura.</summary>
    public enum FaseDaRecentralizacao
    {
        /// <summary>Nada acontecendo: a cena fica à vista.</summary>
        Parada,
        /// <summary>Escurecendo; nada se move ainda.</summary>
        Escurecendo,
        /// <summary>Tela escura, esperando a pose da cabeça ficar estável.</summary>
        Escura,
        /// <summary>Clareando, já no lugar novo.</summary>
        Clareando,
    }
}
