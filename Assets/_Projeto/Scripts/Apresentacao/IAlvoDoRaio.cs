namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Objeto da cena que reage ao gatilho do raio (botão, vértice, face…). Precisa de um Collider no mesmo
    /// objeto ou num filho. O raio do controle (Fase 5) e o clique do modo Editor chamam <see cref="AoAcionar"/>.
    /// </summary>
    public interface IAlvoDoRaio
    {
        void AoAcionar();
    }
}
