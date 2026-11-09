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
        public const string AppVersao = "app.versao";
        public const string DiagnosticoTitulo = "diagnostico.titulo";
        public const string AberturaComecar = "abertura.comecar";

        public const string GeralAcerto = "geral.acerto";
        public const string GeralErro = "geral.erro";
        public const string GeralProximo = "geral.proximo";
        public const string GeralTentarDeNovo = "geral.tentarDeNovo";
        public const string GeralRecomecar = "geral.recomecar";

        public const string ControlesTitulo = "controles.titulo";
        public const string ControlesApontar = "controles.apontar";
        public const string ControlesPegar = "controles.pegar";
        public const string ControlesTeleporte = "controles.teleporte";
        public const string ControlesGiro = "controles.giro";

        public const string DicaPegar = "dica.pegar";
        public const string DicaEscolher = "dica.escolher";
        public const string DicaTeleporteValido = "dica.teleporteValido";
        public const string DicaTeleporteInvalido = "dica.teleporteInvalido";

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
        public const string DiagonaisObjetivo = "estacao.diagonais.objetivo";
        public const string DiagonaisDimensoes = "estacao.diagonais.dimensoes";
        public const string DiagonaisPasso1 = "estacao.diagonais.passo1";
        public const string DiagonaisPasso2 = "estacao.diagonais.passo2";
        public const string DiagonaisProximoVertice = "estacao.diagonais.proximoVertice";
        public const string DiagonaisVerticeErrado = "estacao.diagonais.verticeErrado";
        public const string DiagonaisContaDaBase = "estacao.diagonais.contaDaBase";
        public const string DiagonaisResultadoDaBase = "estacao.diagonais.resultadoDaBase";
        public const string DiagonaisContaDaDiagonal = "estacao.diagonais.contaDaDiagonal";
        public const string DiagonaisIncognita = "estacao.diagonais.incognita";
        public const string DiagonaisAcerto = "estacao.diagonais.acerto";
        public const string DiagonaisDicaSemRaiz = "estacao.diagonais.dica.semRaiz";
        public const string DiagonaisDicaSomou = "estacao.diagonais.dica.somou";
        public const string DiagonaisDicaDiagonalDaBase = "estacao.diagonais.dica.diagonalDaBase";
        public const string DiagonaisDicaGeral = "estacao.diagonais.dica.geral";
        public const string DiagonaisConcluida = "estacao.diagonais.concluida";
        public const string DiagonaisFormulaGeral = "estacao.diagonais.formulaGeral";
        public const string DiagonaisProximoDesafio = "estacao.diagonais.proximoDesafio";
        public const string DiagonaisRefazer = "estacao.diagonais.refazer";
        public const string DiagonaisRotulo = "estacao.diagonais.rotulo";

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
            AppTitulo, AppCreditos, AppVersao, DiagnosticoTitulo, AberturaComecar,
            GeralAcerto, GeralErro, GeralProximo, GeralTentarDeNovo, GeralRecomecar,
            ControlesTitulo, ControlesApontar, ControlesPegar, ControlesTeleporte, ControlesGiro,
            DicaPegar, DicaEscolher, DicaTeleporteValido, DicaTeleporteInvalido,
            ElementosTitulo, ElementosInstrucao, ElementosVertices, ElementosArestas, ElementosFaces,
            ElementosFaceMontada, ElementosNaoEhFace,
            DiagonaisTitulo, DiagonaisInstrucao, DiagonaisFormulaDaBase, DiagonaisFormulaDaDiagonal,
            DiagonaisTriangulo, DiagonaisObjetivo, DiagonaisDimensoes, DiagonaisPasso1, DiagonaisPasso2,
            DiagonaisProximoVertice, DiagonaisVerticeErrado, DiagonaisContaDaBase, DiagonaisResultadoDaBase,
            DiagonaisContaDaDiagonal, DiagonaisIncognita, DiagonaisAcerto, DiagonaisDicaSemRaiz, DiagonaisDicaSomou,
            DiagonaisDicaDiagonalDaBase, DiagonaisDicaGeral, DiagonaisConcluida, DiagonaisFormulaGeral,
            DiagonaisProximoDesafio, DiagonaisRefazer, DiagonaisRotulo,
            AreasTitulo, AreasInstrucao, AreasAbrir, AreasFechar, AreasPares,
            AreasFormulaDaBase, AreasFormulaLateral, AreasFormulaTotal,
            VolumeTitulo, VolumeInstrucao, VolumeEncher, VolumeEsvaziar, VolumeCamadas, VolumePorCamada,
            VolumeTotal, VolumeFormula, VolumeRessalva,
        };
    }
}
