using System;
using System.Collections.Generic;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Dados;
using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Lê textos.json e desafios.json (TextAsset) com o JsonUtility e entrega ao núcleo para conferir.
    /// Qualquer problema vira <see cref="DadosInvalidosException"/> com mensagem em português.
    /// </summary>
    public static class CarregadorDeDados
    {
        public static CatalogoDeTextos CarregarTextos(TextAsset arquivo) =>
            CarregarTextos(Conteudo(arquivo, "textos.json"), NomeDe(arquivo, "textos.json"));

        public static IReadOnlyList<Desafio> CarregarDesafios(TextAsset arquivo) =>
            CarregarDesafios(Conteudo(arquivo, "desafios.json"), NomeDe(arquivo, "desafios.json"));

        /// <exception cref="DadosInvalidosException">Se o JSON for inválido ou algum texto estiver errado.</exception>
        public static CatalogoDeTextos CarregarTextos(string json, string nomeDoArquivo = "textos.json") =>
            LeitorDeTextos.Ler(Desserializar<ArquivoDeTextosDto>(json, nomeDoArquivo), nomeDoArquivo);

        /// <exception cref="DadosInvalidosException">Se o JSON for inválido ou algum desafio estiver errado.</exception>
        public static IReadOnlyList<Desafio> CarregarDesafios(string json, string nomeDoArquivo = "desafios.json") =>
            LeitorDeDesafios.Ler(Desserializar<ArquivoDeDesafiosDto>(json, nomeDoArquivo), nomeDoArquivo);

        private static T Desserializar<T>(string json, string nomeDoArquivo) where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new DadosInvalidosException(nomeDoArquivo, "o arquivo está vazio.");

            try
            {
                return JsonUtility.FromJson<T>(json) ??
                       throw new DadosInvalidosException(nomeDoArquivo, "o arquivo está vazio.");
            }
            catch (ArgumentException e)
            {
                throw new DadosInvalidosException(nomeDoArquivo,
                    $"o JSON está mal formado ({e.Message}). Confira vírgulas, aspas e chaves.");
            }
        }

        private static string Conteudo(TextAsset arquivo, string nomePadrao)
        {
            if (arquivo == null)
                throw new DadosInvalidosException(nomePadrao, "o arquivo não foi ligado ao componente.");
            return arquivo.text;
        }

        private static string NomeDe(TextAsset arquivo, string nomePadrao) =>
            arquivo != null ? arquivo.name + ".json" : nomePadrao;
    }
}
