namespace LaboratorioParalelepipedo.Editor.Release
{
    /// <summary>
    /// Regras de privacidade do app (alunos do 9º ano, 100% offline), usadas nos testes EditMode (manifesto e
    /// pacotes do projeto), no filtro de assemblies do build e na verificação do APK antes do envio.
    /// Base: ReleasePrivacyRules do Missão Coração. Se uma regra mudar, registre o motivo em Docs/DEPLOY.md.
    /// </summary>
    public static class RegrasDePrivacidade
    {
        /// <summary>
        /// Permissões que o APK não pode ter. Várias são acrescentadas sozinhas pelo Unity ou pelo Meta XR SDK,
        /// por isso o Assets/Plugins/Android/AndroidManifest.xml as remove com tools:node="remove".
        /// HAND_TRACKING também: o app usa só os controles.
        /// </summary>
        public static readonly string[] PermissoesProibidas =
        {
            "android.permission.INTERNET",
            "android.permission.ACCESS_NETWORK_STATE",
            "android.permission.ACCESS_WIFI_STATE",
            "android.permission.CHANGE_WIFI_MULTICAST_STATE",
            "android.permission.RECORD_AUDIO",
            "android.permission.MODIFY_AUDIO_SETTINGS",
            "android.permission.BLUETOOTH",
            "android.permission.BLUETOOTH_CONNECT",
            "android.permission.CAMERA",
            "com.oculus.permission.HAND_TRACKING",
            "com.oculus.permission.USE_SCENE",
            "com.oculus.permission.USE_ANCHOR_API",
            "horizonos.permission.HEADSET_CAMERA",
            "com.oculus.permission.EYE_TRACKING",
            "com.oculus.permission.FACE_TRACKING",
            "com.oculus.permission.BODY_TRACKING",
        };

        /// <summary>
        /// Únicas permissões aceitas. Qualquer outra faz a verificação falhar, para nenhuma entrar sem revisão.
        /// DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION: interna do Android (AndroidX), só do próprio app.
        /// </summary>
        public static readonly string[] PermissoesAceitas =
        {
            "{package}.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION",
        };

        /// <summary>Meta-data que o manifesto não pode ter (identificador de telemetria do Meta XR SDK).</summary>
        public static readonly string[] MetaDataProibida =
        {
            "com.oculus.telemetry.project_guid",
        };

        /// <summary>Pacotes que não podem entrar no projeto (rede, microfone, conta, ambiente ou telemetria).</summary>
        public static readonly string[] PacotesProibidos =
        {
            "com.meta.xr.sdk.all",
            "com.meta.xr.sdk.voice",
            "com.meta.xr.sdk.platform",
            "com.meta.xr.mrutilitykit",
            "com.meta.xr.sdk.interaction",
            "com.meta.xr.sdk.interaction.ovr",
            "com.meta.xr.sdk.avatars",
            "com.meta.xr.sdk.audio",
            "com.unity.modules.unityanalytics",
        };

        /// <summary>
        /// Assemblies do Meta XR Core que não entram no build (<see cref="FiltroDeAssemblies"/>): o AgentBridge
        /// (cliente HTTP de um servidor de agentes de IA, com telemetria), o DevAgent do Immersive Debugger que
        /// depende dele, os blocos de IA (provedores na nuvem) e os blocos multijogador. O app não usa nenhum.
        /// </summary>
        public static readonly string[] AssembliesForaDoBuild =
        {
            "meta.xr.ai.agentbridge",
            "meta.xr.ai.agentbridge.telemetry",
            "Meta.XR.ImmersiveDebugger.DevAgent",
            "Meta.XR.BuildingBlocks.AIBlocks",
            "Meta.XR.MultiplayerBlocks.Shared",
        };

        /// <summary>
        /// Nomes que não podem aparecer nos metadados do IL2CPP (global-metadata.dat): se aparecerem, código de
        /// um dos assemblies fora do build chegou ao APK.
        /// </summary>
        public static readonly string[] NomesProibidosNoCodigo =
        {
            "Meta.XR.AI.AgentBridge",
            "Meta.XR.ImmersiveDebugger.DevAgent",
            "Meta.XR.BuildingBlocks.AIBlocks",
            "Meta.XR.MultiplayerBlocks",
        };

        /// <summary>Pastas de bibliotecas nativas de 32 bits ou x86: a loja do Quest exige só arm64-v8a.</summary>
        public static readonly string[] PastasDeBibliotecasProibidas =
        {
            "lib/armeabi-v7a/",
            "lib/x86/",
            "lib/x86_64/",
            "assets/lib/armeabi-v7a/",
            "assets/lib/x86/",
            "assets/lib/x86_64/",
        };

        /// <summary>Pastas tiradas de dentro dos .aar no projeto Gradle gerado (o pacote do SDK não muda).</summary>
        public static readonly string[] PastasTiradasDosAar =
        {
            "assets/lib/armeabi-v7a/",
            "assets/lib/x86/",
            "assets/lib/x86_64/",
            "jni/armeabi-v7a/",
            "jni/x86/",
            "jni/x86_64/",
        };

        /// <summary>
        /// Classes Java proibidas (prefixo do descritor no .dex): Unity Insights / Engine Diagnostics (lê o ID de
        /// publicidade), ID de publicidade e Firebase.
        /// </summary>
        public static readonly string[] PrefixosDeClassesProibidas =
        {
            "Lcom/unity3d/insights/",
            "Lcom/google/android/gms/ads/identifier/",
            "Lcom/google/firebase/",
        };

        /// <summary>Bibliotecas nativas de telemetria que não podem ir no APK (vêm do Interaction SDK).</summary>
        public static readonly string[] BibliotecasNativasProibidas =
        {
            "libISDKEngineTelemetry.so",
        };
    }
}
