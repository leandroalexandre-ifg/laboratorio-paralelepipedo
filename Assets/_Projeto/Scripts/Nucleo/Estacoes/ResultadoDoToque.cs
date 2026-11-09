namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>O que aconteceu ao tocar um vértice com o gatilho.</summary>
    public enum ResultadoDoToque
    {
        /// <summary>Nada (fora dos passos, ou tocou de novo um vértice já escolhido).</summary>
        Ignorado,
        /// <summary>Vértice aceito: a primeira ponta do segmento (Estação 2) ou mais um vértice da face (Estação 1).</summary>
        Escolheu,
        /// <summary>Completou: ligou a segunda ponta (Estação 2) ou fechou a face (Estação 1).</summary>
        Ligou,
        /// <summary>Vértice que não serve (não é ponta do segmento ou não continua a face): pisca e mostra uma dica, sem penalidade.</summary>
        Errado,
    }
}
