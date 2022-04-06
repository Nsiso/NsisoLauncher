using Microsoft.Win32;
using System;
using System.Linq;
using System.Management;
using System.Text;
using Hardware.Info;
using System.Runtime.InteropServices;

namespace NsisoLauncherCore.Util
{
    public enum ArchEnum
    {
        X86,
        X64,
        Arm,
        Arm64
    }

    public enum OsType
    {
        Windows,
        Linux,
        MacOSX,
        Unknown
    }

    public class SystemTools
    {
        /// <summary>
        /// 获取匹配JAVA位数的最佳内存
        /// </summary>
        /// <param name="arch">JAVA位数</param>
        /// <returns>最佳内存大小</returns>
        public static int GetBestMemory(Java java)
        {
            if (java != null)
            {
                int rm = Convert.ToInt32(Math.Floor(GetRunmemory() * 0.6));
                switch (java.Arch)
                {
                    case ArchEnum.X86:
                        if (rm > 1024) { return 1024; }
                        else { return rm; }

                    case ArchEnum.X64:
                        if (rm > 4096) { return 4096; }
                        else { return rm; }

                    default:
                        return rm;
                }
            }
            else
            {
                return 1024;
            }

        }

        /// <summary>
        /// 获取电脑总内存(MB)
        /// </summary>
        /// <returns>物理内存</returns>
        public static ulong GetTotalMemory()
        {
            HardwareInfo hardwareInfo = new HardwareInfo();
            hardwareInfo.RefreshMemoryStatus();
            ulong total = hardwareInfo.MemoryStatus.TotalPhysical;
            return total / 1048576;
        }

        /// <summary>
        ///     获取系统位数
        /// </summary>
        /// <returns>32 or 64</returns>
        public static ArchEnum GetSystemArch()
        {
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.Arm:
                    return ArchEnum.Arm;
                case Architecture.Arm64:
                    return ArchEnum.Arm64;
                case Architecture.X64:
                    return ArchEnum.X64;
                case Architecture.X86:
                    return ArchEnum.X86;
                default:
                    throw new NotImplementedException();
            }
        }

        public static OsType GetOsType()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OsType.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OsType.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OsType.MacOSX;
            }
            return OsType.Windows;
        }

        /// <summary>
        /// 获取系统剩余内存(MB)
        /// </summary>
        /// <returns>剩余内存</returns>
        public static ulong GetRunmemory()
        {
            HardwareInfo hardwareInfo = new HardwareInfo();
            hardwareInfo.RefreshMemoryStatus();
            ulong avail = hardwareInfo.MemoryStatus.AvailablePhysical;
            return avail / 1048576;
        }

        /// <summary>
        /// 获取显卡信息
        /// </summary>
        /// <returns></returns>
        public static string GetVideoCardInfo()
        {
            try
            {
                HardwareInfo hardwareInfo = new HardwareInfo();
                hardwareInfo.RefreshVideoControllerList();
                var videoControllers = hardwareInfo.VideoControllerList;

                var sb = new StringBuilder();
                foreach (var item in videoControllers)
                {
                    sb.AppendLine(item.ToString());
                }
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取CPU信息
        /// </summary>
        /// <returns></returns>
        public static string GetProcessorInfo()
        {
            try
            {
                var sb = new StringBuilder();
                var i = 0;
                foreach (var mo in new ManagementClass("WIN32_Processor").GetInstances().Cast<ManagementObject>())
                {
                    sb.Append('#').Append(i++).Append(mo["Name"].ToString().Trim()).Append(' ');
                }
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetSystemSummary()
        {
            HardwareInfo hardwareInfo = new HardwareInfo();
            hardwareInfo.RefreshCPUList(false);
            hardwareInfo.RefreshMemoryStatus();
            hardwareInfo.RefreshMemoryList();
            hardwareInfo.RefreshMotherboardList();
            hardwareInfo.RefreshBIOSList();
            hardwareInfo.RefreshVideoControllerList();
            hardwareInfo.RefreshSoundDeviceList();
            hardwareInfo.RefreshNetworkAdapterList();
            hardwareInfo.RefreshDriveList();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("The system summary information:");
            builder.AppendLine();

            // CPU
            builder.AppendLine("The CPU informations");
            foreach (var item in hardwareInfo.CpuList)
            {
                builder.AppendLine(item.ToString());
            }
            builder.AppendLine();

            // RAM
            builder.AppendLine("The Memory informations");
            hardwareInfo.MemoryStatus.ToString();
            builder.AppendLine("The Memory sticks informations");
            foreach (var item in hardwareInfo.MemoryList)
            {
                builder.AppendLine(item.ToString());
            }
            builder.AppendLine();

            // MOTHERBOARD
            builder.AppendLine("The motherboard informations");
            foreach (var item in hardwareInfo.MotherboardList)
            {
                builder.AppendLine(item.ToString());
            }
            builder.AppendLine();

            // BIOS
            builder.AppendLine("The BIOS informations");
            foreach (var item in hardwareInfo.BiosList)
            {
                builder.AppendLine(item.ToString());
            }
            builder.AppendLine();

            // VIDEO CONTROLLER
            builder.AppendLine("The video controller informations");
            foreach (var item in hardwareInfo.VideoControllerList)
            {
                builder.AppendLine(item.ToString());
            }
            builder.AppendLine();

            // SOUND DEVICE
            builder.AppendLine("The sound devices informations");
            foreach (var item in hardwareInfo.SoundDeviceList)
            {
                builder.AppendLine(item.ToString());
            }
            builder.AppendLine();

            // NETWORK
            builder.AppendLine("The network adapter informations");
            foreach (var item in hardwareInfo.NetworkAdapterList)
            {
                builder.AppendLine(item.ToString());
            }
            builder.AppendLine();

            // DRIVE
            builder.AppendLine("The drive informations");
            foreach (var item in hardwareInfo.DriveList)
            {
                builder.AppendLine(item.ToString());
            }
            builder.AppendLine();

            // OS
            builder.AppendLine("The os informations");
            builder.AppendLine(RuntimeInformation.OSDescription);
            builder.AppendLine(Environment.OSVersion.ToString());
            builder.AppendLine("Process Architecture: " + RuntimeInformation.ProcessArchitecture);
            builder.AppendLine("OS Architecture: " + RuntimeInformation.OSArchitecture);
            builder.AppendLine();

            // PROG
            builder.AppendLine("The prog informations");
            builder.AppendLine("Command Line:" + Environment.CommandLine);
            builder.AppendLine("Current Directory:" + Environment.CurrentDirectory);
            builder.AppendLine();

            return builder.ToString();
        }
    }
}
