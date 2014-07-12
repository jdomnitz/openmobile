using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions;
using OpenMobile.Graphics;
using OpenMobile.Data;

namespace OMDSSports
{
    public class OMDSSports : BasePluginCode, IDataSource
    {
        
        public OMDSSports()
            : base("OMDSSports", imageItem.NONE, 0.1f, "DataSource provider for sports information/data", "Peter Yeaney", "peter.yeaney@outlook.com")
        {

        }

        public override eLoadStatus initialize(IPluginHost host)
        {

            DSProviders.DSNFL.CreateCommands();
            DSProviders.DSNFL.CreateDataSources();


            return eLoadStatus.LoadSuccessful;
        }



    }
}
