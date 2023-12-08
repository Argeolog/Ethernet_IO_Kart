
Imports System.Text
Imports System
Imports System.Windows.Forms
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Net.Sockets
Imports System.Threading.Tasks
Imports System.Linq
Imports System.Data

Public Class Ethernet_IO_Kart

    Private GelenDatalarList As ListBox = New ListBox()
    Private VeriGelenPort As Integer = 2019
    Private AyarUdp As UdpClient
    Dim Result As IAsyncResult = Nothing


    Private Sub Receive(ByVal Result As IAsyncResult)
        Dim VerininGeldigiipAdres As IPEndPoint = New IPEndPoint(IPAddress.Parse("255.255.255.255"), VeriGelenPort)
        Dim GelenByte As Byte() = AyarUdp.EndReceive(Result, VerininGeldigiipAdres)
        Dim GelenDataStr As String = Encoding.GetEncoding("ISO-8859-9").GetString(GelenByte)
        AyarUdp.BeginReceive(AddressOf Receive, New Object())

        If GelenDataStr.Length = 24 Then
            Zaman_Asamı_Timer.Enabled = False

            If GelenDataStr.IndexOf("$DTO") = 0 Then
                MessageBox.Show("Ayar Gönderme Başarılı !", "Bilgi")
            Else
                MessageBox.Show("Ayar Gönderme Başarısız !", "Hata !")
            End If
        Else

            If GelenDatalarList.Items.Contains(GelenDataStr) = False Then  ' Gelen Datanın Aynısı Listboxda Varsa Boşuna Eklemedik Tekrardan.
                GelenDatalarList.Items.Add(GelenDataStr)
            End If
        End If
    End Sub


    Sub Role_islemleri(ByVal islemRoleNo As String, ByVal islemKodu As String)
        If Gelen_Datalar_Listview.Items.Count = 0 Then
            MessageBox.Show("İşlem Yapılacak Cihazı Yok")
            Return
        End If

        If Tcp_Radio.Checked = True Then
            Dim Tcp As TcpClient = New TcpClient()

            If Tcp.ConnectAsync(IPAddress.Parse(ip_Adres_Text.Text), Convert.ToInt32(Haberlesme_Portu_Text.Text)).Wait(1000) Then  '500 Ms Boyunca Bağlanmaya Çalışır.
                Me.Text = "TCP Veri Gönderiliyor..." & Now.ToString("HH:mm:ss.fff")
                Dim NetworkSteaming As NetworkStream = Tcp.GetStream()
                Dim Bytes As Byte() = Encoding.GetEncoding("ISO-8859-9").GetBytes(islemRoleNo & islemKodu)
                NetworkSteaming.Write(Bytes, 0, Bytes.Length)
                NetworkSteaming.Flush()
                Threading.Thread.Sleep(10)
                Tcp.Close()
            Else
                MessageBox.Show("Tcp Bağlantısı Sağlanamadı !!")
            End If
        ElseIf Udp_Radio.Checked = True Then
            Me.Text = "UDP Veri Gönderiliyor..." & Now.ToString("HH:mm:ss.fff")
            Dim Udp As UdpClient = New UdpClient()
            Udp.EnableBroadcast = False
            Dim Bytes As Byte() = Encoding.GetEncoding("ISO-8859-9").GetBytes(islemRoleNo & islemKodu)
            Udp.Connect(IPAddress.Parse(ip_Adres_Text.Text), Convert.ToInt32(Haberlesme_Portu_Text.Text)) '2018 Nolu Porta Cihaz Arama Komutu Gönderilir. 255.255.255.255= Broadcast Adresidir.Tüm Ağa Bu Bilgi Gönderilir.
            Udp.Send(Bytes, Bytes.Length)
            Udp.Close()
        Else
            Me.Text = "UDP BroadCast Veri Gönderiliyor..." & Now.ToString("HH:mm:ss.fff")
            Dim Udp As UdpClient = New UdpClient()
            Udp.EnableBroadcast = True
            Dim Bytes As Byte() = Encoding.GetEncoding("ISO-8859-9").GetBytes(islemRoleNo & islemKodu)
            Udp.Connect(IPAddress.Parse("255.255.255.255"), Convert.ToInt32(Haberlesme_Portu_Text.Text)) '2018 Nolu Porta Cihaz Arama Komutu Gönderilir. 255.255.255.255= Broadcast Adresidir.Tüm Ağa Bu Bilgi Gönderilir.
            Udp.Send(Bytes, Bytes.Length)
            Udp.Close()
        End If
    End Sub



    Private Sub Cihaz_Ayarlarını_Yerlestir()
        Bulunan_Cihaz_Sayısı_Label.Text = GelenDatalarList.Items.Count.ToString()

        If GelenDatalarList.Items.Count > 0 Then
            Dim HamData As String

            For i As Integer = 0 To GelenDatalarList.Items.Count - 1
                HamData = GelenDatalarList.Items(i).ToString()
                Dim items = New ListViewItem()
                items.Text = HamData.Substring(15, 17)      ' Mac ID
                items.SubItems.Add(HamData.Substring(4, 3)) ' Versiyon
                items.SubItems.Add(HamData.Substring(7, 8)) 'Model
                items.SubItems.Add(HamData.Substring(32, 16)) ' Cihaz Adı 

                items.SubItems.Add(CInt(HamData.Substring(48, 4)))  ' Cihaz ID Gelecek


                Dim ipAdres As String = CInt(HamData.Substring(54, 3)) & "." & CInt(HamData.Substring(57, 3)) & "." & CInt(HamData.Substring(60, 3)) & "." & CInt(HamData.Substring(63, 3))
                items.SubItems.Add(ipAdres.ToString())
                Dim AltAgMaskesi As String = CInt(HamData.Substring(66, 3)) & "." & CInt(HamData.Substring(69, 3)) & "." & CInt(HamData.Substring(72, 3)) & "." & CInt(HamData.Substring(75, 3))
                items.SubItems.Add(AltAgMaskesi.ToString())


                Dim AltAgGecidi As String = CInt(HamData.Substring(78, 3)) & "." & CInt(HamData.Substring(81, 3)) & "." & CInt(HamData.Substring(84, 3)) & "." & CInt(HamData.Substring(87, 3))
                items.SubItems.Add(AltAgGecidi.ToString())



                Dim DnsAdres As String = CInt(HamData.Substring(90, 3)) & "." & CInt(HamData.Substring(93, 3)) & "." & CInt(HamData.Substring(96, 3)) & "." & CInt(HamData.Substring(99, 3))
                items.SubItems.Add(DnsAdres.ToString())
                items.SubItems.Add(CInt(HamData.Substring(119, 5)))
                items.SubItems.Add(CInt(HamData.Substring(124, 2)))
                items.SubItems.Add(CInt(HamData.Substring(126, 2)))
                items.SubItems.Add(CInt(HamData.Substring(128, 2)))
                items.SubItems.Add(CInt(HamData.Substring(130, 2)))
                items.SubItems.Add(CInt(HamData.Substring(132, 1)))
                items.SubItems.Add(CInt(HamData.Substring(133, 1)))
                Gelen_Datalar_Listview.Items.Add(items)
            Next

            If Role_1_Suresi_Text.Text = "0" Then
                Role_1_Suresi_Text.Text = "1"
            End If

            If Role_2_Suresi_Text.Text = "0" Then
                Role_2_Suresi_Text.Text = "1"
            End If

            If Role_3_Suresi_Text.Text = "0" Then
                Role_3_Suresi_Text.Text = "1"
            End If

            If Role_4_Suresi_Text.Text = "0" Then
                Role_4_Suresi_Text.Text = "1"
            End If

            If Cihaz_ID_Text.Text = "0" Then
                Cihaz_ID_Text.Text = "1"
            End If

            If Gelen_Datalar_Listview.Items.Count > 0 Then Gelen_Datalar_Listview.Items(0).Selected = True
        End If
    End Sub










    Private Sub Zaman_Asamı_Timer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        Zaman_Asamı_Timer.Enabled = False
        MessageBox.Show("Beklenilen Sürede Cevap Gelmedi !")
    End Sub


    Private Sub Cihaz_Ara_Buton_Click(sender As Object, e As EventArgs) Handles Cihaz_Ara_Buton.Click
        If NetworkInterface.GetIsNetworkAvailable() = False Then
            MessageBox.Show("Ethernet Bağlantısı Yok")
            Return
        End If





        ' Network Yoğunluğuna Bağlı Olarak Udp Sorgusu Cihaza Ulaşır veya Ulaşmaz Diye 2 Kez Göndererek İşlem Bağlaması Yapıyoruz.
        ' Cihaz Her Bir Soruya Ayrı Yanıt Verecektir.
        Try
            Bulunan_Cihaz_Sayısı_Label.Text = "0"
            GelenDatalarList.Items.Clear()
            Gelen_Datalar_Listview.Items.Clear()
            Application.DoEvents()

            For i As Integer = 0 To 2 - 1
                Me.Cursor = Cursors.WaitCursor
                Dim HDSModul As UdpClient = New UdpClient()
                HDSModul.EnableBroadcast = True
                Dim Bytes As Byte() = Encoding.GetEncoding("ISO-8859-9").GetBytes("$LMA1989**")
                HDSModul.Connect("255.255.255.255", 2018) '2018 Nolu Porta Cihaz Arama Komutu Gönderilir. 255.255.255.255= Broadcast Adresidir.Tüm Ağa Bu Bilgi Gönderilir.
                HDSModul.Send(Bytes, Bytes.Length)
                HDSModul.Close()
                Threading.Thread.Sleep(250)
            Next

            Cihaz_Ayarlarını_Yerlestir()
            Me.Cursor = Cursors.Default
        Catch ex As Exception
            Me.Cursor = Cursors.[Default]
            MessageBox.Show(ex.Message.ToString())
        End Try
    End Sub





    Private Sub Ayar_Gonder_Buton_Click(sender As Object, e As EventArgs) Handles Ayar_Gonder_Buton.Click
        If NetworkInterface.GetIsNetworkAvailable() = False Then
            MessageBox.Show("Ethernet Bağlantısı Yok")
            Return
        End If

        If Gelen_Datalar_Listview.Items.Count = 0 Then Return
        Dim IPSplit As String() = ip_Adres_Text.Text.Split(".")
        Dim AltAgMaskesi As String() = Alt_Ag_Maskesi_Text.Text.Split(".")
        Dim AltAgGecidi As String() = Alt_Ag_Gecidi_Text.Text.Split(".")
        Dim DnsSunucusu As String() = Dns_Adresi_Text.Text.Split(".")
        Dim Komut As String = "$CAG"
        Komut &= Mac_Adres_Text.Text ' Mac ID
        Komut &= Cihaz_Adı_Text.Text.PadRight(16, " ") ' Cihaz Adı
        Komut &= Cihaz_ID_Text.Text.PadLeft(4, "0") ' Cihaz ID
        Komut &= "00" ' Tcp-Udp \ Server-Client
        Komut &= IPSplit(0).PadLeft(3, "0") & IPSplit(1).PadLeft(3, "0") & IPSplit(2).PadLeft(3, "0") & IPSplit(3).PadLeft(3, "0") ' İp Adres
        Komut &= AltAgMaskesi(0).PadLeft(3, "0") & AltAgMaskesi(1).PadLeft(3, "0") & AltAgMaskesi(2).PadLeft(3, "0") & AltAgMaskesi(3).PadLeft(3, "0") ' Sub Mask
        Komut &= AltAgGecidi(0).PadLeft(3, "0") & AltAgGecidi(1).PadLeft(3, "0") & AltAgGecidi(2).PadLeft(3, "0") & AltAgGecidi(3).PadLeft(3, "0") ' Gateway
        Komut &= DnsSunucusu(0).PadLeft(3, "0") & DnsSunucusu(1).PadLeft(3, "0") & DnsSunucusu(2).PadLeft(3, "0") & DnsSunucusu(3).PadLeft(3, "0") ' Dns
        Komut &= "192168001255" ' Remote İp Adres
        Komut &= "01520" ' Remote Port
        Komut &= Haberlesme_Portu_Text.Text.PadLeft(5, "0") ' Haberleşme Portu
        Komut &= Role_1_Suresi_Text.Text.PadLeft(2, "0")  ' Röle 1 Süresi
        Komut &= Role_2_Suresi_Text.Text.PadLeft(2, "0")  ' Röle 2 Süresi
        Komut &= Role_3_Suresi_Text.Text.PadLeft(2, "0")  ' Röle 3 Süresi
        Komut &= Role_4_Suresi_Text.Text.PadLeft(2, "0")  ' Röle 4 Süresi
        Komut &= Tcp_Soketi_Kapat_Check.CheckState ' Versiyon 3 ve Sonrası İçin Geçerli Olacaktır. Eğerki Socket Bağlantısı varsa Ama 2 saniye İçinde Sorgu Gelmezse Bağlantı Otomatik Olarak Kapanır. Pc Bağlantısı Kopup, Link Varsa Tedbiridir.
        Komut &= UDP_izin_Check.CheckState ' Udp Gelen Komutlar İşlensin mi ? Röle Aç vs gibi.

        Komut &= "00000000000000000000000000000000000000000000000000**" ' Revize. Toplam Gidecek Data Uzunluğu 175 Olmalıdır.


        ' Cihaz Gelen Data Kontrolünü Yapar ve Beklenilen Format Dışında Bir Değer Tespit Ederse Error Komutunu Döndürür.
        ' Örnek Ip Adresi 192.168.1.120  Olması Gerekirken
        '192.168.300.100 Gönderirsen veya İçerisinde Harf Yer Alırsa Gibi  Durumlarda.



        Dim Udp As UdpClient = New UdpClient()
        Udp.EnableBroadcast = True
        Dim Bytes As Byte() = Encoding.GetEncoding("ISO-8859-9").GetBytes(Komut)
        Udp.Connect("255.255.255.255", 2018)
        Udp.Send(Bytes, Bytes.Length)
        Udp.Close()
        Zaman_Asamı_Timer.Enabled = True
    End Sub



    Private Sub Gelen_Datalar_Listview_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Gelen_Datalar_Listview.SelectedIndexChanged
        If Gelen_Datalar_Listview.SelectedItems.Count > 0 Then


            Mac_Adres_Text.Text = Gelen_Datalar_Listview.SelectedItems(0).SubItems(0).Text
            Cihaz_Adı_Text.Text = Gelen_Datalar_Listview.SelectedItems(0).SubItems(3).Text.TrimEnd()
            Cihaz_ID_Text.Text = Gelen_Datalar_Listview.SelectedItems(0).SubItems(4).Text
            ip_Adres_Text.Text = Gelen_Datalar_Listview.SelectedItems(0).SubItems(5).Text
            Alt_Ag_Maskesi_Text.Text = Gelen_Datalar_Listview.SelectedItems(0).SubItems(6).Text
            Alt_Ag_Gecidi_Text.Text = Gelen_Datalar_Listview.SelectedItems(0).SubItems(7).Text
            Dns_Adresi_Text.Text = Gelen_Datalar_Listview.SelectedItems(0).SubItems(8).Text
            Haberlesme_Portu_Text.Text = Gelen_Datalar_Listview.SelectedItems(0).SubItems(9).Text
            Role_1_Suresi_Text.Text = Gelen_Datalar_Listview.SelectedItems(0).SubItems(10).Text
            Role_2_Suresi_Text.Text = Gelen_Datalar_Listview.SelectedItems(0).SubItems(11).Text
            Role_3_Suresi_Text.Text = Gelen_Datalar_Listview.SelectedItems(0).SubItems(12).Text
            Role_4_Suresi_Text.Text = Gelen_Datalar_Listview.SelectedItems(0).SubItems(13).Text
            Tcp_Soketi_Kapat_Check.Checked = CBool(Gelen_Datalar_Listview.SelectedItems(0).SubItems(14).Text)
            UDP_izin_Check.Checked = CBool(Gelen_Datalar_Listview.SelectedItems(0).SubItems(15).Text)
        End If
    End Sub


    Private Sub Role_1_Tetikle_Buton_Click(sender As Object, e As EventArgs) Handles Role_1_Tetikle_Buton.Click
        Role_islemleri("1", "0")
    End Sub

    Private Sub Role_1_Ac_Buton_Click(sender As Object, e As EventArgs) Handles Role_1_Ac_Buton.Click
        Role_islemleri("1", "1")
    End Sub

    Private Sub Role_1_Kapat_Buton_Click(sender As Object, e As EventArgs) Handles Role_1_Kapat_Buton.Click
        Role_islemleri("1", "2")
    End Sub

    Private Sub input_1_Alindi_Buton_Click(sender As Object, e As EventArgs) Handles input_1_Alindi_Buton.Click
        Role_islemleri("5", "1")
    End Sub

    Private Sub Role_2_Tetikle_Buton_Click(sender As Object, e As EventArgs) Handles Role_2_Tetikle_Buton.Click
        Role_islemleri("2", "0")
    End Sub

    Private Sub Role_2_Ac_Buton_Click(sender As Object, e As EventArgs) Handles Role_2_Ac_Buton.Click
        Role_islemleri("2", "1")
    End Sub

    Private Sub Role_2_Kapat_Buton_Click(sender As Object, e As EventArgs) Handles Role_2_Kapat_Buton.Click
        Role_islemleri("2", "2")
    End Sub

    Private Sub input_2_Alindi_Buton_Click(sender As Object, e As EventArgs) Handles input_2_Alindi_Buton.Click
        Role_islemleri("5", "2")
    End Sub

    Private Sub Role_3_Tetikle_Buton_Click(sender As Object, e As EventArgs) Handles Role_3_Tetikle_Buton.Click
        Role_islemleri("3", "0")
    End Sub

    Private Sub Role_3_Ac_Buton_Click(sender As Object, e As EventArgs) Handles Role_3_Ac_Buton.Click
        Role_islemleri("3", "1")
    End Sub

    Private Sub Role_3_Kapat_Buton_Click(sender As Object, e As EventArgs) Handles Role_3_Kapat_Buton.Click
        Role_islemleri("3", "2")
    End Sub

    Private Sub input_3_Alindi_Buton_Click(sender As Object, e As EventArgs) Handles input_3_Alindi_Buton.Click
        Role_islemleri("5", "3")
    End Sub

    Private Sub Role_4_Tetikle_Buton_Click(sender As Object, e As EventArgs) Handles Role_4_Tetikle_Buton.Click
        Role_islemleri("4", "0")
    End Sub

    Private Sub Role_4_Ac_Buton_Click(sender As Object, e As EventArgs) Handles Role_4_Ac_Buton.Click
        Role_islemleri("4", "1")
    End Sub

    Private Sub Role_4_Kapat_Buton_Click(sender As Object, e As EventArgs) Handles Role_4_Kapat_Buton.Click
        Role_islemleri("4", "2")
    End Sub

    Private Sub input_4_Alindi_Buton_Click(sender As Object, e As EventArgs) Handles input_4_Alindi_Buton.Click
        Role_islemleri("5", "4")
    End Sub


    Private Sub Durum_Sorgula_Buton_Click(sender As Object, e As EventArgs) Handles Durum_Sorgula_Buton.Click
        If NetworkInterface.GetIsNetworkAvailable() = False Then
            MessageBox.Show("Ethernet Bağlantısı Yok")
            Return
        End If

        If Gelen_Datalar_Listview.Items.Count = 0 Then Return
        Dim Tcp As TcpClient = New TcpClient()

        If Tcp.ConnectAsync(IPAddress.Parse(ip_Adres_Text.Text), Convert.ToInt32(Haberlesme_Portu_Text.Text)).Wait(1000) Then  '500 Ms Boyunca Bağlanmaya Çalışır.
            Dim NetworkSteaming As NetworkStream = Tcp.GetStream()
            Dim Bytes As Byte() = Encoding.GetEncoding("ISO-8859-9").GetBytes("64")
            NetworkSteaming.Write(Bytes, 0, Bytes.Length)
            Dim GelenCevapBytes As Byte() = New Byte(1023) {}
            Dim GelenDataUzunlugu As Integer = NetworkSteaming.Read(GelenCevapBytes, 0, GelenCevapBytes.Length)
            Dim GelenCevapStr As String = Encoding.ASCII.GetString(GelenCevapBytes, 0, GelenDataUzunlugu)

            If GelenCevapStr.Length = 26 Then
                input_1_Picturebox.Image = My.Resources.Disconnect
                input_2_Picturebox.Image = My.Resources.Disconnect
                input_3_Picturebox.Image = My.Resources.Disconnect
                input_4_Picturebox.Image = My.Resources.Disconnect
                Role_1_Picturebox.Image = My.Resources.Disconnect
                Role_2_Picturebox.Image = My.Resources.Disconnect
                Role_3_Picturebox.Image = My.Resources.Disconnect
                Role_4_Picturebox.Image = My.Resources.Disconnect

                If Equals(GelenCevapStr.Substring(18, 1), "1") Then
                    input_1_Picturebox.Image = My.Resources.Connect
                End If

                If Equals(GelenCevapStr.Substring(19, 1), "1") Then
                    input_2_Picturebox.Image = My.Resources.Connect
                End If

                If Equals(GelenCevapStr.Substring(20, 1), "1") Then
                    input_3_Picturebox.Image = My.Resources.Connect
                End If

                If Equals(GelenCevapStr.Substring(21, 1), "1") Then
                    input_4_Picturebox.Image = My.Resources.Connect
                End If

                If Equals(GelenCevapStr.Substring(22, 1), "1") Then
                    Role_1_Picturebox.Image = My.Resources.Connect
                End If

                If Equals(GelenCevapStr.Substring(23, 1), "1") Then
                    Role_2_Picturebox.Image = My.Resources.Connect
                End If

                If Equals(GelenCevapStr.Substring(24, 1), "1") Then
                    Role_3_Picturebox.Image = My.Resources.Connect
                End If

                If Equals(GelenCevapStr.Substring(25, 1), "1") Then
                    Role_4_Picturebox.Image = My.Resources.Connect
                End If
            End If

            NetworkSteaming.Flush()
            Threading.Thread.Sleep(10)
            Tcp.Close()
        End If
    End Sub


    Private Sub Anasayfa_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Control.CheckForIllegalCrossThreadCalls = False
        AyarUdp = New UdpClient(VeriGelenPort)
        AyarUdp.BeginReceive(AddressOf Receive, New Object())

        If NetworkInterface.GetIsNetworkAvailable() = True Then
            Cihaz_Ara_Buton.PerformClick()
        End If
    End Sub
End Class
