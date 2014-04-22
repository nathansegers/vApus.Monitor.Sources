﻿/*
 * Copyright 2014 (c) Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 * Author(s):
 *    Dieter Vandroemme
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using vApus.Monitor.Sources.Base;
using VimApi;

namespace vApus.Monitor.Sources.ESXi {
    public class ESXiClient : BasePollingClient {
        private ESXiHelper _esxiHelper;
        private string _hostNameOrIPAddress;

        private string HostNameOrIPAddress {
            get {
                if (_hostNameOrIPAddress == null)
                    _hostNameOrIPAddress = GetParameter("Host Name or IP address").Value as string;
                return _hostNameOrIPAddress;
            }
        }

        public override bool IsConnected { get { return _esxiHelper != null && _esxiHelper.IsReachable; } }

        public override int RefreshCountersInterval { get { return 20000; } }

        public override string DecimalSeparator { get { return CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator; } }

        public override string Config {
            get {
                if (base._config == null) {
                    //HostHardwareInfo hardwareInfo = _esxiHelper.GetHostHardwareInfo();
                    
                    //var sb = new StringBuilder();
                    //using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings())) {
                    //    xmlWriter.WriteStartElement("Configuration");

                    //    RecurseThroughHardwareInfo(xmlWriter, hardwareInfo.GetType(), hardwareInfo);

                    //    xmlWriter.WriteEndElement();
                    //    xmlWriter.Flush();
                    //}
                    //base._config = sb.ToString();
                }
                return base._config;
            }
        }

        private void RecurseThroughHardwareInfo(XmlWriter writer, Type type, object obj) {
            //get alle the properties for this type
            PropertyInfo[] infos = type.GetProperties();

            if (infos.Length == 0)
                writer.WriteString(obj.ToString());

            foreach (PropertyInfo p in infos)
                if (p.PropertyType.IsClass && !p.PropertyType.IsValueType && !p.PropertyType.IsPrimitive && p.PropertyType.FullName != "System.String") {
                    //if the property does not have get method we don't even bother to watch, nor when its a DynamicProperty
                    if (p.GetGetMethod() != null && obj != null && p.PropertyType.Name != "DynamicProperty[]" && p.Name != "SyncRoot") {
                        //we need to check if the propertyValue is an array
                        object propertyValue = p.GetValue(obj, null);
                        if (propertyValue != null && propertyValue.GetType().IsArray) {
                            //open element (new package)
                            //s to indicate it's a collection
                            //we cant place this line in the beginning of the function because if this block not gets executed (but the else block) we need to know something before we're writing the starting element
                            writer.WriteStartElement(p.Name + "s"); 

                            //recurse throught the array
                            int index = 1;
                            Array arr = propertyValue as Array;
                            foreach (var arrElement in arr) {
                                //for each element in the array its own package
                                writer.WriteStartElement(p.Name + index++);
                                RecurseThroughHardwareInfo(writer, arrElement.GetType(), arrElement);
                                writer.WriteEndElement();
                            }
                        } else {
                            object objectToInvoke = p.GetGetMethod().Invoke(obj, null);

                            //if null the property of the HardwareInfo was null and we cant retrieve an instance of this back so we have to discard this
                            if (objectToInvoke == null) return;

                            //open element (new package)
                            writer.WriteStartElement(p.Name); //we can't place this line in the beginning of the function because we have to know if the object is not null

                            RecurseThroughHardwareInfo(writer, p.PropertyType, objectToInvoke);
                        }

                        //write the end element (to package things)
                        writer.WriteEndElement();
                    }
                } else if (obj != null && p.Name != "dynamicType" && p.Name != "dynamicProperty" && p.DeclaringType.FullName != "System.Array" && p.GetValue(obj, null) != null) {
                    try {
                        //dead end, just write the value. We're never interested in dynamicType and dynamicProperty
                        //declaring type because we don't want any properties from its array-origin
                        writer.WriteElementString(p.Name, p.GetValue(obj, null).ToString());
                    } catch (Exception ex) {
                        //sometimes nullreference exception
                        Debug.WriteLine("function recurseThroughHardwareInfo:" + ex.Message + " " + ex.StackTrace);
                    }
                }
        }


        public override Entities WDYH {
            get {
                if (base._wdyh == null) {
                    base._wdyh = new Entities();

                   


                }
                return base._wdyh;
            }
        }

        public ESXiClient() {
            var hostNameOrIPAddress = new Parameter() { Name = "Host Name or IP address", Description = "Where the monitor source is bound to.", DefaultValue = string.Empty };
            var username = new Parameter() { Name = "Username", DefaultValue = string.Empty };
            var password = new Parameter() { Name = "Password", DefaultValue = string.Empty, Encrypted = true };
            base._parameters = new Parameter[] { hostNameOrIPAddress, username, password };
        }

        public override bool Connect() {
            bool isConnected = IsConnected;
            if (!isConnected) {
                string username = GetParameter("Username").Value as string;
                string password = GetParameter("Password").Value as string;

                _esxiHelper = new ESXiHelper(HostNameOrIPAddress, username, password);
                isConnected = IsConnected;
            }
            return isConnected;
        }

        protected override Entities PollCounters() {
            throw new NotImplementedException();
        }

        public override bool Disconnect() {
            if (IsConnected) {
                _esxiHelper.Dispose();
                _esxiHelper = null;
                Stop();
            }
            return !IsConnected;
        }
    }
}
