using UnityEngine;

// Aplica ajustes livianos al iniciar el juego en celulares.
// Mantiene una configuracion estable sin depender de tocar cada escena.
public static class MobilePerformanceSettings
{
    private const string NombreCalidadMobile = "Low";
    private const int FpsMobile = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Aplicar()
    {
        if(!Application.isMobilePlatform)
        {
            return;
        }

        int indiceCalidad = ObtenerIndiceCalidad(NombreCalidadMobile);

        if(indiceCalidad >= 0 && QualitySettings.GetQualityLevel() != indiceCalidad)
        {
            QualitySettings.SetQualityLevel(indiceCalidad, true);
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = FpsMobile;
    }

    private static int ObtenerIndiceCalidad(string nombreCalidad)
    {
        string[] nombresCalidad = QualitySettings.names;

        for(int i = 0; i < nombresCalidad.Length; i++)
        {
            if(nombresCalidad[i] == nombreCalidad)
            {
                return i;
            }
        }

        return -1;
    }
}
