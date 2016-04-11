﻿//******************************************************************************************************
//  VoltageDisturbanceAnalyzer.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/10/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using SOEDataProcessing.Database;
using SOEDataProcessing.DataResources;
using SOEDataProcessing.DataSets;
using GSF;
using GSF.Configuration;
using Phase = GSF.PQDIF.Logical.Phase;

namespace SOEDataProcessing.DataAnalysis
{
    public class VoltageDisturbanceAnalyzer
    {
        #region [ Members ]

        // Constants
        private const double Sqrt3 = 1.7320508075688772935274463415059D;

        // Fields
        private double m_systemFrequency;

        private Func<DataPoint, bool> m_isDisturbed;
        private Func<DataSeries, double> m_getMagnitude;
        private Func<DataPoint, DataPoint, DataPoint, DataPoint> m_getVAllPoint;
        private EventClassification m_eventType;

        private Dictionary<DataGroup, List<Disturbance>> m_disturbances;

        #endregion

        #region [ Constructors ]

        public VoltageDisturbanceAnalyzer(Func<DataPoint, bool> isDisturbed, Func<DataSeries, double> getMagnitude, Func<DataPoint, DataPoint, DataPoint, DataPoint> getVAllPoint, EventClassification eventType)
        {
            if ((object)isDisturbed == null)
                throw new ArgumentNullException("isDisturbed");

            if ((object)getMagnitude == null)
                throw new ArgumentNullException("getMagnitude");

            if ((object)getVAllPoint == null)
                throw new ArgumentNullException("getVAllPoint");

            m_isDisturbed = isDisturbed;
            m_getMagnitude = getMagnitude;
            m_getVAllPoint = getVAllPoint;
            m_eventType = eventType;
        }

        #endregion

        #region [ Properties ]

        [Setting]
        public double SystemFrequency
        {
            get
            {
                return m_systemFrequency;
            }
            set
            {
                m_systemFrequency = value;
            }
        }

        public Dictionary<DataGroup, List<Disturbance>> Disturbances
        {
            get
            {
                return m_disturbances;
            }
        }

        #endregion

        #region [ Methods ]

        public void Initialize(MeterDataSet meterDataSet, DbAdapterContainer dbAdapterContainer)
        {
            DataGroup dataGroup;
            VICycleDataGroup viCycleDataGroup;
            CycleDataResource cycleDataResource;

            ConnectionStringParser.ParseConnectionString(meterDataSet.ConnectionString, this);

            m_disturbances = new Dictionary<DataGroup, List<Disturbance>>();

            cycleDataResource = CycleDataResource.GetResource(meterDataSet, dbAdapterContainer);

            for (int i = 0; i < cycleDataResource.DataGroups.Count; i++)
            {
                dataGroup = cycleDataResource.DataGroups[i];
                viCycleDataGroup = cycleDataResource.VICycleDataGroups[i];
                DetectDisturbances(dataGroup, viCycleDataGroup);
            }
        }

        private void DetectDisturbances(DataGroup dataGroup, VICycleDataGroup viCycleDataGroup)
        {
            double nominalVoltage = dataGroup.Line.VoltageKV * 1000.0D / Sqrt3;
            double nominalVoltageReciprocal = 1.0D / nominalVoltage;
            DataSeries vAll = GetVAllSeries(viCycleDataGroup);

            List<Range<int>> phase1DisturbanceRanges = DetectDisturbanceRanges(viCycleDataGroup.VX1.RMS.Multiply(nominalVoltageReciprocal));
            List<Range<int>> phase2DisturbanceRanges = DetectDisturbanceRanges(viCycleDataGroup.VX2.RMS.Multiply(nominalVoltageReciprocal));
            List<Range<int>> phase3DisturbanceRanges = DetectDisturbanceRanges(viCycleDataGroup.VX3.RMS.Multiply(nominalVoltageReciprocal));
            List<Range<int>> allDisturbanceRanges = DetectDisturbanceRanges(vAll.Multiply(nominalVoltageReciprocal));

            List<Disturbance> disturbanceList = phase1DisturbanceRanges.Select(range => ToDisturbance(viCycleDataGroup.VX1.RMS, range, Phase.AN))
                .Concat(phase2DisturbanceRanges.Select(range => ToDisturbance(viCycleDataGroup.VX2.RMS, range, Phase.BN)))
                .Concat(phase3DisturbanceRanges.Select(range => ToDisturbance(viCycleDataGroup.VX3.RMS, range, Phase.CN)))
                .Concat(allDisturbanceRanges.Select(range => ToDisturbance(vAll, range, Phase.None)))
                .ToList();

            m_disturbances.Add(dataGroup, disturbanceList);
        }

        private DataSeries GetVAllSeries(VICycleDataGroup viCycleDataGroup)
        {
            DataSeries dataSeries = new DataSeries();

            for (int i = 0; i < viCycleDataGroup.VX1.RMS.DataPoints.Count; i++)
                dataSeries.DataPoints.Add(m_getVAllPoint(viCycleDataGroup.VX1.RMS[i], viCycleDataGroup.VX2.RMS[i], viCycleDataGroup.VX3.RMS[i]));

            return dataSeries;
        }

        private List<Range<int>> DetectDisturbanceRanges(DataSeries rms)
        {
            int samplesPerCycle;
            List<Range<int>> disturbanceRanges;
            int start;
            int end;

            // Determine the sample rate so that we can exclude
            // disturbances that are shorter than half a cycle
            samplesPerCycle = (int)Math.Round(rms.SampleRate / m_systemFrequency);

            // Initialize disturbanceRanges and the start index
            disturbanceRanges = new List<Range<int>>();
            start = 0;

            // Iterate over all data points in the RMS series
            while (start < rms.DataPoints.Count)
            {
                // If the data point at the start index was measured during
                // disturbed conditions, attempt to locate the end of the disturbance
                if (m_isDisturbed(rms[start]))
                {
                    // Initialize the end index to the data
                    // point just past the start of the disturbance
                    end = start + 1;

                    // Don't iterate beyond the end of the data series
                    while (end < rms.DataPoints.Count)
                    {
                        // If the data point at the end index was not measured during
                        // the disturbance, the end of the disturbance has been found
                        if (!m_isDisturbed(rms[end]))
                            break;

                        // Increment the end index
                        end++;
                    }

                    // If the disturbance lasted for at least half a cycle,
                    // store it in the list of disturbance ranges
                    if ((end - start) > (samplesPerCycle / 2))
                        disturbanceRanges.Add(new Range<int>(start, end - 1));

                    // Move the start index up
                    // to the end of the disturbance
                    start = end;
                }

                // Increment the start index
                start++;
            }

            return disturbanceRanges;
        }

        private Disturbance ToDisturbance(DataSeries rms, Range<int> range, Phase phase)
        {
            Disturbance disturbance = new Disturbance();

            disturbance.EventType = m_eventType;
            disturbance.Phase = phase;
            disturbance.StartIndex = range.Start;
            disturbance.EndIndex = range.End;
            disturbance.StartTime = rms[range.Start].Time;
            disturbance.EndTime = rms[range.End].Time;
            disturbance.Magnitude = m_getMagnitude(rms.ToSubSeries(range.Start, range.End));

            return disturbance;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConnectionStringParser<SettingAttribute, CategoryAttribute> ConnectionStringParser = new ConnectionStringParser<SettingAttribute, CategoryAttribute>();

        #endregion
    }
}
