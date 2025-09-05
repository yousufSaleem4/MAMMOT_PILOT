using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PlusCP.Externals
{
    public partial class EmailVerification : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            
            if (!IsPostBack)
            {
                string url = "";
                string DeployMode = WebConfigurationManager.AppSettings["DEPLOYMODE"];
                DataTable dtURL = new DataTable();
                dtURL = cCommon.GetEmailURL(DeployMode, "Login");

                if (DeployMode == "PROD") {
                    url = dtURL.Rows[0]["URL"].ToString();
                }
                else
                {
                    url = dtURL.Rows[0]["URL"].ToString();
                }
                    
                    

                // Set the dynamic URL to the HyperLink control
                //dynamicLink.NavigateUrl = url;
            }
        }

      
    }
}