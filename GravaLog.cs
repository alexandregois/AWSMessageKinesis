using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace OFD_MessageService
{
    public static class GravaLog
    {
        public static IConfigurationSection workConfig {get;set;}

        public static void GravaLogInformacao(String Message)
        {
            if (workConfig["GravaLogInformacao"] == "1")
                Log.Information("INFO - " + Message);

        }

        public static void GravaLogErro(String Message)
        {
            if (workConfig["GravaLogErro"] == "1")
                Log.Error("ERRO - " + Message);

        }
    }
}
