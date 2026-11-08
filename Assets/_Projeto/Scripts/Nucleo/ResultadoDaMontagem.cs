namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>Resultado de avaliar os vértices tocados num desafio de montar face.</summary>
    public readonly struct ResultadoDaMontagem
    {
        /// <summary>Verdadeiro se os toques formaram a face pedida (ou qualquer face, quando vale qualquer).</summary>
        public bool Correta { get; }

        /// <summary>Face que os toques formaram, ou null se não formaram nenhuma.</summary>
        public Face FaceMontada { get; }

        public bool FormouAlgumaFace => FaceMontada != null;

        public ResultadoDaMontagem(bool correta, Face faceMontada)
        {
            Correta = correta;
            FaceMontada = faceMontada;
        }
    }
}
