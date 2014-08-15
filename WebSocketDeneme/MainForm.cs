using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using System.Collections.Generic;

namespace WebSocketDeneme
{
	using System;
	using System.Windows.Forms;
	using SuperWebSocket;

	public class MainForm : Form
	{
		//mesajlaşma kontrolleri
		private Button btn_Baglan;
		private TextBox txt_Mesaj;
		private RichTextBox txt_Mesajlar;

		//websocket server
		private WebSocketServer anaServer;
		private bool ServerBasladiMi = false;

		//paneller
		private Panel panelAna;
		private Panel panelKullanicilar;

		private Label lb_Etiket;
		private ListBox lb_Kullanicilar;

		//bağlı kullanıcılar burada tutulacak.
		private List<WebSocketSession> bagliKullanicilar;


		public MainForm ()
		{
			this.KontrolleriYukle ();

			//bu sınıflar nuget üzerinden SuperWebSocket aratılarak eklenecek projeye!
			this.anaServer = new WebSocketServer ();
			//portumuz
			this.anaServer.Setup (8585);
			//eventlar.
			this.anaServer.NewDataReceived += OnDataGeldi;
			this.anaServer.NewMessageReceived += OnMesajGeldi;
			this.anaServer.NewSessionConnected += OnYeniBaglantiKuruldu;
			this.anaServer.SessionClosed += OnBaglantiKoptu;

			//bağlı kullanıcılar.
			this.bagliKullanicilar = new List<WebSocketSession> ();
		}

		//program ilk açılış.
		public static void Main (string[] args)
		{
			Application.SetCompatibleTextRenderingDefault (true);

			Application.EnableVisualStyles ();

			Application.Run (new MainForm ());
		}

		private void KontrolleriYukle ()
		{
			//form ayarlansın.
			this.Text = "WebSocket Server Uygulamalası";
			this.Size = new System.Drawing.Size (640, 480);
			this.StartPosition = FormStartPosition.CenterScreen;

			//paneller ayarlansın.
			this.panelAna = new Panel ();
			this.panelKullanicilar = new Panel () { 
				Width = 300
			};

			this.panelAna.Dock = DockStyle.Fill;
			this.panelKullanicilar.Dock = DockStyle.Right;

			this.Controls.Add (this.panelAna);
			this.Controls.Add (this.panelKullanicilar);

			//panel anaya kontroller eklensin.
			this.txt_Mesajlar = new RichTextBox () { 
				Dock = DockStyle.Fill
			};
			this.panelAna.Controls.Add (this.txt_Mesajlar);

			this.btn_Baglan = new Button () { 
				Text = "Server Başlasın",
				Dock = DockStyle.Top
			};
			this.panelAna.Controls.Add (this.btn_Baglan);

			this.txt_Mesaj = new TextBox () { 
				Dock = DockStyle.Bottom
			};
			this.panelAna.Controls.Add (this.txt_Mesaj);

			//events:

			this.btn_Baglan.Click += this.btn_Baglan_Clicked;
			this.txt_Mesaj.KeyUp += this.txt_Mesaj_KeyUpEvent;

			//panelKullanicilara Listbox Eklensin.
			this.lb_Kullanicilar = new ListBox ();

			this.lb_Etiket = new Label { 
				Text = "Bağlı Kullanıcılar",
				TextAlign = System.Drawing.ContentAlignment.MiddleCenter, 
				BorderStyle = BorderStyle.Fixed3D 
			};

			this.lb_Kullanicilar.Dock = DockStyle.Fill;
			this.lb_Etiket.Dock = DockStyle.Top;

			this.panelKullanicilar.Controls.Add (this.lb_Kullanicilar);
			this.panelKullanicilar.Controls.Add (this.lb_Etiket);
		}

		//bağlan butonuna tıklandığında çalışacak event.
		private void btn_Baglan_Clicked (object sender, EventArgs e)
		{
			if (this.ServerBasladiMi == false) {
				this.btn_Baglan.Text = "Server Durdurulsun!";
				this.ServerBasladiMi = true;
				this.anaServer.Start ();
				this.EkranaMesajYaz ("Server Başlatıldı!");

			} else {
				this.btn_Baglan.Text = "Server Başlasın";
				this.ServerBasladiMi = false;
				this.anaServer.Stop ();
				this.EkranaMesajYaz ("Server Durduruldu!");
			}
		}

		//txt_mesaj kutusunda entera basılınca çalışacak event.
		private void txt_Mesaj_KeyUpEvent (object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter) {
				this.EkranaMesajYaz (string.Format ("Mesaj Gönderdiniz: {0}", this.txt_Mesaj.Text.Trim ()));
				foreach (var item in bagliKullanicilar) {
					item.Send (this.txt_Mesaj.Text.Trim ());
				}
				this.txt_Mesaj.ResetText ();
			}
		}

		private void EkranaMesajYaz (string mesaj)
		{
			this.txt_Mesajlar.AppendText (string.Format ("{0}{1}", mesaj, Environment.NewLine));
			this.txt_Mesajlar.ScrollToCaret ();
		}

		//listbox ı güncelleyen metod.
		private void ListBoxGuncelle ()
		{
			this.lb_Kullanicilar.Items.Clear ();
			foreach (var bk in bagliKullanicilar) {
				this.lb_Kullanicilar.Items.Add (bk.SessionID);
			}
		}

		#region Socket Events
		//blog yada arraybuffer data gelirse bu metod çalışıyor!
		private void OnDataGeldi (WebSocketSession session, byte[] value)
		{

		}

		//string mesaj gelirse bu metod çalışıyor.
		private void OnMesajGeldi (WebSocketSession session, string value)
		{
			this.EkranaMesajYaz (string.Format ("{1} IDsinden Mesaj Geldi: {0}", value, session.SessionID));
		}

		//bağlantı kurulduğunda bu metod çalışıyor.
		private void OnYeniBaglantiKuruldu (WebSocketSession session)
		{
			if (!this.bagliKullanicilar.Contains (session)) {
				this.bagliKullanicilar.Add (session);
			}
			this.EkranaMesajYaz (string.Format ("{0} kullanıcısı bağlantıyı açtı!", session.SessionID));

			this.ListBoxGuncelle ();
		}

		//bağlantı kesildiğinde bu metod çalışıyor.
		private void OnBaglantiKoptu (WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
		{
			if (this.bagliKullanicilar.Contains (session)) {
				this.bagliKullanicilar.Remove (session);
			}
			this.EkranaMesajYaz (string.Format ("{0} kullanıcısı bağlantıyı kapattı!", session.SessionID));

			this.ListBoxGuncelle ();
		}

		#endregion
	}
}