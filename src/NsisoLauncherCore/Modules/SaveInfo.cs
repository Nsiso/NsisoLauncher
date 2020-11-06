using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cyotek.Data.Nbt;

namespace NsisoLauncherCore.Modules
{
    public class SaveInfo
    {

        /// <summary>
        /// 地图基础路径
        /// </summary>
        public string SaveBasePath { get; private set; }

        /// <summary>
        /// 地图icon路径
        /// </summary>
        public string IconPath { get => SaveBasePath + "\\icon.png"; }

        /// <summary>
        /// 地图名称
        /// </summary>
        public string LevelName { get; private set; }

        /// <summary>
        /// 游戏类型
        /// </summary>
        public GameType GameType { get; private set; }

        /// <summary>
        /// 硬核模式
        /// </summary>
        public bool Hardcore { get; private set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 源地图 NBT Tag字典
        /// </summary>
        public TagDictionary SourceTagDictionary { get; set; }

        public SaveInfo(string mapPath, TagDictionary tags)
        {
            this.SaveBasePath = mapPath;
            SourceTagDictionary = tags;
            if (tags.Contains("LevelName"))
            {
                LevelName = (string)tags["LevelName"].GetValue();
            }
            if (tags.Contains("GameType"))
            {
                GameType = (GameType)tags["GameType"].GetValue();
            }
            if (tags.Contains("hardcore"))
            {
                byte hardcoreByte = (byte)tags["hardcore"].GetValue();
                if (hardcoreByte == 1)
                {
                    Hardcore = true;
                }
                else
                {
                    Hardcore = false;
                }
            }
            if (tags.Contains("LastPlayed"))
            {
                long binTimeStamp = (long)tags["LastPlayed"].GetValue();
                Time = UnixTimeStampToDateTime(binTimeStamp);
            }
        }

        public void DeleteSave()
        {
            try
            {
                Directory.Delete(SaveBasePath, true);
            }
            catch (Exception)
            {
                //icon img?
            }

        }

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }

    public enum GameType
    {
        SURVIVAL = 0,
        CREATIVE = 1,
        ADVENTURE = 2,
        SPECTATOR = 3
    }
}
