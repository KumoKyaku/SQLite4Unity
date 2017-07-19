using UnityEngine;
using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Devart.Data.SQLite
{
    public partial class SQLiteHelper
    {
        /// <summary>
        /// 数据库连接定义
        /// </summary>
        private SQLiteConnection dbConnection;

        /// <summary>
        /// SQL命令定义
        /// </summary>
        private SQLiteCommand dbCommand;

        /// <summary>
        /// 数据读取定义
        /// </summary>
        private SQLiteDataReader dataReader;

        public SQLiteConnection DbConnection
        {
            get { return dbConnection; }
            private set { dbConnection = value; }
        }

        /// <summary>
        /// 构造函数    
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        public SQLiteHelper(string connectionString)
        {
            try
            {
                //构造数据库连接
                DbConnection = new SQLiteConnection(connectionString);
                //打开数据库
                DbConnection.Open();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
            }
        }

        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <returns>The query.</returns>
        /// <param name="queryString">SQL命令字符串</param>
        public SQLiteDataReader ExecuteQuery(string queryString)
        {
            dbCommand = DbConnection.CreateCommand();
            dbCommand.CommandText = queryString;
            dataReader = dbCommand.ExecuteReader();
            return dataReader;
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void CloseConnection()
        {
            //销毁Command
            if (dbCommand != null)
            {
                dbCommand.Cancel();
            }
            dbCommand = null;

            //销毁Reader
            if (dataReader != null)
            {
                dataReader.Close();
            }
            dataReader = null;

            //销毁Connection
            if (DbConnection != null)
            {
                DbConnection.Close();
            }
            DbConnection = null;
        }

        /// <summary>
        /// 读取整张数据表
        /// </summary>
        /// <returns>The full table.</returns>
        /// <param name="tableName">数据表名称</param>
        public SQLiteDataReader ReadFullTable(string tableName)
        {
            string queryString = "SELECT * FROM " + tableName;
            return ExecuteQuery(queryString);
        }

        /// <summary>
        /// 向指定数据表中插入数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="values">插入的数值</param>
        public SQLiteDataReader InsertValues(string tableName, string[] values)
        {
            //获取数据表中字段数目
            int fieldCount = ReadFullTable(tableName).FieldCount;
            //当插入的数据长度不等于字段数目时引发异常
            if (values.Length != fieldCount)
            {
                throw new ArgumentException("values.Length!=fieldCount");
            }

            string queryString = "INSERT INTO " + tableName + " VALUES (" + values[0];
            for (int i = 1; i < values.Length; i++)
            {
                queryString += ", " + values[i];
            }
            queryString += " )";
            return ExecuteQuery(queryString);
        }

        /// <summary>
        /// 更新指定数据表内的数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colValues">字段名对应的数据</param>
        /// <param name="key">关键字</param>
        /// <param name="value">关键字对应的值</param>
        public SQLiteDataReader UpdateValues(string tableName, string[] colNames, string[] colValues, string key, string operation, string value)
        {
            //当字段名称和字段数值不对应时引发异常
            if (colNames.Length != colValues.Length)
            {
                throw new ArgumentException("colNames.Length!=colValues.Length");
            }

            string queryString = "UPDATE " + tableName + " SET " + colNames[0] + "=" + colValues[0];
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += ", " + colNames[i] + "=" + colValues[i];
            }
            queryString += " WHERE " + key + operation + value;
            return ExecuteQuery(queryString);
        }

        /// <summary>
        /// 删除指定数据表内的数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colValues">字段名对应的数据</param>
        public SQLiteDataReader DeleteValuesOR(string tableName, string[] colNames, string[] operations, string[] colValues)
        {
            //当字段名称和字段数值不对应时引发异常
            if (colNames.Length != colValues.Length || operations.Length != colNames.Length || operations.Length != colValues.Length)
            {
                throw new ArgumentException("colNames.Length!=colValues.Length || operations.Length!=colNames.Length || operations.Length!=colValues.Length");
            }

            string queryString = "DELETE FROM " + tableName + " WHERE " + colNames[0] + operations[0] + colValues[0];
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += "OR " + colNames[i] + operations[0] + colValues[i];
            }
            return ExecuteQuery(queryString);
        }

        /// <summary>
        /// 删除指定数据表内的数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colValues">字段名对应的数据</param>
        public SQLiteDataReader DeleteValuesAND(string tableName, string[] colNames, string[] operations, string[] colValues)
        {
            //当字段名称和字段数值不对应时引发异常
            if (colNames.Length != colValues.Length || operations.Length != colNames.Length || operations.Length != colValues.Length)
            {
                throw new ArgumentException("colNames.Length!=colValues.Length || operations.Length!=colNames.Length || operations.Length!=colValues.Length");
            }

            string queryString = "DELETE FROM " + tableName + " WHERE " + colNames[0] + operations[0] + colValues[0];
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += " AND " + colNames[i] + operations[i] + colValues[i];
            }
            return ExecuteQuery(queryString);
        }

        /// <summary>
        /// 创建数据表
        /// </summary> +
        /// <returns>The table.</returns>
        /// <param name="tableName">数据表名</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colTypes">字段名类型</param>
        public SQLiteDataReader CreateTable(string tableName, string[] colNames, string[] colTypes)
        {
            string queryString = "CREATE TABLE " + tableName + "( " + colNames[0] + " " + colTypes[0];
            for (int i = 1; i < colNames.Length; i++)
            {
                queryString += ", " + colNames[i] + " " + colTypes[i];
            }
            queryString += "  ) ";
            return ExecuteQuery(queryString);
        }

        /// <summary>
        /// Reads the table.
        /// </summary>
        /// <returns>The table.</returns>
        /// <param name="tableName">Table name.</param>
        /// <param name="items">Items.</param>
        /// <param name="colNames">Col names.</param>
        /// <param name="operations">Operations.</param>
        /// <param name="colValues">Col values.</param>
        public SQLiteDataReader ReadTable(string tableName, string[] items, string[] colNames, string[] operations, string[] colValues)
        {
            string queryString = "SELECT " + items[0];
            for (int i = 1; i < items.Length; i++)
            {
                queryString += ", " + items[i];
            }
            queryString += " FROM " + tableName + " WHERE " + colNames[0] + " " + operations[0] + " " + colValues[0];
            for (int i = 0; i < colNames.Length; i++)
            {
                queryString += " AND " + colNames[i] + " " + operations[i] + " " + colValues[0] + " ";
            }
            return ExecuteQuery(queryString);
        }
    }

    public partial class SQLiteHelper
    {
        private const string NEW_LINE_IN_TABLE_DEF = ",\n";

        public static void CreateSQLiteFromDataSet(DataSet p_dataSetSource, string p_strSQLiteFullPathTarget)
        {
            try
            {
                using (var conn = new SQLiteConnection(@"data source=" + p_strSQLiteFullPathTarget))
                {
                    conn.Open();

                    //CREATE Tables with PK and FK
                    foreach (DataTable dtTable in p_dataSetSource.Tables)
                    {
                        var strSqlCreateTable = GetSQLiteCreateTableIfNotExists(dtTable);

                        using (var cmd = new SQLiteCommand(strSqlCreateTable, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // without transaction, each command will commit, and therefore, the save process will be very slow.
                    // that's why we use transaction.
                    var transaction = conn.BeginTransaction();

                    // Fill data
                    foreach (DataTable dtTableSource in p_dataSetSource.Tables)
                    {
                        FillSQLiteSingleTableFromDataTable(dtTableSource, conn, transaction);
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
        }

        private static void FillSQLiteSingleTableFromDataTable(DataTable p_dtTableSource, SQLiteConnection p_connectionToTarget, SQLiteTransaction p_transaction)
        {
            #region Manualy create INSERT command - not in use since SQLiteCommandBuilder is creating the INSERT command

            /* foreach (DataRow row in dtTable.Rows)
            {
                var strInsert = "INSERT INTO " + dtTable.TableName + " VALUES (";
                for (int intColumn = 0; intColumn < dtTable.Columns.Count; intColumn++)
                {
                    strInsert += "@" + dtTable.Columns[intColumn].ColumnName + ",";
                }
                strInsert = strInsert.TrimEnd(',') + ")";
                using (var cmdInsertRow = new SQLiteCommand(strInsert, con))
                {
                    for (int intColumn = 0; intColumn < dtTable.Columns.Count; intColumn++)
                    {
                        cmdInsertRow.Parameters.AddWithValue(dtTable.Columns[intColumn].ColumnName, row[intColumn]);
                    }
                    cmdInsertRow.ExecuteNonQuery();
                }
            }*/

            #endregion Manualy create INSERT command - not in use since SQLiteCommandBuilder is creating the INSERT command

            #region Truncate all data from table

            using (var dbCommand = p_connectionToTarget.CreateCommand())
            {
                dbCommand.Transaction = p_transaction;

                // SQLite got TRUNCATE optimizer
                // https://www.techonthenet.com/sqlite/truncate.php
                dbCommand.CommandText = "DELETE FROM " + p_dtTableSource.TableName;

                dbCommand.ExecuteNonQuery();
            }

            #endregion Truncate all data from table

            #region Fill SQLite Table from DataTable

            using (var dbCommand = p_connectionToTarget.CreateCommand())
            {
                dbCommand.Transaction = p_transaction;
                dbCommand.CommandText = "SELECT * FROM [" + p_dtTableSource.TableName + "]";
                using (var sqliteAdapterToTarget = new SQLiteDataAdapter(dbCommand))
                {
                    // Need this line (without it Update line would fail)
                    //https://www.devart.com/dotconnect/sqlite/docs/Devart.Data.SQLite~Devart.Data.SQLite.SQLiteCommandBuilder.html
                    using (new SQLiteCommandBuilder(sqliteAdapterToTarget))
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();

                        int intTotalRowsUpdatedFromSource = sqliteAdapterToTarget.Update(p_dtTableSource);
                        sw.Stop();
                        
                        throw new Exception(String.Format("Finished update table - <TableName = {0}> <TotalRowsUpdated = {1}>" +
                            " <ElapsedTime = {2}>", p_dtTableSource.TableName, intTotalRowsUpdatedFromSource, sw.Elapsed));
                    }
                }
            }

            #endregion Fill SQLite Table from DataTable
        }

        /// <summary>
        /// Send new instance of StronglyTyped Dataset
        /// returns same instance filled with data
        /// </summary>
        /// <param name="p_dataSetTarget">this parameter must be instanciated before calling the method!</param>
        /// <param name="p_strDBFullPathSource"></param>
        /// <returns></returns>
        public static bool FillDataSetFromSQLite(string p_strDBFullPathSource, DataSet p_dataSetTarget)
        {
            try
            {
                if (p_dataSetTarget == null)
                {
                    throw new Exception("Parameter p_dataSetTarget should not be null");
                }

                if (!File.Exists(p_strDBFullPathSource))
                {
                    throw new Exception(string.Format("File '{0}' not exists", p_strDBFullPathSource));
                    return false;
                }

                using (var con = new SQLiteConnection(@"data source=" + p_strDBFullPathSource))
                {
                    con.Open();

                    foreach (DataTable dataTable in p_dataSetTarget.Tables)
                    {
                        using (var adapter = new SQLiteDataAdapter("SELECT * FROM " + dataTable.TableName, con))
                        {
                            adapter.FillLoadOption = LoadOption.Upsert;
                            adapter.Fill(dataTable);
                        }
                    }

                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool SaveAllDataToDB(DataSet p_dataSetSource, string p_strDBFullPathTarget)
        {
            // if file exists - we will erase it, since the whole flow is to re-create everything
            // to create logic that dynamically UPDATES the data from DataSet is too complicated for now...
            if (File.Exists(p_strDBFullPathTarget))
            {
                File.Delete(p_strDBFullPathTarget);
            }

            // connection will create new file if needed
            using (var conn = new SQLiteConnection(@"data source=" + p_strDBFullPathTarget))
            {
                conn.Open();

                // without transaction, each command will commit, and therefore, the save process will be very slow.
                // that's why we use transaction.
                var transaction = conn.BeginTransaction();

                foreach (DataTable dataTableSource in p_dataSetSource.Tables)
                {
                    // create table if not exists
                    var strSqlCreateTable = GetSQLiteCreateTableIfNotExists(dataTableSource);
                    using (var cmdCreateTable = new SQLiteCommand(strSqlCreateTable, conn))
                    {
                        cmdCreateTable.ExecuteNonQuery();
                    }

                    // fill all data (auto-generate INSERT command for each row)
                    FillSQLiteSingleTableFromDataTable(dataTableSource, conn, transaction);
                }

                transaction.Commit();

                conn.Close();
            }
            return true;
        }

        public static string GetSQLiteCreateTableIfNotExists(DataTable p_dtTableSource)
        {
            // NOTE: in SQLite type is recommended, not required. Any column can still store any type of data!
            // https://www.sqlite.org/datatype3.html
            var typesToSqliteDictionary = new Dictionary<Type, string>();
            typesToSqliteDictionary.Add(typeof(System.Int16), "INTEGER");
            typesToSqliteDictionary.Add(typeof(System.Int32), "INTEGER");
            typesToSqliteDictionary.Add(typeof(System.Int64), "INTEGER");
            typesToSqliteDictionary.Add(typeof(System.Byte), "INTEGER");
            typesToSqliteDictionary.Add(typeof(System.Boolean), "INTEGER");
            typesToSqliteDictionary.Add(typeof(System.Decimal), "NUMERIC");
            typesToSqliteDictionary.Add(typeof(float), "REAL");
            typesToSqliteDictionary.Add(typeof(System.Double), "REAL");
            typesToSqliteDictionary.Add(typeof(System.String), "NVARCHAR");
            typesToSqliteDictionary.Add(typeof(System.DateTime), "TEXT"); // TEXT as ISO8601 strings ("YYYY-MM-DD HH:MM:SS.SSS").

            var strSqlCreateTable = "CREATE TABLE IF NOT EXISTS " + p_dtTableSource.TableName + "(";

            #region Columns definition

            for (int intColToGetDef = 0; intColToGetDef < p_dtTableSource.Columns.Count; intColToGetDef++)
            {
                #region Column definition

                strSqlCreateTable += " [" + p_dtTableSource.Columns[intColToGetDef].ColumnName + "] ";

                if (p_dtTableSource.Columns[intColToGetDef].DataType == typeof(System.String))
                {
                    if (p_dtTableSource.Columns[intColToGetDef].MaxLength == -1)
                    {
                        strSqlCreateTable += " TEXT ";
                    }
                    else
                    {
                        strSqlCreateTable += string.Format(" NVARCHAR({0}) ", p_dtTableSource.Columns[intColToGetDef].MaxLength);
                    }
                }
                else
                {
                    strSqlCreateTable += " " + typesToSqliteDictionary[p_dtTableSource.Columns[intColToGetDef].DataType] + " ";
                }

                // if this column is the only primary key
                // multiple primary key will be handled later
                if (p_dtTableSource.PrimaryKey.Length == 1 && p_dtTableSource.PrimaryKey.Contains(p_dtTableSource.Columns[intColToGetDef]))
                {
                    strSqlCreateTable += " PRIMARY KEY ";

                    // using AUTOINCREMENT explicitly without specifiyng PRIMARY KEY,
                    // will throw error - "near "AUTOINCREMENT": syntax error"
                    // have fun - http://stackoverflow.com/a/6157337/426315
                    if (p_dtTableSource.Columns[intColToGetDef].AutoIncrement)
                    {
                        //strSqlCreateTable += " IDENTITY(" + table.Columns[i].AutoIncrementSeed.ToString() + "," + table.Columns[i].AutoIncrementStep.ToString() + ") ";
                        strSqlCreateTable += " AUTOINCREMENT ";
                    }
                }

                if (p_dtTableSource.Columns[intColToGetDef].Unique)
                {
                    strSqlCreateTable += " UNIQUE ";
                }

                if (!p_dtTableSource.Columns[intColToGetDef].AllowDBNull)
                {
                    strSqlCreateTable += " NOT NULL ";
                }

                #endregion Column definition

                // add newline
                strSqlCreateTable += NEW_LINE_IN_TABLE_DEF;
            }

            #endregion Columns definition

            #region Relations to FK

            for (int intColToGetRelation = 0; intColToGetRelation < p_dtTableSource.Columns.Count; intColToGetRelation++)
            {
                // if this column needs FK
                var lstRelations =
                    p_dtTableSource.ParentRelations.Cast<DataRelation>()
                        .Where(r => r.ChildColumns.Contains(p_dtTableSource.Columns[intColToGetRelation]))
                        .ToList();

                if (lstRelations.Any())
                {
                    foreach (var relation in lstRelations)
                    {
                        // row for Example: FOREIGN KEY(trackartist) REFERENCES artist(artistid)
                        // https://www.sqlite.org/foreignkeys.html
                        strSqlCreateTable +=
                            string.Format(" FOREIGN KEY({0}) REFERENCES {1}({2}){3}",
                                            p_dtTableSource.Columns[intColToGetRelation].ColumnName,
                                            relation.ParentTable.TableName,
                                            string.Join(",", relation.ParentColumns.Select(pc => pc.ColumnName).ToArray()),
                                            NEW_LINE_IN_TABLE_DEF);
                    }
                }
            }

            #endregion Relations to FK

            #region Multiple PrimaryKeys

            // if there are multiple primary keys
            if (p_dtTableSource.PrimaryKey.Length > 1)
            {
                strSqlCreateTable +=
                    string.Format(" PRIMARY KEY ({0}){1}",
                                    string.Join(",", p_dtTableSource.PrimaryKey.Select(pk => pk.ColumnName).ToArray()),
                                    NEW_LINE_IN_TABLE_DEF);
            }

            #endregion Multiple PrimaryKeys

            // remove last comma
            strSqlCreateTable = strSqlCreateTable.TrimEnd(NEW_LINE_IN_TABLE_DEF.ToCharArray()) + ")";

            return strSqlCreateTable;
        }
    }
}