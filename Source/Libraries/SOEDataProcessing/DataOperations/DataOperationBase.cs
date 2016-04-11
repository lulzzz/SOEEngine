﻿//******************************************************************************************************
//  DataOperationBase.cs - Gbtc
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
//  02/26/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using SOEDataProcessing.Database;
using SOEDataProcessing.DataSets;

namespace SOEDataProcessing.DataOperations
{
    public abstract class DataOperationBase<T> : IDataOperation where T : class, IDataSet
    {
        public abstract void Prepare(DbAdapterContainer dbAdapterContainer);
        public abstract void Execute(T dataSet);
        public abstract void Load(DbAdapterContainer dbAdapterContainer);

        public void Execute(IDataSet dataSet)
        {
            T dataSetAsT = dataSet as T;

            if ((object)dataSetAsT != null)
                Execute(dataSetAsT);
        }
    }
}