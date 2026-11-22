using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

[assembly: AssemblyTitle("USD TL Hesap")]
[assembly: AssemblyProduct("USD TL Hesap")]
[assembly: AssemblyCompany("Önsel Özden")]
[assembly: AssemblyCopyright("Hazırlayan: Önsel Özden © 2026")]
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]

class Program
{
    [STAThread]
    static void Main()
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // TLS 1.2
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new AnaForm());
    }
}

class Tema
{
    public Color Zemin, Kart, Yazi, Soluk, Kutu, Vurgu;
    public static readonly Tema Acik = new Tema
    {
        Zemin = Color.White, Kart = Color.FromArgb(243, 245, 248), Yazi = Color.FromArgb(20, 24, 31),
        Soluk = Color.FromArgb(102, 112, 133), Kutu = Color.White, Vurgu = Color.FromArgb(10, 92, 255)
    };
    public static readonly Tema Koyu = new Tema
    {
        Zemin = Color.FromArgb(14, 17, 22), Kart = Color.FromArgb(23, 27, 34), Yazi = Color.FromArgb(233, 237, 243),
        Soluk = Color.FromArgb(152, 162, 179), Kutu = Color.FromArgb(30, 35, 44), Vurgu = Color.FromArgb(10, 92, 255)
    };
}

class MenuRenk : ProfessionalColorTable
{
    Tema t;
    public MenuRenk(Tema t) { this.t = t; UseSystemColors = false; }
    public override Color MenuStripGradientBegin { get { return t.Zemin; } }
    public override Color MenuStripGradientEnd { get { return t.Zemin; } }
    public override Color ToolStripDropDownBackground { get { return t.Kart; } }
    public override Color ImageMarginGradientBegin { get { return t.Kart; } }
    public override Color ImageMarginGradientMiddle { get { return t.Kart; } }
    public override Color ImageMarginGradientEnd { get { return t.Kart; } }
    public override Color MenuItemSelected { get { return t.Vurgu; } }
    public override Color MenuItemSelectedGradientBegin { get { return t.Vurgu; } }
    public override Color MenuItemSelectedGradientEnd { get { return t.Vurgu; } }
    public override Color MenuItemPressedGradientBegin { get { return t.Kart; } }
    public override Color MenuItemPressedGradientEnd { get { return t.Kart; } }
    public override Color MenuItemBorder { get { return t.Vurgu; } }
    public override Color MenuBorder { get { return t.Soluk; } }
    public override Color CheckBackground { get { return t.Vurgu; } }
    public override Color CheckSelectedBackground { get { return t.Vurgu; } }
    public override Color CheckPressedBackground { get { return t.Vurgu; } }
    public override Color SeparatorDark { get { return t.Soluk; } }
}

static class Logo
{
    static Image cache;
    public static Image Al()
    {
        if (cache != null) return cache;
        try
        {
            var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("logo96.png");
            if (s != null) cache = Image.FromStream(s);
        }
        catch { }
        return cache;
    }
    public static Icon Ikon()
    {
        try { return Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { return null; }
    }
}

class AnaForm : Form
{
    static readonly CultureInfo TR = new CultureInfo("tr-TR");
    static readonly string AyarYolu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UsdTlHesap", "tema.txt");

    TextBox txtUsd, txtKur;
    Label lblAna, lblKdv, lblTop, lblDurum, lblBaslik, lblTopYazi;
    Label[] etiketler;
    Button btnYenile;
    Panel pnlTop, pnlKart;
    MenuStrip menu;
    ToolStripMenuItem miAcik, miKoyu;
    System.Windows.Forms.Timer zamanlayici;
    Tema tema = Tema.Acik;

    public AnaForm()
    {
        Text = "USD → TL Fiyat Hesap";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(440, 470);
        Font = new Font("Segoe UI", 10f);
        var ik = Logo.Ikon(); if (ik != null) Icon = ik;

        // Menü
        menu = new MenuStrip();
        var mTema = new ToolStripMenuItem("Tema");
        miAcik = new ToolStripMenuItem("Beyaz (açık)", null, delegate { TemaUygula(Tema.Acik, true); });
        miKoyu = new ToolStripMenuItem("Siyah (koyu)", null, delegate { TemaUygula(Tema.Koyu, true); });
        mTema.DropDownItems.Add(miAcik);
        mTema.DropDownItems.Add(miKoyu);
        var mHak = new ToolStripMenuItem("Hakkında", null, delegate { using (var h = new HakkindaForm(tema)) h.ShowDialog(this); });
        menu.Items.Add(mTema);
        menu.Items.Add(mHak);
        MainMenuStrip = menu;

        int y0 = 34;

        // Başlık: logo + ad
        var pbLogo = new PictureBox { Image = Logo.Al(), SizeMode = PictureBoxSizeMode.Zoom, Location = new Point(20, y0 + 6), Size = new Size(48, 48) };
        Controls.Add(pbLogo);
        lblBaslik = new Label { Text = "USD → TL Fiyat Hesap", Font = new Font("Segoe UI", 15f, FontStyle.Bold), Location = new Point(78, y0 + 16), AutoSize = true };
        Controls.Add(lblBaslik);

        // Kur kartı
        pnlKart = new Panel { Location = new Point(20, y0 + 66), Size = new Size(400, 70) };
        var lKur = Etiket("1 USD =", 12, 14, 62);
        txtKur = new TextBox { Location = new Point(76, 11), Width = 90, BorderStyle = BorderStyle.FixedSingle };
        txtKur.TextChanged += delegate { Hesapla(); };
        var lTl = Etiket("TL", 170, 14, 28);
        btnYenile = new Button { Text = "Kuru yenile", Location = new Point(280, 9), Width = 108, Height = 30, FlatStyle = FlatStyle.Flat };
        btnYenile.Click += delegate { KurCek(); };
        lblDurum = Etiket("", 12, 44, 380);
        lblDurum.Font = new Font("Segoe UI", 9f);
        pnlKart.Controls.AddRange(new Control[] { lKur, txtKur, lTl, btnYenile, lblDurum });
        Controls.Add(pnlKart);

        int y = y0 + 150;
        var lUsd = Etiket("USD fiyat", 20, y, 200);
        Controls.Add(lUsd);
        txtUsd = new TextBox { Location = new Point(20, y + 24), Width = 400, Font = new Font("Segoe UI", 20f), BorderStyle = BorderStyle.FixedSingle };
        txtUsd.TextChanged += delegate { Hesapla(); };
        Controls.Add(txtUsd);

        y += 84;
        var lAna = Etiket("TL fiyat (KDV hariç)", 20, y, 200);
        lblAna = Deger(190, y - 2);
        var lKdv = Etiket("KDV %20", 20, y + 36, 200);
        lblKdv = Deger(190, y + 34);
        Controls.AddRange(new Control[] { lAna, lblAna, lKdv, lblKdv });

        pnlTop = new Panel { Location = new Point(20, y + 80), Size = new Size(400, 60), Cursor = Cursors.Hand };
        lblTopYazi = new Label { Text = "KDV dahil TL", ForeColor = Color.White, Font = new Font("Segoe UI", 13f, FontStyle.Bold), Location = new Point(12, 17), AutoSize = true };
        lblTop = new Label { Text = "0,00 ₺", ForeColor = Color.White, Font = new Font("Segoe UI", 15f, FontStyle.Bold), Location = new Point(150, 14), Size = new Size(238, 32), TextAlign = ContentAlignment.MiddleRight };
        EventHandler kopyala = delegate { try { Clipboard.SetText(lblTop.Text.Replace(" ₺", "")); lblDurum.Text = "KDV dahil tutar kopyalandı"; } catch { } };
        pnlTop.Click += kopyala; lblTopYazi.Click += kopyala; lblTop.Click += kopyala;
        pnlTop.Controls.Add(lblTopYazi); pnlTop.Controls.Add(lblTop);
        Controls.Add(pnlTop);

        var ipucu = Etiket("Tutara tıklayınca kopyalanır", 20, y + 146, 400);
        ipucu.Font = new Font("Segoe UI", 8.5f);
        Controls.Add(ipucu);

        Controls.Add(menu);

        etiketler = new[] { lKur, lTl, lUsd, lAna, lKdv };
        soluklar = new[] { lblDurum, ipucu };

        zamanlayici = new System.Windows.Forms.Timer { Interval = 30 * 60 * 1000 };
        zamanlayici.Tick += delegate { KurCek(); };
        zamanlayici.Start();

        TemaUygula(AyarOku() == "koyu" ? Tema.Koyu : Tema.Acik, false);
        Shown += delegate { txtUsd.Focus(); KurCek(); };
    }

    Label[] soluklar;
    bool hata;

    void TemaUygula(Tema t, bool kaydet)
    {
        tema = t;
        BackColor = t.Zemin;
        lblBaslik.ForeColor = t.Yazi;
        foreach (var l in etiketler) l.ForeColor = t.Yazi;
        foreach (var l in soluklar) l.ForeColor = t.Soluk;
        if (hata) lblDurum.ForeColor = Color.FromArgb(228, 0, 43);
        lblAna.ForeColor = t.Yazi; lblKdv.ForeColor = t.Yazi;
        pnlKart.BackColor = t.Kart;
        foreach (var tb in new[] { txtKur, txtUsd }) { tb.BackColor = t.Kutu; tb.ForeColor = t.Yazi; }
        btnYenile.BackColor = t.Kutu; btnYenile.ForeColor = t.Yazi;
        btnYenile.FlatAppearance.BorderColor = t.Soluk;
        pnlTop.BackColor = t.Vurgu;

        menu.Renderer = new ToolStripProfessionalRenderer(new MenuRenk(t));
        menu.BackColor = t.Zemin;
        foreach (ToolStripMenuItem m in menu.Items)
        {
            m.ForeColor = t.Yazi;
            foreach (ToolStripItem a in m.DropDownItems) { a.ForeColor = t.Yazi; a.BackColor = t.Kart; }
        }
        miAcik.Checked = t == Tema.Acik;
        miKoyu.Checked = t == Tema.Koyu;
        if (kaydet) AyarYaz(t == Tema.Koyu ? "koyu" : "acik");
    }

    static string AyarOku()
    {
        try { return File.Exists(AyarYolu) ? File.ReadAllText(AyarYolu).Trim() : ""; } catch { return ""; }
    }

    static void AyarYaz(string s)
    {
        try { Directory.CreateDirectory(Path.GetDirectoryName(AyarYolu)); File.WriteAllText(AyarYolu, s); } catch { }
    }

    Label Etiket(string t, int x, int y, int w)
    {
        return new Label { Text = t, Location = new Point(x, y), Size = new Size(w, 22), BackColor = Color.Transparent };
    }

    Label Deger(int x, int y)
    {
        return new Label { Text = "0,00 ₺", Location = new Point(x, y), Size = new Size(230, 26), TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 12f) };
    }

    static decimal Sayi(string s)
    {
        s = (s ?? "").Trim().Replace(" ", "");
        if (s.Length == 0) return 0;
        if (s.Contains(",") && s.Contains(".")) s = s.Replace(".", "");
        s = s.Replace(",", ".");
        decimal d;
        return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out d) ? d : 0;
    }

    void Hesapla()
    {
        decimal usd = Sayi(txtUsd.Text), kur = Sayi(txtKur.Text);
        decimal ana = Math.Round(usd * kur, 2);
        decimal kdv = Math.Round(ana * 0.20m, 2);
        lblAna.Text = ana.ToString("N2", TR) + " ₺";
        lblKdv.Text = kdv.ToString("N2", TR) + " ₺";
        lblTop.Text = (ana + kdv).ToString("N2", TR) + " ₺";
    }

    void KurCek()
    {
        btnYenile.Enabled = false;
        hata = false;
        lblDurum.ForeColor = tema.Soluk;
        lblDurum.Text = "Kur çekiliyor…";
        new Thread(() =>
        {
            string kaynak = null; decimal kur = 0;
            var denemeler = new Func<decimal>[] { Tcmb, ErApi, CurrencyApi };
            string[] adlar = { "TCMB döviz satış", "open.er-api.com", "currency-api" };
            for (int i = 0; i < denemeler.Length && kur <= 0; i++)
            {
                try { kur = denemeler[i](); kaynak = adlar[i]; } catch { kur = 0; }
            }
            BeginInvoke((Action)(() =>
            {
                btnYenile.Enabled = true;
                if (kur > 0)
                {
                    txtKur.Text = kur.ToString("0.0000", TR);
                    lblDurum.ForeColor = tema.Soluk;
                    lblDurum.Text = "Kaynak: " + kaynak + " — " + DateTime.Now.ToString("HH:mm");
                }
                else
                {
                    hata = true;
                    lblDurum.ForeColor = Color.FromArgb(228, 0, 43);
                    lblDurum.Text = "Kur çekilemedi, elle girebilirsin.";
                }
            }));
        }) { IsBackground = true }.Start();
    }

    static string Indir(string url)
    {
        using (var wc = new WebClient())
        {
            wc.Encoding = System.Text.Encoding.UTF8;
            wc.Headers.Add("User-Agent", "Mozilla/5.0");
            return wc.DownloadString(url);
        }
    }

    static decimal Oku(string metin, string desen)
    {
        var m = Regex.Match(metin, desen, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (!m.Success) throw new Exception("bulunamadı");
        return decimal.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
    }

    static decimal Tcmb()
    {
        var x = Indir("https://www.tcmb.gov.tr/kurlar/today.xml");
        return Oku(x, "CurrencyCode=\"USD\".*?<ForexSelling>([0-9.]+)</ForexSelling>");
    }

    static decimal ErApi()
    {
        return Oku(Indir("https://open.er-api.com/v6/latest/USD"), "\"TRY\"\\s*:\\s*([0-9.]+)");
    }

    static decimal CurrencyApi()
    {
        return Oku(Indir("https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies/usd.json"), "\"try\"\\s*:\\s*([0-9.]+)");
    }
}

class HakkindaForm : Form
{
    public HakkindaForm(Tema t)
    {
        Text = "Hakkında";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(340, 260);
        Font = new Font("Segoe UI", 10f);
        BackColor = t.Zemin;
        var ik = Logo.Ikon(); if (ik != null) Icon = ik;

        var pb = new PictureBox { Image = Logo.Al(), SizeMode = PictureBoxSizeMode.Zoom, Size = new Size(80, 80), Location = new Point(130, 20) };
        var ad = new Label { Text = "USD → TL Fiyat Hesap", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = t.Yazi, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, 108), Size = new Size(340, 28) };
        var sur = new Label { Text = "Sürüm 1.1", ForeColor = t.Soluk, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, 136), Size = new Size(340, 22) };
        var haz = new Label { Text = "Hazırlayan: Önsel Özden", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = t.Yazi, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, 166), Size = new Size(340, 24) };
        var tamam = new Button { Text = "Tamam", DialogResult = DialogResult.OK, Size = new Size(100, 32), Location = new Point(120, 210), FlatStyle = FlatStyle.Flat, BackColor = t.Vurgu, ForeColor = Color.White };
        tamam.FlatAppearance.BorderSize = 0;
        AcceptButton = tamam; CancelButton = tamam;
        Controls.AddRange(new Control[] { pb, ad, sur, haz, tamam });
    }
}
