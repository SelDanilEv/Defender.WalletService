namespace Defender.ServiceTemplate.Application.Helpers;

public static class SimpleLogger
{
    public enum LogLevel { Debug, Info, Warning }

    public static void Log(Exception ex, string? message = null)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            Serilog.Log.Error(message);
        }

        Serilog.Log.Error("ERROR --- " + DateTime.Now.ToString() + " : " + ex.Message);
        Serilog.Log.Error("ERROR --- " + DateTime.Now.ToString() + " : " + ex.StackTrace);

        if (ex.InnerException != null)
        {
            Serilog.Log.Error("ERROR INNER --- " + DateTime.Now.ToString() + " : " + ex.InnerException.Message);
            Serilog.Log.Error("ERROR INNER --- " + DateTime.Now.ToString() + " : " + ex.InnerException.StackTrace);
        }
    }

    public static void Log(string msg, LogLevel logLevel = LogLevel.Info)
    {
        switch (logLevel)
        {
            case LogLevel.Debug:
                Serilog.Log.Debug("DEBUG --- " + DateTime.Now.ToString() + " : " + msg);
                break;
            case LogLevel.Info:
                Serilog.Log.Information("INFO --- " + DateTime.Now.ToString() + " : " + msg);
                break;
            case LogLevel.Warning:
                Serilog.Log.Warning("WARN --- " + DateTime.Now.ToString() + " : " + msg);
                break;
        }
    }
}
