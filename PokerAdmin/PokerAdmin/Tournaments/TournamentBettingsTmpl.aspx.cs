using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Common;
using Common.Web;
using System.Xml; 
using DesktopAPIEngine; 

namespace Admin.Tournaments
{
	/// <summary>
	/// Summary description for TournamentBettings.
	/// </summary>
	public class TournamentBettingsTmpl  : Common.Web.Page
	{
		protected System.Web.UI.WebControls.DataGrid oGrid;
		protected System.Web.UI.WebControls.TextBox txtCoeff;
		protected System.Web.UI.WebControls.TextBox txtName;

        protected int BID=0;

		DataTable tbBlind=null ;
		DataView vwBlind=null;

		private void Page_Load(object sender, System.EventArgs e)
		{
			  BID=Utils.GetInt(Request["BettingID"]); 
			if (!IsPostBack)
                 Session["Blind_dtTmpl"]=null;
	    	  GetData();
			if (!IsPostBack)
			{
				BindGrid();
			}
		}

		private void GetData()
		{

			if (Session["Blind_dtTmpl"]==null)
			{
				tbBlind= new DataTable();
				tbBlind.Columns.Add("level",Type.GetType("System.Int32"));
				tbBlind.Constraints.Add("cnstr_level",tbBlind.Columns["level"],true);
				tbBlind.Columns["level"].AutoIncrement =true;
				tbBlind.Columns["level"].AutoIncrementStep =1;
				tbBlind.Columns["level"].AutoIncrementSeed =1;
				tbBlind.Columns.Add("blind",Type.GetType("System.Decimal"));
				tbBlind.Columns.Add("ante",Type.GetType("System.Decimal"));

				DataRow dr=DBase.GetFirstRow(Config.DbGetTournamentBettingsXMLTmpl,"@ID",BID);
				if (dr !=null)
				{
					string xml=dr["Data"].ToString() ;
					txtName.Text=dr["Name"].ToString() ;
					XmlDocument xd = new XmlDocument();
					xd.LoadXml(xml); 
					XmlNode nd= xd.SelectSingleNode("tsbettings");
					txtCoeff.Text= nd.Attributes["coefficient"].InnerText; 
					XmlNodeList nl=nd.ChildNodes;
					foreach(XmlNode en in nl)
					{
						DataRow  xdr = tbBlind.NewRow(); 
						xdr["blind"]= Utils.GetDecimal( en.Attributes["blind"].InnerText ); 
						xdr["ante"]= Utils.GetDecimal( en.Attributes["ante"].InnerText ); 
						tbBlind.Rows.Add(xdr);  
					}
				}
				Session["Blind_dtTmpl"]=tbBlind; 
			}
			else
			{
				tbBlind=(DataTable)Session["Blind_dtTmpl"];
			}
			vwBlind=tbBlind.DefaultView; 
		}

		protected void btnAdd_Click(object sender, System.EventArgs e)
		{
			tbBlind.Rows.Add(tbBlind.NewRow());   
			vwBlind=tbBlind.DefaultView; 
			BindGrid();
		  }
		protected void btnSaveBet_Click(object sender, System.EventArgs e)
		{
             string xml=String.Format("<tsbettings tournamentid=\"{0}\" coefficient=\"{1}\">",0,txtCoeff.Text) ; 
			foreach(DataRow dr in tbBlind.Rows)
			{
				xml+=String.Format("<level num=\"{0}\" blind=\"{1}\" ante=\"{2}\"/>",dr["level"],dr["blind"],dr["ante"]);
			}
            xml+="</tsbettings>";
			BID=DBase.ExecuteReturnInt(Config.DbSaveTournamentBettingsXMLTmpl,"@ID",BID,"@Name",txtName.Text,"@Data",xml );
			Response.Redirect(Session["TournamentBettingsMaintenanceUrl"].ToString() ); 
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.oGrid.CancelCommand += new System.Web.UI.WebControls.DataGridCommandEventHandler(this.oGrid_CancelCommand);
			this.oGrid.EditCommand += new System.Web.UI.WebControls.DataGridCommandEventHandler(this.oGrid_EditCommand);
			this.oGrid.UpdateCommand += new System.Web.UI.WebControls.DataGridCommandEventHandler(this.oGrid_UpdateCommand);
			this.oGrid.DeleteCommand += new System.Web.UI.WebControls.DataGridCommandEventHandler(this.oGrid_DeleteCommand);
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion

		private void oGrid_DeleteCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
		{
			TableCell itemCell = e.Item.Cells[0];
			int item = Utils.GetInt( itemCell.Text);
			DataRow dr= tbBlind.Rows.Find(item);
			tbBlind.Rows.Remove(dr); 
			vwBlind=tbBlind.DefaultView ;
			BindGrid();
		}

		private void oGrid_CancelCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
		{
			oGrid.EditItemIndex = -1;
			BindGrid();
		}

		private void oGrid_UpdateCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
		{
			TextBox betText = (TextBox)e.Item.Cells[1].Controls[0];
			TextBox anteText = (TextBox)e.Item.Cells[2].Controls[0];
 
			int item = Utils.GetInt( e.Item.Cells[0].Text);
			String sAnte = anteText.Text;
			String sBet = betText.Text;
			DataRow dr=tbBlind.Rows.Find(item);  
			dr[1] = Utils.GetDecimal(sBet) ;
			dr[2] = Utils.GetDecimal(sAnte) ;
			vwBlind=tbBlind.DefaultView; 
			oGrid.EditItemIndex = -1;
			BindGrid();

		}

		private void oGrid_EditCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
		{
			oGrid.EditItemIndex = e.Item.ItemIndex;
			BindGrid();
		}

		private void BindGrid()
		{
			oGrid.DataSource =vwBlind;
			oGrid.DataBind(); 
		}


	}
}
