using OFD_MessageService.Dto;
using RepoDb;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace OFD_MessageService
{
    public static class Data
    {
        public static int GetIdBitola(SqlConnection conexao, string nome_bitola)
        {
            int id_t_bitola = 0;

            try
            {
                
                using (var connection = conexao)
                {
                    var result = connection.ExecuteQuery<Bitola>
                    (
                        "s_get_bitola_por_nome", new
                        {
                            nome_bitola_escala_passe = nome_bitola,
                            nome_bitola_kinesis = nome_bitola
                        }, commandType: CommandType.StoredProcedure
                    );

                    if (result != null && result.Count() > 0)
                        id_t_bitola = result.ToList()[0].id_t_bitola;

                }

                return id_t_bitola;
            }
            catch (Exception ex)
            {
                GravaLog.GravaLogErro("GetIdBitola - Data Linha: 42 - " + ex.Message);
            }

            return id_t_bitola;

        }

        public static int GetIdBitolaNumBloco(SqlConnection conexao, int pNum_Bloco)
        {
            int id_t_bitola = 0;

            try
            {

                using (var connection = conexao)
                {
                    var result = connection.ExecuteQuery<Bitola>
                    (
                        "s_get_bitola_por_num_bloco_laminacao_cambio", new
                        {
                            num_bloco = pNum_Bloco

                        }, commandType: CommandType.StoredProcedure
                    );

                    if (result != null && result.Count() > 0)
                        id_t_bitola = result.ToList()[0].id_t_bitola;

                }

                return id_t_bitola;
            }
            catch (Exception ex)
            {
                GravaLog.GravaLogErro("GetIdBitola - Data Linha: 42 - " + ex.Message);
            }

            return id_t_bitola;

        }

        public static List<Dashboard_Programacao> GetListaDashboard_Programacao(SqlConnection conexao, int p_id_t_bitola, int p_id_t_campanha,
            int p_id_t_producao_status, byte p_registro_ativo)
        {

            List<Dashboard_Programacao> listaDashboard_Programacao = null;

            try
            {

                using (var connection = conexao)
                {
                    var result = connection.ExecuteQuery<Dashboard_Programacao>
                    (
                        "s_get_t_dashboard_programacao_kinesis", new
                        {
                            id_t_bitola = p_id_t_bitola,
                            id_t_campanha = p_id_t_campanha,
                            id_t_status_laminacao = p_id_t_producao_status,
                            registro_ativo = p_registro_ativo

                        }, commandType: CommandType.StoredProcedure
                    );

                    listaDashboard_Programacao = result.ToList();
                }

            }
            catch (Exception ex)
            {

                GravaLog.GravaLogErro("GetListaDashboard_Programacao - Data Linha: 79 - " + ex.Message);
            }

            return listaDashboard_Programacao;

        }

        public static int GetCampanha_Bitola(SqlConnection conexao, int p_id_t_bitola, int p_Mes, int p_Ano)
        {

            int id_t_campanha = 0;

            try
            {
                using (var connection = conexao)
                {
                    var result = connection.ExecuteQuery
                    (
                        "s_get_t_campanha_from_bitola", new
                        {
                            id_t_bitola = p_id_t_bitola,
                            mes_numero = p_Mes,
                            ano = p_Ano

                        }, commandType: CommandType.StoredProcedure
                    );

                    id_t_campanha = result.ToList()[0].id_t_campanha;
                }

            }
            catch (Exception ex)
            {

                GravaLog.GravaLogErro("GetCampanha_Bitola - Data Linha: 113 - " + ex.Message);
            }

            return id_t_campanha;

        }

        public static void SetListaDashboard_Programacao(SqlConnection conexao, int p_id_t_bitola, int p_id_t_campanha,
            int p_id_t_producao_status_anterior, int p_id_t_producao_status, byte p_registro_ativo)
        {

            try
            {
                using (var connection = conexao)
                {
                    var result = connection.ExecuteQuery
                    (
                        "s_exe_update_lista_dashboard_programacao", new
                        {
                            id_t_bitola = p_id_t_bitola,
                            id_t_campanha = p_id_t_campanha,
                            id_t_producao_status_anterior = p_id_t_producao_status_anterior,
                            id_t_producao_status = p_id_t_producao_status,
                            registro_ativo = p_registro_ativo

                        }, commandType: CommandType.StoredProcedure
                    );

                }
            }
            catch (Exception ex)
            {

                GravaLog.GravaLogErro("SetListaDashboard_Programacao - Data Linha: 146 - " + ex.Message);
            }

        }

        public static void SetListaDashboard_ProgramacaoLaminacaoCambio(SqlConnection conexao, int p_id_t_bitola, int p_id_t_campanha,
            int p_id_t_producao_status, int p_id_t_laminacao_status, byte p_registro_ativo)
        {

            try
            {
                using (var connection = conexao)
                {
                    var result = connection.ExecuteQuery
                    (
                        "s_exe_update_laminacao_cambio", new
                        {
                            id_t_bitola = p_id_t_bitola,
                            id_t_campanha = p_id_t_campanha,
                            id_t_producao_status = p_id_t_producao_status,
                            id_t_laminacao_cambio_status = p_id_t_laminacao_status,
                            registro_ativo = p_registro_ativo

                        }, commandType: CommandType.StoredProcedure
                    );

                }
            }
            catch (Exception ex)
            {

                GravaLog.GravaLogErro("SetListaDashboard_ProgramacaoLaminacaoCambio - Data Linha: 177 - " + ex.Message);
            }

        }

        public static void SetListaDashboard_ProgramacaoLaminacaoCambioQuantidade(SqlConnection conexao, int p_id_t_bitola, double p_quantidade,
            int p_id_t_laminacao_cambio_status, byte p_registro_ativo)
        {
            try
            {
                using (var connection = conexao)
                {
                    var result = connection.ExecuteQuery
                    (
                        "s_exe_update_laminacao_cambio", new
                        {
                            id_t_bitola = p_id_t_bitola,
                            quantidade_laminada = p_quantidade,
                            id_t_laminacao_cambio_status = p_id_t_laminacao_cambio_status,
                            registro_ativo = p_registro_ativo

                        }, commandType: CommandType.StoredProcedure
                    );

                }

            }
            catch (Exception ex)
            {

                GravaLog.GravaLogErro("SetListaDashboard_ProgramacaoLaminacaoCambioQuantidade - Data Linha: 207 - " + ex.Message);
            }
           
        }

        public static void SetFila_Tabela(SqlConnection conexao, string p_conteudo_linha, DateTime p_data_registro,
            string p_nome_fila, string p_nome_tag, string p_tag_valor, string p_tag_prefixo, string p_message_id)
        {

            try
            {
                using (var connection = conexao)
                {

                    var result = connection.ExecuteQuery
                    (
                        "s_set_t_producao_n2_tag", new
                        {
                            conteudo_linha = p_conteudo_linha,
                            conteudo_data = p_data_registro,
                            conteudo_nome_fila = p_nome_fila,
                            conteudo_nome_tag = p_nome_tag,
                            conteudo_tag_valor = p_tag_valor,
                            conteudo_tag_prefixo = p_tag_prefixo,
                            conteudo_message_id = p_message_id

                        },
                        commandType: CommandType.StoredProcedure
                    );

                }
            }
            catch (Exception ex)
            {

                GravaLog.GravaLogErro("SetFila_Tabela - Data Linha: 242 - " + ex.Message);
            }

           
        }

        public static void SetFila_Tabela_Mudanca(SqlConnection conexao,
            string p_nome_fila, string p_nome_tag, string p_tag_valor, string p_tag_prefixo, string p_tag_valor_anterior)
        {

            try
            {
                using (var connection = conexao)
                {

                    var result = connection.ExecuteQuery
                    (
                        "s_set_t_producao_n2_tag_mudanca", new
                        {
                        //id_t_producao_n2_tag_mudanca , 
                        conteudo_nome_fila = p_nome_fila,
                            conteudo_tag_prefixo_anterior = p_tag_prefixo,
                            conteudo_tag_prefixo_atual = p_tag_prefixo,
                            conteudo_nome_tag_anterior = p_nome_tag,
                            conteudo_nome_tag_atual = p_nome_tag,
                            conteudo_tag_valor_anterior = p_tag_valor_anterior,
                            conteudo_tag_valor_atual = p_tag_valor,
                            id_t_usuario = 999
                        //id_t_cadastro_acao, 
                        //data_criacao datetime, 
                        //data_ultima_atualizacao 
                    },
                        commandType: CommandType.StoredProcedure
                    );

                }
            }
            catch (Exception ex)
            {

                GravaLog.GravaLogErro("SetFila_Tabela_Mudanca - Data Linha: 282 - " + ex.Message);
            }

        }

        public static void SetLaminacaoCambio_Canal(SqlConnection conexao, int p_id_t_bitola, int p_posicao_canal, int p_id_t_gaiola, int p_num_bloco)
        {

            try
            {
                using (var connection = conexao)
                {
                    var result = connection.ExecuteQuery
                    (
                        "s_exe_update_laminacao_cambio_canal", new
                        {
                            id_t_bitola = p_id_t_bitola,
                            posicao = p_posicao_canal,
                            id_t_gaiola = p_id_t_gaiola,
                            num_bloco = p_num_bloco

                        }, commandType: CommandType.StoredProcedure
                    );

                }

            }
            catch (Exception ex)
            {

                GravaLog.GravaLogErro("SetLaminacaoCambio_Canal - Data Linha: 310 - " + ex.Message);
            }
        }
    }

}
