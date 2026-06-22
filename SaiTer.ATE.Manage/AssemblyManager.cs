using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Manage
{
    public class AssemblyManager<T>
    {
        //所要加载的节点程序集
        private readonly Assembly assembly = null;
        private string _assemblyString = string.Empty;
        /// <summary>
        /// 反射类型集合
        /// </summary>
        public Dictionary<int, T> Sessions
        {
            get;
            private set;
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        public AssemblyManager(string assemblyString, Dictionary<int, string> classNames, string appendSpace)
        {
            try
            {
                _assemblyString = assemblyString;
                assembly = Assembly.Load(assemblyString);
                Sessions = CreateSessions(classNames, appendSpace);
            }
            catch(Exception ex) 
            {
                Log.Log.LogException(ex);
            }
        }
        /// <summary>
        /// 构造方法2
        /// </summary>
        /// <param name="assemblyString"></param>
        /// <param name="classNames"></param>
        /// <param name="appendSpace"></param>
        /// <param name="path"></param>
        public AssemblyManager(string assemblyString, Dictionary<int, string> classNames, string appendSpace, string path)
        {
            _assemblyString = assemblyString;
            string strPath = Environment.CurrentDirectory + path;
            assembly = Assembly.LoadFile(strPath);
            Sessions = CreateSessions(classNames, appendSpace);
        }

        private Dictionary<int, T> CreateSessions(Dictionary<int, string> classNames, string appendSpace)
        {
            try
            {
                //业务字典
                Dictionary<int, T> sessions = new Dictionary<int, T>();
                foreach (KeyValuePair<int, string> keyVal in classNames)
                {
                    //工位编号
                    int stationId = keyVal.Key;
                    //类名称
                    string className = keyVal.Value;
                    if (string.IsNullOrEmpty(className))
                        continue;
                    if (assembly == null)
                        return null;
                    string s;
                    if (!string.IsNullOrEmpty(appendSpace))
                        s = _assemblyString + "." + appendSpace + "." + className;
                    else
                        s = _assemblyString + "." + className;
                    T t = (T)assembly.CreateInstance(s, true, BindingFlags.CreateInstance, null, new object[] { keyVal.Key }, null, null);
                    //业务类保存到字典中
                    sessions[stationId] = t;
                }
                return sessions;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return null;
            }
        }
    }
}

