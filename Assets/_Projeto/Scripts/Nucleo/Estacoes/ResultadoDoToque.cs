namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>O que aconteceu ao tocar um vértice com o gatilho.</summary>
    public enum ResultadoDoToque
    {
        /// <summary>Nada (fora dos passos, ou tocou de novo o vértice já escolhido).</summary>
        Ignorado,
        /// <summary>Escolheu a primeira ponta do segmento.</summary>
        Escolheu,
        /// <summary>Ligou a segunda ponta: o passo terminou.</summary>
        Ligou,
        /// <summary>Vértice que não é ponta do segmento: pisca e mostra uma dica, sem penalidade.</summary>
        Errado,
    }
}
