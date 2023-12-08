using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;

namespace Ethernet_IO_Kart
{
    public partial class Ethernet_IO_Kart : Form
    {
        public Ethernet_IO_Kart()
        {
            InitializeComponent();
        }

        private ListBox GelenDatalarList = new ListBox();
        private int VeriGelenPort = 2019;
        private UdpClient AyarUdp;
            
     

    private void Receive(IAsyncResult Result)
    {
        IPEndPoint VerininGeldigiipAdres = new IPEndPoint(IPAddress.Parse("255.255.255.255"), VeriGelenPort);
        byte[] GelenByte = AyarUdp.EndReceive(Result, ref VerininGeldigiipAdres);
        String GelenDataStr = Encoding.GetEncoding("ISO-8859-9").GetString(GelenByte);

       AyarUdp.BeginReceive(Receive, new object());



       if (GelenDataStr.Length == 24)
       {

           Zaman_Asamı_Timer.Enabled = false;
          

           if (GelenDataStr.IndexOf("$DTO")==0){

               MessageBox.Show("Ayar Gönderme Başarılı !","Bilgi");


           }

           else
           {


               MessageBox.Show("Ayar Gönderme Başarısız !","Hata !");

           }


       }


       else
       {
           if (GelenDatalarList.Items.Contains(GelenDataStr) == false)
           {  // Gelen Datanın Aynısı Listboxda Varsa Boşuna Eklemedik Tekrardan.
               GelenDatalarList.Items.Add(GelenDataStr);
           }


       }
                 

    
     }



    void Role_islemleri(string islemRoleNo, string islemKodu)
    {

        if (Gelen_Datalar_Listview.Items.Count == 0)
        {
            MessageBox.Show("İşlem Yapılacak Cihazı Yok");
            return;
        }


        if (Tcp_Radio.Checked == true) {

            TcpClient Tcp = new TcpClient();
                      


             if (Tcp.ConnectAsync(IPAddress.Parse(ip_Adres_Text.Text), Convert.ToInt32(Haberlesme_Portu_Text.Text)).Wait(500))  //500 Ms Boyunca Bağlanmaya Çalışır.
            {
                    this.Text = "TCP Veri Gönderiliyor..." + DateTime.Now.ToString("HH:mm:ss.fff");
            NetworkStream NetworkSteaming = Tcp.GetStream();
            byte[] Bytes = Encoding.GetEncoding("ISO-8859-9").GetBytes(islemRoleNo + islemKodu);
            NetworkSteaming.Write(Bytes, 0, Bytes.Length);
            NetworkSteaming.Flush();
            System.Threading.Thread.Sleep(10);
            Tcp.Close();



            }
             else
             {
                 MessageBox.Show("Tcp Bağlantısı Sağlanamadı !!");

             }

                



        }
        
        
        else if (Udp_Radio.Checked==true){


                this.Text = "UDP Veri Gönderiliyor..." + DateTime.Now.ToString("HH:mm:ss.fff");
                UdpClient Udp = new UdpClient();
            Udp.EnableBroadcast = false;
            byte[] Bytes = Encoding.GetEncoding("ISO-8859-9").GetBytes(islemRoleNo + islemKodu);
            Udp.Connect(IPAddress.Parse(ip_Adres_Text.Text), Convert.ToInt32(Haberlesme_Portu_Text.Text)); //2018 Nolu Porta Cihaz Arama Komutu Gönderilir. 255.255.255.255= Broadcast Adresidir.Tüm Ağa Bu Bilgi Gönderilir.
            Udp.Send(Bytes, Bytes.Length);
            Udp.Close();



           
        }

        else

        {


                this.Text = "UDP BroadCast Veri Gönderiliyor..." + DateTime.Now.ToString("HH:mm:ss.fff");
                UdpClient Udp = new UdpClient();
            Udp.EnableBroadcast = true;
            byte[] Bytes = Encoding.GetEncoding("ISO-8859-9").GetBytes(islemRoleNo + islemKodu);
            Udp.Connect(IPAddress.Parse("255.255.255.255"), Convert.ToInt32(Haberlesme_Portu_Text.Text)); //2018 Nolu Porta Cihaz Arama Komutu Gönderilir. 255.255.255.255= Broadcast Adresidir.Tüm Ağa Bu Bilgi Gönderilir.
            Udp.Send(Bytes, Bytes.Length);
            Udp.Close();


        }


 




    }




        
        private void Ethernet_IO_Kart_Load(object sender, EventArgs e)
        {

            Control.CheckForIllegalCrossThreadCalls = false;
            AyarUdp = new UdpClient(VeriGelenPort);
            AyarUdp.BeginReceive(Receive, new object());
            if (NetworkInterface.GetIsNetworkAvailable() == true)
            {
                Cihaz_Ara_Buton.PerformClick();
            }



        }



      
       private void Cihaz_Ayarlarını_Yerlestir(){
        Bulunan_Cihaz_Sayısı_Label.Text = GelenDatalarList.Items.Count.ToString();
        if (GelenDatalarList.Items.Count>0) {
            String HamData;
            for (int i = 0; i < GelenDatalarList.Items.Count; i++)
            {

                HamData = GelenDatalarList.Items[i].ToString();

             
                 
                var items = new ListViewItem();
                 
                items.Text = HamData.Substring(15, 17);      // Mac ID
                items.SubItems.Add(HamData.Substring(4, 3)); // Versiyon
                items.SubItems.Add(HamData.Substring(7, 8)); //Model
                items.SubItems.Add(HamData.Substring(32, 16)); // Cihaz Adı 
                items.SubItems.Add( Convert.ToInt32(HamData.Substring(48, 4)).ToString());  // Cihaz ID Gelecek
              
                var ipAdres = Convert.ToInt32(HamData.Substring(54, 3)) + "." + Convert.ToInt32(HamData.Substring(57, 3)) + "." + Convert.ToInt32(HamData.Substring(60, 3)) + "." + Convert.ToInt32(HamData.Substring(63, 3));
                items.SubItems.Add(ipAdres.ToString());

                var AltAgMaskesi = Convert.ToInt32(HamData.Substring(66, 3)) + "." + Convert.ToInt32(HamData.Substring(69, 3)) + "." + Convert.ToInt32(HamData.Substring(72, 3)) + "." + Convert.ToInt32(HamData.Substring(75, 3));
                items.SubItems.Add(AltAgMaskesi.ToString());


                var AltAgGecidi = Convert.ToInt32(HamData.Substring(78, 3)) + "." + Convert.ToInt32(HamData.Substring(81, 3)) + "." + Convert.ToInt32(HamData.Substring(84, 3)) + "." + Convert.ToInt32(HamData.Substring(87, 3));
                items.SubItems.Add(AltAgGecidi.ToString());


                var DnsAdres = Convert.ToInt32(HamData.Substring(90, 3)) + "." + Convert.ToInt32(HamData.Substring(93, 3)) + "." + Convert.ToInt32(HamData.Substring(96, 3)) + "." + Convert.ToInt32(HamData.Substring(99, 3));
                items.SubItems.Add(DnsAdres.ToString());


                  


                               
                 items.SubItems.Add(Convert.ToString(Convert.ToInt32(HamData.Substring(119, 5))));
                 items.SubItems.Add(Convert.ToString(Convert.ToInt32(HamData.Substring(124, 2))));
                 items.SubItems.Add(Convert.ToString(Convert.ToInt32(HamData.Substring(126, 2))));
                 items.SubItems.Add(Convert.ToString(Convert.ToInt32(HamData.Substring(128, 2))));
                 items.SubItems.Add(Convert.ToString(Convert.ToInt32(HamData.Substring(130, 2))));
                 items.SubItems.Add(Convert.ToString(Convert.ToInt32(HamData.Substring(132, 1))));
                    items.SubItems.Add(Convert.ToString(Convert.ToInt32(HamData.Substring(133, 1))));
                    Gelen_Datalar_Listview.Items.Add(items);

                    



            }


           

             if (Role_1_Suresi_Text.Text == "0")  Role_1_Suresi_Text.Text = "1";
               if (Role_2_Suresi_Text.Text == "0") Role_2_Suresi_Text.Text = "1";
                if (Role_3_Suresi_Text.Text == "0") Role_3_Suresi_Text.Text = "1";
                if (Role_4_Suresi_Text.Text == "0") Role_4_Suresi_Text.Text = "1";
                if (Cihaz_ID_Text.Text == "0") Cihaz_ID_Text.Text = "1";

                if (Gelen_Datalar_Listview.Items.Count>0) Gelen_Datalar_Listview.Items[0].Selected = true;



        }



    }




        private void Cihaz_Ara_Buton_Click(object sender, EventArgs e)
        {


            if (NetworkInterface.GetIsNetworkAvailable() == false)
            {
             MessageBox.Show ("Ethernet Bağlantısı Yok");
             return;
            }




            // Network Yoğunluğuna Bağlı Olarak Udp Sorgusu Cihaza Ulaşır veya Ulaşmaz Diye 2 Kez Göndererek İşlem Bağlaması Yapıyoruz.
            // Cihaz Her Bir Soruya Ayrı Yanıt Verecektir.
            try
            {

                Bulunan_Cihaz_Sayısı_Label.Text = "0";
                GelenDatalarList.Items.Clear();
                Gelen_Datalar_Listview.Items.Clear();
                Application.DoEvents();


                for (int i = 0; i < 2; i++)
                {
                    this.Cursor = Cursors.WaitCursor;

                    UdpClient HDSModul = new UdpClient();
                    HDSModul.EnableBroadcast = true;
                    byte[] Bytes = Encoding.GetEncoding("ISO-8859-9").GetBytes("$LMA1989**");
                    HDSModul.Connect("255.255.255.255", 2018); //2018 Nolu Porta Cihaz Arama Komutu Gönderilir. 255.255.255.255= Broadcast Adresidir.Tüm Ağa Bu Bilgi Gönderilir.
                    HDSModul.Send(Bytes, Bytes.Length);
                    HDSModul.Close();
                    System.Threading.Thread.Sleep(250); 
                }


               
                Cihaz_Ayarlarını_Yerlestir();









                this.Cursor = Cursors.Default;

              
            }

            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;                                       
                MessageBox.Show(ex.Message.ToString());

            }


         
                           
            

        }

       

        private void Role_1_Tetikle_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("1", "0");
        }

        private void Role_1_Ac_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("1", "1");
        }

        private void Role_1_Kapat_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("1", "2");
        }

        private void input_1_Alindi_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("5", "1");
        }

        private void Role_2_Tetikle_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("2", "0");
        }

        private void Role_2_Ac_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("2", "1");
        }

        private void Role_2_Kapat_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("2", "2");
        }

        private void input_2_Alindi_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("5", "2");
        }

        private void Role_3_Tetikle_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("3", "0");
        }

        private void Role_3_Ac_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("3", "1");
        }

        private void Role_3_Kapat_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("3", "2");
        }

        private void input_3_Alindi_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("5", "3");
        }

        private void Role_4_Tetikle_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("4", "0");
        }

        private void Role_4_Ac_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("4", "1");
        }

        private void Role_4_Kapat_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("4", "2");
        }

        private void input_4_Alindi_Buton_Click(object sender, EventArgs e)
        {
            Role_islemleri("5", "4");
        }






        private void Ayar_Gonder_Buton_Click(object sender, EventArgs e)
        {

            if (NetworkInterface.GetIsNetworkAvailable() == false)
            {
                MessageBox.Show("Ethernet Bağlantısı Yok");
                return;
            }
          


                if (Gelen_Datalar_Listview.Items.Count == 0) return;
               


                string[] IPSplit = ip_Adres_Text.Text.Split('.');
                string[] AltAgMaskesi = Alt_Ag_Maskesi_Text.Text.Split('.');
                string[] AltAgGecidi = Alt_Ag_Gecidi_Text.Text.Split('.');
                string[] DnsSunucusu = Dns_Adresi_Text.Text.Split('.');
                 
                string Komut = "$CAG";
                Komut +=  Mac_Adres_Text.Text; // Mac ID
                Komut +=  Cihaz_Adı_Text.Text.PadRight(16, ' '); // Cihaz Adı
                Komut +=  Cihaz_ID_Text.Text.PadLeft(4, '0'); // Cihaz ID
                Komut +=  "00"; // Tcp-Udp \ Server-Client
                Komut +=  IPSplit[0].PadLeft(3, '0') + IPSplit[1].PadLeft(3, '0') + IPSplit[2].PadLeft(3, '0') + IPSplit[3].PadLeft(3, '0'); // İp Adres
                Komut +=  AltAgMaskesi[0].PadLeft(3, '0') + AltAgMaskesi[1].PadLeft(3, '0') + AltAgMaskesi[2].PadLeft(3, '0') + AltAgMaskesi[3].PadLeft(3, '0');// Sub Mask
                Komut +=  AltAgGecidi[0].PadLeft(3, '0') + AltAgGecidi[1].PadLeft(3, '0') + AltAgGecidi[2].PadLeft(3, '0') + AltAgGecidi[3].PadLeft(3, '0'); // Gateway
                Komut +=  DnsSunucusu[0].PadLeft(3, '0') + DnsSunucusu[1].PadLeft(3, '0') + DnsSunucusu[2].PadLeft(3, '0') + DnsSunucusu[3].PadLeft(3, '0'); // Dns
                Komut +=  "192168001255"; // Remote İp Adres
                Komut +=  "01520"; // Remote Port
                Komut +=  Haberlesme_Portu_Text.Text.PadLeft(5, '0'); // Haberleşme Portu
                Komut +=  Role_1_Suresi_Text.Text.PadLeft(2, '0');  // Röle 1 Süresi
                Komut +=  Role_2_Suresi_Text.Text.PadLeft(2, '0');  // Röle 2 Süresi
                Komut +=  Role_3_Suresi_Text.Text.PadLeft(2, '0');  // Röle 3 Süresi
                Komut +=  Role_4_Suresi_Text.Text.PadLeft(2, '0');  // Röle 4 Süresi
                Komut +=  ((int)Tcp_Soketi_Kapat_Check.CheckState); // Versiyon 3 ve Sonrası İçin Geçerli Olacaktır. Eğerki Socket Bağlantısı varsa Ama 2 saniye İçinde Sorgu Gelmezse Bağlantı Otomatik Olarak Kapanır. Pc Bağlantısı Kopup, Link Varsa Tedbiridir.
                Komut += ((int)UDP_izin_Check.CheckState); // UDP Gelen Komutlar İşlensin mi ? Röle Aç vs gibi.

                Komut +="00000000000000000000000000000000000000000000000000**"; // Revize. Toplam Gidecek Data Uzunluğu 175 Olmalıdır.
               
                // Cihaz Gelen Data Kontrolünü Yapar ve Beklenilen Format Dışında Bir Değer Tespit Ederse Error Komutunu Döndürür.
                // Örnek Ip Adresi 192.168.1.120  Olması Gerekirken
                //192.168.300.100 Gönderirsen veya İçerisinde Harf Yer Alırsa Gibi  Durumlarda.


                 
                UdpClient Udp = new UdpClient();
                Udp.EnableBroadcast = true;
                byte[] Bytes = Encoding.GetEncoding("ISO-8859-9").GetBytes(Komut);
                Udp.Connect("255.255.255.255", 2018);
                Udp.Send(Bytes, Bytes.Length);
                Udp.Close();
                Zaman_Asamı_Timer.Enabled = true;


           

           

        }

        private void Zaman_Asamı_Timer_Tick(object sender, EventArgs e)
        {

            Zaman_Asamı_Timer.Enabled = false;
            MessageBox.Show("Beklenilen Sürede Cevap Gelmedi !");

        }

        private void Durum_Sorgula_Buton_Click(object sender, EventArgs e)
        {


            if (NetworkInterface.GetIsNetworkAvailable() == false)
            {
                MessageBox.Show("Ethernet Bağlantısı Yok");
                return;
            }

            if (Gelen_Datalar_Listview.Items.Count == 0) return;
          

              TcpClient Tcp = new TcpClient();
          if (Tcp.ConnectAsync(IPAddress.Parse(ip_Adres_Text.Text), Convert.ToInt32(Haberlesme_Portu_Text.Text)).Wait(500))  //500 Ms Boyunca Bağlanmaya Çalışır.
            {

            NetworkStream NetworkSteaming = Tcp.GetStream();
            byte[] Bytes = Encoding.GetEncoding("ISO-8859-9").GetBytes("64");
            NetworkSteaming.Write(Bytes, 0, Bytes.Length);
            byte[] GelenCevapBytes = new byte[1024];
            int GelenDataUzunlugu = NetworkSteaming.Read(GelenCevapBytes, 0, GelenCevapBytes.Length);
            string GelenCevapStr = Encoding.ASCII.GetString(GelenCevapBytes, 0, GelenDataUzunlugu);


            if (GelenCevapStr.Length == 26)
            {

                input_1_Picturebox.Image = Properties.Resources.Disconnect;
                input_2_Picturebox.Image = Properties.Resources.Disconnect;
                input_3_Picturebox.Image = Properties.Resources.Disconnect;
                input_4_Picturebox.Image = Properties.Resources.Disconnect;

                Role_1_Picturebox.Image = Properties.Resources.Disconnect;
                Role_2_Picturebox.Image = Properties.Resources.Disconnect;
                Role_3_Picturebox.Image = Properties.Resources.Disconnect;
                Role_4_Picturebox.Image = Properties.Resources.Disconnect;



                if (GelenCevapStr.Substring(18, 1) == "1")
                {
                    input_1_Picturebox.Image = Properties.Resources.Connect;

                }

                if (GelenCevapStr.Substring(19, 1) == "1")
                {
                    input_2_Picturebox.Image = Properties.Resources.Connect;

                }


                if (GelenCevapStr.Substring(20, 1) == "1")
                {
                    input_3_Picturebox.Image = Properties.Resources.Connect;

                }

                if (GelenCevapStr.Substring(21, 1) == "1")
                {
                    input_4_Picturebox.Image = Properties.Resources.Connect;

                }


                if (GelenCevapStr.Substring(22, 1) == "1")
                {
                    Role_1_Picturebox.Image = Properties.Resources.Connect;

                }


                if (GelenCevapStr.Substring(23, 1) == "1")
                {
                    Role_2_Picturebox.Image = Properties.Resources.Connect;

                }


                if (GelenCevapStr.Substring(24, 1) == "1")
                {
                    Role_3_Picturebox.Image = Properties.Resources.Connect;

                }


                if (GelenCevapStr.Substring(25, 1) == "1")
                {
                    Role_4_Picturebox.Image = Properties.Resources.Connect;

                }





            }
           


         

            NetworkSteaming.Flush();
            System.Threading.Thread.Sleep(10);
            Tcp.Close();


      }
     }

        private void Gelen_Datalar_Listview_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Gelen_Datalar_Listview.SelectedItems.Count > 0)
            {

                Mac_Adres_Text.Text = Gelen_Datalar_Listview.SelectedItems[0].SubItems[0].Text;
                Cihaz_Adı_Text.Text = Gelen_Datalar_Listview.SelectedItems[0].SubItems[3].Text.TrimEnd();
                Cihaz_ID_Text.Text = Gelen_Datalar_Listview.SelectedItems[0].SubItems[4].Text;
                ip_Adres_Text.Text = Gelen_Datalar_Listview.SelectedItems[0].SubItems[5].Text;
                Alt_Ag_Maskesi_Text.Text = Gelen_Datalar_Listview.SelectedItems[0].SubItems[6].Text;
                Alt_Ag_Gecidi_Text.Text = Gelen_Datalar_Listview.SelectedItems[0].SubItems[7].Text;
                Dns_Adresi_Text.Text = Gelen_Datalar_Listview.SelectedItems[0].SubItems[8].Text;
                Haberlesme_Portu_Text.Text = Gelen_Datalar_Listview.SelectedItems[0].SubItems[9].Text;
                Role_1_Suresi_Text.Text = Gelen_Datalar_Listview.SelectedItems[0].SubItems[10].Text;
                Role_2_Suresi_Text.Text = Gelen_Datalar_Listview.SelectedItems[0].SubItems[11].Text;
                Role_3_Suresi_Text.Text = Gelen_Datalar_Listview.SelectedItems[0].SubItems[12].Text;
                Role_4_Suresi_Text.Text = Gelen_Datalar_Listview.SelectedItems[0].SubItems[13].Text;
                Tcp_Soketi_Kapat_Check.Checked = Gelen_Datalar_Listview.SelectedItems[0].SubItems[14].Text == "1" ? true : false;
                UDP_izin_Check.Checked = Gelen_Datalar_Listview.SelectedItems[0].SubItems[15].Text == "1" ? true : false;
            }
        }
    }
}
