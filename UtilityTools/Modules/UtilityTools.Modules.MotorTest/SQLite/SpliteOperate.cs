using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace UtilityTools.Modules.MotorTest.SQLite
{
    public class SpliteOperate
    {
        public static readonly string dbName = "Data Source=Demo.db;Version=3;";
        /// <summary>
        /// 创建表格
        /// </summary>
        /// <param name="connectionString"></param>
        public static void CreateTable(string connectionString)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                /*
                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS MotorMessage (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    DeviceID TEXT NOT NULL,
                    Time TEXT NOT NULL,
                    FirmwareVersion TEXT,
                    HistoricalFirmwareVersion TEXT,
                    HardwareVersion TEXT,
                    SampleStageType TEXT,
                    MessageData TEXT
                )";
              */
                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Demo(
                  Id INTEGER PRIMARY KEY AUTOINCREMENT,
                  DeviceID TEXT NOT NULL
              )";



                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="name"></param>
        /// <param name="age"></param>
        public static void InsertData(string connectionString, string DeviceID, string Time, string FirmwareVersion,string HistoricalFirmwareVersion, string HardwareVersion, string SampleStageType, string MessageData)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string insertQuery = "INSERT INTO Demo (DeviceID) VALUES (@DeviceID)";
                using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@DeviceID", DeviceID);
                    /*
                command.Parameters.AddWithValue("@Time", Time);

                command.Parameters.AddWithValue("@FirmwareVersion", FirmwareVersion);
                command.Parameters.AddWithValue("@HistoricalFirmwareVersion", HistoricalFirmwareVersion);
                command.Parameters.AddWithValue("@HistoricalFirmwareVersion", HistoricalFirmwareVersion);
                command.Parameters.AddWithValue("@SampleStageType", HistoricalFirmwareVersion);
                command.Parameters.AddWithValue("@MessageData", MessageData);
                */
                    command.ExecuteNonQuery(); 
                }
            }
        }
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="connectionString"></param>
        public static void QueryData(string connectionString  )
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Demo";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Id: {reader["Id"]}, DeviceID: {reader["DeviceID"]}");
                        }
                    }
                }
            }

            Console.WriteLine("---");
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="name"></param>
        /// <param name="newAge"></param>
        public static void UpdateData(string connectionString, string name, int newAge)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string updateQuery = "UPDATE Users SET Age = @Age WHERE Name = @Name";
                using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@Age", newAge);
                    command.Parameters.AddWithValue("@Name", name);
                    command.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="name"></param>
        public static void DeleteData(string connectionString, string name)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM Users WHERE Name = @Name";
                using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
