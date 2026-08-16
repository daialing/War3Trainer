using System;
using System.Diagnostics;

namespace War3Trainer
{
    class GameContext
    {
        public int ProcessId { get; private set; }
        public string ProcessVersion { get; private set; }

        public UInt32 ThisGameAddress { get; private set; }
        public UInt32 UnitListAddress { get; private set; }
        public UInt32 MoveSpeedAddress { get; private set; }

        public UInt32 AttackAttributesOffset { get; private set; }
        public UInt32 HeroAttributesOffset { get; private set; }
        public UInt32 ItemsListOffset { get; private set; }
        public UInt32 MoveSpeedOffset { get; private set; }

        // 补充血量偏移量
        public UInt32 HPOffset { get; private set; }

        private uint _moduleAddress;

        public static GameContext FindGameRunning(string processName, string moduleName)
        {
            Process[] processesByName = Process.GetProcessesByName(processName);
            if (processesByName.Length > 0)
            {
                GameContext context = new GameContext(processesByName[0], moduleName);

                // 及时释放 Process 对象资源，防止句柄泄漏
                for (int i = 0; i < processesByName.Length; i++)
                {
                    processesByName[i].Dispose();
                }

                return context;
            }
            return null;
        }

        public GameContext(Process gameProcess, string moduleName)
        {
            GetProcessInfo(gameProcess);
            GetModuleInfo(gameProcess, moduleName);
            GetGameAddressAndOffset();
        }

        private void GetProcessInfo(Process gameProcess)
        {
            try
            {
                this.ProcessId = gameProcess.Id;
            }
            catch
            {
                throw new InvalidOperationException("Failed to fetch process Id");
            }
        }

        private void GetModuleInfo(Process gameProcess, string moduleName)
        {
            WindowsApi.ProcessModule mainModule =
                new WindowsApi.ProcessModule(
                    ProcessId,
                    moduleName);

            string moduleFileName = mainModule.FileName;
            FileVersionInfo moduleVersion = FileVersionInfo.GetVersionInfo(moduleFileName);
            string fileVersion = moduleVersion.FileVersion;
            if (fileVersion == null)
                throw new InvalidOperationException("Bad file version");

            this.ProcessVersion = fileVersion.Replace(", ", ".");
            _moduleAddress = (uint)mainModule.BaseAddress;
        }

        private void GetGameAddressAndOffset()
        {
            switch (ProcessVersion)
            {
                case "1.20.4.6074":
                    SetVersionData(0x87C744, 0x8722BC, 0x55BDF0, 0x1E4, 0x1EC, 0x1F4, 0x1D8, 0x1E0);
                    break;
                case "1.21.0.6263":
                    SetVersionData(0x87D7BC, 0x873334, 0x55FE80, 0x1E4, 0x1EC, 0x1F4, 0x1D8, 0x1E0);
                    break;
                case "1.21.1.6300":
                    SetVersionData(0x87D7BC, 0x873334, 0x55FEA0, 0x1E4, 0x1EC, 0x1F4, 0x1D8, 0x1E0);
                    break;
                case "1.22.0.6328":
                    SetVersionData(0xAA4178, 0xAA2FFC, 0x201190, 0x1E4, 0x1EC, 0x1F4, 0x1D8, 0x1E0);
                    break;
                case "1.23.0.6352":
                    SetVersionData(0xABCFC8, 0xABBE4C, 0x2026D0, 0x1E4, 0x1EC, 0x1F4, 0x1D8, 0x1E0);
                    break;
                case "1.24.0.6372":
                case "1.24.1.6374":
                case "1.24.2.6378":
                    SetVersionData(0xACE5E0, 0xACD44C, 0x202780, 0x1E4, 0x1EC, 0x1F4, 0x1D8, 0x1E0);
                    break;
                case "1.24.3.6384":
                    SetVersionData(0xACE5E0, 0xACD44C, 0x202780, 0x1E8, 0x1F0, 0x1F8, 0x1DC, 0x1E4);
                    break;
                case "1.24.4.6387":
                    SetVersionData(0xACE5E0, 0xACD44C, 0x2027E0, 0x1E8, 0x1F0, 0x1F8, 0x1DC, 0x1E4);
                    break;
                case "1.25.1.6397":
                    SetVersionData(0xAB7788, 0xAB65F4, 0x201AA0, 0x1E8, 0x1F0, 0x1F8, 0x1DC, 0x1E4);
                    break;
                case "1.26.0.6401":
                    SetVersionData(0xAB7788, 0xAB65F4, 0x201CD0, 0x1E8, 0x1F0, 0x1F8, 0x1DC, 0x1E4);
                    break;
                case "1.27.0.52240":
                    SetVersionData(0xBE40A8, 0xBE4238, 0x5DF420, 0x1E8, 0x1F0, 0x1F8, 0x1DC, 0x1E4);
                    break;

                case "1.27.1.7085":
                    SetVersionData(0xD68610, 0xD687A8, 0x5FCB40, 0x1E8, 0x1F0, 0x1F8, 0x1DC, 0x1E4);
                    break;

                   
                case "1.28.0.7205":
                    SetVersionData(0xD72F58, 0xD730F0, 0x604470, 0x1E8, 0x1F0, 0x1F8, 0x1DC, 0x1E4);
                    break;
                case "1.28.5.7680":
                    SetVersionData(0xD30448, 0xD305E0, 0x630C70, 0x1E8, 0x1F0, 0x1F8, 0x1DC, 0x1E4);
                    break;
                default:
                    // 保持原项目中的拼写，防止外部 catch 异常报错
                    throw new UnkonwnGameVersionExpection(
                        this.ProcessId,
                        ProcessVersion);
            }
        }

        private void SetVersionData(uint thisGame, uint unitList, uint moveSpeed,
                                    uint attackAttr, uint heroAttr, uint itemsList, uint moveSpeedOffset, uint hpOffset)
        {
            ThisGameAddress = _moduleAddress + thisGame;
            UnitListAddress = _moduleAddress + unitList;
            MoveSpeedAddress = _moduleAddress + moveSpeed;

            AttackAttributesOffset = attackAttr;
            HeroAttributesOffset = heroAttr;
            ItemsListOffset = itemsList;
            MoveSpeedOffset = moveSpeedOffset;
            HPOffset = hpOffset;
        }
    }
}