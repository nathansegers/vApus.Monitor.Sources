﻿/*
 * 2014 Sizing Servers Lab, affiliated with IT bachelor degree NMCT
 * University College of West-Flanders, Department GKG (www.sizingservers.be, www.nmct.be, www.howest.be/en)
 * 
 * Author(s):
 *    Dieter Vandroemme
 */
using vApus.Monitor.Sources.Generic.Agent;

namespace vApus.Monitor.Sources.HMT.Agent {
    /// <summary>
    /// A generic agent implementation having 5557 for a default port to connect to.
    /// </summary>
    public class HMTAgentClient : GenericAgentClient {
        public HMTAgentClient()
            : base() {
            base.GetParameter("Port").DefaultValue = 5557;
        }
    }
}
