using GSF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace openEAS.Configuration
{
    public class OpenEASSettings
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
