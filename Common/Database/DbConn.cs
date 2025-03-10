﻿using Common.Utils;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace Common.Database
{
    public class DbConn : IDisposable
    {
        readonly SqlConnection _conn;

        public static async Task<DbConn?> Factory(ConsoleVariables cVars)
        {
            string address  = cVars.GetCVar("db.address", ConsoleVariableType.String, "127.0.0.1");
            string database = cVars.GetCVar("db.name", ConsoleVariableType.String, "DiscordSharded");
            string username = cVars.GetCVar("db.user", ConsoleVariableType.String, "censored");
            string password = cVars.GetCVar("db.password", ConsoleVariableType.String, "censored");
            int port = cVars.GetCVar("db.port", ConsoleVariableType.Int, 5521);

            string connectionString = $"Server={address},{port}; Database={database}; User Id={username}; Password={password}; Encrypt=True; TrustServerCertificate=True;";
            try
            {
                DbConn conn  = new DbConn(connectionString);
                await conn._conn.OpenAsync();
                return conn;
            }
            catch (Exception)
            {
                Console.WriteLine($"Failed to connect to database on address {address}");
                return null;
            }
            
        }
        internal DbConn(string connectionString) => _conn = new SqlConnection(connectionString);
        

        //find a better solution PLS PLS PLS this is unboxing land... Too late -.-
        public async Task<List<object[]>?> ExecuteQuery(string query)
        {
            using SqlTransaction transaction = _conn.BeginTransaction();
            try
            {
                using SqlCommand command = _conn.CreateCommand();
                command.CommandText = query;
                command.Transaction = transaction;

                using SqlDataReader reader = await command.ExecuteReaderAsync();

                List<object[]> values = new List<object[]>();
                object[] rowValues = new object[reader.FieldCount];

                while (await reader.ReadAsync())
                {
                    reader.GetValues(rowValues);
                    values.Add(rowValues);
                }

                await reader.CloseAsync();
                await transaction.CommitAsync();

                return values;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DbConn.ExecuteQuery failed {ex.Message}");
                await transaction.RollbackAsync();
                return null;
            }
        }
        public async Task<int> ExecuteNonResultProcedure(string procedure, Dictionary<string, object?> values)
        {
            using SqlCommand command = _conn.CreateCommand();

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedure;

            foreach (KeyValuePair<string, object?> kvp in values)
                command.Parameters.AddWithValue(kvp.Key, kvp.Value);

            return await command.ExecuteNonQueryAsync();
        }
        public async Task<Dictionary<string, object?>> ExecuteOutParamProcedure(string procedure, Dictionary<string, SqlDbType> outputParams, int defaultSize = 64)
        {
            using SqlCommand command = _conn.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedure;

            foreach(var kvp in outputParams)
            {
                SqlParameter param = new(kvp.Key, kvp.Value);
                param.Direction = ParameterDirection.Output;
                param.Size = defaultSize;
                command.Parameters.Add(param);
            }

            await command.ExecuteNonQueryAsync();

            Dictionary<string, object?> output = new(); 
            foreach(SqlParameter param in command.Parameters)
            {
                output[param.ParameterName] = param.Value;
            }

            return output;
        }
        public async Task<SqlDataReader> ExecuteResultProcedure(string procedure, Dictionary<string, object?> values)
        {
            using SqlCommand command = _conn.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedure;

            foreach (KeyValuePair<string, object?> kvp in values)
            {
                SqlParameter parameter = command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                parameter.Direction = ParameterDirection.Input;
                parameter.Size = 64;
            }
            try
            {
                return await command.ExecuteReaderAsync();
            } catch(Exception e)
            {
                Console.WriteLine("Sql error: " + e.Message);
                throw;
            }
        }
        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }
    }
}
