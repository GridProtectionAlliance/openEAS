using GSF;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;

namespace openEAS.Configuration
{
    public class openEASSettings
    {
        private Dictionary<string, string> m_dependentAssemblyLookup = new Dictionary<string, string>();

        [Setting]
        [DefaultValue("")]
        public string DependentAssemblies
        {
            get
            {
                return m_dependentAssemblyLookup.JoinKeyValuePairs();
            }
            set
            {
                m_dependentAssemblyLookup = value.ParseKeyValuePairs();
            }
        }

        public Dictionary<string, string> DependentAssemblyLookup
        {
            get
            {
                return m_dependentAssemblyLookup;
            }
        }

    }
}
