using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile;
using OpenMobile.Framework;
using OpenMobile.Controls;
using OpenMobile.Plugin;
using OpenMobile.Graphics;
using OpenMobile.Data;

namespace OMNetworking
{
    public class OMNetworking : HighLevelCode
    {

        public OMNetworking()
            : base("OMNetworking", imageItem.NONE, 1.0F, "WiFi Management", "Networking/WiFi", "Peter Yeaney", "peter.yeaney@outlook.com")
        {
        }

        public override eLoadStatus initialize(IPluginHost host)
        {
            OMPanel networkingPanel = new OMPanel("networkingPanel", "Networking");

            OMBasicShape networkListBackground = new OMBasicShape("networkListBackground", (OM.Host.ClientArea_Init.Width / 2) - (int)(OM.Host.ClientArea_Init.Width * .25), 80, (int)(OM.Host.ClientArea_Init.Width * .5), 350, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 10));
            networkingPanel.addControl(networkListBackground);

            OMList networkList = new OMList("networkList", networkListBackground.Left + 7, networkListBackground.Top + 7, networkListBackground.Width - 14, networkListBackground.Height - 14);
            networkList.ItemColor1 = Color.Transparent;
            networkList.FontSize = 15;
            //networkList[0].subitemFormat.font = new Font(networkList.Font);
            networkList.SeparatorSize = 2;
            networkList.SeparatorColor = Color.Silver;
            networkList.ListStyle = eListStyle.MultiList;
            networkList.OnClick += new userInteraction(networkList_OnClick);
            networkingPanel.addControl(networkList);

            OMButton wifiRefresh = OMButton.PreConfigLayout_BasicStyle("wifiRefresh", networkList.Left  + (networkList.Width / 4), networkList.Top + networkList.Height + 12, (networkList.Width / 4) * 2, 80, GraphicCorners.All);
            wifiRefresh.Text = "Scan";
            networkingPanel.addControl(wifiRefresh);

            //WinWifi


            //base.PanelManager.loadPanel(networkingPanel, true);
            base.PanelManager.loadSharedPanel(networkingPanel, true);

            OM.Host.DataHandler.SubscribeToDataSource("WinWifi;Networking.WiFi.StatusChange", Subscription_Updated);
            OM.Host.DataHandler.SubscribeToDataSource("WinWifi;Networking.WiFi.Networks", Subscription_Updated);

            return eLoadStatus.LoadSuccessful;    
        }

        private void Subscription_Updated(DataSource sensor)
        {
            if (sensor.Value == null)
                return;
            OMList networkList = (OMList)base.PanelManager[0, "networkingPanel"]["networkList"];

            if (sensor.NameLevel3 == "StatusChange")
            {
                string[] sensorSplit = sensor.Value.ToString().Split('.');
                for (int i = 0; i < networkList.Count; i++)
                {
                    if (networkList[i].text.Contains(sensorSplit[0]))
                    {
                        if (sensorSplit[1] == "Connecting")
                        {
                            if (!networkList[i].subItem.Contains("( Connecting )" + " - " + sensorSplit[2]))
                                networkList[i].subItem = "( Connecting )";
                        }
                        else if (sensorSplit[1] == "Connected")
                        {
                            if (!networkList[i].subItem.Contains("( Click To Disconnect )" + " - " + sensorSplit[2]))
                                networkList[i].subItem = "( Click To Disconnect )" + " - " + sensorSplit[2];
                        }
                        else if (sensorSplit[1] == "NotConnected")
                        {
                            if (!networkList[i].subItem.Contains("( Connecting )" + " - " + sensorSplit[2]))
                                networkList[i].subItem = "( Connecting )" + " - " + sensorSplit[2];
                        }
                        else if (sensorSplit[1] == "Disonnected")
                        {
                            if (!networkList[i].subItem.Contains("( Click To Connect )" + " - " + sensorSplit[2]))
                                networkList[i].subItem = "( Click To Connect )" + " - " + sensorSplit[2];
                        }
                    }
                }
            }
            else
            {
                List<Dictionary<string, object>> results = (List<Dictionary<string, object>>)sensor.Value;
                for (int i = 0; i < results.Count; i++)
                {
                    if (networkList.Items.Count <= i)
                    {
                        //add
                        networkList.Add(new OMListItem(String.Format("{0} ( {1} )", results[i]["NetworkName"].ToString(), results[i]["ConnectionType"].ToString()), "", ((imageItem)results[i]["SignalImage"]).image, new OMListItem.subItemFormat(), results[i]["UID"]));
                        networkList[networkList.Count - 1].subitemFormat.font = new Font(networkList.Font);

                    }
                    else
                    {
                        //change
                        networkList[i].text = String.Format("{0} ( {1} )", results[i]["NetworkName"].ToString(), results[i]["ConnectionType"].ToString());
                        networkList[i].image = ((imageItem)results[i]["SignalImage"]).image;
                        networkList[i].tag = results[i]["UID"];
                    }
                    if (results[i]["IsConnected"].ToString() == "True")
                    {
                        if (!networkList[i].subItem.Contains("( Click To Disconnect )" + " - " + results[i]["InterfaceConnectedName"]))
                            networkList[i].subItem = "( Click To Disconnect )" + " - " + results[i]["InterfaceConnectedName"];
                    }
                    else
                    {
                        if (!networkList[i].subItem.Contains("( Click To Connect )"))
                            networkList[i].subItem = "( Click To Connect )";
                    }
                }
                if (networkList.Items.Count > results.Count)
                {
                    //remove the rest
                    for (int i = results.Count; i < networkList.Count; i++)
                    {
                        networkList.Items.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        void networkList_OnClick(OMControl sender, int screen)
        {
            OMList networkList = (OMList)sender;
            if (networkList.SelectedIndex < 0)
                return;
            if (networkList.SelectedItem.subItem.Contains("( Click To Connect )"))
            {
                //connect to this item
                OM.Host.CommandHandler.ExecuteCommand("Networking.WiFi.Connect", new object[] { screen, networkList.SelectedItem.tag.ToString() });
            }
            else
            {
                //disconnect from this item
                OM.Host.CommandHandler.ExecuteCommand("Networking.WiFi.Disconnect", new object[] { networkList.SelectedItem.tag.ToString() });
            }
        }
    }
}
