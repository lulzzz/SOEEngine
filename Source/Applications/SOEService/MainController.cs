﻿//******************************************************************************************************
//  WebApi.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/08/2018 - Billy Ernest
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.Security;
using GSF.Web.Hosting;
using GSF.Web.Model;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json.Linq;
using SOE.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ValidateAntiForgeryToken = System.Web.Mvc.ValidateAntiForgeryTokenAttribute;

namespace SOEService
{
    public class MainController: ApiController
    {
        #region [ GET Operations ]

        /// <summary>
        /// Common page request handler.
        /// </summary>
        /// <param name="pageName">Page name to render.</param>
        /// <param name="cancellationToken">Propagates notification from client that operations should be canceled.</param>
        /// <returns>Rendered page result for given page.</returns>
        [HttpGet]
        public Task<HttpResponseMessage> GetPage(CancellationToken cancellationToken)
        {
            return WebServer.Default.RenderResponse(Request, "Summary.cshtml", false, cancellationToken, Program.Host.Model, typeof(AppModel));
        }

        /// <summary>
        /// Return record count
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetRecordCount(string modelName)
        {

            // Proxy all other requests
            SecurityPrincipal securityPrincipal = RequestContext.Principal as SecurityPrincipal;

            if ((object)securityPrincipal == null || (object)securityPrincipal.Identity == null || !securityPrincipal.IsInRole("Viewer,Administrator"))
                return BadRequest($"User \"{RequestContext.Principal?.Identity.Name}\" is unauthorized.");

            object record;

            using (DataContext dataContext = new DataContext("systemSettings"))
            {
                try
                {
                    Type type = typeof(Meter).Assembly.GetType("SOE.Model." + modelName);
                    record = dataContext.Table(type).QueryRecordCount();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.ToString());
                }
            }

            return Ok(record);
        }


        /// <summary>
        /// Return single Record
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetRecord(int id, string modelName)
        {
            // Proxy all other requests
            SecurityPrincipal securityPrincipal = RequestContext.Principal as SecurityPrincipal;

            if ((object)securityPrincipal == null || (object)securityPrincipal.Identity == null || !securityPrincipal.IsInRole("Viewer,Administrator"))
                return BadRequest($"User \"{RequestContext.Principal?.Identity.Name}\" is unauthorized.");


            object record;

            using (DataContext dataContext = new DataContext("systemSettings"))
            {
                try
                {
                    Type type = typeof(Meter).Assembly.GetType("SOE.Model." + modelName);
                    record = dataContext.Table(type).QueryRecordWhere("ID = {0}", id);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.ToString());
                }
            }

            return Ok(record);
        }

        /// <summary>
        /// Return single Record
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetRecordsWhere(string id, string modelName)
        {

            // Proxy all other requests
            SecurityPrincipal securityPrincipal = RequestContext.Principal as SecurityPrincipal;

            if ((object)securityPrincipal == null || (object)securityPrincipal.Identity == null || !securityPrincipal.IsInRole("Viewer,Administrator"))
                return BadRequest($"User \"{RequestContext.Principal?.Identity.Name}\" is unauthorized.");


            object record;

            using (DataContext dataContext = new DataContext("systemSettings"))
            {
                try
                {
                    Type type = typeof(Meter).Assembly.GetType("SOE.Model." + modelName);
                    record = dataContext.Table(type).QueryRecordsWhere(id);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.ToString());
                }
            }

            return Ok(record);
        }

        /// <summary>
        /// Returns multiple records
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public IHttpActionResult GetRecords(string id, string modelName)
        {

            // Proxy all other requests
            SecurityPrincipal securityPrincipal = RequestContext.Principal as SecurityPrincipal;

            if ((object)securityPrincipal == null || (object)securityPrincipal.Identity == null || !securityPrincipal.IsInRole("Viewer,Administrator"))
                return BadRequest($"User \"{RequestContext.Principal?.Identity.Name}\" is unauthorized.");


            object record;

            string idList = "";

            try
            {
                if (id != "all")
                {
                    string[] ids = id.Split(',');

                    if (ids.Count() > 0)
                        idList = $"ID IN ({ string.Join(",", ids.Select(x => int.Parse(x)))})";
                }
            }
            catch (Exception)
            {
                return BadRequest("The id field must be a comma separated integer list.");
            }

            using (DataContext dataContext = new DataContext("systemSettings"))
            {
                try
                {
                    Type type = typeof(Meter).Assembly.GetType("SOE.Model." + modelName);

                    if (idList.Length == 0)
                        record = dataContext.Table(type).QueryRecords();
                    else
                        record = dataContext.Table(type).QueryRecordsWhere(idList);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.ToString());
                }
            }

            return Ok(record);
        }

        #endregion

        #region [ PUT Operations ]

        [HttpPut]
        [ValidateAntiForgeryToken]
        public IHttpActionResult UpdateRecord(string modelName, [FromBody]JObject record)
        {
            // Proxy all other requests
            SecurityPrincipal securityPrincipal = RequestContext.Principal as SecurityPrincipal;

            if ((object)securityPrincipal == null || (object)securityPrincipal.Identity == null || !securityPrincipal.IsInRole("Administrator"))
                return BadRequest($"User \"{RequestContext.Principal?.Identity.Name}\" is unauthorized.");


            using (DataContext dataContext = new DataContext("systemSettings"))
            {
                try
                {
                    Type type = typeof(Meter).Assembly.GetType("SOE.Model." + modelName);
                    object obj = record.ToObject(type);
                    dataContext.Table(typeof(Meter).Assembly.GetType("SOE.Model." + modelName)).UpdateRecord(obj);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.ToString());
                }
            }

            return Ok();
        }

        #endregion

        #region [ POST Operations ]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IHttpActionResult CreateRecord(string modelName, [FromBody]JObject record)
        {

            // Proxy all other requests
            SecurityPrincipal securityPrincipal = RequestContext.Principal as SecurityPrincipal;

            if ((object)securityPrincipal == null || (object)securityPrincipal.Identity == null || !securityPrincipal.IsInRole("Administrator"))
                return BadRequest($"User \"{RequestContext.Principal?.Identity.Name}\" is unauthorized.");


            using (DataContext dataContext = new DataContext("systemSettings"))
            {
                try
                {
                    Type type = typeof(Meter).Assembly.GetType("SOE.Model." + modelName);
                    object obj = record.ToObject(type);

                    dataContext.Table(typeof(Meter).Assembly.GetType("SOE.Model." + modelName)).AddNewRecord(obj);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.ToString());
                }
            }

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IHttpActionResult GetView(string modelName, [FromBody]JObject record)
        {
            int numBuckets;
            string timeContext;
            DateTime startDate;
            DateTime endDate;
            string limits;
            string levels;
            string circuitName;
            string systemName;
            //var dates;
            // Proxy all other requests
            SecurityPrincipal securityPrincipal = RequestContext.Principal as SecurityPrincipal;

            if ((object)securityPrincipal == null || (object)securityPrincipal.Identity == null || !securityPrincipal.IsInRole("Viewer,Administrator"))
                return BadRequest($"User \"{RequestContext.Principal?.Identity.Name}\" is unauthorized.");
            try {

                numBuckets = record["numBuckets"]?.Value<int>() ?? 20;
                timeContext = record["timeContext"]?.Value<string>() ?? "Days";
                startDate = record["date"]?.Value<DateTime>() ?? DateTime.Now.AddDays(-20);
                endDate = (DateTime)typeof(DateTime).GetMethod("Add" + timeContext).Invoke(startDate, new object[] { numBuckets });
                limits = record["limits"]?.Value<string>() ?? "";
                levels = record["levels"]?.Value<string>() ?? "Circuit";
                circuitName = record["circuitName"]?.Value<string>();
                systemName = record["systemName"]?.Value<string>();
            }
            catch (Exception ex) {
                return BadRequest($"{ex.ToString()}");
            }

            string groupByString;
            string dates;
            string sumString;
            string sumDates;

            if (timeContext == "Months") {
                dates = string.Join(",", Enumerable.Range(0, 1 + numBuckets).Select(offset => (startDate).AddMonths(offset)).Select(x => "[" + x.Date.ToString("M/yyyy") + "]"));
                sumDates = string.Join(",", Enumerable.Range(0, 1 + numBuckets).Select(offset => (startDate).AddMonths(offset)).Select(x => "SUM([" + x.Date.ToString("M/yyyy") + "]) as [" + x.Date.ToString("M/yyyy") + "]"));
                groupByString = "cast(datepart(Month, IncidentQuery.StartTime) as varchar(max)) + '/' + cast(datepart(year,IncidentQuery.StartTime) as varchar(max))";
                sumString = string.Join("+", Enumerable.Range(0, 1 + numBuckets).Select(offset => (startDate).AddMonths(offset)).Select(x => "COALESCE([" + x.Date.ToString("M/yyyy") + "],0)"));
            }
            else if (timeContext == "Days") {
                dates = string.Join(",", Enumerable.Range(0, 1 + numBuckets).Select(offset => (startDate).AddDays(offset)).Select(x => "[" + x.Date.ToString("MM/dd/yyyy") + "]"));
                sumDates = string.Join(",", Enumerable.Range(0, 1 + numBuckets).Select(offset => (startDate).AddDays(offset)).Select(x => "SUM([" + x.Date.ToString("MM/dd/yyyy") + "]) as [" + x.Date.ToString("MM/dd/yyyy") + "]"));
                groupByString = "Cast(IncidentQuery.StartTime as date)";
                sumString = string.Join("+", Enumerable.Range(0, 1 + numBuckets).Select(offset => (startDate).AddDays(offset)).Select(x => "COALESCE([" + x.Date.ToString("MM/dd/yyyy") + "],0)"));
            }
            else {
                dates = string.Join(",", Enumerable.Range(0, 1 + numBuckets).Select(offset => (startDate).AddHours(offset)).Select(x => "[" + x.ToString("M/dd H:00") + "]"));
                sumDates = string.Join(",", Enumerable.Range(0, 1 + numBuckets).Select(offset => (startDate).AddHours(offset)).Select(x => "SUM([" + x.ToString("M/dd H:00") + "]) as [" + x.ToString("M/dd H:00") + "]"));
                groupByString = "cast(datepart(Month, IncidentQuery.StartTime) as varchar(max)) + '/' + cast(datepart(day, IncidentQuery.StartTime) as varchar(max)) + ' '+ cast(datepart(HOUR,IncidentQuery.StartTime) as varchar(max)) + ':00'";
                sumString = string.Join("+", Enumerable.Range(0, 1 + numBuckets).Select(offset => (startDate).AddHours(offset)).Select(x => "COALESCE([" + x.ToString("M/dd H:00") + "],0)"));
            }

            using (AdoDataConnection conn = new AdoDataConnection("systemSettings"))
            {
                try
                {
                    string s = $"SELECT {(limits.ToUpper() != "ALL" ? limits : "")} SystemName as System, {(levels.ToUpper() == "SYSTEM" ? "COUNT(DISTINCT CircuitName)" : "CircuitName")} as Circuit, {(levels.ToUpper() == "SYSTEM" ? "COUNT(MeterName)" : "")}{(levels.ToUpper() == "CIRCUIT" ? "COUNT(DISTINCT MeterName)" : "")}{(levels.ToUpper() == "DEVICE" ? "MeterName" : "")} as Device, {sumDates}, SUM({sumString}) as Total, SUM(FileCount) as [CT Files], SUM(SOECount) as SOE " +
                                "FROM ( " +
                                   $"SELECT System.Name as SystemName, Circuit.Name as CircuitName, {(levels.ToUpper() == "SYSTEM" ? "COUNT(DISTINCT Meter.Name)": "Meter.Name")} as MeterName, COUNT(*) as Count, {groupByString} as date, SUM(IncidentQuery.FileCount) as FileCount, SUM(SOECount) as SOECount " +
                                    "FROM " +
                                        "( " +
                                           "SELECT Incident.Id, Incident.StartTime, Incident.MeterID, Count(EventQuery.FileGroupID) as FileCount, SUM(SOECount) as SOECount " +
                                           "FROM " +
                                                "Incident Join " +
                                                "( " +
                                                    "SELECT Event.ID, Event.FileGroupID, Event.IncidentID, Count(SOEPoint.ID) as SOECount " +
                                                    "FROM Event JOIN " +
                                                         "CycleData ON CycleData.EventID = event.ID JOIN " +
                                                         "SOEPoint ON SOEPoint.CycleDataID = CycleData.ID " +
                                                    "Group By Event.ID, Event.FileGroupID, Event.IncidentID " +
                                                ") as EventQuery On Incident.ID = EventQuery.IncidentID " +
                                          $"Where Incident.StartTime BETWEEN '{startDate}' AND '{endDate}' " +
                                           "GROUP BY Incident.Id, Incident.StartTime, Incident.MeterID " +
                                        ") AS IncidentQuery Join " +
                                        "Meter ON Meter.ID = incidentquery.MeterID Join " +
                                        "Circuit ON Circuit.ID = Meter.CircuitNormalID JOIN " +
                                        "System ON System.ID = Circuit.SystemID " +
                                    (circuitName != null ? $"WHERE Circuit.Name LIKE '{circuitName}'" : "") +
                                    (systemName != null ? $"WHERE System.Name LIKE '{systemName}'" : "") +
                                    "GROUP BY " +
                                        $"System.Name, Circuit.Name, Meter.Name, {groupByString} " +

                                " ) as t " +
                                " PIVOT(SUM(count) " +
                               $"       FOR Date IN ({dates})) AS Pivoted " +
                               $"Group By SystemName{(levels.ToUpper() != "SYSTEM" ? ", CircuitName" : "")}{(levels.ToUpper() == "DEVICE" ? ", MeterName" : "")}";

                    DataTable table = conn.RetrieveData(s);
                    return Ok(table);

                }
                catch (Exception ex)
                {
                    return BadRequest(ex.ToString());
                }
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IHttpActionResult GetIncidentGroups(string modelName, [FromBody]JObject record)
        {
            int circuitID;
            DateTime startTime;
            DateTime endTime;
            DataTable table;

            // Proxy all other requests
            SecurityPrincipal securityPrincipal = RequestContext.Principal as SecurityPrincipal;

            if ((object)securityPrincipal == null || (object)securityPrincipal.Identity == null || !securityPrincipal.IsInRole("Viewer,Administrator"))
                return BadRequest($"User \"{RequestContext.Principal?.Identity.Name}\" is unauthorized.");
            try
            {
                circuitID = record["circuitId"]?.Value<int>() ?? 0;
                startTime = DateTime.Parse(record["startDate"].ToString());
                endTime = DateTime.Parse(record["endDate"].ToString());
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.ToString()}");
            }

            using (AdoDataConnection conn = new AdoDataConnection("systemSettings"))
            {
                try
                {
                    string s = $"select * from GetNearbyIncidentsByCircuit({circuitID},'{startTime.ToString()}', '{endTime.ToString()}', 0)";

                    table = conn.RetrieveData(s);
                    return Ok(table);

                }
                catch (Exception ex)
                {
                    return BadRequest(ex.ToString());
                }

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IHttpActionResult GetIncidentData(string modelName, [FromBody]JObject record)
        {
            int meterId;
            int circuitId;
            DateTime startTime;
            DateTime endTime;
            int pixels;
            string type;
            DataTable table;

            // Proxy all other requests
            SecurityPrincipal securityPrincipal = RequestContext.Principal as SecurityPrincipal;

            if ((object)securityPrincipal == null || (object)securityPrincipal.Identity == null || !securityPrincipal.IsInRole("Viewer,Administrator"))
                return BadRequest($"User \"{RequestContext.Principal?.Identity.Name}\" is unauthorized.");
            try
            {
                meterId = record["meterId"]?.Value<int>() ?? 0;
                circuitId = record["circuitId"]?.Value<int>() ?? 0;
                startTime = DateTime.Parse(record["startDate"].ToString());
                endTime = DateTime.Parse(record["endDate"].ToString());
                pixels = record["pixels"]?.Value<int>() ?? 0;
                type = record["type"].Value<string>();
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.ToString()}");
            }

            using (AdoDataConnection conn = new AdoDataConnection("systemSettings"))
            {
                try
                {
                    Dictionary<string, List<double[]>> dict = new Dictionary<string, List<double[]>>();

                    string s = $"select Event.ID from GetNearbyIncidentsByCircuit({circuitId},'{startTime.ToString()}', '{endTime.ToString()}', 0)as incident join event on Incident.ID = event.IncidentID where event.MeterID = {meterId}";

                    table = conn.RetrieveData(s);
                    foreach (DataRow row in table.Rows) {                        
                        Dictionary<string, List<double[]>> temp = QueryEventData(int.Parse(row["ID"].ToString()), type);
                        foreach(string key in temp.Keys)
                        {
                            if (dict.ContainsKey(key))
                            {
                                dict[key] = dict[key].Concat(temp[key]).ToList();
                            }
                            else {
                                dict.Add(key, temp[key]);
                            }
                        }
                    }

                    Dictionary<string, List<double[]>> returnDict = new Dictionary<string, List<double[]>>();
                    foreach (string key in dict.Keys)
                    {
                        returnDict.Add(key,Downsample(dict[key].OrderBy(x => x[0]).ToList(), pixels, new Range<DateTime>(startTime, endTime)));
                    }

                    return Ok(returnDict);

                }
                catch (Exception ex)
                {
                    return BadRequest(ex.ToString());
                }

            }
        }

        #endregion

        #region [ DELETE Operations ]

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public IHttpActionResult DeleteRecord(int id, string modelName)
        {

            // Proxy all other requests
            SecurityPrincipal securityPrincipal = RequestContext.Principal as SecurityPrincipal;

            if ((object)securityPrincipal == null || (object)securityPrincipal.Identity == null || !securityPrincipal.IsInRole("Administrator"))
                return BadRequest($"User \"{RequestContext.Principal?.Identity.Name}\" is unauthorized.");


            using (DataContext dataContext = new DataContext("systemSettings"))
            {
                Type type = typeof(Meter).Assembly.GetType("openXDA.Model." + modelName);
                dataContext.Table(type).DeleteRecordWhere("ID = {0}", id);
            }

            return Ok();
        }

        #endregion

        #region [ OpenSEE Table Operations ]

        private Dictionary<string, List<double[]>> QueryEventData(int eventID, string type)
        {

                const string EventDataQueryFormat =
                    "SELECT " +
                    "    EventData.TimeDomainData, " +
                    "    EventData.FrequencyDomainData " +
                    "FROM " +
                    "    Event JOIN " +
                    "    EventData ON Event.EventDataID = EventData.ID " +
                    "WHERE Event.ID = {0}";

                Dictionary<int, List<double[]>> dataLookup = new Dictionary<int, List<double[]>>();
                byte[] timeDomainData = null;

                using(AdoDataConnection connection = new AdoDataConnection("SystemSettings"))
                using (IDataReader reader = connection.ExecuteReader(EventDataQueryFormat, eventID))
                {
                    while (reader.Read())
                    {
                        timeDomainData = Decompress((byte[])reader["TimeDomainData"]);
                    }
                }


            return GetDataLookup(timeDomainData, type);
        }

        private byte[] Decompress(byte[] compressedBytes)
        {
            using (MemoryStream memoryStream = new MemoryStream(compressedBytes))
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            using (MemoryStream destinationStream = new MemoryStream())
            {
                gzipStream.CopyTo(destinationStream);
                return destinationStream.ToArray();
            }
        }

        private Dictionary<string, List<double[]>> GetDataLookup(byte[] bytes, string type)
        {
            int offset;
            int samples;
            double[] times;

            string channelName;
            List<double[]> dataSeries;
            Dictionary<string, List<double[]>> dataLookup = new Dictionary<string, List<double[]>>();

            offset = 0;
            samples = LittleEndian.ToInt32(bytes, offset);
            offset += sizeof(int);

            long epoch = new DateTime(1970, 1, 1).Ticks;

            times = new double[samples];

            for (int i = 0; i < samples; i++)
            {
                times[i] = (LittleEndian.ToInt64(bytes, offset) - epoch) / (double)TimeSpan.TicksPerMillisecond;
                offset += sizeof(long);
            }


            while (offset < bytes.Length)
            {
                dataSeries = new List<double[]>();
                channelName = GetChannelName(LittleEndian.ToInt32(bytes, offset));
                offset += sizeof(int);

                for (int i = 0; i < samples; i++)
                {
                    dataSeries.Add(new double[] { times[i], LittleEndian.ToDouble(bytes, offset) });
                    offset += sizeof(double);
                }
                
                if(channelName.Contains(type))
                    dataLookup.Add(channelName, dataSeries);
            }

            return dataLookup;
        }

        private string GetChannelName(int seriesID)
        {
            using (AdoDataConnection connection = new AdoDataConnection("SystemSettings"))
            {
                const string QueryFormat =
                    "SELECT Channel.Name " +
                    "FROM " +
                    "    Channel JOIN " +
                    "    Series ON Series.ChannelID = Channel.ID " +
                    "WHERE Series.ID = {0}";

                return connection.ExecuteScalar<string>(QueryFormat, seriesID);
            }
        }

        private List<double[]> Downsample (List<double[]> series, int sampleCount, Range<DateTime> range)
        {
            List<double[]> data = new List<double[]>();
            DateTime epoch = new DateTime(1970, 1, 1);
            double startTime = range.Start.Subtract(epoch).TotalMilliseconds;          
            int index = 0;

            for (int n = 0; n < sampleCount; n += 2)
            {
                double end = startTime + (n + 2) * range.End.Subtract(range.Start).TotalMilliseconds / sampleCount;

                double[] min = null;
                double[] max = null;

                while (index < series.Count && series[index][0] < end)
                {
                    if (min == null || min[1] > series[index][1])
                        min = series[index];

                    if (max == null || max[1] <= series[index][1])
                        max = series[index];

                    ++index;
                }

                if (min != null)
                {
                    if (min[0] < max[0])
                    {
                        data.Add(min);
                        data.Add(max);
                    }
                    else if (min[0] > max[0])
                    {
                        data.Add(max);
                        data.Add(min);
                    }
                    else
                    {
                        data.Add(min);
                    }
                }
                else {
                    if(data.Any() && data.Last() != null)
                        data.Add(null);
                }
            }

            return data;

        }
        #endregion

    }
}