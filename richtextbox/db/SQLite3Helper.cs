using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 
/// </summary>
namespace richtextbox.db {
    /// <summary>
    /// This is the one of classes downloaded from Internet.
    /// </summary>

        public class SQLite3Helper {
            SQLiteCommand cmd = null;

            public SQLite3Helper(SQLiteCommand command) {
                cmd = command;
            }

            #region DB Info

            public DataTable GetTableStatus() {
                return Select("SELECT * FROM sqlite_master;");
            }

            public DataTable GetTableList() {
                DataTable dt = GetTableStatus();
                DataTable dt2 = new DataTable();
                dt2.Columns.Add("Tables");
                for(int i = 0; i < dt.Rows.Count; i++) {
                    string t = dt.Rows[i]["name"] + "";
                    if(t != "sqlite_sequence")
                        dt2.Rows.Add(t);
                }
                return dt2;
            }

            public DataTable GetColumnStatus(string tableName) {
                return Select(string.Format("PRAGMA table_info(`{0}`);", tableName));
            }

            public DataTable ShowDatabase() {
                return Select("PRAGMA database_list;");
            }

            #endregion

            #region Query

            public void BeginTransaction() {
                cmd.CommandText = "begin transaction;";
                cmd.ExecuteNonQuery();
            }

            public void Commit() {
                cmd.CommandText = "commit;";
                cmd.ExecuteNonQuery();
            }

            public void Rollback() {
                cmd.CommandText = "rollback";
                cmd.ExecuteNonQuery();
            }

            public DataTable Select(string sql) {
                return Select(sql, new List<SQLiteParameter>());
            }

            public DataTable Select(string sql, IEnumerable<SQLiteParameter> parameters = null) {
                cmd.CommandText = sql;
                if(parameters != null) {
                    foreach(var param in parameters) {
                        cmd.Parameters.Add(param);
                    }
                }
                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable();
                try {
                    da.Fill(dt);
                } catch(Exception ex) {
                    throw new Exception(sql, ex);
                }
                return dt;
            }

            public void Execute(string sql) {
                Execute(sql, new List<SQLiteParameter>());
            }

            public void Execute(string sql, Dictionary<string, object> dicParameters = null) {
                List<SQLiteParameter> lst = GetParametersList(dicParameters);
                Execute(sql, lst);
            }

            public void Execute(string sql, IEnumerable<SQLiteParameter> parameters = null) {
                cmd.CommandText = sql;
                if(parameters != null) {
                    foreach(var param in parameters) {
                        cmd.Parameters.Add(param);
                    }
                }
                cmd.ExecuteNonQuery();
            }

            public object ExecuteScalar(string sql) {
                cmd.CommandText = sql;
                return cmd.ExecuteScalar();
            }

            public object ExecuteScalar(string sql, Dictionary<string, object> dicParameters = null) {
                List<SQLiteParameter> lst = GetParametersList(dicParameters);
                return ExecuteScalar(sql, lst);
            }

            public object ExecuteScalar(string sql, IEnumerable<SQLiteParameter> parameters = null) {
                cmd.CommandText = sql;
                if(parameters != null) {
                    foreach(var parameter in parameters) {
                        cmd.Parameters.Add(parameter);
                    }
                }
                return cmd.ExecuteScalar();
            }

            public dataType ExecuteScalar<dataType>(string sql, Dictionary<string, object> dicParameters = null) {
                List<SQLiteParameter> lst = null;
                if(dicParameters != null) {
                    lst = new List<SQLiteParameter>();
                    foreach(KeyValuePair<string, object> kv in dicParameters) {
                        lst.Add(new SQLiteParameter(kv.Key, kv.Value));
                    }
                }
                return ExecuteScalar<dataType>(sql, lst);
            }

            public dataType ExecuteScalar<dataType>(string sql, IEnumerable<SQLiteParameter> parameters = null) {
                cmd.CommandText = sql;
                if(parameters != null) {
                    foreach(var parameter in parameters) {
                        cmd.Parameters.Add(parameter);
                    }
                }
                return (dataType)Convert.ChangeType(cmd.ExecuteScalar(), typeof(dataType));
            }

            public dataType ExecuteScalar<dataType>(string sql) {
                cmd.CommandText = sql;
                return (dataType)Convert.ChangeType(cmd.ExecuteScalar(), typeof(dataType));
            }

            private List<SQLiteParameter> GetParametersList(Dictionary<string, object> dicParameters) {
                List<SQLiteParameter> lst = new List<SQLiteParameter>();
                if(dicParameters != null) {
                    foreach(KeyValuePair<string, object> kv in dicParameters) {
                        lst.Add(new SQLiteParameter(kv.Key, kv.Value));
                    }
                }
                return lst;
            }

            public string Escape(string data) {
                data = data.Replace("'", "''");
                data = data.Replace("\\", "\\\\");
                return data;
            }

            public int Insert(string tableName, Dictionary<string, object> dic) {

                cmd.CommandText = BuildInsertSql(tableName, dic);

                foreach(KeyValuePair<string, object> kv in dic) {
                    cmd.Parameters.AddWithValue("@v" + kv.Key, kv.Value);
                }
                int i = 0;
                try {
                    i = cmd.ExecuteNonQuery();
                } catch(Exception) { }
                return i;
            }

            public object InsertWithReturnKey(string tableName, Dictionary<string, object> dic) {

                cmd.CommandText = BuildInsertSql(tableName, dic);

                foreach(KeyValuePair<string, object> kv in dic) {
                    cmd.Parameters.AddWithValue("@v" + kv.Key, kv.Value);
                }

                cmd.ExecuteNonQuery();
                string sql = @"select last_insert_rowid()";
                cmd.CommandText = sql;
                return cmd.ExecuteScalar();
            }

            public object InsertWithReturnKey(string tableName, DataColumnCollection Columns, string primaryKey, DataRow row) {
                //DataTable table = row.Table;
                cmd.CommandText = BuildInsertSQL(tableName, Columns, primaryKey);
                cmd.CommandType = System.Data.CommandType.Text;

                foreach(DataColumn column in Columns) {
                    if(!column.AutoIncrement) {
                        string parameterName = "@" + column.ColumnName;
                        InsertParameter(cmd, parameterName,
                                          column.ColumnName,
                                          row[column.ColumnName]);
                    }
                }
                cmd.ExecuteNonQuery();
                string sql = @"select last_insert_rowid()";
                cmd.CommandText = sql;
                return cmd.ExecuteScalar();
            }

            private static void InsertParameter(SQLiteCommand command,
                                         string parameterName,
                                         string sourceColumn,
                                         object value) {
                SQLiteParameter parameter = new SQLiteParameter(parameterName, value);

                parameter.Direction = ParameterDirection.Input;
                parameter.ParameterName = parameterName;
                parameter.SourceColumn = sourceColumn;
                parameter.SourceVersion = DataRowVersion.Current;

                command.Parameters.Add(parameter);
            }

            private static string BuildInsertSQL(string tableName, DataColumnCollection Columns, string pk) {
                StringBuilder sql = new StringBuilder("INSERT INTO " + tableName + " (");
                StringBuilder values = new StringBuilder("VALUES (");
                bool bFirst = true;
                bool bIdentity = false;
                string identityType = null;

                foreach(DataColumn column in Columns) {
                    if(column.AutoIncrement || column.ColumnName.Equals(pk)) {
                        bIdentity = true;

                        switch(column.DataType.Name) {
                            case "Int16":
                                identityType = "smallint";
                                break;
                            case "SByte":
                                identityType = "tinyint";
                                break;
                            case "Int64":
                                identityType = "bigint";
                                break;
                            case "Decimal":
                                identityType = "decimal";
                                break;
                            default:
                                identityType = "int";
                                break;
                        }
                    } else {
                        if(bFirst)
                            bFirst = false;
                        else {
                            sql.Append(", ");
                            values.Append(", ");
                        }

                        sql.Append(column.ColumnName);
                        values.Append("@");
                        values.Append(column.ColumnName);
                    }
                }
                sql.Append(") ");
                sql.Append(values.ToString());
                sql.Append(")");

                //if(bIdentity) {
                //    sql.Append("; SELECT CAST(scope_identity() AS ");
                //    sql.Append(identityType);
                //    sql.Append(")");
                //}

                return sql.ToString();

            }

            public static string BuildInsertSql(string tableName, Dictionary<string, object> dic) {
                StringBuilder sbCol = new System.Text.StringBuilder();
                StringBuilder sbVal = new System.Text.StringBuilder();

                foreach(KeyValuePair<string, object> kv in dic) {
                    if(sbCol.Length == 0) {
                        sbCol.Append("insert into ");
                        sbCol.Append(tableName);
                        sbCol.Append("(");
                    } else {
                        sbCol.Append(",");
                    }

                    sbCol.Append("`");
                    sbCol.Append(kv.Key);
                    sbCol.Append("`");

                    if(sbVal.Length == 0) {
                        sbVal.Append(" values(");
                    } else {
                        sbVal.Append(", ");
                    }

                    sbVal.Append("@v");
                    sbVal.Append(kv.Key);
                }

                sbCol.Append(") ");
                sbVal.Append(");");
                return sbCol.ToString() + sbVal.ToString();
            }

            public int Update(string tableName, Dictionary<string, object> dicData, string colCond, object varCond) {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic[colCond] = varCond;
                return Update(tableName, dicData, dic);
            }

            public int Update(string tableName, Dictionary<string, object> dicData, Dictionary<string, object> dicCond) {
                if(dicData.Count == 0)
                    throw new Exception("dicData is empty.");

                StringBuilder sbData = new System.Text.StringBuilder();

                Dictionary<string, object> _dicTypeSource = new Dictionary<string, object>();

                foreach(KeyValuePair<string, object> kv1 in dicData) {
                    _dicTypeSource[kv1.Key] = null;
                }

                foreach(KeyValuePair<string, object> kv2 in dicCond) {
                    if(!_dicTypeSource.ContainsKey(kv2.Key))
                        _dicTypeSource[kv2.Key] = null;
                }

                sbData.Append("update `");
                sbData.Append(tableName);
                sbData.Append("` set ");

                bool firstRecord = true;

                foreach(KeyValuePair<string, object> kv in dicData) {
                    if(firstRecord)
                        firstRecord = false;
                    else
                        sbData.Append(",");

                    sbData.Append("`");
                    sbData.Append(kv.Key);
                    sbData.Append("` = ");

                    sbData.Append("@v");
                    sbData.Append(kv.Key);
                }

                sbData.Append(" where ");

                firstRecord = true;

                foreach(KeyValuePair<string, object> kv in dicCond) {
                    if(firstRecord)
                        firstRecord = false;
                    else {
                        sbData.Append(" and ");
                    }

                    sbData.Append("`");
                    sbData.Append(kv.Key);
                    sbData.Append("` = ");

                    sbData.Append("@c");
                    sbData.Append(kv.Key);
                }

                sbData.Append(";");

                cmd.CommandText = sbData.ToString();

                foreach(KeyValuePair<string, object> kv in dicData) {
                    cmd.Parameters.AddWithValue("@v" + kv.Key, kv.Value);
                }

                foreach(KeyValuePair<string, object> kv in dicCond) {
                    cmd.Parameters.AddWithValue("@c" + kv.Key, kv.Value);
                }

                int r = cmd.ExecuteNonQuery();
                return r;
            }
            public int Update(string sql) {
                //Console.WriteLine("Update Sql: "+sql);
                cmd.CommandText = sql;
                return cmd.ExecuteNonQuery();
            }

            public long LastInsertRowId() {
                return ExecuteScalar<long>("select last_insert_rowid();");
            }

            #endregion

            #region Utilities

            public void CreateTable(SQLiteTable table) {
                StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("create table if not exists `");
                sb.Append(table.TableName);
                sb.AppendLine("`(");

                bool firstRecord = true;

                foreach(SQLiteColumn col in table.Columns) {
                    if(col.ColumnName.Trim().Length == 0) {
                        throw new Exception("Column name cannot be blank.");
                    }

                    if(firstRecord)
                        firstRecord = false;
                    else
                        sb.AppendLine(",");

                    sb.Append(col.ColumnName);
                    sb.Append(" ");

                    if(col.AutoIncrement) {

                        sb.Append("integer primary key autoincrement");
                        continue;
                    }

                    switch(col.ColDataType) {
                        case ColType.Text:
                            sb.Append("text");
                            break;
                        case ColType.Integer:
                            sb.Append("integer");
                            break;
                        case ColType.Decimal:
                            sb.Append("decimal");
                            break;
                        case ColType.DateTime:
                            sb.Append("datetime");
                            break;
                        case ColType.BLOB:
                            sb.Append("blob");
                            break;
                    }

                    if(col.PrimaryKey)
                        sb.Append(" primary key");
                    else if(col.NotNull)
                        sb.Append(" not null");
                    else if(col.DefaultValue.Length > 0) {
                        sb.Append(" default ");

                        if(col.DefaultValue.Contains(" ") || col.ColDataType == ColType.Text || col.ColDataType == ColType.DateTime) {
                            sb.Append("'");
                            sb.Append(col.DefaultValue);
                            sb.Append("'");
                        } else {
                            sb.Append(col.DefaultValue);
                        }
                    }
                }

                sb.AppendLine(");");

                cmd.CommandText = sb.ToString();
                cmd.ExecuteNonQuery();
            }

            public void RenameTable(string tableFrom, string tableTo) {
                cmd.CommandText = string.Format("alter table `{0}` rename to `{1}`;", tableFrom, tableTo);
                cmd.ExecuteNonQuery();
            }

            public void CopyAllData(string tableFrom, string tableTo) {
                DataTable dt1 = Select(string.Format("select * from `{0}` where 1 = 2;", tableFrom));
                DataTable dt2 = Select(string.Format("select * from `{0}` where 1 = 2;", tableTo));

                Dictionary<string, bool> dic = new Dictionary<string, bool>();

                foreach(DataColumn dc in dt1.Columns) {
                    if(dt2.Columns.Contains(dc.ColumnName)) {
                        if(!dic.ContainsKey(dc.ColumnName)) {
                            dic[dc.ColumnName] = true;
                        }
                    }
                }

                foreach(DataColumn dc in dt2.Columns) {
                    if(dt1.Columns.Contains(dc.ColumnName)) {
                        if(!dic.ContainsKey(dc.ColumnName)) {
                            dic[dc.ColumnName] = true;
                        }
                    }
                }

                StringBuilder sb = new System.Text.StringBuilder();

                foreach(KeyValuePair<string, bool> kv in dic) {
                    if(sb.Length > 0)
                        sb.Append(",");

                    sb.Append("`");
                    sb.Append(kv.Key);
                    sb.Append("`");
                }

                StringBuilder sb2 = new System.Text.StringBuilder();
                sb2.Append("insert into `");
                sb2.Append(tableTo);
                sb2.Append("`(");
                sb2.Append(sb.ToString());
                sb2.Append(") select ");
                sb2.Append(sb.ToString());
                sb2.Append(" from `");
                sb2.Append(tableFrom);
                sb2.Append("`;");

                cmd.CommandText = sb2.ToString();
                cmd.ExecuteNonQuery();
            }

            public void DropTable(string table) {
                cmd.CommandText = string.Format("drop table if exists `{0}`", table);
                cmd.ExecuteNonQuery();
            }

            public void UpdateTableStructure(string targetTable, SQLiteTable newStructure) {
                newStructure.TableName = targetTable + "_temp";

                CreateTable(newStructure);

                CopyAllData(targetTable, newStructure.TableName);

                DropTable(targetTable);

                RenameTable(newStructure.TableName, targetTable);
            }

            public void AttachDatabase(string database, string alias) {
                Execute(string.Format("attach '{0}' as {1};", database, alias));
            }

            public void DetachDatabase(string alias) {
                Execute(string.Format("detach {0};", alias));
            }

            #endregion

        }
        public class SQLiteTable {
            public string TableName = "";
            public SQLiteColumnList Columns = new SQLiteColumnList();

            public SQLiteTable() { }

            public SQLiteTable(string name) {
                TableName = name;
            }
        }

        public class SQLiteColumnList : IList<SQLiteColumn> {
            List<SQLiteColumn> _lst = new List<SQLiteColumn>();

            private void CheckColumnName(string colName) {
                for(int i = 0; i < _lst.Count; i++) {
                    if(_lst[i].ColumnName == colName)
                        throw new Exception("Column name of \"" + colName + "\" is already existed.");
                }
            }

            public int IndexOf(SQLiteColumn item) {
                return _lst.IndexOf(item);
            }

            public void Insert(int index, SQLiteColumn item) {
                CheckColumnName(item.ColumnName);

                _lst.Insert(index, item);
            }

            public void RemoveAt(int index) {
                _lst.RemoveAt(index);
            }

            public SQLiteColumn this[int index] {
                get {
                    return _lst[index];
                }
                set {
                    if(_lst[index].ColumnName != value.ColumnName) {
                        CheckColumnName(value.ColumnName);
                    }

                    _lst[index] = value;
                }
            }

            public void Add(SQLiteColumn item) {
                CheckColumnName(item.ColumnName);

                _lst.Add(item);
            }

            public void Clear() {
                _lst.Clear();
            }

            public bool Contains(SQLiteColumn item) {
                return _lst.Contains(item);
            }

            public void CopyTo(SQLiteColumn[] array, int arrayIndex) {
                _lst.CopyTo(array, arrayIndex);
            }

            public int Count {
                get { return _lst.Count; }
            }

            public bool IsReadOnly {
                get { return false; }
            }

            public bool Remove(SQLiteColumn item) {
                return _lst.Remove(item);
            }

            public IEnumerator<SQLiteColumn> GetEnumerator() {
                return _lst.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return _lst.GetEnumerator();
            }
        }

        public class SQLiteColumn {
            public string ColumnName = "";
            public bool PrimaryKey = false;
            public ColType ColDataType = ColType.Text;
            public bool AutoIncrement = false;
            public bool NotNull = false;
            public string DefaultValue = "";

            public SQLiteColumn() { }

            public SQLiteColumn(string colName) {
                ColumnName = colName;
                PrimaryKey = false;
                ColDataType = ColType.Text;
                AutoIncrement = false;
            }

            public SQLiteColumn(string colName, ColType colDataType) {
                ColumnName = colName;
                PrimaryKey = false;
                ColDataType = colDataType;
                AutoIncrement = false;
            }

            public SQLiteColumn(string colName, bool autoIncrement) {
                ColumnName = colName;

                if(autoIncrement) {
                    PrimaryKey = true;
                    ColDataType = ColType.Integer;
                    AutoIncrement = true;
                } else {
                    PrimaryKey = false;
                    ColDataType = ColType.Text;
                    AutoIncrement = false;
                }
            }

            public SQLiteColumn(string colName, ColType colDataType, bool primaryKey, bool autoIncrement, bool notNull, string defaultValue) {
                ColumnName = colName;

                if(autoIncrement) {
                    PrimaryKey = true;
                    ColDataType = ColType.Integer;
                    AutoIncrement = true;
                } else {
                    PrimaryKey = primaryKey;
                    ColDataType = colDataType;
                    AutoIncrement = false;
                    NotNull = notNull;
                    DefaultValue = defaultValue;
                }
            }
        }

    public enum ColType {
        Text,
        DateTime,
        Integer,
        Decimal,
        BLOB
    }

}
