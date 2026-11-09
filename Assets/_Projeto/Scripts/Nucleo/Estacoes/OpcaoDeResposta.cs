namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>Um botão da pergunta: o texto (como no desafios.json ou calculado), o valor e a dica se estiver errada.</summary>
    public sealed class OpcaoDeResposta
    {
        public string Texto { get; }
        public double Valor { get; }
        public bool Correta { get; }

        /// <summary>Chave da dica no textos.json (nula na opção correta). O texto recebe <see cref="Texto"/> em {0}.</summary>
        public string ChaveDaDica { get; }

        public OpcaoDeResposta(string texto, double valor, bool correta, string chaveDaDica)
        {
            Texto = texto;
            Valor = valor;
            Correta = correta;
            ChaveDaDica = chaveDaDica;
        }

        public override string ToString() => Texto;
    }
}
