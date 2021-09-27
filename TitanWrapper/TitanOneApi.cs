using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TitanWrapper
{
    public class TitanOne
    {
        public enum KEY_STATE
        {
            UP = 0,
            DOWN = 100,
            AXIS_NUR = 0,
            AXIS_MIN = -100,
            AXIS_MAX = 100
        }

        public enum KEY_MAP_PS3
        {
            PS = 0,
            SELECT = 1,
            START = 2,
            R1 = 3,
            R2 = 4,
            R3 = 5,
            L1 = 6,
            L2 = 7,
            L3 = 8,
            RX = 9,
            RY = 10,
            LX = 11,
            LY = 12,
            UP = 13,
            DOWN = 14,
            LEFT = 15,
            RIGHT = 16,
            TRIANGLE = 17,
            CIRCLE = 18,
            CROSS = 19,
            SQUARE = 20,
            ACCX = 21,
            ACCY = 22,
            ACCZ = 23,
            GYRO = 24,
        }

        public enum KEY_MAP_PS4
        {
            PS = 0,
            SHARE = 1,
            OPTIONS = 2,
            R1 = 3,
            R2 = 4,
            R3 = 5,
            L1 = 6,
            L2 = 7,
            L3 = 8,
            RX = 9,
            RY = 10,
            LX = 11,
            LY = 12,
            UP = 13,
            DOWN = 14,
            LEFT = 15,
            RIGHT = 16,
            TRIANGLE = 17,
            CIRCLE = 18,
            CROSS = 19,
            SQUARE = 20,
            ACCX = 21,
            ACCY = 22,
            ACCZ = 23,
            GYROX = 24,
            GYROY = 25,
            GYROZ = 26,
            TOUCH = 27,
            TOUCHX = 28,
            TOUCHY = 29,
        }

        public enum KEY_MAP_XB360
        {
            XBOX = 0,
            BACK = 1,
            START = 2,
            RB = 3,
            RT = 4,
            RS = 5,
            LB = 6,
            LT = 7,
            LS = 8,
            RX = 9,
            RY = 10,
            LX = 11,
            LY = 12,
            UP = 13,
            DOWN = 14,
            LEFT = 15,
            RIGHT = 16,
            Y = 17,
            B = 18,
            A = 19,
            X = 20,
        }

        public enum KEY_MAP_XB1
        {
            XBOX = 0,
            VIEW = 1,
            MENU = 2,
            RB = 3,
            RT = 4,
            RS = 5,
            LB = 6,
            LT = 7,
            LS = 8,
            RX = 9,
            RY = 10,
            LX = 11,
            LY = 12,
            UP = 13,
            DOWN = 14,
            LEFT = 15,
            RIGHT = 16,
            Y = 17,
            B = 18,
            A = 19,
            X = 20,
        }

        public enum KEY_MAP_SWITCH
        {
            HOME = 0,
            MINUS = 1,
            PLUS = 2,
            R = 3,
            ZR = 4,
            SR = 5,
            L = 6,
            ZL = 7,
            SL = 8,
            RX = 9,
            RY = 10,
            LX = 11,
            LY = 12,
            UP = 13,
            DOWN = 14,
            LEFT = 15,
            RIGHT = 16,
            X = 17,
            A = 18,
            B = 19,
            Y = 20,
            ACCX = 21,
            ACCY = 22,
            ACCZ = 23,
            GYROX = 24,
            GYROY = 25,
            GYROZ = 26,
            CAPTURE = 27,
        }

        public enum DEVICE_TYPE
        {
            None = 0, 
            PS3 = 1,
            XB360 = 2,
            PS4 = 3,
            XB1 = 4,
            SWITCH = 5
        }

        public static readonly Dictionary<DEVICE_TYPE, System.Type> DEVICE_MAP = new Dictionary<DEVICE_TYPE, Type>()
        {
            {DEVICE_TYPE.PS3, typeof(KEY_MAP_PS3) },
            {DEVICE_TYPE.PS4, typeof(KEY_MAP_PS4) },
            {DEVICE_TYPE.XB360, typeof(KEY_MAP_XB360) },
            {DEVICE_TYPE.XB1, typeof(KEY_MAP_XB1) },
            {DEVICE_TYPE.SWITCH, typeof(KEY_MAP_SWITCH) },
        };

        public DEVICE_TYPE CurrentOutputType { get { return outputType; } }
        public bool IsConnected { get { return _IsConnected(); } }

        private DEVICE_TYPE outputType = DEVICE_TYPE.None;

        private IntPtr hModule;
        private bool functionsLoaded = false;
        private bool isUnloaded = false;
        private GCDAPI_Load _Load;
        private GCDAPI_Unload _Unload;
        private GCAPI_IsConnected _IsConnected;
        private GCAPI_GetFWVer _GetFWVer;
        private GCAPI_Read _Read;
        private GCAPI_Write _Write;
        private GCAPI_GetTimeVal _GetTimeVal;
        private GCAPI_CalcPressTime _CalcPressTime;

        private sbyte[] outputState = new sbyte[GCMAPIConstants.Output];

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GCDAPI_Load();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void GCDAPI_Unload();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GCAPI_IsConnected();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate ushort GCAPI_GetFWVer();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate ushort GPPAPI_DevicePID();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GCAPI_Read([In, Out] ref GCMAPIReport Report);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GCAPI_Write(sbyte[] Output);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GCAPI_GetTimeVal();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GCAPI_CalcPressTime(uint Button);

#pragma warning disable 0649

        private struct GCMAPIConstants
        {
            public const int Input = 30;
            public const int Output = 36;
        }

        private struct GCMAPIStatus
        {
            public sbyte Value;
            public sbyte Previous;
            public int Holding;
        }

        private struct GCMAPIReport
        {
            public byte Console;
            public byte Controller;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] LED;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] Rumble;
            public byte Battery;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = GCMAPIConstants.Input, ArraySubType = UnmanagedType.Struct)]
            public GCMAPIStatus[] Input;
        }

#pragma warning restore 0649

        public TitanOne()
        {
        }

        ~TitanOne()
        {
            Unload();
        }

        public bool Init()
        {
            if (!functionsLoaded)
            {
                try
                {
                    string working = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    hModule = LoadLibrary(Path.Combine(working, "gcdapi.dll"));

                    if (hModule == IntPtr.Zero)
                    {
                        return false;
                    }
                    _Load = GetFunction<GCDAPI_Load>(hModule, "gcdapi_Load");
                    _Unload = GetFunction<GCDAPI_Unload>(hModule, "gcdapi_Unload");
                    _IsConnected = GetFunction<GCAPI_IsConnected>(hModule, "gcapi_IsConnected");
                    _GetFWVer = GetFunction<GCAPI_GetFWVer>(hModule, "gcapi_GetFWVer");
                    _Read = GetFunction<GCAPI_Read>(hModule, "gcapi_Read");
                    _Write = GetFunction<GCAPI_Write>(hModule, "gcapi_Write");
                    _GetTimeVal = GetFunction<GCAPI_GetTimeVal>(hModule, "gcapi_GetTimeVal");
                    _CalcPressTime = GetFunction<GCAPI_CalcPressTime>(hModule, "gcapi_CalcPressTime");
                    functionsLoaded = _Load();
                    
                }
                catch
                {
                    functionsLoaded = false;
                }
            }

            RefreshControllerTypes();

            return functionsLoaded;
        }

        public void Unload()
        {
            if (!isUnloaded)
            {
                if (functionsLoaded)
                {
                    _Unload();
                }
                UnloadDll();
                isUnloaded = true;
                functionsLoaded = false;
            }
        }

        public void RefreshControllerTypes()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while ((outputType == TitanOne.DEVICE_TYPE.None) && watch.ElapsedMilliseconds < 3000)
            {
                var report = GetReport();
                outputType = GetOutputType();
                Thread.Sleep(10);
            }
        }

        public bool SetOutputIdentifier(int identifier, int state)
        {
            outputState[identifier] = (sbyte)state;
            _Write(outputState);
            return true;
        }

        public void ClearOutputIdentifier(int state)
        {
            Array.Clear(outputState, state, outputState.Length);
            _Write(outputState);
        }

        public string[] GetKeyMap(DEVICE_TYPE device)
        {
            return (Enum.GetNames(DEVICE_MAP[device]));
        }

        private static T GetFunction<T>(IntPtr hModule, String procName)
        {
            try
            {
                return (T)(object)Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, procName), typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        private GCMAPIReport GetReport()
        {
            GCMAPIReport report = new GCMAPIReport();
            _Read(ref report);
            return report;
        }

        private DEVICE_TYPE GetOutputType()
        {
            var report = GetReport();
            return (DEVICE_TYPE)report.Console;
        }

        private void UnloadDll()
        {
            FreeLibrary(hModule);
        }
    }
}