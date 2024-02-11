using System;

namespace FreePIE.Core.ScriptEngine
{
    public interface IScriptEngine
    {
        void Start(string script, string scriptPath = null, string profile = null);
        void Stop();
    }
}
