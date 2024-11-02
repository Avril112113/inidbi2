using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using IniParser;
using IniParser.Model;
using System.Reflection;
using System.Formats.Asn1;

namespace inidbi2
{
    public class inidbi2
    {
#if WIN64
        [UnmanagedCallersOnly(EntryPoint = "RVExtension", CallConvs = [typeof(CallConvStdcall)])]
#else
        // Untested
        [UnmanagedCallersOnly(EntryPoint = "_RVExtension@12", CallConvs = [typeof(CallConvStdcall)])]
#endif
        public unsafe static void RVExtension(byte* _output, uint outputSize, char* _function)
        {
            string function = Marshal.PtrToStringAnsi((IntPtr)_function);

            if (_instance == null)
                _instance = new inidbi2();

            string ret = _instance.Invoke(function);

            var len = Math.Min(ret.Length, outputSize);
            var bytes = Encoding.ASCII.GetBytes(ret);
            for (int i = 0; i < len; i++) {
                _output[i] = bytes[i];
            }
            _output[len] = 0;
            return;
        }

        public static string DebugRv(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string function)
        {
            if (_instance == null)
                _instance = new inidbi2();

            string ret = _instance.Invoke(function);
            return ret;
        }

        static inidbi2 _instance;
        static string[] stringSeparators = ["|"];
        FileIniDataParser parser = new();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, uint nSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(IntPtr lpModuleName);

        public static string GetDllPath()
        {
            StringBuilder path = new StringBuilder(260); // MAX_PATH in Windows
            IntPtr handle = GetModuleHandle(IntPtr.Zero); // NULL gets the current module
            GetModuleFileName(handle, path, (uint)path.Capacity);
            return path.ToString();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Dl_info
        {
            public IntPtr dli_fname;
            public IntPtr dli_fbase;
            public IntPtr dli_sname;
            public IntPtr dli_saddr;
        }

        [DllImport("libdl.so.2")]
        private static extern int dladdr(IntPtr addr, ref Dl_info info);

        public static string GetSoPath()
        {
            Dl_info info = new Dl_info();
            dladdr(Addr(), ref info);
            return Marshal.PtrToStringAnsi(info.dli_fname);
        }

        public static unsafe IntPtr Addr()
        {
            return (IntPtr)(delegate*<IntPtr>)&Addr;
        }


        public string Invoke(string parameters)
        {
            string[] lines = parameters.Split(stringSeparators, StringSplitOptions.None);

            string function = lines[0];
            string result = "";

            string mypath;
            if (OperatingSystem.IsWindows()) {
                mypath = Path.Join(Path.GetDirectoryName(GetDllPath()), "db");
            } else {
                mypath = Path.Join(Path.GetDirectoryName(GetSoPath()), "db");
            }
            //Console.WriteLine($"DB Path: {mypath}");

            switch (function) {
                case "version":
                    result = this.Version();
                    break;
                case "write":
                    result = this.Write(Path.Join(mypath, lines[1]), lines[2], lines[3], lines[4]);
                    break;
                case "read":
                    result = this.Read(Path.Join(mypath, lines[1]), lines[2], lines[3]);
                    break;
                case "deletesection":
                    result = this.DeleteSection(Path.Join(mypath, lines[1]), lines[2]);
                    break;
                case "deletekey":
                    result = this.DeleteKey(Path.Join(mypath, lines[1]), lines[2], lines[3]);
                    break;
                case "delete":
                    result = this.Delete(Path.Join(mypath, lines[1]));
                    break;
                case "exists":
                    result = this.Exists(Path.Join(mypath, lines[1]));
                    break;
                case "gettimestamp":
                    result = this.GetTimeStamp();
                    break;
                case "decodebase64":
                    result = this.DecodeBase64(lines[1]);
                    break;
                case "encodebase64":
                    result = this.EncodeBase64(lines[1]);
                    break;
                case "setseparator":
                    result = SetSeparator(lines[1]);
                    break;
                case "getseparator":
                    result = GetSeparator();
                    break;
                case "getsections":
                    result = GetSections(Path.Join(mypath, lines[1]));
                    break;
                case "getkeys":
                    result = GetKeys(Path.Join(mypath, lines[1]), lines[2]);
                    break;
                default:
                    break;
            }
            return result;
        }

        public static string SetSeparator(string separator)
        {
            stringSeparators[0] = "|" + separator;
            return stringSeparators[0];
        }

        public static string GetSeparator()
        {
            return stringSeparators[0];
        }

        public string Version()
        {
            string version = "2.06";
            return version;
        }

        public string Delete(string File)
        {
            string result = "true";
            try {
                if (!System.IO.File.Exists(File)) {
                    throw new Exception("File doesn't exist");
                }
                System.IO.File.Delete(File);
            } catch {
                return "false";
            }
            return result;
        }

        public string Exists(string File)
        {
            return System.IO.File.Exists(File).ToString();
        }

        public string Write(string File, string Section, string Key, string Value)
        {
            try {
                IniData data = System.IO.File.Exists(File) ? parser.ReadFile(File) : new IniData();
                if (!data.Sections.ContainsSection(Section))
                    data.Sections.AddSection(Section);
                data[Section][Key] = Value;
                parser.WriteFile(File, data);
                return "true";
            } catch (Exception e) {
                Console.Error.WriteLine(e.ToString());
                return "false";
            }
        }

        public string Read(string File, string Section, string Key)
        {
            try {
                IniData data = System.IO.File.Exists(File) ? parser.ReadFile(File) : new IniData();
                if (!data.Sections.ContainsSection(Section) || !data[Section].ContainsKey(Key))
                    return "[false, \"\"]";
                var s = data[Section][Key];
                // This might add compat with old Linux port?
                if (s.StartsWith('"') && s.EndsWith('"'))
                    s = s[1..^1];
                return "[true, " + s + "]";
            } catch (Exception e) {
                Console.Error.WriteLine(e.ToString());
                return "[false, \"\"]";
            }
        }

        public string DeleteSection(string File, string Section)
        {
            try {
                IniData data = System.IO.File.Exists(File) ? parser.ReadFile(File) : new IniData();
                bool ok = data.Sections.RemoveSection(Section);
                if (ok)
                    parser.WriteFile(File, data);
                return ok ? "true" : "false";
            } catch (Exception e) {
                Console.Error.WriteLine(e.ToString());
                return "false";
            }
        }


        public string DeleteKey(string File, string Section, string key)
        {
            try {
                IniData data = System.IO.File.Exists(File) ? parser.ReadFile(File) : new IniData();
                if (!data.Sections.ContainsSection(Section))
                    return "false";
                bool ok = data[Section].RemoveKey(Section);
                if (ok)
                    parser.WriteFile(File, data);
                return ok ? "true" : "false";
            } catch (Exception e) {
                Console.Error.WriteLine(e.ToString());
                return "false";
            }
        }

        public string GetSections(string File)
        {
            try {
                IniData data = System.IO.File.Exists(File) ? parser.ReadFile(File) : new IniData();
                StringBuilder sb = new();
                sb.Append('[');
                foreach (var sec in data.Sections) {
                    sb.Append('"');
                    sb.Append(sec.SectionName);
                    sb.Append("\",");
                }
                sb.Append(']');
                return sb.ToString();
            } catch (Exception e) {
                Console.Error.WriteLine(e.ToString());
                return "[]";
            }
        }

        public string GetKeys(string File, string section)
        {
            try {
                IniData data = System.IO.File.Exists(File) ? parser.ReadFile(File) : new IniData();
                if (!data.Sections.ContainsSection(section))
                    return "[]";
                StringBuilder sb = new();
                sb.Append('[');
                foreach (var key in data[section]) {
                    sb.Append('"');
                    sb.Append(key.KeyName);
                    sb.Append("\",");
                }
                sb.Append(']');
                return sb.ToString();
            } catch (Exception e) {
                Console.Error.WriteLine(e.ToString());
                return "[]";
            }
        }

        public string GetTimeStamp()
        {
            string ret = string.Format("[{0:yyyy,MM,dd,HH,mm,ss}]", DateTime.UtcNow);
            return ret;
        }

        public string EncodeBase64(string plainText)
        {
            string ret = "";
            try {
                var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                ret = Convert.ToBase64String(plainTextBytes);
            } catch {
                return ret;
            }
            return ret;
        }

        public string DecodeBase64(string base64EncodedData)
        {
            string ret = "";
            try {
                var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
                ret = Encoding.UTF8.GetString(base64EncodedBytes);
            } catch {
                return ret;
            }
            return ret;
        }
    }
}

