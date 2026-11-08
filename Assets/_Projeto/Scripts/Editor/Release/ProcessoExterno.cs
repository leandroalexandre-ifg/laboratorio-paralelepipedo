using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LaboratorioParalelepipedo.Editor.Release
{
    /// <summary>
    /// Roda uma ferramenta externa (aapt2, apksigner, ovr-platform-util, security, git) e devolve a saída.
    /// Os segredos passados são trocados por "***" na saída e na linha de comando mostrada no log.
    /// </summary>
    public static class ProcessoExterno
    {
        public readonly struct Resultado
        {
            public Resultado(int codigoDeSaida, string saida)
            {
                CodigoDeSaida = codigoDeSaida;
                Saida = saida;
            }

            public int CodigoDeSaida { get; }
            public string Saida { get; }
        }

        public static Resultado Rodar(string arquivo, IReadOnlyList<string> argumentos,
            IDictionary<string, string> ambiente, IReadOnlyList<string> segredos)
        {
            var info = new ProcessStartInfo(arquivo, Juntar(argumentos))
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            if (ambiente != null)
                foreach (var par in ambiente)
                    info.EnvironmentVariables[par.Key] = par.Value;

            var saida = new StringBuilder();
            using (var processo = new Process { StartInfo = info })
            {
                processo.OutputDataReceived += (_, e) => { if (e.Data != null) lock (saida) saida.AppendLine(e.Data); };
                processo.ErrorDataReceived += (_, e) => { if (e.Data != null) lock (saida) saida.AppendLine(e.Data); };
                processo.Start();
                processo.BeginOutputReadLine();
                processo.BeginErrorReadLine();
                processo.WaitForExit();
                return new Resultado(processo.ExitCode, Esconder(saida.ToString(), segredos));
            }
        }

        /// <summary>Linha de comando para o log, com os segredos escondidos.</summary>
        public static string Descrever(string arquivo, IReadOnlyList<string> argumentos, IReadOnlyList<string> segredos) =>
            Esconder($"{arquivo} {Juntar(argumentos)}", segredos);

        public static string Esconder(string texto, IReadOnlyList<string> segredos)
        {
            if (segredos == null) return texto;
            foreach (string segredo in segredos)
                if (!string.IsNullOrEmpty(segredo))
                    texto = texto.Replace(segredo, "***");
            return texto;
        }

        // Aspas em volta de cada argumento com espaço ou aspas (caminhos como "Build Profiles").
        private static string Juntar(IReadOnlyList<string> argumentos)
        {
            var linha = new StringBuilder();
            foreach (string argumento in argumentos)
            {
                if (linha.Length > 0) linha.Append(' ');
                if (argumento.Length > 0 && argumento.IndexOfAny(new[] { ' ', '"', '\t' }) < 0) linha.Append(argumento);
                else linha.Append('"').Append(argumento.Replace("\"", "\\\"")).Append('"');
            }

            return linha.ToString();
        }
    }
}
