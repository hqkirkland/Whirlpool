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
    class SpineworldItemData
    {
        public String Description = "";

        private bool _canUse = false;
        private string _grolobItemType;

        private static List<String> _nativeTypes = new List<String> { "shoes", "pants", "shirt", "hair" };

        public int Id
        {
            get;
            set;
        }

        public String Type
        {
            get
            {
                return _grolobItemType;
            }

            set
            {
                this.Description = value;

                if (_nativeTypes.Contains(value.ToLower()))
                {
                    _grolobItemType = value;
                }

                else
                {
                    switch (value)
                    {
                        case "acc1":
                            _grolobItemType = "Glasses";
                            break;
                        case "acc2":
                            _grolobItemType = "Hat";
                            break;
                        case "base":
                            _grolobItemType = "Body";
                            break;
                        case "head":
                            _grolobItemType = "Face";
                            break;
                        default:
                            _grolobItemType = "Collectible";
                            break;
                    }    
                }
            }
        }

        public String GId
        {
            get;
            set;
        }

        public String Name
        {
            get;
            set;
        }

        public int Usable
        {
            get;
            set;
        }

        public int Givable
        {
            get;
            set;
        }

        public int Tier
        {
            get;
            set;
        }
    }
}