# USD → TL Fiyat Hesap

USD ile fiyatlanan ürünlerin TL fiyatını saniyeler içinde hesaplayan küçük bir Windows programı.

## Özellikler

- Dolar kurunu internetten kendisi çeker (TCMB döviz satış; ulaşılamazsa open.er-api.com ve currency-api), 30 dakikada bir günceller
- KDV hariç TL fiyatı, %20 KDV ve KDV dahil toplam ayrı ayrı gösterilir
- Toplam tutara tıklayınca kopyalanır
- Beyaz / siyah tema (seçim hatırlanır)
- Kur çekilemezse elle girilebilir
- Kurulum gerektirmez, tek dosya, .NET Framework 4.x ile çalışır (Windows 10/11'de hazır gelir)

## İndirme

[Releases](../../releases) sayfasından **USD-TL-Hesap.zip** dosyasını indir, çıkar, **USD-TL-Hesap.exe**'yi çalıştır.

Program imzasız olduğu için Windows "Bilinmeyen yayımcı" uyarısı verebilir: **Ek bilgi → Yine de çalıştır**.

## Derleme

Exe, her güncellemede GitHub Actions ile otomatik derlenir. Elle derlemek için (Windows):

~~~
python assets\logo.py
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /codepage:65001 /win32icon:assets\logo.ico /resource:assets\logo96.png,logo96.png /r:System.Windows.Forms.dll /r:System.Drawing.dll /out:USD-TL-Hesap.exe src\UsdTlHesap.cs
~~~

## Hazırlayan

Önsel Özden
