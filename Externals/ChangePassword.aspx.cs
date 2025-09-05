using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Text;
using System.Web.UI.WebControls;
using IP.Classess;

namespace PlusCP.Externals
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        { 
            if(!Page.IsPostBack)
            {
                //string signId = "mohsin";

                string id = Request.QueryString["id"].ToString().Trim();
                string signId = BasicEncrypt.Instance.Decrypt(id); 
                lblsignid.Text = signId;

            }

        }
    }
}