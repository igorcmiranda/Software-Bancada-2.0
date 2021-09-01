using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADODB;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Ajustador_Calibrador_ADR3000.Helpers
{
    public class MSAccess : IDisposable
    {
        //
        // Private classes and variables to be used
        //
        private bool disposed;
        private readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        private readonly Connection connection;

        /// <summary>
        /// Constructor for the MSAccess class
        /// </summary>
        /// <param name="_connectionString">The connection string for the Microsoft Access database.</param>
        public MSAccess(string _connectionString)
        {
            connection = new Connection
            {
                ConnectionString = _connectionString
            };
        }

        /// <summary>
        /// Opens the connection with the database.
        /// </summary>
        public void ConnectToDatabase()
        {
            connection.Open();
        }

        /// <summary>
        /// Closes the connection with the database.
        /// </summary>
        public void CloseConnection()
        {
            connection.Close();
        }

        /// <summary>
        /// Performs a select operation to the database and stores the records of the chosen fields.
        /// </summary>
        /// <param name="table">The table to look for data.</param>
        /// <param name="fields">A list with the wanted fields.</param>
        /// <param name="condition">A string containing any other condition for the select operation.</param>
        /// <param name="rs">The recordset where the records will be kept.</param>
        public void GetRecords(string table, List<string> fields, string condition, ref Recordset rs)
        {
            string SQL;
            int i;
         
            SQL = "SELECT ";
            for (i = 0; i < fields.Count; i++)
            {
                if(i==(fields.Count-1)) SQL += fields[i] + " ";
                else SQL += fields[i] + ", ";
            }
            SQL += "FROM " + table + " " + condition;
                           
            rs.Open(SQL, connection);
        }

        /// <summary>
        /// Inserts a new record into a specific table.
        /// </summary>
        /// <param name="table">The specific table.</param>
        /// <param name="fields">A list containing the field names.</param>
        /// <param name="values">A list containing the values for each field.</param>
        public void InsertData(string table,List<string> fields, List<string> values)
        {
            string SQL;
            int i;
            
            SQL = "INSERT INTO " + table + "(";
            for (i = 0; i < fields.Count; i++)
            {
                if (i == (fields.Count - 1)) SQL += fields[i] + ") ";
                else SQL += fields[i] + ", ";
            }
            SQL += "VALUES (";
            for (i = 0; i < values.Count; i++)
            {
                if (i == (values.Count - 1)) SQL += values[i] + ")";
                else SQL += values[i] + ", ";
            }
            connection.Execute(SQL, out object records);      
        }

        /// <summary>
        /// Updates contents of chosen fields from a table.
        /// </summary>
        /// <param name="table">The name of the table.</param>
        /// <param name="fields">A list containing wich fields to be updated.</param>
        /// <param name="values">The new values for the fields.</param>
        /// <param name="condition">A string containing any other condition for the update operation.</param>
        public void UpdateData(string table, List<string> fields, List<string> values, string condition)
        {
            string SQL;
            int i;
                       
            SQL = "UPDATE " + table + " SET ";
            for (i = 0; i < values.Count; i++)
            {
                if (i == (values.Count - 1)) SQL += fields[i] + "=" + values[i] + " ";
                else SQL += fields[i] + "=" + values[i] + ", ";
            }
            SQL += condition;
            connection.Execute(SQL, out object records);
        }

        /// <summary>
        /// Deletes a record of a table.
        /// </summary>
        /// <param name="table">The name of the table.</param>
        /// <param name="condition">A string containing any other condition for the delete operation.</param>
        public void DeleteData(string table, string condition)
        {
            string SQL;
            
            SQL = "DELETE FROM " + table + " " + condition;
            connection.Execute(SQL, out object records);
        }

        /// <summary>
        /// Counts how many records has a specific field.
        /// </summary>
        /// <param name="table">The name of the table.</param>
        /// <param name="field">The name of the field.</param>
        /// <param name="condition">A string containing any other condition for the counting operation.</param>
        /// <returns>The number of records.</returns>
        public long RecordsCount(string table, string field, string condition)
        {
            Recordset rs = new Recordset();
            string SQL = "SELECT COUNT(" + field + ") FROM " + table + " " + condition;
            
            rs.Open(SQL, connection);

            return (((dynamic)rs.Fields[0]).Value);
        }

        /// <summary>
        /// Finds the record with the maximum value for a specific field.
        /// </summary>
        /// <param name="table">The name of the table.</param>
        /// <param name="field">The wanted field.</param>
        /// <param name="condition">Any condition to use.</param>
        /// <param name="rs">The recordset to store the found record.</param>
        public void MaxRecord(string table, string field, string condition, ref Recordset rs)
        {
            string SQL = "SELECT MAX(" + field + ") FROM " + table + " " + condition;

            rs.Open(SQL, connection);
        }

        /// <summary>
        /// Selects fields from a table, but without repeated field values.
        /// </summary>
        /// <param name="table">The name of the table.</param>
        /// <param name="field">The wanted fields.</param>
        /// <param name="rs">The Recordset to store the records.</param>
        public void GetDistinctData(string table, string field, ref Recordset rs)
        {
            string SQL = "SELECT DISTINCT " + field + " FROM " + table;
            rs.Open(SQL, connection);          
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (handle != null) handle.Dispose();
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        /// <summary>
        /// Frees any used resources by the instance of the MSAccess class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
