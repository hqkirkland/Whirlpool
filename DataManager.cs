using System;
using System.Collections.Generic;
using System.Text;

using WhirlpoolCore.Game;

using MySql.Data.MySqlClient;
using System.Data.Common;

namespace WhirlpoolCore
{
    static class DataManager
    {
        static MySqlConnectionStringBuilder ConnectionString;
        static MySqlClientFactory ConnectionFactory = new MySqlClientFactory();

        public static void InitializeData()
        {
            ConnectionString = new MySqlConnectionStringBuilder
            {
                Server = "127.0.0.1",
                UserID = "root",
                Password = "?????",
                Database = "Grolob",
                Pooling = true
            };
        }

        public static String GetUserGameTicket(String PlayerId)
        {
            using (MySqlConnection DataConnection = new MySqlConnection(ConnectionString.ToString()))
            {
                DataConnection.Open();

                MySqlCommand DbCmd = DataConnection.CreateCommand();
                DbCmd.Parameters.AddWithValue("@Username", PlayerId);
                DbCmd.CommandText = "SELECT " +
                                    "GameTicket " +
                                    "FROM `User` " +
                                    "WHERE Username = @Username";

                try
                {
                    MySqlDataReader DbReader = DbCmd.ExecuteReader();

                    if (!DbReader.HasRows)
                    {
                        return null;
                    }

                    DbReader.Read();

                    return DbReader.GetString("GameTicket");
                }

                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return "";
            }
        }

        public static User GetUserData(String PlayerId)
        {
            using (MySqlConnection DataConnection = new MySqlConnection(ConnectionString.ToString()))
            {
                DataConnection.Open();

                MySqlCommand DbCmd = DataConnection.CreateCommand();
                DbCmd.Parameters.AddWithValue("@Username", PlayerId);
                DbCmd.CommandText = "SELECT " +
                                    "Id, " +
                                    "Username, " +
                                    "Appearance " +
                                    "FROM `User` " +
                                    "WHERE Username = @Username";

                MySqlDataReader DbReader = DbCmd.ExecuteReader();

                if (!DbReader.HasRows)
                {
                    return null;
                }

                DbReader.Read();

                User NewUser = new User()
                {
                    Username = DbReader.GetString("Username"),
                    Appearance = DbReader.GetString("Appearance")
                };

                return NewUser;
            }
        }
        
        public static void InsertGameItems(List<GameItem> GameItemsList)
        {
            using (MySqlConnection DataConnection = new MySqlConnection(ConnectionString.ToString()))
            {
                DataConnection.Open();

                StringBuilder QueryBuilder;
                String InsertText = "INSERT INTO GameItem " + 
                "(Id, " + 
                "ItemType, " +
                "ItemName, " +
                "SetId, " +
                "Description, " +
                "Layered, " +
                "Cost, " +
                "Sale, " +
                "Available, " +
                "MembersOnly) VALUES\n";

                String SelectText = "SELECT Id, ItemName FROM GameItem WHERE Id = @GameItemId";

                List<int> SkippedItemSet = new List<int>();

                int CurrentItemIndex = 0;

                foreach (GameItem NewItem in GameItemsList)
                {
                    MySqlCommand DbCmd = DataConnection.CreateCommand();
                    DbCmd.CommandText = InsertText;

                    QueryBuilder = new StringBuilder();
                    QueryBuilder.Append("('");
                    QueryBuilder.Append(NewItem.Id.ToString());
                    QueryBuilder.Append("','");
                    QueryBuilder.Append(NewItem.ItemType);
                    QueryBuilder.Append("','");
                    QueryBuilder.Append(NewItem.ItemName.Replace('\'', '`'));
                    QueryBuilder.Append("','");
                    QueryBuilder.Append(NewItem.SetId.ToString());
                    QueryBuilder.Append("','");
                    QueryBuilder.Append(NewItem.Description);
                    QueryBuilder.Append("',");
                    QueryBuilder.Append(NewItem.Layered ? "\'Y\'" : "\'N\'");
                    QueryBuilder.Append(",");
                    QueryBuilder.Append(NewItem.Cost.ToString());
                    QueryBuilder.Append(",");
                    QueryBuilder.Append(NewItem.Sale.ToString());
                    QueryBuilder.Append(",");
                    QueryBuilder.Append(NewItem.Available ? "1" : "0");
                    QueryBuilder.Append(",");
                    QueryBuilder.Append(NewItem.MembersOnly ? "1": "0");
                    QueryBuilder.Append(");");
                    
                    DbCmd.CommandText += QueryBuilder.ToString();

                    try
                    {
                        DbCmd.ExecuteNonQuery();
                        Console.WriteLine("Loaded: {0}", QueryBuilder.ToString());
                    }

                    catch (MySqlException)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Skipped: {0}", QueryBuilder.ToString());
                        Console.ForegroundColor = ConsoleColor.White;

                        SkippedItemSet.Add(CurrentItemIndex);
                    }

                    CurrentItemIndex++;
                }

                if (SkippedItemSet.Count > 0)
                {
                    Console.WriteLine("Checking skipped items for duplicate entry and match..");
                    QueryBuilder = new StringBuilder();

                    foreach (int SkippedGameItemIndex in SkippedItemSet)
                    {
                        MySqlCommand DbCmd = DataConnection.CreateCommand();
                        DbCmd.CommandText = SelectText;
                        DbCmd.Parameters.AddWithValue("@GameItemId", GameItemsList[SkippedGameItemIndex].Id);

                        MySqlDataReader DbReader = DbCmd.ExecuteReader();
                        DbReader.Read();

                        if (DbReader.GetString("ItemName") == GameItemsList[SkippedGameItemIndex].ItemName)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Item ID {0} {1} already exists!", GameItemsList[SkippedGameItemIndex].Id, GameItemsList[SkippedGameItemIndex].ItemName);
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Item ID {0} does not match given name: {1} ", GameItemsList[SkippedGameItemIndex].Id, GameItemsList[SkippedGameItemIndex].ItemName);
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                        DbReader.Close();
                    }
                }
            }
        }

        public static Dictionary<int, GameItem> BuildGameItemCatalog()
        {
            Dictionary<int, GameItem> MasterSet = new Dictionary<int, GameItem>();

            using (MySqlConnection DataConnection = new MySqlConnection(ConnectionString.ToString()))
            {
                DataConnection.Open();

                MySqlCommand DbCmd = DataConnection.CreateCommand();
                
                DbCmd.CommandText = "SELECT " +
                                    "Id, " +
                                    "IconId, " +
                                    "ItemType," +
                                    "ItemName," +
                                    "Description," +
                                    "Cost," +
                                    "Sale," +
                                    "MembersOnly," +
                                    "Layered," +
                                    "Available" +
                                    "FROM `GameItem`";

                MySqlDataReader DbReader = DbCmd.ExecuteReader();

                if (!DbReader.HasRows)
                {
                    return MasterSet;
                }

                while (DbReader.Read())
                {
                    GameItem Item = new GameItem()
                    {
                        Id = DbReader.GetUInt16("Id"),
                        SetId = DbReader.GetInt16("SetId"),
                        ItemType = DbReader.GetString("ItemType")
                    };

                    MasterSet.Add(Item.Id, Item);
                }
            }

            return MasterSet;
        }
    }
}
