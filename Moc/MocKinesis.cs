using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Extensions.Configuration;
using OFD_MessageService.Dto;
using RepoDb;


namespace OFD_MessageService.Moc
{
    public static class MocKinesis
    {
        public static List<Fila> GetDados(SqlConnection conexao)
        {
            List<Fila> resultado = new List<Fila>();

            using (var connection = conexao)
            {

                var result = connection.ExecuteQuery<Fila>("SELECT TOP(1000) [conteudo_linha] FROM [OFDUOBLPDB007].[dbo].[t_producao_n2_tag] where 1 = 1 " +
                                "where 1 = 1 and conteudo_data > '20200827 12:00:00' and conteudo_data < '20200827 12:10:00'",
                    commandType: CommandType.Text);

                resultado = (List<Fila>)result;


            }

            return resultado;
        }

        public static List<String> GetDados2(SqlConnection conexao)
        {
            List<String> resultado = new List<String>();

            using (SqlConnection connection = conexao)
            {
                try
                {
                    connection.Open();


                    using (SqlCommand command = connection.CreateCommand())
                    {

                        command.CommandText = "SELECT TOP(1000) [conteudo_linha] FROM [OFDUOBLPDB007].[dbo].[t_producao_n2_tag] where 1 = 1 " +
                                "and conteudo_data > '20200827 12:00:00' and conteudo_data < '20200827 12:10:00'";
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                    resultado.Add(reader.GetString(0) + System.Environment.NewLine);
                            }
                        }

                        reader.Close();

                    }


                    connection.Close();

                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    connection.Close();
                }
                finally
                {

                }
            }


            return resultado;
        }

    }
}
