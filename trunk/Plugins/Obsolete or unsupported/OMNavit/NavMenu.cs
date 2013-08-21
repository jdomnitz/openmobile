using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile.Controls;

namespace OpenMobile.Plugins.OMNavit
{
    public class NavMenu : OMPanel
    {
        public NavMenu()
        {
            var button = new OMButton(50, 30)

                {
                    Text = "Return To Map"
                    
                };

            button.OnClick += OnBack;
            this.addControl(button);

        }

        private void OnBack(OMControl pSender, int pScreen)
        {

        }
    }
}

/*
 * State Machine
 * Map ->   Menu    ->  Search Address
 *                  ->  Search POI
 *                  ->
 */
