using System;
using System.IO;
using System.IO.Compression;
using UnityEditor.Android;
using UnityEngine;

namespace LaboratorioParalelepipedo.Editor.Release
{
    /// <summary>
    /// Tira as bibliotecas de 32 bits (e x86) de dentro dos .aar copiados para o projeto Gradle gerado pelo
    /// Unity, antes de o Gradle montar o APK. O OVRPlugin.aar leva cópias armeabi-v7a em assets/lib/, que o
    /// Gradle não filtra; a loja da Meta avisa. Só a cópia em Library/Bee muda; o pacote do SDK não.
    /// Mesma solução do Missão Coração (StripNon64BitLibraries).
    /// </summary>
    public sealed class RemoverBibliotecasDe32Bits : IPostGenerateGradleAndroidProject
    {
        // Depois dos processadores do Meta XR SDK.
        public int callbackOrder => 10000;

        public void OnPostGenerateGradleAndroidProject(string caminho)
        {
            // caminho aponta para o módulo unityLibrary; a raiz do projeto Gradle é a pasta de cima.
            string raiz = Directory.GetParent(caminho)?.FullName ?? caminho;
            int removidos = 0;
            foreach (string aar in Directory.GetFiles(raiz, "*.aar", SearchOption.AllDirectories))
            {
                if (aar.Contains($"{Path.DirectorySeparatorChar}build{Path.DirectorySeparatorChar}")) continue;
                removidos += LimparAar(aar);
            }

            foreach (string pasta in RegrasDePrivacidade.PastasTiradasDosAar)
            {
                string solta = Path.Combine(caminho, "src", "main", pasta.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(solta)) continue;
                Directory.Delete(solta, true);
                removidos++;
            }

            if (removidos > 0)
                Debug.Log($"[Release] {removidos} arquivo(s) de 32 bits/x86 retirados do projeto Gradle.");
        }

        /// <summary>
        /// Remove do .aar as entradas proibidas e devolve quantas. Monta um .aar novo só com as outras entradas
        /// (ZipArchiveMode.Update no Mono grava tamanhos que o Gradle não aceita).
        /// </summary>
        public static int LimparAar(string caminhoDoAar)
        {
            if (Contar(caminhoDoAar) == 0) return 0;

            string temporario = caminhoDoAar + ".tmp";
            int removidos = 0;
            using (FileStream entrada = File.OpenRead(caminhoDoAar))
            using (var origem = new ZipArchive(entrada, ZipArchiveMode.Read))
            using (var saida = new FileStream(temporario, FileMode.Create, FileAccess.Write))
            using (var destino = new ZipArchive(saida, ZipArchiveMode.Create))
            {
                foreach (ZipArchiveEntry item in origem.Entries)
                {
                    if (EhRetirado(item.FullName))
                    {
                        removidos++;
                        continue;
                    }

                    ZipArchiveEntry copia = destino.CreateEntry(item.FullName, System.IO.Compression.CompressionLevel.Optimal);
                    copia.LastWriteTime = item.LastWriteTime;
                    using (Stream de = item.Open())
                    using (Stream para = copia.Open())
                        de.CopyTo(para);
                }
            }

            File.Delete(caminhoDoAar);
            File.Move(temporario, caminhoDoAar);
            return removidos;
        }

        private static int Contar(string caminhoDoAar)
        {
            int total = 0;
            using (FileStream entrada = File.OpenRead(caminhoDoAar))
            using (var arquivo = new ZipArchive(entrada, ZipArchiveMode.Read))
                foreach (ZipArchiveEntry item in arquivo.Entries)
                    total += EhRetirado(item.FullName) ? 1 : 0;
            return total;
        }

        private static bool EhRetirado(string nome)
        {
            foreach (string pasta in RegrasDePrivacidade.PastasTiradasDosAar)
                if (nome.StartsWith(pasta, StringComparison.Ordinal))
                    return true;
            return false;
        }
    }
}
