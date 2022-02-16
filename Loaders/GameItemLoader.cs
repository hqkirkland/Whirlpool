using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

using WhirlpoolCore.Game;

using MySql.Data.MySqlClient;
using System.Data.Common;

namespace WhirlpoolCore.Loaders
{
    static class GameItemLoader
    {
        private static ReadOnlySpan<byte> Utf8Bom => new byte[] { 0xEF, 0xBB, 0xBF };
        
        public static void InitializeListBuild(String FilePath)
        {
            if (File.Exists(FilePath))
            {
                ReadOnlySpan<byte> FileSpan = File.ReadAllBytes(FilePath);
                
                if (FileSpan.StartsWith(Utf8Bom))
                {
                    FileSpan.Slice(Utf8Bom.Length);
                }

                JsonSerializerOptions JsonParseOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                
                Dictionary<String, SpineworldItemData> ItemDict = JsonSerializer.Deserialize<Dictionary<String, SpineworldItemData>>(FileSpan, JsonParseOptions);
                
                List<GameItem> GameItemList = new List<GameItem>();
                List<String> FurnitureItemTypes = new List<String> { "table", "electronic", "bed", "misc", "lamp", "seat", "storage", "plant", "rug" };
                
                foreach (String ItemKey in ItemDict.Keys)
                {
                    SpineworldItemData ItemEntry = ItemDict[ItemKey];
                    
                    GameItem CurrentItem = new GameItem()
                    {
                        Id = int.Parse(ItemEntry.GId),
                        SetId = int.Parse(ItemKey),
                        ItemType = ItemEntry.Type,
                        ItemName = ItemEntry.Name,
                        Description = ItemEntry.Description,
                        Layered = false,
                        Cost = 0,
                        Sale = 0,
                        Available = true,
                        MembersOnly = false,
                        CanUse = false,
                        CanGive = false
                    };
                    
                    GameItemList.Add(CurrentItem);
                }
                
                DataManager.InsertGameItems(GameItemList);
            }
        }
    }
}