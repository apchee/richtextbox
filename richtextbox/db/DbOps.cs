using richtextbox.userctrls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace richtextbox.db {
    class DbOps {
        const string TABLE_NAME = "posts";
        public List<Category> FindAll() {
            string sql = "select * from "+ TABLE_NAME;
            return ConvertRecordsToObjects(ExcuteSelectToTable(sql));
        }

        public byte[] LoadContent(int id) {
            string sql = "select * from " + TABLE_NAME + " where id = " + id;
            Category cate= FindFirst(sql);
            if(cate != null)
                return cate.Content;
            return null;
        }

        public bool Update(byte[] data, string version, int id) {
            var dataDic = new Dictionary<string, object>();
            dataDic[CONTENT] = data;
            dataDic["version"] = version;

            var condDic = new Dictionary<string, object>();
            condDic["id"] = id;

            return Update(dataDic, condDic)>0;
        }

        public int UpdateCateId(int id, byte[] content) {
            var data = new Dictionary<string, object>();
            var cond = new Dictionary<string, object>();
            cond["id"] = id;
            data[CONTENT] = content;
            return Update(data, cond);
        }

        public int Update(Dictionary<string, object> data, Dictionary<string, object> cond) {
            int rowsAffected = -1;
            using(SQLiteConnection conn = new SQLiteConnection(GetDBConn())) {
                using(SQLiteCommand cmd = new SQLiteCommand()) {
                    cmd.Connection = conn;
                    conn.Open();
                    SQLite3Helper su = new SQLite3Helper(cmd);
                    rowsAffected = su.Update(TABLE_NAME, data, cond);
                    conn.Close();
                }
            }
            return rowsAffected;
        }

        private Category FindFirst(string sql, SQLiteParameter[] parameters = null) {
            DataRow dr = ExcuteSelectFirstDataRow(sql, parameters);
            if(dr == null)
                return null;
            Category r = ConvertDicToObject(GetRecord(dr, dr.Table.Columns));
            return r;
        }

        private DataRow ExcuteSelectFirstDataRow(string sql, SQLiteParameter[] parameters = null) {
            DataTable res = ExcuteSelectToTable(sql, parameters);
            if(res != null && res.Rows.Count > 0)
                return res.Rows[0];
            return null;
        }

/*        private Dictionary<string, object> GetRecord(DataRow dr, DataColumnCollection cs) {
            var d = new Dictionary<string, object>();
            foreach(DataColumn dc in cs) {
                if(!(dr[dc.ColumnName] is DBNull)) {
                    if(VirtualFloatFields.Count > 0 && VirtualFloatFields.ContainsKey(dc.ColumnName)) {
                        d[dc.ColumnName] = Convert.ToDecimal(dr[dc.ColumnName].ToString());
                    } else
                        d[dc.ColumnName] = dr[dc.ColumnName];
                }
            }
            return d;
        }
*/


        private SQLiteConnection GetDBConn() {
            string f = Path.GetFullPath("../../test.sqlite");
            if(!File.Exists(f)) {
                throw new Exception("DB file doesn't exits. " + f);
            }
            string connectionStr = string.Format("Data Source={0};Version={1};", f, 3);

            SQLiteConnection connection = null;
            connection = new SQLiteConnection(connectionStr);

            return connection;
        }

        private DataTable ExcuteSelectToTable(string sql, SQLiteParameter[] parameters = null) {
            DataTable dt = null;
            //log.Info(string.Format("______ {0} - 001   -  at {1}", "ExcuteSelectToTable", DateTime.Now.ToString("HH:mm:ss fff tt")));
            using(SQLiteConnection conn = this.GetDBConn()) {
                //log.Info(string.Format("______ {0} - 002   -  at {1}", "ExcuteSelectToTable", DateTime.Now.ToString("HH:mm:ss fff tt")));
                using(SQLiteCommand cmd = new SQLiteCommand()) {
                    //log.Info(string.Format("______ {0} - 003   -  at {1}", "ExcuteSelectToTable", DateTime.Now.ToString("HH:mm:ss fff tt")));
                    conn.Open();
                    //log.Info(string.Format("______ {0} - 004   -  at {1}", "ExcuteSelectToTable", DateTime.Now.ToString("HH:mm:ss fff tt")));
                    cmd.Connection = conn;
                    SQLite3Helper sh = new SQLite3Helper(cmd);
                    //log.Info(string.Format("______ {0} - 005   -  at {1}", "ExcuteSelectToTable", DateTime.Now.ToString("HH:mm:ss fff tt")));
                    dt = sh.Select(sql, parameters);
                    //log.Info(string.Format("______ {0} - 006   -  at {1}", "ExcuteSelectToTable", DateTime.Now.ToString("HH:mm:ss fff tt")));
                    conn.Close();
                }
            }
            return dt;
        }


        private List<Category> ConvertRecordsToObjects(DataTable dt) {
            var rr = new List<Category>();
            if(dt.Rows == null || dt.Rows.Count == 0)
                return rr;

            foreach(var row in GetRecords(dt)) {
                rr.Add(ConvertDicToObject(row));
            }
            return rr;
        }

        private List<Dictionary<string, object>> GetRecords(DataTable dt) {
            var re = new List<Dictionary<string, object>>();
            DataColumnCollection cs = dt.Columns;
            foreach(DataRow dr in dt.Rows) {
                var d = GetRecord(dr, cs);
                re.Add(d);
            }
            return re;
        }

        private Dictionary<string, int> VirtualFloatFields = new Dictionary<string, int>();
        private Dictionary<string, object> GetRecord(DataRow dr, DataColumnCollection cs) {
            var d = new Dictionary<string, object>();
            foreach(DataColumn dc in cs) {
                if(!(dr[dc.ColumnName] is DBNull)) {
                    if(VirtualFloatFields.Count > 0 && VirtualFloatFields.ContainsKey(dc.ColumnName)) {
                        d[dc.ColumnName] = Convert.ToDecimal(dr[dc.ColumnName].ToString());
                    } else
                        d[dc.ColumnName] = dr[dc.ColumnName];
                }
            }
            return d;
        }


        private List<Category> ConvertDicsToObjects(List<Dictionary<string, object>> dt) {
            var rr = new List<Category>();
            foreach(var row in dt) {
                rr.Add(ConvertDicToObject(row));
            }
            return rr;
        }


        private Category ConvertDicToObject(Dictionary<string, object> row) {
            Category cate = new Category();
            cate.Id = int.Parse(row["id"].ToString());
            if(row.ContainsKey(CONTENT) && row[CONTENT] != DBNull.Value) {
                cate.Content = row[CONTENT] as byte[];
            }
            cate.Title = row["title"].ToString();
            cate.Version = row["version"].ToString();

            return cate;
        }

        private Dictionary<string, object> ConvertObjectToDict(Category cate) {
            var dic = new Dictionary<string, object>();
            dic[CONTENT] = cate.Content;
            dic["title"] = cate.Title;
            dic["version"] = cate.Version;
            return dic;
        }

        const string CONTENT = "content";
    }
}
