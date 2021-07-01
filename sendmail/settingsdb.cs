using System;
using System.Data;
using System.Data.SQLite;

namespace sendmail
{
    class settingsdb
    {
        private const string DBName = @".\db\settingsdb.db";
        static SQLiteConnection db = new SQLiteConnection(string.Format("Data Source={0};Version=3;", DBName));

        // CONSTRUCTOR, VERIFICA CONEXION A LA BASE DE DATOS DE CONFIGURACION
        private settingsdb()
        {
            try
            {
                db.Open();
                Console.WriteLine("Conexion a BD satisfactoria!");
                db.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // INICIA UNA INSTANCIA DE LA BASE DE DATOS DE CONFIGURACION
        static private SQLiteConnection GetInstance()
        {
            db.Open();
            return db;
        }

        // CONSULTA TODOS LOS SERVIDORES SMTP CAPTURADOS
        static public DataTable qSelect()
        {
            var query = $"SELECT * FROM smtpServer";
            var con = GetInstance();
            var cmd = new SQLiteCommand(query, con);
            SQLiteDataReader dr = cmd.ExecuteReader();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            dt.Load(dr);
            ds.Tables.Add(dt);
            con.Close();
            return dt;
        }

        // CONSULTA SERVIDOR SMTP POR EL CUAL ENVIAR EL CORREO
        static public DataTable qSelectByName(string smtpName)
        {
            var query = $"SELECT * FROM smtpServer WHERE smtpName='{smtpName}'";
            var con = GetInstance();
            var cmd = new SQLiteCommand(query, con);
            SQLiteDataReader dr = cmd.ExecuteReader();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            dt.Load(dr);
            ds.Tables.Add(dt);
            con.Close();
            return dt;
        }

        // INGRESA UN REGISTRO SMTP PARA ENVIO DE CORREO
        static public void qInsert(
            string smtpName, 
            string smtpHost, 
            int smtpPort, 
            bool smtpEncrypt, 
            string smtpFrom,
            string smtpUser, 
            string smtpPass)
        {
            var query = "INSERT INTO smtpServer (smtpName,smtpHost,smtpPort,smtpEncrypt,smtpFrom,smtpUser,smtpPass) " +
                $"VALUES('{smtpName}', '{smtpHost}', {smtpPort}, '{smtpEncrypt}', '{smtpFrom}', '{smtpUser}', '{smtpPass}')";
            try
            {
                var con = GetInstance();
                var cmd = new SQLiteCommand(query, con);
                cmd.ExecuteNonQuery();
                con.Close();
                Console.WriteLine("Registro agregado correctamente.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // ACTUALIZA UN REGISTRO SMTP PARA  ENVIO DE CORREO

        // ELIMINA REGISTRO SMTP PARA ENVIO DE CORREO
        static public void qDelete(string smtpName)
        {
            var query = $"DELETE FROM smtpServer WHERE smtpName='{smtpName}'";
            try
            {
                var con = GetInstance();
                var cmd = new SQLiteCommand(query, con);
                cmd.ExecuteNonQuery();
                con.Close();
                Console.WriteLine("Registro se elimino correctamente\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
