using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace OFD_MessageService
{
    public class Conexao
    {
        public static SqlConnection GetConnectionProd(IConfigurationSection workConfig)
        {
            return new SqlConnection(
            //$@"server = {IP};user id=sa;password={Pwd()};persistsecurityinfo=True;database={DB}"
             @"data source=" + workConfig["DatabaseIp"] + ";initial catalog="
                + workConfig["DatabaseProdName"] + ";user id=" + workConfig["UserProdDb"]
                + ";password=" + workConfig["PassProdDb"] + ";Connect Timeout=60"
            );

        }

        public static SqlConnection GetConnectionHist(IConfigurationSection workConfig)
        {
            return new SqlConnection(
            //$@"server = {IP};user id=sa;password={Pwd()};persistsecurityinfo=True;database={DB}"
             @"data source=" + workConfig["DatabaseIp"] + ";initial catalog="
                + workConfig["DatabaseHistName"] + ";user id=" + workConfig["UserHistDb"]
                + ";password=" + workConfig["PassHistDb"] + ";Connect Timeout=60"
            );

        }
    }
}
