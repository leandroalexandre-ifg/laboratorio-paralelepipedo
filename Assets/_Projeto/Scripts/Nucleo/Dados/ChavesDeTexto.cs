namespace LaboratorioParalelepipedo.Nucleo.Dados
{
    /// <summary>
    /// Chaves dos textos do app em textos.json. Os testes conferem que o arquivo tem todas estas chaves e
    /// nenhuma a mais. Textos com {0}, {1} recebem valores na hora de mostrar (ex.: "Vértices: {0} de {1}").
    /// </summary>
    public static class ChavesDeTexto
    {
        public const string AppTitulo = "app.titulo";
        public const string AppCreditos = "app.creditos";

        public const string GeralAcerto = "geral.acerto";
        public const string GeralErro = "geral.erro";
        public const string GeralProximo = "geral.proximo";
        public const string GeralTentarDeNovo = "geral.tentarDeNovo";
        public const string GeralRecomecar = "geral.recomecar";

        public const string ControlesApontar = "controles.apontar";
        public const string ControlesPegar = "controles.pegar";
        public const string ControlesTeleporte = "controles.teleporte";
        public const string ControlesGiro = "controles.giro";

        public const string ElementosTitulo = "estacao.elementos.titulo";
        public const string ElementosInstrucao = "estacao.elementos.instrucao";
        public const string ElementosVertices = "estacao.elementos.vertices";
        public const string ElementosArestas = "estacao.elementos.arestas";
        public const string ElementosFaces = "estacao.elementos.faces";
        public const string ElementosFaceMontada = "estacao.elementos.faceMontada";
        public const string ElementosNaoEhFace = "estacao.elementos.naoEhFace";

        public const string DiagonaisTitulo = "estacao.diagonais.titulo";
        public const string DiagonaisInstrucao = "estacao.diagonais.instrucao";
        public const string DiagonaisFormulaDaBase = "estacao.diagonais.formulaDaBase";
        public const string DiagonaisFormulaDaDiagonal = "estacao.diagonais.formulaDaDiagonal";
        public const string DiagonaisTriangulo = "estacao.diagonais.triangulo";

        public const string AreasTitulo = "estacao.areas.titulo";
        public const string AreasInstrucao = "estacao.areas.instrucao";
        public const string AreasAbrir = "estacao.areas.abrir";
        public const string AreasFechar = "estacao.areas.fechar";
        public const string AreasPares = "estacao.areas.pares";
        public const string AreasFormulaDaBase = "estacao.areas.formulaDaBase";
        public const string AreasFormulaLateral = "estacao.areas.formulaLateral";
        public const string AreasFormulaTotal = "estacao.areas.formulaTotal";

        public const string VolumeTitulo = "estacao.volume.titulo";
        public const string VolumeInstrucao = "estacao.volume.instrucao";
        public const string VolumeEncher = "estacao.volume.encher";
        public const string VolumeEsvaziar = "estacao.volume.esvaziar";
        public const string VolumeCamadas = "estacao.volume.camadas";
        public const string VolumePorCamada = "estacao.volume.porCamada";
        public const string VolumeTotal = "estacao.volume.total";
        public const string VolumeFormula = "estacao.volume.formula";
        public const string VolumeRessalva = "estacao.volume.ressalva";

        /// <summary>Todas as chaves que o textos.json deve ter.</summary>
        public static readonly string[] Todas =
        {
            AppTitulo, AppCreditos,
            GeralAcerto, GeralErro, GeralProximo, GeralTentarDeNovo, GeralRecomecar,
            ControlesApontar, ControlesPegar, ControlesTeleporte, ControlesGiro,
            ElementosTitulo, ElementosInstrucao, ElementosVertices, ElementosArestas, ElementosFaces,
            ElementosFaceMontada, ElementosNaoEhFace,
            DiagonaisTitulo, DiagonaisInstrucao, DiagonaisFormulaDaBase, DiagonaisFormulaDaDiagonal,
            DiagonaisTriangulo,
            AreasTitulo, AreasInstrucao, AreasAbrir, AreasFechar, AreasPares,
            AreasFormulaDaBase, AreasFormulaLateral, AreasFormulaTotal,
            VolumeTitulo, VolumeInstrucao, VolumeEncher, VolumeEsvaziar, VolumeCamadas, VolumePorCamada,
            VolumeTotal, VolumeFormula, VolumeRessalva,
        };
    }
}
