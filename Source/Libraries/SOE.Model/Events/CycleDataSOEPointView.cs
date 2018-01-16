﻿//******************************************************************************************************
//  CycleDataSOEPointView.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  08/29/2017 - Billy Ernest
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using GSF.Data.Model;

namespace SOE.Model
{
    [TableName("CycleDataSOEPointView")]
    public class CycleDataSOEPointView
    {
        [PrimaryKey(true)]
        public int ID { get; set; }

        public string Name { get; set; }

        public int MeterID { get; set; }

        public DateTime Timestamp { get; set; }

        public float Vmin { get; set; }

        public float Vmax { get; set; }

        public float Imax { get; set; }

        public string StatusElement { get; set; }

        public string BreakerElementA { get; set; }

        public string BreakerElementB { get; set; }

        public string BreakerElementC { get; set; }

        public string UpState { get; set; }

        public string DownState { get; set; }

        public string Phasing { get; set; }

        public int IncidentID { get; set; }

        public int ParentID { get; set; }

        public int EventID { get; set; }
    }
}